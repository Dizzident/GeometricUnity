# Phase597 implementation: actual quadratic and joint-null controls

Prospective A51 study in `studies/phase597_actual_quadratic_joint_null_audit_001`.
It implements exact Fourier/Clifford full-chain controls, direct and adjoint
quadratic Hessians, a declared joint-coordinate null direction, pure-gauge
flatness jets and pairing-independent zero-torsion stationarity controls.

The complete code, proof, fixture menu and lineage were frozen and independently
reviewed before explicit coordinator approval of the first scientific Release
run. That unchanged run passed in0.385seconds wall time, exit0, with terminal
`actual-quadratic-joint-null-controls-pass-dynamics-unselected`.

All known-answer and scientific controls passed exactly:256 word products,
16384 Hodge masks,4 operator rows,8 full formal-c chain slots,4 raw reciprocity
rejections,12 mass/joint rows,12 wrong-lift rejections,12 fixed-epsilon non-null
controls,8 epsilon jets,32 adjoint pairing checks,56 grade controls,
24 separate pointwise stationarity rows and2 Maurer-Cartan controls.
All15 file bindings and the live726-file core tree passed. No frozen files
were changed after execution.

Full and summary reports are byte-identical, SHA256
`825feb58360b8a17817cfafd2bf323112c00fc2f8d51085dd8e080b59fb72cf6`.
Frozen contract SHA256
`3bb6423f17a2bea308a437be16542ab94942149f60d80fab66041ad92aaf0b34`;
Program SHA256
`f71922fe1e1052afaa2918cc00350c602a8a7bbdb6cc4d91dc700c8d603a7fc5`.

The exact covector Hessian is symmetric; the raw curvature derivative is not
its replacement. The declared joint-coordinate direction is null, while the
fixed-epsilon direction is not. Nonzero pointwise curvature force is detected
by a mixed variation despite its null self-pairing. These are action-consistency
controls, not claims of physical dynamics or stability. All fourteen authority
flags remain false; external review and O4 remain pending. No physical spectrum,
mass or source choice is selected.
