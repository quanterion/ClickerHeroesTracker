﻿// <copyright file="AncientLevelSummaryViewModel.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Calculator
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using Game;
    using SaveData;

    /// <summary>
    /// The model for the ancient level summary view.
    /// </summary>
    public class AncientLevelSummaryViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AncientLevelSummaryViewModel"/> class.
        /// </summary>
        public AncientLevelSummaryViewModel(AncientsData ancientsData)
        {
            var ancientLevels = new SortedDictionary<Ancient, long>(AncientComparer.Instance);
            foreach (var ancientData in ancientsData.Ancients.Values)
            {
                var ancient = Ancient.Get(ancientData.Id);
                if (ancient == null)
                {
                    // An ancient we didn't know about?
                    continue;
                }

                ancientLevels.Add(ancient, ancientData.Level);
            }

            this.AncientLevels = ancientLevels;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AncientLevelSummaryViewModel"/> class.
        /// </summary>
        public AncientLevelSummaryViewModel(SqlDataReader reader)
        {
            var ancientLevels = new SortedDictionary<Ancient, long>(AncientComparer.Instance);
            while (reader.Read())
            {
                var ancientId = Convert.ToInt32(reader["AncientId"]);
                var level = Convert.ToInt64(reader["Level"]);

                var ancient = Ancient.Get(ancientId);

                ancientLevels.Add(ancient, level);
            }

            this.AncientLevels = ancientLevels;
        }

        /// <summary>
        /// Gets the levels for each ancient.
        /// </summary>
        public IDictionary<Ancient, long> AncientLevels { get; private set; }

        private class AncientComparer : IComparer<Ancient>
        {
            private static AncientComparer instance = new AncientComparer();

            public static AncientComparer Instance
            {
                get
                {
                    return instance;
                }
            }

            public int Compare(Ancient x, Ancient y)
            {
                return x.Name.CompareTo(y.Name);
            }
        }
    }
}