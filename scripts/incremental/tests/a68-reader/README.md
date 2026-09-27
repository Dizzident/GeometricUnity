# Synthetic A68 C# wire-reader tests

Run only this standalone metadata test project:

```sh
dotnet run -c Release --project scripts/incremental/tests/a68-reader/A68ReaderMetadataTests.csproj
```

It references the study assembly and invokes only `MixedTrace.ReadGraph` by
reflection with fabricated JSON, parser limits and independently computed
byte hashes. Loading the assembly does not invoke its `Program.Main`.
No study constructor, evaluator, coefficient replay or scientific output
emitter is called. The Phase627 scientific gate remains unchanged.

Every test must execute and pass. Missing APIs, unexpected exceptions and
assertion failures produce a nonzero exit; there is no skip or warning path.
These checks establish parser behavior, not the correctness or completeness
of the physical recipe, arithmetic replay or boson predictions.
