﻿using StardewModdingAPI.Events;

namespace TheLion.Stardew.Professions.Framework.Events
{
	public class ScavengerHuntDayStartedEvent : DayStartedEvent
	{
		/// <inheritdoc/>
		public override void OnDayStarted(object sender, DayStartedEventArgs e)
		{
			if (ModEntry.ScavengerHunt != null) ModEntry.ScavengerHunt.ResetAccumulatedBonus();
		}
	}
}