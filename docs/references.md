# Engineering Equation and Validation References

This document is the traceability source for the ideal rocket/nozzle equations
implemented by Liquid Rocket Workbench. Production code and automated tests
should cite the equation IDs defined here rather than copying an unexplained
formula.

Last reviewed: 2026-07-26

## 1. Model Boundary

The MVP model is one-dimensional, steady, adiabatic, isentropic, and calorically
perfect. Chamber pressure and chamber temperature are treated as nozzle-inlet
total (stagnation) conditions. Specific heat ratio and specific gas constant are
constant. The throat is choked at Mach 1, and the diverging nozzle uses the
supersonic branch of the area–Mach relation.

The results are ideal estimates. They do not model combustion equilibrium,
boundary layers, divergence loss, shocks, separated flow, erosion, heat
transfer, or transient operation.

## 2. Authoritative Sources

### R-01 — NASA Glenn rocket thrust equation set

NASA Glenn Research Center, [Thrust Equations
Summary](https://www1.grc.nasa.gov/beginners-guide-to-aeronautics/thrust-equations-summary/).

Primary source for choked mass flow, exit area–Mach ratio, exit static state,
exit velocity, and the rocket thrust equation. The page explicitly treats the
throat as choked and chamber values as total pressure and temperature.

### R-02 — NASA Glenn isentropic-flow relations

NASA Glenn Research Center, [Isentropic Flow
Equations](https://www.grc.nasa.gov/www/k-12/airplane/isentrop.html).

Primary source for static-to-total pressure, temperature, and density ratios;
speed of sound; Mach number; and the area–Mach relation. It also documents that
area ratio has subsonic and supersonic solutions.

### R-03 — NASA Glenn choked-flow derivation

NASA Glenn Research Center, [Mass Flow
Choking](https://www.grc.nasa.gov/WWW/k-12/airplane/mflchk.html).

Primary derivation for compressible mass flow and its maximum at Mach 1. Use
this source for the geometry-driven mass-flow implementation.

### R-04 — NASA Glenn specific impulse

NASA Glenn Research Center, [Specific
Impulse](https://www1.grc.nasa.gov/beginners-guide-to-aeronautics/specific-impulse/).

Primary source for rocket thrust, equivalent exhaust velocity, total impulse,
and `Isp = F / (mdot * g0)`.

### R-05 — NACA Report 1135

Ames Research Staff, [Equations, Tables, and Charts for Compressible
Flow](https://ntrs.nasa.gov/citations/19930091059), NACA Report 1135, 1953.

Primary archival reference for one-dimensional perfect-gas compressible-flow
equations and tabulated dimensionless values. Use the continuous-flow tables as
an independent comparison for isentropic test cases.

### R-06 — NASA SP-125

Dieter K. Huzel and David H. Huang, [Design of Liquid Propellant Rocket
Engines, Second Edition](https://ntrs.nasa.gov/citations/19710019929), NASA
SP-125, 1971, Chapter 1, especially equations 1-31 through 1-33.

Primary NASA reference for characteristic velocity, thrust coefficient, and
their role in separating chamber and nozzle performance.

### R-07 — NIST SI guidance

NIST, [Special Publication 811: Guide for the Use of the International System
of Units](https://www.nist.gov/pml/special-publication-811).

Reference for SI quantity and unit conventions. Use standard acceleration of
gravity `g0 = 9.80665 m/s²`.

### R-08 — U.S. Standard Atmosphere 1976

NOAA, NASA, and U.S. Air Force, [U.S. Standard Atmosphere,
1976](https://ntrs.nasa.gov/citations/19770009539), NASA-TM-X-74335.

Primary source for the standard pressure-versus-altitude model used by the
future thrust-versus-altitude plot. Standard sea-level pressure is `101325 Pa`.
Do not silently substitute live weather pressure for this standard atmosphere.

### R-09 — NASA oxidizer/fuel mixture-ratio definition

William M. Marshall and Julie E. Kleinhenz, NASA Glenn Research Center,
[Hot-Fire Testing of 100 LBf LOX/LCH4 Reaction Control Engine at Altitude
Conditions](https://ntrs.nasa.gov/citations/20100040703), E-17514, 2010,
glossary.

Defines mixture ratio as oxidizer mass flow divided by fuel mass flow. The flow
split equations below follow algebraically from that definition and conservation
of total propellant mass flow.

### R-10 — NASA liquid-nozzle flow-separation survey

R. H. Schmucker, [Status of Flow Separation Prediction in Liquid Propellant
Rocket Nozzles](https://ntrs.nasa.gov/citations/19750003989), NASA
TM-X-64890, 1974.

Primary model-limit reference for overexpanded liquid-rocket nozzles. It
summarizes experimental data and multiple empirical/theoretical separation
methods rather than defining one universal boundary. The MVP therefore uses a
conservative exit-to-ambient pressure-ratio warning proxy, not a separation or
shock prediction.

### R-11 — NASA LOX/methane test-condition gas properties

Richard J. Priem and Kevin J. Breisacher, [Nonlinear Combustion Instability
Model in Two- to Three-Dimensions](https://ntrs.nasa.gov/citations/19910021894),
NASA TM-102381, 1991, table II.

Primary source for the LOX/methane preset's MSFC operating condition:
`O/F = 3.48`, chamber temperature `6560 °R`, chamber-gas speed of sound
`51819 in/s`, and `gamma = 1.182`. The application converts the temperature
and speed to SI and derives `R` using `a² = gamma R T`. These reported
test-condition values are only a constant-property starting point; selecting the
preset does not reproduce the report's combustion model or measured engine.

### R-12 — Public LOX/kerosene and LOX/hydrogen summaries

Encyclopedia Astronautica, [LOX/Kerosene](https://www.astronautix.com/l/loxkerosene.html)
and [LOX/LH2](https://www.astronautix.com/l/loxlh2.html).

Secondary public references for the comparison presets. The LOX/kerosene
summary lists `O/F = 2.56`, `Tc = 3670 K`, `gamma = 1.24`, and molecular
weight `23.30 kg/kmol`. The LOX/LH2 summary lists `O/F = 6`,
`Tc = 2985 K`, `gamma = 1.26`, and molecular weight `10 kg/kmol`.
Because these are generic propellant summaries rather than engine-specific
NASA CEA cases, the UI labels them as reference estimates and keeps every value
editable.

### R-13 — NIST 2022 CODATA molar gas constant

NIST, [2022 CODATA recommended values of the fundamental physical
constants](https://physics.nist.gov/cuu/pdf/JPCRD2022CODATA.pdf), table XXXIII.

Source for the exact molar gas constant `8.314462618... J/(mol·K)`, represented
as `8314.46261815324 J/(kmol·K)` in the preset catalog. The LOX/kerosene and
LOX/hydrogen preset gas constants use `R = Ru / molecularWeight`.

### Built-in thermodynamic preset register

Presets are UI-boundary conveniences for the same calorically perfect,
constant-property Core model. Selecting one copies its propellant label,
mixture ratio, chamber temperature, `gamma`, and `R` into ordinary editable
fields. Editing any copied field changes the selector to **Custom** without
discarding the edit.

| Preset | `O/F` | `Tc` (K) | `gamma` | `R` (J/(kg·K)) | Basis |
|---|---:|---:|---:|---:|---|
| Synthetic reference gas · VC-04 | `3.5` | `3500` | `1.22` | `355` | Fixed synthetic validation case VC-04 |
| LOX / Methane · NASA test condition | `3.48` | `3644.44444444444` | `1.182` | `402.157968052826` | R-11; `R` derived from the reported sound speed |
| LOX / Kerosene · public reference | `2.56` | `3670` | `1.24` | `356.843889191126` | R-12 and R-13; generic comparison estimate |
| LOX / Hydrogen · public reference | `6` | `2985` | `1.26` | `831.446261815324` | R-12 and R-13; generic comparison estimate |

The preset catalog is not a chemistry solver and does not claim that one
constant property set is valid across an engine or nozzle. Chamber pressure,
nozzle geometry, ambient pressure, optional target flow, and burn duration are
never changed by preset selection.

## 3. Symbols and Conventions

| Symbol | Meaning | SI unit |
|---|---|---|
| `At`, `A*` | Nozzle throat/sonic area | m² |
| `Ae` | Nozzle exit area | m² |
| `dt`, `de` | Throat and exit diameters | m |
| `epsilon` | Expansion ratio, `Ae / At` | 1 |
| `Pc`, `pt` | Chamber/nozzle-inlet total pressure | Pa |
| `Tc`, `Tt` | Chamber/nozzle-inlet total temperature | K |
| `Pa`, `p0` | Ambient pressure | Pa |
| `Pe`, `Te` | Exit static pressure and temperature | Pa, K |
| `gamma` | Ratio of specific heats | 1 |
| `R` | Specific gas constant | J/(kg·K) |
| `M`, `Me` | Local and exit Mach number | 1 |
| `x` | Normalized chamber-to-exit display position | 1 |
| `h`, `hb` | Geopotential altitude and atmosphere-layer base altitude | m |
| `L` | Atmosphere-layer temperature lapse rate | K/m |
| `Pb`, `Tb` | Atmosphere-layer base pressure and temperature | Pa, K |
| `M0` | Standard sea-level molar mass, `0.0289644` | kg/mol |
| `Rstar` | Standard-atmosphere molar gas constant, `8.31432` | J/(mol·K) |
| `mdot` | Calculated total propellant mass flow | kg/s |
| `OF` | Oxidizer-to-fuel mass-flow ratio | 1 |
| `Ve` | Ideal exit velocity | m/s |
| `F` | Thrust at the selected ambient pressure | N |
| `Cf` | Thrust coefficient | 1 |
| `c*` | Characteristic velocity | m/s |
| `Isp` | Specific impulse | s |
| `g0` | Standard acceleration of gravity, `9.80665` | m/s² |

Use `Pc` and `Tc` in the application domain and map them to the total-condition
notation `pt` and `Tt` used in the sources. Do not treat exit values as total
conditions.

## 4. Equation Register

### Geometry and propellant accounting

| ID | Relationship | Source |
|---|---|---|
| EQ-GEO-01 | `A = pi * d² / 4` | Euclidean circle-area identity |
| EQ-GEO-02 | `epsilon = Ae / At` | Definition used by R-01 and R-06 |
| EQ-MIX-01 | `OF = mdot_ox / mdot_fuel` | R-09 |
| EQ-MIX-02 | `mdot_ox = mdot * OF / (1 + OF)` | Algebra from EQ-MIX-01 and `mdot = mdot_ox + mdot_fuel` |
| EQ-MIX-03 | `mdot_fuel = mdot / (1 + OF)` | Algebra from EQ-MIX-01 and total-flow conservation |
| EQ-MASS-01 | `mass = mdot * duration` for constant flow | Definition of mass flow; consistent with R-04 |

### Choked mass flow and nozzle state

| ID | Relationship | Source |
|---|---|---|
| EQ-FLOW-01 | `mdot = At * Pc / sqrt(Tc) * sqrt(gamma / R) * ((gamma + 1) / 2)^(-(gamma + 1) / (2 * (gamma - 1)))` | R-01 and R-03 at `M = 1` |
| EQ-FLOW-02 | `A/A* = (1/M) * ((2/(gamma+1)) * (1 + (gamma-1)M²/2))^((gamma+1)/(2(gamma-1)))` | R-01, R-02, and R-05 |
| EQ-STATE-01 | `T/Tc = (1 + (gamma - 1)M²/2)^-1` | R-01, R-02, and R-05 |
| EQ-STATE-02 | `P/Pc = (1 + (gamma - 1)M²/2)^(-gamma/(gamma-1))` | R-01, R-02, and R-05 |
| EQ-VELOCITY-01 | `V = M * sqrt(gamma * R * T)` | R-01 and R-02 |

For the exit, set `A/A* = epsilon` and solve EQ-FLOW-02 on `M > 1`.
For a plotted converging section use the subsonic branch `0 < M < 1`; for a
plotted diverging section use the supersonic branch `M > 1`. Both meet at
`M = 1` at the throat.

### Performance

| ID | Relationship | Source |
|---|---|---|
| EQ-THRUST-01 | `F = mdot * Ve + (Pe - Pa) * Ae` | R-01 and R-04 |
| EQ-THRUST-02 | `F_momentum = mdot * Ve` | Momentum term of EQ-THRUST-01 |
| EQ-THRUST-03 | `F_pressure = (Pe - Pa) * Ae` | Pressure term of EQ-THRUST-01 |
| EQ-ISP-01 | `Isp = F / (mdot * g0)` | R-04, with `g0` from R-07 |
| EQ-CSTAR-01 | `c* = Pc * At / mdot` | R-06 |
| EQ-CSTAR-02 | `c* = sqrt(R * Tc / gamma) * ((gamma + 1) / 2)^((gamma + 1) / (2 * (gamma - 1)))` | EQ-CSTAR-01 combined with EQ-FLOW-01; consistent with R-06 |
| EQ-CF-01 | `Cf = F / (Pc * At)` | R-06 |

Vacuum performance evaluates EQ-THRUST-01 with `Pa = 0`. Standard sea-level
performance evaluates it with `Pa = 101325 Pa` from R-08. Selected-ambient
performance uses the pressure entered by the user. Keep these three results
distinct.

### Standard atmosphere

| ID | Relationship | Source |
|---|---|---|
| EQ-ATM-01 | For a gradient layer, `T = Tb + L(h - hb)` and `P = Pb * (Tb/T)^(g0*M0/(Rstar*L))` | R-08, Part 1, hydrostatic and perfect-gas atmosphere equations |
| EQ-ATM-02 | For an isothermal layer, `P = Pb * exp(-g0*M0*(h - hb)/(Rstar*Tb))` | R-08, Part 1, isothermal-layer pressure equation |

The MVP evaluates the R-08 geopotential-altitude layers from `0` through
`50,000 m`: lapse rates `-0.0065`, `0`, `0.001`, `0.0028`, and `0 K/m` with
bases at `0`, `11,000`, `20,000`, `32,000`, and `47,000 m`. Layer-base
pressure and temperature carry forward continuously from the preceding layer.
This is standard atmosphere, not geometric altitude, measured weather, or a
trajectory atmosphere.

### Application policy equations

These are deterministic product policies rather than claims from the external
flow model.

| ID | Relationship | Authority |
|---|---|---|
| POLICY-NOZZLE-01 | `relativeDifference = abs(Pe - Pa) / max(abs(Pe), abs(Pa))`; define it as zero when both pressures are zero | Decision D-009 |
| POLICY-NOZZLE-02 | `relativeDifference <= 0.02` means ideally expanded; otherwise `Pe > Pa` means underexpanded and `Pe < Pa` means overexpanded | Decision D-009 |
| POLICY-FLOW-01 | Calculated geometry-driven mass flow is authoritative; an entered target flow is comparison-only | Decision D-006 |
| POLICY-FLOW-02 | `relativeTargetDifference = abs(mdot_target - mdot_calculated) / mdot_calculated` | D-006; the authoritative calculated flow is the comparison baseline |
| POLICY-FLOW-03 | `relativeTargetDifference > 0.05` emits a target-flow mismatch warning; the exact 5% boundary does not warn | Product warning policy D-018 |
| POLICY-MODEL-01 | Model-diagnostic evaluation always warns that the 1D steady isentropic ideal-gas model omits chemistry, losses, shocks, and separation; P1-07 attaches these diagnostics to the composed result | Model boundary and D-018 |
| POLICY-MODEL-02 | When `Pa > 0` and `Pe / Pa < 0.4`, warn that severe overexpansion is outside the model's shock/separation capability; do not assert that separation occurs | Conservative product proxy informed by R-10 and confirmed in D-018 |
| POLICY-CALC-01 | Invalid inputs, unbracketed/nonconverged roots, and nonrepresentable arithmetic return structured error issues and no partial performance result | Deterministic failure policy D-019 |
| POLICY-PROFILE-01 | Use normalized display positions `x = 0` at the chamber, `x = 0.35` at the choked throat, and `x = 1` at the exit; these positions do not represent dimensional hardware length | Visualization policy D-027 |
| POLICY-PROFILE-02 | Sample 8 chamber-to-throat segments and 16 throat-to-exit segments. Within each segment use `smoothstep(t) = t²(3 - 2t)`; set `M = smoothstep(t)` before the throat and `M = 1 + (Me - 1)smoothstep(t)` after it, then evaluate EQ-STATE-01/02 at every sample | Deterministic visualization interpolation D-027; state equations remain sourced to R-01, R-02, and R-05 |
| POLICY-PROFILE-03 | Show profiles only for a complete successful result and label the interpolation as neither a solved contour nor a CFD, boundary-layer, shock, or separation prediction | Model-boundary communication D-027 |
| POLICY-ALTITUDE-01 | Sample geopotential altitude from `0` through `50,000 m` in `1,000 m` increments; pair every plotted thrust with its EQ-ATM-01/02 ambient pressure; show a selected-pressure marker only when it has a standard-atmosphere equivalent in that range | Visualization and selected-marker policy D-028 |
| POLICY-ALTITUDE-02 | Hold the solved chamber, mass flow, geometry, and ideal exit state fixed across the altitude sweep and vary only `Pa` in EQ-THRUST-01; label the curve as neither weather/trajectory data nor a prediction of shocks, separation, or other off-design losses | Model-boundary communication D-028 |
| POLICY-COMPARISON-01 | Save at most four immutable, in-memory snapshots by copying the canonical validated inputs and complete successful result; allow one snapshot per current result and retain saved points through later edits, recalculations, and calculation failures | Comparison policy D-029 |
| POLICY-COMPARISON-02 | Saving, removing, and clearing comparison snapshots change only the session comparison collection; no comparison command writes to, restores, or otherwise mutates the active input form | Input-isolation policy D-029 |

## 5. Normalized Profile Visualization

The MVP has throat and exit diameters but no dimensional chamber length,
converging contour, diverging contour, or axial station geometry. A plotted
horizontal position therefore cannot be presented as physical distance. The
profile uses the fixed normalized positions in POLICY-PROFILE-01 and smooth
station-to-station Mach interpolation in POLICY-PROFILE-02 solely to make the
known chamber, choked-throat, and solved-exit states explorable.

The chamber point uses `M = 0`, `P = Pc`, and `T = Tc`; the throat uses
`M = 1`; and the exit uses the supersonic area-Mach solution `Me`. Static
pressure and temperature at every positive-Mach sample use EQ-STATE-01 and
EQ-STATE-02. Automated tests require the profile endpoints to agree with the
composed Core result, the throat to remain exactly Mach 1, every value to be
finite, and Mach/pressure/temperature to be monotonic for the ideal attached
flow represented here.

The interpolation is not an area distribution and must not be used to infer a
nozzle contour, gradient, shock location, separation point, heat-transfer
condition, or boundary-layer behavior.

## 6. Numerical Solver Requirements

The area–Mach relation is double-valued away from the throat. The MVP exit
solver must:

1. Search only the supersonic interval `M in (1, Mmax]`.
2. Use a bounded deterministic method such as bisection or Brent's method.
3. Reject `epsilon < 1` before solving.
4. Return exactly `M = 1` for `epsilon` equal to 1 within the shared numerical
   tolerance.
5. Stop with a diagnostic if the root is not bracketed or the iteration limit is
   reached.
6. Demonstrate a relative area-ratio residual no larger than `1e-8`.

Choose and document a finite `Mmax` when implementing the solver. Do not return a
boundary value as if convergence succeeded.

The P1-04 implementation uses deterministic bisection with a default search
interval of `M in (1, 50]`, a maximum of 100 iterations, and the shared `1e-8`
relative area-ratio tolerance. It evaluates the area ratio in logarithmic form
to reduce overflow risk during bracketing. A caller may configure a tighter
finite Mach bound or iteration limit, but not an unbounded search. A failed
bracket or exhausted iteration limit returns an error diagnostic and no Mach
number.

## 7. Validation Cases

Expected values below are fixed test oracles. Tests must store them as literals;
they must not calculate an “expected” value by calling the production method
being tested.

Unless a case states otherwise:

- Compare ordinary positive scalars with relative error
  `abs(actual - expected) / abs(expected)`.
- Use a relative tolerance of `1e-8` for elementary algebra and `1e-6` for the
  full equation chain.
- Use an absolute tolerance of `1e-12` when the expected value is zero.
- A classification must match exactly.

### VC-01 — Geometry and mixture split

Purpose: validate EQ-GEO-01, EQ-GEO-02, and EQ-MIX-01 through EQ-MIX-03.

Inputs:

| Quantity | Value |
|---|---:|
| Throat diameter | `0.05 m` |
| Exit diameter | `0.31622776601683794 m` |
| Total mass flow | `20 kg/s` |
| Mixture ratio | `3.5` |

Expected:

| Quantity | Value |
|---|---:|
| Throat area | `0.001963495408493621 m²` |
| Exit area | `0.07853981633974483 m²` |
| Expansion ratio | `40` |
| Oxidizer mass flow | `15.555555555555555 kg/s` |
| Fuel mass flow | `4.444444444444445 kg/s` |

Source basis: Euclidean geometry and the R-09 definition of `OF`.

Implementation status: automated in
`NozzleGeometryCalculatorTests.Calculate_WithVc01Geometry_MatchesFixedReferenceValues`
and
`PropellantFlowCalculatorTests.Split_WithVc01Flow_MatchesFixedReferenceValues`.

### VC-02 — NACA perfect-gas isentropic point

Purpose: validate the area–Mach and static-to-total state relations against the
perfect-gas formulation and tables in R-05.

Inputs: `gamma = 1.4`, `M = 2`.

Expected:

| Quantity | Value |
|---|---:|
| `A/A*` | `1.6875` |
| `T/Tt` | `0.5555555555555556` |
| `P/Pt` | `0.12780452546295096` |
| `rho/rho_t` | `0.23004814583331168` |

The test tolerance may be relaxed to `5e-4` only when comparing with values
rounded as printed in the R-05 tables. Tests against the literals above use
`1e-8`.

Implementation status: the supersonic `M = 2` solution is automated in
`SupersonicAreaMachSolverTests`, and the static pressure and temperature ratios
are automated in `NozzleExitCalculatorTests`. The entire point, including the
derived density ratio, is also exercised through `EnginePerformanceCalculator`
in `EnginePerformanceCalculatorTests`.

### VC-03 — Choked mass flow and characteristic velocity identity

Purpose: validate EQ-FLOW-01 and confirm that EQ-CSTAR-01 and EQ-CSTAR-02 agree.

Inputs:

| Quantity | Value |
|---|---:|
| `gamma` | `1.4` |
| `R` | `287.05 J/(kg·K)` |
| `Tc` | `300 K` |
| `Pc` | `1,000,000 Pa` |
| `At` | `0.01 m²` |

Expected:

| Quantity | Value |
|---|---:|
| Choked `mdot` | `23.333553155106348 kg/s` |
| `c* = Pc*At/mdot` | `428.56739106669596 m/s` |
| Closed-form `c*` | `428.56739106669596 m/s` |

Source basis: R-03 choked-flow equation and R-06 characteristic-velocity
definition.

Implementation status: the fixed choked-flow and both characteristic-velocity
oracles are automated in `ChokedMassFlowCalculatorTests` and
`CharacteristicVelocityCalculatorTests`, then rechecked through the composed
calculation boundary in `EnginePerformanceCalculatorTests`.

### VC-04 — Complete ideal-nozzle equation chain

Purpose: integration-style validation from geometry through ambient performance.
This is a synthetic constant-property gas case, not a LOX/methane chemistry
prediction.

Inputs:

| Quantity | Value |
|---|---:|
| `Pc` | `8,000,000 Pa` |
| `Tc` | `3,500 K` |
| `gamma` | `1.22` |
| `R` | `355 J/(kg·K)` |
| Throat diameter | `0.05 m` |
| Expansion ratio | `40` |
| Sea-level `Pa` | `101,325 Pa` |
| `g0` | `9.80665 m/s²` |

Expected:

| Quantity | Value |
|---|---:|
| `At` | `0.001963495408493621 m²` |
| `Ae` | `0.07853981633974483 m²` |
| Exit Mach number | `4.355537019605814` |
| `Te/Tc` | `0.32396247048106236` |
| `Te` | `1,133.8686466837182 K` |
| `Pe/Pc` | `0.0019296147981860943` |
| `Pe` | `15,436.918385488754 Pa` |
| `Ve` | `3,052.229422333306 m/s` |
| Choked `mdot` | `9.193408634242926 kg/s` |
| Momentum thrust | `28,060.392324969314 N` |
| Vacuum pressure thrust | `1,212.412734847917 N` |
| Vacuum thrust | `29,272.80505981723 N` |
| Sea-level pressure thrust | `-6,745.634155776728 N` |
| Sea-level thrust | `21,314.758169192588 N` |
| Vacuum `Isp` | `324.6886449456306 s` |
| Sea-level `Isp` | `236.41943206867248 s` |
| `c*` | `1,708.6114511913577 m/s` |
| Vacuum `Cf` | `1.8635646493741427` |
| Sea-level `Cf` | `1.3569396493741428` |
| Vacuum nozzle state | Underexpanded |
| Sea-level nozzle state | Overexpanded |

Source basis: the R-01 equation chain, R-04 specific impulse, R-06 `c*` and
`Cf`, R-07 `g0`, R-08 sea-level pressure, and project policy D-009.

Implementation status: all numerical VC-04 values through mass flow, exit
state, velocity, vacuum/sea-level thrust, `Isp`, `c*`, and `Cf` are automated
across the P1-03 through P1-05 calculator tests. Vacuum and sea-level
classification are automated in `AmbientPerformanceCalculatorTests`. The full
fixed-oracle chain and attached diagnostics are automated through
`EnginePerformanceCalculatorTests`.

### VC-05 — Nozzle classification tolerance

Purpose: validate POLICY-NOZZLE-01 and POLICY-NOZZLE-02 on each side of the 2%
boundary. Set `Pe = 100000 Pa`.

| Ambient pressure | Symmetric relative difference | Expected class |
|---:|---:|---|
| `101900 Pa` | `0.018645731108930325` | Ideally expanded |
| `102100 Pa` | `0.020568070519098923` | Overexpanded |
| `97000 Pa` | `0.03` | Underexpanded |

Also test `Pe = Pa = 0` as ideally expanded without division by zero.

Implementation status: all listed cases, both exact 2% boundary directions,
values just outside the boundary, equal zero/nonzero pressures, and invalid
inputs are automated in `NozzleExpansionClassifierTests`. The three listed
nonzero cases are also exercised through the composed calculation boundary.

### VC-06 — Standard-atmosphere thrust sweep

Purpose: validate EQ-ATM-01/02 against fixed R-08 lower-atmosphere checkpoints,
then validate POLICY-ALTITUDE-01/02 by applying those pressures to the fixed
VC-04 ideal nozzle state.

Altitudes are geopotential. The engine/nozzle state remains the VC-04 state;
only ambient pressure changes in EQ-THRUST-01.

| Altitude | Standard pressure | Ideal thrust |
|---:|---:|---:|
| `0 m` | `101325 Pa` | `21314.7581691926 N` |
| `10000 m` | `26436.2675938076 Pa` | `27196.5054582912 N` |
| `20000 m` | `5474.88866967778 Pa` | `28842.8083092202 N` |
| `30000 m` | `1171.86650015665 Pa` | `29180.7668801202 N` |
| `40000 m` | `277.521554012952 Pa` | `29251.0085679347 N` |
| `50000 m` | `75.9447675845625 Pa` | `29266.8403717192 N` |

Additional atmosphere checkpoints at the `11`, `32`, and `47 km` layer bases
are `22632.0639734629`, `868.018684755228`, and
`110.906305554966 Pa`. Component tests also require pressure to decrease,
ideal thrust to increase, inverse pressure-to-altitude mapping to recover the
tabulated checkpoints, and unsupported/invalid values to fail deterministically.

Source basis: R-08 for geopotential altitude, constants, layers, and pressure;
VC-04 and EQ-THRUST-01 for the ideal thrust values.

Implementation status: all pressure checkpoints and inverse/boundary behavior
are automated in `StandardAtmosphere1976CalculatorTests`. The 51-point sweep,
fixed thrust checkpoints, nozzle-state progression, and selected-pressure marker
policy are automated in `StandardAtmosphereThrustProfileCalculatorTests` and
rechecked through `EnginePerformanceCalculatorTests`.

## 8. Future Reference Work

- VC-01 through VC-06 now have fixed-literal component coverage. P1-07 added
  composed coverage for VC-02 through VC-05, including the full VC-04 equation
  chain, attached warnings, validation failure, solver failure, and numerical
  failure without a partial result. No tolerance changes were required.
- `P2-05` added the sourced, editable preset register above. A propellant label
  alone never changes thermodynamic values; only an explicit preset selection
  applies the documented set.
- `P3-03` implements R-08 pressure through `50 km` geopotential altitude,
  validates layer/checkpoint pressures in VC-06, and plots the corresponding
  ideal thrust while retaining explicit model-limit text.
- NASA CEA output may become a later validation/import source, but chemistry is
  outside the MVP calculation model.

When adding an equation, add its source and validation case here in the same
change. When superseding a source, preserve the old reference and explain why it
was replaced.
