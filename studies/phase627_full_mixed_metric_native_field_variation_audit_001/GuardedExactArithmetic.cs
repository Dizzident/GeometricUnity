#if A68_GUARDED_EXACT_ARITHMETIC
using System.Numerics;
using System.Security.Cryptography;

// Phase627-local guarded derivative of the COMPLETE47-line source:
// ../phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/ExactArithmetic.cs
// SHA25643e4c103ef58602f0876994209762b4c5a2be833252e10fb1543f6237c514b9f
// Guards preserve exact algebra, operand order, reduction and default Rational.
// Conditional until the production project explicitly replaces its old link.
// This is only primitive admission, NOT full producer resource authorization.
internal static class Algebra
{
 public static int Degree(int mask)=>BitOperations.PopCount((uint)mask);
 public static int Sigma(int axis)=>axis<7?1:-1;
 public static int Shuffle(int a,int b){int n=0;for(int i=0;i<14;i++)if((a&(1<<i))!=0)n+=Degree(b&((1<<i)-1));return n%2==0?1:-1;}
 public static int BladeSign(int a,int b)=>Shuffle(a,b)*(Degree(a&b&0x3f80)%2==0?1:-1);
 public static int WordSign(int a,int b)
 {MixedProducerAdmission.Current.Word();var word=Enumerable.Range(0,14).Where(i=>(a&(1<<i))!=0).ToList();int s=1;foreach(int i in Enumerable.Range(0,14).Where(i=>(b&(1<<i))!=0)){if(word.Count(j=>j>i)%2!=0)s=-s;if(word.Contains(i)){word.Remove(i);s*=Sigma(i);}else{word.Add(i);word.Sort();}}return s;}
 public static int HodgeSign(int mask)=>Shuffle(mask,16383^mask)*(Degree(mask&0x3f80)%2==0?1:-1);
 public static int AdjointSign(int mask)=>Degree(mask)*(Degree(mask)+1)/2%2==0?1:-1;
}
readonly struct Rational : IEquatable<Rational>
{
 private readonly BigInteger n,d;
 public BigInteger Numerator=>n;public BigInteger Denominator=>d.IsZero?BigInteger.One:d;
 public Rational(long numerator,long denominator=1):this(MixedProducerAdmission.Current.FromLong(numerator),MixedProducerAdmission.Current.FromLong(denominator)){}
 public Rational(BigInteger numerator,BigInteger denominator)
 {
  var admission=MixedProducerAdmission.Current;admission.RationalConstruction(numerator,denominator);
  if(denominator.IsZero){admission.Poison();throw new DivideByZeroException();}
  if(denominator.Sign<0){admission.Unary(numerator);numerator=-numerator;admission.Unary(denominator);denominator=-denominator;}
  admission.Unary(numerator);var absolute=BigInteger.Abs(numerator);admission.Gcd(absolute,denominator);var g=BigInteger.GreatestCommonDivisor(absolute,denominator);
  admission.Binary("divide",numerator,g);n=numerator/g;admission.Binary("divide",denominator,g);d=denominator/g;admission.Reduced(n,d);
 }
 public static implicit operator Rational(long n)=>new(n);
 public static Rational operator +(Rational a,Rational b)
 {
  var admission=Inputs(a,b);admission.Binary("multiply",a.n,b.Denominator);var left=a.n*b.Denominator;
  admission.Binary("multiply",b.n,a.Denominator);var right=b.n*a.Denominator;admission.Binary("add",left,right);var numerator=left+right;
  admission.Binary("multiply",a.Denominator,b.Denominator);var denominator=a.Denominator*b.Denominator;return new(numerator,denominator);
 }
 public static Rational operator -(Rational a,Rational b)
 {
  var admission=Inputs(a,b);admission.Binary("multiply",a.n,b.Denominator);var left=a.n*b.Denominator;
  admission.Binary("multiply",b.n,a.Denominator);var right=b.n*a.Denominator;admission.Binary("subtract",left,right);var numerator=left-right;
  admission.Binary("multiply",a.Denominator,b.Denominator);var denominator=a.Denominator*b.Denominator;return new(numerator,denominator);
 }
 public static Rational operator *(Rational a,Rational b)
 {
  var admission=Inputs(a,b);admission.Binary("multiply",a.n,b.n);var numerator=a.n*b.n;
  admission.Binary("multiply",a.Denominator,b.Denominator);var denominator=a.Denominator*b.Denominator;return new(numerator,denominator);
 }
 static MixedProducerAdmission.Scope Inputs(Rational a,Rational b)
 {var admission=MixedProducerAdmission.Current;admission.RationalInputs(a.n,a.Denominator);admission.RationalInputs(b.n,b.Denominator);return admission;}
 public bool Equals(Rational b)
 {var admission=Inputs(this,b);admission.Compare(n,b.n);if(n!=b.n)return false;admission.Compare(Denominator,b.Denominator);return Denominator==b.Denominator;}
 public override bool Equals(object? b)=>b is Rational r&&Equals(r);
 public override int GetHashCode()
 {var admission=MixedProducerAdmission.Current;admission.RationalInputs(n,Denominator);admission.Compare(n,n);admission.Compare(Denominator,Denominator);return HashCode.Combine(n,Denominator);}
 public static bool operator ==(Rational a,Rational b)=>a.Equals(b);public static bool operator !=(Rational a,Rational b)=>!a.Equals(b);
 public override string ToString()
 {MixedProducerAdmission.Current.Format(n,Denominator,Denominator.IsOne);return Denominator.IsOne?n.ToString(System.Globalization.CultureInfo.InvariantCulture):$"{n}/{Denominator}";}
}
readonly struct Scalar(Rational real,Rational imaginary) : IEquatable<Scalar>
{
 public Rational Real{get;}=Admit(real);public Rational Imaginary{get;}=Admit(imaginary);
 static Rational Admit(Rational value){MixedProducerAdmission.Current.RationalInputs(value.Numerator,value.Denominator);return value;}
 public Scalar(long real):this(new Rational(real),0){}
 public static implicit operator Scalar(long n)=>new(n);public static Scalar I=>new(0,1);
 public bool IsZero=>Real==0&&Imaginary==0;public Scalar Conjugate()=>new(Real,Imaginary*-1);
 public static Scalar operator +(Scalar a,Scalar b)=>new(a.Real+b.Real,a.Imaginary+b.Imaginary);
 public static Scalar operator *(Scalar a,Scalar b)=>new(a.Real*b.Real-a.Imaginary*b.Imaginary,a.Real*b.Imaginary+a.Imaginary*b.Real);
 public bool Equals(Scalar b)=>Real==b.Real&&Imaginary==b.Imaginary;public override bool Equals(object? b)=>b is Scalar s&&Equals(s);public override int GetHashCode()=>HashCode.Combine(Real,Imaginary);
 public static bool operator ==(Scalar a,Scalar b)=>a.Equals(b);public static bool operator !=(Scalar a,Scalar b)=>!a.Equals(b);
}
sealed class Binding(string idValue,string pathValue,string hashValue)
{
 public string id{get;}=idValue;public string path{get;}=pathValue;public string sha256{get;}=hashValue;
 public bool hashMatches
 {
  get
  {
   var admission=MixedProducerAdmission.Current;if(!File.Exists(path))return false;
   // Read from the same opened handle whose length is admitted. Exact original
   // SHA semantics, but no unbounded File.ReadAllBytes TOCTOU growth allocation.
   try
   {
    using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);
    long length=stream.Length;if(length>int.MaxValue)admission.Reject("binding byte-array implementation ceiling");
    admission.ReadBytes(length+1); // Include the growth-detection sentinel read.
    var bytes=new byte[(int)length];stream.ReadExactly(bytes);if(stream.ReadByte()!=-1)admission.Reject("binding changed length while reading");
    return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()==sha256;
   }
   catch{admission.Poison();throw;}
  }
 }
}
#endif
