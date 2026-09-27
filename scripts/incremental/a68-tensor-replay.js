"use strict";

// Standalone retained coefficient replay, moved from the uncalled A68 verifier
// block without changing its arithmetic. No source geometry or phase runs here.
// Generic successful replay is NOT source authentication; the source adapter
// must bind its immutable plan and leaves independently.
// In particular, external geometry/source-scalar resolver values are not
// authenticated merely by recording their expression names in this receipt.
// The current point-export consumer menu has no such external dependencies;
// future germ/diagnostic receipt use needs its own authenticated bindings.
const assert = require("node:assert/strict");
const crypto = require("node:crypto");

const a61Equal = (a,b) => JSON.stringify(a) === JSON.stringify(b);
const a62Rat = value => { const parts=String(value).split("/"); let n=BigInt(parts[0]),d=parts.length===2?BigInt(parts[1]):1n; assert(parts.length<=2&&d!==0n,"A62 invalid rational record."); if(d<0n){n=-n;d=-d;} let a=n<0n?-n:n,b=d;while(b){const c=a%b;a=b;b=c;} return [n/a,d/a]; };
const a62Text = a => a[1]===1n?String(a[0]):String(a[0])+"/"+String(a[1]);
const a62Degree = mask => mask.toString(2).replaceAll("0","").length;
const a64Rat=(x,limit=128)=>typeof x==="string"&&x.length<=limit&&/^-?\d+(\/[1-9]\d*)?$/.test(x)&&a62Text(a62Rat(x))===x;
const a64Tensor=(x,degree,limit=128)=>Array.isArray(x)&&x.every((t,i)=>a61Equal(Object.keys(t),["form","blade","k0","k1","real","imaginary"])&&Number.isInteger(t.form)&&t.form>=0&&t.form<16384&&(degree===undefined||a62Degree(t.form)===degree)&&Number.isInteger(t.blade)&&t.blade>=0&&t.blade<16384&&t.k0===0&&t.k1===0&&a64Rat(t.real,limit)&&a64Rat(t.imaginary,limit)&&(t.real!=="0"||t.imaginary!=="0")&&(i===0||x[i-1].form<t.form||x[i-1].form===t.form&&x[i-1].blade<t.blade));

// No exported mint/factory: only the actual numerical verifier below can make
// a receipt. Reading/copying a receipt's contents never creates another brand.
const verifiedReceipts = new WeakMap();
function readVerifiedReplayReceipt(receipt) {
  const value = verifiedReceipts.get(receipt);
  assert(value, "A68 replay: private complete numerical replay receipt required"); return value;
}

