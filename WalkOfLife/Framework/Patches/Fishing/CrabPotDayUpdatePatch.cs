﻿using Harmony;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using TheLion.Common.Extensions;
using SObject = StardewValley.Object;
using SUtility = StardewValley.Utility;

namespace TheLion.AwesomeProfessions
{
	internal class CrabPotDayUpdatePatch : BasePatch
	{
		/// <summary>Construct an instance.</summary>
		internal CrabPotDayUpdatePatch() { }

		/// <summary>Apply internally-defined Harmony patches.</summary>
		/// <param name="harmony">The Harmony instance for this mod.</param>
		protected internal override void Apply(HarmonyInstance harmony)
		{
			harmony.Patch(
				AccessTools.Method(typeof(CrabPot), nameof(CrabPot.DayUpdate)),
				prefix: new HarmonyMethod(GetType(), nameof(CrabPotDayUpdatePrefix))
			);
		}

		#region harmony patches
		/// <summary>Patch for Trapper fish quality + Luremaster bait mechanics + Conservationist trash collection mechanics.</summary>
		protected static bool CrabPotDayUpdatePrefix(ref CrabPot __instance, GameLocation location)
		{
			Farmer who = Game1.getFarmer(__instance.owner.Value);
			if (__instance.bait.Value == null && !Utility.SpecificPlayerHasProfession("conservationist", who) || __instance.heldObject.Value != null)
				return false; // don't run original logic

			__instance.tileIndexToShow = 714;
			__instance.readyForHarvest.Value = true;

			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + (int)__instance.TileLocation.X * 1000 + (int)__instance.TileLocation.Y);
			Dictionary<string, string> locationData = Game1.content.Load<Dictionary<string, string>>(Path.Combine("Data", "Locations"));
			Dictionary<int, string> fishData = Game1.content.Load<Dictionary<int, string>>(Path.Combine("Data","Fish"));
			int whichFish = -1;
			if (Utility.SpecificPlayerHasProfession("luremaster", who))
			{
				if (!Utility.IsUsingMagnet(__instance))
				{
					var rawFishData = Utility.IsUsingMagicBait(__instance) ? Utility.GetRawFishDataForAllSeasons(location, locationData) : Utility.GetRawFishDataForThisSeason(location, locationData);
					var rawFishDataWithLocation = Utility.GetRawFishDataWithLocation(rawFishData);
					whichFish = Utility.ChooseFish(__instance, fishData, rawFishDataWithLocation, location, r);
					if (whichFish < 0)
						whichFish = Utility.ChooseTrapFish(__instance, fishData, location, r, isLuremaster: true);
				}
				else
					whichFish = Utility.ChoosePirateTreasure(r, who);
			}
			else if (__instance.bait.Value != null)
				whichFish = Utility.ChooseTrapFish(__instance, fishData, location, r, isLuremaster: false);

			if (whichFish.AnyOf(14, 51))
			{
				MeleeWeapon weapon = new MeleeWeapon(whichFish) { specialItem = true };
				__instance.heldObject.Value = (SObject)(weapon as Item);
				return false; // don't run original logic
			}
			else if (whichFish.AnyOf(516, 517, 518, 519, 527, 529, 530, 531, 532, 533, 534))
			{
				Ring ring = new Ring(whichFish);
				__instance.heldObject.Value = (SObject)(ring as Item);
				return false; // don't run original logic
			}

			int fishQuality = 0;
			if (whichFish < 0)
			{
				whichFish = Utility.GetTrash(r);
				if (Utility.SpecificPlayerHasProfession("conservationist", who) && who.IsLocalPlayer)
				{
					if (++_data.OceanTrashCollectedThisSeason % 10 == 0)
						SUtility.improveFriendshipWithEveryoneInRegion(who, 1, 2);
				}
			}
			else
				fishQuality = Utility.GetTrapperFishQuality(who, r);

			int fishQuantity = Utility.GetFishQuantity(__instance, whichFish, who, r);
			__instance.heldObject.Value = new SObject(whichFish, initialStack: fishQuantity, quality: fishQuality);
			return false; // don't run original logic
		}
		#endregion harmony patches
	}
}