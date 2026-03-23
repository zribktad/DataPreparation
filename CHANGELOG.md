# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2026-03-23

### Added
- Multi-target framework support: .NET 8.0, .NET 9.0, .NET 10.0
- SourceLink support for debuggable NuGet packages
- Symbol packages (.snupkg) for improved debugging experience
- `Directory.Build.props` for consistent build configuration
- `.editorconfig` for code style consistency
- `CHANGELOG.md` for tracking changes

### Changed
- **BREAKING**: Upgraded from NUnit 3.14.0 to NUnit 4.5.1
- Updated `Microsoft.Extensions.DependencyInjection` to 9.0.3
- Updated `Microsoft.Extensions.Logging.Abstractions` to 9.0.3
- Bumped version to 2.0.0
- Expanded NuGet package metadata (project URL, copyright, release notes)
- Enabled XML documentation file generation
- Enabled deterministic builds
- Replaced `Console.WriteLine` with structured `ILogger` calls in `DockerHelper`
- Fixed exception aggregation bug in `DataPreparationHandler.DataDown`
- Fixed typo: `CastExeption` → `CastException` in `SourceFactory`
- Replaced string interpolation in logger calls with structured logging
- Applied modern C# language features throughout

### Removed
- Removed empty `Database/Executer.cs` class
- Removed unused `using static System.Runtime.InteropServices.JavaScript.JSType` from `DataRelationStore`

## [1.1.0] - 2025-01-01

### Added
- Initial public release with NUnit 3 support
- Factory pattern for test data creation
- Async data preparation support
- DI integration via `IDataPreparationTestServices`
- BDD and Screenplay pattern support
- SQLite in-memory database support
