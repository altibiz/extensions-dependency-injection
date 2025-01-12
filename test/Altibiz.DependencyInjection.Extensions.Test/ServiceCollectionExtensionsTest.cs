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
        typeof(GenericImplementation<>),
        typeof(GenericImplementation<>),
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
        lifetime),
      new ServiceDescriptor(
        typeof(ThirdImplementation),
        typeof(ThirdImplementation),
        lifetime),
      new ServiceDescriptor(
        typeof(IInterface),
        typeof(ThirdImplementation),
        lifetime),
      new ServiceDescriptor(
        typeof(IHostedLifecycleService),
        typeof(ThirdImplementation),
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

    if (lifetime == ServiceLifetime.Singleton)
    {
      List<object> concrete =
      [
        serviceProvider.GetRequiredService<FirstImplementation>(),
        serviceProvider.GetRequiredService<SecondImplementation>()
      ];
      var interfaces = serviceProvider
        .GetServices<IInterface>()
        .OfType<object>()
        .ToList();
      var hostedServices = serviceProvider
        .GetServices<IHostedService>()
        .OfType<object>()
        .ToList();
      concrete.Zip(interfaces).Zip(hostedServices).Should().AllSatisfy(
        x =>
        {
          var ((concrete, @interface), hostedService) = x;
          concrete.Should().BeSameAs(@interface).And.BeSameAs(hostedService);
        });
    }
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
          || service.ImplementationType == typeof(ThirdImplementation)
          || service.ImplementationType == typeof(GenericImplementation<>)
          || (service.ImplementationType is { IsGenericType: true } &&
            service.ImplementationType?.GetGenericTypeDefinition()
            == typeof(HostedServiceModularTenantEvents<>))
          || (service.ImplementationType is { IsGenericType: true } &&
            service.ImplementationType?.GetGenericTypeDefinition()
            == typeof(HostedLifecycleServiceModularTenantEvents<>)))
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
        typeof(GenericImplementation<>),
        typeof(GenericImplementation<>),
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
        typeof(ThirdImplementation),
        typeof(ThirdImplementation),
        ServiceLifetime.Singleton),
      new ServiceDescriptor(
        typeof(IInterface),
        typeof(ThirdImplementation),
        ServiceLifetime.Singleton),
      new ServiceDescriptor(
        typeof(IModularTenantEvents),
        typeof(HostedServiceModularTenantEvents<FirstImplementation>),
        ServiceLifetime.Singleton),
      new ServiceDescriptor(
        typeof(IModularTenantEvents),
        typeof(HostedServiceModularTenantEvents<SecondImplementation>),
        ServiceLifetime.Singleton),
      new ServiceDescriptor(
        typeof(IModularTenantEvents),
        typeof(HostedLifecycleServiceModularTenantEvents<ThirdImplementation>),
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

internal class ThirdImplementation : IInterface, IHostedLifecycleService
{
  public Task StartAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task StartedAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task StartingAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task StoppedAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public Task StoppingAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}

#pragma warning disable S2326 // Unused type parameters should be removed
internal class GenericImplementation<T> : IInterface, IHostedService
#pragma warning restore S2326 // Unused type parameters should be removed
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
