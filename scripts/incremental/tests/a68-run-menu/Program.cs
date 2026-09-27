// Standalone manufactured METADATA tests only. Synthetic Check(true) values
// exercise census state transitions, not any numerical scientific predicate.
internal static class Program
{
 static void Require(bool condition,string reason)
 {if(!condition)throw new InvalidOperationException("metadata test: "+reason);}
 static void Reject(Action action,string reason)
 {
  try{action();}catch(InvalidOperationException){return;}
  throw new InvalidOperationException("expected rejection: "+reason);
 }
 static void Feed(MixedCallbackCensus census,MixedPlannedCallback entry)
 {
  if(entry.Category=="Check")census.Check(entry.Name,true);
  else census.Accept(entry.Category,entry.Name,entry.Degree,entry.Expanded,entry.Length);
 }
 static void ContextMenu()
 {
  var ids=MixedAuditPlan.RunContextIds();Require(ids.Length==705&&MixedAuditPlan.RunContextCount==705,"705 exact IDs");
  Require(ids.Distinct(StringComparer.Ordinal).Count()==705,"unique IDs");
  Require(ids.Count(id=>id.StartsWith("diagnostic/",StringComparison.Ordinal))==3,"three separate diagnostic contexts");
  foreach(string diagnostic in new[]{"grade10","acceleration","secondJets"})Require(ids.Contains("diagnostic/"+diagnostic,StringComparer.Ordinal),"dedicated "+diagnostic);
  for(int point=0;point<2;point++)
  {
   Require(ids.Contains("point"+point,StringComparer.Ordinal),"point baseline");
   Require(ids.Count(id=>id.StartsWith($"point{point}/",StringComparison.Ordinal))==350,"unchanged350 germs per point");
   for(int metric=0;metric<10;metric++)for(int jet=0;jet<35;jet++)Require(ids.Contains($"point{point}/m{metric}_j{jet}",StringComparer.Ordinal),"complete germ ID");
  }
  MixedAuditPlan.ValidateRunContextIds(ids);MixedAuditPlan.ValidateRunContextIds(ids.Reverse());
 }
 static void ContextRejections()
 {
  var ids=MixedAuditPlan.RunContextIds();
  Reject(()=>MixedAuditPlan.ValidateRunContextIds(ids.Where(id=>id!="diagnostic/secondJets")),"old704 set omits required secondJets");
  Reject(()=>MixedAuditPlan.ValidateRunContextIds(ids.Append("diagnostic/secondJets")),"duplicate/excess context");
  var duplicate=ids.ToArray();duplicate[^1]=duplicate[0];Reject(()=>MixedAuditPlan.ValidateRunContextIds(duplicate),"duplicate replacing required context");
  var wrong=ids.ToArray();wrong[^1]="diagnostic/grade10/secondJets";Reject(()=>MixedAuditPlan.ValidateRunContextIds(wrong),"silently embedded secondJets owner");
  wrong[^1]="diagnostic/secondjets";Reject(()=>MixedAuditPlan.ValidateRunContextIds(wrong),"case-sensitive exact context");
  wrong[^1]="point0/m10_j0";Reject(()=>MixedAuditPlan.ValidateRunContextIds(wrong),"extra scientific metric row");
 }
 static void SecondJetPlan()
 {
  var plan=MixedAuditPlan.SecondJets();var names=MixedAuditPlan.SecondJetNames();
  Require(plan.Id=="diagnostic/secondJets","dedicated callback-plan ID");Require(plan.Callbacks.Length==842,"Begin+420*(SecondJet+Check)+End");
  Require(names.Length==420&&names.Distinct(StringComparer.Ordinal).Count()==420,"unique420 image names");
  Require(plan.Callbacks[0]==new MixedPlannedCallback("Begin","$"),"normalized Begin");
  Require(plan.Callbacks[^1]==new MixedPlannedCallback("End","$"),"normalized End");int i=0,diagonal=0,offDiagonal=0;
  for(int coefficient=0;coefficient<2;coefficient++)for(int first=0;first<14;first++)for(int second=first;second<14;second++)for(int route=0;route<2;route++)
  {
   string name=$"eta{coefficient}_second_{first}_{second}_route{route}";Require(names[i]==name,"exact coefficient/slot/route order");
   Require(plan.Callbacks[1+2*i]==new MixedPlannedCallback("SecondJet",name,2),"complete degree-two dedicated callback");
   Require(plan.Callbacks[2+2*i]==new MixedPlannedCallback("Check",name),"same-name zero check immediately follows");
   if(first==second)diagonal++;else offDiagonal++;i++;
  }
  Require(i==420&&diagonal==56&&offDiagonal==364,"all14 diagonal+91 off-diagonal slots in four routes");
  Require(plan.Callbacks.All(c=>c.Expanded is null),"retention policy remains independently explicit, not weakened here");
 }
 static void CompleteSecondJetCensus()
 {
  var plan=MixedAuditPlan.SecondJets();var census=new MixedCallbackCensus(plan);
  foreach(var entry in plan.Callbacks)Feed(census,entry);census.Finish();
  Reject(()=>census.Accept("Begin","$"),"closed context cannot restart");
 }
 static void OmissionPoisonsCensus()
 {
  var plan=MixedAuditPlan.SecondJets();var census=new MixedCallbackCensus(plan);var missing=plan.Callbacks[212];
  foreach(var entry in plan.Callbacks)if(entry!=missing)Feed(census,entry);
  Reject(census.Finish,"missing a late zero check");Reject(()=>Feed(census,missing),"cannot repair failed census");
  var noEnd=new MixedCallbackCensus(plan);foreach(var entry in plan.Callbacks[..^1])Feed(noEnd,entry);
  Reject(noEnd.Finish,"End is mandatory");
 }
 static void DuplicateAndWrongCategory()
 {
  var plan=MixedAuditPlan.SecondJets();var first=plan.Callbacks[1];
  var duplicatePlan=new MixedContextPlan(plan.Id,[..plan.Callbacks,first]);Reject(()=>new MixedCallbackCensus(duplicatePlan),"duplicate planned callback");
  var duplicate=new MixedCallbackCensus(plan);Feed(duplicate,first);Reject(()=>Feed(duplicate,first),"duplicate observed callback");
  var wrongCategory=new MixedCallbackCensus(plan);Reject(()=>wrongCategory.Accept("Tensor",first.Name,2),"SecondJet is not an ordinary Tensor callback");
  var wrongName=new MixedCallbackCensus(plan);Reject(()=>wrongName.Accept("SecondJet","eta0_second_14_14_route0",2),"outside105-slot menu");
 }
 static void WrongTypeAndFalseCheck()
 {
  var plan=MixedAuditPlan.SecondJets();string name=plan.Callbacks[1].Name;var type=new MixedCallbackCensus(plan);
  Reject(()=>type.Accept("SecondJet",name,1),"wrong tensor degree");Reject(()=>type.Accept("SecondJet",name,2),"wrong-degree failure poisons context");
  var policy=new MixedCallbackCensus(plan);Reject(()=>policy.Accept("SecondJet",name,2,false),"unexpected callback expansion metadata");
  var scientific=new MixedCallbackCensus(plan);Reject(()=>scientific.Check(name,false),"synthetic false predicate rejected");Reject(()=>scientific.Check(name,true),"false predicate cannot be repaired");
  var bypass=new MixedCallbackCensus(plan);Reject(()=>bypass.Accept("Check",name),"scientific Check cannot use generic Accept");
 }
 static void ScientificMenusUnchanged()
 {
  int germs=0;
  for(int point=0;point<2;point++)
  {
   var p=MixedAuditPlan.Point(point);Require(p.Callbacks.Count(c=>c.Category=="Germ")==350,"point keeps350 children");
   Require(!p.Callbacks.Any(c=>c.Category=="SecondJet"),"secondJets not hidden in baseline");
   for(int metric=0;metric<10;metric++)for(int jet=0;jet<35;jet++)
   {var g=MixedAuditPlan.Germ(point,metric,jet);Require(!g.Callbacks.Any(c=>c.Category=="SecondJet"),"secondJets not hidden in scientific germ");germs++;}
  }
  Require(germs==700,"unchanged700 scientific germs");
  var run=new MixedAuditRunCensus();Reject(run.Finish,"context list is not a scientific run-completion certificate");
 }
 static void ReturnedMenusAreIndependent()
 {
  var ids=MixedAuditPlan.RunContextIds();ids[0]="corrupt";Require(MixedAuditPlan.RunContextIds()[0]=="point0","fresh context metadata");
  var names=MixedAuditPlan.SecondJetNames();names[0]="corrupt";Require(MixedAuditPlan.SecondJetNames()[0]=="eta0_second_0_0_route0","fresh name metadata");
  var plan=MixedAuditPlan.SecondJets();plan.Callbacks[0]=new("corrupt","$");Require(MixedAuditPlan.SecondJets().Callbacks[0].Category=="Begin","fresh callback metadata");
 }
 static void PointPrefix(MixedAuditRunCensus run,int point)
 {
  run.BeginPoint(point);
  foreach(var entry in MixedAuditPlan.Point(point).Callbacks)
   if(entry.Category is not ("BeginPoint" or "SealPointBackground" or "Germ" or "EndPoint"))Feed(run.Current,entry);
 }
 static void PointSealMenu()
 {
  for(int point=0;point<2;point++)
  {
   var entries=MixedAuditPlan.Point(point).Callbacks;int at=Array.FindIndex(entries,e=>e.Category=="SealPointBackground");
   Require(at==(point==0?21:18),"seal follows all background observations and anchors");
   Require(entries.Count(e=>e.Category=="SealPointBackground")==1,"single mandatory seal");
   Require(entries[at]==new MixedPlannedCallback("SealPointBackground","$"),"exact seal identity");
   Require(entries.Skip(at+1).Count(e=>e.Category=="Germ")==350&&entries[^1].Category=="EndPoint","seal leaves full traversal pending");
  }
 }
 static void SealCannotBeSkippedOrFaked()
 {
  var skipped=new MixedAuditRunCensus();PointPrefix(skipped,0);Reject(()=>skipped.BeginGerm(0,0),"background callback prefix alone cannot open germ");
  Reject(()=>skipped.SealPointBackground(()=>{}),"skipping seal permanently poisons run");
  var fake=new MixedAuditRunCensus();PointPrefix(fake,0);fake.Current.Accept("SealPointBackground","$");
  Reject(()=>fake.BeginGerm(0,0),"direct census flag is not completed seal transaction");
  var early=new MixedAuditRunCensus();early.BeginPoint(0);bool called=false;
  Reject(()=>early.SealPointBackground(()=>called=true),"missing point callbacks");Require(!called,"no sealing work before prefix census");
 }
 static void FailedOrReentrantSealPoisons()
 {
  var failed=new MixedAuditRunCensus();PointPrefix(failed,0);
  Reject(()=>failed.SealPointBackground(()=>throw new InvalidOperationException("manufactured replay failure")),"failed replay");
  Reject(()=>failed.SealPointBackground(()=>{}),"no retry after failed replay");Reject(()=>failed.BeginGerm(0,0),"no child after failed replay");
  var nested=new MixedAuditRunCensus();PointPrefix(nested,0);
  Reject(()=>nested.SealPointBackground(()=>Reject(()=>nested.BeginGerm(0,0),"nested germ")),"swallowed nested failure poisons outer seal");
  Reject(()=>nested.BeginGerm(0,0),"no child after reentry");
 }
 static void SealDoesNotCompleteTraversal()
 {
  var run=new MixedAuditRunCensus();PointPrefix(run,0);int calls=0;run.SealPointBackground(()=>calls++);
  Require(calls==1,"one manufactured validation call");Reject(run.EndPoint,"seal does not replace350 germs");
  var twice=new MixedAuditRunCensus();PointPrefix(twice,0);twice.SealPointBackground(()=>{});
  Reject(()=>twice.SealPointBackground(()=>{}),"seal cannot repeat");
 }
 static void SealedCompleteTraversal()
 {
  var run=new MixedAuditRunCensus();int seals=0,germs=0;
  for(int point=0;point<2;point++)
  {
   PointPrefix(run,point);run.SealPointBackground(()=>seals++);
   for(int metric=0;metric<10;metric++)for(int jet=0;jet<35;jet++)
   {
    run.BeginGerm(metric,jet);
    foreach(var entry in MixedAuditPlan.Germ(point,metric,jet).Callbacks)
    {
     if(entry.Category is "BeginGerm" or "EndGerm")continue;
     if(entry.Category is "Ward" or "Original" or "Acceleration")run.Current.AcceptStructured(entry.Category,entry.Name,MixedAuditPlan.Structured(entry.Category).Callbacks);
     else Feed(run.Current,entry);
    }
    run.EndGerm();germs++;
   }
   run.EndPoint();
  }
  run.Finish();Require(seals==2&&germs==700,"both complete manufactured traversals still required");
 }
 public static int Main()
 {
  (string Name,Action Test)[] tests=[("exact705 contexts and unchanged700 germs",ContextMenu),("old704/duplicate/wrong owner rejected",ContextRejections),
   ("complete842-callback secondJets plan",SecondJetPlan),("complete synthetic secondJets census",CompleteSecondJetCensus),
   ("omission and lifecycle failures poison census",OmissionPoisonsCensus),("duplicate/name/category failures",DuplicateAndWrongCategory),
   ("type/expansion/false-check failures",WrongTypeAndFalseCheck),("scientific point/germ menus unchanged",ScientificMenusUnchanged),
   ("returned menus cannot mutate future plans",ReturnedMenusAreIndependent),
   ("point seal is mandatory after computational prefix",PointSealMenu),
   ("seal cannot be skipped or replaced by a census flag",SealCannotBeSkippedOrFaked),
   ("failed and swallowed reentrant seals poison traversal",FailedOrReentrantSealPoisons),
   ("background seal does not complete point traversal",SealDoesNotCompleteTraversal),
   ("both sealed points retain all700 child lifecycles",SealedCompleteTraversal)];
  try{foreach(var (name,test) in tests){test();Console.WriteLine("PASS metadata-only: "+name);}Console.WriteLine($"{tests.Length}/{tests.Length} metadata-only tests passed; no scientific assembly or evaluator linked.");return 0;}
  catch(Exception error){Console.Error.WriteLine(error);return 1;}
 }
}
