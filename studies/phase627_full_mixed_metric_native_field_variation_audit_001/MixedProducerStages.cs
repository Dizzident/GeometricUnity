using System.Diagnostics;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Partial call-site admission, normally conditional in production. The explicit
// TensorFingerprintAdmitted entry is mandatory in every build. Charges are cumulative
// logical envelopes, not allocator bytes or simultaneous live storage. Recorder
// internals, caller-created params arrays, static bootstrap and geometry stages
// still require their own admission; these methods never create trace nodes.
internal static class MixedProducerStages
{
 public static FT Empty()
 {
#if A68_GUARDED_FOURIER_TENSOR
  MixedProducerAdmission.Current.TensorNew();
#endif
  return new FT();
 }
 public static long Add(long value,long amount)
 {
  try{return checked(value+amount);}catch(OverflowException){Poison();throw;}
 }
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void Poison()=>MixedProducerAdmission.Current.Poison();
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void Arrays(long slots,long objects=1)
 {
  var s=MixedProducerAdmission.Current;s.RawArray(slots,objects);
  // Declared buffers:zero initialization plus one initial population store.
  // A zero-only allocation is deliberately overcharged. Filling-expression
  // reads, later mutations/copies and library-internal buffers are separate.
  s.MatrixVisits(s.CountProduct(2,slots));
 }
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void Metadata(long count)=>MixedProducerAdmission.Current.MetadataObjects(count);
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void Trace(long inputs,bool parameter=false)
 {Arrays(inputs);if(parameter)Metadata(1);}
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void Input(FT tensor)
 {
  var s=MixedProducerAdmission.Current;s.TensorSupport(tensor.Count);s.TensorVisits(tensor.Count);
  foreach(var q in tensor){Factor(q.Value.Real);Factor(q.Value.Imaginary);}
 }
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void Factor(Rational value)=>MixedProducerAdmission.Current.RationalInputs(value.Numerator,value.Denominator);
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void Scan(long records)=>MixedProducerAdmission.Current.TensorVisits(records);
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void Matrix(Matrix matrix,bool fourteen=true)
 {
  var s=MixedProducerAdmission.Current;s.MatrixDimension(matrix.N);
  if(fourteen&&matrix.N<14){s.Poison();throw new ArgumentException("fourteen-axis matrix required");}
  s.MatrixLoop(matrix.N,2);for(int i=0;i<matrix.N;i++)for(int j=0;j<matrix.N;j++)Factor(matrix[i,j]);
 }
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void RecordedMatrix(Matrix matrix)
 {if(matrix.N!=14){MixedProducerAdmission.Current.Poison();throw new ArgumentException("recorded matrix must have fourteen axes");}Matrix(matrix);}
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void MatrixArgument(Matrix matrix)
 {
  var s=MixedProducerAdmission.Current;
  if(matrix is null||matrix.N!=14){s.Poison();throw new ArgumentException("recorded matrix must have fourteen axes");}
  // Two explicit object[] buffers, at most196 entry headers and one envelope.
  // Body touches: first zero196 + matrix reads392 + stores196 + final zero196
  // + copy read/write392 =1372. Reserve worst case even for an empty matrix.
  // RecordedMatrix adds196 borrowed-input validation reads BEFORE allocation.
  // Array.Copy's logical read/write is not a library/VM/RSS cost proof.
  s.RawArray(392,2);s.MetadataObjects(197);s.MatrixVisits(1372);
  RecordedMatrix(matrix);
  // Rational formatting has its own primitive guard. Serializer/string
  // headers, recorder retention and whole-lifetime accounting remain separate.
 }
 public static (long ArrayTouches,long ComponentComparisons) FingerprintSortCosts(int records)
 {
  if(records<0)throw new ArgumentOutOfRangeException(nameof(records));
  long n=records,levels=0;for(int remaining=records;remaining>0;remaining/=2)levels++;
  // Fewer than2n sift calls, each at most bit-length(n) iterations. Each
  // iteration: two key comparisons (four components each), eight array
  // reads/writes. Extraction swaps add four touches. Zero/copy/output add3n.
  long iterations=records<2?0:checked(2*n*levels);
  return(checked(3*n+4*Math.Max(n-1,0)+8*iterations),checked(8*iterations));
 }
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void TensorFingerprint(int records,bool ownsWriter)=>TensorFingerprintAdmitted(records,ownsWriter);
 public static void TensorFingerprintAdmitted(int records,bool ownsWriter)
 {
  var s=MixedProducerAdmission.Current;s.TensorSupport(records);
  var costs=FingerprintSortCosts(records);
  // A slot here is a key/value record, NOT a byte or a reference. The four
  // named scratch record roles are distinct from this raw array's slots.
  s.RawArray(records);s.TensorSequence(records);s.TensorTemporary(4);
  s.TensorVisits(s.CountProduct(2,records)); // validation and dictionary copy
  s.MatrixVisits(costs.ArrayTouches);s.TensorSortComparisons(costs.ComponentComparisons);
  if(ownsWriter)
  {
   // Known wrappers: HashStream, IncrementalHash, Utf8JsonWriter; digest
   // result32 bytes and two64-character hex strings. Provider/pool/runtime
   // internals and writer buffers are NOT covered by these named objects.
   s.MetadataObjects(5);s.RawArray(32);s.TextCharacters(128);
  }
 }
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void RecorderOperandScan(int inputs)
 {
  var s=MixedProducerAdmission.Current;
  // One outer input read plus at most i prior-reference reads at position i.
  // No HashSet allocation or coefficient work in the distinct-empty census.
  s.MatrixVisits(s.CountProduct(inputs,s.CountSum(inputs,1))/2);
 }
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void RecorderOperandArrays(int inputs)
 {
  var s=MixedProducerAdmission.Current;
  // Source IDs and degrees: zero/store4n plus input read, ID read and node
  // list-index read3n. List/weak-table internals require separate accounting.
  Arrays(s.CountProduct(2,inputs),2);s.MatrixVisits(s.CountProduct(3,inputs));
 }
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void MatrixSlots(long slots)=>MixedProducerAdmission.Current.MatrixVisits(slots);
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void ExternalDictionary()=>MixedProducerAdmission.Current.TensorNew();
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void Bits(long calls)
 {
  var s=MixedProducerAdmission.Current;
  // Enumerable.Range/Where/ToArray: full14 visits, conservative growth slots.
  s.TensorVisits(s.CountProduct(14,calls));s.RawArray(s.CountProduct(56,calls),s.CountProduct(5,calls));s.MetadataObjects(s.CountProduct(4,calls));
  // Known final int[<=14] result initialization/stores. Existing growth slack
  // is not a proof of arbitrary LINQ-internal work or runtime allocations.
  s.MatrixVisits(s.CountProduct(28,calls));
 }
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void Transpose(FT a,FT b,bool oracle)
 {
  Input(a);Input(b);var s=MixedProducerAdmission.Current;s.SequenceProduct(a.Count,b.Count);
  if(!oracle){s.TensorNew();return;}
  long pairs=s.CountProduct(a.Count,b.Count);Bits(s.CountProduct(15,pairs));s.TensorVisits(s.CountProduct(196,pairs));
 }
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void Motion(Matrix matrix,FT tensor,bool oracle)
 {
  RecordedMatrix(matrix);Input(tensor);var s=MixedProducerAdmission.Current;Bits(tensor.Count);
  // Each record independently has0..14 occupied axes; no homogeneous-grade
  // or cancellation assumption. Reserve all replacement/rejection work first.
  long slots=s.CountProduct(196,tensor.Count);
  // Per replacement:condition+coefficient matrix reads (2), indices[slot] (1),
  // and at most14 Where/Contains source-array reads. No zero/grade shortcut.
  s.MatrixVisits(s.CountProduct(17,slots));s.TensorVisits(s.CountProduct(14,slots));
  // Indexed Where delegate/iterator per replacement plus captured slot
  // display classes per record; reserve slack independent of zero skips.
  s.MetadataObjects(s.CountSum(s.CountProduct(4,slots),s.CountProduct(4,tensor.Count)));
  if(oracle)
  {
   s.RawArray(s.CountProduct(14,slots),slots);s.TensorVisits(s.CountProduct(210,slots));
   // Clone zero/copy read/write42; replacement store1;91 inversion pairs*2
   // source reads; aggregate14 =239 per potential replacement. Preserve the
   // legacy tensor-work ceiling in addition, not as the array-access proof.
   s.MatrixVisits(s.CountProduct(239,slots));
  }
 }
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void Pullback(Matrix map,FT tensor)
 {
  RecordedMatrix(map);Input(tensor);var s=MixedProducerAdmission.Current;
  s.TensorSort(tensor.Count);s.TensorVisits(tensor.Count);s.MetadataObjects(s.CountSum(8,s.CountProduct(4,tensor.Count)));
  s.TensorTemporary(s.CountProduct(8,s.CountSum(tensor.Count,1)));Bits(tensor.Count);
 }
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void Derivatives(FT[] values)
 {if(values.Length<14){MixedProducerAdmission.Current.Poison();throw new ArgumentException("fourteen derivatives required");}}
 [Conditional("A68_GUARDED_FOURIER_TENSOR")]
 public static void Axis(int axis)
 {if(axis<0||axis>=14){MixedProducerAdmission.Current.Poison();throw new ArgumentOutOfRangeException(nameof(axis));}}
}
