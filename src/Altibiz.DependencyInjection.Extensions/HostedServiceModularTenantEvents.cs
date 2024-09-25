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
) : ModularTenantEvents, IDisposable, IAsyncDisposable
  where THostedService : IHostedService
{
  private bool _disposed;

  private bool _started;

  /// <inheritdoc />
  public async ValueTask DisposeAsync()
  {
    if (hostedService is not IAsyncDisposable asyncDisposable)
    {
      return;
    }

    if (_disposed)
    {
      _disposed = true;
      return;
    }

    if (_started)
    {
      await hostedService.StopAsync(CancellationToken.None);
      _started = false;
    }

    await asyncDisposable.DisposeAsync();
    _disposed = true;
  }

  /// <inheritdoc />
  public void Dispose()
  {
    if (hostedService is not IDisposable disposable)
    {
      return;
    }

    if (_disposed)
    {
      _disposed = true;
      return;
    }

    if (_started)
    {
      hostedService
        .StopAsync(CancellationToken.None)
        .GetAwaiter()
        .GetResult();
      _started = false;
    }

    disposable.Dispose();
    _disposed = true;
  }

  /// <inheritdoc />
  public override async Task ActivatedAsync()
  {
    ObjectDisposedException.ThrowIf(
      _disposed,
      hostedService
    );

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

  /// <inheritdoc />
  public override async Task TerminatingAsync()
  {
    ObjectDisposedException.ThrowIf(
      _disposed,
      hostedService
    );

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
