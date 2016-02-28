﻿// <copyright file="Startup.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using System;
    using System.Web.Http;
    using ClickerHeroesTrackerWebsite.Instrumentation;
    using ClickerHeroesTrackerWebsite.Unity;
    using Configuration;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Practices.Unity;
    using Models.Game;
    using Models.Settings;
    using Owin;
    using UploadProcessing;
    using static ClickerHeroesTrackerWebsite.Configuration.Environment;

    /// <summary>
    /// Startup class used by Owin.
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Startup configuration. Called by Owin.
        /// </summary>
        /// <param name="app">The Owin app builder</param>
        public void Configuration(IAppBuilder app)
        {
            var container = ConfigureContainer();
            var environmentProvider = container.Resolve<IEnvironmentProvider>();

            // Should be first (and last) since it may dispose objects other middleware uses.
            app.Use<UnityPerRequestLifetimeDisposingMiddleware>();

            // We want to start measuring latency as soon as possible during a request.
            app.Use<UnityOwinMiddleware<MeasureLatencyMiddleware>>(container);

            // Auth middleware. Needs to be added before any middleware that uses the user.
            ConfigureAuth(app, environmentProvider);

            // Instrument the user as soon as they're auth'd.
            app.Use<UnityOwinMiddleware<UserInstrumentationMiddleware>>(container);

            // Flush any changes to user settings
            app.Use<UnityOwinMiddleware<UserSettingsFlushingMiddleware>>(container);

            // Routing middleware
            ConfigureWebApi(app, container.Resolve<HttpConfiguration>(), container);

            // Configure Mvc
            ConfigureMvc(container);

            // Only allow telemetry in production
            if (environmentProvider.Environment != Production)
            {
                TelemetryConfiguration.Active.DisableTelemetry = true;
            }

            // Warm up the game data parsing
            container.Resolve<GameData>();

            // Only in production, start the background upload processing.
            if (environmentProvider.Environment == Production)
            {
                container.Resolve<IUploadProcessor>().Start();
            }
        }
    }
}