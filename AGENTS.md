# Liquid Rocket Workbench — Living Project Context

> This is the shared source of truth for humans and coding agents working in this
> repository. Keep it accurate, concise, and updated in the same change as the
> code it describes.

## 1. Project Snapshot

| Field | Current value |
|---|---|
| Product | Liquid Rocket Engine Performance Workbench |
| Repository state | MVP complete; v1.0.0 is published as a verified self-contained Windows x64 release |
| Current phase | Phase 4 complete — Release quality |
| Current focus | Maintain the released MVP and prioritize post-MVP work deliberately |
| MVP completion | 100% (24 of 24 tracked MVP tasks complete) |
| Last context update | 2026-07-26 |
| Next recommended task | Define the post-MVP backlog; no tracked MVP task remains |

### Status legend

- `[ ]` Not started
- `[~]` In progress
- `[x]` Complete and verified
- `[!]` Blocked; explain the blocker in **Open Questions and Blockers**
- `[-]` Intentionally removed from scope; record why in **Decision Log**

The percentage above is based on completed tracked MVP tasks, not an estimate of
engineering effort. When adding or removing an MVP task, update both numbers.

## 2. Instructions for Every Agent

Before making changes:

1. Read this document completely.
2. Inspect the repository and do not assume this file map is perfectly current.
3. Check **Current Work**, **Open Questions and Blockers**, and **Decision Log**.
4. Move only the task you are actively handling to `[~]`.
5. Do not silently change a confirmed engineering or product decision.

Before handing work back:

1. Run the checks appropriate to the change.
2. Mark a task `[x]` only when its acceptance criteria pass.
3. Add concise verification evidence to the task row.
4. Update the project snapshot, file map, decisions, risks, and handoff notes if
   the change affects them.
5. Leave the repository in a buildable state, or clearly mark and explain the
   blocker.

### Rules for trustworthy completion tracking

- A file existing does not mean its task is complete.
- A calculation task requires automated tests, including boundary or failure
  cases, before it can be marked complete.
- A UI task requires the relevant workflow to run, not merely compile.
- Use SI units inside the calculation domain. Convert only at the UI/import/export
  boundaries.
- Record the source and assumptions for every engineering equation.
- Never present an idealized estimate as measured or high-fidelity performance.
- Do not add combustion chemistry to the first version. Thermodynamic inputs are
  supplied by the user or a preset.
- If work reveals that a task is too broad, split it into smaller rows while
  preserving its original acceptance criteria.

## 3. Product Vision

Build a C# desktop application for exploring and comparing idealized liquid
rocket engine operating conditions. It should make the equations visible enough
to be educational while producing useful first-order engineering estimates.

The workbench should let a user enter a propellant label or preset, chamber and
ambient conditions, mass flow, nozzle geometry, and gas properties. It should
calculate engine/nozzle performance, show warnings when inputs or model
assumptions are questionable, and visualize how the solution behaves through the
nozzle and across operating conditions.

### Intended users

- Engineering students learning compressible flow and propulsion
- Early-career propulsion engineers performing first-order trade studies
- Reviewers evaluating the project as a C# and engineering portfolio piece

### Success criteria for the first release

- A user can enter a complete operating point and calculate a result without
  editing code.
- Results are unit-safe, deterministic, and accompanied by assumptions/warnings.
- Core equations agree with documented reference cases within defined tolerances.
- Invalid or nonphysical inputs produce actionable validation messages.
- The interface clearly distinguishes vacuum, ambient, and sea-level results.
- The calculation library is independent of the desktop UI and covered by tests.

## 4. MVP Scope

### Inputs

- Propellant combination or descriptive label
- Chamber pressure, `Pc`
- Chamber/combustion temperature, `Tc`
- Mixture ratio, `O/F`
- Optional target propellant mass flow rate, `mdot_target`, used for a consistency
  comparison rather than as the authoritative flow
- Nozzle throat diameter or throat area
- Nozzle exit diameter or exit area
- Specific heat ratio, `gamma`
- Specific gas constant, `R`
- Ambient pressure, `Pa`
- Standard gravity, `g0`, fixed by the application unless deliberately overridden

### Calculated outputs

- Throat area, exit area, and expansion ratio
- Choked mass-flow consistency check
- Exit Mach number
- Exit static pressure and temperature
- Ideal exit velocity
- Momentum thrust and pressure thrust
- Total thrust at the selected ambient pressure
- Vacuum thrust and sea-level thrust
- Specific impulse at selected ambient, vacuum, and sea-level conditions
- Characteristic velocity, `c*`
- Thrust coefficient, `Cf`
- Oxidizer and fuel mass-flow rates from total flow and mixture ratio
- Propellant mass consumption for a user-supplied burn duration, when available
- Nozzle state: underexpanded, ideally expanded, or overexpanded

### MVP visuals

- Labeled engine/nozzle stations: injector, chamber, throat, and exit
- Summary results and validation/warning panel
- Pressure, temperature, and Mach number profiles along a normalized nozzle axis
- Thrust versus altitude/ambient pressure

### Explicitly deferred

- Chemical equilibrium or finite-rate combustion
- Automatic NASA CEA integration
- Real-gas and multiphase flow
- Boundary-layer, divergence, combustion, and nozzle erosion losses
- Shock location or separated-flow prediction in an overexpanded nozzle
- Transient start/stop behavior
- Feed systems, regenerative cooling, injector sizing, and test-data analysis
- Hardware design certification or safety-critical use

These are possible later workbench modules, not implied MVP requirements.

## 5. Engineering Model and Conventions

The first release uses one-dimensional, steady, adiabatic, isentropic ideal-gas
flow through a converging-diverging nozzle. Flow is assumed choked at the throat.
Gas properties are constant over the nozzle solution.

### Canonical units

| Quantity | Internal unit |
|---|---|
| Pressure | Pa |
| Temperature | K |
| Mass flow | kg/s |
| Length | m |
| Area | m² |
| Velocity | m/s |
| Thrust | N |
| Specific impulse | s |
| Specific gas constant | J/(kg·K) |

The MVP displays convenient metric units such as MPa, bar, kPa, mm, and kN.
Domain calculations must receive and return strongly identified base SI values.
Switchable US customary display units are deferred until after the MVP.

### Core relationships to implement and cite

The equations below define intent, not a substitute for source citations in code
or tests.

- Circular area: `A = pi * d² / 4`
- Expansion ratio: `epsilon = Ae / At`
- Oxidizer flow: `mdot_ox = mdot * (O/F) / (1 + O/F)`
- Fuel flow: `mdot_fuel = mdot / (1 + O/F)`
- Isentropic area–Mach relation, solved on the supersonic branch for the exit
- Exit temperature: `Te/Tc = 1 / (1 + (gamma - 1) * Me² / 2)`
- Exit pressure: `Pe/Pc = (Te/Tc)^(gamma / (gamma - 1))`
- Exit velocity: `Ve = Me * sqrt(gamma * R * Te)`
- Thrust: `F = mdot * Ve + (Pe - Pa) * Ae`
- Specific impulse: `Isp = F / (mdot * g0)`
- Ideal characteristic velocity:
  `c* = sqrt(R * Tc / gamma) * ((gamma + 1) / 2)^((gamma + 1) / (2 * (gamma - 1)))`
