using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.Modules;

namespace Altibiz.DependencyInjection.Extensions;

/// <summary>
///   Extension methods for adding services to an
///   <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
  /// <summary>
  ///   Adds singleton services of the type specified in
  ///   <paramref name="assignableTo" /> to the specified
  ///   <see cref="IServiceCollection" />.
  /// </summary>
  /// <typeparam name="T">The type of the service to register.</typeparam>
  /// <param name="services">
  ///   The <see cref="IServiceCollection" /> to add the
  ///   service to.
  /// </param>
  /// <returns>
  ///   The <see cref="IServiceCollection" /> with the services added.
  /// </returns>
  public static IServiceCollection AddSingletonAssignableTo<T>(
    this IServiceCollection services
  )
  {
    return AddAssignableTo(
      services,
      typeof(T),
      ServiceLifetime.Singleton
    );
  }

  /// <summary>
  ///   Adds singleton services of the type specified in
  ///   <paramref name="assignableTo" /> to the specified
  ///   <see cref="IServiceCollection" />.
  /// </summary>
  /// <param name="services">
  ///   The <see cref="IServiceCollection" /> to add the
  ///   service to.
  /// </param>
  /// <param name="assignableTo">The type of the service to register.</param>
  /// <returns>
  ///   The <see cref="IServiceCollection" /> with the services added.
  /// </returns>
  public static IServiceCollection AddSingletonAssignableTo(
    this IServiceCollection services,
    Type assignableTo
  )
  {
    return AddAssignableTo(
      services,
      assignableTo,
      ServiceLifetime.Singleton
    );
  }

  /// <summary>
  ///   Adds scoped services of the type specified in
  ///   <paramref name="assignableTo" /> to the specified
  ///   <see cref="IServiceCollection" />.
  /// </summary>
  /// <typeparam name="T">The type of the service to register.</typeparam>
  /// <param name="services">
  ///   The <see cref="IServiceCollection" /> to add the
  ///   service to.
  /// </param>
  /// <returns>
  ///   The <see cref="IServiceCollection" /> with the services added.
  /// </returns>
  public static IServiceCollection AddScopedAssignableTo<T>(
    this IServiceCollection services
  )
  {
    return AddAssignableTo(
      services,
      typeof(T),
      ServiceLifetime.Scoped
    );
  }

  /// <summary>
  ///   Adds scoped services of the type specified in
  ///   <paramref name="assignableTo" /> to the specified
  ///   <see cref="IServiceCollection" />.
  /// </summary>
  /// <param name="services">
  ///   The <see cref="IServiceCollection" /> to add the
  ///   service to.
  /// </param>
  /// <param name="assignableTo">The type of the service to register.</param>
  /// <returns>
  ///   The <see cref="IServiceCollection" /> with the services added.
  /// </returns>
  public static IServiceCollection AddScopedAssignableTo(
    this IServiceCollection services,
    Type assignableTo
  )
  {
    return AddAssignableTo(
      services,
      assignableTo,
      ServiceLifetime.Scoped
    );
  }

  /// <summary>
  ///   Adds transient services of the type specified in
  ///   <paramref name="assignableTo" /> to the specified
  ///   <see cref="IServiceCollection" />.
  /// </summary>
  /// <typeparam name="T">The type of the service to register.</typeparam>
  /// <param name="services">
  ///   The <see cref="IServiceCollection" /> to add the
  ///   service to.
  /// </param>
  /// <returns>
  ///   The <see cref="IServiceCollection" /> with the services added.
  /// </returns>
  public static IServiceCollection AddTransientAssignableTo<T>(
    this IServiceCollection services
  )
  {
    return AddAssignableTo(
      services,
      typeof(T),
      ServiceLifetime.Transient
    );
  }

  /// <summary>
  ///   Adds transient services of the type specified in
  ///   <paramref name="assignableTo" /> to the specified
  ///   <see cref="IServiceCollection" />.
  /// </summary>
  /// <param name="services">
  ///   The <see cref="IServiceCollection" /> to add the
  ///   service to.
  /// </param>
  /// <param name="assignableTo">The type of the service to register.</param>
  /// <returns>
  ///   The <see cref="IServiceCollection" /> with the services added.
  /// </returns>
  public static IServiceCollection AddTransientAssignableTo(
    this IServiceCollection services,
    Type assignableTo
  )
  {
    return AddAssignableTo(
      services,
      assignableTo,
      ServiceLifetime.Transient
    );
  }

  /// <summary>
  ///   Converts all services registered as <see cref="IHostedService" /> into
  ///   <see cref="ModularTenantEvents" /> ready to be activated and terminated
  ///   by OrchardCore.
  /// </summary>
  /// <param name="services">
  ///   The <see cref="IServiceCollection" /> to convert
  ///   the services from.
  /// </param>
  /// <returns>
  ///   The <see cref="IServiceCollection" /> with the services converted.
  /// </returns>
  public static IServiceCollection ConvertHostedServicesToModularTenantEvents(
    this IServiceCollection services
  )
  {
    var hostedServices = services
      .Where(
        service =>
          service.ServiceType == typeof(IHostedService) &&
          service.Lifetime == ServiceLifetime.Singleton)
      .Where(
        service =>
          !(service.ImplementationInstance?.GetType().Namespace
              ?.StartsWith(nameof(Microsoft))
            ?? service.ImplementationType?.Namespace?.StartsWith(
              nameof(Microsoft))
            ?? service.ImplementationFactory?.Method?.Module.Name?.StartsWith(
              nameof(Microsoft))
            ?? false)
          && !(service.ImplementationInstance?.GetType().Namespace
              ?.StartsWith(nameof(OrchardCore))
            ?? service.ImplementationType?.Namespace?.StartsWith(
              nameof(OrchardCore))
            ?? service.ImplementationFactory?.Method.Module.Name?.StartsWith(
              nameof(OrchardCore))
            ?? false)
      )
      .ToList();
    foreach (var hostedService in hostedServices)
    {
      services.Remove(hostedService);
      var implementationType =
        typeof(HostedServiceModularTenantEvents<>)
          .MakeGenericType(
            hostedService.ImplementationType ??
            hostedService.ServiceType);
      var modularTenantEvents =
        hostedService.ImplementationFactory is { } factory
          ? new ServiceDescriptor(
            typeof(IModularTenantEvents),
            services => ActivatorUtilities.CreateInstance(
              services,
              implementationType,
              factory(services)),
            ServiceLifetime.Singleton
          )
          : new ServiceDescriptor(
            typeof(IModularTenantEvents),
            services => ActivatorUtilities.CreateInstance(
              services,
              implementationType,
              ActivatorUtilities.CreateInstance(
                services,
                hostedService.ImplementationType ??
                hostedService.ServiceType)),
            ServiceLifetime.Singleton
          );
      modularTenantEvents.SetImplementationType(implementationType);
      services.Add(modularTenantEvents);
    }

    return services;
  }

  private static IServiceCollection AddAssignableTo(
    IServiceCollection services,
    Type assignableTo,
    ServiceLifetime lifetime
  )
  {
    var conversionTypes = AppDomain.CurrentDomain
      .GetAssemblies()
      .SelectMany(
        assembly => assembly
          .GetTypes()
          .Where(
            type =>
              !type.IsAbstract &&
              type.IsClass &&
              type.IsAssignableTo(assignableTo)))
      .OrderBy(type => type.FullName);

    foreach (var conversionType in conversionTypes)
    {
      var conversionTypeDescriptor = new ServiceDescriptor(
        conversionType,
        conversionType,
        lifetime);
      services.Add(conversionTypeDescriptor);
      foreach (var interfaceType in conversionType
        .GetAllInterfaces()
        .OrderBy(type => type.FullName))
      {
        var interfaceTypeDescriptor = new ServiceDescriptor(
          interfaceType,
          services => services.GetRequiredService(conversionType),
          lifetime);
        interfaceTypeDescriptor.SetImplementationType(conversionType);
        services.Add(interfaceTypeDescriptor);
      }
    }

    return services;
  }

  private static ServiceDescriptor SetImplementationType(
    this ServiceDescriptor serviceDescriptor,
    Type implementationType
  )
  {
    typeof(ServiceDescriptor)
      .GetField(
        "_implementationType",
        BindingFlags.Instance
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
        | BindingFlags.NonPublic)
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
      !.SetValue(serviceDescriptor, implementationType);
    return serviceDescriptor;
  }

  private static IEnumerable<Type> GetAllInterfaces(this Type type)
  {
    return type.GetInterfaces()
      .Concat(type.GetInterfaces().SelectMany(GetAllInterfaces))
      .Concat(type.BaseType?.GetAllInterfaces() ?? Array.Empty<Type>())
      .Distinct();
  }
}
