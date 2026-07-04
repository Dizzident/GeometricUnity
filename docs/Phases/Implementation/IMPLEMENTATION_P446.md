# Implementation P446 - RG Scheme-Dependence Resolution Probe

Phase446 resolves the scheme dependence phase445 recorded: were the
first-ever interior minima genuine RG-resummation structure, or an
artifact of the fit prescription? A PRE-REGISTERED artifact mechanism
was under test: the phase445 one-loop fit basis {t^4} leaks the
t-independent one-loop constant (deeply negative for the Einsteinian
members; pure vacuum normalization that provably cannot move the true
V_eff minimum) into the fitted coefficient, forcing V_RG -> 0 at t -> 0
and manufacturing an interior dip for any deeply negative, slowly
rising one-loop curve. The positive-one-loop identity control is
structurally blind to this artifact class.

## Construction

The V_eff(t) rays are recomputed with the VERBATIM phase443/445
hardened machinery (same members, seeds, and ray directions as
phase445) on a denser 48-point grid whose strided subset is EXACTLY
the phase445 16-point grid. Four decisive arms:

1. REPLICATION - verbatim phase445 prescription on the even subset,
   compared against the recorded phase445 classifications.
2. OFFSET INVARIANCE - the same prescription with the one-loop curve
   shifted by constants (min-t subtract / mean subtract / +1000); the
   raw V_eff argmin is exactly shift-invariant (asserted battery), so
   any classification change under a shift is unphysical.
3. FIT-BASIS MENU - enriched one-loop bases ({1,t^4}, {1,ln t,t^4},
   canonical CW {t^4, t^4 ln t}) x windows x both grids x raw/
   constant-subtracted normalizations; classification stability across
   the menu is the honest scheme-dependence measurement.
4. DIRECT (CONSTANT-IMMUNE) - the log-derivative operator L = t d/dt
   annihilates constants exactly; g(t) = dV_1loop/d ln t by jump-aware
   midpoint differences (only unchanged-mode-count intervals), CW log
   coefficient cL extracted in the L-image basis {1, 2t^2, 3t^3, 4t^4,
   4t^4 ln t + t^4} (the constant column is the L-image of the known
   s ln t one-loop asymptotic - omitting it provably biases cL); a
   resummation minimum requires cL > 0 with in-range t*; offset-immunity
   battery re-derives cL under V_1loop + 1000; fit quality gated on
   CENTERED R^2.
5. SYNTHETIC ARTIFACT-SENSITIVE CONTROL - a provably monotone hand-built
   curve with the member shape (positive tree + deep negative constant +
   slow log rise; dV/dt > 0 everywhere). The identity control is
   structurally blind to the constant-leakage artifact; this control is
   not. Any scheme classifying it interior-finite is manufacturing
   minima (recorded); the direct arm supporting a minimum on it
   fail-closes the phase.

Survival requires ALL of: menu survival, constant-subtraction survival,
and direct support. The phase passes on internal consistency regardless
of the resolution outcome.

## Verdict: RESOLVED - the phase445 minima are a fit-normalization artifact

- Replication exact on the strided 48-point grid (matches=true).
- Constant subtraction kills every interior minimum at every window on
  both grids; the raw V_eff argmin is provably shift-invariant.
- The 4-basis fit menu is irreducibly scheme-dependent; NO member
  survives; one enriched scheme hands even the identity control a
  manufactured minimum.
- The provably-monotone synthetic control receives a manufactured
  minimum from the verbatim scheme at every window; the direct arm
  stays clean on it.
- The constant-immune direct measurement supports no minimum:
  sd2 cL=-0.79 (nonpositive); asd2 cL=+4.45 with implied t* far
  outside the grid; offset-immunity exact; 30-40 usable jump-free
  intervals per member; centered-R^2 gate met.
- Theta gate: relative <= 1e-8 or absolute-gradient floor 1e-10 (2
  smallest-t points via the floor - the Phase443 dimensional-gate
  lesson at tiny t).

einsteinianRgSaturationObserved=false;
candidateSurvivesSchemeControl=false. THE RG-IMPROVED POTENTIAL-FIT
ROUTE ON THE MINIMAL MESH IS CLOSED. Method lessons: gate on CENTERED
R^2; a control must be able to exhibit the artifact class it guards;
windowed-fit classifiers must be constant-shift invariant before their
minima are believed. Runtime ~15 min (Release).

## Named Next

If the resolution is artifact/grid-fragile: the RG-improved
potential-fit route on the minimal mesh is closed, and the frontier
returns to the named levers (the two phase444 unlock projects -
lattice-canonical geometry conventions or the adjoint/joint-gradient
path - plus beyond-one-loop structure or a source anchor). If it
survives: the first scheme-controlled workbench-relative
dynamical-scale candidate (still candidate-only, no GeV). Every RG
prescription here remains a workbench convention with
physicistReviewPending=true. Nothing promoted either way.
