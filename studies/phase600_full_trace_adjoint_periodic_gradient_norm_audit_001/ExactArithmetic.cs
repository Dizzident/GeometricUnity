using System.Numerics;
using System.Security.Cryptography;

// Same Clifford conventions as the independently bound592 helper; unrestricted
// rational numerator/denominator size avoids overflow in finite Fourier products.
internal static class Algebra
{
 public static int Degree(int mask)=>BitOperations.PopCount((uint)mask);
 public static int Sigma(int axis)=>axis<7?1:-1;
 public static int Shuffle(int a,int b){int n=0;for(int i=0;i<14;i++)if((a&(1<<i))!=0)n+=Degree(b&((1<<i)-1));return n%2==0?1:-1;}
 public static int BladeSign(int a,int b)=>Shuffle(a,b)*(Degree(a&b&0x3f80)%2==0?1:-1);
 public static int WordSign(int a,int b)
 {var word=Enumerable.Range(0,14).Where(i=>(a&(1<<i))!=0).ToList();int s=1;foreach(int i in Enumerable.Range(0,14).Where(i=>(b&(1<<i))!=0)){if(word.Count(j=>j>i)%2!=0)s=-s;if(word.Contains(i)){word.Remove(i);s*=Sigma(i);}else{word.Add(i);word.Sort();}}return s;}
 public static int HodgeSign(int mask)=>Shuffle(mask,16383^mask)*(Degree(mask&0x3f80)%2==0?1:-1);
 public static int AdjointSign(int mask)=>Degree(mask)*(Degree(mask)+1)/2%2==0?1:-1;
}
readonly struct Rational : IEquatable<Rational>
{
 private readonly BigInteger n,d;
 public BigInteger Numerator=>n;public BigInteger Denominator=>d.IsZero?BigInteger.One:d;
 public Rational(long numerator,long denominator=1):this(new BigInteger(numerator),new BigInteger(denominator)){}
 public Rational(BigInteger numerator,BigInteger denominator)
 {if(denominator.IsZero)throw new DivideByZeroException();if(denominator.Sign<0){numerator=-numerator;denominator=-denominator;}var g=BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator),denominator);n=numerator/g;d=denominator/g;}
 public static implicit operator Rational(long n)=>new(n);
 public static Rational operator +(Rational a,Rational b)=>new(a.n*b.Denominator+b.n*a.Denominator,a.Denominator*b.Denominator);
 public static Rational operator -(Rational a,Rational b)=>new(a.n*b.Denominator-b.n*a.Denominator,a.Denominator*b.Denominator);
 public static Rational operator *(Rational a,Rational b)=>new(a.n*b.n,a.Denominator*b.Denominator);
 public bool Equals(Rational b)=>n==b.n&&Denominator==b.Denominator;public override bool Equals(object? b)=>b is Rational r&&Equals(r);public override int GetHashCode()=>HashCode.Combine(n,Denominator);
 public static bool operator ==(Rational a,Rational b)=>a.Equals(b);public static bool operator !=(Rational a,Rational b)=>!a.Equals(b);
 public override string ToString()=>Denominator.IsOne?n.ToString(System.Globalization.CultureInfo.InvariantCulture):$"{n}/{Denominator}";
}
readonly struct Scalar(Rational real,Rational imaginary) : IEquatable<Scalar>
{
 public Rational Real{get;}=real;public Rational Imaginary{get;}=imaginary;
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
 public bool hashMatches=>File.Exists(path)&&Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant()==sha256;
}
