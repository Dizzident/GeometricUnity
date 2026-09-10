using System.Globalization;
using System.Numerics;
using System.Text.Json;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

internal static class Feedback
{
 public static long MatrixProducts{get;private set;}
 private static Rational Mul(Rational a,Rational b){MatrixProducts++;return a*b;}
 public static Rational Parse(string text){var s=text.Split('/');if(s.Length>2)throw new ArgumentException("rational");return new Rational(BigInteger.Parse(s[0],CultureInfo.InvariantCulture),s.Length==1?BigInteger.One:BigInteger.Parse(s[1],CultureInfo.InvariantCulture));}
 public static FT Read(JsonElement e)
 {var t=new FT();foreach(var q in e.EnumerateArray()){var k=(Form:q.GetProperty("form").GetInt32(),Blade:q.GetProperty("blade").GetInt32(),K0:q.GetProperty("k0").GetInt32(),K1:q.GetProperty("k1").GetInt32());var z=new Scalar(Parse(q.GetProperty("real").GetString()!),Parse(q.GetProperty("imaginary").GetString()!));if(k.Form<0||k.Form>Full||k.Blade<0||k.Blade>Full||k.K0!=0||k.K1!=0||z.IsZero||t.ContainsKey(k))throw new ArgumentException("tensor record");Put(t,k,z);}return t;}
 public static int[] Bits(int mask)=>Enumerable.Range(0,14).Where(a=>(mask&(1<<a))!=0).ToArray();
 public static FT Slice(FT j,int a){var t=new FT();foreach(var q in j)if(q.Key.Form==(1<<a))Put(t,(0,q.Key.Blade,0,0),q.Value);return t;}
 public static FT Cyclic(FT j)
 {var t=new FT();foreach(var q in j){int a=Bits(q.Key.Form).Single();if((q.Key.Blade&(1<<a))==0)Put(t,(0,q.Key.Blade|(1<<a),0,0),q.Value*(Sigma(a)*Shuffle(1<<a,q.Key.Blade)));}return t;}
 // Vector action matrices for all fourteen bivector coefficients J_a.
 public static Rational[,,] Actions(FT j)
 {var l=new Rational[14,14,14];foreach(var q in j){int a=Bits(q.Key.Form).Single();var v=Bits(q.Key.Blade);if(v.Length!=2||q.Value.Imaginary!=0)throw new ArgumentException("bivector J");int c=v[0],d=v[1];l[a,c,d]+=q.Value.Real*(2*Sigma(d));l[a,d,c]-=q.Value.Real*(2*Sigma(c));}return l;}
 public static FT VectorAction(Rational[,,] l,int a,int b)
 {var t=new FT();for(int c=0;c<14;c++)Put(t,(0,1<<c,0,0),new Scalar(l[a,c,b],0));return t;}
 public static FT Quadratic(Rational[,,] l)
 {var f=new FT();for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)for(int c=0;c<14;c++)for(int d=c+1;d<14;d++){Rational z=0;for(int e=0;e<14;e++){if(l[a,c,e]!=0&&l[b,e,d]!=0)z+=Mul(l[a,c,e],l[b,e,d]);if(l[b,c,e]!=0&&l[a,e,d]!=0)z-=Mul(l[b,c,e],l[a,e,d]);}Put(f,((1<<a)|(1<<b),(1<<c)|(1<<d),0,0),new Scalar(z*new Rational(Sigma(d),2),0));}return f;}
 public static FT Adjoint(Rational[,,] l)
 {var t=new FT();for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)for(int c=0;c<14;c++)Put(t,((1<<a)|(1<<b),1<<c,0,0),new Scalar(l[a,c,b]-l[b,c,a],0));return t;}
 public static (FT First,FT Second,FT Full) Contraction(FT f)
 {
  var first=new FT();var second=new FT();Rational diagonal=0;
  foreach(var q in f)
  {
   var ab=Bits(q.Key.Form);var cd=Bits(q.Key.Blade);if(ab.Length!=2||cd.Length!=2||q.Value.Imaginary!=0)throw new ArgumentException("bivector two-form");
   int a=ab[0],b=ab[1],c=cd[0],d=cd[1];var z=q.Value;
   if(a==c)Put(first,(1<<b,1<<d,0,0),z*-2);if(a==d)Put(first,(1<<b,1<<c,0,0),z*2);
   if(b==c)Put(first,(1<<a,1<<d,0,0),z*2);if(b==d)Put(first,(1<<a,1<<c,0,0),z*-2);
   if(q.Key.Form==q.Key.Blade)diagonal+=z.Real;
   if((q.Key.Form&q.Key.Blade)==0){int blade=q.Key.Form|q.Key.Blade;int sign=Sigma(a)*Sigma(b)*Shuffle(q.Key.Form,q.Key.Blade);for(int k=0;k<14;k++)if((blade&(1<<k))==0)Put(second,(1<<k,blade|(1<<k),0,0),z*(-2*sign*Shuffle(1<<k,blade)));}
  }
  for(int k=0;k<14;k++)Put(second,(1<<k,1<<k,0,0),new Scalar(2*diagonal,0));
  return(first,second,Add(first,second));
 }
 public static FT DqAdjoint(Rational[,,] l,FT y)
 {var t=new FT();foreach(var q in y){var ab=Bits(q.Key.Form);int a=ab[0],b=ab[1],c=Bits(q.Key.Blade).Single();if(q.Value.Imaginary!=0)throw new ArgumentException("vector two-form");for(int d=0;d<14;d++){if(l[a,d,c]!=0)Put(t,(1<<b,1<<d,0,0),new Scalar(Mul(l[a,d,c],q.Value.Real)*-Sigma(a),0));if(l[b,d,c]!=0)Put(t,(1<<a,1<<d,0,0),new Scalar(Mul(l[b,d,c],q.Value.Real)*Sigma(b),0));}}return t;}
 public static bool Grades(FT t,params int[] grades)=>t.Keys.All(q=>grades.Contains(Degree(q.Blade))&&q.K0==0&&q.K1==0);
 public static FT Grade(FT t,int grade){var r=new FT();foreach(var q in t)if(Degree(q.Key.Blade)==grade)Put(r,q.Key,q.Value);return r;}
 public static IEnumerable<(int U,int V)> Monomials(int order){for(int n=0;n<=order;n++)for(int u=0;u<=n;u++)yield return(u,n-u);}
 public static Rational Get(Dictionary<(int U,int V),Rational> p,int u,int v)=>p.GetValueOrDefault((u,v));
 public static void PutScalar(Dictionary<(int U,int V),Rational> p,(int U,int V) m,Rational z)=>p[m]=p.GetValueOrDefault(m)+z;
 public static object[] MatrixRows(Rational[,,] l)=>Enumerable.Range(0,14).Select(a=>(object)new{axis=a,matrix=Enumerable.Range(0,14).Select(c=>Enumerable.Range(0,14).Select(d=>l[a,c,d].ToString()).ToArray()).ToArray()}).ToArray();
}
