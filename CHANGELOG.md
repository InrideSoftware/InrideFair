# Changelog

All notable changes to this project are documented in this file.

## [1.2.1] - 2026-06-05

### Fixed
- Startup crash (`NullReferenceException`) when the threat filter ComboBox initialized before the findings list during XAML load.

## [1.2.0] - 2026-06-05

### Added
- Timestamped report filenames and scan diff vs previous report.
- Settings screen for exclusions, custom signatures, DNS/Prefetch toggles.
- Threat list filter, double-click to open path/URL, admin restart button.
- External `signatures.json` for updatable signature packs.
- Parallel scanning of files, archives, browsers, and registry.
- Threat deduplication across scan modules.
- HTML report self-detection and expanded own-report exclusions.
- Additional unit and integration tests (24+).

### Changed
- Config heuristic requires multiple indicator categories (fewer false positives).
- Trimmed config field indicators; removed generic UI/game terms from scoring.
- DNS cache scan disabled by default (`DeepScanDns` in config).
- Browser checker scans all Chromium profiles in one pass.
- Dependency injection for loggers in browser and archive checkers.
- Version bumped to 1.2.0.

### Fixed
- Inride Fair JSON/HTML reports no longer detected as cheat configs.
- Duplicate log lines in scan journal.
- Stale CommunityToolkit dependency removed.

## [1.1.0] - 2026-03-24

### Added
- Added typed threat model usage across scan pipeline.
- Added initial automated test project and CI test execution.
- Added branded application and UI icons.
- Added refreshed release and distribution packaging flow.

### Changed
- Refactored scanner and checker composition for cleaner dependency lifetimes.
- Unified config usage around the application project config.
- Updated WPF interface with refined layout, progress feedback, smoother scrolling, and improved branding.
- Updated README and contributor guidance for the current release workflow.
- Updated executable version metadata to 1.1.0.

### Fixed
- Fixed stale icon resource loading in the title bar and sidebar.
- Fixed developer avatar rendering in the sidebar menu.
- Fixed GitHub sidebar icon rendering.
- Fixed release script version hardcodes left at 1.0.0.

## [1.0.0]

### Added
- Initial public Windows release of Inride Fair.
