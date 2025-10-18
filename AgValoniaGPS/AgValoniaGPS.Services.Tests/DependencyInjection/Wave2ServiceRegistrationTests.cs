using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using AgValoniaGPS.Desktop.DependencyInjection;
using AgValoniaGPS.Services.Guidance;
using Xunit;

namespace AgValoniaGPS.Services.Tests.DependencyInjection
{
    /// <summary>
    /// Tests for Wave 2 Guidance Line service DI registrations.
    /// Validates that services are correctly registered with proper lifetimes,
    /// can be resolved from the container, and have no circular dependencies.
    /// </summary>
    public class Wave2ServiceRegistrationTests
    {
        /// <summary>
        /// Creates a service collection with all AgValoniaGPS services registered.
        /// </summary>
        private IServiceCollection CreateServiceCollection()
        {
            var services = new ServiceCollection();
            services.AddAgValoniaServices();
            return services;
        }

        [Fact]
        public void IABLineService_ResolvesCorrectly()
        {
            // Arrange
            var services = CreateServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var service = serviceProvider.GetService<IABLineService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<ABLineService>(service);
        }

        [Fact]
        public void ICurveLineService_ResolvesCorrectly()
        {
            // Arrange
            var services = CreateServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var service = serviceProvider.GetService<ICurveLineService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<CurveLineService>(service);
        }

        [Fact]
        public void IContourService_ResolvesCorrectly()
        {
            // Arrange
            var services = CreateServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var service = serviceProvider.GetService<IContourService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<ContourService>(service);
        }

        [Fact]
        public void Wave2Services_HaveScopedLifetime()
        {
            // Arrange
            var services = CreateServiceCollection();

            // Act - Find the service descriptors for Wave 2 services
            var abLineDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IABLineService));
            var curveLineDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(ICurveLineService));
            var contourDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IContourService));

            // Assert
            Assert.NotNull(abLineDescriptor);
            Assert.Equal(ServiceLifetime.Scoped, abLineDescriptor.Lifetime);

            Assert.NotNull(curveLineDescriptor);
            Assert.Equal(ServiceLifetime.Scoped, curveLineDescriptor.Lifetime);

            Assert.NotNull(contourDescriptor);
            Assert.Equal(ServiceLifetime.Scoped, contourDescriptor.Lifetime);
        }

        [Fact]
        public void Wave2Services_CanBeInstantiated()
        {
            // Arrange
            var services = CreateServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert - Should not throw exceptions
            using (var scope = serviceProvider.CreateScope())
            {
                var abLineService = scope.ServiceProvider.GetRequiredService<IABLineService>();
                var curveLineService = scope.ServiceProvider.GetRequiredService<ICurveLineService>();
                var contourService = scope.ServiceProvider.GetRequiredService<IContourService>();

                Assert.NotNull(abLineService);
                Assert.NotNull(curveLineService);
                Assert.NotNull(contourService);
            }
        }

        [Fact]
        public void ScopedServices_ReturnSameInstanceWithinScope()
        {
            // Arrange
            var services = CreateServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            using (var scope = serviceProvider.CreateScope())
            {
                var service1 = scope.ServiceProvider.GetRequiredService<IABLineService>();
                var service2 = scope.ServiceProvider.GetRequiredService<IABLineService>();

                // Assert - Same instance within scope
                Assert.Same(service1, service2);
            }
        }

        [Fact]
        public void ScopedServices_ReturnDifferentInstancesAcrossScopes()
        {
            // Arrange
            var services = CreateServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            IABLineService service1;
            IABLineService service2;

            using (var scope1 = serviceProvider.CreateScope())
            {
                service1 = scope1.ServiceProvider.GetRequiredService<IABLineService>();
            }

            using (var scope2 = serviceProvider.CreateScope())
            {
                service2 = scope2.ServiceProvider.GetRequiredService<IABLineService>();
            }

            // Assert - Different instances across scopes
            Assert.NotSame(service1, service2);
        }

        [Fact]
        public void AllWave2Services_RegisteredInContainer()
        {
            // Arrange
            var services = CreateServiceCollection();

            // Act
            var abLineRegistered = services.Any(sd => sd.ServiceType == typeof(IABLineService));
            var curveLineRegistered = services.Any(sd => sd.ServiceType == typeof(ICurveLineService));
            var contourRegistered = services.Any(sd => sd.ServiceType == typeof(IContourService));

            // Assert
            Assert.True(abLineRegistered, "IABLineService should be registered");
            Assert.True(curveLineRegistered, "ICurveLineService should be registered");
            Assert.True(contourRegistered, "IContourService should be registered");
        }

        [Fact]
        public void ServiceProvider_BuildsWithoutCircularDependencies()
        {
            // Arrange
            var services = CreateServiceCollection();

            // Act & Assert - Should not throw circular dependency exception
            var exception = Record.Exception(() =>
            {
                var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });
            });

            Assert.Null(exception);
        }
    }
}
