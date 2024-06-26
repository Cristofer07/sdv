﻿namespace DaLion.Professions.Framework.Events.GameLoop.TimeChanged;

#region using directives

using DaLion.Professions.Framework.TreasureHunts;
using DaLion.Shared.Events;
using StardewModdingAPI.Events;

#endregion using directives

/// <summary>Initializes a new instance of the <see cref="ProspectorHuntTimeChangedEvent"/> class.</summary>
/// <param name="manager">The <see cref="EventManager"/> instance that manages this event.</param>
[UsedImplicitly]
internal sealed class ProspectorHuntTimeChangedEvent(EventManager? manager = null)
    : TimeChangedEvent(manager ?? ProfessionsMod.EventManager)
{
    private static uint _previousHuntRocksCrushed;

    /// <inheritdoc />
    protected override void OnEnabled()
    {
        _previousHuntRocksCrushed = 0;
    }

    /// <inheritdoc />
    protected override void OnTimeChangedImpl(object? sender, TimeChangedEventArgs e)
    {
        if (!Game1.currentLocation.IsSuitablePropsectorHuntLocation())
        {
            return;
        }

        var delta = Game1.player.stats.RocksCrushed - _previousHuntRocksCrushed;
        State.ProspectorHunt ??= new ProspectorHunt();
        if (State.ProspectorHunt.TryStart(Math.Pow(1.0016, delta) - 1d))
        {
            _previousHuntRocksCrushed += delta;
        }
    }
}