// Snapshot the exact data that will be both evaluated and certified, before
// invoking caller callbacks. No getter/toJSON/ordinary Proxy get is consumed.
// Each blob has a conservative logical JSON-unit ceiling equal to graphBytes;
// this is an additional metadata quota, NOT a Reflect/allocator/RSS proof.
function snapshotReplayData(value, ceiling) {
  let remaining = ceiling; const ancestors = new WeakSet();
  const charge = count => { assert(Number.isSafeInteger(count) && count >= 0 && count <= remaining, "A68 replay metadata snapshot ceiling"); remaining -= count; };
  function visit(x, depth) {
    assert(depth <= 64, "A68 replay metadata depth64");
    if (x === null || typeof x === "boolean") { charge(1); return x; }
    if (typeof x === "string") { charge(x.length + 2); return x; }
    if (typeof x === "number") { assert(Number.isSafeInteger(x) && !Object.is(x, -0), "A68 replay metadata safe integer"); charge(1); return x; }
    assert(x && typeof x === "object" && !ancestors.has(x), "A68 replay acyclic own-data metadata");
    charge(2); ancestors.add(x); let result;
    if (Array.isArray(x)) {
      const n = Object.getOwnPropertyDescriptor(x, "length")?.value;
      assert(Number.isSafeInteger(n) && n >= 0 && n <= remaining, "A68 replay bounded dense array");
      assert(Reflect.ownKeys(x).length === n + 1, "A68 replay dense array keys"); result = [];
      for (let i = 0; i < n; i++) {
        const d = Object.getOwnPropertyDescriptor(x, String(i)); assert(d && Object.hasOwn(d, "value"), "A68 replay own data index"); result.push(visit(d.value, depth + 1));
      }
    } else {
      const names = Reflect.ownKeys(x); assert(names.length <= remaining, "A68 replay bounded field count"); result = {};
      for (const name of names) {
        assert(typeof name === "string", "A68 replay string field name"); charge(name.length + 1);
        const d = Object.getOwnPropertyDescriptor(x, name); assert(d && Object.hasOwn(d, "value"), "A68 replay own data field");
        Object.defineProperty(result, name, { value: visit(d.value, depth + 1), enumerable: true });
      }
    }
    ancestors.delete(x); return Object.freeze(result);
  }
  return visit(value, 0);
}
function replayOptions(value, names) {
  assert(value && typeof value === "object" && !Array.isArray(value) && a61Equal(Reflect.ownKeys(value), names), "A68 replay closed object options");
  return Object.fromEntries(names.map(name => {
    const d = Object.getOwnPropertyDescriptor(value, name); assert(d && Object.hasOwn(d, "value"), "A68 replay own data option"); return [name, d.value];
  }));
}
function prepareExports(config, tensorPlan, limits) {
  if (config === null) return null;
  config = snapshotReplayData(config, limits.graphBytes);
  const { contextId, requests, limits: caps } = replayOptions(config, ["contextId", "requests", "limits"]);
  replayOptions(caps, ["tensors", "records", "characters", "recordVisits"]);
  for (const cap of Object.values(caps)) assert(Number.isSafeInteger(cap) && cap > 0, "A68 replay positive export quota");
  const named = x => typeof x === "string" && x.length > 0 && /^[\x20-\x7e]+$/.test(x) && x.trim().length > 0;
  assert(named(contextId) && Array.isArray(requests) && requests.length > 0 && requests.length <= caps.tensors, "A68 replay named bounded exports");
  const nodes = new Map(), ids = new Set(); let namesCharacters = BigInt(contextId.length);
  for (const request of requests) {
    const { id, degree, node } = replayOptions(request, ["id", "degree", "node"]);
    assert(named(id) && !ids.has(id) && Number.isInteger(degree) && degree >= 0 && degree <= 14 &&
      Number.isInteger(node) && node >= 0 && node < tensorPlan.nodes.length && !nodes.has(node), "A68 replay distinct typed export IDs/nodes");
    assert(tensorPlan.nodes[node].degree === degree || tensorPlan.nodes[node].degree === -1, "A68 replay export source degree");
    ids.add(id); nodes.set(node, request); namesCharacters += BigInt(id.length + 64 + 128);
  }
  // Full per-node support bound, never observed cancellation/support. Distinct
  // nodes are NOT deduplicated by equal hashes. Capture shares the frozen
  // current wire (no second coefficient copy) and retains it after node release.
  const reservedRecords = BigInt(requests.length) * BigInt(limits.tensorRecords);
  const reservedCharacters = namesCharacters + 2n * BigInt(limits.rationalCharacters) * reservedRecords;
  assert(reservedRecords <= BigInt(caps.records) && reservedRecords <= BigInt(caps.recordVisits) && reservedCharacters <= BigInt(caps.characters), "A68 replay prospective export storage/work admission");
  return { contextId, requests, nodes, values: new Map(), records: 0,
    reservation: Object.freeze({ tensors: requests.length, records: Number(reservedRecords), characters: Number(reservedCharacters),
      sharedCurrentWire: true, additionalToReplayAndWireReserves: true, metadataCopiesMemoryProved: false, totalProcessMemoryProved: false }) };
}

