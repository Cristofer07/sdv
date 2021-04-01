﻿using Harmony;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using SObject = StardewValley.Object;

namespace TheLion.AwesomeProfessions
{
	internal class ObjectCtorPatch : BasePatch
	{
		/// <inheritdoc/>
		public override void Apply(HarmonyInstance harmony)
		{
			harmony.Patch(
				AccessTools.Constructor(typeof(SObject), new Type[] { typeof(Vector2), typeof(int), typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(bool) }),
				postfix: new HarmonyMethod(GetType(), nameof(ObjectCtorPostfix))
			);
		}

		#region harmony patches

		/// <summary>Patch for Ecologist wild berry recovery.</summary>
		private static void ObjectCtorPostfix(ref SObject __instance)
		{
			Farmer owner = Game1.getFarmer(__instance.owner.Value);
			if (Utility.IsWildBerry(__instance) && Utility.SpecificFarmerHasProfession("ecologist", owner))
				__instance.Edibility = (int)(__instance.Edibility * 1.5f);
		}

		#endregion harmony patches
	}
}