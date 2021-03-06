﻿// <copyright file="ISiteNewsProvider.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace Website.Services.SiteNews
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles retrieving and updating site news
    /// </summary>
    public interface ISiteNewsProvider
    {
        /// <summary>
        /// Retrieves all the news sorted by date.
        /// </summary>
        /// <returns>The site news ordered by date.</returns>
        Task<IDictionary<DateTime, IList<string>>> RetrieveSiteNewsEntriesAsync();

        /// <summary>
        /// Adds news entries for the specified date.
        /// </summary>
        /// <param name="newsDate">The date to add the news for.</param>
        /// <param name="messages">The messages for the site news.</param>
        /// <returns>Task to await on.</returns>
        Task AddSiteNewsEntriesAsync(DateTime newsDate, IList<string> messages);

        /// <summary>
        /// Deletes all the news entries for the specified date.
        /// </summary>
        /// <param name="newsDate">The date of the enws to delete.</param>
        /// <returns>Task to await on.</returns>
        Task DeleteSiteNewsForDateAsync(DateTime newsDate);
    }
}
