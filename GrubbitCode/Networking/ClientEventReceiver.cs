// ==============================================
 // ClientEventReceiver: handles RPCs and sends intents (v2)
 // ==============================================
 using UnityEngine;
 using Unity.Netcode;
 using Grubbit.NetShared;

 namespace Grubbit.Client
 {
     public class ClientEventReceiver : NetworkBehaviour
     {
         public static ClientEventReceiver Instance { get; private set; }

         private void Awake()
         {
             if (Instance != null && Instance != this) { Destroy(gameObject); return; }
             Instance = this;
             DontDestroyOnLoad(gameObject);
         }

         [ClientRpc]
         public void ReceiveServerEventClientRpc(int eventType, string json, ClientRpcParams sendParams = default)
         {
             var evt = (S2CEvent)eventType;
             ClientEventBus.Raise(evt, json);
         }

         public void AcknowledgeEvent(int eventId) => SubmitAcknowledgeServerRpc(eventId);

         [ServerRpc(RequireOwnership = false)]
         private void SubmitAcknowledgeServerRpc(int eventId, ServerRpcParams rpcParams = default)
         {
             var session = FindFirstObjectByType<Grubbit.Server.GameSession>();
             session?.OnClientAcknowledged(rpcParams.Receive.SenderClientId, eventId);
         }

         public void SendAcceptResponse(string matchId, bool accept) => SubmitAcceptResponseServerRpc(matchId, accept);

         [ServerRpc(RequireOwnership = false)]
         private void SubmitAcceptResponseServerRpc(string matchId, bool accept, ServerRpcParams rpcParams = default)
         {
             var session = FindFirstObjectByType<Grubbit.Server.GameSession>();
             session?.OnClientAcceptResponse(rpcParams.Receive.SenderClientId, matchId, accept);
         }

         public void EndTurn() => SubmitEndTurnServerRpc();

         [ServerRpc(RequireOwnership = false)]
         private void SubmitEndTurnServerRpc(ServerRpcParams rpcParams = default)
         {
             var session = FindFirstObjectByType<Grubbit.Server.GameSession>();
             session?.OnClientEndTurn(rpcParams.Receive.SenderClientId);
         }

         public void PlayCard(PlayCardRequest req) => SubmitPlayCardServerRpc(NetShared.NetJson.ToJson(req));

         [ServerRpc(RequireOwnership = false)]
         private void SubmitPlayCardServerRpc(string jsonReq, ServerRpcParams rpcParams = default)
         {
             var req = NetShared.NetJson.FromJson<PlayCardRequest>(jsonReq);
             var session = FindFirstObjectByType<Grubbit.Server.GameSession>();
             session?.OnClientPlayCard(rpcParams.Receive.SenderClientId, req);
         }

         public void DiscardCard(DiscardCardRequest req) => SubmitDiscardCardServerRpc(NetShared.NetJson.ToJson(req));

         [ServerRpc(RequireOwnership = false)]
         private void SubmitDiscardCardServerRpc(string jsonReq, ServerRpcParams rpcParams = default)
         {
             var req = NetShared.NetJson.FromJson<DiscardCardRequest>(jsonReq);
             var session = FindFirstObjectByType<Grubbit.Server.GameSession>();
             session?.OnClientDiscardCard(rpcParams.Receive.SenderClientId, req);
         }

         public void ExileCard(ExileCardRequest req) => SubmitExileCardServerRpc(NetShared.NetJson.ToJson(req));

         [ServerRpc(RequireOwnership = false)]
         private void SubmitExileCardServerRpc(string jsonReq, ServerRpcParams rpcParams = default)
         {
             var req = NetShared.NetJson.FromJson<ExileCardRequest>(jsonReq);
             var session = FindFirstObjectByType<Grubbit.Server.GameSession>();
             session?.OnClientExileCard(rpcParams.Receive.SenderClientId, req);
         }

		// Add to the class (public methods call ServerRpcs)

		// Add card to stack
		public void AddCardToStack(AddCardToStackRequest req)
			=> SubmitAddCardToStackServerRpc(Grubbit.NetShared.NetJson.ToJson(req));

		[ServerRpc(RequireOwnership = false)]
		private void SubmitAddCardToStackServerRpc(string jsonReq, ServerRpcParams _ = default)
		{
			var req = Grubbit.NetShared.NetJson.FromJson<AddCardToStackRequest>(jsonReq);
			var session = FindFirstObjectByType<Grubbit.Server.GameSession>();
			session?.OnClientAddCardToStack(NetworkManager.LocalClientId, req);
		}

		// Choice response
		public void SendChoiceResponse(ChoiceResponseRequest req)
			=> SubmitChoiceResponseServerRpc(Grubbit.NetShared.NetJson.ToJson(req));

		[ServerRpc(RequireOwnership = false)]
		private void SubmitChoiceResponseServerRpc(string jsonReq, ServerRpcParams _ = default)
		{
			var req = Grubbit.NetShared.NetJson.FromJson<ChoiceResponseRequest>(jsonReq);
			var session = FindFirstObjectByType<Grubbit.Server.GameSession>();
			session?.OnClientChoiceResponse(NetworkManager.LocalClientId, req);
		}

		// Concede
		public void Concede() => SubmitConcedeServerRpc();

		[ServerRpc(RequireOwnership = false)]
		private void SubmitConcedeServerRpc(ServerRpcParams _ = default)
		{
			var session = FindFirstObjectByType<Grubbit.Server.GameSession>();
			session?.OnClientConcede(NetworkManager.LocalClientId);
		}

		// Emote
		public void SendEmote(string emoteId)
			=> SubmitEmoteServerRpc(emoteId);

		[ServerRpc(RequireOwnership = false)]
		private void SubmitEmoteServerRpc(string emoteId, ServerRpcParams _ = default)
		{
			var session = FindFirstObjectByType<Grubbit.Server.GameSession>();
			session?.OnClientEmote(NetworkManager.LocalClientId, emoteId);
		}

		// Ping
		public void Ping(long clientTime) => SubmitPingServerRpc(clientTime);

		[ServerRpc(RequireOwnership = false)]
		private void SubmitPingServerRpc(long clientTime, ServerRpcParams _ = default)
		{
			var session = FindFirstObjectByType<Grubbit.Server.GameSession>();
			session?.OnClientPing(NetworkManager.LocalClientId, clientTime);
		}

	}
}

