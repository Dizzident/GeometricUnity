"use strict";
// Static input extraction for one phase project.
//
// Extracts, from Program.cs (and any other .cs files in the project dir)
// plus the .csproj:
//   - every repo-path string literal ("studies/...", "docs/...", "src/...",
//     "native/...", "scripts/...") -- the catch-all that covers the ~103
//     phases reading undeclared inputs via constant paths
//   - directory-scanner roots (Directory.EnumerateFiles/GetFiles/...),
//     resolving simple const-string identifiers and Path.Combine chains;
//     anything unresolvable is reported (fail-closed: forces RUN)
//   - transitive ProjectReference closure into src/
//
// Static extraction is a lower bound on the true read-set (interpolated or
// computed paths escape it); recorded read-sets from capture_readset.js are
// folded into the fingerprint on top of this.

const fs = require("fs");
const path = require("path");

const RE_PATH_LITERAL = /"((?:studies|docs|src|native|scripts)\/[^"\\\n]*)"/g;
const RE_CONST_STRING =
  /(?:const\s+string|static\s+readonly\s+string|(?:^|[\s(])(?:string|var))\s+([A-Za-z_]\w*)\s*=\s*"([^"\\\n]*)"/g;
const RE_CONST_INTERP =
  /(?:const\s+string|static\s+readonly\s+string|(?:^|[\s(])(?:string|var))\s+([A-Za-z_]\w*)\s*=\s*\$"([^"\\\n]*)"/g;
const RE_PROJECT_REF = /ProjectReference\s+Include="([^"]+)"/g;
const RE_REPO_PATH = /^(?:studies|docs|src|native|scripts)\//;

// Resolve a C# interpolated-string template whose placeholders are all
// resolvable identifiers ({Name} only; format specifiers or expressions
// fail resolution). Returns the resolved string or null.
function resolveInterp(template, constMap) {
  let ok = true;
  const out = template.replace(/\{([A-Za-z_]\w*)\}/g, (_, id) => {
    if (Object.prototype.hasOwnProperty.call(constMap, id)) return constMap[id];
    ok = false;
    return "";
  });
  if (!ok || /[{}]/.test(out)) return null;
  return out;
}

function readIfExists(p) {
  try {
    return fs.readFileSync(p, "utf8");
  } catch {
    return null;
  }
}

// Resolve a scanner-call first argument expression to a repo-relative path
// if possible. Handles: "literal", Identifier (via const map), and
// Path.Combine(a, b, ...) where every arg resolves.
function resolveExpr(expr, constMap) {
  expr = expr.trim();
  let m = expr.match(/^"([^"\\]*)"$/);
  if (m) return m[1];
  m = expr.match(/^\$"([^"\\]*)"$/);
  if (m) return resolveInterp(m[1], constMap);
  m = expr.match(/^[A-Za-z_]\w*$/);
  if (m && Object.prototype.hasOwnProperty.call(constMap, expr)) {
    return constMap[expr];
  }
  m = expr.match(/^Path\.Combine\s*\((.*)\)$/s);
  if (m) {
    const parts = splitTopLevelArgs(m[1]).map((a) => resolveExpr(a, constMap));
    if (parts.every((p) => p !== null)) return parts.join("/");
  }
  return null;
}

function splitTopLevelArgs(argText) {
  const args = [];
  let depth = 0;
  let cur = "";
  let inStr = false;
  for (let i = 0; i < argText.length; i++) {
    const c = argText[i];
    if (inStr) {
      cur += c;
      if (c === '"' && argText[i - 1] !== "\\") inStr = false;
      continue;
    }
    if (c === '"') {
      inStr = true;
      cur += c;
    } else if (c === "(") {
      depth++;
      cur += c;
    } else if (c === ")") {
      depth--;
      cur += c;
    } else if (c === "," && depth === 0) {
      args.push(cur);
      cur = "";
    } else {
      cur += c;
    }
  }
  if (cur.trim() !== "") args.push(cur);
  return args;
}

// The first argument of a scanner call, taking parenthesis nesting into
// account (RE_SCAN_CALL's naive split breaks on Path.Combine(...)), so we
// re-extract from source at each match position.
function firstScannerArg(src, matchIndex) {
  const open = src.indexOf("(", matchIndex);
  if (open < 0) return null;
  let depth = 0;
  let inStr = false;
  let cur = "";
  for (let i = open; i < src.length; i++) {
    const c = src[i];
    if (i === open) {
      depth = 1;
      continue;
    }
    if (inStr) {
      cur += c;
      if (c === '"' && src[i - 1] !== "\\") inStr = false;
      continue;
    }
    if (c === '"') {
      inStr = true;
      cur += c;
    } else if (c === "(") {
      depth++;
      cur += c;
    } else if (c === ")") {
      depth--;
      if (depth === 0) return cur;
      cur += c;
    } else if (c === "," && depth === 1) {
      return cur;
    } else {
      cur += c;
    }
  }
  return null;
}

function normalizeRel(p) {
  const n = path.posix.normalize(p.replace(/\\/g, "/"));
  return n.replace(/\/+$/, "");
}

