// ==============================================
 // Example card implementations
 // ==============================================
 namespace Grubbit.Server.Cards
 {
     public class SimpleDamageCard : IGrubbitCard
     {
         private readonly int _amount;
         public SimpleDamageCard(int amount) { _amount = amount; }

         public void Resolve(GameSession session, CardContext ctx)
         {
             ulong target = session.GetOpponentClientId(ctx.ownerClientId);
             if (ctx.targetClientIds != null && ctx.targetClientIds.Length > 0)
                 target = ctx.targetClientIds[0];
             session.ApplyDamage(target, _amount, ctx.cardId);
         }
     }

     public class SimpleHealCard : IGrubbitCard
     {
         private readonly int _amount;
         public SimpleHealCard(int amount) { _amount = amount; }

         public void Resolve(GameSession session, CardContext ctx)
         {
             session.ApplyHeal(ctx.ownerClientId, _amount, ctx.cardId);
         }
     }
 }