- Thrust coefficient: `Cf = F / (Pc * At)`

The geometry-driven model calculates ideal choked mass flow from chamber
pressure, throat area, chamber temperature, and gas properties. A user-entered
mass flow is an optional target used only for comparison. Performance outputs use
the calculated ideal mass flow. When a target is present, show its absolute and
relative discrepancy from the calculated flow and issue a warning outside a
documented tolerance.

### Nozzle-state classification

Compare exit pressure `Pe` with ambient pressure `Pa` using a 2% symmetric
relative tolerance rather than exact floating-point equality. Define the shared
relative difference as `abs(Pe - Pa) / max(abs(Pe), abs(Pa))`. If both pressures
are zero, treat the difference as zero:

- Relative difference `<= 0.02`: ideally expanded
- Otherwise, `Pe > Pa`: underexpanded
- Otherwise, `Pe < Pa`: overexpanded

The classification does not assert that attached flow is physically maintained.
Very low exit-to-ambient pressure ratios should warn that the ideal isentropic
model does not predict separation or shocks.

### Numerical expectations

- Reject NaN, infinity, zero/negative dimensional inputs where nonphysical.
- Require `gamma > 1`, `R > 0`, `Tc > 0`, `Pc > 0`, `mdot > 0`, and `O/F > 0`.
- Require `Pc > Pa >= 0` for the assumed forward-expanding nozzle flow.
- Require `Ae >= At > 0` for the MVP converging-diverging nozzle model.
- Use a bounded, deterministic root solver for the supersonic exit Mach number.
- Report convergence failures without crashing or returning plausible-looking
  default values.
- Define tolerances in one shared location and test values near each boundary.

## 6. Proposed Technical Shape

The application stack has been confirmed:

- **Confirmed:** .NET 10 LTS
- **Confirmed:** WPF desktop UI using MVVM
- **Confirmed:** Windows-only desktop support
- **Proposed:** A plotting library selected after license and maintenance review
- **Confirmed:** C# is the implementation language
- **Confirmed:** Calculation code remains independent of the UI

### Suggested solution projects

```text
LiquidRocketWorkbench.slnx
src/
  LiquidRocketWorkbench.App/          Desktop UI and composition root
  LiquidRocketWorkbench.Core/         Domain models, validation, calculations
tests/
  LiquidRocketWorkbench.Core.Tests/   Unit and reference-case tests
docs/
  references.md                       Equation sources and validation cases
```

Dependencies should point inward:

```text
App  ──>  Core
Tests ──> Core
Core ──>  no UI framework
```

Do not create all proposed projects merely to match this diagram. Confirm the
stack first and add a project when it has a clear responsibility.

### Suggested core model boundaries

- `EngineInputs`: complete, immutable operating-point input
- `NozzleGeometry`: throat/exit dimensions and derived areas
- `GasProperties`: `gamma`, specific gas constant, chamber temperature
- `EnginePerformanceResult`: calculated values plus diagnostics
- `ValidationIssue`: severity, field, stable code, and user-facing message
- `IEnginePerformanceCalculator`: calculation boundary used by the UI
- Unit/value types or an established units library to prevent accidental mixing

## 7. Planned User Workflow

1. Select a propellant preset or choose custom gas properties.
2. Enter chamber conditions, mixture ratio, nozzle geometry, and ambient
   pressure; optionally enter a target mass flow for comparison.
3. Resolve inline validation errors.
4. Run the calculation.
5. Review headline thrust and specific-impulse results.
6. Inspect detailed values, model warnings, and nozzle plots.
7. Change a parameter and compare the recalculated operating point.

Example demonstration case (a product target, not yet a validation reference):

```text
Propellants:          LOX / Methane
Chamber pressure:     8 MPa
Mixture ratio:        3.5
Target mass flow:     20 kg/s (optional comparison)
Expansion ratio:      40
Illustrative output:  about 72 kN, 355 s vacuum Isp, 305 s sea-level Isp
```

Those illustrative outputs must not become hard-coded expectations. They are
incomplete without temperature, gas properties, throat size, and loss
assumptions.

## 8. Delivery Plan and Completion Tracker

### Phase 0 — Define and scaffold (4/4)

- [x] `P0-01` Confirm target .NET version, desktop UI framework, and supported OS.
- [x] `P0-02` Create App, Core, and Core.Tests projects with correct references.
- [x] `P0-03` Add baseline build/test commands and repository ignore settings.
- [x] `P0-04` Create `docs/references.md` with primary equation and reference-case
      sources.

### Phase 1 — Calculation foundation (7/7)

- [x] `P1-01` Implement input, geometry, gas-property, result, and diagnostic
      models.
- [x] `P1-02` Implement field and cross-field validation.
- [x] `P1-03` Implement areas, expansion ratio, and mixture-based flow split.
- [x] `P1-04` Implement the supersonic area–Mach solver with convergence handling.
- [x] `P1-05` Implement exit state, velocity, `c*`, thrust, `Cf`, and `Isp`.
- [x] `P1-06` Implement nozzle-state classification and model-limit warnings.
- [x] `P1-07` Validate against documented analytical/reference cases.

### Phase 2 — Usable desktop workflow (6/6)

- [x] `P2-01` Implement the application shell and MVVM composition.
- [x] `P2-02` Implement input form with units and inline validation.
- [x] `P2-03` Implement calculation command and headline result summary.
- [x] `P2-04` Implement detailed results and warnings.
- [x] `P2-05` Add propellant labels and editable thermodynamic presets.
- [x] `P2-06` Add loading, empty, error, and successful-result UI states.

### Phase 3 — Visualization and comparison (4/4)

- [x] `P3-01` Add labeled nozzle/station diagram.
- [x] `P3-02` Add pressure, temperature, and Mach profiles.
- [x] `P3-03` Add thrust versus altitude/ambient-pressure plot.
- [x] `P3-04` Add operating-point comparison without corrupting the active input.

### Phase 4 — Release quality (3/3)

- [x] `P4-01` Add end-to-end happy-path and invalid-input tests.
- [x] `P4-02` Complete accessibility, layout, units, and numerical-format review.
- [x] `P4-03` Publish a documented runnable build with screenshots and known limits.

### Task evidence

Add or update a row when a task becomes active or complete. Keep evidence short:
a test command, test name, screenshot path, or manual verification note.

