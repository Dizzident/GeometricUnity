# Phase426: Cox GU Series (I-V) Boson Contract Audit

The 2026-07-01 literature sweep resolved the hinted "Geometric Unity V"
monitor target: Cox published a renumbered five-part series on Zenodo
(2026-06-02..05). Phase426 audits all five records
(GU I 20550275, GU II 20517363, GU III 20517502, GU IV 20518853,
GU V 20531776; md5-verified PDFs, pdftotext extractions).

Result:

- GU I/III/IV/V carry no electroweak contract content (classical framework
  base, BRST/BV legality layer, cosmological export/audit rigs).
- GU II "The Matter Ledger" is electroweak-adjacent: it derives the
  tree-level hypercharge kernel with `g_Y^2 = (3/5) g^2` at a Pati-Salam
  unification point - independently corroborating the repository's blind
  Phase404 result (`tan^2 = 3/5`) - and names the minimal Pati-Salam
  bi-doublet scalar channel `(1,2,2)`, matching the Phase403/409
  doublet-carrier requirement. Its own scope boundaries deny a scalar
  potential, VEV, mass spectrum, unification scale, or measured fit, and it
  does not prove any internal fluctuation contains the channel.

No Phase201 or Phase256 field is filled; no W/Z/H mass is promoted.

Run:

```bash
dotnet run --project studies/phase426_cox_gu_series_boson_contract_audit_001/Phase426CoxGuSeriesBosonContractAudit.csproj
```
