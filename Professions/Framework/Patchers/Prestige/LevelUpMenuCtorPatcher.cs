﻿namespace DaLion.Professions.Framework.Patchers.Prestige;

#region using directives

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DaLion.Shared.Extensions.Reflection;
using DaLion.Shared.Harmony;
using HarmonyLib;
using StardewValley.Menus;

#endregion using directives

[UsedImplicitly]
internal sealed class LevelUpMenuCtorPatcher : HarmonyPatcher
{
    /// <summary>Initializes a new instance of the <see cref="LevelUpMenuCtorPatcher"/> class.</summary>
    /// <param name="harmonizer">The <see cref="Harmonizer"/> instance that manages this patcher.</param>
    internal LevelUpMenuCtorPatcher(Harmonizer harmonizer)
        : base(harmonizer)
    {
        this.Target = this.RequireConstructor<LevelUpMenu>(typeof(int), typeof(int));
    }

    #region harmony patches

    /// <summary>Patch to allow choosing professions above level 10.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? LevelUpMenuCtorTranspiler(
        IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        var helper = new ILHelper(original, instructions);

        // From: if ((currentLevel == 5 || currentLevel == 10) && currentSkill != 5)
        // To: if (currentLevel % 5 == 0 && currentSkill != 5)
        try
        {
            helper
                .PatternMatch(
                [
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, typeof(LevelUpMenu).RequireField("currentLevel")),
                    new CodeInstruction(OpCodes.Ldc_I4_5),
                    new CodeInstruction(OpCodes.Beq_S),
                ])
                .Move(3)
                .Insert(
                [
                    new CodeInstruction(OpCodes.Rem_Un),
                    new CodeInstruction(OpCodes.Ldc_I4_0),
                ])
                .RemoveUntil([new CodeInstruction(OpCodes.Ldc_I4_S, 10)]);
        }
        catch (Exception ex)
        {
            Log.E($"Failed patching profession choices above level 10.\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches
}
