# CEP HMC feasibility prototype (2026-07-04 review board)

The lattice-physicist's working HMC harness against the REAL platform
action, preserved verbatim from the review-board session and
independently rerun by the adversarial referee. This is a FEASIBILITY
DEMO, not a phase and not a physics result: it samples the omega
sector at theta = 0 (a physically inert slice) on the 16-vertex mesh.

Referee-reproduced numbers: 8.5-8.8 ms/trajectory (20 leapfrog steps,
195 omega-DOF), acceptance 0.996-0.999, <e^{-dH}> = 1.0000
(measure-preservation gate), equipartition <sum omega_i beta dS/domega_i>
= 192-195 vs N = 195 (the platform-gradient + sampler joint gate),
tau_int(S_B) ~ 2, tau_int(Phi) ~ 20-33.

Phase450 (constraint-EP HMC) must NOT reuse this as-is; its four
binding conditions from the adversarial review are recorded in
docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md ("READ THIS FIRST"
section): gauge-invariant invariant-ray collective coordinate,
theta-Haar measure, demonstrated ergodicity control, and the theta=0
slice is never a verdict.

Run: `dotnet run -c Release --project Hmc.csproj` (env knobs in
Program.cs header). Not registered in the traversal build or the
generator; builds standalone.