| Task | Status | Owner | Acceptance criteria | Verification evidence |
|---|---|---|---|---|
| P0-01 | Complete | User | Decision is recorded in `D-001` and `D-002` | User confirmed WPF, Windows-only, and .NET 10 LTS on 2026-07-25; SDK 10.0.302 detected |
| P0-02 | Complete | Codex | Solution builds and tests discover successfully | `dotnet build`: 0 warnings/errors; `dotnet test`: 1/1 passed on 2026-07-25 |
| P0-03 | Complete | Codex | Clean clone can build/test using documented commands | After `dotnet clean`: restore succeeded, build had 0 warnings/errors, and 1/1 tests passed on 2026-07-25 |
| P0-04 | Complete | Codex | Each core equation and validation case has a source | `docs/references.md`: 10 sources, 18 equation IDs, 8 policies, and 5 independently recomputed validation cases |
| P1-01 | Complete | Codex | Immutable SI-named inputs and structured results/diagnostics compile and are tested | Build: 0 warnings/errors; 8/8 tests passed on 2026-07-25 |
| P1-02 | Complete | Codex | Every field and dependent constraint returns stable actionable diagnostics | Build: 0 warnings/errors; 16/16 tests passed on 2026-07-25 |
| P1-03 | Complete | Codex | EQ-GEO-01/02 and EQ-MIX-01/02/03 match VC-01 and reject invalid numerical calls | Build: 0 warnings/errors; 36/36 tests passed on 2026-07-25 |
| P1-04 | Complete | Codex | Supersonic EQ-FLOW-02 solve is bounded, deterministic, residual-checked, and reports convergence failures without a fallback Mach value | Restore current; build: 0 warnings/errors; 60/60 tests passed, including 24 focused solver cases, on 2026-07-25 |
| P1-05 | Complete | Codex | Geometry-driven flow, optional flow accounting, exit state/velocity, `c*`, thrust components, `Isp`, and `Cf` match VC-02 through numerical VC-04 and reject invalid/nonrepresentable calls | Restore current; build: 0 warnings/errors; 148/148 tests passed, including 88 P1-05 cases, on 2026-07-25 |
| P1-06 | Complete | Codex | The 2% symmetric policy matches VC-05 and ambient results; deterministic warnings cover ideal assumptions, target differences above 5%, and the conservative `Pe/Pa < 0.4` model-limit proxy | Restore current; build: 0 warnings/errors; 184/184 tests passed, including 36 P1-06 cases, on 2026-07-25 |
| P1-07 | Complete | Codex | The composed calculation boundary matches VC-02 through VC-05, attaches warnings, and returns structured validation/solver/numerical failures without partial results | Restore current; build: 0 warnings/errors; 195/195 tests passed, including 11 P1-07 cases, on 2026-07-25 |
| P2-01 | Complete | Codex | App startup composes the Core boundary into a view model, and the nonblank shell starts as a responsive WPF window | Restore current; build: 0 warnings/errors; 195/195 tests passed; smoke launch responsive with title `Liquid Rocket Workbench` on 2026-07-25 |
| P2-02 | Complete | Codex | Metric display fields convert to canonical SI, optional values remain optional, and parsing/Core validation issues appear inline while editing | Restore current; build: 0 warnings/errors; 204/204 tests passed, including 9 App input cases; formatting verified; smoke launch responsive with title `Liquid Rocket Workbench` on 2026-07-25 |
| P2-03 | Complete | Codex | Valid inputs execute the composed Core boundary; selected-ambient, vacuum, and sea-level headlines render; failures remain structured; input edits clear stale results | Restore current; build: 0 warnings/errors; 217/217 tests passed, including 13 new command/state cases; formatting verified; UI Automation invoked Calculate and found `21.31 kN`, `236.4 s`, and `29.27 kN` in the responsive window on 2026-07-25 |
| P2-04 | Complete | Codex | Every MVP result group is presented in documented metric units; optional comparisons remain explicit; successful-result diagnostics preserve Core order, severity, code, field, and message | Restore current; build: 0 warnings/errors; 225/225 tests passed, including 8 new detailed-projection cases; formatting verified; UI Automation invoked Calculate and found `19.635 cm²`, `Mach 4.3555`, `3 warnings`, and the severe-overexpansion code in the responsive window on 2026-07-26 |
| P2-05 | Complete | Codex | A user can choose a sourced propellant/thermodynamic preset, see its provenance and assumptions, edit the applied label and thermodynamic values, and continue with a valid custom operating point | Restore current; build: 0 warnings/errors; 232/232 tests passed, including 7 new preset/catalog/state cases; changed C# formatting verified; UI Automation selected NASA LOX/methane, found the applied label, `3644.44444444444 K`, `gamma 1.182`, and `R 402.157968052826`, confirmed an edit switches to Custom, calculated successfully, and found the result label in the responsive window on 2026-07-26 |
| P2-06 | Complete | Codex | Empty, loading, error, and success are mutually exclusive, accessible UI states; calculation is non-reentrant, keeps the window responsive, and never publishes a stale result after inputs change | Restore current; build: 0 warnings/errors; 242/242 tests passed, including 20 App command/workflow-state cases; changed C# formatting verified; UI Automation found empty, indeterminate loading, success, edit-reset empty, and structured error states, confirmed the calculate button was disabled while loading, and kept the window responsive on 2026-07-26 |
| P3-01 | Complete | Codex | A responsive vector schematic labels injector, chamber, throat, and exit; annotations follow active inputs and successful results, remain accessible, and clearly state that geometry is not to scale | Restore current; build: 0 warnings/errors; 249/249 tests passed, including 7 new diagram/state cases; UI Automation verified all four stations, pending and solved annotations, Mach 4.356, and containment at 1040×680 on 2026-07-26 |
| P3-02 | Complete | Codex | Successful results include accessible pressure, temperature, and Mach profiles along a normalized chamber-to-exit axis; endpoints agree with the active Core solution and interpolation/model limits remain explicit | Restore current; build: 0 warnings/errors; 277/277 tests passed, including 28 new Core/App profile, boundary, presentation, converter, and state cases; UI Automation verified hidden-before-success behavior, all station summaries, model-limit text, and containment at 1040×680; all three traces visually inspected on 2026-07-26 |
| P3-03 | Complete | Codex | Successful results include an accessible thrust-versus-standard-altitude curve with paired ambient pressures; standard-atmosphere checkpoints and thrust endpoints are tested, selected/vacuum context remains explicit, and model limits stay visible | Restore current; build: 0 warnings/errors; 322/322 tests passed, including 45 new atmosphere, sweep, model, projection, accessibility, and workflow cases; UI Automation verified hidden-before-success behavior, six paired checkpoints, selected marker, source/model text, and containment at 1040×680; curve visually inspected on 2026-07-26 |
| P3-04 | Complete | Codex | Users can save, compare, remove, and clear immutable successful-result snapshots without any command writing to active inputs; snapshots survive input edits/recalculations, remain bounded and accessible, and preserve the values from their source result | Restore current; build: 0 warnings/errors; 337/337 tests passed, including 15 new snapshot, command, isolation, persistence, and bound cases; changed C# formatting verified; UI Automation saved distinct 101.325 and 90 kPa points, verified accessible summaries, removed Point 1 without changing the active 90 kPa input, and confirmed containment at 1040×680; two-card layout visually inspected on 2026-07-26 |
| P4-01 | Complete | Codex | Automated end-to-end tests exercise the production happy path and invalid-input recovery from editable display values through real WPF controls, bindings, the Core calculator, and rendered workflow state | Restore current; build: 0 warnings/errors; 340/340 tests passed, including 3 WPF happy-path, malformed-input, cross-field-validation, and recovery cases; changed C# formatting verified on 2026-07-26 |
| P4-02 | Complete | Codex | The compiled workflow passes an accessibility, minimum-layout, metric-unit, culture-aware numerical-format, and text/essential-graphic contrast review with automated regression coverage | Restore current; build: 0 warnings/errors; 344/344 tests passed, including 4 new real-window accessibility, 1040×680 containment, German-culture/metric-unit, and WCAG AA theme cases; UI Automation found 11/11 named edit controls and a successful result at minimum size; `artifacts/p4-02/minimum-window-success.png` visually inspected; changed C# formatting verified on 2026-07-26 |
| P4-03 | Complete | Codex | A reproducible documented Windows publish produces a runnable release artifact with durable workflow screenshots, explicit requirements, and known engineering limits | Release build: 0 warnings/errors; 344/344 tests passed; self-contained v1.0.0 ZIP contains 407 files, release guide, references, and 4 screenshots; SHA-256 `8980a7e9b5fbafff14c5cef81d7e26144c82a42de7a0792a4104a34d6e79add1`; fresh-extraction UI Automation calculated successfully and exited 0 on 2026-07-26 |