// projectRelPath: e.g. "studies/phase330_x_001/Phase330X.csproj" (repo-relative)
function extractPhaseInputs(repoRoot, projectRelPath) {
  const projectDirRel = path.posix.dirname(projectRelPath.replace(/\\/g, "/"));
  const projectDirAbs = path.join(repoRoot, projectDirRel);
  const ownOutputPrefix = projectDirRel + "/output";

  const csFiles = [];
  const stack = [projectDirAbs];
  while (stack.length) {
    const d = stack.pop();
    let names = [];
    try {
      names = fs.readdirSync(d);
    } catch {
      continue;
    }
    for (const name of names) {
      if (name === "obj" || name === "bin" || name === "output") continue;
      const abs = path.join(d, name);
      const st = fs.statSync(abs);
      if (st.isDirectory()) stack.push(abs);
      else if (name.endsWith(".cs")) csFiles.push(abs);
    }
  }
  csFiles.sort();

  const pathLiterals = new Set();
  const scanRoots = new Set();
  const unresolvedScanRoots = new Set();
  const interpolatedPathHints = new Set();

  for (const csAbs of csFiles) {
    const src = readIfExists(csAbs);
    if (src === null) continue;

    const constMap = {};
    for (const m of src.matchAll(RE_CONST_STRING)) constMap[m[1]] = m[2];

    // Fixpoint-resolve interpolated const strings whose placeholders are
    // themselves resolvable consts (e.g. const F = $"{Root}/fermions").
    const interpDefs = [...src.matchAll(RE_CONST_INTERP)].map((m) => [m[1], m[2]]);
    let progressed = true;
    while (progressed) {
      progressed = false;
      for (const [name, tmpl] of interpDefs) {
        if (Object.prototype.hasOwnProperty.call(constMap, name)) continue;
        const resolved = resolveInterp(tmpl, constMap);
        if (resolved !== null) {
          constMap[name] = resolved;
          progressed = true;
        }
      }
    }

    for (const m of src.matchAll(RE_PATH_LITERAL)) {
      const rel = normalizeRel(m[1]);
      if (rel === projectDirRel || rel.startsWith(projectDirRel + "/")) continue; // self
      pathLiterals.add(rel);
    }

    // Resolved const values that are repo paths count as declared inputs
    // (this covers interpolated consts like $"{Phase12Root}/fermions").
    for (const v of Object.values(constMap)) {
      if (typeof v !== "string" || !RE_REPO_PATH.test(v)) continue;
      const rel = normalizeRel(v);
      if (rel === projectDirRel || rel.startsWith(projectDirRel + "/")) continue;
      pathLiterals.add(rel);
    }

    // Interpolated strings containing repo paths that did NOT resolve are
    // statically unresolvable; record a hint so callers can fail closed.
    for (const m of src.matchAll(/\$"((?:studies|docs|src|native|scripts)\/[^"\n]*)"/g)) {
      if (resolveInterp(m[1], constMap) === null) interpolatedPathHints.add(m[1]);
    }

    const scanRe = /Directory\.(?:EnumerateFiles|GetFiles|EnumerateDirectories|GetDirectories)\s*\(/g;
    for (const m of src.matchAll(scanRe)) {
      const argExpr = firstScannerArg(src, m.index);
      if (argExpr === null) {
        unresolvedScanRoots.add("<unparseable-scanner-call>");
        continue;
      }
      const resolved = resolveExpr(argExpr, constMap);
      if (resolved === null) {
        unresolvedScanRoots.add(argExpr.trim());
        continue;
      }
      const rel = normalizeRel(resolved);
      if (rel === projectDirRel || rel.startsWith(projectDirRel + "/")) continue; // own dirs
      scanRoots.add(rel);
    }
  }

  // Transitive ProjectReference closure.
  const srcRefs = new Set();
  const csprojFiles = fs
    .readdirSync(projectDirAbs)
    .filter((n) => n.endsWith(".csproj"))
    .map((n) => path.join(projectDirAbs, n))
    .sort();
  const seenProj = new Set();
  const projQueue = [...csprojFiles];
  while (projQueue.length) {
    const projAbs = projQueue.shift();
    if (seenProj.has(projAbs)) continue;
    seenProj.add(projAbs);
    const text = readIfExists(projAbs);
    if (text === null) continue;
    for (const m of text.matchAll(RE_PROJECT_REF)) {
      const refAbs = path.resolve(path.dirname(projAbs), m[1].replace(/\\/g, "/"));
      const refRel = path.relative(repoRoot, refAbs).replace(/\\/g, "/");
      if (refRel.startsWith("..")) continue; // outside repo: ignored (fingerprint cannot see it)
      const refDirRel = path.posix.dirname(refRel);
      if (refDirRel !== projectDirRel) srcRefs.add(refDirRel);
      if (fs.existsSync(refAbs)) projQueue.push(refAbs);
    }
  }

  return {
    projectDirRel,
    ownOutputPrefix,
    csprojFiles: csprojFiles.map((p) => path.relative(repoRoot, p).replace(/\\/g, "/")),
    pathLiterals: [...pathLiterals].sort(),
    scanRoots: [...scanRoots].sort(),
    unresolvedScanRoots: [...unresolvedScanRoots].sort(),
    interpolatedPathHints: [...interpolatedPathHints].sort(),
    srcRefs: [...srcRefs].sort(),
  };
}

module.exports = { extractPhaseInputs, resolveExpr, splitTopLevelArgs };
