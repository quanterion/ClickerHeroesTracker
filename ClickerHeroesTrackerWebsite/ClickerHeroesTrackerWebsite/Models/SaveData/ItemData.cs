﻿// <copyright file="ItemData.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.SaveData
{
    using Newtonsoft.Json;

    /// <summary>
    /// Save game data for an item.
    /// </summary>
    [JsonObject]
    public class ItemData
    {
        /// <summary>
        /// Gets or sets the item name
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the first item bonus type.
        /// </summary>
        [JsonProperty(PropertyName = "bonusType1")]
        public int? Bonus1Type { get; set; }

        /// <summary>
        /// Gets or sets the first item bonus level.
        /// </summary>
        [JsonProperty(PropertyName = "bonus1Level")]
        public int? Bonus1Level { get; set; }

        /// <summary>
        /// Gets or sets the second item bonus type.
        /// </summary>
        [JsonProperty(PropertyName = "bonusType2")]
        public int? Bonus2Type { get; set; }

        /// <summary>
        /// Gets or sets the second item bonus level.
        /// </summary>
        [JsonProperty(PropertyName = "bonus2Level")]
        public int? Bonus2Level { get; set; }

        /// <summary>
        /// Gets or sets the third item bonus type.
        /// </summary>
        [JsonProperty(PropertyName = "bonusType3")]
        public int? Bonus3Type { get; set; }

        /// <summary>
        /// Gets or sets the third item bonus level.
        /// </summary>
        [JsonProperty(PropertyName = "bonus3Level")]
        public int? Bonus3Level { get; set; }

        /// <summary>
        /// Gets or sets the fourth item bonus type.
        /// </summary>
        [JsonProperty(PropertyName = "bonusType4")]
        public int? Bonus4Type { get; set; }

        /// <summary>
        /// Gets or sets the fourth item bonus level.
        /// </summary>
        [JsonProperty(PropertyName = "bonus4Level")]
        public int? Bonus4Level { get; set; }
    }
}