## 9. Current Work

No implementation task is currently assigned. The tracked MVP is complete.

When new work starts, add entries in this form:

```text
- P1-04 — Agent/name — Started YYYY-MM-DD
  - Goal: Implement the supersonic area–Mach solver.
  - Files: anticipated or currently edited paths
  - Notes: coordination details or constraints
```

Remove the entry when the task is completed or returned to not-started status.
An agent must not claim a task here if another active entry already owns it
unless collaboration is explicit.

## 10. Verification Commands

Run these commands from the repository root:

```powershell
dotnet restore LiquidRocketWorkbench.slnx
dotnet build LiquidRocketWorkbench.slnx --no-restore
dotnet test LiquidRocketWorkbench.slnx --no-build --no-restore
```

Verified after `dotnet clean` on 2026-07-25 with SDK 10.0.302: restore succeeds
and the solution builds with zero warnings and errors. Most recently reverified
in Release after P4-03 on 2026-07-26: all 344 tests pass, including seven
real-window WPF cases. The final v1.0.0 archive checksum matched, and its
freshly extracted executable completed the production calculation workflow and
exited with code 0.

## 11. Repository File Map

```text
AGENTS.md                                      Living context and completion tracker
README.md                                      Developer setup, build, test, and run guide
LiquidRocketWorkbench.slnx                     Solution containing App, Core, and tests
global.json                                    .NET 10 SDK feature-band selection
Directory.Build.props                          Shared compilation and analysis settings
.editorconfig                                  Repository-wide text and C# style rules
.gitignore                                     Visual Studio, .NET, test, and OS exclusions
docs/references.md                             Equation traceability and fixed validation cases
docs/release.md                                v1.0.0 requirements, usage, verification, and known limits
docs/screenshots/                              Durable workflow screenshots from the published app
scripts/Publish-Release.ps1                    Reproducible self-contained Windows release packaging
scripts/Capture-ReleaseScreenshots.ps1         Published-app UI smoke check and screenshot capture
src/LiquidRocketWorkbench.App/Properties/PublishProfiles/Windows-x64.pubxml  Release publish defaults
src/LiquidRocketWorkbench.App/App.xaml(.cs)    Theme resources and application composition root
src/LiquidRocketWorkbench.App/MainWindow.xaml  Responsive workflow shell
src/LiquidRocketWorkbench.App/ViewModels/      Shell, async workflow state/commands, sourced presets, validated inputs, immutable comparison snapshots, result projections, station annotations, and profile/altitude chart state
src/LiquidRocketWorkbench.App/Converters/      WPF presentation converters for code-native charts
src/LiquidRocketWorkbench.App/Views/           Reusable input, result, comparison, station-diagram, nozzle-profile, and thrust-altitude views
src/LiquidRocketWorkbench.Core/Models/         Immutable SI inputs, structured results, normalized nozzle profiles, and standard-atmosphere thrust points
src/LiquidRocketWorkbench.Core/Diagnostics/    Stable validation issue and severity models
src/LiquidRocketWorkbench.Core/Validation/     Field/cross-field operating-point validation
src/LiquidRocketWorkbench.Core/Calculations/   Sourced ideal-flow/performance calculators, normalized profile sampler, standard atmosphere/sweep, and numerical policies
src/LiquidRocketWorkbench.Core/Calculations/EnginePerformanceCalculator.cs  Composed UI-independent calculation boundary
src/LiquidRocketWorkbench.Core/Calculations/IEnginePerformanceCalculator.cs  Application-facing Core calculation contract
src/LiquidRocketWorkbench.Core/PhysicalConstants.cs  Shared sourced physical constants
tests/LiquidRocketWorkbench.App.Tests/         App unit tests plus dedicated-STA real-window WPF end-to-end tests
tests/LiquidRocketWorkbench.Core.Tests/        xUnit tests mirroring Core responsibilities
artifacts/p4-02/                               Ignored local release-review screenshots
publish/                                       Ignored generated release folder, ZIP, and checksum
```

Update this map for important entry points and directories. Do not list every
generated or implementation file.

## 12. Decision Log

Never rewrite the history of a confirmed decision. Add a new row that supersedes
the old decision.

