# Changelog

## [0.2.0] - 2026-11-12
### Added
- Skill `responsive-ui` (`AI~/skills/responsive-ui`): project-agnostic workflow for building uGUI screens from a mockup that adapt to phones, notches and tablets — SKILL.md, design-template.md, project-values-template.md (`## responsive-ui` keys), snippets.md. Uses com.quang.ui 0.10.0 (`SafeAreaFitter`, `UniformScaleFitter`, `ResponsivePreview`, `MockupProbe`) and com.quang.utilities 0.3.0 (`SafeAreaMath`). Replaces project-local `unity-responsive-ui` copies.

## [0.1.2] - 2026-10-10
### Changed
- validate-bug-ledger.ps1: delegate collection and multi-domain ledger validation to shared validator validate-bug-memory.js when present; add -ProjectPath and -RecordsDirectory parameters while preserving backward compatibility.
- bug-lifecycle-tracking.md: document collection mode, global domain search, and dependency on shared validator for collection hosts.

 - 2026-10-09
### Fixed
- Unity MCP guidance names optional Project rules without Markdown links, so hosts lacking those rules no longer report broken links (BUG-261008-f143). Policy and runtime behavior unchanged.

## [0.1.0] - 2026-10-08
### Added
- Package created (doc-only). Framework rules, tools and skills moved from PetVsMonster.
- AI~: rules compile-verification, unity-mcp, focused-test-runs, bug-lifecycle-tracking, bug-audit-workflow, csharp-style; tools gen-asmdef-matrix, validate-bug-ledger; skills find-bug, unity-csproj-sync.
