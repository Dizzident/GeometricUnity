"use strict";

// Audit a captured NONSHARED Release Csc command against post-build MSBuild
// item lists. This discovers declared compiler-file obligations, not trusted
// hashes, complete toolchain imports or source-to-binary/scientific proof.
const path = require("node:path");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const need = (ok, why) => { if (!ok) throw new Error("A68 compiler inputs: " + why); };
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
function fields(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value) && same(Reflect.ownKeys(value), names), "closed ordered own-data fields");
  return Object.fromEntries(names.map(name => { const d = Object.getOwnPropertyDescriptor(value, name); need(d && Object.hasOwn(d, "value"), "own data field " + name); return [name, d.value]; }));
}
function freeze(value) { if (value && typeof value === "object" && !Object.isFrozen(value)) { Object.values(value).forEach(freeze); Object.freeze(value); } return value; }
function auditCompilerInputs(projectDirectory, observed, limits) {
  need(typeof projectDirectory === "string" && path.isAbsolute(projectDirectory) && path.resolve(projectDirectory) === projectDirectory && projectDirectory !== path.parse(projectDirectory).root, "canonical absolute project directory");
  limits = fields(limits, ["arguments", "inputFiles", "pathCharacters", "snapshot"]);
  const snapshot = snapshotCanonicalMetadata({ observed, limits: { arguments: limits.arguments, inputFiles: limits.inputFiles, pathCharacters: limits.pathCharacters } }, limits.snapshot);
  observed = fields(snapshot.value.observed, ["properties", "items"]); const cap = snapshot.value.limits;
  need(Object.values(cap).every(n => Number.isSafeInteger(n) && n > 0), "explicit positive inventory limits");
  const properties = fields(observed.properties, ["Configuration", "TargetFramework", "MSBuildVersion", "NETCoreSdkVersion", "UseSharedCompilation", "Deterministic", "Optimize", "DebugType", "DefineConstants", "TargetPath"]);
  need(properties.Configuration === "Release" && properties.TargetFramework === "net10.0" && properties.UseSharedCompilation === "false" && properties.Deterministic === "true" && properties.Optimize === "true" && properties.DebugType === "portable", "nonshared deterministic optimized Release/net10.0 capture");
  need([properties.MSBuildVersion, properties.NETCoreSdkVersion].every(v => typeof v === "string" && /^\d+\.\d+\.\d+$/.test(v)), "explicit captured tool versions, not authentication");
  const defines = properties.DefineConstants.split(";");
  need(defines.length > 0 && defines.every(v => /^[A-Za-z_][A-Za-z0-9_]*$/.test(v)) && new Set(defines).size === defines.length && defines.includes("TRACE") && defines.includes("RELEASE") && !defines.includes("DEBUG"), "complete unique Release definitions");
  const items = fields(observed.items, ["Compile", "ReferencePath", "Analyzer", "AdditionalFiles", "EditorConfigFiles", "CscCommandLineArgs"]);
  for (const name of Object.keys(items)) need(Array.isArray(items[name]) && items[name].every(value => typeof value === "string" && value.length > 0), "string item arrays " + name);
  const args = items.CscCommandLineArgs;
  need(args.length > 0 && args.length <= cap.arguments, "actual nonempty compiler arguments; incremental skipped compile is not evidence");
  let pathCharacters = 0;
  const resolve = text => {
    need(typeof text === "string" && text.length > 0 && !text.includes("\"") && !text.includes("\\") && !/[\x00-\x1f*?]/.test(text), "unambiguous literal POSIX compiler path without wildcard expansion");
    const absolute = path.resolve(projectDirectory, text);
    need(absolute !== path.parse(absolute).root && absolute.length <= cap.pathCharacters - pathCharacters, "cumulative resolved path-character ceiling");
    pathCharacters += absolute.length; return absolute;
  };
  const unquote = value => {
    if (value.startsWith('"')) { need(value.endsWith('"') && value.length > 2 && !value.slice(1, -1).includes('"'), "one complete compiler path quote pair"); return value.slice(1, -1); }
    need(!value.includes('"'), "no partial path quotes"); return value;
  };
  const categories = { Compile: [], ReferencePath: [], Analyzer: [], AdditionalFiles: [], EditorConfigFiles: [], EmbeddedFiles: [], SourceLink: [] };
  const outputs = new Map(), settings = new Map();
  const fileOptions = { reference: "ReferencePath", analyzer: "Analyzer", additionalfile: "AdditionalFiles", analyzerconfig: "EditorConfigFiles", embed: "EmbeddedFiles", sourcelink: "SourceLink" };
  const exactSettings = new Set(["/noconfig", "/unsafe-", "/checked-", "/fullpaths", "/nostdlib+", "/errorreport:prompt", "/warn:10", "/highentropyva+", "/nullable:enable",
    "/debug-", "/debug:portable", "/filealign:512", "/optimize+", "/target:exe", "/warnaserror-", "/utf8output", "/deterministic+", "/langversion:latest"]);
  for (const raw of args) {
    need(!raw.startsWith("@") && !raw.startsWith('\"@'), "response-file indirection requires separate reviewed closure");
    if (!raw.startsWith("/") || raw.endsWith(".cs") && !/^\/[A-Za-z][A-Za-z0-9+-]*:/.test(raw)) {
      const source = unquote(raw);
      need(source.endsWith(".cs") && !source.startsWith("@") && !/^\/[A-Za-z][A-Za-z0-9+-]*:/.test(source), "literal C# source path, not a quoted compiler switch");
      categories.Compile.push(resolve(source)); continue;
    }
    const colon = raw.indexOf(":"), name = colon < 0 ? raw.slice(1) : raw.slice(1, colon), value = colon < 0 ? null : raw.slice(colon + 1);
    if (Object.hasOwn(fileOptions, name)) {
      need(value !== null, "file option requires a path");
      need(value.startsWith('"') || !/[,;]/.test(value), "list separators require one quoted literal path");
      const file = unquote(value);
      // Alias lists, multi-reference options and other Csc path syntaxes are
      // intentionally not silently interpreted as single filenames.
      need(name !== "reference" || !/[=,;]/.test(file), "single unaliased reference path required");
      categories[fileOptions[name]].push(resolve(file)); continue;
    }
    if (name === "out" || name === "refout") { need(value !== null && !outputs.has(name), "unique compiler output path"); outputs.set(name, resolve(unquote(value))); continue; }
    let key = name;
    if (name === "debug") key = "debug:portable";
    if (name === "define") need(value === properties.DefineConstants, "compiler definitions match post-build properties");
    else if (name === "nowarn" || name === "warnaserror+") need(value !== null && /^(?:[A-Za-z]*\d+)(?:,[A-Za-z]*\d+)*$/.test(value), "explicit diagnostic code list");
    else if (name === "features") need(value === '"InterceptorsNamespaces=;Microsoft.Extensions.Validation.Generated"', "closed current SDK feature switch");
    else need(exactSettings.has(raw), "unreviewed compiler option " + name);
    need(!settings.has(key), "duplicate compiler setting " + key); settings.set(key, raw);
  }
  for (const required of ["noconfig", "nostdlib+", "define", "optimize+", "deterministic+", "target", "debug:portable"])
    need(settings.has(required), "mandatory compiler setting " + required);
  need(outputs.has("out") && outputs.has("refout") && outputs.get("out") !== outputs.get("refout"), "distinct assembly and reference outputs");
  need(categories.SourceLink.length === 1, "one declared SourceLink input");
  need(typeof properties.TargetPath === "string" && path.isAbsolute(properties.TargetPath) && path.basename(properties.TargetPath) === path.basename(outputs.get("out")), "reported target names the compiled assembly (copy not proved)");
  for (const name of ["Compile", "ReferencePath", "Analyzer", "AdditionalFiles", "EditorConfigFiles"]) {
    const listed = items[name].map(resolve), actual = categories[name];
    need(new Set(listed).size === listed.length && new Set(actual).size === actual.length && same(actual, listed), "exact complete ordered item/argument census " + name);
  }
  need(categories.Compile.length > 0 && categories.ReferencePath.length > 0 && categories.Analyzer.length > 0 && categories.EditorConfigFiles.length > 0, "complete nonempty source/reference/analyzer/config lists");
  need(new Set(categories.EmbeddedFiles).size === categories.EmbeddedFiles.length && categories.EmbeddedFiles.every(file => categories.Compile.includes(file)), "embedded source files must be declared compiler inputs");
  const byPath = new Map();
  for (const [role, files] of Object.entries(categories)) for (const file of files) {
    if (!byPath.has(file)) { need(byPath.size < cap.inputFiles, "unique input file ceiling"); byPath.set(file, []); }
    byPath.get(file).push(role);
  }
  for (const file of outputs.values()) need(!byPath.has(file), "output cannot alias a compiler input");
  return freeze({ properties, definitions: defines, arguments: args, categories,
    inputs: [...byPath].sort(([a], [b]) => a < b ? -1 : a > b ? 1 : 0).map(([file, roles]) => ({ path: file, roles })),
    outputs: { assembly: outputs.get("out"), referenceAssembly: outputs.get("refout"), target: properties.TargetPath },
    usage: { arguments: args.length, inputFiles: byPath.size, pathCharacters, snapshot: snapshot.usage },
    scope: { compilerArgumentItemCensusMatched: true, reportedCompilerInputsEnumerated: true,
      inputBytesVerified: false, toolchainImportsComplete: false, runtimeDependenciesComplete: false,
      generatedInMemorySourcesCaptured: false, sourceToBinaryCorrespondenceProved: false, scientificExecutionAuthorized: false } });
}
module.exports = { auditCompilerInputs };
