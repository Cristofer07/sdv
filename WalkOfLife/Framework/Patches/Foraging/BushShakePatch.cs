﻿using Harmony;
using System;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Reflection.Emit;
using TheLion.Common.Harmony;

namespace TheLion.AwesomeProfessions
{
	internal class BushShakePatch : BasePatch
	{
		private static ILHelper _helper;

		/// <summary>Construct an instance.</summary>
		internal BushShakePatch()
		{
			_helper = new ILHelper(_monitor);
		}

		/// <summary>Apply internally-defined Harmony patches.</summary>
		/// <param name="harmony">The Harmony instance for this mod.</param>
		protected internal override void Apply(HarmonyInstance harmony)
		{
			harmony.Patch(
				AccessTools.Method(typeof(Bush), name: "shake"),
				prefix: new HarmonyMethod(GetType(), nameof(BushShakePrefix)),
				transpiler: new HarmonyMethod(GetType(), nameof(BushShakeTranspiler)),
				postfix: new HarmonyMethod(GetType(), nameof(BushShakePostfix))
			);
		}

		#region harmony patches
		/// <summary>Patch to count foraged berries for Ecologist.</summary>
		protected static bool BushShakePrefix(ref Bush __instance, ref int __state)
		{
			__state = __instance.tileSheetOffset.Value;
			return true; // run original logic
		}

		/// <summary>Patch to nerf Ecologist berry quality.</summary>
		private static IEnumerable<CodeInstruction> BushShakeTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			_helper.Attach(instructions).Log($"Patching method {typeof(Bush)}::shake.");

			/// From: Game1.player.professions.Contains(16) ? 4 : 0
			/// To: Game1.player.professions.Contains(16) ? _GetForageQualityForEcologist() : 0

			try
			{
				_helper
					.FindProfessionCheck(Farmer.botanist)		// find index of botanist check
					.AdvanceUntil(
						new CodeInstruction(OpCodes.Ldc_I4_4)
					)
					.GetLabels(out List<Label> labels)
					.ReplaceWith(								// replace with custom quality
						new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Utility), nameof(Utility.GetEcologistForageQuality)))
					)
					.SetLabels(labels);
			}
			catch(Exception ex)
			{
				_helper.Error($"Failed while patching modded Ecologist wild berry quality.\nHelper returned {ex}").Restore();
			}

			return _helper.Flush();
		}

		/// <summary>Patch to count foraged berries for Ecologist.</summary>
		protected static void BushShakePostfix(ref Bush __instance, ref int __state)
		{
			if (__state - __instance.tileSheetOffset.Value == 1) ++_data.ItemsForaged;
		}
		#endregion harmony patches
	}
}