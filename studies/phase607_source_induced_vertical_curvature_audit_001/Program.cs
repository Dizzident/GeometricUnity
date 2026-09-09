using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Geometry;

const string Root="studies/phase607_source_induced_vertical_curvature_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase607-a57-source-induced-vertical-curvature-v1";
const string Success="source-induced-vertical-curvature-controls-pass-flat-reference-not-induced";
const string FixtureJson="""
{
 "baseDimension":4,"verticalDimension":10,"ambientDimension":14,
 "basis":["E00","E11","E22","E33","E01+E10","E02+E20","E03+E30","E12+E21","E13+E31","E23+E32"],
 "points":[[-1,1,1,1],[-1,4,9,16]],"transportL":[1,2,3,4],"alpha":"1","betas":["0","-1/2"],
 "generalFamily":"g_y(A,B)=alpha Tr(p A p B)+beta Tr(p A)Tr(p B),p=y^-1,alpha!=0,alpha+4beta!=0",
 "connection":"Gamma_y(A,B)=-(A p B+B p A)/2",
 "curvatureConvention":"R(A,B)C=nabla_A nabla_B C-nabla_B nabla_A C for commuting coordinate fields",
 "curvature":"R_y(A,B)C=-y[[p A,p B],p C]/4",
 "independentRoute":"inverse-metric first/second jets; Koszul linear solve; differentiated Koszul solve including -(Dg)Gamma; composition of recovered coefficients",
 "determinant":"64 alpha^9(alpha+4beta)/(det y)^5",
 "expectedGramDeterminants":[["-64","64"],["-1/990677827584","1/990677827584"]],"expectedInertias":[[7,3,0],[6,4,0]],
 "singularControls":[{"alpha":"0","beta":"1","inertia":[1,0,9]},{"alpha":"1","beta":"-1/4","inertia":[6,3,1]}],
 "witness":"A=diag(0,1,-1,0),B=E12+E21; use L A L^T,L B L^T at second point",
 "witnessPrediction":{"RABB":"-A","normA":"2","normB":"2","cross":"0","lowered":"-2","sectional":"-1/2"},
 "flatControls":["commuting diagonal A and C=diag(0,1,1,-2)","central trace direction y"],
 "flatConnectionDecoy":"Gamma=0 gives D_D g(D,D)=12 at eta (transport at second point), not metric compatibility",
 "omittedDerivativeDecoy":"Gamma(A,Gamma(B,B))-Gamma(B,Gamma(A,B))=+A instead of -A",
 "ambientControls":{"horizontalBlocks":["y","y^-1"],"crossBlock":"zero identically","baseDerivatives":"zero identically","VVChristoffel":"vertical recovered Gamma and all four horizontal components zero","horizontalVerticalDerivative":"nonzero; no product metric assertion","sourceHorizontalIdentificationSelected":false},
 "nonzeroPerContext":{"connectionOutputs":58,"connectionMatrixEntries":112,"connectionCoordinates":64,"curvatureOutputs":336,"curvatureMatrixEntries":672,"curvatureCoordinates":384,"loweredCurvatureEntries":384},
 "expectedCounts":{"arithmeticControls":8,"contexts":4,"inverseControls":8,"gramEntries":400,"metricFirstJets":4000,"metricSecondJets":40000,"metricSecondJetSymmetry":40000,"connectionOutputs":400,"koszulPairings":4000,"torsionControls":400,"metricCompatibility":4000,"connectionDerivativeOutputs":4000,"curvatureOutputs":4000,"firstPairSkew":4000,"bianchi":4000,"loweredEntries":40000,"lastPairSkew":40000,"pairExchange":40000,"gramDeterminants":4,"gramInertias":4,"singularMetricControls":4,"congruenceMetric":200,"congruenceConnection":200,"congruenceCurvature":2000,"witnesses":4,"flatControls":8,"flatConnectionDecoys":4,"omittedDerivativeDecoys":4,"ambientVVOutputs":800,"nonproductControls":8},
 "expectedNonzeroTotals":{"connectionOutputs":232,"connectionMatrixEntries":448,"connectionCoordinates":256,"curvatureOutputs":1344,"curvatureMatrixEntries":2688,"curvatureCoordinates":1536,"loweredCurvatureEntries":1536},
 "exactTolerance":0,"coreFileCount":726,
 "resources":{"estimatedCpuSeconds":20,"maximumEstimatedCpuSeconds":120,"estimatedPeakBytes":134217728,"maximumEstimatedPeakBytes":536870912,"maximumTrackedMatrixProducts":100000000,"maximumMatrixDimension":14,"largestJetEntries":10000},
 "sourceAnchors":["3.7-3.10 metric-fiber vertical metric and trace/sign freedom","3.4 downstairs LC split induces upstairs LC","9.1 domain G x MET(X), not arbitrary MET(Y)","9.4 FB induced spin curvature"],
 "scope":{"sourceNormalizationSelected":false,"horizontalConventionSelected":false,"shiabContractionNonzeroInferred":false,"inducedStationaryBackgroundRejected":false,"fullMetricEulerComputed":false,"historicalFlatControlsRewritten":false}
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","metric-connection-control-failed","full-curvature-control-failed","induced-ambient-control-failed","nonzero-flat-decoy-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]=Root+"/Program.cs",["project"]=Root+"/Phase607SourceInducedVerticalCurvatureAudit.csproj",["study"]=Root+"/STUDY.md",["geometry-helper"]=Root+"/VerticalGeometry.cs",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",["phase600-contract"]=P600+"/preregistration/contract_v1.json",["phase600-program"]=P600+"/Program.cs",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
 ["core-flat-lc"]="src/Gu.Phase4.Dirac/CpuSpinConnectionBuilder.cs",["core-toy-geometry"]="src/Gu.Geometry/ToyGeometryFactory.cs",["core-fiber-metadata"]="src/Gu.Geometry/FiberBundleMesh.cs",["core-classifier"]="src/Gu.Phase5.Reporting/GeometryEvidenceClassifier.cs",["core-flat-biconnection"]="src/Gu.ReferenceCpu/BiConnectionBuilder.cs",
 ["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==607&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 using var ud=JsonDocument.Parse(File.ReadAllBytes(paths["phase600-summary"]));var q=ud.RootElement;bool upstream=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths["phase600-contract"])&&q.GetProperty("verdictKind").GetString()=="full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
 string source=File.ReadAllText(paths["primary-source"]);bool anchors=new[]{"(3.7)","(3.8)","(3.10)","(9.1)","(9.4)","(9.11)"}.All(source.Contains)&&File.ReadAllText(paths["core-flat-lc"]).Contains("FlatLeviCivitaAssumption")&&File.ReadAllText(paths["core-toy-geometry"]).Contains("CreateStructuredFiberBundle4D")&&File.ReadAllText(paths["core-fiber-metadata"]).Contains("EmbeddingDimension")&&File.ReadAllText(paths["core-classifier"]).Contains("DraftAligned")&&File.ReadAllText(paths["core-flat-biconnection"]).Contains("WithFlatA0");
 if(!upstream||!anchors){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream,anchors});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}

var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);var nonzeroExpected=fx.GetProperty("expectedNonzeroTotals");var nonzero=nonzeroExpected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);
bool knownAnswerPassed=true,connectionPassed=true,curvaturePassed=true,ambientPassed=true,decoysPassed=true;
var eta=Matrix.Diagonal(-1,1,1,1);var l=Matrix.Diagonal(1,2,3,4);Matrix[] points=[eta,l*eta*l];Rational[] betas=[0,new Rational(-1,2)];var basis=Basis();
var test=new Matrix(2);test[0,1]=1;test[1,0]=1;var singular=Matrix.Diagonal(1,0);var mixed=Matrix.Diagonal(2,-3);
bool[] arithmetic=[new Rational(2,4)==new Rational(1,2),new Rational(-3,-6)==new Rational(1,2),eta.Inverse().Same(eta),(l*l.Inverse()).Same(Matrix.Identity(4)),test.Inertia()==(1,1,0),singular.Inertia()==(1,0,1),mixed.Determinant()==-6,mixed.Inertia()==(1,1,0)];
foreach(bool pass in arithmetic){counts["arithmeticControls"]++;knownAnswerPassed&=pass;}if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
var rows=new List<object>();var witnessRows=new List<object>();var connections=new Matrix[2,2,10,10];var curvatures=new Matrix[2,2,10,10,10];var metrics=new Matrix[2,2];
for(int point=0;point<2;point++)for(int bi=0;bi<2;bi++)
{
 counts["contexts"]++;var y=points[point];var p=y.Inverse();Rational beta=betas[bi];var jet=new MetricJet(p,basis,1,beta);var gram=jet.Gram;metrics[point,bi]=gram;var gi=gram.Inverse();counts["inverseControls"]+=2;connectionPassed&=(y*p).Same(Matrix.Identity(4))&&(gram*gi).Same(Matrix.Identity(10));
 var gc=new Rational[10,10][];var dg=new Matrix[10,10,10];var lowered=new Rational[10,10,10,10];var local=nonzeroExpected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);var gammaRows=new List<object>();var curvatureRows=new List<object>();
 for(int a=0;a<10;a++)for(int b=0;b<10;b++)
 {
  counts["gramEntries"]++;connectionPassed&=gram[a,b]==Metric(p,basis[a],basis[b],1,beta);
  for(int x=0;x<10;x++)
  {
   counts["metricFirstJets"]++;connectionPassed&=jet.D[x,a,b]==DMetric(p,basis[x],basis[a],basis[b],1,beta);
   for(int z=0;z<10;z++){counts["metricSecondJets"]++;counts["metricSecondJetSymmetry"]++;connectionPassed&=jet.DD[x,z,a,b]==DDMetric(p,basis[x],basis[z],basis[a],basis[b],1,beta)&&jet.DD[x,z,a,b]==jet.DD[z,x,a,b];}
  }
  Rational[] cov=Enumerable.Range(0,10).Select(c=>(jet.D[a,b,c]+jet.D[b,a,c]-jet.D[c,a,b])*new Rational(1,2)).ToArray();gc[a,b]=Solve(gi,cov);var g=From(gc[a,b]);connections[point,bi,a,b]=g;counts["connectionOutputs"]++;connectionPassed&=g.Same(Gamma(p,basis[a],basis[b]));
  if(!g.Zero)local["connectionOutputs"]++;local["connectionMatrixEntries"]+=g.Nonzero;local["connectionCoordinates"]+=gc[a,b].Count(v=>v!=0);
  for(int d=0;d<10;d++)if(gc[a,b][d]!=0)gammaRows.Add(new{a,b,d,value=gc[a,b][d].ToString()});
  for(int c=0;c<10;c++){counts["koszulPairings"]++;connectionPassed&=Metric(p,g,basis[c],1,beta)==cov[c];}
 }
 Matrix Compose(int a,Rational[] v){Rational[] r=new Rational[10];for(int b=0;b<10;b++)for(int d=0;d<10;d++)r[d]+=v[b]*gc[a,b][d];return From(r);}
 for(int a=0;a<10;a++)for(int b=0;b<10;b++)
 {
  counts["torsionControls"]++;connectionPassed&=connections[point,bi,a,b].Same(connections[point,bi,b,a]);
  for(int c=0;c<10;c++){counts["metricCompatibility"]++;connectionPassed&=jet.D[a,b,c]==Metric(p,connections[point,bi,a,b],basis[c],1,beta)+Metric(p,basis[b],connections[point,bi,a,c],1,beta);}
  for(int x=0;x<10;x++)
  {
   Rational[] cov=new Rational[10];for(int c=0;c<10;c++){cov[c]=(jet.DD[x,a,b,c]+jet.DD[x,b,a,c]-jet.DD[x,c,a,b])*new Rational(1,2);for(int d=0;d<10;d++)cov[c]-=jet.D[x,d,c]*gc[a,b][d];}
   dg[x,a,b]=From(Solve(gi,cov));counts["connectionDerivativeOutputs"]++;connectionPassed&=dg[x,a,b].Same(DGamma(p,basis[x],basis[a],basis[b]));
  }
 }
 for(int a=0;a<10;a++)for(int b=0;b<10;b++)for(int c=0;c<10;c++)
 {
  var r=dg[a,b,c]-dg[b,a,c]+Compose(a,gc[b,c])-Compose(b,gc[a,c]);curvatures[point,bi,a,b,c]=r;counts["curvatureOutputs"]++;curvaturePassed&=r.Same(Curvature(y,p,basis[a],basis[b],basis[c]));
  if(!r.Zero)local["curvatureOutputs"]++;local["curvatureMatrixEntries"]+=r.Nonzero;var rc=Coordinates(r);local["curvatureCoordinates"]+=rc.Count(v=>v!=0);
  for(int d=0;d<10;d++){counts["loweredEntries"]++;lowered[a,b,c,d]=Metric(p,r,basis[d],1,beta);if(lowered[a,b,c,d]!=0)local["loweredCurvatureEntries"]++;if(rc[d]!=0)curvatureRows.Add(new{a,b,c,d,value=rc[d].ToString()});}
 }
 for(int a=0;a<10;a++)for(int b=0;b<10;b++)for(int c=0;c<10;c++)
 {
  counts["firstPairSkew"]++;counts["bianchi"]++;curvaturePassed&=(curvatures[point,bi,a,b,c]+curvatures[point,bi,b,a,c]).Zero&&(curvatures[point,bi,a,b,c]+curvatures[point,bi,b,c,a]+curvatures[point,bi,c,a,b]).Zero;
  for(int d=0;d<10;d++){counts["lastPairSkew"]++;counts["pairExchange"]++;curvaturePassed&=lowered[a,b,c,d]==lowered[a,b,d,c]*-1&&lowered[a,b,c,d]==lowered[c,d,a,b];}
 }
 Rational determinant=gram.Determinant(),detOracle=64*(1+beta*4)*Matrix.Inv(Pow(y.Determinant(),5));var inertia=gram.Inertia();counts["gramDeterminants"]++;counts["gramInertias"]++;connectionPassed&=determinant==detOracle&&determinant.ToString()==fx.GetProperty("expectedGramDeterminants")[point][bi].GetString()&&inertia==(bi==0?(7,3,0):(6,4,0));
 if(bi==0)foreach(var (alphaBad,betaBad,inertiaBad) in new[]{((Rational)0,(Rational)1,(1,0,9)),((Rational)1,new Rational(-1,4),(6,3,1))}){counts["singularMetricControls"]++;var bad=new MetricJet(p,basis,alphaBad,betaBad).Gram;decoysPassed&=bad.Determinant()==0&&bad.Inertia()==inertiaBad;}
 var transport=point==0?Matrix.Identity(4):l;var aWitness=transport*Matrix.Diagonal(0,1,-1,0)*transport;var bWitness=transport*basis[7]*transport;var dWitness=transport*Matrix.Diagonal(0,1,1,-2)*transport;
 Matrix RecoveredR(Matrix a,Matrix b,Matrix c){var ac=Coordinates(a);var bc=Coordinates(b);var cc=Coordinates(c);var output=new Matrix(4);for(int i=0;i<10;i++)if(ac[i]!=0)for(int j=0;j<10;j++)if(bc[j]!=0)for(int k=0;k<10;k++)if(cc[k]!=0)output+=curvatures[point,bi,i,j,k].Scale(ac[i]*bc[j]*cc[k]);return output;}
 var witness=RecoveredR(aWitness,bWitness,bWitness);Rational normA=Metric(p,aWitness,aWitness,1,beta),normB=Metric(p,bWitness,bWitness,1,beta),cross=Metric(p,aWitness,bWitness,1,beta),low=Metric(p,witness,aWitness,1,beta),section=low*Matrix.Inv(normA*normB-cross*cross);counts["witnesses"]++;
 bool witnessPass=witness.Same(aWitness.Scale(-1))&&normA==2&&normB==2&&cross==0&&low==-2&&section==new Rational(-1,2);decoysPassed&=witnessPass;
 counts["flatControls"]+=2;decoysPassed&=RecoveredR(aWitness,dWitness,bWitness).Zero&&RecoveredR(y,bWitness,aWitness).Zero;
 Rational flatFailure=DMetric(p,dWitness,dWitness,dWitness,1,beta);counts["flatConnectionDecoys"]++;decoysPassed&=flatFailure==12;
 var noDerivative=Gamma(p,aWitness,Gamma(p,bWitness,bWitness))-Gamma(p,bWitness,Gamma(p,aWitness,bWitness));counts["omittedDerivativeDecoys"]++;decoysPassed&=noDerivative.Same(aWitness)&&!noDerivative.Same(witness);
 witnessRows.Add(new{point,beta=beta.ToString(),passed=witnessPass,A=aWitness.Text(),B=bWitness.Text(),RABB=witness.Text(),normA=normA.ToString(),normB=normB.ToString(),cross=cross.ToString(),lowered=low.ToString(),sectional=section.ToString(),flatMetricCompatibilityDefect=flatFailure.ToString(),omittedDerivative=noDerivative.Text()});
 // Independent 14-dimensional block Koszul solve. Neither horizontal choice is
 // selected as the draft's identification; both expose the nonproduct pitfall.
 foreach(bool inverseHorizontal in new[]{false,true})
 {
  var horizontal=inverseHorizontal?p:y;var block=new Matrix(14);var first=new Rational[14,14,14];for(int i=0;i<4;i++)for(int j=0;j<4;j++)block[i,j]=horizontal[i,j];for(int a=0;a<10;a++)for(int b=0;b<10;b++)block[a+4,b+4]=gram[a,b];
  for(int x=0;x<10;x++){var dh=inverseHorizontal?(p*basis[x]*p).Scale(-1):basis[x];for(int i=0;i<4;i++)for(int j=0;j<4;j++)first[x+4,i,j]=dh[i,j];for(int a=0;a<10;a++)for(int b=0;b<10;b++)first[x+4,a+4,b+4]=jet.D[x,a,b];}
  var blockInverse=block.Inverse();counts["nonproductControls"]++;ambientPassed&=first[4,0,0]!=0&&(block*blockInverse).Same(Matrix.Identity(14));
  for(int a=0;a<10;a++)for(int b=0;b<10;b++){Rational[] cov=Enumerable.Range(0,14).Select(c=>(first[a+4,b+4,c]+first[b+4,a+4,c]-first[c,a+4,b+4])*new Rational(1,2)).ToArray();var solved=Solve(blockInverse,cov);counts["ambientVVOutputs"]++;ambientPassed&=solved.Take(4).All(v=>v==0)&&solved.Skip(4).SequenceEqual(gc[a,b]);}
 }
 foreach(var key in local.Keys){nonzero[key]+=local[key];curvaturePassed&=local[key]==fx.GetProperty("nonzeroPerContext").GetProperty(key).GetInt32();}
 rows.Add(new{point,beta=beta.ToString(),gram=gram.Text(),determinant=determinant.ToString(),inertia=new[]{inertia.Positive,inertia.Negative,inertia.Zero},nonzero=local,connectionCoefficients=gammaRows,curvatureCoefficients=curvatureRows});
}
for(int bi=0;bi<2;bi++)for(int a=0;a<10;a++)for(int b=0;b<10;b++)
{
 var ta=l*basis[a]*l;var tb=l*basis[b]*l;Rational sa=Coordinates(ta)[a],sb=Coordinates(tb)[b];counts["congruenceMetric"]++;counts["congruenceConnection"]++;connectionPassed&=metrics[1,bi][a,b]*sa*sb==metrics[0,bi][a,b]&&connections[1,bi,a,b].Scale(sa*sb).Same(l*connections[0,bi,a,b]*l);
 for(int c=0;c<10;c++){Rational sc=Coordinates(l*basis[c]*l)[c];counts["congruenceCurvature"]++;curvaturePassed&=curvatures[1,bi,a,b,c].Scale(sa*sb*sc).Same(l*curvatures[0,bi,a,b,c]*l);}
}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32())&&nonzero.All(q=>q.Value==nonzeroExpected.GetProperty(q.Key).GetInt32());bool resourcesPassed=Matrix.Products<=fx.GetProperty("resources").GetProperty("maximumTrackedMatrixProducts").GetInt64();
bool controlsPassed=knownAnswerPassed&&connectionPassed&&curvaturePassed&&ambientPassed&&decoysPassed&&countsPassed&&resourcesPassed;string verdict=!knownAnswerPassed?precedence[1]:!connectionPassed?precedence[2]:!curvaturePassed?precedence[3]:!ambientPassed?precedence[4]:!decoysPassed||!countsPassed||!resourcesPassed?precedence[5]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,connectionPassed,curvaturePassed,ambientPassed,decoysPassed,countsPassed,resourcesPassed,counts,nonzero,trackedMatrixProducts=Matrix.Products,rows,witnessRows,sourceNormalizationSelected=false,horizontalConventionSelected=false,shiabContractionNonzeroInferred=false,inducedStationaryBackgroundRejected=false,fullMetricEulerComputed=false,historicalFlatControlsRewritten=false});

void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=607,phaseId="phase607-source-induced-vertical-curvature-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/source_induced_vertical_curvature_audit.json",json);File.WriteAllText(Root+"/output/source_induced_vertical_curvature_audit_summary.json",json);Console.WriteLine($"Phase607 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
