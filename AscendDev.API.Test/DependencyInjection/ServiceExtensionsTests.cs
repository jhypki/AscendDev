using AscendDev.API.DependencyInjection;
using AscendDev.Core.CodeExecution;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Interfaces.Services;
using AscendDev.Core.Interfaces.Utils;
using AscendDev.Core.TestsExecution;
using AscendDev.Services.Services;
using AscendDev.Services.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace AscendDev.API.Test.DependencyInjection
{
    [TestFixture]
    public class ServiceExtensionsTests
    {
        [Test]
        public void AddApplicationServices_RegistersAllServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = ServiceExtensions.AddApplicationServices(services);

            // Assert
            Assert.That(result, Is.EqualTo(services));

            // Verify that all expected services are registered
            VerifyServiceRegistration<IAuthService, AuthService>(services);
            VerifyServiceRegistration<ICourseService, CourseService>(services);
            VerifyServiceRegistration<ILessonService, LessonService>(services);
            VerifyServiceRegistration<ICachingService, CachingService>(services);
            VerifyServiceRegistration<ICodeTestService, CodeTestService>(services);
            VerifyServiceRegistration<ICodeExecutionService, CodeExecutionService>(services);
            VerifyServiceRegistration<IUserProgressService, UserProgressService>(services);

            // Verify utilities
            VerifyServiceRegistration<IPasswordHasher, PasswordHasher>(services);
            VerifyServiceRegistration<IJwtHelper, JwtHelper>(services);

            // Verify executors
            VerifyServiceRegistration<ITestsExecutor, DockerTestsExecutor>(services);
            VerifyServiceRegistration<ICodeExecutor, DockerCodeExecutor>(services);
        }

        [Test]
        public void AddUtilities_RegistersSingletonUtilities()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = ServiceExtensions.AddUtilities(services);

            // Assert
            Assert.That(result, Is.EqualTo(services));

            // Verify that utilities are registered as singletons
            VerifyServiceRegistration<JwtHelper, JwtHelper>(services, ServiceLifetime.Singleton);
            VerifyServiceRegistration<PasswordHasher, PasswordHasher>(services, ServiceLifetime.Singleton);
        }

        private static void VerifyServiceRegistration<TService, TImplementation>(
            IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            var serviceDescriptor = services.FirstOrDefault(
                sd => sd.ServiceType == typeof(TService) &&
                      sd.ImplementationType == typeof(TImplementation) &&
                      sd.Lifetime == lifetime);

            Assert.That(serviceDescriptor, Is.Not.Null,
                $"Service {typeof(TService).Name} with implementation {typeof(TImplementation).Name} " +
                $"and lifetime {lifetime} was not registered.");
        }
    }
}