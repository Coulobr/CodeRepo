// ==============================================
 // IGrubbitCard & CardContext
 // ==============================================
 namespace Grubbit.Server.Cards
 {
     public struct CardContext
     {
         public string cardId;
         public ulong ownerClientId;
         public ulong[] targetClientIds;
         public string[] targetCardIds;
     }

     public interface IGrubbitCard
     {
         void Resolve(GameSession session, CardContext ctx);
     }
 }

