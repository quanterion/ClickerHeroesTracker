﻿// <copyright file="StatType.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Api.Stats
{
    /// <summary>
    /// The kind of stat
    /// </summary>
    public enum StatType
    {
        Unknown,

        /* Ancients */
        AncientArgaiv,
        AncientAtman,
        AncientBerserker,
        AncientBhaal,
        AncientBubos,
        AncientChawedo,
        AncientChronos,
        AncientDogcog,
        AncientDora,
        AncientEnergon,
        AncientFortuna,
        AncientFragsworth,
        AncientHecatoncheir,
        AncientIris,
        AncientJuggernaut,
        AncientKhrysos,
        AncientKleptos,
        AncientKumawakamaru,
        AncientLibertas,
        AncientMammon,
        AncientMimzee,
        AncientMorgulis,
        AncientPluto,
        AncientRevolc,
        AncientSiyalatas,
        AncientSniperino,
        AncientSolomon,
        AncientThusia,
        AncientVaagur,

        /* Ancient boosts from Items */
        ItemArgaiv,
        ItemAtman,
        ItemBerserker,
        ItemBhaal,
        ItemBubos,
        ItemChawedo,
        ItemChronos,
        ItemDogcog,
        ItemDora,
        ItemEnergon,
        ItemFortuna,
        ItemFragsworth,
        ItemHecatoncheir,
        ItemIris,
        ItemJuggernaut,
        ItemKhrysos,
        ItemKleptos,
        ItemKumawakamaru,
        ItemLibertas,
        ItemMammon,
        ItemMimzee,
        ItemMorgulis,
        ItemPluto,
        ItemRevolc,
        ItemSiyalatas,
        ItemSniperino,
        ItemSolomon,
        ItemThusia,
        ItemVaagur,

        /* Suggested ancient levels */
        SuggestedArgaiv,
        SuggestedAtman,
        SuggestedBerserker,
        SuggestedBhaal,
        SuggestedBubos,
        SuggestedChawedo,
        SuggestedChronos,
        SuggestedDogcog,
        SuggestedDora,
        SuggestedEnergon,
        SuggestedFortuna,
        SuggestedFragsworth,
        SuggestedHecatoncheir,
        SuggestedIris,
        SuggestedJuggernaut,
        SuggestedKhrysos,
        SuggestedKleptos,
        SuggestedKumawakamaru,
        SuggestedLibertas,
        SuggestedMammon,
        SuggestedMimzee,
        SuggestedMorgulis,
        SuggestedPluto,
        SuggestedRevolc,
        SuggestedSiyalatas,
        SuggestedSniperino,
        SuggestedSolomon,
        SuggestedThusia,
        SuggestedVaagur,

        /* Outsiders */
        OutsiderXyliqil,
        OutsiderChorgorloth,
        OutsiderPhandoryss,
        OutsiderBorb,
        OutsiderPonyboy,

        /* Computed stats */
        SoulsPerHour,
        OptimalLevel,
        SoulsPerAscension,
        OptimalAscensionTime,
        SoulsSpent,
        TitanDamage,
    }
}