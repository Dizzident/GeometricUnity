"use strict";
const test = require("node:test"), assert = require("node:assert/strict");
const { auditCompilerInputs } = require("../a68-compiler-inputs");
const root = "/tmp/manufactured-project";
const limits = () => ({ arguments: 100, inputFiles: 100, pathCharacters: 100000,
  snapshot: { nodes: 10000, arraySlots: 10000, stringCharacters: 100000, maxDepth: 16 } });
function fixture() {
  return { properties: { Configuration: "Release", TargetFramework: "net10.0", MSBuildVersion: "18.0.2", NETCoreSdkVersion: "10.0.100",
    UseSharedCompilation: "false", Deterministic: "true", Optimize: "true", DebugType: "portable", DefineConstants: "TRACE;RELEASE;NET10_0", TargetPath: root + "/bin/Release/a.dll" },
    items: { Compile: ["Program.cs", "obj/generated.cs", "obj/a,version.cs"], ReferencePath: ["/tmp/ref/core.dll"], Analyzer: ["/tmp/sdk/analyzer.dll"],
      AdditionalFiles: ["data/input.json"], EditorConfigFiles: ["obj/compiler.editorconfig"], CscCommandLineArgs: [
        "/noconfig", "/nostdlib+", "/define:TRACE;RELEASE;NET10_0", "/optimize+", "/deterministic+", "/debug-", "/debug:portable", "/target:exe",
        "/out:obj/a.dll", "/refout:obj/ref/a.dll", "/reference:/tmp/ref/core.dll", "/analyzer:/tmp/sdk/analyzer.dll", "/additionalfile:data/input.json",
        "/analyzerconfig:obj/compiler.editorconfig", "/sourcelink:obj/source.json", "/embed:obj/generated.cs", '/embed:"obj/a,version.cs"',
        "Program.cs", "obj/generated.cs", '"obj/a,version.cs"' ] } };
}
test("reported compiler inputs include generated embedded sources references analyzers configs and SourceLink without provenance claims", () => {
  const result = auditCompilerInputs(root, fixture(), limits());
  assert.equal(result.inputs.length, 8); assert.equal(result.categories.Compile.length, 3); assert.equal(result.categories.EmbeddedFiles.length, 2);
  assert.deepEqual(result.inputs.find(f => f.path === root + "/obj/generated.cs").roles, ["Compile", "EmbeddedFiles"]);
  assert.deepEqual(result.outputs, { assembly: root + "/obj/a.dll", referenceAssembly: root + "/obj/ref/a.dll", target: root + "/bin/Release/a.dll" });
  for (const key of ["inputBytesVerified", "toolchainImportsComplete", "runtimeDependenciesComplete", "generatedInMemorySourcesCaptured", "sourceToBinaryCorrespondenceProved", "scientificExecutionAuthorized"]) assert.equal(result.scope[key], false);
  assert.ok(Object.isFrozen(result.inputs[0].roles));
});
test("empty incremental compiler arguments cannot stand in for an observed compile", () => {
  const f = fixture(); f.items.CscCommandLineArgs = []; assert.throws(() => auditCompilerInputs(root, f, limits()), /incremental skipped/);
});
test("shared compiler Debug nondeterministic and altered definitions cannot enter the frozen Release lane", () => {
  for (const [key, value] of [["Configuration", "Debug"], ["UseSharedCompilation", "true"], ["Optimize", "false"], ["Deterministic", "false"],
    ["TargetFramework", "net9.0"], ["DebugType", "none"], ["DefineConstants", "TRACE;RELEASE;DEBUG"], ["DefineConstants", "TRACE;RELEASE;RELEASE"], ["DefineConstants", "TRACE;RELEASE;CHANGED"]]) {
    const f = fixture(); f.properties[key] = value; assert.throws(() => auditCompilerInputs(root, f, limits()));
  }
});
test("hidden omitted duplicated reordered and aliased source or dependency items fail exact normalized census", () => {
  for (const key of ["Compile", "ReferencePath", "Analyzer", "AdditionalFiles", "EditorConfigFiles"]) {
    for (const mode of ["missing", "extra", "duplicate"]) {
      const f = fixture(); if (mode === "missing") f.items[key].pop(); else f.items[key].push(mode === "extra" ? "hidden.file" : f.items[key][0]);
      assert.throws(() => auditCompilerInputs(root, f, limits()), /census/);
    }
  }
  const f = fixture(); f.items.Compile.reverse(); assert.throws(() => auditCompilerInputs(root, f, limits()), /census/);
  const g = fixture(); g.items.Compile.push("obj/../Program.cs"); g.items.CscCommandLineArgs.push("obj/../Program.cs"); assert.throws(() => auditCompilerInputs(root, g, limits()), /census/);
});
test("response files unreviewed switches and reference aliases cannot hide additional inputs", () => {
  for (const arg of ["@hidden.rsp", '\"@hidden.rsp\"', '\"/reference:/tmp/hidden.cs\"', "/keyfile:secret.snk", "/resource:hidden.bin", "/link:extra.dll", "/reference:alias=/tmp/ref/core.dll",
    "/reference:/tmp/ref/core.dll,/tmp/ref/extra.dll", "/features:unreviewed", "/analyzer:", '/analyzer:"partial', "/embed:extra.cs",
    "*.cs", "/analyzer:/tmp/*.dll", "/embed:obj/a.cs,obj/b.cs", "/additionalfile:one.json;two.json"]) {
    const f = fixture(); f.items.CscCommandLineArgs.push(arg); assert.throws(() => auditCompilerInputs(root, f, limits()));
  }
});
test("mandatory switches SourceLink and assembly output identity cannot be omitted duplicated or made to alias inputs", () => {
  for (const prefix of ["/noconfig", "/define:", "/optimize+", "/deterministic+", "/target:", "/sourcelink:", "/out:", "/refout:"]) {
    const f = fixture(); f.items.CscCommandLineArgs = f.items.CscCommandLineArgs.filter(a => !a.startsWith(prefix)); assert.throws(() => auditCompilerInputs(root, f, limits()));
  }
  const f = fixture(); f.items.CscCommandLineArgs.push("/optimize+"); assert.throws(() => auditCompilerInputs(root, f, limits()), /duplicate/);
  const g = fixture(); g.items.CscCommandLineArgs[8] = "/out:Program.cs"; g.properties.TargetPath = root + "/bin/Program.cs";
  assert.throws(() => auditCompilerInputs(root, g, limits()), /output cannot alias/);
  const h = fixture(); h.properties.TargetPath = root + "/bin/other.dll"; assert.throws(() => auditCompilerInputs(root, h, limits()), /reported target/);
});
test("own-data snapshots quotas and detached immutable inventory are enforced", () => {
  const f = fixture(); let calls = 0; Object.defineProperty(f.properties, "Configuration", { enumerable: true, get() { calls++; return "Release"; } });
  assert.throws(() => auditCompilerInputs(root, f, limits()), /own data/); assert.equal(calls, 0);
  for (const change of [c => c.arguments = 1, c => c.inputFiles = 1, c => c.pathCharacters = 1, c => c.snapshot.nodes = 1]) {
    const cap = limits(); change(cap); assert.throws(() => auditCompilerInputs(root, fixture(), cap));
  }
  const g = fixture(), result = auditCompilerInputs(root, g, limits()); g.items.Compile[0] = "mutated.cs"; g.properties.DefineConstants = "DEBUG";
  assert.equal(result.categories.Compile[0], root + "/Program.cs"); assert.ok(result.definitions.includes("RELEASE"));
});
test("absolute quoted source paths and normalized SDK parent segments stay distinct from compiler switches", () => {
  const f = fixture(); f.items.Compile[0] = root + "/Program.cs"; f.items.CscCommandLineArgs[17] = '"' + root + '/Program.cs"';
  f.items.Analyzer[0] = "/tmp/sdk/targets/../analyzer.dll";
  assert.equal(auditCompilerInputs(root, f, limits()).categories.Analyzer[0], "/tmp/sdk/analyzer.dll");
});
