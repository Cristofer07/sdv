﻿using Harmony;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TheLion.Common.Harmony;

namespace TheLion.AwesomeProfessions
{
	internal class GameLocationCheckActionPatch : BasePatch
	{
		/// <inheritdoc/>
		public override void Apply(HarmonyInstance harmony)
		{
			harmony.Patch(
				TargetMethod(),
				transpiler: new HarmonyMethod(GetType(), nameof(GameLocationCheckActionTranspiler))
			);
		}

		#region harmony patches

		/// <summary>Patch to nerf Ecologist forage quality + add quality to foraged minerals for Gemologist.</summary>
		private static IEnumerable<CodeInstruction> GameLocationCheckActionTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
		{
			Helper.Attach(instructions).Log($"Patching method {typeof(GameLocation)}::{nameof(GameLocation.checkAction)}.");

			/// From: if (who.professions.Contains(<botanist_id>) && objects[key].isForage()) objects[key].Quality = 4
			/// To: if (who.professions.Contains(<ecologist_id>) && objects[key].isForage()) objects[key].Quality = GetForageQualityForEcologist()
			/// Injected: else if (who.professions.Contains(<gemologist_id>) && _IsForagedMineral(objects[key])) objects[key].Quality = GetMineralQualityForGemologist()

			Label resumeExecution = iLGenerator.DefineLabel();
			try
			{
				Helper
					.FindProfessionCheck(Farmer.botanist)                                   // find index of botanist check
					.AdvanceUntil(
						new CodeInstruction(OpCodes.Ldc_I4_4)                               // start of objects[key].Quality = 4
					)
					.ReplaceWith(                                                           // replace with custom quality
						new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Utility), nameof(Utility.GetEcologistForageQuality)))
					)
					.Return()                                                               // return to profession check
					.Retreat(2)                                                             // retreat to start of check
					.ToBufferUntil(                                                         // copy entire section until done setting quality
						stripLabels: true,
						advance: true,
						new CodeInstruction(OpCodes.Br)
					)
					.InsertBuffer()                                                         // paste
					.GetLabels(out var labels)                                              // copy labels from following section
					.StripLabels()                                                          // remove those labels
					.AddLabels(resumeExecution)                                             // add new label to branch to if either inserted condition fails
					.Return()
					.AddLabels(labels)                                                      // restore copied labels to inserted section
					.AdvanceUntil(
						new CodeInstruction(OpCodes.Ldc_I4_S, operand: Farmer.botanist)     // find index of inserted botanist check
					)
					.SetOperand(Utility.ProfessionMap.Forward["gemologist"])                // replace with gemologist check
					.AdvanceUntil(
						new CodeInstruction(OpCodes.Brfalse)                                // end of is profession condition
					)
					.SetOperand(resumeExecution)                                            // replace branch destination with newly added label
					.AdvanceUntil(
						new CodeInstruction(OpCodes.Brfalse)                                // end of is forage condition
					)
					.SetOperand(resumeExecution)                                            // replace branch destination with newly added label
					.Retreat(3)                                                             // start of call to .isForage(this)
					.Remove(3)                                                              // remove this call
					.Insert(                                                                // replace with call to IsForagedMineral()
						new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Utility), nameof(Utility.IsForagedMineral)))
					)
					.AdvanceUntil(
						new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Utility), nameof(Utility.GetEcologistForageQuality)))
					)
					.SetOperand(AccessTools.Method(typeof(Utility), nameof(Utility.GetGemologistMineralQuality)));  // replace correct method call
			}
			catch (Exception ex)
			{
				Helper.Error($"Failed while patching modded Ecologist and Gemologist forage quality.\nHelper returned {ex}").Restore();
			}

			return Helper.Flush();
		}

		#endregion harmony patches

		#region private methods

		/// <summary>Get the inner method to patch.</summary>
		private static MethodBase TargetMethod()
		{
			var targetMethod = typeof(GameLocation).InnerMethodsStartingWith("<checkAction>b__0").First();
			if (targetMethod == null)
				throw new MissingMethodException("Target method '<checkAction>b__0' was not found.");

			return targetMethod;
		}

		#endregion private methods
	}
}