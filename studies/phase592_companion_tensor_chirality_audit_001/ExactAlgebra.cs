using System.Numerics;
using System.Security.Cryptography;
using Tensor=System.Collections.Generic.Dictionary<(int Form,int Blade),Scalar>;
using PairMatrix=System.Collections.Generic.Dictionary<(int Row,int Column),long>;

// Phase591 arithmetic copied verbatim except public visibility and this wrapper.
// The bound upstream program and contract identify the original exact implementation.
internal static class Algebra
{
public static int Degree(int mask)=>BitOperations.PopCount((uint)mask);
public static int Sigma(int a)=>a<7?1:-1;
public static int Shuffle(int a,int b){int inversions=0;for(int i=0;i<14;i++)if((a&(1<<i))!=0)inversions+=Degree(b&((1<<i)-1));return inversions%2==0?1:-1;}
public static int BladeSign(int a,int b)=>Shuffle(a,b)*(Degree(a&b&0x3f80)%2==0?1:-1);
public static int WordProductSign(int a,int b)
{
 var word=Enumerable.Range(0,14).Where(i=>(a&(1<<i))!=0).ToList();int sign=1;
 foreach(int i in Enumerable.Range(0,14).Where(i=>(b&(1<<i))!=0))
 {int larger=word.Count(j=>j>i);if(larger%2!=0)sign=-sign;if(word.Contains(i)){word.Remove(i);sign*=Sigma(i);}else {word.Add(i);word.Sort();}}
 return sign;
}
public static int HodgeSign(int mask)=>Shuffle(mask,((1<<14)-1)^mask)*(Degree(mask&0x3f80)%2==0?1:-1);
public static void Put(Tensor t,(int Form,int Blade) key,Scalar value)
{if(value.IsZero)return;Scalar sum=t.GetValueOrDefault(key)+value;if(sum.IsZero)t.Remove(key);else t[key]=sum;}
public static Tensor Single(int form,int blade,Scalar value){var t=new Tensor();Put(t,(form,blade),value);return t;}
public static Tensor ScaleTensor(Tensor a,Scalar scale){var r=new Tensor();foreach(var x in a)Put(r,x.Key,x.Value*scale);return r;}
public static Tensor Star(Tensor a){var r=new Tensor();foreach(var x in a)Put(r,(((1<<14)-1)^x.Key.Form,x.Key.Blade),x.Value*HodgeSign(x.Key.Form));return r;}
public static Tensor UndoStarOne(Tensor a)
{if(a.Keys.Any(k=>Degree(k.Form)!=13))throw new InvalidOperationException("Expected degree13 before inverse star1");return Star(a);}
public static Tensor BracketWedge(Tensor a,Tensor b,char bracket)
{
 var r=new Tensor();foreach(var x in a)foreach(var y in b)
 {
  if((x.Key.Form&y.Key.Form)!=0)continue;
  int xy=BladeSign(x.Key.Blade,y.Key.Blade),yx=BladeSign(y.Key.Blade,x.Key.Blade);
  int coefficient=Shuffle(x.Key.Form,y.Key.Form)*(bracket=='C'?xy-yx:xy+yx);
  Scalar value=x.Value*y.Value*coefficient;if(bracket=='A')value*=Scalar.I;
  Put(r,(x.Key.Form|y.Key.Form,x.Key.Blade^y.Key.Blade),value);
 }
 return r;
}
public static bool Same(Tensor a,Tensor b)=>a.Count==b.Count&&a.All(x=>b.TryGetValue(x.Key,out var y)&&x.Value==y);
public static string Canonical(Tensor t)=>string.Join(";",t.OrderBy(x=>x.Key.Form).ThenBy(x=>x.Key.Blade).Select(x=>$"{x.Key.Form},{x.Key.Blade}:{x.Value.Real}:{x.Value.Imaginary}"));
public static object[] Terms(Tensor t)=>t.OrderBy(x=>x.Key.Form).ThenBy(x=>x.Key.Blade).Select(x=>(object)new {formMask=x.Key.Form,cliffordMask=x.Key.Blade,real=x.Value.Real.ToString(),imaginary=x.Value.Imaginary.ToString()}).ToArray();
public static List<(int A,int B)> Pairs(){var p=new List<(int,int)>();for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)p.Add((a,b));return p;}
public static List<(int A,int B,int C,int D)> Quadruples(){var q=new List<(int,int,int,int)>();for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)for(int c=b+1;c<14;c++)for(int d=c+1;d<14;d++)q.Add((a,b,c,d));return q;}
public static int Pair(int a,int b)=>a*(27-a)/2+b-a-1;
public static long Get(PairMatrix m,int i,int j)=>m.GetValueOrDefault((i,j));
public static long R(PairMatrix m,int a,int b,int c,int d)
{if(a==b||c==d)return 0;int sign=1;if(a>b){(a,b)=(b,a);sign=-sign;}if(c>d){(c,d)=(d,c);sign=-sign;}return checked(sign*Get(m,Pair(a,b),Pair(c,d)));}
public static void SetSym(PairMatrix m,int a,int b,long value){m[(a,b)]=value;m[(b,a)]=value;}
public static PairMatrix Diagonal(int a,int b,long value)=>new(){[(Pair(a,b),Pair(a,b))]=value};
public static List<BasisRow> RiemannBasis(List<(int A,int B)> pairs,List<(int A,int B,int C,int D)> quads)
{
 var rows=new List<BasisRow>();
 for(int i=0;i<pairs.Count;i++)rows.Add(new($"d-{i}","diagonal",(i,i),new PairMatrix{[(i,i)]=1}));
 for(int i=0;i<pairs.Count;i++)for(int j=i+1;j<pairs.Count;j++)
 {
  var a=pairs[i];var b=pairs[j];if(a.A!=b.A&&a.A!=b.B&&a.B!=b.A&&a.B!=b.B)continue;
  var m=new PairMatrix();SetSym(m,i,j,1);rows.Add(new($"s-{i}-{j}","shared-index",(i,j),m));
 }
 foreach(var q in quads)
 {
  var t1=(Pair(q.A,q.B),Pair(q.C,q.D));var t2=(Pair(q.A,q.C),Pair(q.B,q.D));var t3=(Pair(q.A,q.D),Pair(q.B,q.C));
  var m1=new PairMatrix();SetSym(m1,t1.Item1,t1.Item2,1);SetSym(m1,t2.Item1,t2.Item2,1);
  var m2=new PairMatrix();SetSym(m2,t2.Item1,t2.Item2,1);SetSym(m2,t3.Item1,t3.Item2,1);
  rows.Add(new($"q-{q.A}-{q.B}-{q.C}-{q.D}-1","bianchi-quadruple",t1,m1));
  rows.Add(new($"q-{q.A}-{q.B}-{q.C}-{q.D}-2","bianchi-quadruple",t3,m2));
 }
 return rows;
}
public static Tensor Curvature(PairMatrix matrix,List<(int A,int B)> pairs)
{
 var t=new Tensor();foreach(var entry in matrix)
 {var ab=pairs[entry.Key.Row];var cd=pairs[entry.Key.Column];Put(t,((1<<ab.A)|(1<<ab.B),(1<<cd.A)|(1<<cd.B)),
  new Scalar(new Rational(checked(entry.Value*Sigma(cd.A)*Sigma(cd.B)),2),0));}return t;
}
public static (long[,] Ricci,long Scalar) RicciScalar(PairMatrix m)
{
 var ricci=new long[14,14];for(int b=0;b<14;b++)for(int d=0;d<14;d++)for(int a=0;a<14;a++)ricci[b,d]=checked(ricci[b,d]+Sigma(a)*R(m,a,b,a,d));
 long scalar=0;for(int a=0;a<14;a++)scalar=checked(scalar+Sigma(a)*ricci[a,a]);return (ricci,scalar);
}
}
sealed record BasisRow(string Id,string Family,(int Row,int Column) Pivot,PairMatrix Matrix);
readonly struct Rational : IEquatable<Rational>
{
 private readonly long n,d;public long Numerator=>n;public long Denominator=>d==0?1:d;
 public Rational(long numerator,long denominator=1)
 {if(denominator==0)throw new DivideByZeroException();if(denominator<0){numerator=checked(-numerator);denominator=checked(-denominator);}
  long a=System.Math.Abs(numerator),b=denominator;while(b!=0){long r=a%b;a=b;b=r;}n=numerator/a;d=denominator/a;}
 public static implicit operator Rational(long n)=>new(n);
 public static Rational operator +(Rational a,Rational b)=>new(checked(a.n*b.Denominator+b.n*a.Denominator),checked(a.Denominator*b.Denominator));
 public static Rational operator -(Rational a,Rational b)=>new(checked(a.n*b.Denominator-b.n*a.Denominator),checked(a.Denominator*b.Denominator));
 public static Rational operator *(Rational a,Rational b)=>new(checked(a.n*b.n),checked(a.Denominator*b.Denominator));
 public bool Equals(Rational x)=>n==x.n&&Denominator==x.Denominator;public override bool Equals(object? x)=>x is Rational r&&Equals(r);
 public override int GetHashCode()=>HashCode.Combine(n,Denominator);public static bool operator ==(Rational a,Rational b)=>a.Equals(b);public static bool operator !=(Rational a,Rational b)=>!a.Equals(b);
 public override string ToString()=>Denominator==1?n.ToString(System.Globalization.CultureInfo.InvariantCulture):$"{n}/{Denominator}";
}
readonly struct Scalar(Rational real,Rational imaginary) : IEquatable<Scalar>
{
 public Rational Real{get;}=real;public Rational Imaginary{get;}=imaginary;
 public Scalar(long real):this(new Rational(real),0){}public static Scalar I=>new(0,1);public bool IsZero=>Real.Numerator==0&&Imaginary.Numerator==0;
 public static implicit operator Scalar(long value)=>new(value);
 public static Scalar operator +(Scalar a,Scalar b)=>new(a.Real+b.Real,a.Imaginary+b.Imaginary);
 public static Scalar operator *(Scalar a,Scalar b)=>new(a.Real*b.Real-a.Imaginary*b.Imaginary,a.Real*b.Imaginary+a.Imaginary*b.Real);
 public bool Equals(Scalar x)=>Real==x.Real&&Imaginary==x.Imaginary;public override bool Equals(object? x)=>x is Scalar s&&Equals(s);public override int GetHashCode()=>HashCode.Combine(Real,Imaginary);
 public static bool operator ==(Scalar a,Scalar b)=>a.Equals(b);public static bool operator !=(Scalar a,Scalar b)=>!a.Equals(b);
}
sealed class Binding(string idValue,string pathValue,string hashValue)
{
 public string id{get;}=idValue;public string path{get;}=pathValue;public string sha256{get;}=hashValue;
 public bool hashMatches=>File.Exists(path)&&Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant()==sha256;
}
