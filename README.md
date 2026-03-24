# Inride Fair

Inride Fair is a Windows desktop utility for detecting suspicious files, processes, browser artifacts, registry traces, and other indicators commonly associated with cheats or unwanted software.

Current release: **1.1.0**

## Highlights in 1.1.0

- Typed threat model instead of loosely structured dictionaries
- Cleaner dependency injection and scanner lifecycle
- Updated premium WPF interface with branded icons
- Sidebar progress tracking and smoother scrolling behavior
- Test project with automated checks
- Refreshed release and distribution scripts for GitHub publishing

## Core Features

- Process signature scanning
- File system analysis with heuristic checks
- Archive inspection
- Browser artifact analysis
- Registry and autorun checks
- JSON and HTML report generation
- WPF desktop UI for guided local scans

## Requirements

- Windows 10/11 x64
- .NET 11 SDK for development
- No runtime installation required for published release builds

## Repository Layout

```text
InrideFair.sln
InrideFair/
  Checkers/      scan modules
  Config/        app settings, signatures, version helpers
  Database/      signature and indicator sources
  Models/        typed report and threat models
  Scanner/       orchestration layer
  Services/      logging, reports, validation, DI
  UI/            WPF window, styles, interaction logic
InrideFair.Tests/
Release/         generated full release artifacts
Distribution/    generated portable user package
```

## Local Development

Restore dependencies:

```powershell
dotnet restore InrideFair.sln
```

Build the solution:

```powershell
dotnet build InrideFair.sln -c Release
```

Run tests:

```powershell
dotnet test InrideFair.sln -c Release
```

Run the desktop app:

```powershell
dotnet run --project InrideFair/InrideFair.csproj -c Release
```

## Release Packaging

Build a full self-contained release in `Release/`:

```powershell
.\build_release.ps1
```

or:

```powershell
.\publish_release.ps1
```

Build a compact portable package in `Distribution/`:

```powershell
.\publish_distribution.ps1
```

## Reports and Logs

- Runtime logs are written into `Logs/`
- JSON and HTML reports are produced next to the executable or working directory
- False positives are possible and should be manually reviewed

## Security Notes

- The application does not delete files automatically
- Running as administrator improves scan coverage
- Some antivirus products may flag self-contained single-file executables heuristically

## GitHub Releases

GitHub Actions creates release artifacts automatically when a tag like `v1.1.0` is pushed.

## License

MIT. See [LICENSE](LICENSE).
