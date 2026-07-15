#!/usr/bin/env node
"use strict";

// Exact O4 coverage register generator (non-phase tooling).
//
//   node scripts/o4_register/generate.js [--check]
//
// Coverage is governed by coverage_contract.json. Discovery remains recursive,
// but every pending marker, pointer, and structured review predicate must match
// that contract exactly. Nothing is inferred from prose or filenames.

const fs = require("fs");
const path = require("path");
const {
  collectCoverage,
  loadJsonStrict,
  renderRegister,
  validateContract,
} = require("./lib");

const repoRoot = path.resolve(__dirname, "..", "..");
const contractPath = path.join(__dirname, "coverage_contract.json");
const outPath = path.join(
  repoRoot,
  "docs",
  "Phases",
  "Adjudication",
  "O4_CONVENTIONS_REGISTER.md"
);

function main() {
  try {
    const check = process.argv.includes("--check");
    const unknownArgs = process.argv.slice(2).filter((arg) => arg !== "--check");
    if (unknownArgs.length !== 0) {
      throw new Error(`unknown argument(s): ${unknownArgs.join(", ")}`);
    }

    const contract = loadJsonStrict(contractPath);
    validateContract(contract);
    const rows = collectCoverage(repoRoot, contract);
    const content = renderRegister(rows, contract);

    if (check) {
      const existing = fs.existsSync(outPath) ? fs.readFileSync(outPath, "utf8") : "";
      if (existing !== content) {
        process.stderr.write(
          "O4 register is stale; run: node scripts/o4_register/generate.js\n"
        );
        process.exitCode = 1;
        return;
      }
      process.stdout.write("O4 register is current.\n");
      return;
    }

    fs.mkdirSync(path.dirname(outPath), { recursive: true });
    fs.writeFileSync(outPath, content);
    process.stdout.write(`wrote ${path.relative(repoRoot, outPath)}\n`);
  } catch (error) {
    process.stderr.write(`O4 coverage error: ${error.message}\n`);
    process.exitCode = 1;
  }
}

main();
