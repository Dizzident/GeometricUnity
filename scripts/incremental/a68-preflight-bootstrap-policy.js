"use strict";

// Finite reviewed TRUST ROOT for the engineering-only initialization probe.
// These expected values are frozen after code review, not regenerated at run
// time. This policy itself and Node/native/builtin behavior are explicit trust
// assumptions, not a recursively self-authenticating executable closure.
const expectedRuntime = Object.freeze({ node: "v24.3.0", v8: "13.6.233.10-node.18", platform: "linux", arch: "x64" });
const sourcePins = Object.freeze([
  {
    "id": "a68-canonical-metadata.js",
    "bytes": 3497,
    "sha256": "0351c2f2b593aa6ffee315181454d71010b574658ff03cd64a4b8b94c6b875ef"
  },
  {
    "id": "a68-preflight-initialization-census.js",
    "bytes": 5844,
    "sha256": "15cd53859eb4c1ba301b7cd1ecd53a28aff23a17d79dda8d98e945ce4f5dd314"
  },
  {
    "id": "a68-preflight-initialization-probe.js",
    "bytes": 10807,
    "sha256": "7b0b0ebd471826bf92c42c2f71aa9e2946e9357b41907f9b4560f8535b34d5ae"
  },
  {
    "id": "a68-preflight-module-manifest.js",
    "bytes": 12533,
    "sha256": "58eeaf273f3350294b64ec3c3e6ddf6e628a0748302d2f366dc4ed768b8439b3"
  },
  {
    "id": "a68-trusted-module-loader.js",
    "bytes": 11545,
    "sha256": "3ea7faa4b7af79dc9e51deddad50e147b3a82206803010abd5797174b9e02a2f"
  }
].map(Object.freeze));
module.exports = { expectedRuntime, sourcePins };
