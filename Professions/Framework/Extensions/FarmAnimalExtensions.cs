﻿namespace DaLion.Professions.Framework.Extensions;

#region using directives

using DaLion.Shared.Extensions.Stardew;

#endregion

/// <summary>Extensions for the <see cref="FarmAnimal"/> class.</summary>
internal static class FarmAnimalExtensions
{
    /// <summary>Determines whether the owner of the <paramref name="animal"/> has the specified <paramref name="profession"/>.</summary>
    /// <param name="animal">The <see cref="FarmAnimal"/>.</param>
    /// <param name="profession">An <see cref="IProfession"/>..</param>
    /// <param name="prestiged">Whether to check for the prestiged variant.</param>
    /// <returns><see langword="true"/> if the <see cref="Farmer"/> who owns the <paramref name="animal"/> has the <paramref name="profession"/>, otherwise <see langword="false"/>.</returns>
    internal static bool DoesOwnerHaveProfession(this FarmAnimal animal, IProfession profession, bool prestiged = false)
    {
        return animal.GetOwner().HasProfession(profession, prestiged);
    }

    /// <summary>Adjusts the price of the <paramref name="animal"/> for <see cref="Profession.Breeder"/>.</summary>
    /// <param name="animal">The <see cref="FarmAnimal"/>.</param>
    /// <returns>The adjusted friendship value.</returns>
    internal static double GetBreederAdjustedFriendship(this FarmAnimal animal)
    {
        return Data.ReadAs<bool>(animal, DataKeys.BredByPrestigedBreeder)
            ? 10
            : Math.Pow(Math.Sqrt(10) * animal.friendshipTowardFarmer.Value / 1000, 2);
    }
}
