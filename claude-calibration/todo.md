# TODO

Remaining work, current as of the framework migration (2026-07-19).

## Open

- **Relocate `claude-calibration/`.** The curator wants the setup fully
  by-guidelines; this folder was freestyled. Decide the guideline-compliant home
  for the memory files (state, backlog, rejected, archetypes, halls, todo) and
  move them. *Pending curator's choice.*
- **CI (GitHub Actions).** On push/PR: build/run every exhibit so they don't rot
  on SDK bumps; run `next-id.cs`, `check-links.cs`, and `gen-frontpage.cs`
  (fail if the front page is stale) - all three exit 1 on failure. Package
  exhibits (EF, DI, STJ) need restore; watch CI time.
- **Launch polish.** Badges (exhibit count), final proofread, LinkedIn poll copy
  (<=30 chars, 4 options).
- **Tags cross-index.** Once tags are consistent across exhibits, generate a
  tag/archetype index alongside the front page.

## Done

- Full framework migration to native Claude Code mechanisms: root `CLAUDE.md`,
  path-scoped `.claude/rules/`, the `add-exhibit` / `propose-exhibits` /
  `reject-exhibit` skills. Retired the homemade `conventions.md`,
  `exhibit-recipe.md`, `playbook.md`.
- Tools: `next-id.cs`, `check-links.cs`, `gen-frontpage.cs` (front page is now
  generated, list-style, no difficulty levels).
- Hall taxonomy expanded to ~30 in `halls.md`.
- Exhibits 0001-0023.
