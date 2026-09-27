using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;

// Source-MENU metadata only: no geometry construction, coefficients, hashes,
// files or scientific operators. These paths declare the existing upstream
// lineage; only independent reconstruction/replay can authenticate its values.
internal sealed record MixedSourceMark(string Name,int Degree,bool? RequiredExpanded);
internal sealed record MixedSourceTemplateBudget(string Id,MixedSinkResources Resources,long? CheckpointBytes);
internal sealed record MixedSourceRetention(bool Background,bool Geometry,bool Structured,bool SecondJets);
internal sealed record MixedSourceContextStructure(string Id,IReadOnlyList<MixedSinkLeafDeclaration> Leaves,
 IReadOnlyDictionary<string,string> LeafRoles,IReadOnlyList<MixedSourceMark> Marks,MixedContextPlan Callbacks);

internal sealed class MixedSourceContextFactory
{
 const string GeometryRoot="studies/phase621_induced_metric_full_variation_scope_audit_001/output";
 const string PolynomialRoot="studies/phase626_fixed_cubic_full_stationary_residual_certificate_001/output/chunks";
 static readonly int[] Forward=[2,12,13,14,0,1,13,1],Reverse=[1,13,2,1,0,14,12,2,2];
 static readonly string[] Pieces=["source","kinetic","cubic","mass","total"];
 readonly Dictionary<string,MixedSourceTemplateBudget> budgets;
 readonly MixedSourceRetention retention;
 static void Need(bool ok,string why){if(!ok)throw new InvalidOperationException("A68 source template: "+why);}
 public MixedSourceContextFactory(IReadOnlyList<MixedSourceTemplateBudget> budgets,MixedSourceRetention retention)
 {
  ArgumentNullException.ThrowIfNull(budgets);ArgumentNullException.ThrowIfNull(retention);
  Need(budgets.Count==705,"explicit complete705 resource budgets");var copy=budgets.ToArray();
  Need(copy.Length==705&&copy.All(b=>b is not null),"complete nonnull budget snapshot");
  MixedAuditPlan.ValidateRunContextIds(copy.Select(b=>b.Id));
  this.budgets=copy.ToDictionary(b=>b.Id,StringComparer.Ordinal);this.retention=retention;
  foreach(var budget in copy)
  {
   ArgumentNullException.ThrowIfNull(budget.Resources);ArgumentNullException.ThrowIfNull(budget.Resources.Trace);MixedTrace.CheckLimits(budget.Resources.Trace);
   var r=budget.Resources;Need(r.FileBytes>0&&r.ContextBytes>0&&r.MetadataBytes>0&&r.FailureGraphBytes>0,"positive context resources");
   bool point=budget.Id is "point0" or "point1";Need(point==(budget.CheckpointBytes is not null),"checkpoint budget iff point");
   if(point)Need(budget.CheckpointBytes>0&&budget.CheckpointBytes<=r.FileBytes&&budget.CheckpointBytes<=r.ContextBytes,"checkpoint within file/context caps");
  }
 }
 public MixedSinkContextTemplate Create(string id)
 {
  Need(budgets.TryGetValue(id,out var budget),"declared resource context");var structure=Describe(id);
  // Ordinal position in the sorted COMPLETE name set, never observed support,
  // gives a portable collision-free tensor path even for names containing [].
  var marks=structure.Marks.Select((mark,index)=>
  {
   bool expanded=mark.RequiredExpanded??(mark.Name.StartsWith("structured/",StringComparison.Ordinal)?retention.Structured:
    mark.Name.StartsWith("secondJet/",StringComparison.Ordinal)?retention.SecondJets:
    mark.Name.StartsWith("background/",StringComparison.Ordinal)?retention.Background:retention.Geometry);
   return new MixedSinkMark(mark.Name,mark.Degree,expanded,expanded?id+"/tensors/t"+index.ToString("D6",CultureInfo.InvariantCulture)+".json":null);
  }).ToArray();
  return new(id,id+"/graph.json",id+"/metadata.json",budget!.Resources,structure.Leaves,structure.LeafRoles,marks,structure.Callbacks,
   budget.CheckpointBytes is long bytes?new(id+"/background-checkpoint.json",bytes):null);
 }
 public static MixedSourceContextStructure Describe(string id)
 {
  var match=Regex.Match(id??"",@"\Apoint([01])(?:/m([0-9])_j(0|[1-9]|[12][0-9]|3[0-4]))?\z",RegexOptions.CultureInvariant);
  Need(match.Success||id is "diagnostic/grade10" or "diagnostic/acceleration" or "diagnostic/secondJets","exact closed source context ID");
  var marks=new List<MixedSourceMark>();var leaves=new List<MixedSinkLeafDeclaration>();var roles=new Dictionary<string,string>(StringComparer.Ordinal);
  MixedContextPlan callbacks;
  void Mark(string name,int degree,bool? expanded=null)=>marks.Add(new(name,degree,expanded));
  void Tensor(string name,int degree,bool expanded=true)=>Mark("tensor/"+name,degree,expanded);
  void Leaf(string role,int degree,string source){string key=id+"/"+role;leaves.Add(new(key,degree,source));roles.Add(role,key);}
  void Curvature(string role)=>Leaf("geometry/"+role,2,GeometryRoot+"/induced_metric_full_variation_scope_audit_summary.json#independent-source-spin-curvature");
  void VariationLeaves(string prefix,int point,int metric,int jet)
  {
   string source=GeometryRoot+"/shards/jet_p"+point+"_m"+metric.ToString("D2",CultureInfo.InvariantCulture)+"_j"+jet.ToString("D2",CultureInfo.InvariantCulture)+".json#independent-source-spin-curvature-variation";
   foreach(string route in new[]{"deltaCurvatureAdapted","deltaCurvatureOracle"})Leaf("geometry/"+prefix+route,2,source);
  }
  void Background()
  {
   foreach(var role in MixedAuditPlan.BaselineRoles())Mark("background/"+role.Role,role.Degree);
   foreach(var item in new[]{("NativeX",1),("DXOracle",2),("ReverseX",1),("Gradient",1)})Mark("background/"+item.Item1,item.Item2);
   for(int mu=0;mu<14;mu++)foreach(string name in new[]{"NativePartial","NativePartialOracle","CovariantCoordinate"})Mark($"background/{name}[{mu}]",1);
  }
  void Geometry(string prefix)
  {foreach(string name in new[]{"DeltaB","DeltaFAdapted","DeltaFOracle","DeltaFFixed","DeltaBExterior","DeltaCurvatureFromConnection"})Mark(prefix+"/"+name,name=="DeltaB"?1:2);}
  void Structured(string category,string name)
  {foreach(var field in MixedAuditPlan.Structured(category).Callbacks)if(field.Category=="Tensor")Mark($"structured/{category}/{name}/{field.Name}",field.Degree!.Value);}
  if(match.Success)
  {
   int point=int.Parse(match.Groups[1].Value,CultureInfo.InvariantCulture);
   if(!match.Groups[2].Success)
   {
    callbacks=MixedAuditPlan.Point(point);Background();
    Leaf("input/X",1,PolynomialRoot+$"/p{point}_kinetic_X_inputs_g000.json#independent-five-order-reconstruction");Curvature("curvature");
   }
   else
   {
    int metric=int.Parse(match.Groups[2].Value,CultureInfo.InvariantCulture),jet=int.Parse(match.Groups[3].Value,CultureInfo.InvariantCulture);
    callbacks=MixedAuditPlan.Germ(point,metric,jet);Geometry("geometry");
    foreach(var role in MixedAuditPlan.BaselineRoles())
    {
     string key="baseline/"+role.Role,canonical="baseline/"+role.CanonicalId;
     if(key==canonical)Leaf(key,role.Degree,$"verified-point-replay/point{point}/"+role.CanonicalId);
     else roles.Add(key,id+"/"+canonical);
    }
    VariationLeaves("",point,metric,jet);
    foreach(var entry in callbacks.Callbacks)
    {
     if(entry.Category=="Tensor")Tensor(entry.Name,entry.Degree!.Value,entry.Expanded!.Value);
     else if(entry.Category is "Ward" or "Original" or "Acceleration")Structured(entry.Category,entry.Name);
    }
   }
  }
  else if(id=="diagnostic/secondJets")
  {callbacks=MixedAuditPlan.SecondJets();foreach(string name in MixedAuditPlan.SecondJetNames())Mark("secondJet/"+name,2);}
  else
  {
   bool grade=id=="diagnostic/grade10";var menu=grade?MixedDiagnostics.GradeTenMenu():MixedDiagnostics.AccelerationMenu();
   Tensor("input/X",1);Background();Curvature("background/curvature");
   if(grade)
   {
    Tensor("input/UFrame",1);Tensor("input/UNative",1);Tensor("input/dUNative",2);
    string[] families=["raw0","raw2","fieldFirst0","fieldFirst2","word0","word2","eulerCovariant","eulerMoving","native0","nativeEuler"];
    for(int basis=0;basis<4;basis++)
    {
     string prefix="basis"+basis;Geometry(prefix);VariationLeaves(prefix+"/",0,basis,4);
     for(int p=0;p<3;p++)for(int s=0;s<8;s++)foreach(string part in new[]{"value","delta"})Tensor($"{prefix}/intermediate/fixed_p{p}_s{s}_{part}",Forward[s],false);
     for(int s=0;s<9;s++)foreach(string part in new[]{"value","delta"})Tensor($"{prefix}/intermediate/reverse_s{s}_{part}",Reverse[s],false);
     for(int mu=0;mu<14;mu++)foreach(string kind in new[]{"covariant_z","covariantOracle_z","partial_mu"})Tensor($"{prefix}/intermediate/{kind}{mu}",2,false);
     for(int f=0;f<families.Length;f++)foreach(string piece in Pieces)Tensor($"{prefix}/{families[f]}/{piece}",f is 1 or 3 or 5?2:1);
     string[] singles=["fixedAdjoint","wordAdjoint","movingAdjointDelta","ordinaryAdjoint","native2","nativeDivergence","nativeAdjointAsFrame"];
     for(int s=0;s<singles.Length;s++)Tensor(prefix+"/"+singles[s],s is 0 or 1 or 2 or 4?2:1);
     for(int mu=0;mu<14;mu++)for(int nu=0;nu<14;nu++)foreach(string kind in new[]{"current","currentPartial","green"})Tensor($"{prefix}/{kind}/{mu}/{nu}",0,kind!="currentPartial");
    }
    for(int f=0;f<8;f++)foreach(string piece in Pieces)Tensor($"combined/{families[f]}/{piece}",f is 1 or 3 or 5?2:1);
    string[] hand=["motionX","crossQ","motionCrossQ","adjointMotionX","wordAdjointMotionX","fixedAdjoint","movingAdjointDelta","pairingMotionAdjoint","adjointPhi1","pairingMotionAdjointPhi1"];
    for(int i=0;i<hand.Length;i++)Tensor("combined/"+hand[i],i==0?1:2);
    for(int s=0;s<8;s++)foreach(string route in new[]{"literal","word"})Tensor($"forwardHand/{route}/{s}",Forward[s]);
   }
   else{Tensor("input/eta",0);Tensor("input/dEta",1);Geometry("germ");VariationLeaves("germ/",1,0,10);}
   var c=new List<MixedPlannedCallback>{new("Begin","$"),new("Background","background")};
   foreach(var leaf in roles.Keys)c.Add(new("GeometryLeaf",leaf["geometry/".Length..],2));
   foreach(string name in grade?new[]{"basis0","basis1","basis2","basis3"}:new[]{"germ"})c.Add(new("Geometry",name));
   var byName=marks.Where(m=>m.Name.StartsWith("tensor/",StringComparison.Ordinal)).ToDictionary(m=>m.Name[7..],StringComparer.Ordinal);
   Need(byName.Count==menu.Tensors.Length&&menu.Tensors.All(byName.ContainsKey),"complete diagnostic tensor callback coverage");
   foreach(string name in menu.Tensors){var mark=byName[name];c.Add(new("Tensor",name,mark.Degree,mark.RequiredExpanded));}
   foreach(string name in menu.ScalarArrays)c.Add(new("ScalarArray",name,Length:4));
   foreach(var group in new[]{("Original",menu.OriginalActions),("Ward",menu.WardActions),("Acceleration",menu.Accelerations)})
    foreach(string name in group.Item2){c.Add(new(group.Item1,name));Structured(group.Item1,name);}
   foreach(string name in menu.Checks)c.Add(new("Check",name));c.Add(new("End","$"));callbacks=new(id!,c.ToArray());
  }
  Need(marks.Select(m=>m.Name).Distinct(StringComparer.Ordinal).Count()==marks.Count,"unique complete source mark menu");
  Need(callbacks.Callbacks.Select(c=>(c.Category,c.Name)).Distinct().Count()==callbacks.Callbacks.Length,"unique callback census");
  return new(id!,Array.AsReadOnly(leaves.ToArray()),new ReadOnlyDictionary<string,string>(roles),
   Array.AsReadOnly(marks.OrderBy(m=>m.Name,StringComparer.Ordinal).ToArray()),callbacks);
 }
}
