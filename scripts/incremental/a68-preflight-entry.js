"use strict";

// Single entry for the reviewed METADATA preflight module set. Loading defines
// factories and fixed menus only; it does not spawn, evaluate source geometry,
// admit a profile or authorize science. Use exports from the private loader's
// receipt, not a second ambient copy. Initialization still needs admission.
const { runTemplatePreflightProcess, runConfiguredTemplatePreflightProcess, validatedCheckpointConfiguration } = require("./a68-template-process-host");
const { createTopologySourceAdmission } = require("./a68-topology-source-admission");
const { profileCommitment } = require("./a68-template-preflight-service");
module.exports = Object.freeze({ runTemplatePreflightProcess, runConfiguredTemplatePreflightProcess, validatedCheckpointConfiguration, createTopologySourceAdmission, profileCommitment });
