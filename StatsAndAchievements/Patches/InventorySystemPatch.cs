using DV.InventorySystem;
using HarmonyLib;
using UnityEngine;

namespace StatsAndAchievements.Patches;

[HarmonyPatch(typeof(Inventory))]
internal static class InventorySystemPatch
{
    [HarmonyPatch(nameof(Inventory.AddItemToInventory), typeof(GameObject), typeof(int), typeof(bool), typeof(bool))]
    [HarmonyPostfix]
    private static void AddItemToInventoryPostfix(GameObject item, int __result)
    {
        if (__result >= 0 && item != null)
        {
            Events.Actions.InvokeItemAcquired(item.name);
        }
    }
}