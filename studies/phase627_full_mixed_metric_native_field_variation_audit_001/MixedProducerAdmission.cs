using System.Numerics;

// PARTIAL producer admission only. These are conservative logical charges,
// not allocator bytes, GCD complexity/internal storage, GC/RSS, or a complete
// geometry/tensor/metadata resource proof. No production ceiling is supplied.
internal static class MixedProducerAdmission
{
 public sealed record TensorLimits(long RecordsPerTensor,long Dictionaries,long Insertions,long Mutations,
  long RecordVisits,long PairVisits,long TemporarySlots,long SequenceSlots,long SortWork,long MetadataObjects);
 public sealed record MatrixLimits(long Dimension,long Objects,long Cells,long SlotVisits,long ArrayObjects,long ArraySlots);
 public sealed record Limits(long IntegerBits,long IntermediateBits,long IntegerOperations,
  long IntegerWorkBits,long IntegerObjects,long RationalConstructions,long GcdCalls,
  long FormatCharacters,long WordVisits,long PrimitiveScratchSlots,long InputBytes,TensorLimits Tensors,MatrixLimits Matrices);
 public sealed record TensorReport(long Dictionaries,long Insertions,long Mutations,long RecordVisits,
  long PairVisits,long TemporarySlots,long SequenceSlots,long SortWork,long MetadataObjects);
 public sealed record MatrixReport(long Objects,long Cells,long SlotVisits,long ArrayObjects,long ArraySlots);
 public sealed record Report(long IntegerOperations,long IntegerWorkBits,long IntegerObjects,
  long RationalConstructions,long GcdCalls,long FormatCharacters,long WordVisits,
  long MaximumPrimitiveScratchSlots,long InputBytes,bool Poisoned,TensorReport Tensors,MatrixReport Matrices);
 public sealed class Refused(string message):Exception("A68 producer admission: "+message);
 [ThreadStatic] static Scope? active;
 public static Scope Current
 {get{var scope=active??throw new Refused("mandatory active scope missing");scope.EnsureActive();return scope;}}
 public static Scope Enter(Limits limits)
 {
  if(active is not null)active.Reject("nested producer scope");
  ArgumentNullException.ThrowIfNull(limits);
  ArgumentNullException.ThrowIfNull(limits.Tensors);
  ArgumentNullException.ThrowIfNull(limits.Matrices);
  foreach(long n in new[]{limits.IntegerBits,limits.IntermediateBits,limits.IntegerOperations,
   limits.IntegerWorkBits,limits.IntegerObjects,limits.RationalConstructions,limits.GcdCalls,
   limits.FormatCharacters,limits.WordVisits,limits.PrimitiveScratchSlots,limits.InputBytes,
   limits.Tensors.RecordsPerTensor,limits.Tensors.Dictionaries,limits.Tensors.Insertions,
   limits.Tensors.Mutations,limits.Tensors.RecordVisits,limits.Tensors.PairVisits,
   limits.Tensors.TemporarySlots,limits.Tensors.SequenceSlots,limits.Tensors.SortWork,limits.Tensors.MetadataObjects,
   limits.Matrices.Dimension,limits.Matrices.Objects,limits.Matrices.Cells,limits.Matrices.SlotVisits,
   limits.Matrices.ArrayObjects,limits.Matrices.ArraySlots})
   if(n<=0||n>9007199254740991)throw new Refused("explicit positive safe-integer ceilings required");
  if(limits.IntegerBits>limits.IntermediateBits)throw new Refused("integer ceiling exceeds intermediate ceiling");
  return active=new Scope(limits);
 }
 // Avoid allocating Abs(value) merely to inspect its magnitude. For negatives,
 // two's-complement GetBitLength plus one is a conservative magnitude bound.
 internal static long Bits(BigInteger value)=>value.GetBitLength()+(value.Sign<0?1:0);
 public sealed class Scope:IDisposable
 {
  readonly Limits limits;readonly int thread=Environment.CurrentManagedThreadId;
  long integerOperations,integerWorkBits,integerObjects,rationalConstructions,gcdCalls,
   formatCharacters,wordVisits,maximumPrimitiveScratchSlots,inputBytes;
  long dictionaries,insertions,mutations,recordVisits,pairVisits,temporarySlots,sequenceSlots,sortWork,metadataObjects;
  long matrixObjects,matrixCells,matrixSlotVisits,arrayObjects,arraySlots;
  bool poisoned,disposed;
  internal Scope(Limits limits){this.limits=limits;}
  public Report Snapshot()=>new(integerOperations,integerWorkBits,integerObjects,
   rationalConstructions,gcdCalls,formatCharacters,wordVisits,maximumPrimitiveScratchSlots,inputBytes,poisoned,
   new(dictionaries,insertions,mutations,recordVisits,pairVisits,temporarySlots,sequenceSlots,sortWork,metadataObjects),
   new(matrixObjects,matrixCells,matrixSlotVisits,arrayObjects,arraySlots));
  public void Poison(){poisoned=true;}
  internal void Reject(string message){poisoned=true;throw new Refused(message);}
  void Ready()
  {if(disposed||poisoned||thread!=Environment.CurrentManagedThreadId||!ReferenceEquals(active,this))Reject("poisoned, disposed or wrong-thread scope");}
  internal void EnsureActive()=>Ready();
  long Plus(long a,long b)
  {if(a<0||b<0||a>long.MaxValue-b)Reject("admission integer overflow");return a+b;}
  long Times(long a,long b)
  {if(a<0||b<0||(b!=0&&a>long.MaxValue/b))Reject("admission integer product overflow");return a*b;}
  void Charge(ref long used,long amount,long ceiling,string name)
  {Ready();if(amount<0||used>ceiling-amount)Reject(name+" ceiling before work");used+=amount;}
  void Bound(long bits,long ceiling,string name){Ready();if(bits<0||bits>ceiling)Reject(name+" bit ceiling before work");}
  void Scratch(long slots)
  {Ready();if(slots<0||slots>limits.PrimitiveScratchSlots)Reject("primitive scratch ceiling before work");maximumPrimitiveScratchSlots=Math.Max(maximumPrimitiveScratchSlots,slots);}
  void Integer(long outputBits,long inputBits,long objects,long scratch)
  {
   Bound(outputBits,limits.IntermediateBits,"prospective integer");Scratch(scratch);
   Charge(ref integerOperations,1,limits.IntegerOperations,"integer operation");
   Charge(ref integerWorkBits,Plus(inputBits,outputBits),limits.IntegerWorkBits,"integer bit-work");
   Charge(ref integerObjects,objects,limits.IntegerObjects,"logical integer object");
  }
  public void RationalInputs(BigInteger numerator,BigInteger denominator)
  {Bound(Bits(numerator),limits.IntegerBits,"rational numerator");Bound(Bits(denominator),limits.IntegerBits,"rational denominator");}
  public void RationalConstruction(BigInteger numerator,BigInteger denominator)
  {
   Bound(Bits(numerator),limits.IntermediateBits,"unreduced numerator");Bound(Bits(denominator),limits.IntermediateBits,"unreduced denominator");
   Charge(ref rationalConstructions,1,limits.RationalConstructions,"rational construction");
  }
  public void Reduced(BigInteger numerator,BigInteger denominator)=>RationalInputs(numerator,denominator);
  public void Compare(BigInteger a,BigInteger b)
  {
   long x=Bits(a),y=Bits(b);Bound(x,limits.IntegerBits,"comparison input");Bound(y,limits.IntegerBits,"comparison input");
   Integer(1,Plus(x,y),0,2);
  }
  public void Binary(string operation,BigInteger a,BigInteger b)
  {
   long x=Bits(a),y=Bits(b);Bound(x,limits.IntermediateBits,"integer input");Bound(y,limits.IntermediateBits,"integer input");
   long output=operation switch{"multiply"=>Plus(x,y),"add" or "subtract"=>Plus(Math.Max(x,y),1),"divide"=>x,_=>-1};
   Integer(output,Plus(x,y),1,3);
  }
  public void Unary(BigInteger value)
  {long bits=Bits(value);Bound(bits,limits.IntermediateBits,"integer input");Integer(bits,bits,1,2);}
  public BigInteger FromLong(long value)
  {
   // Fixed64 upper bound includes long.MinValue without negating it.
   Integer(64,64,1,1);return new BigInteger(value);
  }
  public void Gcd(BigInteger a,BigInteger b)
  {
   long x=Bits(a),y=Bits(b);Bound(x,limits.IntermediateBits,"GCD operand");Bound(y,limits.IntermediateBits,"GCD operand");
   Charge(ref gcdCalls,1,limits.GcdCalls,"GCD call");Integer(Math.Max(x,y),Plus(x,y),1,4);
   // Four slots count named operands/output/absolute-value scratch only.
   // BigInteger.GreatestCommonDivisor internal storage is NOT bounded here.
  }
  public long FormattingCharacters(BigInteger numerator,BigInteger denominator,bool denominatorOne)
  {
   RationalInputs(numerator,denominator);
   // A b-bit integer has at most b+1 decimal digits (also covers zero).
   // Include each culture-specific negative-sign string before formatting.
   long first=Plus(Bits(numerator),1);
   if(numerator.Sign<0)first=Plus(first,denominatorOne?1:System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NegativeSign.Length);
   long characters=denominatorOne?first:Plus(Plus(first,1),Plus(Bits(denominator),1));
   Scratch(denominatorOne?2:4);
   // Numerator and denominator temporary strings coexist with joined output.
   return denominatorOne?characters:Plus(characters,characters);
  }
  public void RequireFormattingCharacters(long characters)
  {Ready();if(characters<0||formatCharacters>limits.FormatCharacters-characters)Reject("prospective formatting characters before preparation");}
  public void TextCharacters(long characters)=>Charge(ref formatCharacters,characters,limits.FormatCharacters,"formatted text characters");
  public void Format(BigInteger numerator,BigInteger denominator,bool denominatorOne)
  {TextCharacters(FormattingCharacters(numerator,denominator,denominatorOne));}
  public void Word()
  {
   // Every int mask is inspected only on axes0..13, just as the source.
   // Upper charge per second-word axis: Count14+Contains14+Remove14+
   // linear scan/shift14+conservative insertion-sort comparisons196.
   Charge(ref wordVisits,14+14*(14+14+14+14+196),limits.WordVisits,"ordered Clifford word visits");
   Scratch(64); // list growth/backing slots and short-lived enumerator roles.
  }
  public void ReadBytes(long bytes)
  {Charge(ref inputBytes,bytes,limits.InputBytes,"input bytes");}
  public long CountProduct(long a,long b){Ready();return Times(a,b);}
  public long CountSum(long a,long b){Ready();return Plus(a,b);}
  public void MetadataObjects(long count)=>Charge(ref metadataObjects,count,limits.Tensors.MetadataObjects,"stage metadata objects");
  public void TensorSupport(long records)
  {Ready();if(records<0||records>limits.Tensors.RecordsPerTensor)Reject("tensor support ceiling before work");}
  public void TensorVisits(long visits)=>Charge(ref recordVisits,visits,limits.Tensors.RecordVisits,"tensor record visits");
  public void TensorTemporary(long slots)=>Charge(ref temporarySlots,slots,limits.Tensors.TemporarySlots,"cumulative tensor temporary slots");
  public void TensorSequence(long slots)
  {if(slots>int.MaxValue)Reject("sequence implementation ceiling");Charge(ref sequenceSlots,slots,limits.Tensors.SequenceSlots,"cumulative sequence slots");}
  public void TensorNew(long clonedRecords=0)
  {
   TensorSupport(clonedRecords);Charge(ref dictionaries,1,limits.Tensors.Dictionaries,"tensor dictionaries");
   Charge(ref insertions,clonedRecords,limits.Tensors.Insertions,"cloned tensor insertions");
   // Logical entry/backing-slot uppercharge, NOT a .NET allocation-byte proof.
   TensorTemporary(Plus(1,Times(8,clonedRecords)));
  }
  public void TensorMutation(long currentRecords,bool inserting)
  {
   TensorSupport(inserting?Plus(currentRecords,1):currentRecords);
   Charge(ref mutations,1,limits.Tensors.Mutations,"tensor mutations");
   if(inserting)
   {
    Charge(ref insertions,1,limits.Tensors.Insertions,"tensor insertions");
    // Charge a full prospective backing-array-sized logical envelope at EACH
    // insertion: no dependence on observed capacity, cancellation or GC.
    TensorTemporary(Times(8,Plus(currentRecords,1)));
   }
  }
  public void TensorProduct(long left,long right,bool grouped)
  {
   TensorSupport(left);TensorSupport(right);SequenceProduct(left,right);long pairs=Times(left,right);
   if(grouped)
   {
    // Right GroupBy is rebuilt for EVERY left group in the historical loop.
    // At most left groups: reserve all repeated grouping and group iteration.
    long groupedRecords=Plus(left,pairs);
    // Including SequenceProduct's left+pairs: total2*left+4*pairs.
    // This covers grouping input reads(left+pairs), ga records reread for
    // every gb group(pairs), gb records(pairs), and group iteration(left+pairs).
    // In particular scalar commuting factors can vanish without any Put
    // visits; that cancellation must not hide these input enumerations.
    TensorVisits(Plus(left,Times(3,pairs)));
    TensorTemporary(Times(8,Plus(groupedRecords,1)));
    Charge(ref metadataObjects,Times(4,Plus(groupedRecords,1)),limits.Tensors.MetadataObjects,"grouping metadata objects");
   }
  }
  public void SequenceProduct(long left,long right)
  {long pairs=Times(left,right);Charge(ref pairVisits,pairs,limits.Tensors.PairVisits,"Cartesian pairs before pruning");TensorVisits(Plus(left,pairs));}
  public void TensorSort(long records)
  {
   TensorSupport(records);TensorVisits(records);
   // Deliberately loose quadratic comparison envelope, not measured timing.
   Charge(ref sortWork,Times(4,Times(Plus(records,1),Plus(records,1))),limits.Tensors.SortWork,"tensor sort work");
   TensorTemporary(Times(8,Plus(records,1)));TensorSequence(records);
   Charge(ref metadataObjects,Plus(records,8),limits.Tensors.MetadataObjects,"sorted record metadata");
  }
  public void TensorSortComparisons(long comparisons)=>Charge(ref sortWork,comparisons,limits.Tensors.SortWork,"explicit sort component comparisons");
  public void MatrixDimension(long dimension)
  {Ready();if(dimension<0||dimension>limits.Matrices.Dimension||dimension>int.MaxValue)Reject("matrix dimension ceiling");}
  public void MatrixNew(long dimension)
  {
   MatrixDimension(dimension);long cells=Times(dimension,dimension);
   Charge(ref matrixObjects,1,limits.Matrices.Objects,"matrix objects");
   Charge(ref matrixCells,cells,limits.Matrices.Cells,"matrix backing cells");
   // Constructor body admission precedes the backing-array allocation, NOT
   // the CLR object header that was allocated to enter that constructor.
  }
  // Call-site-declared logical work. Reviewed source-access envelopes count
  // indexer/array touches and initialization; legacy callers may still use
  // loop units or deliberate overcharges. No uniform VM/memory/RSS meaning
  // follows merely from summing this counter; closure must audit every caller.
  public void MatrixVisits(long visits)=>Charge(ref matrixSlotVisits,visits,limits.Matrices.SlotVisits,"matrix logical work before pruning");
  public void MatrixLoop(long dimension,int power,long multiplier=1)
  {
   MatrixDimension(dimension);if(power<0||power>8)Reject("bounded matrix loop power");
   long visits=1;for(int i=0;i<power;i++)visits=Times(visits,dimension);MatrixVisits(Times(visits,multiplier));
  }
  public void RawArray(long slots,long objects=1)
  {
   if(slots<0||objects<0)Reject("negative raw-array reservation");
   Charge(ref arrayObjects,objects,limits.Matrices.ArrayObjects,"raw-array objects");
   Charge(ref arraySlots,slots,limits.Matrices.ArraySlots,"raw-array logical slots");
  }
  public void Dispose()
  {
   if(disposed)return;
   if(thread!=Environment.CurrentManagedThreadId||!ReferenceEquals(active,this))Reject("wrong scope disposal");
   disposed=true;active=null;
  }
 }
}
