// ==============================================
 // CardRegistry: id -> constructor mapping
 // ==============================================
 using System;
 using System.Collections.Generic;

 namespace Grubbit.Server.Cards
 {
     public static class CardRegistry
     {
         private static readonly Dictionary<string, Func<IGrubbitCard>> _map = new();

         static CardRegistry()
         {
             Register("card_simple_damage_2", () => new SimpleDamageCard(2));
             Register("card_heal_3", () => new SimpleHealCard(3));
         }

         public static void Register(string cardId, Func<IGrubbitCard> ctor)
         {
             _map[cardId] = ctor;
         }

         public static IGrubbitCard Create(string cardId)
         {
             if (_map.TryGetValue(cardId, out var ctor)) return ctor();
             return null;
         }
     }
 }

