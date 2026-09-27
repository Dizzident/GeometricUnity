"use strict";

// UNCALLED on GU geometry. Each side is authenticated by private object
// identity, not a copied hash or a caller-supplied successful boolean.
// This closes comparison with retained621; it does not alone authenticate
// every new627 recipe matrix, C# metadata field or polynomial leaf.
const { sourceGeometryIdentity } = require("./a68-source-geometry");
const { retainedGeometryIdentity } = require("./a68-retained-inputs");
const { GeometryComparison } = require("./a68-geometry-comparison");
const need = (ok, why) => { if (!ok) throw new Error("A68 geometry binding: " + why); };
const bindings = new WeakMap();
function bindRetainedGeometry(a, rebuilt, retained, limits) {
  const source = sourceGeometryIdentity(a, rebuilt), target = retainedGeometryIdentity(retained);
  need(source.kind === target.kind && source.point === target.point, "same source/retained identity");
  const compare = new GeometryComparison(a, limits);
  if (source.kind === "baseline") {
    const h0 = a.matrix(4, (i, j) => i === j ? a.number(i === 0 ? -1 : 1) : a.zero);
    compare.dense(h0, retained.h0); compare.dense(rebuilt.source.y, retained.fibreMetricY);
    compare.dense(rebuilt.frame.Frame, retained.frame);
    compare.tensor(rebuilt.spinReference, retained.spinReference, 1); compare.tensor(rebuilt.spinCurvature, retained.spinCurvature, 2);
    need(compare.snapshot().coordinateVisits === 228, "complete baseline geometry census");
  } else {
    need(source.metricBasis === target.metricBasis && source.jetIndex === target.jetIndex, "same fixed metric germ");
    const { germ, matrices, connectionVariation: delta } = rebuilt, k = germ.metricVariation;
    need(germ.order === retained.order && JSON.stringify(germ.multiindex) === JSON.stringify(retained.multiindex), "same divided-monomial germ");
    compare.dense(matrices.Motion, retained.frameMotion); compare.dense(germ.shear.Value, retained.shearVariation);
    compare.dense(k.Value, retained.metricVariation); compare.dense(rebuilt.inverseVariation, retained.inverseVariation);
    compare.matrices(k.D, retained.metricFirstJets); compare.blocks(k.DD, retained.metricSecondJets);
    compare.matrices(delta.Gamma, retained.connectionVariation); compare.blocks(delta.DGamma, retained.connectionDerivative);
    compare.blocks(delta.Curvature, retained.curvatureVariation, true);
    compare.tensor(rebuilt.spinConnectionVariation, retained.spinConnectionVariation, 1);
    compare.tensor(rebuilt.spinCurvatureVariation, retained.spinCurvatureVariation, 2);
    need(compare.snapshot().coordinateVisits === 121520, "complete all-coordinate geometry census");
  }
  const result = Object.freeze({ kind: source.kind, point: source.point, source: retained.source,
    comparison: compare.snapshot(), fullSourceRecipeBound: false, scientificExecutionAuthorized: false });
  bindings.set(result, Object.freeze({ a, rebuilt, retained })); return result;
}
function boundGeometryIdentity(a, binding) {
  const identity = bindings.get(binding); need(identity && identity.a === a, "complete verified geometry binding identity"); return identity;
}
module.exports = { bindRetainedGeometry, boundGeometryIdentity };