| ID | Date | Status | Decision | Reason / consequence |
|---|---|---|---|---|
| D-001 | 2026-07-25 | Confirmed | Use WPF with MVVM for a Windows-only desktop UI. | Best fit for this Windows engineering workbench and Visual Studio workflow. |
| D-002 | 2026-07-25 | Confirmed | Target .NET 10 LTS. | SDK 10.0.302 is installed; .NET 10 is the active LTS release. |
| D-003 | 2026-07-25 | Confirmed | Use an ideal-gas, 1D, steady, adiabatic, isentropic MVP model. | Keeps the first release explainable and testable; fidelity limits must remain visible. |
| D-004 | 2026-07-25 | Rejected | Treat user mass flow as the performance input and warn when it disagrees with ideal choked flow implied by `Pc`, `At`, and gas state. | Replaced by the geometry-driven model in `D-006`. |
| D-005 | 2026-07-25 | Confirmed | Use SI units inside the domain layer. | Prevents hidden conversion errors and provides one calculation convention. |
| D-006 | 2026-07-25 | Confirmed | Use geometry-driven ideal mass flow; accept entered mass flow only as an optional comparison target. | Avoids an over-specified operating point and preserves physical consistency. |
| D-007 | 2026-07-25 | Confirmed | Use convenient metric display units for the MVP and base SI internally. | Keeps the first UI useful without the conversion scope of switchable unit systems. |
| D-008 | 2026-07-25 | Confirmed | Let implementers select and document authoritative public equation and validation sources. | Favors traceable NASA material and established propulsion references without depending on user-supplied sources. |
| D-009 | 2026-07-25 | Confirmed | Classify the nozzle as ideally expanded when exit and ambient pressures differ by no more than 2% using symmetric relative difference. | Avoids fragile exact equality and defines deterministic boundary behavior. |
| D-010 | 2026-07-25 | Confirmed | Use xUnit for Core automated tests. | The .NET 10 template integrates with `dotnet test` and Visual Studio Test Explorer. |
| D-011 | 2026-07-25 | Confirmed | Apply nullable analysis, deterministic builds, build-time code-style checks, and warnings-as-errors across projects. | Makes local and automated builds fail consistently on quality regressions. |
| D-012 | 2026-07-25 | Confirmed | Use traceable NASA/NIST sources and fixed literal test oracles for the engineering model. | Prevents production code from validating itself and keeps every equation reviewable. |
| D-013 | 2026-07-25 | Confirmed | Use immutable Core models with SI units stated in numeric property names and defensively copied diagnostic collections. | Keeps unit meaning visible without adding a units package and prevents completed results from being mutated indirectly. |
| D-014 | 2026-07-25 | Confirmed | Return immutable, deterministically ordered validation results with stable codes and field IDs; avoid cascading cross-field issues when a source field is already invalid. | Supports reliable tests and future inline WPF validation without misleading duplicate errors. |
| D-015 | 2026-07-25 | Confirmed | Keep elementary geometry and propellant accounting in small stateless calculators that validate direct calls and use numerically stable operation ordering. | Makes equations independently testable and prevents overflow/cancellation where a representable result exists. |
| D-016 | 2026-07-25 | Confirmed | Solve the supersonic area–Mach relation by bisection on a default finite interval of `M in (1, 50]`, with 100 iterations, logarithmic equation evaluation, and a shared `1e-8` relative area-ratio tolerance. Return a status, error diagnostic, and no Mach value when a root is unbracketed or convergence is exhausted. | Guarantees deterministic branch selection and prevents a bound or placeholder from appearing as a valid nozzle solution. |
| D-017 | 2026-07-25 | Superseded | Define optional target-flow relative difference as `abs(mdot_target - mdot_calculated) / mdot_calculated`. Keep expansion-state classification outside the ambient performance equation component and supply it separately. | Superseded by D-018 once the shared classifier existed; the target-difference formula remains unchanged. |
| D-018 | 2026-07-25 | Confirmed | Ambient performance obtains expansion state from the shared 2% classifier. Model-diagnostic evaluation always returns the ideal-model warning; target differences above 5% warn; and `Pa > 0` with `Pe/Pa < 0.4` triggers a severe-overexpansion model-limit warning informed by NASA TM-X-64890. | Completes deterministic classification and warning generation while explicitly avoiding a claim that the ideal model predicts shocks, separation, or side loads; P1-07 attaches diagnostics to the composed result. |
| D-019 | 2026-07-25 | Confirmed | Expose the composed Core calculation through `IEnginePerformanceCalculator` and return `EngineCalculationResult`: either one complete `EnginePerformanceResult` or structured validation, solver, or numerical error issues with no partial result. | Gives the WPF layer one deterministic, UI-independent boundary and prevents extreme inputs or convergence failures from appearing as plausible performance. |
| D-020 | 2026-07-25 | Confirmed | Use `App.OnStartup` as the explicit composition root, constructor-inject `IEnginePerformanceCalculator` into `MainWindowViewModel`, and keep `MainWindow` code-behind limited to view initialization/DataContext assignment. Do not add an MVVM framework for the static shell. | Establishes testable dependency direction with no unnecessary package while leaving commands and mutable form state to later Phase 2 tasks. |
| D-021 | 2026-07-25 | Confirmed | Keep editable WPF input values as display-unit strings, parse with the current or invariant culture, convert MPa/kPa/mm to canonical SI in `TryCreateInputs`, and map stable Core validation fields back to inline form errors through `INotifyDataErrorInfo`. | Preserves user-friendly metric entry without leaking display units into Core and gives P2-03 one validated `EngineInputs` boundary. |
| D-022 | 2026-07-25 | Confirmed | Use a small synchronous `ICommand` for the bounded calculation, enable it only for valid inputs, project complete Core results into display-unit headline state, and clear that state whenever an input changes. Show only minimal calculation-failure feedback in P2-03; retain full diagnostics for P2-04. | Keeps the UI calculation path deterministic and testable, prevents stale values from appearing current, and preserves the Core/App boundary without adding an MVVM framework. |
| D-023 | 2026-07-26 | Confirmed | Project a successful `EnginePerformanceResult` into immutable detailed display state using cm², kPa, kN, and existing SI-friendly units; preserve diagnostic order, severity, stable code, field, and message verbatim from Core. | Makes every MVP result inspectable without coupling WPF to engineering calculations or weakening model-limit communication. |
| D-024 | 2026-07-26 | Confirmed | Keep thermodynamic presets in the App boundary as sourced, transparent starting points that copy a propellant label, mixture ratio, `Tc`, `gamma`, and `R` into the existing editable inputs. Preserve chamber pressure, geometry, environment, and optional values; changing any copied value switches the selection to Custom. | Adds a useful workflow without introducing combustion chemistry, hiding source assumptions, or coupling the Core calculation model to UI preset policy. |
| D-025 | 2026-07-26 | Confirmed | Represent calculation presentation with one `CalculationWorkflowState` (`Empty`, `Loading`, `Error`, or `Success`). Run the synchronous Core boundary on a worker task through a non-reentrant async command, show loading for at least 200 ms in the production composition, disable inputs during ordinary loading, and discard outcomes whose captured input revision is stale. | Keeps the WPF dispatcher responsive, makes every state visible and testable, prevents duplicate execution, and guarantees that results never describe a superseded operating point. |
| D-026 | 2026-07-26 | Confirmed | Render the MVP engine/nozzle stations as a responsive code-native WPF vector schematic. Project current editable inputs into injector, chamber, throat, and exit annotations, add solved exit state only after success, and always state that geometry is not to scale. | Keeps the visual dependency-free, accessible, and synchronized with workflow state while preventing the illustration from being mistaken for dimensional hardware geometry. |
| D-027 | 2026-07-26 | Confirmed | Represent nozzle flow profiles on a display-only normalized axis with chamber at `x=0`, throat at `x=0.35`, and exit at `x=1`. Smoothly interpolate Mach across 8 converging and 16 diverging segments, evaluate the sourced isentropic static-state equations at each point, require exact chamber/throat/exit agreement, and label the result as neither a solved contour nor CFD. | The MVP has no dimensional chamber or nozzle contour, so this produces deterministic educational trends without inventing physical axial geometry, shock/separation behavior, or higher-fidelity gradients. |
| D-028 | 2026-07-26 | Confirmed | Evaluate U.S. Standard Atmosphere 1976 pressure from `0` through `50 km` geopotential altitude, sample ideal thrust every `1 km`, and pair every checkpoint with ambient pressure. Show the selected operating point on the curve only when its pressure has an equivalent in range; retain vacuum as a separate reference. | Produces a deterministic, sourced altitude trade curve without presenting standard atmosphere as live weather or a trajectory, and without changing the solved ideal exit state or claiming off-design shock/separation fidelity. |
| D-029 | 2026-07-26 | Confirmed | Allow up to four in-memory operating-point snapshots, each copied from the canonical validated inputs and one complete successful result. Save the current result once, retain snapshots through later edits, recalculations, and failures, and keep save/remove/clear operations isolated from the active input view model. | Provides useful side-by-side comparison without turning historical values into editable state, silently restoring inputs, or implying persistence beyond the current application session. |
| D-030 | 2026-07-26 | Confirmed | Run WPF end-to-end tests in process on one serialized, dedicated STA dispatcher. Construct the real window with the production calculator and validator, drive named controls through bindings and the UI Automation invoke pattern, and assert rendered state as well as App projections. | Covers the control, binding, validation, async-command, Core, and presentation boundary deterministically without an external UI-driver dependency or calculator test double. |
| D-031 | 2026-07-26 | Confirmed | Capture the active Windows display culture when the main workflow is composed and pass it explicitly to every preformatted projection; set the WPF window language to the same culture for bindings. Require descriptive automation names, field-linked validation help, polite status announcements, keyboard shortcuts, WCAG AA text contrast, 3:1 essential chart/station graphics, and containment at 1040×680. | Keeps asynchronous and binding-formatted values consistent, makes errors and actions available to assistive technology, preserves keyboard operation, and prevents the release UI from depending on one locale or oversized window. |
| D-032 | 2026-07-26 | Confirmed | Publish v1.0.0 as an unsigned, self-contained, untrimmed, multi-file `win-x64` folder and ZIP with a SHA-256 sidecar, release guide, references, and durable screenshots. | A Windows 10/11 x64 user can extract and run without installing .NET; avoiding trimming and single-file bundling keeps WPF behavior predictable. The guide explicitly covers portability, SmartScreen, checksum verification, and engineering limits. |

