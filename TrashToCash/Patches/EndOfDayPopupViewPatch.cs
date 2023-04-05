using HarmonyLib;
using Kitchen;

namespace TrashToCash.Patches
{
    [HarmonyPatch(typeof(EndOfDayPopupView))]
    public static class EndOfDayPopupViewPatch
    {
        [HarmonyPatch("AddRow")]
        [HarmonyPrefix]
        public static void AddRow(ref string text, int value, bool is_sum_row = false)
        {
            if (text.Equals(TrashToCash.BinName))
            {
                text = "Trash to Cash";
            }
        }
    }
}