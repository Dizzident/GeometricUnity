"use strict";
// Adversarial metadata-only process fixture. Never loads a scientific module.
const fs = require("node:fs");
const mode = process.argv[2];
if (mode === "exit0") process.exit(0);
if (mode === "exit7") process.exit(7);
if (mode === "stdout") process.stdout.write(Buffer.alloc(4096, 65));
if (mode === "stderr") process.stderr.write(Buffer.alloc(4096, 66));
if (mode === "diagnostic") process.stderr.write("manufactured rejection\n");
if (mode === "malformed") fs.writeSync(3, Buffer.from("{}\n"));
if (mode === "closed") fs.closeSync(3);
if (mode === "ignore-term") process.on("SIGTERM", () => {});
setInterval(() => {}, 1000);
