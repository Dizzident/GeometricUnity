"use strict";

// Complete C# sink geometry metadata comparison. No runtime equality flag is
// accepted as evidence: BOTH metric/connection derivations are checked against
// the independent source reconstruction. Still UNCALLED on scientific data.
const { boundGeometryIdentity } = require("./a68-geometry-binding");
const { GeometryComparison } = require("./a68-geometry-comparison");
const need = (ok, why) => { if (!ok) throw new Error("A68 geometry metadata: " + why); };
const shape = (x, names) => need(x && typeof x === "object" && !Array.isArray(x) &&
  JSON.stringify(Object.keys(x)) === JSON.stringify(names), "complete ordered metadata fields");
// Pure wire identity check, NOT a geometry/source certificate. The C# sink's
// JsonNamingPolicy.CamelCase applies even to inferred anonymous member names.
// Kept separately testable without manufacturing a private source identity.
function validateGeometryWireIdentity(kind, expected, observed) {
  if (kind === "baseline") {
    shape(observed, ["point", "frame", "inverseFrame", "metric", "connection", "frameLift", "framePartial", "omega"]);
    need((expected.point === 0 || expected.point === 1) && observed.point === expected.point, "recorded baseline point");
  } else {
    need(kind === "germ", "known geometry kind");
    need(Number.isInteger(expected.metricBasis) && expected.metricBasis >= 0 && expected.metricBasis < 10 &&
      Number.isInteger(expected.jetIndex) && expected.jetIndex >= 0 && expected.jetIndex < 35 &&
      Number.isInteger(expected.order) && expected.order >= 0 && expected.order <= 3 && Array.isArray(expected.multiindex) &&
      expected.multiindex.length === 4 && expected.multiindex.every(n => Number.isInteger(n) && n >= 0 && n <= 3) &&
      expected.multiindex.reduce((a, b) => a + b, 0) === expected.order, "bounded independent germ identity");
    shape(observed, ["metricBasis", "jetIndex", "multiindex", "order", "shear", "deltaMetric", "blockMetric", "deltaConnection", "palatini", "motion", "motionPartial", "motionCovariant", "deltaOmega", "deltaOmegaPartial", "deltaFrame"]);
    need(observed.metricBasis === expected.metricBasis && observed.jetIndex === expected.jetIndex && observed.order === expected.order &&
      JSON.stringify(observed.multiindex) === JSON.stringify(expected.multiindex), "recorded complete germ identity");
  }
}
function compareRecordedGeometry(a, binding, observed, limits) {
  const { rebuilt } = boundGeometryIdentity(a, binding), c = new GeometryComparison(a, limits);
  if (binding.kind === "baseline") {
    validateGeometryWireIdentity("baseline", { point: binding.point }, observed);
    c.dense(rebuilt.frame.Frame, observed.frame); c.dense(rebuilt.frame.InverseFrame, observed.inverseFrame);
    c.metricJet(rebuilt.source.metric, observed.metric); c.connection(rebuilt.connection, observed.connection);
    c.denseMatrices(rebuilt.frame.FrameLift, observed.frameLift); c.denseMatrices(rebuilt.frame.FramePartial, observed.framePartial);
    c.denseMatrices(rebuilt.frame.Omega, observed.omega);
    need(c.snapshot().coordinateVisits === 129556, "complete baseline metadata coordinate census");
  } else {
    const { germ, matrices, connectionVariation: delta } = rebuilt;
    validateGeometryWireIdentity("germ", germ, observed);
    c.metricJet(germ.shear, observed.shear);
    c.metricJet(germ.metricVariation, observed.deltaMetric); c.metricJet(germ.metricVariation, observed.blockMetric);
    c.connection(delta, observed.deltaConnection); c.connection(delta, observed.palatini);
    c.dense(matrices.Motion, observed.motion); c.dense(matrices.DeltaFrame, observed.deltaFrame);
    c.denseMatrices(matrices.MotionPartial, observed.motionPartial); c.denseMatrices(matrices.MotionCovariant, observed.motionCovariant);
    c.denseMatrices(matrices.DeltaOmega, observed.deltaOmega); c.denseBlocks(matrices.DeltaOmegaPartial, observed.deltaOmegaPartial);
    need(c.snapshot().coordinateVisits === 330260, "complete germ metadata coordinate census");
  }
  return c.snapshot();
}
module.exports = { compareRecordedGeometry, validateGeometryWireIdentity };
