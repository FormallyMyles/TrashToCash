using System;
using HarmonyLib;
using Kitchen;
using KitchenData;
using KitchenMods;
using UnityEngine;

namespace TrashToCash
{
    public class TrashToCash : GenericSystemBase, IModSystem
    {
        public const int TrashID = 1551609169; // ID for the Bin
        public static readonly string BinName = GameData.Main.Get<Appliance>(TrashID).Name;
        
        // Local variables
        private Harmony _harmony;

        public static int GetSellPrice(int applianceID)
        {
            return (int) Math.Floor(Appliance.GetPrice(GameData.Main.Get<Appliance>(applianceID).PriceTier) / 2D);
        }

        protected override void Initialise()
        {
            base.Initialise();
            
            // Get harmony to apply all the patches found in this assembly
            _harmony = new Harmony(GetType().Assembly.FullName);
            _harmony.PatchAll(GetType().Assembly);
            Debug.Log("TrashToCash has successfully been patched into the game.");
        }

        protected override void OnUpdate()
        {
            // Currently doesn't do anything here, logic is handled in System
        }
    }
}