using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Grubbit.NetShared;

namespace Grubbit.Server
{
	public partial class GameSession : NetworkBehaviour
	{
		// ----- Stack model (server-only) -----
		class StackItem
		{
			public int id;
			public string cardId;
			public ulong sourceClientId;
			public string actionId;
			public ulong[] targetClientIds;
			public string[] targetCardIds;
			public bool revealedToOpponent;
		}

		private readonly List<StackItem> _stack = new();
		private int _nextStackItemId = 1;

		// ----- CHOICES -----
		private readonly Dictionary<int, (ulong recipient, ChoiceRequestPayload req)> _pendingChoices = new();
		private int _nextChoiceId = 1;

		// ===== Client Request Handlers =====

		public void OnClientAddCardToStack(ulong clientId, AddCardToStackRequest req)
		{
			if (!IsServer || _state != SessionState.InGame) return;

			// Simple rule: only current player for now (relax later if you want instants)
			if (clientId != _currentTurnClientId) return;

			if (!_players.TryGetValue(clientId, out var ps)) return;
			if (!ps.cardsInHand.Contains(req.cardId) && !ps.cardsInPlay.Contains(req.cardId)) return;

			// If from hand, move to board or a "stack zone" if you prefer:
			if (ps.cardsInHand.Remove(req.cardId)) ps.cardsInPlay.Add(req.cardId);

			var item = new StackItem
			{
				id = _nextStackItemId++,
				cardId = req.cardId,
				sourceClientId = clientId,
				actionId = req.actionId ?? "cast",
				targetClientIds = req.targetClientIds,
				targetCardIds = req.targetCardIds,
				revealedToOpponent = req.revealToOpponent
			};
			_stack.Add(item);

			// Notify: self + opponent (respect reveal)
			var payload = new StackItemPayload
			{
				stackItemId = item.id,
				sourceCardId = item.cardId,
				sourceClientId = item.sourceClientId,
				actionId = item.actionId,
				targetClientIds = item.targetClientIds,
				targetCardIds = item.targetCardIds,
				revealedToOpponent = item.revealedToOpponent
			};
			SendTo(clientId, S2CEvent.StackItemAdded, payload);

			var oppId = GetOpponentClientId(clientId);
			var oppPayload = new StackItemPayload
			{
				stackItemId = item.id,
				sourceCardId = item.revealedToOpponent ? item.cardId : string.Empty,
				sourceClientId = item.sourceClientId,
				actionId = item.actionId,
				targetClientIds = item.targetClientIds,
				targetCardIds = item.targetCardIds,
				revealedToOpponent = item.revealedToOpponent
			};
			SendTo(oppId, S2CEvent.StackItemAdded, oppPayload);

			// (Optional) broadcast full state for spectators/robust UI
			Broadcast(S2CEvent.StackUpdated, new StackStatePayload
			{
				items = BuildStackStateForBroadcast()
			});

			// Resolve immediately for now (or guard behind priority/ACK flow)
			StartCoroutine(ResolveTopOfStackCoroutine());
		}

		public void OnClientChoiceResponse(ulong clientId, ChoiceResponseRequest res)
		{
			if (!IsServer) return;
			if (!_pendingChoices.TryGetValue(res.requestId, out var entry)) return;
			if (entry.recipient != clientId) return;

			_pendingChoices.Remove(res.requestId);

			// Broadcast result to both clients
			Broadcast(S2CEvent.ChoiceResolved, res);

			// Apply choice effect if this choice was gating some effect; call into your card resolver here if needed.
		}

		public void OnClientConcede(ulong clientId)
		{
			if (!IsServer || _state == SessionState.Ended) return;
			Broadcast(S2CEvent.PlayerConceded, new PlayerConcededPayload { playerClientId = clientId });
			_state = SessionState.Ended;
			// Optionally: end match flow, award win to opponent, return to lobby...
		}

		public void OnClientEmote(ulong clientId, string emoteId)
		{
			if (!IsServer) return;
			var opp = GetOpponentClientId(clientId);
			SendTo(opp, S2CEvent.EmoteReceived, new EmotePayload { fromClientId = clientId, emoteId = emoteId });
		}

		public void OnClientPing(ulong clientId, long clientTimeSent)
		{
			if (!IsServer) return;
			SendTo(clientId, S2CEvent.Pong, new PongPayload { clientTimeSent = clientTimeSent });
		}

		// ===== Helpers =====

		private System.Collections.IEnumerator ResolveTopOfStackCoroutine()
		{
			// Let clients play an anticipation animation if you want; tiny delay is fine
			yield return null;

			if (_stack.Count == 0) yield break;
			var top = _stack[_stack.Count - 1];
			_stack.RemoveAt(_stack.Count - 1);

			// Resolve via card system (server authoritative)
			var card = Cards.CardRegistry.Create(top.cardId);
			if (card != null)
			{
				var ctx = new Cards.CardContext
				{
					cardId = top.cardId,
					ownerClientId = top.sourceClientId,
					targetClientIds = top.targetClientIds,
					targetCardIds = top.targetCardIds
				};
				card.Resolve(this, ctx);
			}

			// Notify resolution
			Broadcast(S2CEvent.StackResolved, new StackItemPayload
			{
				stackItemId = top.id,
				sourceCardId = top.cardId,
				sourceClientId = top.sourceClientId,
				actionId = top.actionId,
				targetClientIds = top.targetClientIds,
				targetCardIds = top.targetCardIds,
				revealedToOpponent = true
			});

			// Broadcast updated stack
			Broadcast(S2CEvent.StackUpdated, new StackStatePayload
			{
				items = BuildStackStateForBroadcast()
			});
		}

		private StackItemPayload[] BuildStackStateForBroadcast()
		{
			var arr = new List<StackItemPayload>(_stack.Count);
			foreach (var s in _stack)
			{
				arr.Add(new StackItemPayload
				{
					stackItemId = s.id,
					sourceCardId = s.revealedToOpponent ? s.cardId : string.Empty,
					sourceClientId = s.sourceClientId,
					actionId = s.actionId,
					targetClientIds = s.targetClientIds,
					targetCardIds = s.targetCardIds,
					revealedToOpponent = s.revealedToOpponent
				});
			}
			return arr.ToArray();
		}

		// Ask a player to make a choice (server -> client)
		private int RequestChoice(ulong recipient, string choiceType, string prompt, int min, int max,
								  string[] candidateCardIds = null, ulong[] candidateClientIds = null, int timeoutSeconds = 0)
		{
			int id = _nextChoiceId++;
			var req = new ChoiceRequestPayload
			{
				requestId = id,
				recipientClientId = recipient,
				choiceType = choiceType,
				prompt = prompt,
				min = min,
				max = max,
				candidateCardIds = candidateCardIds,
				candidateClientIds = candidateClientIds,
				timeoutSeconds = timeoutSeconds
			};
			_pendingChoices[id] = (recipient, req);

			// Send only to that player
			SendTo(recipient, S2CEvent.ChoiceRequested, req);
			return id;
		}
	}
}
