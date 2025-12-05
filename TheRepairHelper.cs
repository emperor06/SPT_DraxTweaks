using HarmonyLib;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace DraxTweaks;

[HarmonyPatch(typeof(RepairHelper), nameof(RepairHelper.UpdateItemDurability))]
public class TheRepairHelper
{
    public static void Postfix(
        Item itemToRepair,
        TemplateItem itemToRepairDetails,
        bool isArmor,
        double amountToRepair,
        bool useRepairKit,
        double traderQualityMultiplier,
        bool applyMaxDurabilityDegradation = true
    )
    {
        // Repair mask cracks
        if (itemToRepair.Upd?.FaceShield is not null && itemToRepair.Upd.FaceShield?.Hits > 0) {
            itemToRepair.Upd.FaceShield.Hits = 0;
        }
    }
}
