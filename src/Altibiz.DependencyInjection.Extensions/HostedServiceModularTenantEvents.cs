using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Modules;

namespace Altibiz.DependencyInjection.Extensions;

/// <summary>
///   A modular tenant events that starts and stops a hosted service.
/// </summary>
/// <typeparam name="THostedService">The hosted service type.</typeparam>
internal sealed class HostedServiceModularTenantEvents<THostedService>
  : ModularTenantEvents, IAsyncDisposable
  where THostedService : IHostedService
{
  private readonly THostedService _hostedService;

  private readonly
    ILogger<HostedServiceModularTenantEvents<THostedService>> _logger;

  private readonly ShellSettings _shellSettings;

#pragma warning disable SA1600 // Elements should be documented
  public HostedServiceModularTenantEvents(
    THostedService hostedService,
    ILogger<HostedServiceModularTenantEvents<THostedService>> logger,
    ShellSettings shellSettings,
    IHostApplicationLifetime hostApplicationLifetime
  )
  {
    _hostedService = hostedService;
    _logger = logger;
    _shellSettings = shellSettings;

    hostApplicationLifetime.ApplicationStopping.Register(Stop);
  }
#pragma warning restore SA1600 // Elements should be documented

  /// <inheritdoc />
  public async ValueTask DisposeAsync()
  {
    _logger.LogInformation(
      "Disposing hosted service '{HostedService}'"
      + " for tenant '{TenantName}'.",
      _hostedService.GetType().Name,
      _shellSettings.Name
    );

    if (_hostedService is IDisposable disposable)
    {
      disposable.Dispose();
    }

    if (_hostedService is IAsyncDisposable asyncDisposable)
    {
      await asyncDisposable.DisposeAsync();
    }
  }

  /// <inheritdoc />
  public override async Task ActivatedAsync()
  {
    await StartAsync();
  }

  /// <inheritdoc />
  public override async Task TerminatingAsync()
  {
    await StopAsync();
  }

  /// <inheritdoc />
  public override async Task RemovingAsync(ShellRemovingContext context)
  {
    await StopAsync();
  }

  private void Stop()
  {
    Task.Run(StopAsync).GetAwaiter().GetResult();
  }

  private async Task StartAsync()
  {
    _logger.LogInformation(
      "Starting hosted service '{HostedService}'"
      + " for tenant '{TenantName}'.",
      _hostedService.GetType().Name,
      _shellSettings.Name
    );

    await _hostedService.StartAsync(CancellationToken.None);
  }

  private async Task StopAsync()
  {
    _logger.LogInformation(
      "Stopping hosted service '{HostedService}'"
      + " for tenant '{TenantName}'.",
      _hostedService.GetType().Name,
      _shellSettings.Name
    );

    await _hostedService.StopAsync(CancellationToken.None);
  }
}
