// METADATA ONLY. No geometry, coefficient, polynomial or operator is evaluated
// by these menus. They close callback coverage, not the separate semantic DAG
// proof, rational/resource limits, source provenance or scientific predicates.
internal sealed record MixedPlannedCallback(string Category,string Name,int? Degree=null,bool? Expanded=null,int? Length=null);
internal sealed record MixedBaselineRole(string Role,string CanonicalId,int Degree);
internal sealed record MixedContextPlan(string Id,MixedPlannedCallback[] Callbacks);

internal static class MixedAuditPlan
{
 public const string Schema="phase627-a68-callback-census-v1";
 public const int RunContextCount=705;
 static readonly string[] Pieces=["source","kinetic","cubic","mass"];
 static readonly int[] ForwardDegrees=[2,12,13,14,0,1,13,1];
 static readonly int[] ReverseDegrees=[1,13,2,1,0,14,12,2,2];
 static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException("A68 plan: "+message);}
 // One authoritative complete context set. The third diagnostic is NOT an
 // extra scientific germ: both points still have exactly10*35 germ rows.
 public static string[] RunContextIds()
 {
  var ids=new List<string>(RunContextCount);
  for(int point=0;point<2;point++)
  {ids.Add("point"+point);for(int metric=0;metric<10;metric++)for(int jet=0;jet<35;jet++)ids.Add($"point{point}/m{metric}_j{jet}");}
  ids.AddRange(["diagnostic/grade10","diagnostic/acceleration","diagnostic/secondJets"]);
  Require(ids.Count==RunContextCount&&ids.Distinct(StringComparer.Ordinal).Count()==RunContextCount,"exact705 independent context IDs");return ids.ToArray();
 }
 public static void ValidateRunContextIds(IEnumerable<string> actual)
 {
  var expected=new HashSet<string>(RunContextIds(),StringComparer.Ordinal);var seen=new HashSet<string>(StringComparer.Ordinal);
  foreach(string id in actual)
  {Require(seen.Count<RunContextCount&&id is not null&&expected.Contains(id),"unexpected or excess run context");Require(seen.Add(id!),"duplicate run context");}
  Require(seen.Count==RunContextCount,"requires all705 contexts, including dedicated diagnostic/secondJets");
 }
 public static string[] SecondJetNames()
 {
  var names=new List<string>(420);
  for(int coefficient=0;coefficient<2;coefficient++)for(int first=0;first<14;first++)for(int second=first;second<14;second++)for(int route=0;route<2;route++)
   names.Add($"eta{coefficient}_second_{first}_{second}_route{route}");
  Require(names.Count==420,"all2*105*2 second-jet images");return names.ToArray();
 }
 public static MixedContextPlan SecondJets()
 {
  var callbacks=new List<MixedPlannedCallback>{new("Begin","$")};
  foreach(string name in SecondJetNames()){callbacks.Add(new("SecondJet",name,2));callbacks.Add(new("Check",name));}
  callbacks.Add(new("End","$"));return Plan("diagnostic/secondJets",callbacks);
 }
 // Independent fixed combinatorial menu: same specified divided monomials,
 // but no call into a geometry constructor or observed result is permitted.
 public static int[][] Multiindices()
 {var values=new List<int[]>();for(int order=0;order<=3;order++)for(int a=0;a<=order;a++)for(int b=0;b<=order-a;b++)for(int c=0;c<=order-a-b;c++)values.Add([a,b,c,order-a-b-c]);return values.ToArray();}
 static (int I,int J)[] MetricPairs()=>Enumerable.Range(0,4).Select(i=>(i,i)).Concat(from i in Enumerable.Range(0,4) from j in Enumerable.Range(i+1,3-i) select(i,j)).ToArray();
 public static MixedBaselineRole[] BaselineRoles()
 {
  var r=new List<MixedBaselineRole>();void Add(string role,int degree,string? id=null)=>r.Add(new(role,id??role,degree));
  Add("X",1);Add("B",1);Add("F",2);Add("DX",2);Add("Q",2);Add("AdjointX",2);
  for(int p=0;p<3;p++)Add($"KInputs[{p}]",1);
  for(int p=0;p<4;p++)Add($"GradientPieces[{p}]",1,p==0?"KInputs[0]":null);
  Add("NativeExterior",2);for(int z=0;z<14;z++)Add($"CovariantFrame[{z}]",1);
  Require(r.Count==28&&r.Select(x=>x.CanonicalId).Distinct(StringComparer.Ordinal).Count()==27,"baseline28 roles/27 canonical tensors");return r.ToArray();
 }
 // The alias must be checked as object identity by the eventual importer,
 // not guessed from equal hashes. No other equal-value inputs are merged.
 public static void ValidateBaselineAliases(IReadOnlyDictionary<string,object> objects)
 {
  var roles=BaselineRoles();Require(objects.Count==28&&roles.All(r=>objects.ContainsKey(r.Role)),"exact baseline role dictionary");
  Require(objects.Values.All(v=>v is not null),"nonnull baseline objects");
  for(int a=0;a<roles.Length;a++)for(int b=0;b<roles.Length;b++)Require(ReferenceEquals(objects[roles[a].Role],objects[roles[b].Role])==(roles[a].CanonicalId==roles[b].CanonicalId),"only the declared KInputs[0]/GradientPieces[0] alias is permitted");
 }
 static MixedContextPlan Plan(string id,List<MixedPlannedCallback> callbacks)
 {
  Require(callbacks.All(c=>!string.IsNullOrWhiteSpace(c.Category)&&!string.IsNullOrWhiteSpace(c.Name)),"nonempty callback identifiers");
  Require(callbacks.Select(c=>(c.Category,c.Name)).Distinct().Count()==callbacks.Count,"duplicate planned callback");
  return new(id,callbacks.ToArray());
 }
 public static MixedContextPlan Point(int point)
 {
  Require(point is 0 or 1,"point index");var c=new List<MixedPlannedCallback>{new("BeginPoint","$"),new("GeometryLeaf","curvature",2),new("Background","$")};
  for(int mu=0;mu<14;mu++)c.Add(new("Check",$"nativeFirstJet_{mu}"));c.Add(new("Check","covariantExterior"));
  if(point==0)foreach(string name in new[]{"nativeFirstJetHandAnchor","nativeExteriorHandAnchor","nativeVectorAnchorPositive"})c.Add(new("Check",name));
  c.Add(new("SealPointBackground","$"));
  for(int metric=0;metric<10;metric++)for(int jet=0;jet<35;jet++)c.Add(new("Germ",$"m{metric}_j{jet}"));c.Add(new("EndPoint","$"));return Plan($"point{point}",c);
 }
 public static MixedContextPlan Germ(int point,int metric,int jet)
 {
  Require(point is 0 or 1&&metric is >=0 and <10&&jet is >=0 and <35,"germ indices");var multi=Multiindices()[jet];int order=multi.Sum();
  var c=new List<MixedPlannedCallback>{new("BeginGerm","$"),new("GeometryLeaf","deltaCurvatureAdapted",2),new("GeometryLeaf","deltaCurvatureOracle",2),new("Geometry","$")};
  void Check(string name)=>c.Add(new("Check",name));void Tensor(string name,int degree,bool expanded)=>c.Add(new("Tensor",name,degree,expanded));
  Check("traceFreeMotion");Check("metricValueDualBlocks");
  for(int mu=0;mu<14;mu++)
  {
   Check($"metricFirstDualBlocks_{mu}");Check($"connectionDualPalatini_{mu}");
   for(int nu=0;nu<14;nu++){Check($"metricSecondDualBlocks_{mu}_{nu}");Check($"connectionFirstDualPalatini_{mu}_{nu}");}
  }
  foreach(string name in new[]{"curvatureDualPalatini","spinCurvatureLowering","referenceCurvatureDerivative","nonInvariantCovariantDerivative","fixedAdjointIndependent","movingAdjointPairingMotion","ordinaryAdjointNativeDivergence"})Check(name);
  for(int p=0;p<3;p++)for(int s=0;s<8;s++)foreach(string part in new[]{"value","delta"})Tensor($"intermediate_fixed_p{p}_s{s}_{part}",ForwardDegrees[s],false);
  for(int s=0;s<9;s++)foreach(string part in new[]{"value","delta"})Tensor($"intermediate_reverse_s{s}_{part}",ReverseDegrees[s],false);
  for(int z=0;z<14;z++){Tensor($"intermediate_covariant_z{z}",2,false);Tensor($"intermediate_covariantOracle_z{z}",2,false);Tensor($"intermediate_partial_mu{z}",2,false);}
  foreach(var family in new[]{("raw0",1),("raw2",2),("fieldFirst0",1),("fieldFirst2",2),("word0",1),("word2",2),("eulerCovariant",1),("eulerMoving",1),("native0",1),("nativeEuler",1),("nativeFieldFirst0",1),("nativeMovingEuler",1)})
   foreach(string piece in Pieces.Append("total"))Tensor(family.Item1+"_"+piece,family.Item2,true);
  foreach(var single in new[]{("native2",2),("nativeFieldFirst2",2),("nativeDivergence",1),("ordinaryAdjoint",1),("current",2),("greenCurrent",2)})Tensor(single.Item1,single.Item2,true);
  foreach(string piece in Pieces)
  {
   foreach(string kind in new[]{"rawFieldFirst0","rawFieldFirst2","rawWord0","rawWord2","eulerMoving","nativeEuler"})Check(kind+"_"+piece);
   if(order==3&&piece!="source")Check("thirdGermLowerPiece_"+piece);
  }
  for(int mu=0;mu<14;mu++)for(int nu=0;nu<14;nu++)
  {
   Check($"nativeJetSlot_{mu}_{nu}");Check($"greenCurrent_{mu}_{nu}");Tensor($"currentPartial_{mu}_{nu}",0,false);
   if(mu==nu)Check($"diagonalNull_{mu}");if(mu<nu)Check($"symmetricNull_{mu}_{nu}");
  }
  Check("completeCurrent");c.Add(new("Error","$"));if(order==3)Check("thirdGermZeroBackgroundError");
  if(point==1&&metric==0&&multi.SequenceEqual(new[]{0,3,0,0}))Check("thirdSourceHandAnchor");
  if(point==0&&metric==0&&multi.SequenceEqual(new[]{1,0,0,0}))Check("nonzeroMassHandAnchor");
  int axis=Array.FindIndex(multi,x=>x==3);var pair=MetricPairs()[metric];if(axis>=0&&(pair.I==axis||pair.J==axis))Check("pureBaseDiffeomorphismPrincipalZero");
  if(metric==0&&(multi.SequenceEqual(new[]{1,0,0,0})||multi.SequenceEqual(new[]{0,2,0,0})||multi.SequenceEqual(new[]{0,3,0,0})))AddWardCallbacks(c,point,jet);
  c.Add(new("EndGerm","$"));return Plan($"point{point}/m{metric}_j{jet}",c);
 }
 static void AddWardCallbacks(List<MixedPlannedCallback> c,int point,int jet)
 {
  void Check(string name)=>c.Add(new("Check",name));
  for(int eta=0;eta<2;eta++)for(int route=0;route<2;route++)
  {
   string name=$"p{point}_j{jet}_eta{eta}_route{route}";
   c.Add(new("Ward",name+"_epsilon"));c.Add(new("Ward",name+"_compensated"));c.Add(new("Acceleration",name));c.Add(new("Original",name+"_fixedNativeTangent"));
   foreach(string variant in new[]{"compensated","epsilon"})
   {
    string tag=name+"_"+variant;foreach(string field in new[]{"densityValue","densityMetric","densityField","densityMixed","descendedDerivative","curvatureSquare","curvatureVariation","solderFirst","solderOuter","solderInner"})Check(tag+"_"+field);
   }
   Check(name+"_accelerationExteriorJet");for(int p=0;p<4;p++)foreach(string kind in new[]{"compensatedWard","epsilonMixedWard","accelerationGreen"})Check(name+"_"+kind+"_"+p);
   if(route==0)continue;
   foreach(string variant in new[]{"E","C"})
   {
    Check(name+"_primitiveMenu"+variant);for(int p=0;p<33;p++)Check(name+"_wordPrimitive_"+variant+p);string tag=name+"_wordDensity_"+variant;
    for(int density=0;density<2;density++)
    {
     for(int p=0;p<3;p++)for(int s=0;s<8;s++)Check($"{tag}_{density}_p{p}_s{s}");for(int p=0;p<4;p++)Check($"{tag}_{density}_top{p}");Check($"{tag}_{density}_scalars");
    }
   }
   for(int p=0;p<3;p++)for(int s=0;s<8;s++)Check($"{name}_wordField_p{p}_s{s}");
   foreach(string kind in new[]{"wordFieldScalars","wordAcceleration","wordAccelerationTensors"})Check(name+"_"+kind);
   for(int mu=0;mu<14;mu++)Check($"{name}_wordAccelerationJet{mu}");for(int p=0;p<2;p++)for(int s=0;s<8;s++)Check($"{name}_wordAcceleration_p{p}_s{s}");
  }
 }
 // Child menus describe the FULL structured callback fields. Expanded is
 // deliberately null here: these are object-field schemas, not a newly chosen
 // storage policy. The retention contract must separately pin expansion/DAG
 // policy for each field; every tensor must still be reconstructed completely.
 public static MixedContextPlan Structured(string category)
 {
  var c=new List<MixedPlannedCallback>();void Tensor(string name,int degree)=>c.Add(new("Tensor",name,degree));void ScalarArray(string name,int length)=>c.Add(new("ScalarArray",name,Length:length));
  void Bi(string prefix,int degree){foreach(string part in new[]{"Value","H","U","HU"})Tensor(prefix+"/"+part,degree);}
  void Stages(string prefix,bool bi,int pieces){for(int p=0;p<pieces;p++)for(int s=0;s<8;s++)if(bi)Bi($"{prefix}/{p}/{s}",ForwardDegrees[s]);else Tensor($"{prefix}/{p}/{s}",ForwardDegrees[s]);}
  void Scalars(string prefix){foreach(string name in new[]{"Value","Metric","Field","Mixed"})ScalarArray(prefix+name,4);}
  if(category=="Original"){Stages("Stages",true,3);Scalars("");}
  else if(category=="Ward")
  {
   c.Add(new("Boolean","Compensated"));c.Add(new("Boolean","Oracle"));Tensor("Eta",0);foreach(string name in new[]{"NativeDeta","Deta","DBeta","Tangent"})Tensor(name,1);Tensor("DBTangent",2);
   string[] primitives=["epsilon","inverse","epsilonInverse","inverseEpsilon","referenceCurvature","DBepsilon","DBinverse","DBSquaredEpsilon","DBSquaredEpsilonExpanded","curvatureVariationCommutator","gaugeDifference","DBgaugeDifference","varpi","DBvarpi","T","DBT","rotatedCurvature","covariantT","quadraticT","unrotatedPhiFirst","unrotatedPhiOuter","unrotatedPhiInner","phiFirst","phiOuter","phiInner","descendedT","descendedDerivative","descendedDerivativeOracle","descendedCurvature","descendedQuadratic","descendedPhiFirst","descendedPhiOuter","descendedPhiInner"];
   int[] degrees=[0,0,0,0,2,1,1,2,2,2,1,2,1,2,1,2,2,2,2,1,1,2,1,1,2,1,2,2,2,2,1,1,2];
   for(int p=0;p<33;p++)Bi("Primitives/"+primitives[p],degrees[p]);
   foreach(string density in new[]{"Literal","Descended"}){Stages(density+"/Stages",true,3);for(int p=0;p<4;p++)Bi($"{density}/TopForms/{p}",14);Scalars(density+"/");}
  }
  else if(category=="Acceleration")
  {
   c.Add(new("Boolean","Oracle"));Tensor("Eta",0);Tensor("NativeDeta",1);Tensor("W",1);foreach(string name in new[]{"Dw","DBw","DwFromJet","DBwFromJet","CrossQ"})Tensor(name,2);
   foreach(string family in new[]{"PartialW","CovariantW","DeltaBPartialInFrame","CovariantAdjointX"})for(int mu=0;mu<14;mu++)Tensor(family+"/"+mu,family=="CovariantAdjointX"?2:1);
   Stages("Stages",false,2);foreach(string name in new[]{"Original","Euler"})ScalarArray(name,4);foreach(string name in new[]{"Current","CoordinateCurrent"})ScalarArray(name,14);for(int mu=0;mu<14;mu++)ScalarArray("CurrentCovariantDerivative/"+mu,14);c.Add(new("Scalar","Divergence"));
  }
  else throw new InvalidOperationException("A68 plan: unknown structured category");return Plan("structured/"+category,c);
 }
}

