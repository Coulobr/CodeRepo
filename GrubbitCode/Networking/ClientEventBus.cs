// ==============================================
 // Grubbit Client Event Bus (v2)
 // ==============================================
 using System;
 using Grubbit.NetShared;

 namespace Grubbit.Client
 {
     public static class ClientEventBus
     {
         public static event Action<AcceptPromptPayload> OnAcceptPrompt;
         public static event Action<DrawCardsSelfPayload> OnDrawCardsSelf;
         public static event Action<DrawCardsOpponentPayload> OnDrawCardsOpponent;
         public static event Action<TurnChangedPayload> OnTurnChanged;
         public static event Action<TurnEndedPayload> OnTurnEnded;
         public static event Action<CardPlayedSelfPayload> OnCardPlayedSelf;
         public static event Action<CardPlayedOpponentPayload> OnCardPlayedOpponent;
         public static event Action<CardMovedPayload> OnCardDiscardedSelf;
         public static event Action<CardMovedPayload> OnCardDiscardedOpponent;
         public static event Action<CardMovedPayload> OnCardExiledSelf;
         public static event Action<CardMovedPayload> OnCardExiledOpponent;
         public static event Action<DamageAppliedPayload> OnDamageApplied;
         public static event Action<HealAppliedPayload> OnHealApplied;
         public static event Action<StatusAppliedPayload> OnStatusApplied;
         public static event Action<string> OnMatchCanceled;
         public static event Action<string> OnError;
         public static event Action<S2CEvent, string> OnEvent;

         public static void Raise(S2CEvent evt, string json)
         {
             OnEvent?.Invoke(evt, json);
             switch (evt)
             {
                 case S2CEvent.AcceptPrompt: OnAcceptPrompt?.Invoke(NetJson.FromJson<AcceptPromptPayload>(json)); break;
                 case S2CEvent.DrawCardsSelf: OnDrawCardsSelf?.Invoke(NetJson.FromJson<DrawCardsSelfPayload>(json)); break;
                 case S2CEvent.DrawCardsOpponent: OnDrawCardsOpponent?.Invoke(NetJson.FromJson<DrawCardsOpponentPayload>(json)); break;
                 case S2CEvent.TurnChanged: OnTurnChanged?.Invoke(NetJson.FromJson<TurnChangedPayload>(json)); break;
                 case S2CEvent.TurnEnded: OnTurnEnded?.Invoke(NetJson.FromJson<TurnEndedPayload>(json)); break;
                 case S2CEvent.CardPlayedSelf: OnCardPlayedSelf?.Invoke(NetJson.FromJson<CardPlayedSelfPayload>(json)); break;
                 case S2CEvent.CardPlayedOpponent: OnCardPlayedOpponent?.Invoke(NetJson.FromJson<CardPlayedOpponentPayload>(json)); break;
                 case S2CEvent.CardDiscardedSelf: OnCardDiscardedSelf?.Invoke(NetJson.FromJson<CardMovedPayload>(json)); break;
                 case S2CEvent.CardDiscardedOpponent: OnCardDiscardedOpponent?.Invoke(NetJson.FromJson<CardMovedPayload>(json)); break;
                 case S2CEvent.CardExiledSelf: OnCardExiledSelf?.Invoke(NetJson.FromJson<CardMovedPayload>(json)); break;
                 case S2CEvent.CardExiledOpponent: OnCardExiledOpponent?.Invoke(NetJson.FromJson<CardMovedPayload>(json)); break;
                 case S2CEvent.DamageApplied: OnDamageApplied?.Invoke(NetJson.FromJson<DamageAppliedPayload>(json)); break;
                 case S2CEvent.HealApplied: OnHealApplied?.Invoke(NetJson.FromJson<HealAppliedPayload>(json)); break;
                 case S2CEvent.StatusApplied: OnStatusApplied?.Invoke(NetJson.FromJson<StatusAppliedPayload>(json)); break;
                 case S2CEvent.MatchCanceled: OnMatchCanceled?.Invoke(json); break;
                 case S2CEvent.Error: OnError?.Invoke(json); break;
             }
         }
     }
 }

