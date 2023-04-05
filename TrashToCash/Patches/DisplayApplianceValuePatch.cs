using HarmonyLib;
using Kitchen;
using TrashToCash.Views;

namespace TrashToCash.Patches
{
    [HarmonyPatch(typeof(ApplianceView))]
    public static class DisplayApplianceValuePatch
    {
        [HarmonyPatch("Initialise")]
        [HarmonyPrefix]
        public static void Initialise(ApplianceView __instance)
        {
            // Add our view which controls our price
            __instance.GameObject.AddComponent<ApplianceSellPriceView>();
        }
    }
}