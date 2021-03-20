﻿using Harmony;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TheLion.Common.Harmony;

namespace TheLion.AwesomeProfessions
{
	internal class GameLocationBreakStonePatch : BasePatch
	{
		private static ILHelper _helper;

		/// <summary>Construct an instance.</summary>
		internal GameLocationBreakStonePatch()
		{
			_helper = new ILHelper(_monitor);
		}

		/// <summary>Apply internally-defined Harmony patches.</summary>
		/// <param name="harmony">The Harmony instance for this mod.</param>
		protected internal override void Apply(HarmonyInstance harmony)
		{
			harmony.Patch(
				AccessTools.Method(typeof(GameLocation), name: "breakStone"),
				transpiler: new HarmonyMethod(GetType(), nameof(GameLocationBreakStoneTranspiler)),
				postfix: new HarmonyMethod(GetType(), nameof(GameLocationBreakStonePostfix))
			);
		}

		#region harmony patches
		/// <summary>Patch to remove Miner extra ore + remove Geologist extra gem chance + remove Prospector double coal chance.</summary>
		protected static IEnumerable<CodeInstruction> GameLocationBreakStoneTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			_helper.Attach(instructions).Log($"Patching method {typeof(GameLocation)}::breakStone.");

			/// From: addedOres = (who.professions.Contains(<miner_id>) ? 1 : 0)
			/// To: addedOres = 0

			try
			{
				_helper
					.FindProfessionCheck(Farmer.miner)		// find index of miner check
					.Retreat()
					.RemoveUntil(
						new CodeInstruction(OpCodes.Brtrue)	// remove this check
					)
					.Advance()
					.Remove(2)								// remove true case
					.StripLabels();
			}
			catch (Exception ex)
			{
				_helper.Error($"Failed while removing vanilla Miner extra ore.\nHelper returned {ex}").Restore();
			}

			_helper.Backup();

			/// Skipped: if (who.professions.Contains(<geologist_id>)...

			try
			{
				_helper
					.FindProfessionCheck(Farmer.geologist)		// find index of geologist check
					.Retreat()
					.GetLabels(out var labels)					// copy labels
					.StripLabels()								// remove labels from here
					.AdvanceUntil(
						new CodeInstruction(OpCodes.Brfalse)	// the false case branch
					)
					.GetOperand(out object isNotGeologist)		// copy destination
					.Return()
					.Insert(									// insert uncoditional branch to skip this check
						new CodeInstruction(OpCodes.Br, (Label)isNotGeologist)
					)
					.Retreat()
					.AddLabels(labels);							// restore labels to inserted branch
			}
			catch (Exception ex)
			{
				_helper.Error($"Failed while removing vanilla Geologist paired gems.\nHelper returned {ex}").Restore();
			}

			_helper.Backup();

			/// Skipped: if (who.professions.Contains(<prospector_id>))...

			try
			{
				_helper
					.FindProfessionCheck(Farmer.burrower)		// find index of prospector check
					.Retreat()
					.AdvanceUntil(
						new CodeInstruction(OpCodes.Brfalse)	// the false case branch
					)
					.GetOperand(out object isNotProspector)		// copy destination
					.Return()
					.Insert(									// insert uncoditional branch to skip this check
						new CodeInstruction(OpCodes.Br_S, (Label)isNotProspector)
					);
			}
			catch (Exception ex)
			{
				_helper.Error($"Failed while removing vanilla Prospector double coal chance.\nHelper returned {ex}").Restore();
			}

			return _helper.Flush();
		}

		/// <summary>Patch for Miner extra resources.</summary>
		protected static void GameLocationBreakStonePostfix(ref GameLocation __instance, int indexOfStone, int x, int y, Farmer who, Random r)
		{
			if (Utility.SpecificPlayerHasProfession("miner", who) && r.NextDouble() < 0.10)
			{
				if (Utility.ResourceFromStoneId.TryGetValue(indexOfStone, out int indexOfResource))
					Game1.createObjectDebris(indexOfResource, x, y, who.UniqueMultiplayerID, __instance);
				else if (indexOfStone == 44)	// gem node
					Game1.createObjectDebris(Game1.random.Next(1, 8) * 2, x, y, who.UniqueMultiplayerID, __instance);
				else if (indexOfStone == 46)	// mystic stone
				{
					double rolled = r.NextDouble();
					if (rolled < 0.25)
						Game1.createMultipleObjectDebris(74, x, y, 1, who.UniqueMultiplayerID, __instance);	// drop prismatic shard
					else if (rolled < 0.6)
						Game1.createObjectDebris(765, x, y, who.UniqueMultiplayerID, __instance);			// drop iridium ore
					else
						Game1.createObjectDebris(764, x, y, who.UniqueMultiplayerID, __instance);			// drop gold ore
				}
			}
		}
		#endregion harmony patches
	}
}