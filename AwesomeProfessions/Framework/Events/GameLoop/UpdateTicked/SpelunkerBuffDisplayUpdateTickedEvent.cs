﻿using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Linq;

namespace TheLion.Stardew.Professions.Framework.Events.GameLoop.UpdateTicked;

internal class SpelunkerBuffDisplayUpdateTickedEvent : UpdateTickedEvent
{
    private const int SHEET_INDEX = 40;

    private readonly int _buffID;

    /// <summary>Construct an instance.</summary>
    internal SpelunkerBuffDisplayUpdateTickedEvent()
    {
        _buffID = (ModEntry.Manifest.UniqueID + Utility.Professions.IndexOf("Spelunker")).GetHashCode();
    }

    /// <inheritdoc />
    public override void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        if (Game1.currentLocation is not MineShaft) return;

        var buff = Game1.buffsDisplay.otherBuffs.FirstOrDefault(p => p.which == _buffID);
        if (buff is not null) return;

        var bonusLadderChance = (ModEntry.State.Value.SpelunkerLadderStreak * 0.5f).ToString("0.0");
        var bonusSpeed = Math.Min(ModEntry.State.Value.SpelunkerLadderStreak / 5 + 1, ModEntry.Config.SpelunkerSpeedCap);
        Game1.buffsDisplay.addOtherBuff(
            new(0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                bonusSpeed,
                0,
                0,
                1,
                "Spelunker",
                ModEntry.ModHelper.Translation.Get("spelunker.name." + (Game1.player.IsMale ? "male" : "female")))
            {
                which = _buffID,
                sheetIndex = SHEET_INDEX,
                millisecondsDuration = 0,
                description =
                    ModEntry.ModHelper.Translation.Get("spelunker.buffdesc", new { bonusLadderChance, bonusSpeed })
            }
        );
    }
}