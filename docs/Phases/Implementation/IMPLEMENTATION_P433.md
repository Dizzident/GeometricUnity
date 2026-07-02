# Implementation P433 - Blind Beta-Coefficient Running Ledger

Phase433 (team wave) extends the Phase429 scale-free ledger with exact
rational one-loop beta coefficients from the Phase404-derived family
content (convention: AF-positive, d g/d ln mu = -b g^3/(16 pi^2)). Rows
(exact): n_f=1 no-Higgs (29/3, 6, -4/3); n_f=1 Higgs (29/3, 35/6, -43/30);
n_f=3 no-Higgs (7, 10/3, -4); n_f=3 Higgs (7, 19/6, -41/10) - the last
reproducing the standard SM set as a correctness witness. The blind
running takes the closed form sin^2(x) = 3(1 - b2 x)/[8 - (5 b1 + 3 b2) x]
with sin^2(0) = 3/8 on every row and a FAMILY-INDEPENDENT linear slope
(-55/32 without Higgs, -109/64 with the conditional Higgs). Family count
is an imported structural parameter (not derived); the Higgs row is
conditional on the missing breaking sector; derivation is strictly
separated from comparison (no measured value appears); no contract fill.
