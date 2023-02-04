using HarmonyLib;
using Kitchen;
using TMPro;
using UnityEngine;

namespace TrashToCash.Patches
{
    [HarmonyPatch(typeof(ApplianceView))]
    public static class DisplayApplianceValuePatch
    {
        [HarmonyPatch("UpdateData")]
        [HarmonyPrefix]
        public static void UpdateData(ApplianceView __instance, ApplianceView.ViewData view_data)
        {
            // Grab the old data
            var rawData = AccessTools.Field(typeof(ApplianceView), "Data")?.GetValue(__instance);
            if (!(rawData is ApplianceView.ViewData oldData)) return;

            // Only apply price if it changed (so we don't do it multiple times)
            if (oldData.MarkedForDeletion == view_data.MarkedForDeletion || !view_data.MarkedForDeletion) return;

            // Apply our custom label
            var textHolder = new GameObject("SellPrice");
            textHolder.transform.SetParent(__instance.gameObject.transform);
            textHolder.transform.SetSiblingIndex(0);
            textHolder.transform.localPosition = new Vector3(0, 1.5f, -0.75f);
            textHolder.transform.localRotation = new Quaternion(1, 0, 0, 1);

            // Add our text
            var textMesh = textHolder.AddComponent<TextMeshPro>();
            textMesh.SetText("$" + TrashToCash.GetSellPrice(view_data.ApplianceID));
            textMesh.fontSize = 3;
            textMesh.alignment = TextAlignmentOptions.Center;
        }
    }
}