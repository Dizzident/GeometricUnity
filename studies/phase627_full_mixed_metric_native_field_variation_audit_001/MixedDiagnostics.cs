using static Algebra;
using static Fourier;
using static Mixed;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Three separately planned diagnostic contexts: two finite nonzero controls
// and the complete fixed symmetric-second-jet wedge controls. These do not substitute
// for the700 actual-P5 rows or assert a spectrum at either diagnostic field.
// Begin must establish one trace context covering the complete diagnostic;
// its X is then constructed from traced units, never an unbound input leaf.
internal interface IMixedDiagnosticEvidence:IMixedWardEvidence
{
 void Begin(string diagnostic,DiagnosticMenu menu);
 void Background(string name,MixedBackground background);
 void GeometryLeaf(string name,int degree,FT value);
 void Geometry(string name,MixedBackground background,MixedMetricGerm germ);
 void Tensor(string name,int degree,FT value,bool expanded);
 void Scalars(string name,Rational[] values);
 void End(string diagnostic);
}
// These are exact name SETS, not execution-order promises; validate uniqueness
// and complete exhaustion in the sink. A named structured Original/Ward/
// Acceleration artifact means its FULL fixed-shape tensor/scalar contents,
// not a digest or summary. Geometry leaves/artifacts are separately typed
// callbacks: grade10 has1 background+4 germs+9 geometry leaves; acceleration
// has1 background+1 germ+3 geometry leaves.
internal sealed record DiagnosticMenu(string Id,string[] Tensors,string[] ScalarArrays,
 string[] OriginalActions,string[] WardActions,string[] Accelerations,
 string[] SecondJets,string[] Checks,int Backgrounds,int Germs,int VariationRows);