const verifyMixedDag = (graph, expectedLeaves, expectedMarks, limits, resolveLeaf, validateNode, compareMark, scalarConfig, consumerConfig, exportConfig = null) => {
  const need=(ok,label)=>{assert(ok,"Phase627 DAG "+label);if(!ok)throw new Error("Phase627 malformed retained DAG: "+label);};
  const same=a61Equal,degree=a62Degree,hash=x=>crypto.createHash("sha256").update(x).digest("hex");
  const keys=(x,names)=>need(x!==null&&typeof x==="object"&&!Array.isArray(x)&&same(Object.keys(x),names),"closed object properties/order");
  const integer=(x,min,max)=>Number.isSafeInteger(x)&&x>=min&&x<=max;
  const validHash=x=>typeof x==="string"&&/^[0-9a-f]{64}$/.test(x);
  limits=replayOptions(limits,["nodes","marks","tensorRecords","rationalCharacters","tensorBytes","graphBytes","pairVisits","slotVisits","liveRecords"]);
  for(const name of ["nodes","marks","tensorRecords","rationalCharacters","tensorBytes","graphBytes","pairVisits","slotVisits","liveRecords"])
    need(integer(limits[name],1,Number.MAX_SAFE_INTEGER),"positive safe-integer resource limit: "+name);
  limits=Object.freeze({...limits});
  need(limits.rationalCharacters<=16384,"bounded rational parser/temporary exponent");
  need([resolveLeaf,validateNode,compareMark].every(x=>typeof x==="function"),"independent semantic/leaf/mark callbacks required");
  // Mandatory independent scalar recipe, never a schedule copied from the
  // observed graph. Requires are inside this UNCALLED function so lexical
  // wire-reader extraction does not load or activate numerical replay.
  const {validateMixedRecipe}=require("./a68-mixed-recipe");
  const {createScalarReplay}=require("./a68-scalar-replay");
  const {createAuditConsumerReplay}=require("./a68-audit-consumer-replay");
  scalarConfig=replayOptions(scalarConfig,["tensorPlan","namedRoots","geometry","scheduleLimits","arithmeticLimits","replayLimits","wireLimits","compareRoot"]);
  consumerConfig=replayOptions(consumerConfig,["checks","domainChecks","error","scheduleLimits","arithmeticLimits","replayLimits","resolveGeometryField","resolveSourceScalar","compareConsumer"]);
  for(const key of Object.keys(scalarConfig))if(key!=="compareRoot")scalarConfig[key]=snapshotReplayData(scalarConfig[key],limits.graphBytes);
  for(const key of Object.keys(consumerConfig))if(!["resolveGeometryField","resolveSourceScalar","compareConsumer"].includes(key))consumerConfig[key]=snapshotReplayData(consumerConfig[key],limits.graphBytes);
  graph=snapshotReplayData(graph,limits.graphBytes);
  expectedLeaves=snapshotReplayData(expectedLeaves,limits.graphBytes);expectedMarks=snapshotReplayData(expectedMarks,limits.graphBytes);
  const exportState=prepareExports(exportConfig,scalarConfig.tensorPlan,limits);
  const compareScalarRoot=scalarConfig.compareRoot;
  need(typeof compareScalarRoot==="function","independent scalar comparator");
  keys(scalarConfig.wireLimits,["liveWireRecords","sortReferences","liveWireCharacters","serializationRecordVisits","serializationCharacters"]);
  const wl=Object.freeze({...scalarConfig.wireLimits});
  for(const key of Object.keys(wl))need(integer(wl[key],1,Number.MAX_SAFE_INTEGER),"explicit scalar wire limit: "+key);
  // Logical live objects, NOT collector residency or VM heap/RSS: one
  // current frozen wire plus at most two older Pair operands, no wire cache.
  // One active serialization additionally has an N-reference sort array.
  // Each row holds <=2L coefficient characters; its JSON uses <=128+2L
  // characters including keys, masks, zero frequencies and punctuation.
  const wireR=BigInt(limits.tensorRecords),wireL=BigInt(limits.rationalCharacters);
  const wireJsonBound=n=>2n+BigInt(n)*(128n+2n*wireL);
  // The extra4L+4 covers one rational's numerator/denominator/concatenation
  // conversion strings in addition to the eventual retained row strings.
  const wireRecordReserve=3n*wireR,wireCharacterReserve=6n*wireL*wireR+wireJsonBound(limits.tensorRecords)+4n*wireL+4n;
  need(wireRecordReserve<=BigInt(wl.liveWireRecords)&&wireR<=BigInt(wl.sortReferences)&&wireCharacterReserve<=BigInt(wl.liveWireCharacters),"prospective three-array/sort/string wire admission");
  let serializationRecordVisits=0,serializationCharacters=0;
  const admitSerialization=n=>{
    const characters=wireJsonBound(n)+2n*wireL*BigInt(n);
    need(BigInt(n)<=BigInt(wl.serializationRecordVisits-serializationRecordVisits)&&characters<=BigInt(wl.serializationCharacters-serializationCharacters),"serialization work/characters BEFORE array/string creation");
    serializationRecordVisits+=n;serializationCharacters+=Number(characters);
  };
  keys(graph,["schemaVersion","leaves","nodes","marks"]);
  need(graph.schemaVersion==="phase627-typed-mixed-dag-v1"&&Array.isArray(graph.nodes)&&Array.isArray(graph.marks)&&Array.isArray(expectedLeaves)&&Array.isArray(expectedMarks),"schema/collections");
  need(same(graph.leaves,expectedLeaves)&&graph.nodes.length<=limits.nodes&&graph.marks.length<=limits.marks,"declared input closure/census caps");
  need(Buffer.byteLength(JSON.stringify(graph)+"\n")<=limits.graphBytes,"graph wire byte ceiling");
  validateMixedRecipe(graph,scalarConfig.tensorPlan);
  const freezeMetadata=x=>{if(x&&typeof x==="object"){Object.values(x).forEach(freezeMetadata);Object.freeze(x);}return x;};
  freezeMetadata(graph);
  const independentTensorPlan=scalarConfig.tensorPlan;
  let currentIndex=-1,currentWire=null;
  // Compile/freeze BOTH plans before any caller callback. Predicates and
  // norms add consumers beyond Pair/Top and must control combined releases.
  const consumerReplay=createAuditConsumerReplay({tensorPlan:independentTensorPlan,namedRoots:scalarConfig.namedRoots,geometry:scalarConfig.geometry,
    checks:consumerConfig.checks,domainChecks:consumerConfig.domainChecks,error:consumerConfig.error,
    scheduleLimits:scalarConfig.scheduleLimits,consumerScheduleLimits:consumerConfig.scheduleLimits,
    arithmeticLimits:consumerConfig.arithmeticLimits,replayLimits:consumerConfig.replayLimits,
    resolveTensor:id=>resolveScalarTensor(id),resolveGeometryField:consumerConfig.resolveGeometryField,
    resolveSourceScalar:consumerConfig.resolveSourceScalar,compareConsumer:consumerConfig.compareConsumer});
  const scalarReplay=createScalarReplay({tensorPlan:independentTensorPlan,namedRoots:scalarConfig.namedRoots,geometry:scalarConfig.geometry,
    scheduleLimits:scalarConfig.scheduleLimits,arithmeticLimits:scalarConfig.arithmeticLimits,replayLimits:scalarConfig.replayLimits,
    resolveTensor:id=>resolveScalarTensor(id),compareRoot:(name,value,descriptor)=>{
      need(compareScalarRoot(name,value,descriptor)===true,"independent scalar root comparison");return consumerReplay.captureRoot(name,value);
    }});
  const leafById=new Map(),seenLeaves=new Set(),markByNode=new Map();
  const frozenLeaves=independentTensorPlan.leaves;
  for(const leaf of frozenLeaves){keys(leaf,["id","degree","source","sha256"]);need(typeof leaf.id==="string"&&leaf.id.trim()&&typeof leaf.source==="string"&&leaf.source.trim()&&integer(leaf.degree,0,14)&&validHash(leaf.sha256)&&!leafById.has(leaf.id),"unique typed provenance leaf");leafById.set(leaf.id,leaf);}
  const actualMarkPlan=graph.marks.map(m=>({name:m.name,degree:m.degree,node:m.node,expanded:m.expanded}));
  need(same(actualMarkPlan,expectedMarks)&&new Set(actualMarkPlan.map(m=>m.name)).size===actualMarkPlan.length,"exact independently derived mark menu/order");
  for(let i=0;i<graph.nodes.length;i++){
    const n=graph.nodes[i];keys(n,["id","op","degree","inputs","parameters","records","bytes","sha256"]);
    need(n.id===i&&typeof n.op==="string"&&integer(n.degree,-1,14)&&Array.isArray(n.inputs)&&n.inputs.every(s=>integer(s,0,i-1))&&integer(n.records,0,limits.tensorRecords)&&integer(n.bytes,2,limits.tensorBytes)&&validHash(n.sha256),"strict typed DAG topology/metadata");
    need(validateNode(n,i)===true,"independent expression/parameter provenance rejected node "+i);
  }
  for(const m of graph.marks){keys(m,["name","degree","node","expanded","sha256"]);need(typeof m.name==="string"&&m.name.trim()&&integer(m.degree,0,14)&&integer(m.node,0,graph.nodes.length-1)&&typeof m.expanded==="boolean"&&validHash(m.sha256),"typed mark");if(!markByNode.has(m.node))markByNode.set(m.node,[]);markByNode.get(m.node).push(m);}
  // Internal rationals are independently reduced BigInts. Stored coefficients
  // and parameters have serialized height L. A complex product uses real
  // products of component height <=2L, then sums of height <=4L+1; multiplying
  // by a Clifford factor (at most2) and accumulating a stored coefficient
  // needs at most5L+3 component digits before reduction. The conservative
  // 6L+16 transient COMPONENT bound is therefore separate from the serialized
  // scalar bound L. Pullback stages pass through put(), so this bound does
  // not grow with form degree. Scalar temporaries, JSON/sorting buffers and
  // metadata are NOT tensor records or an RSS measurement.
  const transientDigits=6*limits.rationalCharacters+16;
  const magnitude=10n**BigInt(transientDigits),zero=[0n,1n],one=[1n,1n];
  const abs=n=>n<0n?-n:n;
  const rat=(n,d=1n)=>{need(d!==0n,"zero denominator");if(d<0n){n=-n;d=-d;}need(abs(n)<magnitude&&d<magnitude,"unreduced transient rational component height");let a=abs(n),b=d;while(b){const c=a%b;a=b;b=c;}return [n/a,d/a];};
  const readRat=s=>{need(a64Rat(s,limits.rationalCharacters),"canonical bounded rational");return a62Rat(s);};
  const add=(a,b)=>rat(a[0]*b[1]+b[0]*a[1],a[1]*b[1]),mul=(a,b)=>rat(a[0]*b[0],a[1]*b[1]);
  const neg=a=>[-a[0],a[1]],textRat=a=>a[1]===1n?String(a[0]):String(a[0])+"/"+String(a[1]);
  const complex=(ar,ai,br,bi)=>[add(mul(ar,br),neg(mul(ai,bi))),add(mul(ar,bi),mul(ai,br))];
  const live=new Map(),scratch=new Set();let records=0,maximumRecords=0,maximumTensors=0,pairVisits=0,slotVisits=0,checkedMarks=0;
  const peak=()=>{need(records<=limits.liveRecords,"replay-owned node plus scratch record ceiling");maximumRecords=Math.max(maximumRecords,records);maximumTensors=Math.max(maximumTensors,live.size+scratch.size);};
  const create=()=>{const t=new Map();scratch.add(t);peak();return t;};
  const drop=t=>{need(scratch.delete(t),"unknown/repeated scratch release");records-=t.size;};
  const chargeSlots=n=>{need(integer(n,0,Number.MAX_SAFE_INTEGER)&&slotVisits<=limits.slotVisits-n,"slot ceiling BEFORE primitive loop");slotVisits+=n;};
  const chargePairs=(a,b)=>{const n=a.size*b.size;need(integer(n,0,Number.MAX_SAFE_INTEGER)&&pairVisits<=limits.pairVisits-n,"raw Cartesian pair ceiling BEFORE loop");pairVisits+=n;};
  const put=(t,f,b,r,i)=>{
    need(scratch.has(t)&&integer(f,0,16383)&&integer(b,0,16383),"scratch-only typed coefficient insertion");if(r[0]===0n&&i[0]===0n)return;
    need(textRat(r).length<=limits.rationalCharacters&&textRat(i).length<=limits.rationalCharacters,"incoming canonical term height");
    const key=f*16384+b,old=t.get(key);if(old){r=add(old.r,r);i=add(old.i,i);}
    need(textRat(r).length<=limits.rationalCharacters&&textRat(i).length<=limits.rationalCharacters,"accumulated rational height");
    if(r[0]===0n&&i[0]===0n){if(t.delete(key))records--;return;}
    if(!old){need(t.size<limits.tensorRecords&&records<limits.liveRecords,"support/liveness BEFORE coefficient allocation");records++;}
    t.set(key,{form:f,blade:b,r,i});peak();
  };
  const bits=mask=>Array.from({length:14},(_,i)=>i).filter(i=>mask&(1<<i));
  const metric=mask=>degree(mask&0x3f80)%2?-1:1;
  const wedge=(a,b)=>{let sign=1;for(const j of bits(b))if(degree(a>>j+1)%2)sign=-sign;return sign;};
  // Append each right-hand Clifford letter, commute it through the ordered
  // left word, then remove any equal pair with its signature. No C# kernel.
  const word=(a,b)=>{let current=a,sign=1;for(const j of bits(b)){if(degree(current>>j+1)%2)sign=-sign;if((current&(1<<j))&&j>=7)sign=-sign;current^=1<<j;}return sign;};
  const scalar=(q,n)=>n===1?q:n===-1?neg(q):mul(q,[BigInt(n),1n]);
  const product=(a,b,kind,transpose=false)=>{
    chargePairs(a,b);const out=create();for(const x of a.values())for(const y of b.values()){
      if(transpose?(x.form&y.form)!==x.form:(x.form&y.form)!==0)continue;
      const form=transpose?(y.form^x.form):(x.form|y.form),ab=word(x.blade,y.blade),ba=word(y.blade,x.blade);
      const factor=transpose?metric(x.form)*wedge(x.form,form)*(kind==="C"?ba-ab:ab+ba):wedge(x.form,y.form)*(kind==="W"?ab:kind==="C"?ab-ba:ab+ba);
      if(!factor)continue;let [r,i]=complex(x.r,x.i,y.r,y.i);r=scalar(r,factor);i=scalar(i,factor);if(kind==="A")[r,i]=[neg(i),r];put(out,form,x.blade^y.blade,r,i);
    }return out;
  };
  const matrix=p=>{keys(p,["matrix"]);need(Array.isArray(p.matrix)&&p.matrix.length<=196,"matrix shape");const m=Array.from({length:14},()=>Array.from({length:14},()=>zero));let previous=-1;for(const e of p.matrix){keys(e,["row","column","value"]);need(integer(e.row,0,13)&&integer(e.column,0,13)&&e.row*14+e.column>previous,"ordered sparse matrix");const q=readRat(e.value);need(q[0]!==0n,"explicit matrix zero");m[e.row][e.column]=q;previous=e.row*14+e.column;}return m;};
  const pullback=(a,m)=>{
    const out=create(),cache=new Map();for(const q of a.values()){
      let form=cache.get(q.form);if(!form){form=create();put(form,0,0,one,zero);for(const axis of bits(q.form)){const row=create();chargeSlots(14);for(let j=0;j<14;j++)if(m[axis][j][0]!==0n)put(row,1<<j,0,m[axis][j],zero);const next=product(form,row,"W");drop(form);drop(row);form=next;}cache.set(q.form,form);}
      chargeSlots(form.size);for(const t of form.values()){const [r,i]=complex(t.r,t.i,q.r,q.i);put(out,t.form,q.blade,r,i);}
    }for(const t of cache.values())drop(t);return out;
  };
  const serialize=(t,d)=>{
    admitSerialization(t.size);
    const result=Object.freeze([...t.values()].sort((a,b)=>a.form-b.form||a.blade-b.blade).map(q=>Object.freeze({form:q.form,blade:q.blade,k0:0,k1:0,real:textRat(q.r),imaginary:textRat(q.i)})));
    need(a64Tensor(result,d<0?undefined:d,limits.rationalCharacters)&&(d>=0||result.length===0),"full canonical result/type");const json=JSON.stringify(result);need(Buffer.byteLength(json)<=limits.tensorBytes,"tensor serialization ceiling");return {result,sha256:hash(json),bytes:Buffer.byteLength(json)};
  };
  const snapshotLeaf=(raw,d)=>{
    need(Array.isArray(raw),"independent leaf array");const count=raw.length;
    need(integer(count,0,limits.tensorRecords),"bounded independent leaf array before copy");
    need(count<=limits.liveRecords-records,"leaf liveness before canonical snapshot");
    admitSerialization(count);chargeSlots(count);
    need(Reflect.ownKeys(raw).length===count+1,"dense leaf array without extra keys");
    const fields=["form","blade","k0","k1","real","imaginary"],value=[];
    for(let index=0;index<count;index++){
      const slot=Object.getOwnPropertyDescriptor(raw,String(index));need(slot&&Object.hasOwn(slot,"value"),"leaf array own data indices");
      const row=slot.value;need(row!==null&&typeof row==="object"&&!Array.isArray(row)&&same(Reflect.ownKeys(row),fields),"closed leaf row fields");
      const values=fields.map(name=>{const property=Object.getOwnPropertyDescriptor(row,name);need(property&&Object.hasOwn(property,"value"),"leaf row own data fields");return property.value;});
      // Never hash the supplied object: its inherited toJSON may describe
      // different data. Hash and import THIS SAME immutable literal copy.
      value.push(Object.freeze({form:values[0],blade:values[1],k0:values[2],k1:values[3],real:values[4],imaginary:values[5]}));
    }
    Object.freeze(value);need(a64Tensor(value,d,limits.rationalCharacters),"independent canonical leaf domain");return value;
  };
  function resolveScalarTensor(id){
    need(live.has(id),"scalar requested released/missing tensor");
    if(id===currentIndex)return currentWire.result;
    // Older maps are converted only on demand. Pair owns at most two wire
    // operands; consumers copy values into separately admitted parsed caches.
    // Their parsed rational/record storage is NOT covered by the wire reserve.
    const wire=serialize(live.get(id),graph.nodes[id].degree);
    need(wire.sha256===graph.nodes[id].sha256&&wire.bytes===graph.nodes[id].bytes,"older scalar operand full content recheck");return wire.result;
  }
  scalarReplay.advance(-1);
  consumerReplay.advance(-1);
  for(let index=0;index<graph.nodes.length;index++){
    const n=graph.nodes[index],p=n.parameters,inputs=n.inputs.map(id=>{need(live.has(id),"released/missing input");return live.get(id);}),ds=n.inputs.map(id=>graph.nodes[id].degree);
    const arity=k=>need(inputs.length===k,"primitive arity"),unary=()=>{arity(1);return ds[0];};let out,d;
    switch(n.op){
      case "leaf":{arity(0);keys(p,["id"]);const leaf=leafById.get(p.id);need(leaf&&!seenLeaves.has(p.id),"undeclared/repeated leaf");seenLeaves.add(p.id);d=leaf.degree;const value=snapshotLeaf(resolveLeaf(leaf),d);need(hash(JSON.stringify(value))===leaf.sha256,"independent leaf content");out=create();chargeSlots(value.length);for(const q of value)put(out,q.form,q.blade,readRat(q.real),readRat(q.imaginary));break;}
      case "zero":arity(0);keys(p,[]);d=-1;out=create();break;
      case "unit":arity(0);keys(p,["form","blade","real","imaginary"]);need(integer(p.form,0,16383)&&integer(p.blade,0,16383),"unit masks");d=degree(p.form);out=create();put(out,p.form,p.blade,readRat(p.real),readRat(p.imaginary));break;
      case "sum":{keys(p,[]);need(inputs.length>0,"sum arity");const known=[...new Set(ds.filter(x=>x>=0))];need(known.length<=1,"sum type");d=known.length?known[0]:-1;chargeSlots(inputs.reduce((s,t)=>s+t.size,0));out=create();for(const t of inputs)for(const q of t.values())put(out,q.form,q.blade,q.r,q.i);break;}
      case "scale":{keys(p,["real","imaginary"]);d=unary();const r=readRat(p.real),i=readRat(p.imaginary);chargeSlots(inputs[0].size);out=create();for(const q of inputs[0].values()){const [rr,ii]=complex(q.r,q.i,r,i);put(out,q.form,q.blade,rr,ii);}break;}
      case "product":case "transpose":arity(2);keys(p,["kind"]);need(typeof p.kind==="string"&&p.kind.length===1&&(n.op==="product"?"WCA":"CA").includes(p.kind),"product kind");d=ds.some(x=>x<0)?-1:n.op==="product"?ds[0]+ds[1]:ds[1]-ds[0];if(d<0||d>14)d=-1;out=product(inputs[0],inputs[1],p.kind,n.op==="transpose");break;
      case "pullback":d=unary();out=pullback(inputs[0],matrix(p));break;
      case "motion":case "motionAdjoint":{d=unary();const m=matrix(p);chargeSlots(inputs[0].size*Math.max(d,0)*14);out=create();for(const q of inputs[0].values()){const axes=bits(q.form);for(let slot=0;slot<axes.length;slot++)for(let replacement=0;replacement<14;replacement++){const old=axes[slot],c=n.op==="motion"?m[old][replacement]:scalar(m[replacement][old],(old<7?1:-1)*(replacement<7?1:-1));if(c[0]===0n)continue;const changed=axes.map((a,i)=>i===slot?replacement:a);if(new Set(changed).size!==changed.length)continue;let sign=1;for(let i=0;i<changed.length;i++)for(let j=i+1;j<changed.length;j++)if(changed[i]>changed[j])sign=-sign;put(out,changed.reduce((s,a)=>s|(1<<a),0),q.blade,scalar(mul(q.r,c),sign),scalar(mul(q.i,c),sign));}}break;}
      case "spin":{arity(0);d=0;const m=matrix(p);chargeSlots(91);out=create();for(let a=0;a<14;a++)for(let b=a+1;b<14;b++)put(out,0,(1<<a)|(1<<b),mul(m[b][a],[a<7?-1n:1n,2n]),zero);break;}
      case "star":case "starAdjoint":case "raise":case "component":case "contract":{
        const inputDegree=unary();if(n.op==="starAdjoint"){keys(p,["degree"]);need(integer(p.degree,0,14)&&(inputDegree<0||inputDegree===14-p.degree),"adjoint Hodge input type");d=p.degree;}
        else if(n.op==="component"){keys(p,["form"]);need(integer(p.form,0,16383),"component mask");d=0;}
        else if(n.op==="contract"){keys(p,["axis"]);need(integer(p.axis,0,13),"contraction axis");d=inputDegree<=0?-1:inputDegree-1;}
        else{keys(p,[]);d=n.op==="star"?(inputDegree<0?-1:14-inputDegree):inputDegree;}
        chargeSlots(inputs[0].size);out=create();for(const q of inputs[0].values()){let form=q.form,sign=1;if(n.op==="star"||n.op==="starAdjoint"){sign=wedge(form,16383^form)*metric(form);form^=16383;if(n.op==="starAdjoint"&&p.degree*(14-p.degree)%2)sign=-sign;}else if(n.op==="raise")sign=metric(form);else if(n.op==="component"){if(form!==p.form)continue;form=0;}else{if(!(form&(1<<p.axis)))continue;sign=degree(form&((1<<p.axis)-1))%2?-1:1;form^=1<<p.axis;}put(out,form,q.blade,scalar(q.r,sign),scalar(q.i,sign));}break;
      }
      default:need(false,"unknown primitive: "+n.op);
    }
    need(d===n.degree&&out.size===n.records,"node result type/support");const wire=serialize(out,d);need(wire.sha256===n.sha256&&wire.bytes===n.bytes,"FULL independently reconstructed intermediate "+index);
    need(scratch.delete(out)&&scratch.size===0,"all primitive scratch released");live.set(index,out);peak();
    currentIndex=index;currentWire=wire;
    if(exportState?.nodes.has(index)){
      const request=exportState.nodes.get(index);
      need(n.degree===request.degree||n.degree===-1&&wire.result.length===0,"complete exported tensor type");
      exportState.values.set(index,Object.freeze({id:request.id,degree:request.degree,node:index,sha256:wire.sha256,tensor:wire.result}));
      exportState.records+=wire.result.length;
    }
    for(const m of markByNode.get(index)||[]){need((d<0||m.degree===d)&&m.sha256===wire.sha256,"mark content/type");need(compareMark(m,wire.result)===true,"independent full marked tensor/identity comparison");checkedMarks++;}
    // Run all same-step scalar roots first, then every exact audit consumer.
    // Ignore scalar-only releases: late equality/coefficient/error consumers
    // may still own these tensors. Combined buckets release ALL expired IDs.
    scalarReplay.advance(index);
    for(const id of consumerReplay.advance(index)){need(live.has(id),"complete audit-consumer tensor release");records-=live.get(id).size;live.delete(id);}
    currentWire=null;currentIndex=-1;
  }
  const scalarResult=scalarReplay.finish();
  const consumerResult=consumerReplay.finish();
  need(seenLeaves.size===frozenLeaves.length&&checkedMarks===graph.marks.length&&live.size===0&&scratch.size===0&&records===0,"complete replay census and release");
  let exportReceipt=null;
  if(exportState){
    need(exportState.values.size===exportState.requests.length,"complete requested numerical exports");
    const identity=Object.freeze({contextId:exportState.contextId,tensorPlan:independentTensorPlan,
      namedRoots:scalarConfig.namedRoots,geometry:scalarConfig.geometry,checks:consumerConfig.checks,domainChecks:consumerConfig.domainChecks,error:consumerConfig.error,
      requests:exportState.requests,exports:Object.freeze(exportState.requests.map(r=>exportState.values.get(r.node))),
      completion:Object.freeze({tensor:true,scalar:true,consumers:true,released:true}),reservation:exportState.reservation});
    exportReceipt=Object.freeze({schemaVersion:"phase627-complete-numerical-replay-exports-v1",contextId:exportState.contextId});
    verifiedReceipts.set(exportReceipt,identity);
  }
  return {nodes:graph.nodes.length,marks:checkedMarks,pairVisits,slotVisits,maximumReplayOwnedTensorRecords:maximumRecords,maximumReplayOwnedTensors:maximumTensors,
    scalarReplay:scalarResult,consumerReplay:consumerResult,exportReceipt,wireStorage:{reservedLogicalWireRecords:Number(wireRecordReserve),reservedSortReferences:limits.tensorRecords,reservedLogicalWireCharacters:Number(wireCharacterReserve),serializationRecordVisits,serializationCharacters,
      callbackRetentionIncluded:false,metadataCopiesMemoryProved:false,vmSortAndJsonImplementationMemoryProved:false,garbageCollectorResidencyProved:false,totalProcessMemoryProved:false}};
};

module.exports = { verifyMixedDag, readVerifiedReplayReceipt };
