using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace Altibiz.DependencyInjection.Extensions;

/// <summary>
///   A modular tenant events that starts and stops a hosted service.
/// </summary>
/// <typeparam name="THostedService">The hosted service type.</typeparam>
internal sealed class HostedLifecycleServiceModularTenantEvents<THostedService>(
  THostedService hostedService,
  ILogger<HostedLifecycleServiceModularTenantEvents<THostedService>> logger,
  ShellSettings shellSettings
) : ModularTenantEvents, IAsyncDisposable
  where THostedService : IHostedLifecycleService
{
  private bool _disposed;

  private bool _started;

  private bool _starting;

  private bool _stopping;

  /// <inheritdoc />
  public async ValueTask DisposeAsync()
  {
    if (_disposed)
    {
      return;
    }

    await StoppingAsync();
    await StoppedAsync();

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
  public override async Task ActivatingAsync()
  {
    ObjectDisposedException.ThrowIf(
      _disposed,
      hostedService
    );

    await StartingAsync();
  }

  /// <inheritdoc />
  public override async Task ActivatedAsync()
  {
    ObjectDisposedException.ThrowIf(
      _disposed,
      hostedService
    );

    await StartedAsync();
  }

  /// <inheritdoc />
  public override async Task TerminatingAsync()
  {
    ObjectDisposedException.ThrowIf(
      _disposed,
      hostedService
    );

    await StoppingAsync();
  }

  /// <inheritdoc />
  public override async Task TerminatedAsync()
  {
    ObjectDisposedException.ThrowIf(
      _disposed,
      hostedService
    );

    await StoppedAsync();
  }

  private async Task StartingAsync()
  {
    if (_starting)
    {
      return;
    }

    logger.LogInformation(
      "Starting hosted lifecycle service '{HostedService}' for tenant '{TenantName}'.",
      typeof(THostedService).Name,
      shellSettings.Name
    );

    await hostedService.StartingAsync(CancellationToken.None);

    _starting = true;
  }

  private async Task StartedAsync()
  {
    if (_started)
    {
      return;
    }

    await hostedService.StartAsync(CancellationToken.None);

    await hostedService.StartedAsync(CancellationToken.None);

    logger.LogInformation(
      "Started hosted lifecycle service '{HostedService}' for tenant '{TenantName}'.",
      typeof(THostedService).Name,
      shellSettings.Name
    );

    _started = true;
  }

  private async Task StoppingAsync()
  {
    if (!_starting)
    {
      return;
    }

    if (!_started)
    {
      await StartedAsync();
    }

    logger.LogInformation(
      "Stopping hosted lifecycle service '{HostedService}' for tenant '{TenantName}'.",
      typeof(THostedService).Name,
      shellSettings.Name
    );

    await hostedService.StoppingAsync(CancellationToken.None);

    _stopping = true;
  }

  private async Task StoppedAsync()
  {
    if (!_starting)
    {
      return;
    }

    if (!_started)
    {
      await StartedAsync();
    }

    if (!_stopping)
    {
      await StoppingAsync();
    }

    await hostedService.StopAsync(CancellationToken.None);

    await hostedService.StoppedAsync(CancellationToken.None);

    logger.LogInformation(
      "Stopped hosted lifecycle service '{HostedService}' for tenant '{TenantName}'.",
      typeof(THostedService).Name,
      shellSettings.Name
    );

    _starting = false;
    _started = false;
  }
}
