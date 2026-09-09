using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

const string Root="studies/phase595_invariant_tensor_dimension_audit_001";
const string Prior="studies/phase590_source_clifford_tensor_controls_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath=Prior+"/preregistration/core_source_manifest_v1.json";
const string ContractId="phase595-a50-invariant-tensor-dimension-v1";
const string Success="invariant-tensor-dimensions-two-certified-source-choice-open";
const int Full=(1<<14)-1;
const string FixtureJson="""
{
 "dimension":14,"signature":[1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1],
 "complexification":"negative vector and gamma basis multiply by i; dual coframe multiply by -i; positive axes unchanged",
 "realForm":"u(H) tensor_R C = End_C(S); H=gamma7...gamma13; real kernel complexification preserves dimension",
 "formDegrees":[1,2],"cliffordMasks":"all integers0..16383, increasing order",
 "characters":"13 even coordinate flips (0,j), j=1..13; tensor sign is parity of (I xor J) intersect {0,j}",
 "missingCharacter":13,"quarterTurns":"13 adjacent pairs(p,p+1), p=0..12; e_p maps to e_(p+1), e_(p+1) maps to -e_p",
 "dualAction":"inverse-transpose signed permutation, computed separately; in the complex orthonormal basis it equals the vector action",
 "orbitConvention":"coefficient(target)=formSign*cliffordSign*coefficient(source); both directions added for graph traversal",
 "canonicalCoefficients":"J=I, coefficient1","complementCoefficients":"J=Icomplement, EuclideanBladeSign(full,I)",
 "complexRealCompanionPhases":[1,2],
 "degreeRows":[
  {"degree":1,"formMasks":14,"positions":229376,"survivors":28,"missingCharacterSurvivors":56,"rotationRows":364,"components":2,"orbitSize":14,"rank":26,"nonTreeRows":338,"wrongDualChangedTerms":2,"wrongComplementChangedTerms":2},
  {"degree":2,"formMasks":91,"positions":1490944,"survivors":182,"missingCharacterSurvivors":364,"rotationRows":2366,"components":2,"orbitSize":91,"rank":180,"nonTreeRows":2186,"wrongDualChangedTerms":24,"wrongComplementChangedTerms":24}
 ],
 "predictedCounts":{"formMasks":105,"positions":1720320,"characterEvaluations":22364160,"survivors":210,"missingCharacterSurvivors":420,
  "rotationRows":2730,"forestEdges":206,"nonTreeRows":2524,"components":4,"wrongDualRows":52,"wrongComplementRows":26,
  "rotationMaskKnownAnswers":212992,"complexificationRows":105},
 "wrongDualDecoy":"use inverse quarter-turn on covectors but correct forward adjoint action; every tensor/generator must fail with max coefficient defect2",
 "wrongComplementDecoy":"replace complement orientation coefficients by all+1; every degree/generator must fail with max coefficient defect2",
 "exactTolerance":0,"coreFileCount":726,
 "resource":{"estimatedCpuSeconds":10,"maximumEstimatedCpuSeconds":30,"estimatedPeakBytes":67108864,"maximumEstimatedPeakBytes":134217728}
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected",
 "physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed",
 "samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","character-census-control-failed","signed-orbit-control-failed","real-dimension-or-decoy-control-failed",Success];
var paths=new Dictionary<string,string>{["program"]=Root+"/Program.cs",["project"]=Root+"/Phase595InvariantTensorDimensionAudit.csproj",
 ["study"]=Root+"/STUDY.md",["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
 ["phase590-summary"]=Prior+"/output/source_clifford_tensor_controls_summary.json",["phase590-contract"]=Prior+"/preregistration/contract_v1.json",
 ["phase590-program"]=Prior+"/Program.cs",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var doc=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=doc.RootElement.Clone();
 contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==595
  &&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
  &&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()
  &&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0
  &&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))
  &&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x=>x.GetString()).SequenceEqual(precedence)
  &&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14
  &&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(x=>new Binding(x.GetProperty("id").GetString()!,x.GetProperty("path").GetString()!,x.GetProperty("sha256").GetString()!)).ToArray();
 exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(x=>x.id).Distinct().Count()==paths.Count&&bindings.Select(x=>x.path).Distinct().Count()==paths.Count
  &&bindings.All(x=>paths.TryGetValue(x.id,out var p)&&p==x.path&&x.hashMatches);
 if(contractValid&&exactBindingsValid)
 {
  using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();var entries=m.GetProperty("files").EnumerateArray().ToArray();
  coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1
   &&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"
   &&live.Length==contract.GetProperty("fixtures").GetProperty("coreFileCount").GetInt32()
   &&entries.Select(x=>x.GetProperty("path").GetString()).SequenceEqual(live)
   &&entries.All(x=>Sha(x.GetProperty("path").GetString()!)==x.GetProperty("sha256").GetString())
   &&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();
 }
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException)
{Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}
if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
using var priorDoc=JsonDocument.Parse(File.ReadAllBytes(paths["phase590-summary"]));var prior=priorDoc.RootElement;
bool upstreamValid=prior.GetProperty("auditPassed").GetBoolean()&&prior.GetProperty("contractSha256").GetString()==Sha(paths["phase590-contract"])
 &&prior.GetProperty("verdictKind").GetString()=="mixed-signature-clifford-tensor-controls-pass-source-choice-open"
 &&prior.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&prior.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()
 &&prior.GetProperty("coreSourceTreeValid").GetBoolean()&&prior.GetProperty("externalReviewPending").GetBoolean()
 &&prior.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&prior.GetProperty("authorityFirewalls").EnumerateObject().Count()==14
 &&firewalls.All(k=>prior.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
string source=File.ReadAllText(paths["primary-source"]);bool sourceAnchorsPresent=new[]{"(8.5)","(8.6)","(8.7)","normed"}.All(source.Contains);
if(!upstreamValid||!sourceAnchorsPresent){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstreamValid,sourceAnchorsPresent});return;}
var fx=contract.GetProperty("fixtures");var counts=fx.GetProperty("predictedCounts");

bool knownAnswerPassed=EuclideanSign(1,2)==1&&EuclideanSign(2,1)==-1&&EuclideanSign(Full,Full)==-1;
bool complexBasisPassed=true;var complexBasisRows=new List<object>();
for(int axis=0;axis<14;axis++)
{
 int vectorPhase=axis<7?0:1,dualPhase=axis<7?0:3;
 complexBasisPassed&=((vectorPhase+dualPhase)&3)==0&&(((2*vectorPhase)&3)==0?1:-1)*(axis<7?1:-1)==1;
}
int rotationMaskKnownAnswers=0;
for(int p=0;p<13;p++)for(int mask=0;mask<=Full;mask++)
{
 rotationMaskKnownAnswers++;var f=RotateForm(mask,p,false);var c=RotateClifford(mask,p);var two=RotateClifford(c.Mask,p);
 var three=RotateClifford(two.Mask,p);var four=RotateClifford(three.Mask,p);
 int squareSign=(Degree(mask&((1<<p)|(1<<(p+1))))&1)==0?1:-1;
 knownAnswerPassed&=f==c&&Degree(f.Mask)==Degree(mask)&&two.Mask==mask&&c.Sign*two.Sign==squareSign
  &&four.Mask==mask&&c.Sign*two.Sign*three.Sign*four.Sign==1;
}
knownAnswerPassed&=rotationMaskKnownAnswers==counts.GetProperty("rotationMaskKnownAnswers").GetInt32()&&complexBasisPassed;
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,complexBasisPassed,rotationMaskKnownAnswers});return;}

long positions=0,characterEvaluations=0;int totalForms=0,totalSurvivors=0,totalMissing=0;
int totalRotationRows=0,totalForestEdges=0,totalNonTreeRows=0,totalComponents=0,wrongDualRows=0,wrongComplementRows=0;
bool characterPassed=true,orbitPassed=true,decoysPassed=true,realDimensionPassed=true;
var degreeRows=new List<object>();var orbitRows=new List<object>();var decoyRows=new List<object>();
foreach(var spec in fx.GetProperty("degreeRows").EnumerateArray())
{
 int degree=spec.GetProperty("degree").GetInt32();var forms=Enumerable.Range(0,Full+1).Where(m=>Degree(m)==degree).ToArray();totalForms+=forms.Length;
 var survivors=new List<Node>();int missing=0;long localPositions=0;
 foreach(int form in forms)for(int blade=0;blade<=Full;blade++)
 {
  localPositions++;bool valid=true,omittedValid=true;int xor=form^blade;
  for(int j=1;j<14;j++)
  {
   characterEvaluations++;bool even=(Degree(xor&(1|(1<<j)))&1)==0;valid&=even;if(j!=13)omittedValid&=even;
  }
  if(valid)survivors.Add(new(form,blade));if(omittedValid)missing++;
  characterPassed&=valid==(blade==form||blade==(Full^form));
 }
 positions+=localPositions;totalSurvivors+=survivors.Count;totalMissing+=missing;
 characterPassed&=forms.Length==spec.GetProperty("formMasks").GetInt32()&&localPositions==spec.GetProperty("positions").GetInt32()
  &&survivors.Count==spec.GetProperty("survivors").GetInt32()&&missing==spec.GetProperty("missingCharacterSurvivors").GetInt32();
 foreach(int form in forms)
 {
  int negative=Degree(form&0x3f80),complement=Full^form,extra=degree==1?0:1;
  int etaSign=EuclideanSign(Full,form)*((negative&1)==0?1:-1);
  int transformedPhase=Mod4(extra+negative-Degree(complement&0x3f80)+(etaSign==1?0:2));
  int expectedPhase=Mod4((degree==1?1:2)+(EuclideanSign(Full,form)==1?0:2));
  bool passed=transformedPhase==expectedPhase;complexBasisPassed&=passed;
  complexBasisRows.Add(new{degree,form,complement,transformedPhase,expectedPhase,passed});
 }
 var index=survivors.Select((n,i)=>(n,i)).ToDictionary(x=>x.n,x=>x.i);var edges=new List<Edge>();
 for(int i=0;i<survivors.Count;i++)for(int p=0;p<13;p++)
 {
  var n=survivors[i];var f=RotateForm(n.Form,p,false);var c=RotateClifford(n.Blade,p);var target=new Node(f.Mask,c.Mask);
  if(!index.TryGetValue(target,out int destination)){orbitPassed=false;continue;}
  edges.Add(new(i,destination,f.Sign*c.Sign,p));
 }
 var graph=Enumerable.Range(0,survivors.Count).Select(_=>new List<(int Target,int Sign,int Edge)>()).ToArray();
 for(int i=0;i<edges.Count;i++){var e=edges[i];graph[e.From].Add((e.To,e.Sign,i));graph[e.To].Add((e.From,e.Sign,i));}
 var values=new int[survivors.Count];var forest=new HashSet<int>();var components=new List<int[]>();
 for(int root=0;root<survivors.Count;root++)if(values[root]==0)
 {
  var members=new List<int>();var queue=new Queue<int>();values[root]=1;queue.Enqueue(root);
  while(queue.TryDequeue(out int current))
  {
   members.Add(current);foreach(var arc in graph[current])
   {
    int value=values[current]*arc.Sign;if(values[arc.Target]==0){values[arc.Target]=value;forest.Add(arc.Edge);queue.Enqueue(arc.Target);}
    else orbitPassed&=values[arc.Target]==value;
   }
  }
  components.Add(members.Order().ToArray());
 }
 bool allEdgesPassed=edges.All(e=>values[e.To]==e.Sign*values[e.From]);orbitPassed&=allEdgesPassed;
 foreach(var component in components)
 {
  int root=component[0];bool complement=survivors[root].Blade!=survivors[root].Form;
  int rootExpected=complement?EuclideanSign(Full,survivors[root].Form):1;
  bool consistent=component.All(i=>(survivors[i].Blade!=survivors[i].Form)==complement
   &&values[i]==rootExpected*(complement?EuclideanSign(Full,survivors[i].Form):1));
  orbitPassed&=consistent&&component.Length==spec.GetProperty("orbitSize").GetInt32();
  orbitRows.Add(new{degree,complement,size=component.Length,rootForm=survivors[root].Form,consistent,
   coefficients=component.Select(i=>new{form=survivors[i].Form,blade=survivors[i].Blade,coefficient=values[i]}).ToArray()});
 }
 int rank=survivors.Count-components.Count,nonTree=edges.Count-forest.Count;
 orbitPassed&=components.Count==spec.GetProperty("components").GetInt32()&&rank==spec.GetProperty("rank").GetInt32()
  &&forest.Count==rank&&edges.Count==spec.GetProperty("rotationRows").GetInt32()&&nonTree==spec.GetProperty("nonTreeRows").GetInt32();
 totalRotationRows+=edges.Count;totalForestEdges+=forest.Count;totalNonTreeRows+=nonTree;totalComponents+=components.Count;
 foreach(bool complement in new[]{false,true})for(int p=0;p<13;p++)
 {
  var tensor=forms.ToDictionary(i=>new Node(i,complement?Full^i:i),i=>complement?EuclideanSign(Full,i):1);
  var correct=Transform(tensor,p,false);var wrong=Transform(tensor,p,true);
  var defect=Difference(tensor,wrong);bool passed=SameTensor(tensor,correct)&&defect.Maximum==2&&defect.Changed==spec.GetProperty("wrongDualChangedTerms").GetInt32();
  wrongDualRows++;decoysPassed&=passed;decoyRows.Add(new{degree,complement,generator=p,kind="inverse-instead-of-dual",passed,defect.Maximum,defect.Changed});
  if(complement)
  {
   var wrongCoefficients=forms.ToDictionary(i=>new Node(i,Full^i),_=>1);var signDefect=Difference(wrongCoefficients,Transform(wrongCoefficients,p,false));
   bool signPassed=signDefect.Maximum==2&&signDefect.Changed==spec.GetProperty("wrongComplementChangedTerms").GetInt32();
   wrongComplementRows++;decoysPassed&=signPassed;decoyRows.Add(new{degree,complement,generator=p,kind="missing-complement-orientation",passed=signPassed,signDefect.Maximum,signDefect.Changed});
  }
 }
 int lowerBound=prior.GetProperty("evidence").GetProperty("tensors").GetProperty("invariantDimensionLowerBounds")[degree-1].GetInt32();
 bool realLowerBound=lowerBound==2&&prior.GetProperty("evidence").GetProperty("tensors").GetProperty("independencePassed").GetBoolean()
  &&prior.GetProperty("evidence").GetProperty("tensors").GetProperty("tensorPassed").GetBoolean();
 realDimensionPassed&=realLowerBound&&components.Count==lowerBound;
 degreeRows.Add(new{degree,formMaskCount=forms.Length,positions=localPositions,survivorCount=survivors.Count,missingCharacterSurvivors=missing,
  rotationRows=edges.Count,components=components.Count,rank,forestEdges=forest.Count,nonTreeRows=nonTree,allEdgesPassed,realLowerBound,realInvariantDimension=components.Count});
}
characterPassed&=positions==counts.GetProperty("positions").GetInt64()&&characterEvaluations==counts.GetProperty("characterEvaluations").GetInt64()
 &&totalForms==counts.GetProperty("formMasks").GetInt32()&&totalSurvivors==counts.GetProperty("survivors").GetInt32()&&totalMissing==counts.GetProperty("missingCharacterSurvivors").GetInt32();
orbitPassed&=totalRotationRows==counts.GetProperty("rotationRows").GetInt32()&&totalForestEdges==counts.GetProperty("forestEdges").GetInt32()
 &&totalNonTreeRows==counts.GetProperty("nonTreeRows").GetInt32()&&totalComponents==counts.GetProperty("components").GetInt32();
decoysPassed&=wrongDualRows==counts.GetProperty("wrongDualRows").GetInt32()&&wrongComplementRows==counts.GetProperty("wrongComplementRows").GetInt32();
realDimensionPassed&=complexBasisPassed&&complexBasisRows.Count==counts.GetProperty("complexificationRows").GetInt32();
bool controlsPassed=knownAnswerPassed&&characterPassed&&orbitPassed&&decoysPassed&&realDimensionPassed;
Emit(!characterPassed?precedence[2]:!orbitPassed?precedence[3]:!decoysPassed||!realDimensionPassed?precedence[4]:Success,new{
 knownAnswerPassed,controlsPassed,upstreamValid,sourceAnchorsPresent,complexBasisPassed,rotationMaskKnownAnswers,
 positions,characterEvaluations,totalForms,totalSurvivors,totalMissing,totalRotationRows,totalForestEdges,totalNonTreeRows,totalComponents,
 characterPassed,orbitPassed,decoysPassed,realDimensionPassed,wrongDualRows,wrongComplementRows,degreeRows,orbitRows,decoyRows,complexBasisRows,
 realInvariantDimensions=new[]{2,2},finiteSubgroupUsedForUpperBoundOnly=true,realLowerBoundFromFullSpinInvariants=true,
 complexQuarterTurnsClaimedAsMixedRealRotations=false,sourceNormalizationSelected=false,sourceOperatorSelected=false});

static int Degree(int mask)=>BitOperations.PopCount((uint)mask);
static int Mod4(int n)=>(n%4+4)%4;
static int EuclideanSign(int a,int b){int count=0;for(int i=0;i<14;i++)if((a&(1<<i))!=0)count+=Degree(b&((1<<i)-1));return (count&1)==0?1:-1;}
static (int Mask,int Sign) RotateClifford(int mask,int p)
{
 int result=0,sign=1;for(int i=0;i<14;i++)if((mask&(1<<i))!=0)
 {int target=i==p?p+1:i==p+1?p:i;if(i==p+1)sign=-sign;sign*=EuclideanSign(result,1<<target);result^=1<<target;}return(result,sign);
}
static (int Mask,int Sign) RotateForm(int mask,int p,bool wrongInverse)
{
 // Construct R^-1 first; its row indexed by the input covector determines the dual image.
 int[] inverseIndex=Enumerable.Range(0,14).ToArray(),inverseSign=Enumerable.Repeat(1,14).ToArray();
 inverseIndex[p+1]=p;inverseSign[p+1]=1;inverseIndex[p]=p+1;inverseSign[p]=-1;
 var word=new List<int>();int sign=1,result=0;
 for(int i=0;i<14;i++)if((mask&(1<<i))!=0)
 {
  int target=Array.IndexOf(inverseIndex,i),factor=inverseSign[target];
  if(wrongInverse){target=inverseIndex[i];factor=inverseSign[i];}
  sign*=factor;word.Add(target);result|=1<<target;
 }
 for(int i=0;i<word.Count;i++)for(int j=i+1;j<word.Count;j++)if(word[i]>word[j])sign=-sign;
 return(result,sign);
}
static Dictionary<Node,int> Transform(Dictionary<Node,int> tensor,int p,bool wrongDual)
{
 var r=new Dictionary<Node,int>();foreach(var x in tensor){var f=RotateForm(x.Key.Form,p,wrongDual);var c=RotateClifford(x.Key.Blade,p);r[new(f.Mask,c.Mask)]=x.Value*f.Sign*c.Sign;}return r;
}
static bool SameTensor(Dictionary<Node,int> a,Dictionary<Node,int> b)=>a.Count==b.Count&&a.All(x=>b.TryGetValue(x.Key,out int value)&&value==x.Value);
static (int Maximum,int Changed) Difference(Dictionary<Node,int> a,Dictionary<Node,int> b)
{int maximum=0,changed=0;foreach(var key in a.Keys.Union(b.Keys)){int v=System.Math.Abs(a.GetValueOrDefault(key)-b.GetValueOrDefault(key));if(v!=0)changed++;maximum=System.Math.Max(maximum,v);}return(maximum,changed);}
void Emit(string verdict,object evidence)
{
 var result=new{schemaVersion=1,phase=595,phaseId="phase595-invariant-tensor-dimension-audit",contractId=ContractId,
  contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,
  bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(x=>x.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(x=>x.path).Distinct().Count(),bindings,
  verdictKind=verdict,terminalStatus=verdict,auditPassed=verdict==Success,evidence,deterministic=true,
  authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;
 Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/invariant_tensor_dimension_audit.json",json);
 File.WriteAllText(Root+"/output/invariant_tensor_dimension_audit_summary.json",json);
 Console.WriteLine($"Phase595 verdict: {verdict}");if(verdict!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string t)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(t))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(x=>x is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
sealed record Node(int Form,int Blade);
sealed record Edge(int From,int To,int Sign,int Generator);
sealed record Binding(string id,string path,string sha256){public bool hashMatches=>File.Exists(path)&&Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant()==sha256;}
