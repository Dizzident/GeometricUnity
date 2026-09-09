# Phase595 implementation: invariant tensor dimensions

Amendment A50, prospective exact deterministic audit in
`studies/phase595_invariant_tensor_dimension_audit_001`.

The program classifies the full degree-one and degree-two invariant tensor
spaces by complexified sign characters and signed quarter-turn constraints.
Phase590 supplies independent real lower bounds; the finite subgroup supplies
only upper bounds. The proof explicitly retains the vector/dual basis phases
and distinguishes Euclidean complex volume square-1 from the original square+1.

Expected counts:1720320 coefficient positions,22364160 character evaluations,
28/182 survivors,56/364 missing-character controls,364/2366 signed constraints,
two components per degree and ranks26/180. Both sign-error decoys are frozen.

The first coordinator-approved frozen Release execution passed in0.597seconds
wall time (shell timing), exit0, with terminal
`invariant-tensor-dimensions-two-certified-source-choice-open`.
All known-answer and scientific controls passed exactly. Before execution,
the coordinator and independent reviewer read the complete pack and verified
the nine bindings; the program then verified them and the726-file live core
tree itself. An unavailable `/usr/bin/time` wrapper returned127 before dotnet
started; the approved scientific run used the shell's builtin timing instead.
No scientific failure occurred and no frozen artifact was changed.

Executed counts are1720320 coefficient positions and22364160 character checks;
212992 rotation-mask known answers pass. The degree1/2 survivor counts are
28/182, missing-character controls56/364, signed rows364/2366, ranks26/180,
and non-tree checks338/2186. Each degree has exactly two components, and the
bound real lower bounds match. All52 wrong-dual and26 wrong-complement-sign
controls detect their planted errors. Therefore both declared real invariant
spaces have dimension2. This does not select either tensor direction.

Frozen contract SHA256:
`d2d32ff4e3689a05b4d46276b43e20874fc9e73ca259b80413191c43cc7fc54e`.
Program SHA256:
`36ed19de33b56f5d83fb841db8c0116b24e937f169e6cfdcca703684a08a5b30`.
Identical full/summary output SHA256:
`e42ee13516ce4140a7eb8e37ef4dbf95af4e41af4c08258b16a7aa8f547cc071`.
Only this unbound implementation note was updated after the first run.

No source operator or normalization is selected. All fourteen authority flags
remain false, externalReviewPending=true, promotedPhysicalMassClaimCount=0,
O4 pending and Phase561 closed. Main owns shared integration and validation.
