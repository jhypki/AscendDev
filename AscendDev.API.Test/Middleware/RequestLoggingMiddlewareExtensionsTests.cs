using AscendDev.API.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AscendDev.API.Test.Middleware
{
    [TestFixture]
    public class RequestLoggingMiddlewareExtensionsTests
    {
        [Test]
        public void UseRequestLogging_RegistersMiddleware()
        {
            // This test simply verifies that the extension method exists and returns the IApplicationBuilder
            // We can't easily verify that it actually adds the middleware without integration tests

            // Arrange & Act
            // Create a simple test to verify the method signature and behavior
            var appBuilder = new TestApplicationBuilder();
            var result = RequestLoggingMiddlewareExtensions.UseRequestLogging(appBuilder);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(appBuilder));
            Assert.That(appBuilder.MiddlewareAdded, Is.True);
        }

        // Simple test implementation of IApplicationBuilder that just tracks if middleware was added
        private class TestApplicationBuilder : IApplicationBuilder
        {
            public bool MiddlewareAdded { get; private set; }

            public IServiceProvider ApplicationServices { get; set; }
            public IFeatureCollection ServerFeatures => new FeatureCollection();
            public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();

            public RequestDelegate Build()
            {
                return context => Task.CompletedTask;
            }

            public IApplicationBuilder New()
            {
                return new TestApplicationBuilder();
            }

            public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
            {
                MiddlewareAdded = true;
                return this;
            }

            public IApplicationBuilder UseMiddleware<T>(params object[] args)
            {
                MiddlewareAdded = true;
                return this;
            }

            public IApplicationBuilder UseMiddleware(Type middleware, params object[] args)
            {
                MiddlewareAdded = true;
                return this;
            }
        }
    }
}