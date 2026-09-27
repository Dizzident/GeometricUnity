using System.Diagnostics;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// PARTIAL producer closure, inactive in production. The curvature wrappers
// cover their linked raw arrays and dense slot-access envelopes; guarded Matrix,
// Rational and Fourier kernels separately admit nested arithmetic/mutations.
// Container reservations cover constructor arrays, not the full algorithms
// using those containers. Unwrapped Ambient methods, LINQ-internal scratch,
// static bootstrap, recorder/serializer buffers and VM/GCD/RSS remain separate.
internal static class MixedGeometryAdmission
{
 public enum Stage{EndomorphismFrame,Lower,LoweredFrame,VariationFrame,VariationLowered,Lift,MetricContainer,ConnectionContainer,DualConnectionScratch,LinearizedConnectionBody,MetricJetsBody,MetricJetsBlocksBody,ShearJetsBody,AmbientForwardConstructor,AmbientInverseConstructor,BaselineMetricBody}
 public readonly record struct Plan(long ArraySlots,long ArrayObjects,long SlotVisits,long MetadataObjects,long TensorDictionaries)
 {
  public static Plan operator +(Plan a,Plan b)=>new(checked(a.ArraySlots+b.ArraySlots),checked(a.ArrayObjects+b.ArrayObjects),checked(a.SlotVisits+b.SlotVisits),checked(a.MetadataObjects+b.MetadataObjects),checked(a.TensorDictionaries+b.TensorDictionaries));
 }
 static Plan Repeat(Plan p,long count)=>new(checked(p.ArraySlots*count),checked(p.ArrayObjects*count),checked(p.SlotVisits*count),checked(p.MetadataObjects*count),checked(p.TensorDictionaries*count));
 // Fixed4 Downstairs body, only the extra lengths called by ShearJets.
 // Result r[4],769 Matches counts[4],768 appended derivative RESULTS[k+1].
 // The caller-owned extra array is counted by ShearJetsBody, not here.
 // Matches:4 zero-init+3*derivativeLength index accesses+8 semantic equality
 // reads. Append result:zero-init+stores+(k source reads)=3k+2 accesses.
 // Each of256 quadruples has3*(append+Matches)+8 direct matrix/array accesses.
 // SequenceEqual/Append/ToArray INTERNAL work/allocations, empty params-array
 // bootstrap and runtime-dependent buffers remain unproved. This is a logical
 // source/result-buffer model, NOT an upper bound on all library memory reads.
 public static Plan DownstairsBodyCosts(int extraLength)
 {
  if(extraLength<0||extraLength>2)throw new ArgumentOutOfRangeException(nameof(extraLength));
  return new(3848+768L*extraLength,1538,15124+4608L*extraLength,0,0);
 }
 static Plan ShearJetsBodyCosts(int n)
 {
  if(n!=14)throw new ArgumentOutOfRangeException(nameof(n),"ShearJets source is fixed4+10 only");
  // Downstairs extra lengths0/1/2 occur1/84/16 times. Nonempty params arrays
  // have116 slots in100 arrays (zero-init+stores=232 visits). Outer D/DD stores
  // and Vertical reads add300. Each of111 Shear calls has216 direct visits:
  //4*(2 c reads+32 transpose copy accesses+20 coordinate-to-matrix accesses).
  // Coordinates result arrays and Matrix allocations/operators guard themselves.
  return DownstairsBodyCosts(0)+Repeat(DownstairsBodyCosts(1),84)+Repeat(DownstairsBodyCosts(2),16)+new Plan(116,100,232+300+111*216,0,0);
 }
 static Plan AmbientConstructorCosts(int n,bool inverseHorizontal)
 {
  if(n!=14)throw new ArgumentOutOfRangeException(nameof(n),"Ambient source is fixed4+10 only");
  // D[14,14,14], DD[14,14,14,14], including their logical zero initialization.
  // Default body:2*16+2*100+10*(1+2*16+2*100)+100*2*100=22562.
  // Inverse branch adds100*(4 Vertical reads+2*16 horizontal DD accesses).
  // Two known non-array objects:Ambient itself and its nested MetricJet.
  // Basis/Matrix/MetricJet kernels retain their SEPARATE guarded reservations;
  // this does not certify their internal work, library scratch or total RSS.
  return new(41160,2,41160+22562+(inverseHorizontal?3600:0),2,0);
 }
 // Pure metadata only. Generic n is useful for manufactured hand enumeration;
 // all production call sites below reserve n=14 before invoking fixed14 code.
 public static Plan Costs(Stage stage,int n)
 {
  if(n<1||n>14)throw new ArgumentOutOfRangeException(nameof(n));
  long n2=(long)n*n,n3=n2*n,n4=n3*n,n5=n4*n,n6=n5*n;
  long horizontal=Math.Min(4,n),vertical=n-horizontal;
  return stage switch
  {
   // SlotVisits conservatively covers explicit array/matrix reads+writes in
   // these linked stage bodies, plus one logical initialization per raw-array
   // cell. Compound assignments count a read AND a write. Nested Matrix
   // operators charge their own kernels separately; this is not an exact VM
   // instruction, allocator or library-internal memory-access census.
   Stage.EndomorphismFrame=>new(n4,1,8*n6+8*n4+4*n3+4*n2,0,0),
   Stage.Lower=>new(n4,1,8*n5+2*n4,0,0),
   // Four rank4 Rational outputs and one int[4] per output coordinate/axis.
   // Includes q[axis] assignment, all four q reads, repeated e reads, and
   // q's initialization/target read as well as next's output assignment.
   // q has both logical zero initialization and four literal stores;12n4
   // includes both even at generic n=1 (production remains fixed14).
   Stage.LoweredFrame=>new(20*n4,4+4*n4,4*(10*n5+12*n4),0,0),
   Stage.VariationFrame=>new Plan(n4,1,12*n5+10*n4+4*n2,0,0)+Costs(Stage.EndomorphismFrame,n),
   Stage.VariationLowered=>Costs(Stage.Lower,n)+Costs(Stage.Lower,n)+Costs(Stage.Lower,n)+new Plan(0,0,24*n5+6*n4,0,0)+Costs(Stage.LoweredFrame,n),
   Stage.Lift=>new(0,0,n4+n3+n2+n,0,1),
   // Constructor G/Gamma matrices have their own guarded Matrix admission.
   Stage.MetricContainer=>new(n+n2,2,2*(n+n2),4,0),
   Stage.ConnectionContainer=>new(n+n2+n4,3,n4+2*(n+n2),4,0),
   // DualConnection's ip[n,n], dip[n,n,n], and n^3 dg[n] arrays.
   // A slot here is ONE Dual (two Rational fields), not one Rational/byte.
   // Dense access decomposition, assuming every nonzero branch is taken:
   // zero initialization n2+n3+n4; ip fill 3n2;
   // inverse derivative n4+9n5; gamma t 14n4, dt 20n5;
   // Gamma stores 4n3, DGamma stores 6n4;
   // curvature seed 8n4, products 16n5, final stores 2n4.
   // Each nested D/DD/Gamma/DGamma lookup counts BOTH the reference-array
   // access and matrix cell access. Compound array updates count read+write.
   // Matrix kernels/constructors and Rational/Dual arithmetic are separate;
   // this does not bound VM instructions, struct fields, heap bytes or RSS.
   Stage.DualConnectionScratch=>new(n2+n3+n4,2+n3,45*n5+32*n4+5*n3+4*n2,0,0),
   // Two known n-element ToArray RESULTS (invd/did). This does NOT reserve
   // LINQ iterator/delegate/closure objects or implementation-internal buffers.
   // Result zero-init/stores plus input selector reads:8n; Gamma stores:n;
   // t/dt:14n3; tz/dtz:14n4; DGamma selectors/stores:3n2;
   // curvature reference reads:6n2; curvature copy:2n4.
   Stage.LinearizedConnectionBody=>new(2*n,2,16*n4+14*n3+9*n2+9*n,0,0),
   // MetricJets: each first derivative has two reference reads and one store;
   // each second derivative has six reference reads and one store. SymProduct
   // calls SpinGeometry.Transpose (NOT a guarded Matrix operator): each of
   // 1+2n+4n2 calls reads+writes n2 cells. Include those copies here; its Matrix
   // allocation, other Matrix kernels and MetricData constructor are separate.
   Stage.MetricJetsBody=>new(0,0,3*n+7*n2+2*n2*(1+2*n+4*n2),0,0),
   // Fixed14 source has4 horizontal and10 vertical directions. Manufactured
   // n truncates that split at min(4,n); there is no block when n<=4.
   // Each (horizontal,vertical,vertical) triple:6 G accesses,13 per D slice,
   // 21 per DD slice. A compound matrix target reads its outer array ONCE,
   // then its cell twice; the later symmetric copy reads both outer arrays.
   Stage.MetricJetsBlocksBody=>new(0,0,horizontal*vertical*vertical*(6+13*n+21*n2),0,0),
   Stage.ShearJetsBody=>ShearJetsBodyCosts(n),
   Stage.AmbientForwardConstructor=>AmbientConstructorCosts(n,false),
   Stage.AmbientInverseConstructor=>AmbientConstructorCosts(n,true),
   // Source D/DD read + target outer Matrix reference read + target cell store.
   // Do not count just one copied cell; the result constructor is separate.
   Stage.BaselineMetricBody=>new(0,0,3*(n3+n4),0,0),
   _=>throw new ArgumentOutOfRangeException(nameof(stage))
  };
 }
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void Reserve(Stage stage,int count=1)
 {
  var s=MixedProducerAdmission.Current;
  if(count<0){s.Poison();throw new ArgumentOutOfRangeException(nameof(count));}
  Plan p;try{p=Costs(stage,14);}catch{ s.Poison();throw; }
  s.RawArray(s.CountProduct(p.ArraySlots,count),s.CountProduct(p.ArrayObjects,count));
  s.MatrixVisits(s.CountProduct(p.SlotVisits,count));s.MetadataObjects(s.CountProduct(p.MetadataObjects,count));
  for(int i=0;i<count;i++)for(long j=0;j<p.TensorDictionaries;j++)s.TensorNew();
 }
 public static T[] Vector<T>(int length)
 {
  MixedProducerStages.Arrays(length);return new T[length];
 }
 public static T[,] Grid<T>(int rows,int columns)
 {
  GridShape(rows,columns);
  MixedProducerStages.Arrays((long)rows*columns);return new T[rows,columns];
 }
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 static void GridShape(int rows,int columns)
 {if(rows<0||columns<0){MixedProducerAdmission.Current.Poison();throw new ArgumentOutOfRangeException("grid shape");}}
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void CurvatureInput(Rational[,,,] value,int dimension=14)
 {
  var s=MixedProducerAdmission.Current;
  if(value is null||dimension<1||dimension>14||value.GetLength(0)!=dimension||value.GetLength(1)!=dimension||value.GetLength(2)!=dimension||value.GetLength(3)!=dimension)
  {s.Poison();throw new ArgumentException("complete curvature shape");}
  s.MatrixLoop(dimension,4);
  for(int a=0;a<dimension;a++)for(int b=0;b<dimension;b++)for(int c=0;c<dimension;c++)for(int d=0;d<dimension;d++)MixedProducerStages.Factor(value[a,b,c,d]);
 }
 public static Rational[,,,] EndomorphismFrame(Rational[,,,] r,Matrix e,Matrix inverse)
 {
  CurvatureInput(r);MixedProducerStages.RecordedMatrix(e);MixedProducerStages.RecordedMatrix(inverse);Reserve(Stage.EndomorphismFrame);
  return SpinGeometry.EndomorphismFrame(r,e,inverse);
 }
 public static Rational[,,,] Lower(Rational[,,,] r,Matrix g)
 {
  CurvatureInput(r);MixedProducerStages.RecordedMatrix(g);Reserve(Stage.Lower);return SpinGeometry.Lower(r,g);
 }
 public static Rational[,,,] LoweredFrame(Rational[,,,] r,Matrix e)
 {
  CurvatureInput(r);MixedProducerStages.RecordedMatrix(e);Reserve(Stage.LoweredFrame);return SpinGeometry.LoweredFrame(r,e);
 }
 public static Rational[,,,] VariationFrame(Rational[,,,] r,Rational[,,,] delta,Matrix n,Matrix e,Matrix inverse)
 {
  CurvatureInput(r);CurvatureInput(delta);MixedProducerStages.RecordedMatrix(n);MixedProducerStages.RecordedMatrix(e);MixedProducerStages.RecordedMatrix(inverse);Reserve(Stage.VariationFrame);
  return MetricVariation.CurvatureVariationFrame(r,delta,n,e,inverse);
 }
 public static Rational[,,,] VariationLowered(Rational[,,,] r,Rational[,,,] delta,Matrix g,Matrix k,Matrix n,Matrix e)
 {
  CurvatureInput(r);CurvatureInput(delta);MixedProducerStages.RecordedMatrix(g);MixedProducerStages.RecordedMatrix(k);MixedProducerStages.RecordedMatrix(n);MixedProducerStages.RecordedMatrix(e);Reserve(Stage.VariationLowered);
  return MetricVariation.CurvatureVariationLowered(r,delta,g,k,n,e);
 }
 public static FT Lift(Rational[,,,] low)
 {CurvatureInput(low);Reserve(Stage.Lift);return SpinGeometry.Lift(low);}
 // Aggregate before entering the linked algorithm. This is NOT a complete
 // borrowed-input validator or a producer-wide resource admission boundary.
 public static (ConnectionData Value,ConnectionData Delta) DualConnection(MetricData g,MetricData k)
 {
  Reserve(Stage.ConnectionContainer,2);Reserve(Stage.DualConnectionScratch);
  return MetricVariation.DualConnection(g,k);
 }
 public static ConnectionData LinearizedConnection(MetricData g,MetricData k,ConnectionData baseline)
 {
  Reserve(Stage.ConnectionContainer);Reserve(Stage.LinearizedConnectionBody);
  return MetricVariation.LinearizedConnection(g,k,baseline);
 }
 public static MetricData MetricJets(MetricData g,MetricData n)
 {
  Reserve(Stage.MetricContainer);Reserve(Stage.MetricJetsBody);
  return MetricVariation.MetricJets(g,n);
 }
 public static MetricData MetricJetsBlocks(MetricData g,MetricData n)
 {
  Reserve(Stage.MetricContainer);Reserve(Stage.MetricJetsBlocksBody);
  return MetricVariation.MetricJetsBlocks(g,n);
 }
 public static MetricData ShearJets(Ambient g,Matrix h,Matrix m,int[] multi)
 {
  Reserve(Stage.MetricContainer);Reserve(Stage.ShearJetsBody);
  return MetricVariation.ShearJets(g,h,m,multi);
 }
 public static Ambient CreateAmbient(Matrix y,Rational alpha,Rational beta,Rational sigma,bool inverseHorizontal=false)
 {
  Reserve(inverseHorizontal?Stage.AmbientInverseConstructor:Stage.AmbientForwardConstructor);
  return new Ambient(y,alpha,beta,sigma,inverseHorizontal);
 }
 public static MetricData BaselineMetric(Ambient g)
 {
  Reserve(Stage.MetricContainer);Reserve(Stage.BaselineMetricBody);
  return MetricData.Baseline(g);
 }
}
