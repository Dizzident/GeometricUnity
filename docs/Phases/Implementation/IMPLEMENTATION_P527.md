# Phase527 Implementation Contract

Phase527 implements Amendment A20's exact SD2 theta-zero parity audit. It
reconstructs the registered `sd2-id0/c0.5` member on the lattice-canonical
extent-3 complex using exact rational arithmetic, the exact theta-zero
endomorphism `R=(I+star)/4`, and the frozen 768-row signed-edge menu.

The first preregistered row is decisive. It gives
`action(+omega)-action(-omega)=13/180`, equivalently cubic coefficient
`13/360`, after including all 4,050 faces. Binary64 replay is not evidence.
The result refutes omega-sign evenness only for this registered member on the
theta-zero slice; it makes no general-theta or reflection-positivity claim.

Validated artifact hashes:

- program: `7627fd957db1eb84cc133f351612292f493c2aed81b75c73ae4694cb74172823`
- contract: `eba343403cb34dfa17a77b8d287ee7d6e775ac65c6886fa113f22a3074a556ac`
- summary/full: `a919f6ff4470248ccbcdb1193436dffcf0da2c49123596fc1a6ebb2e79bde797`

All execution and promotion authorities are false, external review remains
pending, and `promotedPhysicalMassClaimCount=0`.
