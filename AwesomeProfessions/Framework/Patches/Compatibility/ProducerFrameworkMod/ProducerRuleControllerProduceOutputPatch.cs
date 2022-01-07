﻿using HarmonyLib;
using JetBrains.Annotations;
using StardewModdingAPI;
using StardewValley;
using System;
using TheLion.Stardew.Common.Harmony;
using TheLion.Stardew.Professions.Framework.Extensions;
using SObject = StardewValley.Object;

namespace TheLion.Stardew.Professions.Framework.Patches.Compatibility.ProducerFrameworkMod;

[UsedImplicitly]
internal class ProducerRuleControllerProduceOutputPatch : BasePatch
{
    /// <summary>Construct an instance.</summary>
    internal ProducerRuleControllerProduceOutputPatch()
    {
        try
        {
            Original = "ProducerFrameworkMod.Controllers.ProducerRuleController".ToType()
                .MethodNamed("ProduceOutput");
        }
        catch
        {
            // ignored
        }
    }

    #region harmony patches

    /// <summary>Patch to apply modded Artisan and Gemologist perks to PFM machines.</summary>
    [HarmonyPostfix]
    private static void ProducerRuleControllerProduceOutputPostfix(SObject producer, Farmer who, SObject input,
        bool probe)
    {
        if (input is null || probe) return;

        var output = producer.heldObject.Value;
        if (producer.IsArtisanMachine() && output.IsArtisanGood())
        {
            if (Context.IsMultiplayer && producer.owner.Value != who.UniqueMultiplayerID ||
                !who.HasProfession("Artisan"))
            {
                output.Quality = SObject.lowQuality;
                return;
            }

            output.Quality = input.Quality;
            if (output.Quality < SObject.bestQuality &&
                new Random(Guid.NewGuid().GetHashCode()).NextDouble() < 0.05)
                output.Quality += output.Quality == SObject.highQuality ? 2 : 1;

            if (who.HasPrestigedProfession("Artisan"))
                producer.MinutesUntilReady -= producer.MinutesUntilReady / 4;
            else
                producer.MinutesUntilReady -= producer.MinutesUntilReady / 10;
        }
        else if (who.IsLocalPlayer && (output.IsForagedMineral() || output.IsGemOrMineral()) &&
                 who.HasProfession("Gemologist"))
        {
            ModData.Increment<uint>(ModData.KEY_GEMOLOGISTMINERALSCOLLECTED_S, who);
            if (producer.name != "Crystalarium") output.Quality = Utility.Professions.GetGemologistMineralQuality();
        }
    }

    #endregion harmony patches
}