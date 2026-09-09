using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Gu.Core;
using Gu.Phase4.Spin;

const string Root="studies/phase590_source_clifford_tensor_controls_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ContractId="phase590-a48-source-clifford-tensor-v1";
const string Success="mixed-signature-clifford-tensor-controls-pass-source-choice-open";
const string FixtureJson="""
{
 "dimension":14,"positive":7,"negative":7,"spinorDimension":128,"signature":[1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1],
 "gammaConvention":"dirac-tensor-product-v1","basisIndex":"seven-bit tensor-product column, left factor most significant",
 "independentGammaRule":"flip bit6-floor(mu/2); trailing-Z parity; odd-mu Y phase i*(-1)^bit; multiply negative directions by i",
 "bladeMasks":"all integers0..16383, generators in increasing index order","bladeCount":16384,"rightGeneratorProducts":229376,
 "hMask":16256,"hFormula":"gamma7 gamma8 gamma9 gamma10 gamma11 gamma12 gamma13",
 "volumeMask":16383,"rawBuilderChiralityPhase":3,"normalizingDiagnosticPhase":1,
 "spinGenerator":"Sigma_ab=gamma_a gamma_b/2; exact tests use twice generator",
 "spinPairs":"all a<b in0..13","spinGeneratorCount":91,"mixedGeneratorCount":49,
 "tensorMenu":[{"id":"canonical-one","formGrade":1,"volumeCompanion":false,"extraPhase":0,"expectedNorm":-14,"cliffordGrade":1},
  {"id":"canonical-two","formGrade":2,"volumeCompanion":false,"extraPhase":0,"expectedNorm":91,"cliffordGrade":2},
  {"id":"volume-one","formGrade":1,"volumeCompanion":true,"extraPhase":0,"expectedNorm":14,"cliffordGrade":13},
  {"id":"volume-two","formGrade":2,"volumeCompanion":true,"extraPhase":1,"expectedNorm":-91,"cliffordGrade":12}],
 "formConvention":"theta^a is dual to vector generator gamma_a; infinitesimal covectors use minus transpose of metric-index vector action",
 "formPairing":"ordered increasing multi-index, product of signature entries; no extra factorial",
 "matrixPairing":"-ReTr(XY)/128","formalCoefficients":"four independent real formal coefficients; none assigned",
 "realAntiHermitianGrades":[1,2,5,6,9,10,13,14],"imaginaryAntiHermitianGrades":[0,3,4,7,8,11,12],
 "realAntiHermitianBladeCount":8128,"imaginaryAntiHermitianBladeCount":8256,
 "gradeCounts":[1,14,91,364,1001,2002,3003,3432,3003,2002,1001,364,91,14,1],
 "tensorInvarianceRows":364,"matrixCommutatorChecks":19110,"wrongDualRejectedPerTensor":91,"wrongMetricRejectedPerTensor":49,
 "wrongMetricDecoy":"only on mixed-signature pairs replace both metric entries by +1 in dual-form action; same-signature pairs retain correct law",
 "exactTolerance":0,"coreFileCount":726,"resource":{"estimatedCpuSeconds":20,"maximumEstimatedCpuSeconds":60,"estimatedPeakBytes":268435456,"maximumEstimatedPeakBytes":536870912}
}
""";
string[] flags=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected",
 "physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed",
 "samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","mixed-signature-matrix-control-failed","tensor-invariance-or-pairing-control-failed",Success];
