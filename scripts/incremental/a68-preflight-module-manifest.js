"use strict";

// Explicit reviewed LOCAL metadata-preflight source declarations. This data
// table is not generated at runtime from current files or require.cache.
// Static require scans are only a drift aid; review binds these exact hashes.
// Builtin implementations, globals, loader/bootstrap/Node/native libraries,
// initialization feasibility and scientific authorization remain OUTSIDE it.
const path = require("node:path");
const freeze = value => { if (value && typeof value === "object") { Object.values(value).forEach(freeze); Object.freeze(value); } return value; };
const PREFLIGHT_SOURCE_PINS = freeze([
  {
    "id": "a68-acceleration-recipe.js",
    "bytes": 7198,
    "sha256": "095f87f705d7ee10e6017aaca83e624285524fcde3f3b08c17820bffffe4a39b",
    "imports": [
      {
        "request": "./a68-background-recipe",
        "target": "a68-background-recipe.js"
      }
    ]
  },
  {
    "id": "a68-audit-context-recipe.js",
    "bytes": 27082,
    "sha256": "948a79fb9a313901860a41662516e0ac0e28ea68d45b180da3f18fd7e260b5d2",
    "imports": [
      {
        "request": "./a68-background-recipe",
        "target": "a68-background-recipe.js"
      },
      {
        "request": "./a68-variation-recipe",
        "target": "a68-variation-recipe.js"
      },
      {
        "request": "./a68-ward-controls-recipe",
        "target": "a68-ward-controls-recipe.js"
      }
    ]
  },
  {
    "id": "a68-background-recipe.js",
    "bytes": 12887,
    "sha256": "f8a98b1e570b7f0bfdca91431d4fd9ce4aef713459f26bc902eb7204fa24d582",
    "imports": []
  },
  {
    "id": "a68-branch-topology-envelope.js",
    "bytes": 3676,
    "sha256": "7a21a1e5b2fe8b4f70b000afeaf142424c7c4fd079d8259bb2118650020a0ffd",
    "imports": []
  },
  {
    "id": "a68-canonical-metadata.js",
    "bytes": 3497,
    "sha256": "0351c2f2b593aa6ffee315181454d71010b574658ff03cd64a4b8b94c6b875ef",
    "imports": []
  },
  {
    "id": "a68-canonical-wire.js",
    "bytes": 2744,
    "sha256": "f3cba078159676d4d7ce0663653edeff779e7e0756c44dac19bf11821cc3b3c0",
    "imports": []
  },
  {
    "id": "a68-checkpoint-catalog.js",
    "bytes": 9464,
    "sha256": "4a0378edb438d5aae7af0ba423e8cfb94b95e0b40a5fa7890f2b1e2d9ee9baef",
    "imports": [
      {
        "request": "./a68-canonical-metadata",
        "target": "a68-canonical-metadata.js"
      },
      {
        "request": "./a68-source-context-menu",
        "target": "a68-source-context-menu.js"
      },
      {
        "request": "./a68-source-template-validator",
        "target": "a68-source-template-validator.js"
      },
      {
        "request": "node:crypto",
        "target": "node:crypto"
      },
      {
        "request": "node:path",
        "target": "node:path"
      }
    ]
  },
  {
    "id": "a68-compiler-inputs.js",
    "bytes": 9649,
    "sha256": "0feb5c1c18dc131580d5965b5e20b65f5b458046d51452eb537ffbe146223a9c",
    "imports": [
      {
        "request": "./a68-canonical-metadata",
        "target": "a68-canonical-metadata.js"
      },
      {
        "request": "node:path",
        "target": "node:path"
      }
    ]
  },
  {
    "id": "a68-context-topology-envelope.js",
    "bytes": 10296,
    "sha256": "4add2a38fe7e4c2cc180423ebb78cc8d7fb7b6246eb2882e86ba4dcda17b9b2f",
    "imports": [
      {
        "request": "./a68-branch-topology-envelope",
        "target": "a68-branch-topology-envelope.js"
      },
      {
        "request": "./a68-source-context-menu",
        "target": "a68-source-context-menu.js"
      },
      {
        "request": "./a68-ward-controls-recipe",
        "target": "a68-ward-controls-recipe.js"
      }
    ]
  },
  {
    "id": "a68-diagnostics-recipe.js",
    "bytes": 33732,
    "sha256": "b7386980ed2404bdcb86467cb14219cabafb842f1bedd1d7df1fdda901b233fe",
    "imports": [
      {
        "request": "./a68-acceleration-recipe",
        "target": "a68-acceleration-recipe.js"
      },
      {
        "request": "./a68-audit-context-recipe",
        "target": "a68-audit-context-recipe.js"
      },
      {
        "request": "./a68-background-recipe",
        "target": "a68-background-recipe.js"
      },
      {
        "request": "./a68-mixed-recipe",
        "target": "a68-mixed-recipe.js"
      },
      {
        "request": "./a68-original-action-recipe",
        "target": "a68-original-action-recipe.js"
      },
      {
        "request": "./a68-variation-recipe",
        "target": "a68-variation-recipe.js"
      },
      {
        "request": "./a68-ward-controls-recipe",
        "target": "a68-ward-controls-recipe.js"
      },
      {
        "request": "./a68-ward-recipe",
        "target": "a68-ward-recipe.js"
      }
    ]
  },
  {
    "id": "a68-launch-file-admission.js",
    "bytes": 7722,
    "sha256": "03a85e1fdbec86379f172e01c00497c0a9fb0f18cb37d361319ab8902f879a2c",
    "imports": [
      {
        "request": "./a68-canonical-metadata",
        "target": "a68-canonical-metadata.js"
      },
      {
        "request": "node:crypto",
        "target": "node:crypto"
      },
      {
        "request": "node:fs",
        "target": "node:fs"
      },
      {
        "request": "node:path",
        "target": "node:path"
      }
    ]
  },
  {
    "id": "a68-mixed-recipe.js",
    "bytes": 18929,
    "sha256": "aeb95711a55e36065ea1d2a33acf1d78fd1c2495567f9e40f8b053fbd5f05990",
    "imports": []
  },
  {
    "id": "a68-original-action-recipe.js",
    "bytes": 7438,
    "sha256": "b74292bed180dae241d5d8f444413d48851a82a04e57b6f0bc86b4362782fc5d",
    "imports": [
      {
        "request": "./a68-mixed-recipe",
        "target": "a68-mixed-recipe.js"
      }
    ]
  },
  {
    "id": "a68-preflight-entry.js",
    "bytes": 833,
    "sha256": "febaabf10b16c04c45bcaadf2d13be6eea0f8ae13fdb059672e9c1237855580b",
    "imports": [
      {
        "request": "./a68-template-preflight-service",
        "target": "a68-template-preflight-service.js"
      },
      {
        "request": "./a68-template-process-host",
        "target": "a68-template-process-host.js"
      },
      {
        "request": "./a68-topology-source-admission",
        "target": "a68-topology-source-admission.js"
      }
    ]
  },
  {
    "id": "a68-second-jet-recipe.js",
    "bytes": 11050,
    "sha256": "9dcc41f4d1f461615a1fc58f0bd539d086065080f952a00a345517697b947505",
    "imports": [
      {
        "request": "./a68-mixed-recipe",
        "target": "a68-mixed-recipe.js"
      }
    ]
  },
  {
    "id": "a68-source-context-menu.js",
    "bytes": 14136,
    "sha256": "0e13bc152cdd0cbc45ea83237341591a5e9ed4240c54f4c95aec0f6742e78feb",
    "imports": [
      {
        "request": "./a68-audit-context-recipe",
        "target": "a68-audit-context-recipe.js"
      },
      {
        "request": "./a68-background-recipe",
        "target": "a68-background-recipe.js"
      },
      {
        "request": "./a68-diagnostics-recipe",
        "target": "a68-diagnostics-recipe.js"
      },
      {
        "request": "./a68-second-jet-recipe",
        "target": "a68-second-jet-recipe.js"
      },
      {
        "request": "./a68-ward-controls-recipe",
        "target": "a68-ward-controls-recipe.js"
      }
    ]
  },
  {
    "id": "a68-source-template-validator.js",
    "bytes": 12255,
    "sha256": "7dcf063502169a98bdb1e7d145e6c32f523ef10f04810f1a286ac6fe9e23e238",
    "imports": [
      {
        "request": "./a68-canonical-metadata",
        "target": "a68-canonical-metadata.js"
      },
      {
        "request": "./a68-context-topology-envelope",
        "target": "a68-context-topology-envelope.js"
      },
      {
        "request": "./a68-source-context-menu",
        "target": "a68-source-context-menu.js"
      }
    ]
  },
  {
    "id": "a68-template-preflight-service.js",
    "bytes": 7600,
    "sha256": "32f63886ea68ba61c9c9b04ae200bb68cbf07f9f9602eea94492abe5037b6aa0",
    "imports": [
      {
        "request": "./a68-canonical-metadata",
        "target": "a68-canonical-metadata.js"
      },
      {
        "request": "./a68-canonical-wire",
        "target": "a68-canonical-wire.js"
      },
      {
        "request": "./a68-checkpoint-catalog",
        "target": "a68-checkpoint-catalog.js"
      },
      {
        "request": "./a68-source-context-menu",
        "target": "a68-source-context-menu.js"
      },
      {
        "request": "./a68-source-template-validator",
        "target": "a68-source-template-validator.js"
      },
      {
        "request": "node:crypto",
        "target": "node:crypto"
      },
      {
        "request": "node:stream/promises",
        "target": "node:stream/promises"
      }
    ]
  },
  {
    "id": "a68-template-process-host.js",
    "bytes": 10631,
    "sha256": "047f4a40725c672ec0bdbc63a900e71cd1579b0f7c8889ea2d7852f8e099b5d8",
    "imports": [
      {
        "request": "./a68-canonical-metadata",
        "target": "a68-canonical-metadata.js"
      },
      {
        "request": "./a68-checkpoint-catalog",
        "target": "a68-checkpoint-catalog.js"
      },
      {
        "request": "./a68-source-template-validator",
        "target": "a68-source-template-validator.js"
      },
      {
        "request": "./a68-template-preflight-service",
        "target": "a68-template-preflight-service.js"
      },
      {
        "request": "node:child_process",
        "target": "node:child_process"
      },
      {
        "request": "node:path",
        "target": "node:path"
      },
      {
        "request": "node:perf_hooks",
        "target": "node:perf_hooks"
      }
    ]
  },
  {
    "id": "a68-topology-source-admission.js",
    "bytes": 7500,
    "sha256": "11fb1d51c0e19cf0ff808ff6c67538a43b2d3e697a8dbac1e788fdd29d5d5f17",
    "imports": [
      {
        "request": "./a68-canonical-metadata",
        "target": "a68-canonical-metadata.js"
      },
      {
        "request": "./a68-compiler-inputs",
        "target": "a68-compiler-inputs.js"
      },
      {
        "request": "./a68-context-topology-envelope",
        "target": "a68-context-topology-envelope.js"
      },
      {
        "request": "./a68-launch-file-admission",
        "target": "a68-launch-file-admission.js"
      },
      {
        "request": "node:crypto",
        "target": "node:crypto"
      },
      {
        "request": "node:path",
        "target": "node:path"
      }
    ]
  },
  {
    "id": "a68-variation-recipe.js",
    "bytes": 10809,
    "sha256": "54197463ba1b3892c124f97ab1a0a0ab66a284d35db35313f97d182954ea8a71",
    "imports": [
      {
        "request": "./a68-background-recipe",
        "target": "a68-background-recipe.js"
      }
    ]
  },
  {
    "id": "a68-ward-controls-recipe.js",
    "bytes": 21807,
    "sha256": "542789fa8f6a5efc8f7869c3898ced26a70b6491ef1b36a4c69d574345bfa5e6",
    "imports": [
      {
        "request": "./a68-acceleration-recipe",
        "target": "a68-acceleration-recipe.js"
      },
      {
        "request": "./a68-background-recipe",
        "target": "a68-background-recipe.js"
      },
      {
        "request": "./a68-mixed-recipe",
        "target": "a68-mixed-recipe.js"
      },
      {
        "request": "./a68-original-action-recipe",
        "target": "a68-original-action-recipe.js"
      },
      {
        "request": "./a68-ward-recipe",
        "target": "a68-ward-recipe.js"
      }
    ]
  },
  {
    "id": "a68-ward-recipe.js",
    "bytes": 10114,
    "sha256": "9ffd45a66311e2e1b7e73c295662030d42b69638e7701639565d7a7b2a321aaf",
    "imports": [
      {
        "request": "./a68-original-action-recipe",
        "target": "a68-original-action-recipe.js"
      }
    ]
  }
]);
const PREFLIGHT_BUILTINS = Object.freeze(["node:child_process","node:crypto","node:fs","node:path","node:perf_hooks","node:stream/promises"]);
const ENTRY = "a68-preflight-entry.js";
function createPreflightModuleManifest(repositoryRoot) {
  if (typeof repositoryRoot !== "string" || repositoryRoot.length > 4096 ||
      !/^[\x20-\x7e]+$/.test(repositoryRoot) || !path.isAbsolute(repositoryRoot) ||
      path.resolve(repositoryRoot) !== repositoryRoot || repositoryRoot === path.parse(repositoryRoot).root)
    throw new Error("A68 preflight manifest: canonical bounded repository root required");
  return freeze({ entry: ENTRY, modules: PREFLIGHT_SOURCE_PINS.map(m => ({
    id: m.id, path: path.join(repositoryRoot, "scripts/incremental", m.id),
    bytes: m.bytes, sha256: m.sha256, imports: m.imports
  })), builtins: PREFLIGHT_BUILTINS });
}
module.exports = { PREFLIGHT_SOURCE_PINS, PREFLIGHT_BUILTINS, ENTRY, createPreflightModuleManifest };