Allowed status values: `Proposed`, `Confirmed`, `Superseded`, `Rejected`.

## 13. Open Questions and Blockers

All preliminary questions were resolved on 2026-07-25 in decisions `D-001`,
`D-002`, and `D-006` through `D-009`.

Current blockers: none.

## 14. Risks and Mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| Optional target mass flow disagrees with the geometry-driven solution | User may misread the target as an authoritative performance input | Label it as a comparison and show the absolute/relative residual |
| Incorrect root branch for area–Mach equation | Subsonic exit solution for a rocket nozzle | Bound and test the supersonic branch explicitly |
| Ideal model appears more authoritative than it is | User misinterprets estimates as hardware predictions | Always emit the ideal-model diagnostic and add a severe-overexpansion warning below the documented pressure-ratio proxy |
| Unit conversion errors | Orders-of-magnitude result errors | SI-only core plus tested boundary conversions |
| Illustrative LOX/methane values become “golden” data | False validation confidence | Use traceable analytical and published reference cases |
| UI and physics become tightly coupled | Tests and later modules become difficult | Keep the UI-independent `IEnginePerformanceCalculator` boundary in Core |
| Inputs change after a successful calculation | Headline values may be mistaken for the edited operating point | Clear all result and failure state on any input-view-model change and require recalculation |
| Model warnings are overlooked beside plausible numerical values | Users may treat ideal output as hardware prediction | Render successful-result diagnostics in Core order with visible severity, stable code, and full message |
| Generic preset values are mistaken for engine-specific combustion predictions | Trade-study output appears more authoritative than the preset basis supports | Show each preset's source and limitations, label secondary-source sets as reference estimates, keep every copied value editable, and switch edited sets to Custom |
| Calculation work blocks the WPF dispatcher or publishes an obsolete result | Window appears frozen or result no longer matches edited inputs | Execute the Core boundary through a non-reentrant async command, disable ordinary input edits while loading, and reject any outcome whose input revision changed |
| Nozzle schematic is mistaken for dimensional hardware geometry | Users infer chamber or contour dimensions that the model does not provide | Keep the vector shape deliberately schematic, label every station, and display a persistent not-to-scale notice |
| Normalized flow-profile interpolation is mistaken for a solved nozzle contour or CFD result | Users infer physical axial gradients, shock locations, or separation behavior | Anchor chamber, Mach-1 throat, and exit values to the Core solution; document the fixed smooth interpolation; label the axis normalized and keep the model-limit notice beside every chart |
| Standard-atmosphere thrust sweep is mistaken for weather, trajectory, or high-fidelity off-design performance | Users apply the curve outside its deterministic ideal-model scope | Label geopotential altitude and R-08 source, pair every altitude with pressure, keep selected/vacuum context distinct, and state that the exit solution is fixed and shocks/separation/losses are not predicted |
| A saved comparison is mistaken for editable input state or durable project data | Users expect comparison actions to restore inputs or persist after the application closes | Label snapshots read-only and session-only, copy immutable values, cap the set at four, and ensure save/remove/clear commands never write to the active input view model |
| WPF controls or bindings regress while view-model tests remain green | A release can compile and pass unit tests while the visible workflow is broken | Run serialized dedicated-STA end-to-end tests through real controls, validation bindings, command invocation, the production calculator, and rendered success state |
| Async presentation uses a different locale than WPF bindings | Decimal separators and accessible summaries disagree after calculation | Capture one display culture at composition, pass it to every preformatted projection, inherit it through `Window.Language`, and test a German-culture workflow |
| Minimum-height or low-contrast presentation hides context | Keyboard, low-vision, or small-window users miss validation, navigation, or model limits | Reserve the persistent model-boundary card, scroll workflow navigation independently, use AA text/essential-graphic colors, and automate 1040×680 containment and contrast checks |
| Unsigned portable release triggers Windows reputation warnings or is mistaken for an installed/supported product | Users may distrust or mis-handle the ZIP, and x86/Arm64 systems are not covered | Document Windows x64 support, absence of installer/updater/signature, SmartScreen behavior, archive checksum verification, extraction, and executable location in the bundled release guide |

## 15. Definition of Done

### Task done

A task may be marked `[x]` only when:

- Its acceptance criteria are satisfied.
- Relevant automated tests pass.
- The full solution still builds.
- New public behavior is documented.
- No unexplained warnings, placeholder values, or silent failure paths remain.
- Verification evidence is recorded in this document.

### MVP done

The MVP is complete when all non-deferred tasks `P0-01` through `P4-03` are
complete, documented reference cases pass within named tolerances, a clean
machine can run the published application, and the UI communicates the model's
assumptions and limitations.

Satisfied on 2026-07-26: all 24 tracked tasks are complete, 344 tests pass, and
the final self-contained v1.0.0 archive was checksum-verified, extracted to an
isolated directory, exercised through the production WPF controls, and closed
with exit code 0.

## 16. Session Handoff Notes

### Latest handoff — 2026-07-26

- Created the living context and completion tracker.
- Completed `P0-01`: confirmed Windows-only WPF on .NET 10 LTS.
- Confirmed metric MVP display, geometry-driven mass flow, implementer-selected
  authoritative sources, and a 2% ideal-expansion tolerance.