var paths=new Dictionary<string,string>{["program"]=Root+"/Program.cs",["project"]=Root+"/Phase590SourceCliffordTensorControls.csproj",
 ["study"]=Root+"/STUDY.md",["core-source-manifest"]=Root+"/preregistration/core_source_manifest_v1.json",["build-props"]="Directory.Build.props",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
 ["phase587-summary"]="studies/phase587_exact_residual_factorization_audit_001/output/exact_residual_factorization_audit_summary.json",
 ["phase587-contract"]="studies/phase587_exact_residual_factorization_audit_001/preregistration/contract_v1.json",
 ["phase588-summary"]="studies/phase588_fixed_domain_action_force_consistency_001/output/fixed_domain_action_force_consistency_summary.json",
 ["phase588-contract"]="studies/phase588_fixed_domain_action_force_consistency_001/preregistration/contract_v1.json",
 ["phase589-summary"]="studies/phase589_section_density_joint_lift_audit_001/output/section_density_joint_lift_audit_summary.json",
 ["phase589-contract"]="studies/phase589_section_density_joint_lift_audit_001/preregistration/contract_v1.json"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var d=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=d.RootElement.Clone();
 contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==590
  &&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
  &&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()
  &&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0
  &&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))
  &&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x=>x.GetString()).SequenceEqual(precedence)
  &&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&flags.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(x=>new Binding(x.GetProperty("id").GetString()!,x.GetProperty("path").GetString()!,x.GetProperty("sha256").GetString()!)).ToArray();
 exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(x=>x.id).Distinct().Count()==paths.Count&&bindings.Select(x=>x.path).Distinct().Count()==paths.Count
  &&bindings.All(x=>paths.TryGetValue(x.id,out var p)&&p==x.path&&x.hashMatches);
 if(contractValid&&exactBindingsValid)
 {
  using var doc=JsonDocument.Parse(File.ReadAllBytes(paths["core-source-manifest"]));var m=doc.RootElement;string[] live=CorePaths();
  var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1
   &&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"
   &&live.Length==contract.GetProperty("fixtures").GetProperty("coreFileCount").GetInt32()
   &&entries.Select(x=>x.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(x=>Sha(x.GetProperty("path").GetString()!)==x.GetProperty("sha256").GetString())
   &&HashText(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();
 }
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException)
{Emit(precedence[0],new {knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}
if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new {knownAnswerPassed=false,controlsPassed=false});return;}
var fx=contract.GetProperty("fixtures");int dimension=fx.GetProperty("dimension").GetInt32(),size=fx.GetProperty("spinorDimension").GetInt32();
int[] signature=fx.GetProperty("signature").EnumerateArray().Select(x=>x.GetInt32()).ToArray();
var algebra=new Blades(signature);var identity=Mono.Identity(size);
var independent=Enumerable.Range(0,dimension).Select(mu=>Mono.Gamma(mu,signature[mu],size)).ToArray();
bool knownAnswerPassed=algebra.Sign(1,2)==1&&algebra.Sign(2,1)==-1&&algebra.Sign(1<<7,1<<7)==-1
 &&algebra.Sign(1,1)==1&&Mono.ProductMatches(independent[0],independent[0],identity,1)
 &&Mono.ProductMatches(independent[7],independent[7],identity,-1);
// Algebraic associativity of signed blade multiplication on every generator triple.
for(int a=0;a<dimension;a++)for(int b=0;b<dimension;b++)for(int c=0;c<dimension;c++)
 knownAnswerPassed&=algebra.Sign(1<<a,1<<b)*algebra.Sign((1<<a)^(1<<b),1<<c)
  ==algebra.Sign(1<<b,1<<c)*algebra.Sign(1<<a,(1<<b)^(1<<c));
if(!knownAnswerPassed){Emit(precedence[1],new {knownAnswerPassed,controlsPassed=false});return;}
bool upstreamValid=new[]{587,588,589}.All(n=>{using var d=JsonDocument.Parse(File.ReadAllBytes(paths[$"phase{n}-summary"]));return d.RootElement.GetProperty("auditPassed").GetBoolean()&&d.RootElement.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;});
string source=File.ReadAllText(paths["primary-source"]);bool sourceAnchorsPresent=new[]{"(8.5)","(8.6)","(8.7)","normed","unable to locate the notes"}.All(source.Contains);
if(!upstreamValid||!sourceAnchorsPresent){Emit(precedence[0],new {knownAnswerPassed,controlsPassed=false,upstreamValid,sourceAnchorsPresent});return;}

var cliffordSignature=new CliffordSignature{Positive=7,Negative=7};
var bundle=new GammaMatrixBuilder().Build(cliffordSignature,new GammaConventionSpec{ConventionId="dirac-tensor-product-v1",Signature=cliffordSignature,
 Representation="standard",SpinorDimension=size,HasChirality=true,AnticommutationTolerance=0},
 new ProvenanceMeta{CreatedAt=DateTimeOffset.UnixEpoch,CodeRevision="phase590-frozen-control",Branch=new BranchRef{BranchId="phase590-cl77",SchemaVersion="1.0.0"},Notes="Deterministic mixed-signature control; no source choice"});
var gammas=new Mono[dimension];bool matrixPassed=bundle.SpinorDimension==size&&bundle.GammaMatrices.Length==dimension;
int denseEntriesChecked=0;
for(int mu=0;mu<dimension;mu++)
{
 gammas[mu]=independent[mu]; // Replace only after validating every dense entry against the independent map.
 bool matches=independent[mu].MatchesDense(bundle.GammaMatrices[mu]);matrixPassed&=matches;denseEntriesChecked+=size*size;
 if(matches)gammas[mu]=Mono.FromDense(bundle.GammaMatrices[mu]);
}
if(!matrixPassed){Emit(precedence[2],new {knownAnswerPassed,controlsPassed=false,matrixPassed,denseEntriesChecked});return;}
int bladeCount=fx.GetProperty("bladeCount").GetInt32();var blades=new Mono[bladeCount];blades[0]=identity;
for(int mask=1;mask<bladeCount;mask++){int bit=BitOperations.Log2((uint)mask);blades[mask]=Mono.Product(blades[mask^(1<<bit)],gammas[bit]);}
int hMask=fx.GetProperty("hMask").GetInt32(),volumeMask=fx.GetProperty("volumeMask").GetInt32();var h=blades[hMask];var volume=blades[volumeMask];
bool hHermitian=Mono.Equal(h.Dagger(),h),hInvolution=Mono.ProductMatches(h,h,identity,1),hTraceZero=h.Trace()==GI.Zero;
bool gammaHAdjointsPassed=true,volumeAnticommutationPassed=true;
for(int mu=0;mu<dimension;mu++)
{
 gammaHAdjointsPassed&=Mono.Equal(Mono.Product(gammas[mu].Dagger(),h),Mono.Product(h,gammas[mu]).WithPhase(2));
 volumeAnticommutationPassed&=Mono.Equal(Mono.Product(volume,gammas[mu]),Mono.Product(gammas[mu],volume).WithPhase(2));
}
int[] gradeCounts=new int[15];int realAntiCount=0,imaginaryAntiCount=0,rightProducts=0;bool allBladeControlsPassed=true;
var gradeRows=new List<object>();
for(int mask=0;mask<bladeCount;mask++)
{
 int grade=BitOperations.PopCount((uint)mask);gradeCounts[grade]++;var b=blades[mask];int sign=((grade*(grade+1)/2)&1)==0?1:-1;
 bool adjoint=Mono.Equal(Mono.Product(b.Dagger(),h),Mono.Product(h,b).WithPhase(sign==1?0:2));
 bool tracePassed=mask==0?b.Trace()==new GI(size,0):b.Trace()==GI.Zero;
 bool squarePassed=Mono.ProductMatches(b,b,identity,algebra.Sign(mask,mask));
 var selected=sign==-1?b:b.WithPhase(1);bool selectedAnti=Mono.Equal(Mono.Product(selected.Dagger(),h),Mono.Product(h,selected).WithPhase(2));
 if(sign==-1)realAntiCount++;else imaginaryAntiCount++;
 allBladeControlsPassed&=adjoint&&tracePassed&&squarePassed&&selectedAnti;
 for(int mu=0;mu<dimension;mu++){allBladeControlsPassed&=Mono.ProductMatches(b,gammas[mu],blades[mask^(1<<mu)],algebra.Sign(mask,1<<mu));rightProducts++;}
}
for(int grade=0;grade<15;grade++)gradeRows.Add(new {grade,count=gradeCounts[grade],hAdjointSign=((grade*(grade+1)/2)&1)==0?1:-1,requiresImaginaryFactor=grade%4 is 0 or 3});
bool rawChiralityMatches=bundle.ChiralityMatrix is not null&&volume.WithPhase(fx.GetProperty("rawBuilderChiralityPhase").GetInt32()).MatchesDense(bundle.ChiralityMatrix);
var raw=rawChiralityMatches?Mono.FromDense(bundle.ChiralityMatrix!):identity;
var normalized=raw.WithPhase(fx.GetProperty("normalizingDiagnosticPhase").GetInt32());
bool rawSquareMinusIdentity=Mono.ProductMatches(raw,raw,identity,-1),volumeSquareIdentity=Mono.ProductMatches(volume,volume,identity,1);
bool normalizedSquareIdentity=Mono.ProductMatches(normalized,normalized,identity,1),normalizedEqualsVolume=Mono.Equal(normalized,volume);
bool gradeCountsPassed=gradeCounts.SequenceEqual(fx.GetProperty("gradeCounts").EnumerateArray().Select(x=>x.GetInt32()))
 &&realAntiCount==fx.GetProperty("realAntiHermitianBladeCount").GetInt32()&&imaginaryAntiCount==fx.GetProperty("imaginaryAntiHermitianBladeCount").GetInt32()
 &&rightProducts==fx.GetProperty("rightGeneratorProducts").GetInt32();
matrixPassed&=hHermitian&&hInvolution&&hTraceZero&&gammaHAdjointsPassed&&volumeAnticommutationPassed&&allBladeControlsPassed&&gradeCountsPassed
 &&rawChiralityMatches&&rawSquareMinusIdentity&&volumeSquareIdentity&&normalizedSquareIdentity&&normalizedEqualsVolume;

var tensorRows=new List<object>();var invariantRows=new List<object>();var tensorLists=new List<Term[]>();
bool tensorPassed=true;int matrixCommutatorChecks=0,spinCount=0;bool spinHAdjointsPassed=true,volumeSpinCommutationPassed=true;
for(int a=0;a<dimension;a++)for(int b=a+1;b<dimension;b++)
{
 int generator=(1<<a)|(1<<b);spinCount++;
 spinHAdjointsPassed&=Mono.Equal(Mono.Product(blades[generator].Dagger(),h),Mono.Product(h,blades[generator]).WithPhase(2));
 volumeSpinCommutationPassed&=Mono.Equal(Mono.Product(volume,blades[generator]),Mono.Product(blades[generator],volume));
}
foreach(var spec in fx.GetProperty("tensorMenu").EnumerateArray())
{
 string id=spec.GetProperty("id").GetString()!;int formGrade=spec.GetProperty("formGrade").GetInt32();
 bool companion=spec.GetProperty("volumeCompanion").GetBoolean();int extraPhase=spec.GetProperty("extraPhase").GetInt32();
 var terms=Enumerable.Range(1,bladeCount-1).Where(mask=>BitOperations.PopCount((uint)mask)==formGrade).Select(mask=>new Term(mask,
  companion?volumeMask^mask:mask,(extraPhase+(companion&&algebra.Sign(volumeMask,mask)==-1?2:0))&3)).ToArray();
 tensorLists.Add(terms);long norm=0;bool termGradesPassed=true,termAdjointsPassed=true,componentPairingPassed=true;
 foreach(var term in terms)
 {
  var m=blades[term.clifford].WithPhase(term.phase);termGradesPassed&=BitOperations.PopCount((uint)term.clifford)==spec.GetProperty("cliffordGrade").GetInt32();
  termAdjointsPassed&=Mono.Equal(Mono.Product(m.Dagger(),h),Mono.Product(h,m).WithPhase(2));
  GI trace=Mono.Product(m,m).Trace();componentPairingPassed&=trace.i==0&&trace.r%size==0;
  norm+=algebra.Metric(term.form)*(-trace.r/size);
 }
 int wrongDualRejected=0,wrongMetricRejected=0;long maximumInvarianceResidual=0;
 for(int a=0;a<dimension;a++)for(int b=a+1;b<dimension;b++)
 {
  int generator=(1<<a)|(1<<b);bool mixedSignature=signature[a]!=signature[b];
  long residual=TensorResidual(terms,a,b,false,false,algebra),wrongDual=TensorResidual(terms,a,b,true,false,algebra),wrongMetric=TensorResidual(terms,a,b,false,mixedSignature,algebra);
  maximumInvarianceResidual=System.Math.Max(maximumInvarianceResidual,residual);if(wrongDual>0)wrongDualRejected++;if(wrongMetric>0)wrongMetricRejected++;
  bool commutatorsPassed=true;
  foreach(var t in terms)
  {
   var actual=blades[t.clifford].WithPhase(t.phase);int coeff=algebra.Sign(generator,t.clifford)-algebra.Sign(t.clifford,generator);
   commutatorsPassed&=Mono.CommutatorMatches(blades[generator],actual,blades[generator^t.clifford].WithPhase(t.phase),coeff);matrixCommutatorChecks++;
  }
  tensorPassed&=residual==0&&commutatorsPassed;
  invariantRows.Add(new {tensor=id,a,b,signatureA=signature[a],signatureB=signature[b],mixedSignature,wrongMetricDecoyApplied=mixedSignature,residual,wrongDualResidual=wrongDual,wrongMetricResidual=wrongMetric,commutatorsPassed});
 }
 bool rowPassed=norm==spec.GetProperty("expectedNorm").GetInt64()&&termGradesPassed&&termAdjointsPassed&&componentPairingPassed&&maximumInvarianceResidual==0
  &&wrongDualRejected==fx.GetProperty("wrongDualRejectedPerTensor").GetInt32()&&wrongMetricRejected==fx.GetProperty("wrongMetricRejectedPerTensor").GetInt32();
 tensorPassed&=rowPassed;tensorRows.Add(new {id,formGrade,termCount=terms.Length,companion,formalCoefficientAssigned=false,norm,termGradesPassed,termAdjointsPassed,
  componentPairingPassed,maximumInvarianceResidual,wrongDualRejected,wrongMetricRejected,rowPassed});
}
var crossRows=new List<object>();bool independencePassed=true;
for(int degree=0;degree<2;degree++)
{
 var canonical=tensorLists[degree];var companion=tensorLists[degree+2];long cross=0;bool disjointGrades=true;
 foreach(var term in canonical)
 {
  var dual=companion.Single(t=>t.form==term.form);disjointGrades&=BitOperations.PopCount((uint)term.clifford)!=BitOperations.PopCount((uint)dual.clifford);
  var trace=Mono.Product(blades[term.clifford].WithPhase(term.phase),blades[dual.clifford].WithPhase(dual.phase)).Trace();
  independencePassed&=trace.i==0&&trace.r%size==0;cross+=algebra.Metric(term.form)*(-trace.r/size);
 }
 independencePassed&=cross==0&&disjointGrades;crossRows.Add(new {formGrade=degree+1,crossPairing=cross,disjointGrades,independentInvariantDirectionsAtLeast=2});
}
tensorPassed&=independencePassed&&spinHAdjointsPassed&&volumeSpinCommutationPassed&&spinCount==fx.GetProperty("spinGeneratorCount").GetInt32()
 &&invariantRows.Count==fx.GetProperty("tensorInvarianceRows").GetInt32()&&matrixCommutatorChecks==fx.GetProperty("matrixCommutatorChecks").GetInt32();
bool controlsPassed=knownAnswerPassed&&matrixPassed&&tensorPassed;
Emit(!matrixPassed?precedence[2]:!tensorPassed?precedence[3]:Success,new {knownAnswerPassed,controlsPassed,upstreamValid,sourceAnchorsPresent,
 matrices=new {matrixPassed,denseEntriesChecked,bladeCount,rightProducts,hHermitian,hInvolution,hTraceZero,gammaHAdjointsPassed,
  balancedHermitianSignature=new[]{64,64},allBladeControlsPassed,gradeCountsPassed,realAntiCount,imaginaryAntiCount,gradeRows,
  rawChiralityMatches,rawSquareMinusIdentity,volumeSquareIdentity,normalizedSquareIdentity,normalizedEqualsVolume,volumeAnticommutationPassed,
  coreChiralityChanged=false,normalizationIsDiagnosticOnly=true},
 tensors=new {tensorPassed,spinCount,spinHAdjointsPassed,volumeSpinCommutationPassed,matrixCommutatorChecks,tensorRows,invariantRows,crossRows,independencePassed,
  invariantDimensionLowerBounds=new[]{2,2},completeInvariantClassification=false,canonicalChoiceUnique=false,
  oneFormPlaneNorm="14*(-a^2+b^2)",twoFormPlaneNorm="91*(c^2-d^2)",formalRealCoefficientsSelected=false},
 nextRequirement="Evaluate the explicitly canonical source-inspired Hodge/bracket subfamily with all declared occurrence choices; retain volume-dual invariant directions and normalization/source-selection ambiguity. No physical bridge follows."});

void Emit(string verdict,object evidence)
{
 var result=new {schemaVersion=1,phase=590,phaseId="phase590-source-clifford-tensor-controls",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",
  contractValid,exactBindingsValid,coreSourceTreeValid,bindingCount=bindings.Length,bindings,verdictKind=verdict,terminalStatus=verdict,auditPassed=verdict==Success,
  evidence,deterministic=true,authorityFirewalls=flags.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");
 File.WriteAllText(Root+"/output/source_clifford_tensor_controls.json",json);File.WriteAllText(Root+"/output/source_clifford_tensor_controls_summary.json",json);
 Console.WriteLine($"Phase590 verdict: {verdict}");if(verdict!=Success)Environment.ExitCode=1;
}
static long TensorResidual(Term[] tensor,int a,int b,bool wrongDual,bool wrongMetric,Blades algebra)
{
 var residue=new Dictionary<(int form,int clifford),GI>();int generator=(1<<a)|(1<<b);
 void Add(int f,int c,GI value){residue.TryGetValue((f,c),out var old);residue[(f,c)]=old+value;}
 foreach(var t in tensor)
 {
  int coefficient=algebra.Sign(generator,t.clifford)-algebra.Sign(t.clifford,generator);Add(t.form,generator^t.clifford,GI.Unit(t.phase)*coefficient);
  // Dual coframe action: X_d^c=2(eta_bd delta_a^c-eta_ad delta_b^c), delta theta^c=-X_d^c theta^d.
  foreach(var (from,to,scalar) in new[]{(a,b,-2*(wrongMetric?1:algebra.signature[b])),(b,a,2*(wrongMetric?1:algebra.signature[a]))})
  {
   if((t.form&(1<<from))==0)continue;int rest=t.form^(1<<from);if((rest&(1<<to))!=0)continue;
   int parity=BitOperations.PopCount((uint)(t.form&((1<<from)-1)))+BitOperations.PopCount((uint)(rest&((1<<to)-1)));
   int sign=(parity&1)==0?1:-1;Add(rest|(1<<to),t.clifford,GI.Unit(t.phase)*(scalar*sign*(wrongDual?-1:1)));
  }
 }
 return residue.Values.Select(x=>System.Math.Max(System.Math.Abs(x.r),System.Math.Abs(x.i))).DefaultIfEmpty(0).Max();
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string HashText(string t)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(t))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(x=>x is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
sealed record Binding(string id,string path,string sha256){public bool hashMatches=>File.Exists(path)&&Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant()==sha256;}
sealed record Term(int form,int clifford,int phase);
readonly record struct GI(long r,long i)
{
 public static GI Zero=>new(0,0);public static GI Unit(int phase)=>(phase&3) switch{0=>new(1,0),1=>new(0,1),2=>new(-1,0),_=>new(0,-1)};
 public static GI operator +(GI a,GI b)=>new(checked(a.r+b.r),checked(a.i+b.i));
 public static GI operator -(GI a,GI b)=>new(checked(a.r-b.r),checked(a.i-b.i));
 public static GI operator *(GI a,long b)=>new(checked(a.r*b),checked(a.i*b));
}
sealed class Blades(int[] signs)
{
 public readonly int[] signature=signs;
 public int Sign(int a,int b){int parity=0;for(int i=0;i<signature.Length;i++)if((a&(1<<i))!=0){parity+=BitOperations.PopCount((uint)(b&((1<<i)-1)));if((b&(1<<i))!=0&&signature[i]<0)parity++;}return (parity&1)==0?1:-1;}
 public int Metric(int mask){int result=1;for(int i=0;i<signature.Length;i++)if((mask&(1<<i))!=0)result*=signature[i];return result;}
}
sealed class Mono(int[] permutation,int[] phases)
{
 public readonly int[] p=permutation;public readonly int[] q=phases;
 public static Mono Identity(int n)=>new(Enumerable.Range(0,n).ToArray(),new int[n]);
 public static Mono Gamma(int mu,int signature,int n){int bit=6-mu/2;var p=new int[n];var q=new int[n];for(int j=0;j<n;j++){p[j]=j^(1<<bit);int phase=2*BitOperations.PopCount((uint)(j&((1<<bit)-1)));
  if((mu&1)!=0)phase+=1+2*((j>>bit)&1);if(signature<0)phase++;q[j]=phase&3;}return new(p,q);}
 public static Mono Product(Mono a,Mono b){var p=new int[a.p.Length];var q=new int[p.Length];for(int j=0;j<p.Length;j++){p[j]=a.p[b.p[j]];q[j]=(b.q[j]+a.q[b.p[j]])&3;}return new(p,q);}
 public Mono WithPhase(int phase)=>new(p,q.Select(x=>(x+phase)&3).ToArray());
 public Mono Dagger(){var r=new int[p.Length];var s=new int[p.Length];for(int j=0;j<p.Length;j++){r[p[j]]=j;s[p[j]]=(-q[j])&3;}return new(r,s);}
 public GI Trace(){var t=GI.Zero;for(int j=0;j<p.Length;j++)if(p[j]==j)t+=GI.Unit(q[j]);return t;}
 public static bool Equal(Mono a,Mono b)=>a.p.SequenceEqual(b.p)&&a.q.SequenceEqual(b.q);
 public static bool ProductMatches(Mono a,Mono b,Mono expected,int sign){int phase=sign==1?0:2;for(int j=0;j<a.p.Length;j++)if(a.p[b.p[j]]!=expected.p[j]||((b.q[j]+a.q[b.p[j]])&3)!=((expected.q[j]+phase)&3))return false;return true;}
 public static bool CommutatorMatches(Mono a,Mono b,Mono expected,int coefficient)
 {
  for(int j=0;j<a.p.Length;j++){int ap=a.p[b.p[j]],bp=b.p[a.p[j]];if(ap!=bp)return false;
   GI value=GI.Unit(b.q[j]+a.q[b.p[j]])-GI.Unit(a.q[j]+b.q[a.p[j]]);
   if(coefficient!=0&&ap!=expected.p[j])return false;if(value!=GI.Unit(expected.q[j])*coefficient)return false;}return true;
 }
 public bool MatchesDense(Complex[,] matrix){if(matrix.GetLength(0)!=p.Length||matrix.GetLength(1)!=p.Length)return false;
  for(int row=0;row<p.Length;row++)for(int col=0;col<p.Length;col++){GI expected=p[col]==row?GI.Unit(q[col]):GI.Zero;var z=matrix[row,col];if(z.Real!=expected.r||z.Imaginary!=expected.i)return false;}return true;}
 public static Mono FromDense(Complex[,] m){int n=m.GetLength(0);var p=new int[n];var q=new int[n];for(int col=0;col<n;col++){int count=0;for(int row=0;row<n;row++)if(m[row,col]!=Complex.Zero){count++;p[col]=row;
   bool found=false;for(int phase=0;phase<4;phase++){var u=GI.Unit(phase);if(m[row,col].Real==u.r&&m[row,col].Imaginary==u.i){q[col]=phase;found=true;break;}}if(!found)throw new InvalidOperationException("Non-Gaussian-unit matrix entry");}
   if(count!=1)throw new InvalidOperationException("Matrix is not monomial");}if(p.Distinct().Count()!=n)throw new InvalidOperationException("Matrix permutation is not bijective");return new(p,q);}
}
