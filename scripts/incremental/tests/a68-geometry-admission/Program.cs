using System.Numerics;
using static MixedGeometryAdmission;

// Metadata reservations and manufactured2D validators/factories ONLY.
// Never execute a source constructor or linked geometry algorithm. Wrapper
// refusal tests use insufficient quotas and null sentinels, never real inputs.
static class Program
{
 static readonly MixedProducerAdmission.Limits Limits=new(4096,16384,1000000,100000000,1000000,1000000,1000000,1000000,1000000,128,1000000,new(1000000,1000000,1000000,1000000,1000000,1000000,10000000,1000000,10000000,1000000),new(64,100000,1000000,1000000000,1000000,1000000000));
 static int tests;
 static void Need(bool value,string message){if(!value)throw new Exception(message);}
 static void Test(string name,Action action){action();tests++;Console.WriteLine("PASS "+name);}
 static void Refuses(Action action){try{action();}catch(MixedProducerAdmission.Refused){return;}throw new Exception("expected refusal");}
 // Independent integer-only loop enumeration of the linked method's explicit
 // array accesses. No geometry, Dual arithmetic or scientific coefficients.
 static Plan EnumerateDualScratch(int n)
 {
  long slots=0,arrays=2,visits=0;
  for(int a=0;a<n;a++)for(int b=0;b<n;b++)
  {slots++;visits+=1+3;for(int z=0;z<n;z++){slots++;visits++;}}
  for(int z=0;z<n;z++)for(int a=0;a<n;a++)for(int b=0;b<n;b++)for(int i=0;i<n;i++)
  {visits++;for(int j=0;j<n;j++)visits+=1+4+4;}
  for(int a=0;a<n;a++)for(int b=0;b<n;b++)for(int c=0;c<n;c++)
  {
   arrays++;for(int z=0;z<n;z++){slots++;visits++;}
   for(int l=0;l<n;l++)
   {visits+=12+1+1;for(int z=0;z<n;z++)visits+=12+1+3+1+3;}
   visits+=4;for(int z=0;z<n;z++)visits+=6;
  }
  for(int a=0;a<n;a++)for(int b=0;b<n;b++)for(int c=0;c<n;c++)for(int e=0;e<n;e++)
  {visits+=8;for(int l=0;l<n;l++)visits+=16;visits+=2;}
  return new(slots,arrays,visits,0,0);
 }
 static Plan EnumerateLinearizedBody(int n)
 {
  long slots=0,visits=0;
  for(int z=0;z<n;z++){slots+=2;visits+=2+2+1+3;}
  for(int a=0;a<n;a++)
  {
   for(int b=0;b<n;b++)for(int l=0;l<n;l++)visits+=2*(3*2+1);
   visits++;
   for(int z=0;z<n;z++)
   {for(int b=0;b<n;b++)for(int l=0;l<n;l++)visits+=2*(3*2+1);visits+=2+1;}
  }
  for(int a=0;a<n;a++)for(int b=0;b<n;b++)
  {visits+=2+4;for(int c=0;c<n;c++)for(int d=0;d<n;d++)visits+=2;}
  return new(slots,2,visits,0,0);
 }
 static long EnumerateMetricJetsBody(int n,bool blocks)
 {
  long visits=0;
  void Transpose(){for(int i=0;i<n;i++)for(int j=0;j<n;j++)visits+=2;}
  if(!blocks)
  {Transpose();for(int z=0;z<n;z++){visits+=2+1;Transpose();Transpose();for(int w=0;w<n;w++){visits+=6+1;Transpose();Transpose();Transpose();Transpose();}}}
  else for(int i=0;i<4&&i<n;i++)for(int a=4;a<n;a++)for(int b=4;b<n;b++)
  {
   visits+=2+1+1+2;
   for(int z=0;z<n;z++)
   {
    visits+=3+2+1+1+2+4;
    for(int w=0;w<n;w++)visits+=3+2+1+2+2+2+2+1+2+4;
   }
  }
  return visits;
 }
 static Plan EnumerateDownstairsBody(int extra)
 {
  long slots=4,arrays=1,visits=8; // four-element result initialization/stores
  void Matches(int length)
  {
   arrays++;slots+=4;
   for(int i=0;i<4;i++)visits++; // counts initialization
   for(int i=0;i<length;i++)visits+=3; // derivative read, counts read/write
   for(int i=0;i<4;i++)visits+=2; // logical full SequenceEqual inputs
  }
  Matches(0);
  for(int i=0;i<4;i++)for(int j=0;j<4;j++)for(int k=0;k<4;k++)for(int l=0;l<4;l++)
  {
   for(int term=0;term<3;term++)
   {
    arrays++;slots+=extra+1;
    for(int a=0;a<=extra;a++)visits+=2; // append result zero-init/store
    for(int a=0;a<extra;a++)visits++; // original extra reads
    Matches(extra+1);
   }
   visits+=3+2+3; // dense m reads, inverse/dinverse, r reference+cell read/write
  }
  return new(slots,arrays,visits,0,0);
 }
 static Plan EnumerateShearJetsBody()
 {
  Plan result=new(0,0,0,0,0);
  void Downstairs(int extra)
  {
   if(extra>0)result+=new Plan(extra,1,2*extra,0,0);
   result+=EnumerateDownstairsBody(extra);
  }
  void Shear()
  {
   long visits=0;
   for(int i=0;i<4;i++)
   {
    visits+=2;
    for(int a=0;a<4;a++)for(int b=0;b<4;b++)visits+=2;
    for(int a=0;a<10;a++)visits+=2;
   }
   result+=new Plan(0,0,visits,0,0);
  }
  Downstairs(0);Shear();
  for(int z=0;z<14;z++)
  {
   result+=new Plan(0,0,1,0,0); // D store
   if(z<4)Downstairs(1);else result+=new Plan(0,0,1,0,0); // Vertical read
   Shear();
   for(int w=0;w<14;w++)
   {
    result+=new Plan(0,0,1,0,0); // DD store even for the zero Matrix branch
    if(z<4&&w<4){Downstairs(2);Shear();}
    else if(z<4||w<4){result+=new Plan(0,0,1,0,0);Downstairs(1);Shear();}
   }
  }
  return result;
 }
 static Plan EnumerateAmbientConstructor(bool inverse)
 {
  long slots=0,visits=0;
  for(int i=0;i<14;i++)for(int a=0;a<14;a++)for(int b=0;b<14;b++)
  {slots++;visits++;for(int j=0;j<14;j++){slots++;visits++;}}
  for(int a=0;a<4;a++)for(int b=0;b<4;b++)visits+=2;
  for(int a=0;a<10;a++)for(int b=0;b<10;b++)visits+=2;
  for(int x=0;x<10;x++)
  {
   visits++; // Vertical reference for either dh branch
   for(int a=0;a<4;a++)for(int b=0;b<4;b++)visits+=2;
   for(int a=0;a<10;a++)for(int b=0;b<10;b++)visits+=2;
   for(int z=0;z<10;z++)
   {
    if(inverse){visits+=4;for(int a=0;a<4;a++)for(int b=0;b<4;b++)visits+=2;}
    for(int a=0;a<10;a++)for(int b=0;b<10;b++)visits+=2;
   }
  }
  return new(slots,2,visits,2,0);
 }
 static long EnumerateBaselineCopies(int n)
 {
  long visits=0;
  for(int i=0;i<n;i++)for(int a=0;a<n;a++)for(int b=0;b<n;b++)
  {visits+=3;for(int j=0;j<n;j++)visits+=3;}
  return visits;
 }
 static void Main()
 {
  Test("generic dimension costs are metadata and fully deterministic",()=>
  {Need(Costs(Stage.EndomorphismFrame,2)==new Plan(16,1,688,0,0),"endomorphism hand count");Need(Costs(Stage.Lower,2)==new Plan(16,1,288,0,0),"lower hand count");Need(Costs(Stage.LoweredFrame,2)==new Plan(320,68,2048,0,0),"four full slots plus index arrays");Need(Costs(Stage.LoweredFrame,1)==new Plan(20,8,88,0,0),"n1 includes q zero initialization and literal stores");});
  Test("variation costs include every nested curvature transformation",()=>
  {var frame=Costs(Stage.VariationFrame,2);Need(frame.ArraySlots==32&&frame.ArrayObjects==2&&frame.SlotVisits==1248,"effective then full frame");var low=Costs(Stage.VariationLowered,2);Need(low.ArraySlots==368&&low.ArrayObjects==71&&low.SlotVisits==3776,"three lowerings plus four-slot transform");});
  Test("fixed14 rank4 allocation census contains all index arrays",()=>
  {Need(Costs(Stage.LoweredFrame,14).ArraySlots==768320&&Costs(Stage.LoweredFrame,14).ArrayObjects==153668,"four153664-value outputs and153664 index arrays");Need(Costs(Stage.VariationLowered,14).ArraySlots==883568&&Costs(Stage.VariationLowered,14).ArrayObjects==153671,"all seven rational rank4 arrays plus indexes");});
  Test("container costs count original initializer arrays",()=>
  {Need(Costs(Stage.MetricContainer,14)==new Plan(210,2,420,4,0),"metric D DD");Need(Costs(Stage.ConnectionContainer,14)==new Plan(38626,3,38836,4,0),"connection Gamma DGamma R");});
  Test("dual scratch formula matches dense integer-only loop census for all supported dimensions",()=>
  {for(int n=1;n<=14;n++)Need(Costs(Stage.DualConnectionScratch,n)==EnumerateDualScratch(n),"dense all-branches census n="+n);Need(Costs(Stage.DualConnectionScratch,1)==new Plan(3,3,86,0,0),"n1 hand count");Need(Costs(Stage.DualConnectionScratch,2)==new Plan(28,10,2008,0,0),"n2 hand count");Need(Costs(Stage.DualConnectionScratch,14).ArraySlots==41356&&Costs(Stage.DualConnectionScratch,14).ArrayObjects==2746,"fixed14 scratch independent of result containers");});
  Test("dual scratch cannot fit by counting only Rational arrays or result containers",()=>
  {var p=Costs(Stage.DualConnectionScratch,14);using(var s=MixedProducerAdmission.Enter(Limits with{Matrices=Limits.Matrices with{ArraySlots=p.ArraySlots-1}})){Refuses(()=>Reserve(Stage.DualConnectionScratch));Need(s.Snapshot().Poisoned&&s.Snapshot().Matrices.SlotVisits==0,"scratch slots before visits");}using(var s=MixedProducerAdmission.Enter(Limits with{Matrices=Limits.Matrices with{ArrayObjects=p.ArrayObjects-1}})){Refuses(()=>Reserve(Stage.DualConnectionScratch));Need(s.Snapshot().Poisoned&&s.Snapshot().Matrices.ArraySlots==0,"per-triple arrays before allocation");}});
  Test("dual wrapper refuses combined container and scratch budget before linked algorithm",()=>
  {var c=Costs(Stage.ConnectionContainer,14);var p=Costs(Stage.DualConnectionScratch,14);using var s=MixedProducerAdmission.Enter(Limits with{Matrices=Limits.Matrices with{ArraySlots=2*c.ArraySlots+p.ArraySlots-1}});Refuses(()=>_=DualConnection(null!,null!));var r=s.Snapshot();Need(r.Poisoned&&r.Matrices.Objects==0&&r.RationalConstructions==0&&r.IntegerOperations==0,"null sentinels never reach upstream constructors or arithmetic");Need(r.Matrices.SlotVisits==2*c.SlotVisits,"scratch refused after two logical container reservations");});
  Test("dual wrapper dense work exhaustion refuses before linked algorithm",()=>
  {var c=Costs(Stage.ConnectionContainer,14);var p=Costs(Stage.DualConnectionScratch,14);using var s=MixedProducerAdmission.Enter(Limits with{Matrices=Limits.Matrices with{SlotVisits=2*c.SlotVisits+p.SlotVisits-1}});Refuses(()=>_=DualConnection(null!,null!));Need(s.Snapshot().Poisoned&&s.Snapshot().Matrices.Objects==0&&s.Snapshot().RationalConstructions==0,"logical dense budget before any upstream work");});
  Test("linearized body census includes result buffers selectors and nested matrix accesses",()=>
  {for(int n=1;n<=14;n++)Need(Costs(Stage.LinearizedConnectionBody,n)==EnumerateLinearizedBody(n),"integer-only linearized enumeration n="+n);Need(Costs(Stage.LinearizedConnectionBody,1)==new Plan(2,2,48,0,0),"n1 hand count");Need(Costs(Stage.LinearizedConnectionBody,2)==new Plan(4,2,422,0,0),"n2 hand count");Need(Costs(Stage.LinearizedConnectionBody,14)==new Plan(28,2,654962,0,0),"fixed14 body excluding LINQ internals");});
  Test("metric-jet census includes reference reads AND every unguarded SymProduct transpose",()=>
  {for(int n=1;n<=14;n++)Need(Costs(Stage.MetricJetsBody,n)==new Plan(0,0,EnumerateMetricJetsBody(n,false),0,0),"jet enumeration n="+n);Need(Costs(Stage.MetricJetsBody,1).SlotVisits==24,"n1 includes seven transpose calls");Need(Costs(Stage.MetricJetsBody,14).SlotVisits==320110,"1414 references plus813 full transposes");});
  Test("mixed-block census counts compound target once but both symmetric-copy references",()=>
  {for(int n=1;n<=14;n++)Need(Costs(Stage.MetricJetsBlocksBody,n)==new Plan(0,0,EnumerateMetricJetsBody(n,true),0,0),"block enumeration n="+n);Need(Costs(Stage.MetricJetsBlocksBody,4).SlotVisits==0&&Costs(Stage.MetricJetsBlocksBody,5).SlotVisits==2384,"empty split and first nonempty manufactured split");Need(Costs(Stage.MetricJetsBlocksBody,14).SlotVisits==1721600,"all400 source block triples");});
  Test("linearized wrapper reserves both additional result arrays before upstream constructors",()=>
  {var c=Costs(Stage.ConnectionContainer,14);foreach(bool objects in new[]{false,true}){using var s=MixedProducerAdmission.Enter(Limits with{Matrices=objects?Limits.Matrices with{ArrayObjects=c.ArrayObjects+1}:Limits.Matrices with{ArraySlots=c.ArraySlots+27}});Refuses(()=>_=LinearizedConnection(null!,null!,null!));Need(s.Snapshot().Poisoned&&s.Snapshot().Matrices.Objects==0&&s.Snapshot().RationalConstructions==0,"result-array refusal before linked code");}});
  Test("linearized wrapper refuses dense body exhaustion before upstream work",()=>
  {var c=Costs(Stage.ConnectionContainer,14);var p=Costs(Stage.LinearizedConnectionBody,14);using var s=MixedProducerAdmission.Enter(Limits with{Matrices=Limits.Matrices with{SlotVisits=c.SlotVisits+p.SlotVisits-1}});Refuses(()=>_=LinearizedConnection(null!,null!,null!));Need(s.Snapshot().Poisoned&&s.Snapshot().Matrices.Objects==0&&s.Snapshot().RationalConstructions==0,"body refused before matrix arithmetic");});
  Test("both metric wrappers admit body loops before even their result constructor",()=>
  {foreach(bool blocks in new[]{false,true}){var p=Costs(blocks?Stage.MetricJetsBlocksBody:Stage.MetricJetsBody,14);using var s=MixedProducerAdmission.Enter(Limits with{Matrices=Limits.Matrices with{SlotVisits=Costs(Stage.MetricContainer,14).SlotVisits+p.SlotVisits-1}});Refuses(()=>_ =blocks?MetricJetsBlocks(null!,null!):MetricJets(null!,null!));Need(s.Snapshot().Poisoned&&s.Snapshot().Matrices.Objects==0&&s.Snapshot().RationalConstructions==0,"jet body refusal before linked source");}});
  Test("Downstairs census includes every Matches counts array and appended result",()=>
  {for(int extra=0;extra<=2;extra++)Need(DownstairsBodyCosts(extra)==EnumerateDownstairsBody(extra),"source loop enumeration extra="+extra);Need(DownstairsBodyCosts(0)==new Plan(3848,1538,15124,0,0),"zero-extra still appends and matches");Need(DownstairsBodyCosts(2)==new Plan(5384,1538,24340,0,0),"second derivative exact logical body");});
  Test("ShearJets aggregate counts all nested calls params arrays transposes and outer stores",()=>
  {Need(Costs(Stage.ShearJetsBody,14)==EnumerateShearJetsBody(),"full fixed4+10 loop census");Need(Costs(Stage.ShearJetsBody,14)==new Plan(477852,155438,2086560,0,0),"source and known result buffers only, not LINQ internals");});
  Test("ShearJets cannot silently use a manufactured smaller geometry domain",()=>
  {foreach(int n in new[]{1,2,4,13}){bool failed=false;try{_=Costs(Stage.ShearJetsBody,n);}catch(ArgumentOutOfRangeException){failed=true;}Need(failed,"fixed14 only");}foreach(int extra in new[]{-1,3,int.MaxValue}){bool failed=false;try{_=DownstairsBodyCosts(extra);}catch(ArgumentOutOfRangeException){failed=true;}Need(failed,"only source-reachable extra lengths");}});
  Test("ShearJets wrapper rejects missing nested-array object and slot budgets before source",()=>
  {var p=Costs(Stage.ShearJetsBody,14)+Costs(Stage.MetricContainer,14);foreach(bool objects in new[]{false,true}){using var s=MixedProducerAdmission.Enter(Limits with{Matrices=objects?Limits.Matrices with{ArrayObjects=p.ArrayObjects-1}:Limits.Matrices with{ArraySlots=p.ArraySlots-1}});Refuses(()=>_=ShearJets(null!,null!,null!,null!));var r=s.Snapshot();Need(r.Poisoned&&r.Matrices.Objects==0&&r.RationalConstructions==0&&r.IntegerOperations==0,"before linked inverse, constructor or Matching");}});
  Test("ShearJets wrapper rejects logical body visits before scientific input access",()=>
  {var p=Costs(Stage.ShearJetsBody,14)+Costs(Stage.MetricContainer,14);using var s=MixedProducerAdmission.Enter(Limits with{Matrices=Limits.Matrices with{SlotVisits=p.SlotVisits-1}});Refuses(()=>_=ShearJets(null!,null!,null!,null!));Need(s.Snapshot().Poisoned&&s.Snapshot().Matrices.Objects==0&&s.Snapshot().RationalConstructions==0,"null sentinels never reach linked code");});
  Test("ShearJets exact metadata budget succeeds once and cannot fund a second germ",()=>
  {var p=Costs(Stage.ShearJetsBody,14)+Costs(Stage.MetricContainer,14);using var s=MixedProducerAdmission.Enter(Limits with{Matrices=Limits.Matrices with{ArraySlots=p.ArraySlots,ArrayObjects=p.ArrayObjects,SlotVisits=p.SlotVisits}});Reserve(Stage.MetricContainer);Reserve(Stage.ShearJetsBody);var r=s.Snapshot();Need(r.Matrices.ArraySlots==478062&&r.Matrices.ArrayObjects==155440&&r.Matrices.SlotVisits==2086980&&r.Matrices.Objects==0,"metadata-only aggregate");Refuses(()=>Reserve(Stage.ShearJetsBody));Need(s.Snapshot().Poisoned,"cumulative quota, no per-germ reset");});
  Test("Ambient constructor census includes raw initialization and both horizontal branches",()=>
  {Need(Costs(Stage.AmbientForwardConstructor,14)==EnumerateAmbientConstructor(false),"default body enumeration");Need(Costs(Stage.AmbientInverseConstructor,14)==EnumerateAmbientConstructor(true),"inverse body enumeration");Need(Costs(Stage.AmbientForwardConstructor,14)==new Plan(41160,2,63722,2,0),"default hand count");Need(Costs(Stage.AmbientInverseConstructor,14)==new Plan(41160,2,67322,2,0),"inverse adds four Vertical reads and32 DD accesses per pair");});
  Test("Ambient fixed split cannot silently shrink with a generic test dimension",()=>
  {foreach(var stage in new[]{Stage.AmbientForwardConstructor,Stage.AmbientInverseConstructor})foreach(int n in new[]{1,4,13}){bool failed=false;try{_=Costs(stage,n);}catch(ArgumentOutOfRangeException){failed=true;}Need(failed,"fixed4+10 domain");}});
  Test("Ambient wrapper refuses raw-array and object-header quotas before initializers",()=>
  {foreach(bool inverse in new[]{false,true})foreach(int mode in new[]{0,1,2}){var limits=mode==0?Limits with{Matrices=Limits.Matrices with{ArrayObjects=1}}:mode==1?Limits with{Matrices=Limits.Matrices with{ArraySlots=41159}}:Limits with{Tensors=Limits.Tensors with{MetadataObjects=1}};using var s=MixedProducerAdmission.Enter(limits);Refuses(()=>_=CreateAmbient(null!,default,default,default,inverse));Need(s.Snapshot().Poisoned&&s.Snapshot().Matrices.Objects==0&&s.Snapshot().RationalConstructions==0,"before Basis initializer, inverse or MetricJet construction");}});
  Test("Ambient wrapper chooses the exact branch budget before linked input access",()=>
  {foreach(bool inverse in new[]{false,true}){var p=Costs(inverse?Stage.AmbientInverseConstructor:Stage.AmbientForwardConstructor,14);using var s=MixedProducerAdmission.Enter(Limits with{Matrices=Limits.Matrices with{SlotVisits=p.SlotVisits-1}});Refuses(()=>_=CreateAmbient(null!,default,default,default,inverse));Need(s.Snapshot().Poisoned&&s.Snapshot().Matrices.Objects==0,"correct branch refused before null input dereference");}});
  Test("Baseline census counts source target-reference and target-cell separately",()=>
  {for(int n=1;n<=14;n++)Need(Costs(Stage.BaselineMetricBody,n)==new Plan(0,0,EnumerateBaselineCopies(n),0,0),"copy enumeration n="+n);Need(Costs(Stage.BaselineMetricBody,1).SlotVisits==6&&Costs(Stage.BaselineMetricBody,2).SlotVisits==72&&Costs(Stage.BaselineMetricBody,14).SlotVisits==123480,"hand-counted copies");});
  Test("Baseline wrapper rejects the old copied-cell-only budget before result construction",()=>
  {using var s=MixedProducerAdmission.Enter(Limits with{Matrices=Limits.Matrices with{SlotVisits=Costs(Stage.MetricContainer,14).SlotVisits+41160}});Refuses(()=>_=BaselineMetric(null!));Need(s.Snapshot().Poisoned&&s.Snapshot().Matrices.Objects==0&&s.Snapshot().RationalConstructions==0,"historical undercharge is not sufficient");});
  Test("metadata reservations never construct geometry or coefficients",()=>
  {foreach(Stage stage in Enum.GetValues<Stage>()){using var s=MixedProducerAdmission.Enter(Limits);Reserve(stage);var report=s.Snapshot();var p=Costs(stage,14);Need(report.Matrices.ArraySlots==p.ArraySlots&&report.Matrices.ArrayObjects==p.ArrayObjects&&report.Matrices.SlotVisits==p.SlotVisits,"full reservation "+stage);Need(report.RationalConstructions==0&&report.Matrices.Objects==0&&report.IntegerOperations==0,"no coefficient or matrix work "+stage);Need(report.Tensors.Dictionaries==p.TensorDictionaries&&report.Tensors.MetadataObjects==p.MetadataObjects,"metadata/dictionary reservation "+stage);}});
  Test("array slot cap refuses stage before visits or numeric work",()=>
  {using var s=MixedProducerAdmission.Enter(Limits with{Matrices=Limits.Matrices with{ArraySlots=768319}});Refuses(()=>Reserve(Stage.LoweredFrame));Need(s.Snapshot().Poisoned&&s.Snapshot().Matrices.SlotVisits==0&&s.Snapshot().RationalConstructions==0,"pre-stage refusal");});
  Test("array object cap catches rank4 index-array multiplication",()=>
  {using var s=MixedProducerAdmission.Enter(Limits with{Matrices=Limits.Matrices with{ArrayObjects=153667}});Refuses(()=>Reserve(Stage.LoweredFrame));Need(s.Snapshot().Poisoned&&s.Snapshot().Matrices.ArraySlots==0,"objects admitted first");});
  Test("dense slot cap refuses without any coefficient execution",()=>
  {using var s=MixedProducerAdmission.Enter(Limits with{Matrices=Limits.Matrices with{SlotVisits=1}});Refuses(()=>Reserve(Stage.VariationFrame));Need(s.Snapshot().Poisoned&&s.Snapshot().RationalConstructions==0,"dense loops not run");});
  Test("container multiplicities and raw Lift seed need separate budgets",()=>
  {using(var s=MixedProducerAdmission.Enter(Limits)){Reserve(Stage.ConnectionContainer,2);Need(s.Snapshot().Matrices.ArraySlots==77252&&s.Snapshot().Matrices.ArrayObjects==6,"dual connection two containers");}using(var s=MixedProducerAdmission.Enter(Limits with{Tensors=Limits.Tensors with{Dictionaries=1}})){Reserve(Stage.Lift);Refuses(()=>Reserve(Stage.Lift));Need(s.Snapshot().Poisoned,"repeated raw seed fails closed");}});
  Test("small vector and grid factories admit before allocation",()=>
  {Refuses(()=>_=Vector<int>(1));using(var s=MixedProducerAdmission.Enter(Limits)){var v=Vector<int>(3);var grid=Grid<int>(2,2);Need(v.Length==3&&grid.Length==4&&s.Snapshot().Matrices.ArraySlots==7,"manufactured arrays");}using(var s=MixedProducerAdmission.Enter(Limits with{Matrices=Limits.Matrices with{ArraySlots=3}})){Refuses(()=>_=Grid<int>(2,2));Need(s.Snapshot().Poisoned,"before array");}});
  Test("negative grid dimensions cannot cancel in slot product",()=>
  {using var s=MixedProducerAdmission.Enter(Limits);bool failed=false;try{_=Grid<int>(-2,-2);}catch(ArgumentOutOfRangeException){failed=true;}Need(failed&&s.Snapshot().Poisoned&&s.Snapshot().Matrices.ArrayObjects==0,"negative dimensions poison");});
  Test("generic full curvature validation includes normally ignored entries",()=>
  {Rational[,,,] r=new Rational[2,2,2,2];using(var s=MixedProducerAdmission.Enter(Limits)){r[1,0,1,0]=new Rational(BigInteger.One<<100,1);CurvatureInput(r,2);Need(s.Snapshot().Matrices.SlotVisits==16,"all slots");}using(var s=MixedProducerAdmission.Enter(Limits with{IntegerBits=64})){Refuses(()=>CurvatureInput(r,2));Need(s.Snapshot().Poisoned,"borrowed ignored half admitted");}});
  Test("generic curvature validator rejects malformed shape before scan",()=>
  {using var s=MixedProducerAdmission.Enter(Limits);bool failed=false;try{CurvatureInput(new Rational[2,2,2,1],2);}catch(ArgumentException){failed=true;}Need(failed&&s.Snapshot().Poisoned&&s.Snapshot().Matrices.SlotVisits==0,"shape before scan");});
  Test("invalid stage and negative count poison reservation",()=>
  {using(var s=MixedProducerAdmission.Enter(Limits)){bool failed=false;try{Reserve((Stage)999);}catch(ArgumentOutOfRangeException){failed=true;}Need(failed&&s.Snapshot().Poisoned,"unknown stage");}using(var s=MixedProducerAdmission.Enter(Limits)){bool failed=false;try{Reserve(Stage.Lower,-1);}catch(ArgumentOutOfRangeException){failed=true;}Need(failed&&s.Snapshot().Poisoned,"negative count");}});
  Test("null curvature input poisons before shape or coefficient access",()=>
  {using var s=MixedProducerAdmission.Enter(Limits);bool failed=false;try{CurvatureInput(null!,2);}catch(ArgumentException){failed=true;}Need(failed&&s.Snapshot().Poisoned&&s.Snapshot().Matrices.SlotVisits==0,"null is a malformed admitted input");});
  Console.WriteLine($"{tests} metadata/manufactured geometry-admission tests passed; no source geometry or curvature operator executed.");
 }
}
