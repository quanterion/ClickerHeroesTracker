﻿// <copyright file="Startup.WebApi.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite
{
    using System;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Web.Http;
    using System.Web.Http.ExceptionHandling;
    using Instrumentation;
    using Microsoft.ApplicationInsights;
    using Microsoft.Practices.Unity;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using Owin;

    /// <summary>
    /// Configure WebAPI
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Configure WebAPI
        /// </summary>
        /// <param name="app">The Owin app builder</param>
        /// <param name="config">The WebAPI configuration object</param>
        /// <param name="container">The unity container</param>
        private static void ConfigureWebApi(
            IAppBuilder app,
            HttpConfiguration config,
            IUnityContainer container)
        {
            // Replace the Json formatter with out own.
            config.Formatters.Remove(config.Formatters.JsonFormatter);
            config.Formatters.Add(new BrowserJsonFormatter());

            // Allow the json formatter to handle requests from the browser
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            // Beautify by default for debuggability. When gzipping, this barely adds anything to the payload.
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;

            // Omit nulls
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            // Use camel-casing for fields (lower case first character)
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // Convert enum values to strings
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });

            // Use attribute-based routing
            config.MapHttpAttributeRoutes();

            // Exception logging
            config.Services.Add(typeof(IExceptionLogger), new InstrumentationExceptionLogger(() => container.Resolve<TelemetryClient>()));

            // Owin wireup
            app.UseWebApi(config);
        }

        /// <summary>
        /// This class behaves jsut like the default but forces a json content type
        /// </summary>
        private sealed class BrowserJsonFormatter : JsonMediaTypeFormatter
        {
            public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
            {
                base.SetDefaultContentHeaders(type, headers, mediaType);

                // Force the json content type so browsers can render it like json.
                headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
        }
    }
}