internal static class MixedDiagnostics
{
 static readonly int[] ForwardDegrees=[2,12,13,14,0,1,13,1];
 static readonly int[] ReverseDegrees=[1,13,2,1,0,14,12,2,2];
 static readonly string[] PieceNames=["source","kinetic","cubic","mass"];
 static readonly string[] VariationFamilies=["raw0","raw2","fieldFirst0","fieldFirst2","word0","word2","eulerCovariant","eulerMoving","native0","nativeEuler"];
 static readonly string[] SingleVariations=["fixedAdjoint","wordAdjoint","movingAdjointDelta","ordinaryAdjoint","native2","nativeDivergence","nativeAdjointAsFrame"];
 static readonly string[] CombinationFamilies=["raw0","raw2","fieldFirst0","fieldFirst2","word0","word2","eulerCovariant","eulerMoving"];
 static bool Same(FT a,FT b)=>Mixed.Equal(a,b);
 static bool Same(BiTensor a,BiTensor b)=>Same(a.Value,b.Value)&&Same(a.H,b.H)&&Same(a.U,b.U)&&Same(a.HU,b.HU);
 static int Jet(params int[] multi)=>Array.FindIndex(MetricVariation.Multiindices(),x=>x.SequenceEqual(multi));
 static IEnumerable<string> IntermediateNames()
 {
  for(int p=0;p<3;p++)for(int s=0;s<8;s++)foreach(string part in new[]{"value","delta"})yield return $"fixed_p{p}_s{s}_{part}";
  for(int s=0;s<9;s++)foreach(string part in new[]{"value","delta"})yield return $"reverse_s{s}_{part}";
  for(int z=0;z<14;z++){yield return $"covariant_z{z}";yield return $"covariantOracle_z{z}";}
  for(int mu=0;mu<14;mu++)yield return $"partial_mu{mu}";
 }
 static int IntermediateDegree(string name)
 {
  if(name.StartsWith("fixed_",StringComparison.Ordinal)){int first=name.IndexOf("_s",StringComparison.Ordinal)+2;return ForwardDegrees[int.Parse(name[first..name.IndexOf('_',first)])];}
  if(name.StartsWith("reverse_s",StringComparison.Ordinal))return ReverseDegrees[int.Parse(name[9..name.IndexOf('_',9)])];
  return 2;
 }
 static IEnumerable<string> VariationTensorNames(string prefix)
 {
  foreach(string intermediate in IntermediateNames())yield return prefix+"/intermediate/"+intermediate;
  foreach(string family in VariationFamilies)foreach(string piece in PieceNames.Append("total"))yield return prefix+"/"+family+"/"+piece;
  foreach(string single in SingleVariations)yield return prefix+"/"+single;
  for(int mu=0;mu<14;mu++)for(int nu=0;nu<14;nu++)foreach(string kind in new[]{"current","currentPartial","green"})yield return $"{prefix}/{kind}/{mu}/{nu}";
 }
 static IEnumerable<string> BackgroundCheckNames()
 {for(int mu=0;mu<14;mu++)yield return "background/nativeJet/"+mu;yield return "background/covariantExterior";}
 static IEnumerable<string> OriginalCheckNames(string prefix)
 {for(int p=0;p<3;p++)for(int s=0;s<8;s++)yield return $"{prefix}/stage/{p}/{s}";yield return prefix+"/scalars";}
 static IEnumerable<string> WardCompareCheckNames(string prefix)
 {
  yield return prefix+"/primitiveMenu";for(int p=0;p<33;p++)yield return $"{prefix}/primitive/{p}";
  for(int route=0;route<2;route++){for(int p=0;p<3;p++)for(int s=0;s<8;s++)yield return $"{prefix}/density/{route}/stage/{p}/{s}";for(int p=0;p<4;p++)yield return $"{prefix}/density/{route}/top/{p}";yield return $"{prefix}/density/{route}/scalars";}
 }
 public static DiagnosticMenu GradeTenMenu()
 {
  //3082 tensor marks,10 scalar arrays,8 original-action records,1019 checks.
  var tensors=new List<string>{"input/X","input/UFrame","input/UNative","input/dUNative","combined/motionX","combined/crossQ","combined/motionCrossQ","combined/adjointMotionX","combined/wordAdjointMotionX","combined/fixedAdjoint","combined/movingAdjointDelta","combined/pairingMotionAdjoint","combined/adjointPhi1","combined/pairingMotionAdjointPhi1"};
  for(int basis=0;basis<4;basis++)tensors.AddRange(VariationTensorNames("basis"+basis));
  foreach(string family in CombinationFamilies)foreach(string piece in PieceNames.Append("total"))tensors.Add("combined/"+family+"/"+piece);
  for(int s=0;s<8;s++){tensors.Add($"forwardHand/literal/{s}");tensors.Add($"forwardHand/word/{s}");}
  var scalars=Enumerable.Range(0,2).Select(r=>"combined/originalRoute"+r).Concat(CombinationFamilies.Select(f=>"combined/projection/"+f)).ToArray();
  var originals=(from basis in Enumerable.Range(0,4) from route in Enumerable.Range(0,2) select $"basis{basis}/original{route}").ToArray();
  var checks=BackgroundCheckNames().Append("background/gradePreservingJets").ToList();
  for(int basis=0;basis<4;basis++)
  {
   string prefix="basis"+basis;foreach(string kind in new[]{"covariantDerivative","ordinaryAdjoint","fixedAdjoint"})checks.Add(prefix+"/"+kind);
   for(int p=0;p<4;p++)foreach(string kind in new[]{"raw0","raw2","euler","nativeEuler","rawOriginal"})checks.Add($"{prefix}/{kind}/{p}");
   for(int mu=0;mu<14;mu++)for(int nu=0;nu<14;nu++)checks.Add($"{prefix}/current/{mu}/{nu}");checks.AddRange(OriginalCheckNames(prefix+"/originalEquality"));
  }
  checks.Add("combined/realizableMotion");for(int route=0;route<2;route++)checks.Add("combined/originalForecast"+route);
  foreach(int f in new[]{0,2,4,6,7})checks.Add("combined/forecast/"+CombinationFamilies[f]);for(int s=0;s<8;s++)checks.Add("forwardHand/stage/"+s);
  foreach(string name in new[]{"probeNorm","quadraticZero","motionX","crossQ","motionCrossZero","reverseWord","reverseCoefficient","forwardCoefficient","movingAdjointPairing","adjointPhi","nonzeroV2"})checks.Add("hand/"+name);
  return new("grade10",tensors.ToArray(),scalars,originals,[],[],[],checks.ToArray(),1,4,4);
 }
 public static DiagnosticMenu AccelerationMenu()
 {
  //3 tensor marks,3 scalar arrays,2 original-action,4 Ward,2 acceleration
  //records and310 checks. Full structured contents remain mandatory.
  var checks=BackgroundCheckNames().ToList();for(int route=0;route<2;route++)
  {
   string prefix="route"+route;foreach(string name in new[]{"forecast","accelerationCoefficient","accelerationJets"})checks.Add(prefix+"/"+name);
   foreach(string variant in new[]{"epsilon","compensated"})foreach(string name in new[]{"descent","derivative","curvature","phiFirst","phiOuter","phiInner"})checks.Add(prefix+"/"+variant+"/"+name);
   for(int p=0;p<4;p++)foreach(string name in new[]{"ward","epsilonWard","green"})checks.Add($"{prefix}/{name}/{p}");
  }
  checks.AddRange(WardCompareCheckNames("word/epsilon"));checks.AddRange(WardCompareCheckNames("word/compensated"));checks.AddRange(OriginalCheckNames("word/original"));
  checks.Add("word/acceleration/value");checks.Add("word/acceleration/scalars");for(int mu=0;mu<14;mu++)checks.Add($"word/acceleration/jet/{mu}");for(int p=0;p<2;p++)for(int s=0;s<8;s++)checks.Add($"word/acceleration/stage/{p}/{s}");
  return new("acceleration",["input/X","input/eta","input/dEta"],
   ["forecast/original","route0/nativeTangent","route1/nativeTangent"],
   ["route0/fixedNativeTangent","route1/fixedNativeTangent"],
   ["route0/epsilon","route0/compensated","route1/epsilon","route1/compensated"],
   ["route0/acceleration","route1/acceleration"],[],checks.ToArray(),1,1,0);
 }
 public static DiagnosticMenu SecondJetsMenu()
 {
  var names=MixedAuditPlan.SecondJetNames();
  return new("secondJets",[],[],[],[],[],names,names.ToArray(),0,0,0);
 }
 public static void SecondJets(IMixedDiagnosticEvidence evidence)
 {
  // Begin establishes its own trace BEFORE either coefficient unit exists.
  // This is not appended silently to grade10/acceleration or a scientific row.
  evidence.Begin("secondJets",SecondJetsMenu());
  MixedWardControls.SymmetricSecondJets(evidence);
  evidence.End("secondJets");
 }
 static void BackgroundChecks(IMixedDiagnosticEvidence evidence,string prefix,MixedBackground b)
 {
  // The diagnostic extension is specified by constant associated-frame
  // coefficients, not by assuming the native first derivative vanishes.
  for(int mu=0;mu<14;mu++)evidence.Check($"{prefix}/nativeJet/{mu}",Same(b.NativePartial[mu],b.NativePartialOracle[mu]));
  evidence.Check(prefix+"/covariantExterior",Same(b.DX,b.DXOracle));
 }
 static FT[][] Families(MixedVariation row)=>[row.Raw0,row.Raw2,row.FieldFirst0,row.FieldFirst2,row.Oracle0,row.Oracle2,row.EulerCovariant,row.EulerMoving,row.Native0,row.NativeEuler];
 static void RetainVariation(IMixedDiagnosticEvidence evidence,string prefix,MixedVariation row,MixedBackground b)
 {
  var families=Families(row);for(int family=0;family<families.Length;family++)
  {
   int degree=family is 1 or 3 or 5?2:1;
   for(int p=0;p<4;p++)evidence.Tensor(prefix+"/"+VariationFamilies[family]+"/"+PieceNames[p],degree,families[family][p],true);
   evidence.Tensor(prefix+"/"+VariationFamilies[family]+"/total",degree,Sum(families[family]),true);
  }
  FT[] singles=[row.CAdjointFixed,row.CAdjointOracle,row.MovingAdjointDelta,row.OrdinaryAdjoint,row.Native2,row.NativeDivergence,row.NativeAdjointAsFrame];
  for(int s=0;s<singles.Length;s++)evidence.Tensor(prefix+"/"+SingleVariations[s],s is 0 or 1 or 2 or 4?2:1,singles[s],true);
  evidence.Check(prefix+"/covariantDerivative",row.DerivativeIdentityPassed);
  evidence.Check(prefix+"/ordinaryAdjoint",Same(row.OrdinaryAdjoint,row.NativeAdjointAsFrame));
  evidence.Check(prefix+"/fixedAdjoint",Same(row.CAdjointFixed,row.CAdjointOracle));
  for(int p=0;p<4;p++)
  {
   evidence.Check($"{prefix}/raw0/{p}",Same(row.Raw0[p],row.FieldFirst0[p])&&Same(row.Raw0[p],row.Oracle0[p]));
   evidence.Check($"{prefix}/raw2/{p}",Same(row.Raw2[p],row.FieldFirst2[p])&&Same(row.Raw2[p],row.Oracle2[p]));
   evidence.Check($"{prefix}/euler/{p}",Same(row.EulerCovariant[p],row.EulerMoving[p]));
   evidence.Check($"{prefix}/nativeEuler/{p}",Same(row.NativeEuler[p],MixedVariation.CoordinateDual(b.Frame,row.EulerCovariant[p])));
  }
  for(int mu=0;mu<14;mu++)for(int nu=0;nu<14;nu++)
  {
   evidence.Tensor($"{prefix}/current/{mu}/{nu}",0,row.CurrentCoefficients[mu,nu],true);
   evidence.Tensor($"{prefix}/currentPartial/{mu}/{nu}",0,row.CurrentPartialCoefficients[mu,nu],false);
   evidence.Tensor($"{prefix}/green/{mu}/{nu}",0,row.GreenVariationCoefficients[mu,nu],true);
   evidence.Check($"{prefix}/current/{mu}/{nu}",Same(row.CurrentCoefficients[mu,nu],row.GreenVariationCoefficients[mu,nu])&&Same(row.CurrentCoefficients[mu,nu],ExteriorComponent(row.Native2,mu,nu)));
  }
 }
 static void CompareOriginal(IMixedDiagnosticEvidence evidence,string prefix,OriginalMixedResult a,OriginalMixedResult b)
 {
  for(int p=0;p<3;p++)for(int s=0;s<8;s++)evidence.Check($"{prefix}/stage/{p}/{s}",Same(a.Stages[p][s],b.Stages[p][s]));
  evidence.Check(prefix+"/scalars",a.Value.SequenceEqual(b.Value)&&a.Metric.SequenceEqual(b.Metric)&&a.Field.SequenceEqual(b.Field)&&a.Mixed.SequenceEqual(b.Mixed));
 }
 public static void GradeTen(IMixedDiagnosticEvidence evidence)
 {
  evidence.Begin("grade10",GradeTenMenu());
  var x=Sum(Unit(1<<1,(1<<1)|(1<<10),1),Unit(1<<3,(1<<5)|(1<<6)|(1<<7)|(1<<8)|(1<<9),1));
  var u=Unit(1<<4,(1<<0)|(1<<1)|(1<<3)|(1<<4)|(1<<5)|(1<<6)|(1<<7)|(1<<8)|(1<<9)|(1<<10),1);
  evidence.Tensor("input/X",1,x,true);evidence.Tensor("input/UFrame",1,u,true);
  var b=new MixedBackground(0,x,(name,degree,value)=>evidence.GeometryLeaf("background/"+name,degree,value));evidence.Background("background",b);BackgroundChecks(evidence,"background",b);
  var nativeU=Pullback(b.InverseFrame,u);var nativeDu=new FT();evidence.Tensor("input/UNative",1,nativeU,true);evidence.Tensor("input/dUNative",2,nativeDu,true);
  evidence.Check("background/gradePreservingJets",b.NativePartial.All(t=>t.Keys.All(k=>Degree(k.Blade) is 2 or 5)));
  Rational[] weights=[new(-1,2),new(1,2),new(-1,2),new(-1,2)];
  var sums=Enumerable.Range(0,8).Select(_=>Enumerable.Range(0,4).Select(_=>new FT()).ToArray()).ToArray();
  var scalarSums=new[]{new Rational[4],new Rational[4]};var motion=new Matrix(14);var adjoint=new FT();var movingAdjoint=new FT();
  for(int basis=0;basis<4;basis++)
  {
   string prefix="basis"+basis;var g=new MixedMetricGerm(b,basis,Jet(1,0,0,0),(name,degree,value)=>evidence.GeometryLeaf(prefix+"/"+name,degree,value));evidence.Geometry(prefix,b,g);
   motion=motion+g.Motion.Scale(weights[basis]);
   var row=new MixedVariation(b,g,(name,t)=>evidence.Tensor(prefix+"/intermediate/"+name,IntermediateDegree(name),t,false));RetainVariation(evidence,prefix,row,b);
   var families=Families(row);for(int f=0;f<8;f++)for(int p=0;p<4;p++)sums[f][p]=Sum(sums[f][p],Times(families[f][p],weights[basis]));
   adjoint=Sum(adjoint,Times(row.CAdjointFixed,weights[basis]));movingAdjoint=Sum(movingAdjoint,Times(row.MovingAdjointDelta,weights[basis]));
   var literal=OriginalMixedAction.Evaluate(b,g,nativeU,nativeDu);var word=OriginalMixedAction.Evaluate(b,g,nativeU,nativeDu,true);
   evidence.Original(prefix+"/original0",literal);evidence.Original(prefix+"/original1",word);CompareOriginal(evidence,prefix+"/originalEquality",literal,word);
   for(int p=0;p<4;p++)
   {
    scalarSums[0][p]+=literal.Mixed[p]*weights[basis];scalarSums[1][p]+=word.Mixed[p]*weights[basis];
    evidence.Check($"{prefix}/rawOriginal/{p}",literal.Mixed[p]==Pair(u,row.Raw0[p]));
   }
  }
  var expectedMotion=new Matrix(14);expectedMotion[1,0]=1;evidence.Check("combined/realizableMotion",motion.Same(expectedMotion));
  Rational[] forecast=[0,0,new(4,3),0];for(int route=0;route<2;route++){evidence.Scalars("combined/originalRoute"+route,scalarSums[route]);evidence.Check("combined/originalForecast"+route,scalarSums[route].SequenceEqual(forecast));}
  for(int f=0;f<8;f++)
  {
   int degree=f is 1 or 3 or 5?2:1;var projection=new Rational[4];
   for(int p=0;p<4;p++){evidence.Tensor("combined/"+CombinationFamilies[f]+"/"+PieceNames[p],degree,sums[f][p],true);projection[p]=degree==1?Pair(u,sums[f][p]):0;}
   evidence.Tensor("combined/"+CombinationFamilies[f]+"/total",degree,Sum(sums[f]),true);evidence.Scalars("combined/projection/"+CombinationFamilies[f],projection);
   if(degree==1)evidence.Check("combined/forecast/"+CombinationFamilies[f],projection.SequenceEqual(forecast));
  }
  var motionX=Motion(motion,x);var cross=Sum(P(x,u),P(u,x));var motionCross=Motion(motion,cross);var reverse=FixedAdjoint(motionX);var reverseWord=FixedAdjoint(motionX,true);
  var pairingAdjoint=PairingMotion(motion,b.AdjointX);var adjointPhi=FixedAdjoint(Phi1);var pairingAdjointPhi=PairingMotion(motion,adjointPhi);
  FT[] hand=[motionX,cross,motionCross,reverse,reverseWord,adjoint,movingAdjoint,pairingAdjoint,adjointPhi,pairingAdjointPhi];
  string[] names=["motionX","crossQ","motionCrossQ","adjointMotionX","wordAdjointMotionX","fixedAdjoint","movingAdjointDelta","pairingMotionAdjoint","adjointPhi1","pairingMotionAdjointPhi1"];
  for(int i=0;i<hand.Length;i++)evidence.Tensor("combined/"+names[i],i==0?1:2,hand[i],true);
  var fdiag=Unit((1<<3)|(1<<4),(1<<0)|(1<<1)|(1<<3)|(1<<4)|(1<<10),1);var stages=FixedForward(fdiag);var words=FixedForward(fdiag,true);
  for(int s=0;s<8;s++){evidence.Tensor("forwardHand/literal/"+s,ForwardDegrees[s],stages[s],true);evidence.Tensor("forwardHand/word/"+s,ForwardDegrees[s],words[s],true);evidence.Check("forwardHand/stage/"+s,Same(stages[s],words[s]));}
  evidence.Check("hand/probeNorm",Pair(u,u)==1);evidence.Check("hand/quadraticZero",b.Q.Count==0);
  evidence.Check("hand/motionX",Same(motionX,Unit(1,(1<<1)|(1<<10),1)));
  evidence.Check("hand/crossQ",Same(cross,Times(fdiag,-2)));evidence.Check("hand/motionCrossZero",motionCross.Count==0);
  evidence.Check("hand/reverseWord",Same(reverse,reverseWord));evidence.Check("hand/reverseCoefficient",reverse.GetValueOrDefault(((1<<3)|(1<<4),(1<<0)|(1<<1)|(1<<3)|(1<<4)|(1<<10),0,0))==-2);
  evidence.Check("hand/forwardCoefficient",stages[7].GetValueOrDefault((1,(1<<1)|(1<<10),0,0))==2);
  evidence.Check("hand/movingAdjointPairing",Same(adjoint,Sum(movingAdjoint,pairingAdjoint)));
  evidence.Check("hand/adjointPhi",Same(adjointPhi,Times(Phi2,-24)));
  evidence.Check("hand/nonzeroV2",pairingAdjointPhi.GetValueOrDefault(((1<<0)|(1<<2),(1<<1)|(1<<2),0,0))==-24);
  evidence.End("grade10");
 }
 static void CompareWard(IMixedDiagnosticEvidence evidence,string prefix,WardActionResult a,WardActionResult b)
 {
  evidence.Check(prefix+"/primitiveMenu",a.Primitives.Select(p=>p.Name).SequenceEqual(b.Primitives.Select(p=>p.Name)));
  for(int p=0;p<a.Primitives.Length;p++)evidence.Check($"{prefix}/primitive/{p}",Same(a.Primitives[p].Tensor,b.Primitives[p].Tensor));
  var first=new[]{a.Literal,a.Descended};var second=new[]{b.Literal,b.Descended};
  for(int route=0;route<2;route++)
  {
   for(int p=0;p<3;p++)for(int s=0;s<8;s++)evidence.Check($"{prefix}/density/{route}/stage/{p}/{s}",Same(first[route].Stages[p][s],second[route].Stages[p][s]));
   for(int p=0;p<4;p++)evidence.Check($"{prefix}/density/{route}/top/{p}",Same(first[route].TopForms[p],second[route].TopForms[p]));
   evidence.Check($"{prefix}/density/{route}/scalars",first[route].Value.SequenceEqual(second[route].Value)&&first[route].Metric.SequenceEqual(second[route].Metric)&&first[route].Field.SequenceEqual(second[route].Field)&&first[route].Mixed.SequenceEqual(second[route].Mixed));
  }
 }
 public static void Acceleration(IMixedDiagnosticEvidence evidence)
 {
  evidence.Begin("acceleration",AccelerationMenu());var x=Unit(1<<7,1,1);var eta=Unit(0,1<<4,1);var deta=new FT();
  evidence.Tensor("input/X",1,x,true);evidence.Tensor("input/eta",0,eta,true);evidence.Tensor("input/dEta",1,deta,true);
  var b=new MixedBackground(1,x,(name,degree,value)=>evidence.GeometryLeaf("background/"+name,degree,value));evidence.Background("background",b);BackgroundChecks(evidence,"background",b);
  var g=new MixedMetricGerm(b,0,Jet(0,2,0,0),(name,degree,value)=>evidence.GeometryLeaf("germ/"+name,degree,value));evidence.Geometry("germ",b,g);
  Rational[] forecast=[0,0,0,-85098];evidence.Scalars("forecast/original",forecast);
  WardActionResult? firstEpsilon=null,firstCompensated=null;WardAccelerationResult? firstAcceleration=null;OriginalMixedResult? firstOriginal=null;
  for(int route=0;route<2;route++)
  {
   bool oracle=route==1;string prefix="route"+route;var epsilon=MixedWard.Evaluate(b,g,eta,deta,false,oracle);var compensated=MixedWard.Evaluate(b,g,eta,deta,true,oracle);var acceleration=MixedWard.Acceleration(b,g,eta,deta,oracle);
   evidence.Ward(prefix+"/epsilon",epsilon);evidence.Ward(prefix+"/compensated",compensated);evidence.Acceleration(prefix+"/acceleration",acceleration);
   var nativeU=Pullback(b.InverseFrame,compensated.Tangent,oracle);var nativeDu=Pullback(b.InverseFrame,Sum(compensated.DBTangent,Times(P(b.B,compensated.Tangent,'C',oracle),-1)),oracle);
   var original=OriginalMixedAction.Evaluate(b,g,nativeU,nativeDu,oracle);evidence.Original(prefix+"/fixedNativeTangent",original);evidence.Scalars(prefix+"/nativeTangent",original.Mixed);
   evidence.Check(prefix+"/forecast",acceleration.Original.SequenceEqual(forecast));
   // Full hand forecast, not just the component entering the mass pairing.
   var expectedW=Times(Sum(Unit(1,1<<7,1),Unit(1<<7,1,1)),new Rational(-3,32));
   evidence.Check(prefix+"/accelerationCoefficient",Same(acceleration.W,expectedW));
   evidence.Check(prefix+"/accelerationJets",Same(acceleration.Dw,acceleration.DwFromJet)&&Same(acceleration.DBw,acceleration.DBwFromJet));
   foreach(var row in new[]{epsilon,compensated})
   {
    string tag=prefix+(row.Compensated?"/compensated":"/epsilon");
    evidence.Check(tag+"/descent",row.Literal.Value.SequenceEqual(row.Descended.Value)&&row.Literal.Metric.SequenceEqual(row.Descended.Metric)&&row.Literal.Field.SequenceEqual(row.Descended.Field)&&row.Literal.Mixed.SequenceEqual(row.Descended.Mixed));
    WardPrimitive Named(string name)=>row.Primitives.Single(p=>p.Name==name);
    evidence.Check(tag+"/derivative",Same(Named("descendedDerivative").Tensor,Named("descendedDerivativeOracle").Tensor));
    evidence.Check(tag+"/curvature",Same(Named("DBSquaredEpsilon").Tensor,Named("DBSquaredEpsilonExpanded").Tensor)&&Same(Named("DBSquaredEpsilon").Tensor.HU,Named("curvatureVariationCommutator").Tensor.HU));
    foreach(string suffix in new[]{"First","Outer","Inner"})evidence.Check(tag+"/phi"+suffix,Same(Named("descendedPhi"+suffix).Tensor,Named("unrotatedPhi"+suffix).Tensor));
   }
   for(int p=0;p<4;p++)
   {
    evidence.Check($"{prefix}/ward/{p}",compensated.Literal.Mixed[p]+acceleration.Original[p]==0);
    evidence.Check($"{prefix}/epsilonWard/{p}",epsilon.Literal.Mixed[p]+original.Mixed[p]+acceleration.Original[p]==0);
    evidence.Check($"{prefix}/green/{p}",acceleration.Original[p]==acceleration.Euler[p]+(p==1?acceleration.Divergence:(Rational)0));
   }
   if(route==0){firstEpsilon=epsilon;firstCompensated=compensated;firstAcceleration=acceleration;firstOriginal=original;}
   else
   {
    CompareWard(evidence,"word/epsilon",firstEpsilon!,epsilon);CompareWard(evidence,"word/compensated",firstCompensated!,compensated);CompareOriginal(evidence,"word/original",firstOriginal!,original);
    var first=firstAcceleration!;evidence.Check("word/acceleration/value",Same(first.W,acceleration.W)&&Same(first.Dw,acceleration.Dw)&&Same(first.DBw,acceleration.DBw)&&Same(first.CrossQ,acceleration.CrossQ));
    evidence.Check("word/acceleration/scalars",first.Original.SequenceEqual(acceleration.Original)&&first.Euler.SequenceEqual(acceleration.Euler)&&first.Current.SequenceEqual(acceleration.Current)&&first.CoordinateCurrent.SequenceEqual(acceleration.CoordinateCurrent)&&first.Divergence==acceleration.Divergence);
    for(int mu=0;mu<14;mu++)evidence.Check($"word/acceleration/jet/{mu}",Same(first.PartialW[mu],acceleration.PartialW[mu])&&Same(first.CovariantW[mu],acceleration.CovariantW[mu])&&Same(first.DeltaBPartialInFrame[mu],acceleration.DeltaBPartialInFrame[mu])&&Same(first.CovariantAdjointX[mu],acceleration.CovariantAdjointX[mu])&&first.CurrentCovariantDerivative[mu].SequenceEqual(acceleration.CurrentCovariantDerivative[mu]));
    for(int p=0;p<2;p++)for(int s=0;s<8;s++)evidence.Check($"word/acceleration/stage/{p}/{s}",Same(first.Stages[p][s],acceleration.Stages[p][s]));
   }
  }
  evidence.End("acceleration");
 }
}
