using System.Globalization;
using System.Numerics;
using System.Text.Json;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

internal static class SpinGeometry
{
 public static Rational Parse(string s)
 {var v=s.Split('/');if(v.Length>2)throw new ArgumentException("rational");return new Rational(BigInteger.Parse(v[0],CultureInfo.InvariantCulture),v.Length==1?BigInteger.One:BigInteger.Parse(v[1],CultureInfo.InvariantCulture));}
 public static Matrix ReadMatrix(JsonElement e)
 {if(e.GetArrayLength()!=14||e.EnumerateArray().Any(r=>r.GetArrayLength()!=14))throw new ArgumentException("matrix shape");var m=new Matrix(14);for(int a=0;a<14;a++)for(int b=0;b<14;b++)m[a,b]=Parse(e[a][b].GetString()!);return m;}
 public static Rational[,,,] ReadCurvature(JsonElement e)
 {var t=new Rational[14,14,14,14];var seen=new HashSet<(int,int,int,int)>();foreach(var v in e.EnumerateArray()){int a=v.GetProperty("a").GetInt32(),b=v.GetProperty("b").GetInt32(),c=v.GetProperty("c").GetInt32(),d=v.GetProperty("d").GetInt32();Rational value=Parse(v.GetProperty("value").GetString()!);if(new[]{a,b,c,d}.Any(x=>x<0||x>=14)||value==0||!seen.Add((a,b,c,d)))throw new ArgumentException("curvature record");t[a,b,c,d]=value;}return t;}
 public static Matrix Transpose(Matrix m){var r=new Matrix(m.N);for(int a=0;a<m.N;a++)for(int b=0;b<m.N;b++)r[a,b]=m[b,a];return r;}
 public static Matrix Eta(){var r=new Matrix(14);for(int i=0;i<14;i++)r[i,i]=Sigma(i);return r;}
 public static Matrix Transport(int point)
 {var r=Matrix.Identity(14);if(point==0)return r;for(int i=0;i<4;i++)r[i,i]=new Rational(1,i+1);for(int i=0;i<10;i++){var p=Geometry.Pairs[i];r[i+4,i+4]=(p.I+1)*(p.J+1);}return r;}
 public static Matrix Frame(int point)
 {
  var e=new Matrix(14);e[0,0]=1;for(int i=1;i<4;i++)e[i,i+6]=1;
  int[,] rows={{1,1,-1,-1},{1,-1,1,-1},{1,-1,-1,1}};
  for(int k=0;k<3;k++)for(int i=0;i<4;i++)e[i+4,k+1]=new Rational((i==0?-1:1)*rows[k,i],2);
  for(int i=0;i<4;i++)e[i+4,10]=new Rational(i==0?1:-1,2);
  for(int i=0;i<3;i++){e[i+8,i+4]=new Rational(1,4);e[i+11,i+4]=new Rational(3,4);e[i+8,i+11]=new Rational(3,4);e[i+11,i+11]=new Rational(1,4);}
  return Transport(point)*e;
 }
 public static Matrix HandInverse(int point)
 {
  var e=new Matrix(14);e[0,0]=1;for(int i=1;i<4;i++)e[i+6,i]=1;
  int[,] rows={{-1,1,-1,-1},{-1,-1,1,-1},{-1,-1,-1,1}};
  for(int k=0;k<3;k++)for(int i=0;i<4;i++)e[k+1,i+4]=new Rational(rows[k,i],2);
  for(int i=0;i<4;i++)e[10,i+4]=new Rational(i==0?1:-1,2);
  for(int i=0;i<3;i++){e[i+4,i+11]=new Rational(3,2);e[i+4,i+8]=new Rational(-1,2);e[i+11,i+11]=new Rational(-1,2);e[i+11,i+8]=new Rational(3,2);}
  return e*Transport(point).Inverse();
 }
 // Route one: mix the two curvature inputs, then conjugate the complete
 // endomorphism. No metric-lowering identity is assumed on this route.
 public static Rational[,,,] EndomorphismFrame(Rational[,,,] r,Matrix e,Matrix inverse)
 {
  var t=new Rational[14,14,14,14];for(int a=0;a<14;a++)for(int b=0;b<14;b++)
  {
   var m=new Matrix(14);for(int i=0;i<14;i++)if(e[i,a]!=0)for(int j=0;j<14;j++)if(e[j,b]!=0)for(int c=0;c<14;c++)for(int d=0;d<14;d++)if(r[i,j,c,d]!=0)m[d,c]+=e[i,a]*e[j,b]*r[i,j,c,d];
   var changed=inverse*m*e;for(int c=0;c<14;c++)for(int d=0;d<14;d++)t[a,b,c,d]=changed[d,c];
  }return t;
 }
 public static Rational[,,,] Lower(Rational[,,,] r,Matrix g)
 {var t=new Rational[14,14,14,14];for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++)for(int d=0;d<14;d++)for(int k=0;k<14;k++)if(r[a,b,c,k]!=0&&g[k,d]!=0)t[a,b,c,d]+=r[a,b,c,k]*g[k,d];return t;}
 // Route two: successively transform four covariant slots of lowered R.
 public static Rational[,,,] LoweredFrame(Rational[,,,] input,Matrix e)
 {
  var current=input;for(int axis=0;axis<4;axis++)
  {var next=new Rational[14,14,14,14];for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++)for(int d=0;d<14;d++){int[] q=[a,b,c,d];int target=q[axis];Rational s=0;for(int old=0;old<14;old++)if(e[old,target]!=0){q[axis]=old;var v=current[q[0],q[1],q[2],q[3]];if(v!=0)s+=e[old,target]*v;}next[a,b,c,d]=s;}current=next;}return current;
 }
 public static FT Lift(Rational[,,,] low,bool omitMetric=false,Rational? factor=null,int sign=-1)
 {var f=new FT();Rational half=factor??new Rational(1,2);for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)for(int c=0;c<14;c++)for(int d=c+1;d<14;d++)if(low[a,b,c,d]!=0)Put(f,((1<<a)|(1<<b),(1<<c)|(1<<d),0,0),new Scalar(sign*half*low[a,b,c,d]*(omitMetric?1:Sigma(c)*Sigma(d)),0));return f;}
 public static FT Slice(FT f,int a,int b)
 {int mask=(1<<a)|(1<<b);var r=new FT();foreach(var q in f)if(q.Key.Form==mask)Put(r,(0,q.Key.Blade,0,0),q.Value);return r;}
 public static FT Vector(Rational[,,,] r,int a,int b,int c)
 {var v=new FT();for(int d=0;d<14;d++)Put(v,(0,1<<d,0,0),new Scalar(r[a,b,c,d],0));return v;}
 public static FT GammaOne()=>Enumerable.Range(0,14).Aggregate(new FT(),(s,a)=>Add(s,One(1<<a,1<<a,1)));
 public static FT GammaTwo(){var t=new FT();for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)Put(t,((1<<a)|(1<<b),(1<<a)|(1<<b),0,0),1);return t;}
 public static FT WeightedGamma(Matrix raised)
 {var t=new FT();for(int a=0;a<14;a++)for(int b=0;b<14;b++)Put(t,(1<<a,1<<b,0,0),new Scalar(raised[b,a],0));return t;}
 public static FT Chiral(FT a,int h)=>Add(a,Scale(Omega(a),h));
 public static FT[] Stages(FT f,FT phi1,FT phi2,bool first,bool naive)
 {FT Prod(FT a,FT b,char kind)=>naive?NaiveProduct(a,b,kind):Product(a,b,kind);var sf=Star(f);var one=first?Prod(phi1,sf,'C'):new FT();var inner=Prod(phi2,sf,'A');var zero=Star(inner);var outer=Prod(phi1,zero,'C');var upper=Add(one,Scale(Star(outer),Fourier.Half*-1));var lower=Star(upper);return [f,sf,one,inner,zero,outer,upper,lower];}
 public static Rational CoefficientSquare(FT f)=>f.Values.Aggregate((Rational)0,(s,c)=>s+c.Real*c.Real+c.Imaginary*c.Imaginary);
 public static Matrix Ricci(Rational[,,,] r){var m=new Matrix(14);for(int b=0;b<14;b++)for(int c=0;c<14;c++)for(int a=0;a<14;a++)m[b,c]+=r[a,b,c,a];return m;}
 public static object[] Nonzero(Rational[,,,] r)
 {var values=new List<object>();for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++)for(int d=0;d<14;d++)if(r[a,b,c,d]!=0)values.Add(new{a,b,c,d,value=r[a,b,c,d].ToString()});return values.ToArray();}
}
