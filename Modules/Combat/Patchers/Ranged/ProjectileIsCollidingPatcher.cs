﻿namespace DaLion.Overhaul.Modules.Combat.Patchers.Ranged;

#region using directives

using DaLion.Shared.Harmony;
using HarmonyLib;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Projectiles;

#endregion using directives

[UsedImplicitly]
internal class ProjectileIsCollidingPatcher : HarmonyPatcher
{
    /// <summary>Initializes a new instance of the <see cref="ProjectileIsCollidingPatcher"/> class.</summary>
    internal ProjectileIsCollidingPatcher()
    {
        this.Target = this.RequireMethod<Projectile>(nameof(Projectile.isColliding));
    }

    #region harmony patches

    /// <summary>Allows projectiles to keep traveling over water.</summary>
    [HarmonyPostfix]
    private static void ProjectileIsCollidingPostfix(
        ref bool __result,
        NetPosition ___position,
        GameLocation location)
    {
        if (!__result)
        {
            return;
        }

        var tile = new Vector2(___position.X / Game1.tileSize, ___position.Y / Game1.tileSize);
        if (location.doesTileHaveProperty(
                (int)___position.X / Game1.tileSize,
                (int)___position.Y / Game1.tileSize,
                "Water",
                "Back") == "T")
        {
            __result = false;
            return;
        }

        if (location is BuildableGameLocation buildable)
        {
            foreach (var building in buildable.buildings)
            {
                if 
            }
        }
    }

    #endregion harmony patches
}