- Completed `P0-02`: scaffolded App, Core, and Core.Tests with inward project
  references.
- Verified the full solution build with zero warnings/errors and one passing
  discovered xUnit test.
- Completed `P0-03`: added SDK selection, shared build policy, formatting,
  ignore rules, and developer commands.
- Reverified the documented restore/build/test workflow after cleaning outputs.
- Completed `P0-04`: mapped 18 equations and 3 policies to 9 authoritative
  sources and defined 5 fixed validation cases.
- Independently recomputed all validation literals; discrepancies were at or
  below floating-point rounding.
- Completed `P1-01`: added immutable operating-point, nozzle, gas, result,
  expansion-state, and diagnostic models with explicit SI property names.
- Replaced template placeholders with 8 focused tests; the solution builds with
  zero warnings/errors and all tests pass.
- Completed `P1-02`: all scalar, optional, nested, and cross-field inputs now
  produce stable actionable validation issues without cascading failures.
- Build remains warning-free and all 16 tests pass.
- Completed `P1-03`: implemented sourced circular geometry, expansion ratio, and
  stable oxidizer/fuel flow splitting.
- VC-01 is automated along with boundary, non-finite, overflow, and underflow
  tests; build is warning-free and all 36 tests pass.
- Completed `P1-04`: implemented a bounded, deterministic supersonic area–Mach
  solver with shared residual tolerance and explicit bracket/iteration failures.
- VC-02 Mach 2 and VC-04 exit Mach are automated along with throat-tolerance,
  branch, input, bound, and iteration-limit cases.
- Restore is current, the solution builds with zero warnings/errors, and all
  60 tests pass. Core and test formatting verification also passes; unrelated
  WPF scaffold files retain pre-existing line-ending/encoding format findings.
- Completed `P1-05`: added numerically guarded calculators for choked flow,
  optional target/burn accounting, exit static state and velocity,
  characteristic velocity, and ambient thrust, `Isp`, and `Cf`.
- VC-02 state ratios, VC-03, and all numerical VC-04 values are automated;
  classification remains intentionally assigned to P1-06.
- Restore is current, the solution builds with zero warnings/errors, and all
  148 tests pass. The changed Core and test scope passes formatting verification.
- Completed `P1-06`: added the shared 2% symmetric classifier and integrated it
  into ambient performance without caller-supplied placeholder states.
- Added deterministic warnings for ideal-model assumptions, target-flow
  differences above 5%, and `Pe/Pa < 0.4` as a conservative limitation proxy
  informed by NASA TM-X-64890, not a separation prediction.
- VC-05, exact/near classification boundaries, warning boundaries, invalid
  calls, and deterministic diagnostic ordering are automated.
- Restore is current, the solution builds with zero warnings/errors, and all
  184 tests pass. The changed Core and test scope passes formatting verification.
- Completed `P1-07`: added `IEnginePerformanceCalculator`, its composed
  implementation, and an immutable success/failure outcome model.
- VC-02 through VC-05 now pass through the composed boundary; VC-04 validates
  the full fixed-oracle chain plus attached warnings within the documented
  `1e-6` tolerance.
- Invalid input, unbracketed-root, and representability failures return
  structured errors with no partial/default performance result.
- Restore is current, the solution builds with zero warnings/errors, and all
  195 tests pass. The changed Core and test scope passes formatting verification.
- Phase 1 is complete. Continue with `P2-01`, the WPF application shell and MVVM
  composition.
- Completed `P2-01`: replaced the blank WPF template with a themed, responsive
  workflow shell, persistent model-boundary messaging, placeholder input/result
  regions, and status presentation.
- `App.OnStartup` now composes `EnginePerformanceCalculator` into
  `MainWindowViewModel`; code-behind only initializes the view and DataContext.
- The compiled window launched with title `Liquid Rocket Workbench`, remained
  responsive during the smoke check, and the launched process was then closed.
- Restore is current, the solution builds with zero warnings/errors, and all
  195 tests pass. P2-01 C# formatting verification passes.
- Completed `P2-02`: added a reusable metric operating-point form, immediate
  parsing and Core-backed inline validation, and a validated conversion boundary
  that produces canonical-SI `EngineInputs`.
- Optional target flow and burn duration may be blank. The target remains a
  comparison value, and the calculation button stays disabled until P2-03.
- Added the App test project with 9 focused input cases covering SI conversion,
  optional values, field and cross-field errors, and error recovery.
- Restore is current, the solution builds with zero warnings/errors, and all
  204 tests pass. Changed C# formatting verification passes; the compiled WPF
  window launches responsively with the form loaded.
- Completed `P2-03`: added a validity-aware calculation command that sends
  canonical-SI inputs through `IEnginePerformanceCalculator` and projects only
  complete Core outcomes into headline display state.
- The summary distinguishes selected ambient, vacuum, and sea-level thrust and
  `Isp`, and shows ideal mass flow and nozzle expansion state. Input edits clear
  the prior result or failure state.
- Structured Core failures show a minimal actionable message without partial
  values; detailed diagnostic and warning presentation remains assigned to
  P2-04.
- Added 13 command, projection, state, failure, and notification tests. The full
  solution builds with zero warnings/errors, all 217 tests pass, and changed C#
  formatting verification passes.
- Windows UI Automation invoked the enabled Calculate button in the compiled
  app and found the expected selected thrust, selected `Isp`, and vacuum thrust
  text while the window remained responsive.
- Completed `P2-04`: added immutable detailed display projections for geometry,
  nozzle exit state, flow split, optional target and burn outputs,
  characteristic velocity, and three ambient performance cases.
- Successful-result diagnostics now render in Core order with severity, stable
  code, optional field label, and the full engineering message. The ideal-flow,
  severe-overexpansion, and target-mismatch warnings are visible for the default
  operating point.
- Added 8 detailed-projection and diagnostic tests. The full solution builds
  with zero warnings/errors, all 225 tests pass, and changed C# formatting
  verification passes.
- Live verification exposed and fixed a WPF runtime issue: bindings on
  `Run.Text` must explicitly use `Mode=OneWay` when their source is an immutable
  result property.
- Windows UI Automation invoked Calculate after the fix and found the expected
  throat area, exit Mach, ambient detail, warning count, and severe-
  overexpansion code while the window remained responsive.
- Completed `P2-05`: added Custom, VC-04 synthetic, NASA LOX/methane,
  LOX/kerosene reference, and LOX/hydrogen reference preset choices.
- Selecting a preset copies its label, mixture ratio, chamber temperature,
  `gamma`, and `R` into the existing validated editable fields. Editing any
  copied field switches the selector to Custom without discarding the edit.
- The input view now shows the active preset's source and limitations. The
  reference document records every preset value and derivation; secondary-source
  sets are explicitly labeled as comparison estimates rather than chemistry
  predictions.
- Added 7 preset catalog, application, edit, and stale-result tests. Restore is
  current, the solution builds with zero warnings/errors, all 232 tests pass,
  and changed C# formatting verification passes.
