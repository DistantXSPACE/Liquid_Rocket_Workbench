# Liquid Rocket Workbench

Liquid Rocket Workbench is a Windows desktop application for exploring
idealized, one-dimensional liquid rocket engine and nozzle performance. The
application targets .NET 10 and uses WPF for its interface.

The first release is an educational and preliminary trade-study tool. It is not
intended for hardware certification or safety-critical design.

## Requirements

- Windows with .NET 10 desktop support
- .NET 10 SDK
- Visual Studio with the **.NET desktop development** workload, or the .NET CLI

The repository's `global.json` accepts the latest installed .NET 10 feature band
at or above SDK 10.0.100. SDK 10.0.302 was used for the initial scaffold.

## Build and test

Run from the repository root:

```powershell
dotnet restore LiquidRocketWorkbench.slnx
dotnet build LiquidRocketWorkbench.slnx --no-restore
dotnet test LiquidRocketWorkbench.slnx --no-build --no-restore
```

Run the application:

```powershell
dotnet run --project src/LiquidRocketWorkbench.App/LiquidRocketWorkbench.App.csproj
```

You can also open `LiquidRocketWorkbench.slnx` in Visual Studio and use Test
Explorer to run the tests.

## Publish the Windows release

Create the self-contained Windows x64 folder, ZIP, and SHA-256 sidecar:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Publish-Release.ps1
```

The runnable folder is written to
`publish/LiquidRocketWorkbench-1.0.0-win-x64/`; launch
`LiquidRocketWorkbench.exe` from that folder. The ZIP can be copied to another
Windows 10/11 x64 machine without installing .NET. Keep the executable and its
adjacent runtime files together.

See [docs/release.md](docs/release.md) for archive verification, release
screenshots, run instructions, and the complete known-limits statement.

For a task-oriented walkthrough of every input, result, plot, warning, keyboard
shortcut, and saved comparison, see the
[Liquid Rocket Workbench User Guide](docs/user-guide.md).

## Current input workflow

The operating-point form accepts convenient metric display units and validates
each field as it is edited. Chamber pressure uses MPa, ambient pressure uses
kPa, nozzle diameters use mm, and all values are converted to canonical SI
units before they enter the calculation core. Target mass flow and burn duration
are optional; the target remains a comparison value rather than the
authoritative flow.

The thermodynamic preset selector can apply a sourced propellant label, mixture
ratio, chamber temperature, specific-heat ratio, and gas constant. The applied
values remain normal editable inputs; changing one switches the selector to
**Custom** without discarding the edit. The UI shows each preset's basis, and
`docs/references.md` records its values and derivation. Presets are
constant-property starting points, not combustion-chemistry predictions.

The calculation button is enabled when every input is valid. A successful run
shows selected-ambient, vacuum, and sea-level thrust and specific impulse,
geometry-driven ideal mass flow, and the selected nozzle expansion state.
Editing any input clears the prior headline result so it cannot be mistaken for
the current operating point.

The result area has one explicit workflow state at a time: an empty prompt,
an indeterminate loading state, a structured error with retry text, or a
successful result. Calculation runs asynchronously so the WPF window remains
responsive, and duplicate calculation requests are ignored while one is in
progress. Inputs are disabled during the short calculation; if they are changed
programmatically while work is in flight, the obsolete result is discarded.

Successful results also include nozzle geometry, exit state, flow split,
optional target-flow discrepancy, burn consumption, characteristic velocity,
and thrust components for selected ambient, vacuum, and sea level. Model
diagnostics retain their Core severity, stable code, and message so ideal-model,
overexpansion, and target-flow limitations remain visible beside the result.

The engine/nozzle station card is a responsive educational schematic rather
than a scaled hardware drawing. It labels the injector, chamber, choked throat,
and supersonic exit; current input values remain visible before calculation,
and a successful run adds the solved exit Mach number, pressure, temperature,
and velocity.

Successful results also include pressure, temperature, and Mach small-multiple
profiles on a normalized chamber-to-exit axis. Their chamber, Mach-1 throat, and
exit values come from the same Core solution. Smooth interpolation makes the
station-to-station trend visible but is explicitly labeled as neither a solved
hardware contour nor a CFD, shock, separation, or boundary-layer prediction.

The altitude card evaluates ideal thrust from sea level through 50 km
geopotential altitude using the U.S. Standard Atmosphere 1976. Each checkpoint
pairs altitude, ambient pressure, and thrust; a selected-pressure marker appears
when the active ambient pressure has an equivalent inside that range. The curve
holds the solved nozzle state fixed and is not live weather, a trajectory, or an
off-design shock/separation prediction.

After a successful calculation, the operating-point comparison can save up to
four read-only snapshots. Each card preserves the source inputs and selected,
vacuum, and sea-level performance so later edits and recalculations can be
compared side by side. Saved points last for the current application session
only. Saving, removing, or clearing snapshots never applies values to or changes
the active input form.

## Accessibility and display behavior

Every editable field and action exposes a descriptive automation name. Inline
validation is also attached to its field as automation help text, while input
and application status changes use polite live-region announcements. Keyboard
shortcuts are `Alt+C` to calculate, `Alt+S` to save the current result, and
`Alt+L` to clear saved comparisons.

Displayed engineering values remain metric and use the active Windows culture
for decimal and grouping separators, including values projected after the
asynchronous calculation completes. At the 1040×680 minimum window size, the
calculation workspace avoids horizontal overflow and the workflow navigation
scrolls independently so the persistent model-boundary summary remains visible.

## Solution structure

```text
src/LiquidRocketWorkbench.App/          WPF interface and composition root
src/LiquidRocketWorkbench.Core/         UI-independent engineering domain
tests/LiquidRocketWorkbench.App.Tests/  App unit, WPF end-to-end, and release-quality tests
tests/LiquidRocketWorkbench.Core.Tests/ Automated tests for Core
```

See `AGENTS.md` for the living project context, technical decisions, completion
tracker, and current handoff.
