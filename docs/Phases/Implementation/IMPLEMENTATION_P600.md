# Phase600 implementation: full trace adjoint and periodic gradient norm

Prospective A54 study in
`studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001`.
It implements full literal and simplified trace/exterior adjoints,
support-complete independent forward pairings, the actual periodic action
gradient, formal trace/positive norm controls and actual registered core APIs.

The complete code, helper lineage, proof and fixture contract were frozen,
independently reviewed and coordinator-reviewed before explicit first-science
approval. The coordinator ran the unchanged Release build once. It passed
in0.839seconds wall time (user0.871seconds, system0.047seconds), terminal
`full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch`.

Every frozen control passed exactly:256 word cases,16384 Hodge cases,
7 nonzero adjoint blocks,4 contexts,2912 complete K-adjoint forward pairings
(368 nonzero),336 full DQ-adjoint forward pairings (32 nonzero),4 nonzero
omitted-adjoint controls,16 actual-gradient rows,8 formal norm rows,
2 shortcut rejections,108 actual core matrix entries,9 core Gram entries,
27 core structure entries,36 residual coefficients and2 core norm rows.
The run used56,263 coefficient products with largest temporary tensor312.
All17 unique bindings and the live726-file core tree passed. Frozen code,
helpers, proof and contract remained unchanged.

Full and summary reports are byte-identical, SHA256
`fc907ef3c371ab40fb7db375b7c4f6ff80eed5be08bfcd80ee2d98df8d408b7e`.
Frozen contract SHA256
`f288d88fdcda02b32605322fceb84600301e46de655aee6ce969d47b2b543357`;
Program SHA256
`c81502e6def2f75ab0abb6ec009689aa27369d0280f285f2278041312f292444`.

The computed actual gradient is nonzero, including the inner-leg adjoint
that its forward-only evaluation misses. Its massless trace self-pairing
vanishes under the declared indefinite pairing, while its positive coefficient
diagnostic and the registered pointwise positive residual control are nonzero.
This does not select the source norm, a measure, physical spectrum or a
registered mesh-action bridge. All fourteen flags remain false;
O4/external review remain pending and physical mass claims0.