// Exact set census; metadata order is NOT asserted to be execution order.
// A failure permanently poisons this instance, including a false scientific
// check. A caller cannot catch a failure, supply missing fields and succeed.
internal sealed class MixedCallbackCensus
{
 readonly Dictionary<(string Category,string Name),MixedPlannedCallback> expected;
 readonly HashSet<(string Category,string Name)> seen=[];bool failed,finished;
 public string Id{get;}
 public MixedCallbackCensus(MixedContextPlan plan)
 {
  Id=plan.Id;expected=[];foreach(var entry in plan.Callbacks)
  {
   if(entry.Category=="ScalarArray"?entry.Length is not (4 or 14):entry.Length is not null)throw new InvalidOperationException("invalid planned scalar-array length metadata");
   if(!expected.TryAdd((entry.Category,entry.Name),entry))throw new InvalidOperationException("duplicate planned callback");
  }
 }
 void Need(bool condition,string message){if(!condition){failed=true;throw new InvalidOperationException($"A68 census {Id}: {message}");}}
 public void Accept(string category,string name,int? degree=null,bool? expanded=null,int? length=null)
 {Need(category!="Check","scientific checks require the boolean Check method");Need(category is not ("Ward" or "Original" or "Acceleration"),"structured callbacks require complete child census");AcceptCore(category,name,degree,expanded,length);}
 void AcceptCore(string category,string name,int? degree=null,bool? expanded=null,int? length=null)
 {
  Need(!failed&&!finished,"failed or closed context");var key=(category,name);Need(expected.TryGetValue(key,out var wanted),"unexpected callback "+category+"/"+name);
  Need(wanted!.Degree==degree&&wanted.Expanded==expanded&&wanted.Length==length,"callback degree/expansion/length mismatch");Need(seen.Add(key),"duplicate callback "+category+"/"+name);
 }
 public void Check(string name,bool passed){Need(passed,"scientific check failed: "+name);AcceptCore("Check",name);}
 public void AcceptStructured(string category,string name,IEnumerable<MixedPlannedCallback> fields)
 {
  try{var child=new MixedCallbackCensus(MixedAuditPlan.Structured(category));foreach(var field in fields)child.Accept(field.Category,field.Name,field.Degree,field.Expanded,field.Length);child.Finish();AcceptCore(category,name);}
  catch{failed=true;throw;}
 }
 public void OnlyRemaining(params string[] categories)
 {Need(!failed&&!finished,"failed or closed context");Need(expected.Keys.Where(k=>!seen.Contains(k)).All(k=>categories.Contains(k.Category,StringComparer.Ordinal)),"earlier-stage callback missing");}
 public void Finish()
 {Need(!failed&&!finished,"failed or closed context");Need(seen.Count==expected.Count,"missing callbacks: "+string.Join(",",expected.Keys.Where(k=>!seen.Contains(k)).Select(k=>k.Category+"/"+k.Name)));finished=true;}
}

