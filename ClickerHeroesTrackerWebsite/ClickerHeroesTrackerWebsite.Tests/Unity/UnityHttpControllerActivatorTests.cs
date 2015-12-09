﻿// <copyright file="UnityHttpControllerActivatorTests.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Tests.Unity
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;
    using ClickerHeroesTrackerWebsite.Unity;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UnityHttpControllerActivatorTests
    {
        [TestMethod]
        public void UnityHttpControllerActivator_Create_BasicTest()
        {
            var unityContainer = new UnityContainer();
            unityContainer.RegisterType<ContainerControlledDependency>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<TransientDependency>(new TransientLifetimeManager());

            var requestMessage = new HttpRequestMessage();
            var controllerDescriptor = new HttpControllerDescriptor();

            var unityControllerActivator = new UnityHttpControllerActivator(unityContainer);

            // Validate a controller creation
            var controller1 = (MockHttpController)unityControllerActivator.Create(requestMessage, controllerDescriptor, typeof(MockHttpController));
            Assert.IsNotNull(controller1);
            Assert.IsNotNull(controller1.ContainerControlledDependency);
            Assert.IsNotNull(controller1.TransientDependency);

            // Validate another controller creation
            var controller2 = (MockHttpController)unityControllerActivator.Create(requestMessage, controllerDescriptor, typeof(MockHttpController));
            Assert.IsNotNull(controller2);
            Assert.IsNotNull(controller2.ContainerControlledDependency);
            Assert.IsNotNull(controller2.TransientDependency);

            // Validate the lifetimes between the two resolution
            Assert.AreNotEqual(controller1, controller2);
            Assert.AreEqual(controller1.ContainerControlledDependency, controller2.ContainerControlledDependency);
            Assert.AreNotEqual(controller1.TransientDependency, controller2.TransientDependency);
        }

        private sealed class MockHttpController : IHttpController
        {
            public MockHttpController(
                ContainerControlledDependency containerControlledDependency,
                TransientDependency transientDependency)
            {
                this.ContainerControlledDependency = containerControlledDependency;
                this.TransientDependency = transientDependency;
            }

            public ContainerControlledDependency ContainerControlledDependency { get; }

            public TransientDependency TransientDependency { get; }

            public Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class ContainerControlledDependency
        {
        }

        private sealed class TransientDependency
        {
        }
    }
}
