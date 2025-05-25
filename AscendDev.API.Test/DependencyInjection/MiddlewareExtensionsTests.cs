using AscendDev.API.DependencyInjection;
using AscendDev.API.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AscendDev.API.Test.DependencyInjection
{
    [TestFixture]
    public class MiddlewareExtensionsTests
    {
        [Test]
        public void AddCustomMiddleware_ReturnsServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = MiddlewareExtensions.AddCustomMiddleware(services);

            // Assert
            Assert.That(result, Is.EqualTo(services));
        }

        [Test]
        public void UseCustomMiddleware_RegistersAllMiddleware()
        {
            // Arrange
            var appBuilder = new TestApplicationBuilder();

            // Act
            var result = MiddlewareExtensions.UseCustomMiddleware(appBuilder);

            // Assert
            Assert.That(result, Is.EqualTo(appBuilder));
            Assert.That(appBuilder.MiddlewareCount, Is.EqualTo(2), "Both middleware components should be registered");
        }

        // Test implementation of IApplicationBuilder that tracks middleware registration
        private class TestApplicationBuilder : IApplicationBuilder
        {
            public int MiddlewareCount { get; private set; }

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
                MiddlewareCount++;
                return this;
            }

            public IApplicationBuilder UseMiddleware<T>(params object[] args)
            {
                MiddlewareCount++;
                return this;
            }

            public IApplicationBuilder UseMiddleware(Type middleware, params object[] args)
            {
                MiddlewareCount++;
                return this;
            }
        }
    }
}