// Lifecycle closure across all2*350 contexts. Callbacks inside the current
// scope use Current; only this wrapper consumes its begin/end lifecycle names.
internal sealed class MixedAuditRunCensus
{
 MixedCallbackCensus? point,germ;int nextPoint,nextGerm;bool failed,finished,pointSealed,sealing;
 void Need(bool ok,string message){if(!ok){failed=true;throw new InvalidOperationException("A68 run census: "+message);}}
 public MixedCallbackCensus Current{get{Need(!failed&&!finished&&!sealing&&point is not null,"no active context or reentrant seal");return germ??point!;}}
 public void BeginPoint(int index)
 {Need(!failed&&!finished&&!sealing&&point is null&&index==nextPoint&&index<2,"point order/state");point=new(MixedAuditPlan.Point(index));point.Accept("BeginPoint","$");nextGerm=0;pointSealed=false;}
 // Callback performs the separately admitted retained write+independent
 // numerical replay. A callback returning is NOT by itself scientific proof;
 // the sink's mandatory reviewed prerequisite must bind the actual receipt.
 public void SealPointBackground(Action sealAndValidate)
 {
  Need(!failed&&!finished&&!sealing&&point is not null&&germ is null&&nextGerm==0&&!pointSealed&&sealAndValidate is not null,"one pre-child background seal");
  try
  {
   point!.OnlyRemaining("SealPointBackground","Germ","EndPoint");sealing=true;sealAndValidate!();
   Need(!failed,"swallowed reentrant seal failure");point.Accept("SealPointBackground","$");pointSealed=true;
  }
  catch{failed=true;throw;}finally{sealing=false;}
 }
 public void BeginGerm(int metric,int jet)
 {
  Need(!failed&&!finished&&!sealing&&pointSealed&&point is not null&&germ is null&&metric==nextGerm/35&&jet==nextGerm%35&&nextGerm<350,"germ order/state requires sealed background");
  try{point!.OnlyRemaining("Germ","EndPoint");germ=new(MixedAuditPlan.Germ(nextPoint,metric,jet));germ.Accept("BeginGerm","$");}catch{failed=true;throw;}
 }
 public void EndGerm()
 {
  Need(!failed&&!finished&&!sealing&&pointSealed&&point is not null&&germ is not null,"missing active germ");
  try{germ!.Accept("EndGerm","$");germ.Finish();point!.Accept("Germ",$"m{nextGerm/35}_j{nextGerm%35}");germ=null;nextGerm++;}catch{failed=true;throw;}
 }
 public void EndPoint()
 {
  Need(!failed&&!finished&&!sealing&&pointSealed&&point is not null&&germ is null&&nextGerm==350,"point requires sealed background and all350 germs");
  try{point!.Accept("EndPoint","$");point.Finish();point=null;nextPoint++;}catch{failed=true;throw;}
 }
 public void Finish(){Need(!failed&&!finished&&!sealing&&point is null&&germ is null&&nextPoint==2,"requires both complete points");finished=true;}
}
