// ==============================================
 // GameSession (v2): authoritative logic + intents
 // ==============================================
 using System;
 using System.Collections;
 using System.Collections.Generic;
 using UnityEngine;
 using Unity.Netcode;
 using Grubbit.NetShared;
using Grubbit.Shared;
 using Grubbit.Client;
 using Grubbit.Server.Cards;

 namespace Grubbit.Server
 {
     public enum SessionState
     {
         WaitingForPlayers,
         ReadyCheck,
         StartingGame,
         InGame,
         AwaitingClientChoice,
         Ended
     }

     [Serializable]
     public class PlayerState
     {
         public ulong clientId;
         public int health = 30;
         public int armor = 0;
         public int grub = 0;
         public int power = 0;

         public List<string> deck = new List<string>();
         public List<string> hand = new List<string>();
         public List<string> board = new List<string>();
         public List<string> graveyard = new List<string>();
         public List<string> exile = new List<string>();
     }

     public partial class GameSession : NetworkBehaviour
     {
         [Header("Settings")]
         public int initialHandSize = 7;
         public int readyCheckSeconds = 15;

         private SessionState _state = SessionState.WaitingForPlayers;
         private readonly Dictionary<ulong, PlayerGameStateData> _players = new();
         private readonly Dictionary<int, HashSet<ulong>> _acks = new();
         private int _nextEventId = 1;
         private string _matchId;
         private HashSet<ulong> _accepted = new();
         private HashSet<ulong> _declined = new();

         private ulong _currentTurnClientId;
         private int _turnNumber = 0;

         public override void OnNetworkSpawn()
         {
             if (!IsServer) return;

             NetworkManager.OnClientConnectedCallback += OnClientConnected;
             NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;

             _matchId = Guid.NewGuid().ToString("N");
             _state = SessionState.WaitingForPlayers;
             TryStartReadyCheckIfBothPresent();
         }

         private void OnDestroy()
         {
             if (NetworkManager != null)
             {
                 NetworkManager.OnClientConnectedCallback -= OnClientConnected;
                 NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
             }
         }

         private void OnClientConnected(ulong clientId)
         {
             if (!IsServer) return;
             if (!_players.ContainsKey(clientId))
                 _players[clientId] = CreateInitialPlayerGameState(clientId);
             TryStartReadyCheckIfBothPresent();
         }

         private void OnClientDisconnected(ulong clientId)
         {
             if (!IsServer) return;
             if (_state != SessionState.Ended)
             {
                 Broadcast(S2CEvent.MatchCanceled, new ErrorPayload { message = "Opponent disconnected." });
                 _state = SessionState.Ended;
             }
         }

         private PlayerGameStateData CreateInitialPlayerGameState(ulong clientId)
         {
             var deck = new List<string>();
             for (int i = 1; i <= 30; i++) deck.Add($"card_{i:D2}");
             deck[0] = "card_simple_damage_2";
             deck[1] = "card_heal_3";
             Shuffle(deck);
             return new PlayerGameStateData { clientId = clientId, deck = deck, health = 30 };
         }

         private void TryStartReadyCheckIfBothPresent()
         {
             if (!IsServer || _state != SessionState.WaitingForPlayers) return;
             if (_players.Count < 2) return;

             _state = SessionState.ReadyCheck;
             _accepted.Clear();
             _declined.Clear();

             var payload = new AcceptPromptPayload { matchId = _matchId, acceptWindowSeconds = readyCheckSeconds };
             Broadcast(S2CEvent.AcceptPrompt, payload);
             StartCoroutine(ReadyCheckWindow());
         }

         private IEnumerator ReadyCheckWindow()
         {
             float end = Time.realtimeSinceStartup + readyCheckSeconds;
             while (Time.realtimeSinceStartup < end && _state == SessionState.ReadyCheck)
             {
                 if (_accepted.Count == 2) { StartGame(); yield break; }
                 if (_declined.Count > 0) { CancelMatch("A player declined the match."); yield break; }
                 yield return null;
             }
             if (_state == SessionState.ReadyCheck) CancelMatch("Ready check timed out.");
         }

         public void OnClientAcceptResponse(ulong clientId, string matchId, bool accept)
         {
             if (!IsServer || _state != SessionState.ReadyCheck || matchId != _matchId) return;
             if (accept) _accepted.Add(clientId); else _declined.Add(clientId);
         }

         private void CancelMatch(string reason)
         {
             Broadcast(S2CEvent.MatchCanceled, new ErrorPayload { message = reason });
             _state = SessionState.Ended;
         }

         private void StartGame()
         {
             _state = SessionState.StartingGame;
             Broadcast(S2CEvent.GameStart, new AcceptPromptPayload { matchId = _matchId, acceptWindowSeconds = 0 });

             foreach (var kv in _players) DrawCards(kv.Value, initialHandSize);

             foreach (var kv in _players)
             {
                 var me = kv.Value;
                 var opp = GetOpponent(me.clientId);
                 SendTo(me.clientId, S2CEvent.DrawCardsSelf, new DrawCardsSelfPayload { recipientClientId = me.clientId, cardIds = me.hand.ToArray(), count = me.hand.Count });
                 if (opp != null)
                     SendTo(opp.clientId, S2CEvent.DrawCardsOpponent, new DrawCardsOpponentPayload { recipientClientId = opp.clientId, count = me.hand.Count });
             }

             ulong first = ulong.MaxValue; foreach (var id in _players.Keys) if (id < first) first = id;
             _currentTurnClientId = first;
             _turnNumber = 1;
             Broadcast(S2CEvent.TurnChanged, new TurnChangedPayload { currentTurnClientId = _currentTurnClientId, turnNumber = _turnNumber });
             _state = SessionState.InGame;
         }

         public void OnClientEndTurn(ulong clientId)
         {
             if (!IsServer || _state != SessionState.InGame) return;
             if (clientId != _currentTurnClientId) return;

             Broadcast(S2CEvent.TurnEnded, new TurnEndedPayload { previousTurnClientId = clientId, turnNumber = _turnNumber });
             _currentTurnClientId = GetOpponentClientId(clientId);
             _turnNumber++;
             Broadcast(S2CEvent.TurnChanged, new TurnChangedPayload { currentTurnClientId = _currentTurnClientId, turnNumber = _turnNumber });
         }

         public void OnClientPlayCard(ulong clientId, PlayCardRequest req)
         {
             if (!IsServer || _state != SessionState.InGame) return;
             if (clientId != _currentTurnClientId) return;
             if (!_players.TryGetValue(clientId, out var ps)) return;
             if (!ps.cardsInHand.Remove(req.cardId)) return;

             ps.cardsInPlay.Add(req.cardId);
             SendTo(clientId, S2CEvent.CardPlayedSelf, new CardPlayedSelfPayload { actorClientId = clientId, cardId = req.cardId });
             var oppId = GetOpponentClientId(clientId);
             SendTo(oppId, S2CEvent.CardPlayedOpponent, new CardPlayedOpponentPayload { actorClientId = clientId, cardId = req.cardId });

             var card = GrubbitCardUtility.GetCardTypeById(req.cardId) as CardBase;
             if (card != null)
                 card.Resolve(this, new CardContext { cardId = req.cardId, ownerClientId = clientId, targetClientIds = req.targetClientIds, targetCardIds = req.targetCardIds });
         }

         public void OnClientDiscardCard(ulong clientId, DiscardCardRequest req)
         {
             if (!IsServer || _state != SessionState.InGame) return;
             if (clientId != _currentTurnClientId) return;
             if (!_players.TryGetValue(clientId, out var ps)) return;
             if (!ps.cardsInHand.Remove(req.cardId)) return;

             ps.discardPile.Add(req.cardId);
             SendTo(clientId, S2CEvent.CardDiscardedSelf, new CardMovedPayload { actorClientId = clientId, cardId = req.cardId, revealed = true });
             var oppId = GetOpponentClientId(clientId);
             SendTo(oppId, S2CEvent.CardDiscardedOpponent, new CardMovedPayload { actorClientId = clientId, cardId = req.revealToOpponent ? req.cardId : string.Empty, revealed = req.revealToOpponent });
         }

         public void OnClientExileCard(ulong clientId, ExileCardRequest req)
         {
             if (!IsServer || _state != SessionState.InGame) return;
             if (clientId != _currentTurnClientId) return;
             if (!_players.TryGetValue(clientId, out var ps)) return;
             if (!ps.cardsInHand.Remove(req.cardId)) return;

             ps.exilePile.Add(req.cardId);
             SendTo(clientId, S2CEvent.CardExiledSelf, new CardMovedPayload { actorClientId = clientId, cardId = req.cardId, revealed = true });
             var oppId = GetOpponentClientId(clientId);
             SendTo(oppId, S2CEvent.CardExiledOpponent, new CardMovedPayload { actorClientId = clientId, cardId = req.revealToOpponent ? req.cardId : string.Empty, revealed = req.revealToOpponent });
         }

         public void ApplyDamage(ulong targetClientId, int amount, string sourceCardId)
         {
             if (!_players.TryGetValue(targetClientId, out var ps)) return;
             int dmg = amount;
             if (ps.armor > 0) { int absorb = Math.Min(ps.armor, dmg); ps.armor -= absorb; dmg -= absorb; }
             if (dmg > 0) ps.health = Math.Max(0, ps.health - dmg);
             Broadcast(S2CEvent.DamageApplied, new DamageAppliedPayload { targetClientId = targetClientId, amount = amount, sourceCardId = sourceCardId, resultingHealth = ps.health, resultingArmor = ps.armor });
         }

         public void ApplyHeal(ulong targetClientId, int amount, string sourceCardId)
         {
             if (!_players.TryGetValue(targetClientId, out var ps)) return;
             ps.health = Math.Min(30, ps.health + amount);
             Broadcast(S2CEvent.HealApplied, new HealAppliedPayload { targetClientId = targetClientId, amount = amount, sourceCardId = sourceCardId, resultingHealth = ps.health });
         }

         public ulong GetOpponentClientId(ulong clientId)
         {
             foreach (var kv in _players) if (kv.Key != clientId) return kv.Key;
             return clientId;
         }

         private PlayerGameStateData GetOpponent(ulong clientId)
         {
             foreach (var kv in _players) if (kv.Key != clientId) return kv.Value;
             return null;
         }

         private void DrawCards(PlayerGameStateData ps, int count)
         {
             for (int i = 0; i < count && ps.cardsInDeck.Count > 0; i++)
             {
                 var top = ps.cardsInDeck[0];
                 ps.cardsInDeck.RemoveAt(0);
                 ps.cardsInHand.Add(top);
             }
         }

         public void OnClientAcknowledged(ulong clientId, int eventId)
         {
             if (!_acks.TryGetValue(eventId, out var set)) return;
             set.Add(clientId);
         }

         private IEnumerator WaitForAcks(int eventId, float timeoutSec, Action onDone)
         {
             float end = Time.realtimeSinceStartup + timeoutSec;
             while (Time.realtimeSinceStartup < end)
             {
                 if (_acks.TryGetValue(eventId, out var set) && set.Count >= 2) break;
                 yield return null;
             }
             _acks.Remove(eventId);
             onDone?.Invoke();
         }

         private void Broadcast(S2CEvent evt, object payloadObj)
         {
             string json = NetShared.NetJson.ToJson(payloadObj);
             var recv = UnityEngine.Object.FindObjectOfType<ClientEventReceiver>();
             if (recv == null) return;
             recv.ReceiveServerEventClientRpc((int)evt, json);
         }

         private void SendTo(ulong clientId, S2CEvent evt, object payloadObj)
         {
             string json = NetShared.NetJson.ToJson(payloadObj);
             var recv = UnityEngine.Object.FindObjectOfType<ClientEventReceiver>();
             if (recv == null) return;
             var target = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } } };
             recv.ReceiveServerEventClientRpc((int)evt, json, target);
         }

         private static void Shuffle<T>(IList<T> list)
         {
             var rng = new System.Random();
             int n = list.Count;
             while (n > 1)
             {
                 int k = rng.Next(n--);
                 (list[n], list[k]) = (list[k], list[n]);
             }
         }
     }
 }

