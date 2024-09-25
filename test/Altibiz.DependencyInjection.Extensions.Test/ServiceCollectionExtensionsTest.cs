using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace Altibiz.DependencyInjection.Extensions.Test;

public class ServiceCollectionExtensionsTest
{
  [Theory]
  [InlineData(ServiceLifetime.Singleton, false)]
  [InlineData(ServiceLifetime.Scoped, false)]
  [InlineData(ServiceLifetime.Transient, false)]
  [InlineData(ServiceLifetime.Singleton, true)]
  [InlineData(ServiceLifetime.Scoped, true)]
  [InlineData(ServiceLifetime.Transient, true)]
  public void AddAssignableToTest(
    ServiceLifetime lifetime,
    bool dynamic
  )
  {
    var serviceCollection = new ServiceCollection();

    var actual = dynamic
      ? lifetime switch
      {
        ServiceLifetime.Singleton => serviceCollection
          .AddSingletonAssignableTo(typeof(IInterface))
          .ToList(),
        ServiceLifetime.Scoped => serviceCollection
          .AddScopedAssignableTo(typeof(IInterface))
          .ToList(),
        ServiceLifetime.Transient => serviceCollection
          .AddTransientAssignableTo(typeof(IInterface))
          .ToList(),
        _ => throw new ArgumentOutOfRangeException(
          nameof(lifetime),
          lifetime,
          null
        )
      }
      : lifetime switch
      {
        ServiceLifetime.Singleton => serviceCollection
          .AddSingletonAssignableTo<IInterface>()
          .ToList(),
        ServiceLifetime.Scoped => serviceCollection
          .AddScopedAssignableTo<IInterface>()
          .ToList(),
        ServiceLifetime.Transient => serviceCollection
          .AddTransientAssignableTo<IInterface>()
          .ToList(),
        _ => throw new ArgumentOutOfRangeException(
          nameof(lifetime),
          lifetime,
          null
        )
      };

    List<ServiceDescriptor> expected =
    [
      new ServiceDescriptor(
        typeof(FirstImplementation),
        typeof(FirstImplementation),
        lifetime),
      new ServiceDescriptor(
        typeof(IInterface),
        typeof(FirstImplementation),
        lifetime),
      new ServiceDescriptor(
        typeof(IHostedService),
        typeof(FirstImplementation),
        lifetime),
      new ServiceDescriptor(
        typeof(SecondImplementation),
        typeof(SecondImplementation),
        lifetime),
      new ServiceDescriptor(
        typeof(IInterface),
        typeof(SecondImplementation),
        lifetime),
      new ServiceDescriptor(
        typeof(IHostedService),
        typeof(SecondImplementation),
        lifetime)
    ];

    using var serviceProvider = serviceCollection.BuildServiceProvider();

    actual.Should().HaveCount(expected.Count);
    expected.Zip(actual).Should().AllSatisfy(
      x =>
      {
        var expected = x.First;
        var actual = x.Second;

        actual.ServiceType.Should().Be(expected.ServiceType);
        actual.ImplementationType.Should().Be(expected.ImplementationType);
        actual.Lifetime.Should().Be(expected.Lifetime);

        if (actual.ImplementationFactory is null)
        {
          actual.ServiceType.Should().Be(expected.ImplementationType);
          actual.ImplementationFactory.Should().BeNull();
        }
        else
        {
          actual.ImplementationFactory(serviceProvider).Should()
            .BeOfType(expected.ImplementationType);
        }
      });
  }

  [Fact]
  public void ConvertHostedServicesToModularTenantEventsTest()
  {
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddLogging();
    serviceCollection.AddSingleton(services => new ShellSettings());

    var actual = serviceCollection
      .AddSingletonAssignableTo<IInterface>()
      .ConvertHostedServicesToModularTenantEvents()
      .Where(
        service =>
          service.ImplementationType == typeof(FirstImplementation)
          || service.ImplementationType == typeof(SecondImplementation)
          || (service.ImplementationType is { IsGenericType: true } &&
            service.ImplementationType?.GetGenericTypeDefinition()
            == typeof(HostedServiceModularTenantEvents<>)))
      .ToList();

    List<ServiceDescriptor> expected =
    [
      new ServiceDescriptor(
        typeof(FirstImplementation),
        typeof(FirstImplementation),
        ServiceLifetime.Singleton),
      new ServiceDescriptor(
        typeof(IInterface),
        typeof(FirstImplementation),
        ServiceLifetime.Singleton),
      new ServiceDescriptor(
        typeof(SecondImplementation),
        typeof(SecondImplementation),
        ServiceLifetime.Singleton),
      new ServiceDescriptor(
        typeof(IInterface),
        typeof(SecondImplementation),
        ServiceLifetime.Singleton),
      new ServiceDescriptor(
        typeof(IModularTenantEvents),
        typeof(HostedServiceModularTenantEvents<FirstImplementation>),
        ServiceLifetime.Singleton),
      new ServiceDescriptor(
        typeof(IModularTenantEvents),
        typeof(HostedServiceModularTenantEvents<SecondImplementation>),
        ServiceLifetime.Singleton)
    ];

    using var serviceProvider = serviceCollection.BuildServiceProvider();

    actual.Should().HaveCount(expected.Count);
    actual.Zip(expected).Should().AllSatisfy(
      x =>
      {
        var actual = x.First;
        var expected = x.Second;

        actual.ServiceType.Should().Be(expected.ServiceType);
        actual.ImplementationType.Should().Be(expected.ImplementationType);
        actual.Lifetime.Should().Be(expected.Lifetime);

        if (actual.ImplementationFactory is null)
        {
          actual.ServiceType.Should().Be(expected.ImplementationType);
          actual.ImplementationFactory.Should().BeNull();
        }
        else
        {
          actual.ImplementationFactory(serviceProvider).Should()
            .BeOfType(expected.ImplementationType);
        }
      });
  }
}

internal interface IInterface
{
}

internal class FirstImplementation : IInterface, IHostedService
{
  public Task StartAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}

internal class SecondImplementation : IInterface, IHostedService
{
  public Task StartAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}
