﻿namespace DaLion.Stardew.Professions.Framework.Extensions;

#region using directives

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.FishPond;
using StardewValley.Menus;
using StardewValley.Objects;

using Common.Extensions;

using SObject = StardewValley.Object;

#endregion using directives

/// <summary>Extensions for the <see cref="FishPond"/> class.</summary>
internal static class FishPondExtensions
{
    private const int ROE_INDEX_I = 812, SQUID_INK_INDEX_I = 814, SEAWEED_INDEX_I = 152, ALGAE_INDEX_I = 153;

    private static readonly Func<int, double> _productionChanceByValue = x => (double) 14765 / (x + 120) + 1.5;
    private static readonly FieldInfo _FishPondData = typeof(FishPond).Field("_fishPondData");

    /// <summary>Whether the instance's population has been fully unlocked.</summary>
    internal static bool HasUnlockedFinalPopulationGate(this FishPond pond)
    {
        var fishPondData = (FishPondData) _FishPondData.GetValue(pond);
        return fishPondData?.PopulationGates is null ||
               pond.lastUnlockedPopulationGate.Value >= fishPondData.PopulationGates.Keys.Max();
    }

    /// <summary>Whether a legendary fish lives in this pond.</summary>
    internal static bool IsLegendaryPond(this FishPond pond)
    {
        return pond.GetFishObject().HasContextTag("fish_legendary");
    }

    /// <summary>Increase Roe/Ink stack and quality based on population size and average quality.</summary>
    internal static void AddBonusProduceAmountAndQuality(this FishPond pond)
    {
        var produce = pond.output.Value as SObject;
        if (produce is not null && !produce.ParentSheetIndex.IsAnyOf(ROE_INDEX_I, SQUID_INK_INDEX_I) && !pond.fishType.Value.IsAlgae()) return;

        var fish = pond.GetFishObject();
        var r = new Random(Guid.NewGuid().GetHashCode());

        if (pond.fishType.Value.IsAlgae())
        {
            var stack = r.Next(pond.currentOccupants.Value);
            produce ??= new(pond.fishType.Value, 1);
            produce.Stack = stack;
            pond.output.Value = produce;
            return;
        }

        var quality = pond.GetRoeQuality(r);
        if (produce is not null) produce.Quality = quality;

        var bonusStack = 0;
        var productionChancePerFish = _productionChanceByValue(fish.Price) / 100;
        for (var i = 0; i < pond.FishCount; ++i)
            if (r.NextDouble() < productionChancePerFish)
                ++bonusStack;

        if (bonusStack <= 0) return;

        if (produce is null)
        {
            int produceId;
            if (fish.Name.Contains("Squid"))
                produceId = SQUID_INK_INDEX_I;
            else if (fish.Name != "Coral")
                produceId = ROE_INDEX_I;
            else
                produceId = r.Next(SEAWEED_INDEX_I, ALGAE_INDEX_I + 1);

            if (produceId != ROE_INDEX_I)
            {
                produce = new(produceId, bonusStack);
            }
            else
            {
                var split = Game1.objectInformation[pond.fishType.Value].Split('/');
                var c = TailoringMenu.GetDyeColor(pond.GetFishObject()) ??
                        (pond.fishType.Value == 698 ? new(61, 55, 42) : Color.Orange);
                produce = new ColoredObject(ROE_INDEX_I, 1, c);
                produce.name = split[0] + " Roe";
                produce.preserve.Value = SObject.PreserveType.Roe;
                produce.preservedParentSheetIndex.Value = pond.fishType.Value;
                produce.Price += Convert.ToInt32(split[1]) / 2;
                produce.Stack = bonusStack;
            }

            produce.Quality = quality;
        }
        else
        {
            produce.Stack += bonusStack;
        }

        pond.output.Value = produce;
    }

    /// <summary>Determine the amount of fish of each quality currently in this pond.</summary>
    internal static (int, int, int) GetFishQualities(this FishPond pond, int qualityRating = -1, bool forFamily = false)
    {
        if (qualityRating < 0) qualityRating = pond.ReadDataAs<int>(forFamily ? "FamilyQualityRating" : "QualityRating");

        var numBestQuality = qualityRating / 4096; // 16^3
        qualityRating -= numBestQuality * 4096;

        var numHighQuality = qualityRating / 256; // 16^2
        qualityRating -= numHighQuality * 256;

        var numMedQuality = qualityRating / 16;

        return (numBestQuality, numHighQuality, numMedQuality);
    }

    /// <summary>Determine which quality should be deducted from the total quality rating after fishing in this pond.</summary>
    internal static int GetLowestFishQuality(this FishPond pond, int qualityRating = -1, bool forFamily = false)
    {
        var (numBestQuality, numHighQuality, numMedQuality) = GetFishQualities(pond, qualityRating, forFamily);
        var familyCount = pond.ReadDataAs<int>("FamilyCount");
        return numBestQuality + numHighQuality + numMedQuality < (forFamily ? familyCount : pond.FishCount - familyCount + 1) // fish pond count has already been deducted at this point, so we consider +1
            ? SObject.lowQuality
            : numMedQuality > 0
                ? SObject.medQuality
                : numHighQuality > 0
                    ? SObject.highQuality
                    : SObject.bestQuality;
    }

    /// <summary>Choose the quality value for today's produce by parsing stored quality rating data.</summary>
    /// <param name="r">A random number generator.</param>
    private static int GetRoeQuality(this FishPond pond, Random r)
    {
        var (numBestQuality, numHighQuality, numMedQuality) = pond.GetFishQualities();
        if (pond.IsLegendaryPond())
        {
            var (numBestFamilyQuality, numHighFamilyQuality, numMedFamilyQuality) = pond.GetFishQualities(forFamily: true);
            numBestQuality += numBestFamilyQuality;
            numHighQuality += numHighFamilyQuality;
            numMedQuality += numMedFamilyQuality;
        }

        var roll = r.Next(pond.FishCount);
        return roll < numBestQuality
            ? SObject.bestQuality
            : roll < numBestQuality + numHighQuality
                ? SObject.highQuality
                : roll < numBestQuality + numHighQuality + numMedQuality
                    ? SObject.medQuality
                    : SObject.lowQuality;
    }
}