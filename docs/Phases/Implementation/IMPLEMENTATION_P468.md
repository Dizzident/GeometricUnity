# Implementation P468 - Two-Loop Content-Row Closure Filter (Team C)

Phase468 implements C-CLOSURE items (ii)/(iii), item 8 of the binding Wave-2
plan.

## Pre-committed menu and filter

The four-row menu is fixed and hash-pinned before scoring:

1. the mandatory Phase451 SM witness;
2. the Phase404 complete-family extension;
3. the Phase407 isolated frame-cross doublet subblock as a four-complex-doublet
   scalar proxy;
4. the full Phase407 frame-cross internal-vector content as four scalar
   doublets plus four scalar color triplets.

The Phase407 rows are explicitly conditional proxies. Phase407 supplies
representation content but no low-energy statistics assignment, action, or
intermediate threshold. The filter uses the full input-to-crossing interval as
a fixed maximal proxy and inherits Phase451's declared gauge-only two-loop
truncation.

The committed Phase451 one-loop coefficients and full two-loop matrix reproduce
exactly. Non-SM matrices add documented standard gauge-only matter deltas.
Step-halving and a synthetic within-band reachability control pass.

## Result

| Row | Two-loop score | Difference from scoring target | In band |
|---|---:|---:|---|
| Phase451 witness | 0.2106371 | -0.0205829 | no |
| Phase404 family extension | 0.2104219 | -0.0207981 | no |
| Phase407 doublet proxy | 0.2260429 | -0.0051771 | no |
| Phase407 full-block proxy | 0.2100478 | -0.0211722 | no |

Terminal:
`two-loop-content-row-closure-filter-full-menu-out-of-band-scoped-exhaustion`.
This supplies C-PERMANENCE P4 only at the audited proxy-menu scope. It is not an
absolute running theorem, coupling determination, or physical prediction.

`menuHash = 1fcff9e863df9b06d89b67802fad216976d8fbbdd8d160fe5f9c5d7f958867d5`.
Nothing is promoted; `promotedPhysicalMassClaimCount = 0`.
