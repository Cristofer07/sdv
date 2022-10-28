﻿namespace DaLion.Redux.Professions.Patches.Common;

#region using directives

using System.Collections.Generic;
using System.Reflection;
using DaLion.Redux.Professions.Extensions;
using HarmonyLib;
using StardewValley.Menus;
using HarmonyPatch = DaLion.Shared.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class LevelUpMenuAddProfessionDescriptionsPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="LevelUpMenuAddProfessionDescriptionsPatch"/> class.</summary>
    internal LevelUpMenuAddProfessionDescriptionsPatch()
    {
        this.Target = this.RequireMethod<LevelUpMenu>("addProfessionDescriptions");
    }

    #region harmony patches

    /// <summary>Patch to apply modded profession descriptions.</summary>
    [HarmonyPrefix]
    private static bool LevelUpMenuAddProfessionDescriptionsPrefix(
        List<string> descriptions, string professionName)
    {
        try
        {
            if (!Profession.TryFromName(professionName, true, out var profession) ||
                (Skill)profession.Skill == Farmer.luckSkill)
            {
                return true; // run original logic
            }

            descriptions.Add(profession.DisplayName);

            var skillIndex = profession / 6;
            var currentLevel = Game1.player.GetUnmodifiedSkillLevel(skillIndex);
            var prestiged = Game1.player.HasProfession(profession, true) ||
                            (Game1.activeClickableMenu is LevelUpMenu && currentLevel > 10);
            descriptions.AddRange(profession.GetDescription(prestiged).Split('\n'));

            return false; // don't run original logic
        }
        catch (Exception ex)
        {
            Log.E($"Failed in {MethodBase.GetCurrentMethod()?.Name}:\n{ex}");
            return true; // default to original logic
        }
    }

    #endregion harmony patches
}