"use strict";
// Deterministic content hash of a directory tree.
//
// NOTE (deviation from the binding design's "git tree hashes" wording):
// scanned roots and study output dirs contain gitignored files, and working
// tree edits must be seen, so git plumbing hashes would be blind to exactly
// the content that matters. This walk hashes actual on-disk content:
// sha256 over the sorted list of (relpath, per-file hash) pairs. JSON files
// inside the tree are hashed canonically (volatile keys stripped) so a
// producer rewriting identical-content outputs does not cascade.

const fs = require("fs");
const path = require("path");
const { sha256Hex, hashFileCanonical } = require("./canonical");
const { TREE_EXCLUDE_DIR_NAMES } = require("./config");

const DEFAULT_EXCLUDES = new Set(TREE_EXCLUDE_DIR_NAMES);

// Returns { hash, fileCount } or null if the directory does not exist.
function dirTreeHash(absDir, opts = {}) {
  if (!fs.existsSync(absDir) || !fs.statSync(absDir).isDirectory()) return null;
  const excludeDirNames = new Set(opts.excludeDirNames || DEFAULT_EXCLUDES);
  const entries = [];
  walk(absDir, "", excludeDirNames, entries);
  entries.sort((a, b) => (a.rel < b.rel ? -1 : a.rel > b.rel ? 1 : 0));
  const lines = entries.map((e) => e.rel + "\n" + e.hash + "\n");
  return { hash: sha256Hex(lines.join("")), fileCount: entries.length };
}

function walk(absDir, relPrefix, excludeDirNames, out) {
  const names = fs.readdirSync(absDir).sort();
  for (const name of names) {
    const abs = path.join(absDir, name);
    const rel = relPrefix ? relPrefix + "/" + name : name;
    let st;
    try {
      st = fs.lstatSync(abs);
    } catch {
      continue; // vanished mid-walk; content change will be caught next pass
    }
    if (st.isSymbolicLink()) {
      out.push({ rel, hash: sha256Hex("symlink:" + fs.readlinkSync(abs)) });
    } else if (st.isDirectory()) {
      if (excludeDirNames.has(name)) continue;
      walk(abs, rel, excludeDirNames, out);
    } else if (st.isFile()) {
      out.push({ rel, hash: hashFileCanonical(abs) });
    }
  }
}

module.exports = { dirTreeHash };
