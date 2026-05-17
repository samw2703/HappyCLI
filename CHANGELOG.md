# Changelog

All notable changes to HappyCLI will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-01

### Added
- `ICommandHandler<TCommand>` interface for defining CLI commands with strongly-typed options.
- `OptionsConfigurationBuilder<TOptions>` fluent builder supporting `string`, `int`, `List<string>`, `List<int>`, and `bool` option types.
- Mandatory option enforcement — missing mandatory flags produce actionable error output.
- Built-in help output: `-h` lists all registered commands; `<command> -h` shows per-command flag details.
- `HappyCLI.Execute` static entry point for zero-boilerplate console app setup.
- `HappyCLI.AddHappyCLI` extension method for integrating with an existing `IServiceCollection`.
- Support for injecting custom services into command handlers via the `setupCustomServices` callback.
- Custom output handler support via the `outputHandler` callback on `HappyCLI.Execute`.
- Multi-target support: .NET 8, .NET 9, and .NET 10.
