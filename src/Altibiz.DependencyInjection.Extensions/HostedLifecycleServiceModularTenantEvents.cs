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
internal sealed class HostedLifecycleServiceModularTenantEvents<THostedService>
  : ModularTenantEvents, IAsyncDisposable
  where THostedService : IHostedLifecycleService
{
  private readonly THostedService _hostedService;

  private readonly
    ILogger<HostedLifecycleServiceModularTenantEvents<THostedService>> _logger;

  private readonly ShellSettings _shellSettings;

#pragma warning disable SA1600 // Elements should be documented
  public HostedLifecycleServiceModularTenantEvents(
    THostedService hostedService,
    ILogger<HostedLifecycleServiceModularTenantEvents<THostedService>> logger,
    ShellSettings shellSettings,
    IHostApplicationLifetime hostApplicationLifetime
  )
  {
    _hostedService = hostedService;
    _logger = logger;
    _shellSettings = shellSettings;

    hostApplicationLifetime.ApplicationStopping.Register(Stopping);
    hostApplicationLifetime.ApplicationStopped.Register(Stopped);
  }
#pragma warning restore SA1600 // Elements should be documented

  /// <inheritdoc />
  public async ValueTask DisposeAsync()
  {
    _logger.LogInformation(
      "Disposing hosted lifecycle service '{HostedService}'"
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
  public override async Task ActivatingAsync()
  {
    await StartingAsync();
  }

  /// <inheritdoc />
  public override async Task ActivatedAsync()
  {
    await StartedAsync();
  }

  /// <inheritdoc />
  public override async Task TerminatingAsync()
  {
    await StoppingAsync();
  }

  /// <inheritdoc />
  public override async Task TerminatedAsync()
  {
    await StoppedAsync();
  }

  /// <inheritdoc />
  public override async Task RemovingAsync(ShellRemovingContext context)
  {
    await StoppingAsync();
    await StoppedAsync();
  }

  private void Stopping()
  {
    StoppingAsync().GetAwaiter().GetResult();
  }

  private void Stopped()
  {
    StoppedAsync().GetAwaiter().GetResult();
  }

  private async Task StartingAsync()
  {
    _logger.LogInformation(
      "Starting hosted lifecycle service '{HostedService}'"
      + " for tenant '{TenantName}'.",
      _hostedService.GetType().Name,
      _shellSettings.Name
    );

    await _hostedService.StartingAsync(CancellationToken.None);
  }

  private async Task StartedAsync()
  {
    await _hostedService.StartAsync(CancellationToken.None);

    await _hostedService.StartedAsync(CancellationToken.None);

    _logger.LogInformation(
      "Started hosted lifecycle service '{HostedService}'"
      + " for tenant '{TenantName}'.",
      _hostedService.GetType().Name,
      _shellSettings.Name
    );
  }

  private async Task StoppingAsync()
  {
    _logger.LogInformation(
      "Stopping hosted lifecycle service '{HostedService}'"
      + " for tenant '{TenantName}'.",
      _hostedService.GetType().Name,
      _shellSettings.Name
    );

    await _hostedService.StoppingAsync(CancellationToken.None);
  }

  private async Task StoppedAsync()
  {
    await _hostedService.StopAsync(CancellationToken.None);

    await _hostedService.StoppedAsync(CancellationToken.None);

    _logger.LogInformation(
      "Stopped hosted lifecycle service '{HostedService}'"
      + " for tenant '{TenantName}'.",
      _hostedService.GetType().Name,
      _shellSettings.Name
    );
  }
}
