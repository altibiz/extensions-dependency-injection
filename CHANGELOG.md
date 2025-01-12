<!-- markdownlint-disable MD024 -->

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)
and adheres to [Semantic Versioning](https://semver.org/).

## [1.0.9] - 2025-01-12

### Fixed

- Wrapped stopping `IHostedService` calls in `Task.Run` to avoid deadlocks.

## [1.0.8] - 2025-01-12

### Fixed

- `IHostedService` stopping by adding `IHostApplicationLifetime` handlers.
- `ObjectDisposedException` when disposing `IHostedService` and
  `IHostedLifecycleService` in `DisposeAsync` by not stopping in `DisposeAsync`.

## [1.0.7] - 2025-01-12

### Added

- `IHostedLifecycleService` support via
  `HostedLifecycleServiceModularTenantEvents`.

### Fixed

- `IHostedService` disposal by stopping in `DisposeAsync`.

### Fixed

## [1.0.6] - 2025-01-09

### Fixed

- Special case for generic types in the `Add*AssignableTo` extension methods.
- Readme header.

## [1.0.5] - 2025-01-09

### Added

- Additional assertions for the `Add*AssignableTo` extension methods test.

### Fixed

- Changelog links.

## [1.0.4] - 2025-01-09

### Fixed

- The `publish` command in the GitHub Actions workflow.

## [1.0.3] - 2025-01-09

### Fixed

- The `publish` command in the GitHub Actions workflow.

## [1.0.2] - 2025-01-09

### Added

- Additional instructions in the PR template.

### Fixed

- The `publish` command in the GitHub Actions workflow.

## [1.0.1] - 2025-01-09

### Added

- Adding services assignable to a type to an `IServiceCollection`.
- Converting hosted services to modular tenant events.

[1.0.9]:
  https://github.com/altibiz/extensions-dependency-injection/compare/1.0.8...1.0.9
[1.0.8]:
  https://github.com/altibiz/extensions-dependency-injection/compare/1.0.7...1.0.8
[1.0.7]:
  https://github.com/altibiz/extensions-dependency-injection/compare/1.0.6...1.0.7
[1.0.6]:
  https://github.com/altibiz/extensions-dependency-injection/compare/1.0.5...1.0.6
[1.0.5]:
  https://github.com/altibiz/extensions-dependency-injection/compare/1.0.4...1.0.5
[1.0.4]:
  https://github.com/altibiz/extensions-dependency-injection/compare/1.0.3...1.0.4
[1.0.3]:
  https://github.com/altibiz/extensions-dependency-injection/compare/1.0.2...1.0.3
[1.0.2]:
  https://github.com/altibiz/extensions-dependency-injection/compare/1.0.1...1.0.2
[1.0.1]:
  https://github.com/altibiz/extensions-dependency-injection/releases/tag/1.0.1