- Windows UI Automation selected the NASA LOX/methane preset, verified all
  applied fields, confirmed edit-to-Custom behavior, calculated successfully,
  found the result label, and confirmed the window remained responsive.
- Completed `P2-06`: replaced overlapping result flags with one explicit
  `Empty`, `Loading`, `Error`, or `Success` workflow state and accessible state
  labels in the result region.
- Added a non-reentrant async command. Core calculation runs off the WPF
  dispatcher, the production loading state remains visible for at least 200 ms,
  the calculate button and inputs are disabled during ordinary loading, and a
  captured input revision prevents stale results from being published.
- Structured Core failures and unexpected calculator exceptions enter the error
  state with retry text; a later successful calculation replaces the error.
- Added 10 net new command/state tests, bringing App coverage to 47 tests and
  the full solution to 242 tests. Restore is current, build has zero
  warnings/errors, and changed C# formatting verification passes.
- Windows UI Automation found initial empty, indeterminate loading, successful
  result, edit-reset empty, and structured solver-error states in one compiled
  app session. The calculate button was disabled during loading and the window
  remained responsive.
- Completed `P3-01`: added a responsive WPF vector flow-path schematic with
  accessible injector, chamber, throat, and exit station labels.
- The diagram projects current input annotations in empty/loading/error states
  and adds solved exit Mach, pressure, temperature, and velocity only after a
  successful calculation. A persistent notice identifies it as not to scale.
- Added 7 diagram projection and workflow-integration cases. Restore is current,
  the solution builds with zero warnings/errors, and all 249 tests pass.
- Windows UI Automation verified the four station names, pending and solved
  annotations, minimum-width containment at 1040×680, and a responsive
  successful calculation.
- Completed `P3-02`: the Core now returns a 25-point normalized profile with
  chamber, Mach-1 throat, and solved exit anchors plus isentropic pressure and
  temperature at every sample.
- Added responsive WPF pressure, temperature, and Mach small multiples with
  visible station values, normalized-axis context, accessible summaries, and an
  explicit notice that interpolation is not a solved contour or CFD result.
- Added 28 Core/App profile, boundary, presentation, converter, and workflow
  cases. Restore is current, build has zero warnings/errors, and all 277 tests
  pass.
- UI Automation confirmed profiles are hidden before success, then found all
  three station summaries and the model-limit notice at 1040×680. All three
  traces and their station cards were visually inspected in the compiled app.
- Completed `P3-03`: added sourced lower-atmosphere pressure evaluation through
  50 km geopotential altitude and a 51-point ideal-thrust sweep that holds the
  active nozzle solution fixed while varying ambient pressure.
- The WPF curve pairs six visible altitude/pressure/thrust checkpoints, shows
  selected-condition context when it maps into the standard range, preserves a
  distinct vacuum reference, and states the weather/trajectory/off-design
  limitations.
- Added VC-06 plus 45 atmosphere, inverse mapping, sweep, boundary, model,
  presentation, accessibility, and workflow cases. Restore is current, build
  has zero warnings/errors, and all 322 tests pass.
- UI Automation verified success-only visibility, all paired checkpoints,
  selected marker, R-08 source/model text, and containment at 1040×680. The
  compiled curve and vacuum reference were visually inspected.
- Completed `P3-04`: added up to four immutable, read-only session snapshots
  containing source inputs and selected, vacuum, and sea-level results.
- Saving, removing, and clearing operate only on the comparison collection;
  saved points survive input edits, later successful calculations, and
  calculation failures without changing the active form.
- Added 15 snapshot, command, isolation, persistence, duplicate, and bound
  cases. Restore is current, build has zero warnings/errors, and all 337 tests
  pass.
- UI Automation saved distinct 101.325 and 90 kPa operating points, verified
  their accessible summaries and the two-card layout at 1040×680, then removed
  Point 1 while the active ambient input remained 90 kPa. Phase 3 is complete;
  continue with `P4-01`.

### 2026-07-26 — P4-01

- Completed: added serialized, in-process WPF end-to-end coverage on a dedicated
  STA dispatcher. The happy path edits named controls, invokes the real
  Calculate button through UI Automation, runs `EnginePerformanceCalculator`,
  and verifies headline, detail, diagnostic, profile, and altitude state.
- Completed: malformed chamber-pressure and chamber/ambient cross-field cases
  verify visible binding errors, disabled calculation, correction, and a
  successful retry through the same window.
- Verified: restore current; build has zero warnings/errors; all 340 tests pass,
  including the 3 new end-to-end cases; changed C# formatting passes.
- Remaining: continue with `P4-02`, the accessibility, layout, units, and
  numerical-format review.

### 2026-07-26 — P4-02

- Completed: descriptive automation names now cover every interactive control;
  validation messages are field help text; input/application status uses polite
  live announcements; and Calculate, Save, and Clear expose Alt+C, Alt+S, and
  Alt+L shortcuts.
- Completed: the window and preformatted async projections share one captured
  Windows display culture. German-culture automation verifies comma decimals
  while the complete workflow retains MPa, kPa, mm, cm², K, J/(kg·K), m/s,
  kg/s, kN, seconds, percent, and kilometre context.
- Completed: accent/muted text now meets 4.5:1 WCAG AA contrast on its common
  surfaces, essential station/chart strokes meet 3:1, the top introduction
  flexes at minimum width, and workflow navigation scrolls independently while
  the model-boundary card remains fully visible above the footer.
- Verified: restore current; warning-as-error build has zero warnings/errors;
  all 344 tests pass, including 4 new release-quality cases; changed C#
  formatting passes. A compiled 1040×680 success workflow was visually inspected
  at `artifacts/p4-02/minimum-window-success.png`; UI Automation found 11/11
  named edit controls.
- Remaining: continue with `P4-03`, publishing the documented runnable build,
  durable screenshots, and known limits.

### 2026-07-26 — P4-03

- Completed: added versioned assembly metadata, a self-contained Windows x64
  publish profile, reproducible folder/ZIP/checksum packaging, a bundled release
  guide and reference document, and four durable screenshots generated from the
  published executable.
- Completed: the release guide documents Windows requirements, extraction and
  launch steps, checksum verification, screenshot regeneration, the unsigned
  portable distribution model, and the complete ideal-flow/model-scope limits.
- Verified: Release build has zero warnings/errors and all 344 tests pass. The
  final 63,315,359-byte ZIP contains 407 files and has SHA-256
  `8980a7e9b5fbafff14c5cef81d7e26144c82a42de7a0792a4104a34d6e79add1`.
  A fresh temporary extraction contained the guide and four screenshots; its
  executable calculated successfully through UI Automation and exited 0.
- Remaining: no tracked MVP work. Define and prioritize post-MVP tasks before
  expanding scope.

### Handoff template

Append a new dated subsection only when information would help the next person:

```text
### YYYY-MM-DD — <task ID or short topic>

- Completed:
- Verified:
- Remaining:
- Watch out for:
```

Keep only the most recent useful handoffs. Durable knowledge belongs in the
relevant main section, not only in handoff notes.
