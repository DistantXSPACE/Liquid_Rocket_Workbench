# Liquid Rocket Workbench User Guide

This guide explains how to run Liquid Rocket Workbench 1.0.0, define an
operating point, calculate ideal liquid-rocket performance, interpret the
results, and compare design points.

Liquid Rocket Workbench is an educational and preliminary trade-study tool. It
uses a one-dimensional, steady, adiabatic, isentropic ideal-gas model with
constant gas properties and choked throat flow. Its results are estimates, not
test data, certified hardware predictions, or a substitute for a higher-fidelity
analysis.

## Contents

1. [Start the application](#start-the-application)
2. [Quick start](#quick-start)
3. [Understand the workspace](#understand-the-workspace)
4. [Enter an operating point](#enter-an-operating-point)
5. [Run a calculation](#run-a-calculation)
6. [Interpret the results](#interpret-the-results)
7. [Explore the nozzle profiles](#explore-the-nozzle-profiles)
8. [Explore thrust versus altitude](#explore-thrust-versus-altitude)
9. [Compare operating points](#compare-operating-points)
10. [Troubleshooting](#troubleshooting)
11. [Keyboard and accessibility](#keyboard-and-accessibility)
12. [Engineering limitations](#engineering-limitations)

## Start the application

### From the published Windows release

The self-contained release supports Windows 10 or Windows 11 on x64 systems. A
separate .NET installation is not required.

1. Extract `LiquidRocketWorkbench-1.0.0-win-x64.zip`.
2. Keep the extracted folder and all of its contents together.
3. Open the `LiquidRocketWorkbench-1.0.0-win-x64` folder.
4. Run `LiquidRocketWorkbench.exe`.

The release is portable and does not include an installer, automatic updater, or
code signature. Windows may display a SmartScreen warning. If the archive came
from another person, verify its accompanying SHA-256 checksum before running it.
See [release.md](release.md) for the verification command and supported-release
details.

### From the source repository

With the .NET 10 SDK installed, run this command from the repository root:

```powershell
dotnet run --project src/LiquidRocketWorkbench.App/LiquidRocketWorkbench.App.csproj
```

## Quick start

The application opens with a valid synthetic reference case. Use it for a first
calculation before changing any values:

1. Review the **Operating point** card.
2. Confirm the green **All required inputs are valid** status.
3. Select **Calculate performance**, or press `Alt+C`.
4. Wait briefly while the calculation completes.
5. Review the selected-ambient, vacuum, and sea-level headline results.
6. Scroll down to inspect the detailed values, warnings, nozzle profiles, and
   thrust-versus-altitude chart.

The initial reference case uses an 8 MPa chamber pressure, 50 mm throat, 101.325
kPa ambient pressure, and the documented VC-04 synthetic constant-property gas.
Rounded values should be approximately:

- Selected ambient: 21.31 kN thrust and 236.4 s specific impulse
- Vacuum: 29.27 kN thrust and 324.7 s specific impulse
- Ideal geometry-driven flow: 9.193 kg/s
- Exit pressure: 15.4 kPa

These values verify that the workflow is operating. They are a synthetic
equation-chain reference, not a prediction for a real propellant combination.
Displayed rounding and decimal separators follow the active Windows culture.

![Operating-point input workflow](screenshots/01-input-workflow.png)

## Understand the workspace

The left side of the window summarizes the four-part workflow:

1. **Define inputs** — enter chamber, gas, nozzle, and ambient conditions.
2. **Calculate** — run the deterministic ideal-flow model.
3. **Review** — inspect performance values and model diagnostics.
4. **Explore** — examine profiles, altitude behavior, and saved comparisons.

The calculation workspace scrolls independently. The model-boundary reminder
remains visible so the idealized scope is not separated from the results.

Editing an input after a successful calculation clears the displayed result.
This is intentional: the previous result no longer describes the active form.
Calculate again to publish a result for the revised operating point. Saved
comparison cards are not cleared by ordinary input edits.

## Enter an operating point

All displayed inputs use convenient metric units. The calculation core converts
them to base SI units.

### Chamber and gas inputs

| Input | Unit | How it is used |
|---|---:|---|
| Thermodynamic preset | — | Copies a sourced propellant label, mixture ratio, chamber temperature, specific-heat ratio, and specific gas constant into the editable fields. |
| Propellant label | — | Identifies the operating point in results and saved comparisons; it does not select chemistry by itself. |
| Chamber pressure, `Pc` | MPa | Sets the chamber stagnation pressure and contributes to geometry-driven choked flow. It must be greater than ambient pressure. |
| Chamber temperature, `Tc` | K | Sets the chamber stagnation temperature for the constant-property gas model. |
| Specific-heat ratio, `gamma` | — | Controls the isentropic area–Mach and static-state relationships. It must be greater than 1. |
| Specific gas constant, `R` | J/(kg·K) | Connects temperature to acoustic speed, velocity, and ideal choked flow. |
| Mixture ratio, `O/F` | — | Splits the calculated total flow into oxidizer and fuel flow. It must be positive. |

Presets are transparent starting points, not combustion calculations. Selecting
a preset does not alter chamber pressure, nozzle geometry, ambient pressure,
target flow, or burn duration. All copied values remain editable. Changing a
copied value switches the selector to **Custom** without discarding the edit.
The **Preset basis** panel states the source and assumptions for the active
preset.

Use the synthetic VC-04 preset to exercise the documented reference case. Use a
propellant preset as an initial constant-property estimate, then replace its
values when you have gas properties appropriate to your own analysis.

### Nozzle and environment inputs

| Input | Unit | How it is used |
|---|---:|---|
| Throat diameter, `dt` | mm | Defines throat area and therefore the authoritative ideal choked mass flow. It must be positive. |
| Exit diameter, `de` | mm | Defines exit area and expansion ratio. It must be at least as large as the throat diameter. |
| Ambient pressure, `Pa` | kPa | Sets the selected surrounding pressure used in pressure thrust and nozzle-state classification. It must be nonnegative and lower than chamber pressure. |

Use 101.325 kPa for standard sea-level pressure or 0 kPa for the ideal vacuum
case. The separate sea-level and vacuum results are always calculated, so the
selected ambient is best used for the actual condition you want to study.

### Optional comparison inputs

| Input | Unit | How it is used |
|---|---:|---|
| Target mass flow | kg/s | Compares a desired or externally estimated flow with the geometry-driven ideal flow. It never overrides the calculated flow. |
| Burn duration | s | Multiplies the calculated flow by time to estimate total ideal propellant consumption. |

Both optional fields may be left blank. A target-flow difference above the
documented 5% tolerance produces a warning. That warning means the target is
inconsistent with the ideal flow implied by chamber pressure, throat geometry,
temperature, and gas properties; it does not force the solution to the target.

### Correct validation errors

Inputs are validated as you type. Invalid fields show an inline, actionable
message and the calculation button remains disabled. Common requirements are:

- all required numeric values must be finite numbers;
- pressure, temperature, diameters, gas constant, and mixture ratio must be
  physically positive where required;
- `gamma` must be greater than 1;
- chamber pressure must be greater than ambient pressure;
- exit diameter must be at least the throat diameter; and
- optional target flow and burn duration must be positive when supplied.

The form accepts decimal formatting from the active Windows culture and also
accepts invariant decimal input. Follow the example shown by other values in the
form if a decimal value is rejected.

## Run a calculation

Select **Calculate performance** or press `Alt+C`. During the short loading
state, inputs and duplicate calculation requests are disabled. The window remains
responsive.

A successful result is published as one complete set. A solver, validation, or
numerical failure displays a structured error instead of partial or
plausible-looking fallback values. Correct the input identified by the message
and calculate again.

## Interpret the results

### Performance summary

The first result card provides the main comparison:

- **Selected-ambient thrust** and **selected-ambient Isp** use the ambient
  pressure entered in the form.
- **Vacuum** uses zero ambient pressure.
- **Sea level** uses standard 101.325 kPa ambient pressure.
- **Ideal flow** is calculated from chamber conditions, gas properties, and
  throat geometry.
- **Nozzle state** compares exit pressure with the selected ambient pressure.

![Successful performance summary](screenshots/02-performance-summary.png)

The nozzle-state labels mean:

- **Underexpanded** — exit pressure is higher than ambient pressure.
- **Ideally expanded** — exit and ambient pressures are within the documented
  2% symmetric relative tolerance.
- **Overexpanded** — exit pressure is lower than ambient pressure.

The classification describes the ideal pressure relationship. It does not prove
that flow remains attached and does not locate shocks or separation.

### Detailed result

The detailed cards expose the values behind the summary:

- **Nozzle geometry** — throat area, exit area, and expansion ratio `Ae/At`.
- **Nozzle exit state** — exit Mach number, pressure, temperature, and ideal
  velocity.
- **Geometry-driven flow** — total, oxidizer, and fuel mass flow, plus ideal
  characteristic velocity `c*`.
- **Optional target-flow comparison** — target, absolute difference, and
  relative difference when a target was supplied.
- **Burn consumption** — total propellant consumed over the entered duration.
- **Performance by ambient condition** — total, momentum, and pressure thrust;
  specific impulse; thrust coefficient; and nozzle state for selected ambient,
  vacuum, and sea level.

Pressure thrust can be negative for an overexpanded ideal nozzle because exit
pressure is below ambient pressure. Total thrust is the sum of momentum thrust
and pressure thrust.

### Model warnings and diagnostics

Always review the diagnostic cards, even when the numerical result looks
reasonable. Every successful calculation includes an ideal-model notice.
Additional warnings may identify:

- a target-flow difference above 5%; or
- severe overexpansion where the ideal isentropic model is outside the range in
  which attached-flow behavior should be assumed.

Warnings do not modify the solution. They identify assumptions that require
engineering judgment or a more appropriate model.

## Explore the nozzle profiles

The profile card plots static pressure, static temperature, and Mach number from
the chamber through the throat to the exit. The fixed normalized positions are:

- chamber: `x = 0.00`;
- throat: `x = 0.35`; and
- exit: `x = 1.00`.

The chamber, Mach-1 throat, and exit anchors agree with the active ideal
solution. The smooth curves between those stations are display interpolation.
They are not a dimensional nozzle contour, CFD result, boundary-layer solution,
or prediction of shock or separation location.

![Normalized nozzle pressure, temperature, and Mach profiles](screenshots/03-nozzle-profiles.png)

Use the profiles to understand direction and relative change between stations.
Do not read a physical length, wall contour, or local hardware gradient from the
horizontal axis.

## Explore thrust versus altitude

The altitude card holds the solved ideal nozzle exit state fixed while changing
ambient pressure according to the U.S. Standard Atmosphere 1976. It displays
ideal thrust from sea level through 50 km geopotential altitude and provides
paired altitude, pressure, and thrust checkpoints.

![Ideal thrust versus standard-atmosphere altitude](screenshots/04-thrust-altitude.png)

When the selected ambient pressure corresponds to a pressure in this standard
range, the plot shows its standard-altitude equivalent. The vacuum value remains
a separate reference because vacuum is not an altitude within the plotted
atmosphere.

Use this chart for a first-order view of ambient-pressure sensitivity. It is not
live weather, a flight trajectory, or a prediction of off-design shocks,
separation, side loads, or losses.

## Compare operating points

You can save up to four successful results for a side-by-side session
comparison:

1. Calculate the first valid operating point.
2. Select **Save current point**, or press `Alt+S`.
3. Edit one or more active inputs.
4. Calculate the revised point.
5. Save it and compare the cards.

Each read-only card preserves the source chamber, gas, nozzle, and ambient inputs
plus selected, vacuum, and sea-level performance. Saving, removing, or clearing
cards never writes values back into the active form.

Use the remove button on a card to delete one point. Select **Clear all** or
press `Alt+L` to remove every saved point. Saved comparisons exist only for the
current application session and are lost when the application closes.

## Troubleshooting

### The application does not start

- Confirm that the operating system is Windows 10 or Windows 11 x64.
- Extract the ZIP before running the application.
- Keep `LiquidRocketWorkbench.exe` beside all files from the published folder;
  do not copy only the executable elsewhere.
- If Windows displays SmartScreen, verify the archive checksum and provenance
  before choosing whether to run the unsigned application.

### Calculate performance is disabled

At least one input is invalid. Scroll through the **Operating point** card,
locate the highlighted field and inline message, and correct it. The status
changes to **All required inputs are valid** when the form can be calculated.

### A result disappeared after editing

This is expected. Any input edit clears the current result so a stale solution
cannot be mistaken for the revised operating point. Select **Calculate
performance** again. Previously saved comparison cards remain available.

### The target mass flow does not match the result

The target is only a consistency check. Change chamber pressure, throat diameter,
chamber temperature, or gas properties if you intend to change the authoritative
geometry-driven ideal flow. Do not expect the target field to force the
calculation.

### An overexpansion warning appears

The chosen nozzle exit pressure is below the selected ambient pressure. Treat
the output as an ideal estimate. This application cannot determine shock
location, separation, side loads, or whether attached flow is maintained.

### The selected point has no altitude marker

The entered ambient pressure does not map to a standard-atmosphere altitude in
the plotted 0–50 km range. The selected-ambient calculation is still valid
within the application's ideal model; only the plot marker is omitted.

### No more comparison points can be saved

The session limit is four. Remove an existing card or clear all saved points,
then save the current successful result.

## Keyboard and accessibility

The application supports keyboard navigation and exposes descriptive automation
names, field-linked validation help, and polite status announcements for
assistive technologies.

| Shortcut | Action |
|---|---|
| `Alt+C` | Calculate performance |
| `Alt+S` | Save the current successful result |
| `Alt+L` | Clear all saved comparison points |
| `Tab` / `Shift+Tab` | Move forward or backward through interactive controls |

Engineering values use the active Windows decimal and grouping conventions while
keeping metric unit labels visible.

## Engineering limitations

Do not use Liquid Rocket Workbench as a hardware design authority or for
safety-critical analysis.

The 1.0.0 model does not include:

- combustion chemistry, chemical equilibrium, finite-rate chemistry, or NASA
  CEA;
- combustion, divergence, boundary-layer, erosion, heat-transfer, real-gas, or
  multiphase losses;
- shock location, flow separation, side loads, or proof of attached flow;
- dimensional nozzle-contour design or CFD;
- transient start and shutdown behavior;
- feed systems, injectors, regenerative cooling, structures, or materials;
- manufacturing tolerances or test-data correlation; or
- live weather or trajectory simulation.

Presets supply editable constant-property inputs. They do not predict the gas
state for a specific engine. Pressure, temperature, and Mach profiles use a
normalized display axis, and the altitude curve holds the ideal exit solution
fixed while varying standard atmospheric pressure.

For the equation sources, validation cases, preset provenance, numerical
policies, and full model traceability, see [references.md](references.md).
