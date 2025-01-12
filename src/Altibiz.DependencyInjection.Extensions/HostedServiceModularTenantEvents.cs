using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace Altibiz.DependencyInjection.Extensions;

/// <summary>
///   A modular tenant events that starts and stops a hosted service.
/// </summary>
/// <typeparam name="THostedService">The hosted service type.</typeparam>
internal sealed class HostedServiceModularTenantEvents<THostedService>(
  THostedService hostedService,
  ILogger<HostedServiceModularTenantEvents<THostedService>> logger,
  ShellSettings shellSettings
) : ModularTenantEvents, IAsyncDisposable
  where THostedService : IHostedService
{
  private bool _disposed;

  private bool _started;

  /// <inheritdoc />
  public async ValueTask DisposeAsync()
  {
    if (_disposed)
    {
      return;
    }

    await StopAsync();

    if (hostedService is IDisposable disposable)
    {
      disposable.Dispose();
    }

    if (hostedService is IAsyncDisposable asyncDisposable)
    {
      await asyncDisposable.DisposeAsync();
    }

    _disposed = true;
  }

  /// <inheritdoc />
  public override async Task ActivatedAsync()
  {
    ObjectDisposedException.ThrowIf(
      _disposed,
      hostedService
    );

    await StartAsync();
  }

  /// <inheritdoc />
  public override async Task TerminatingAsync()
  {
    ObjectDisposedException.ThrowIf(
      _disposed,
      hostedService
    );

    await StopAsync();
  }

  private async Task StartAsync()
  {
    if (_started)
    {
      return;
    }

    logger.LogInformation(
      "Starting hosted service '{HostedService}' for tenant '{TenantName}'.",
      typeof(THostedService).Name,
      shellSettings.Name
    );

    await hostedService.StartAsync(CancellationToken.None);

    _started = true;
  }

  private async Task StopAsync()
  {
    if (!_started)
    {
      return;
    }

    logger.LogInformation(
      "Stopping hosted service '{HostedService}' for tenant '{TenantName}'.",
      typeof(THostedService).Name,
      shellSettings.Name
    );

    await hostedService.StopAsync(CancellationToken.None);

    _started = false;
  }
}
