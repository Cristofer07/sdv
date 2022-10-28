﻿namespace DaLion.Redux.Rings.Patches;

#region using directives

using DaLion.Redux.Rings.Extensions;
using HarmonyLib;
using StardewValley.Menus;
using StardewValley.Objects;
using HarmonyPatch = DaLion.Shared.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class ForgeMenuGetForgeCostPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="ForgeMenuGetForgeCostPatch"/> class.</summary>
    internal ForgeMenuGetForgeCostPatch()
    {
        this.Target = this.RequireMethod<ForgeMenu>(nameof(ForgeMenu.GetForgeCost));
    }

    #region harmony patches

    /// <summary>Modify forge cost for Infinity Band.</summary>
    [HarmonyPrefix]
    private static bool ForgeMenuGetForgeCostPrefix(ref int __result, Item left_item, Item right_item)
    {
        if (!ModEntry.Config.Rings.TheOneIridiumBand ||
            left_item is not Ring left || left.ParentSheetIndex != Globals.InfinityBandIndex ||
            right_item is not Ring right || !right.IsGemRing())
        {
            return true; // run original logic
        }

        __result = 10;
        return false; // don't run original logic
    }

    #endregion harmony patches
}