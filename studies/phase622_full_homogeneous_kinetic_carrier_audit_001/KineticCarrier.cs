using System.Text.Json;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Full exterior/Clifford coefficient operations. No response matrix is used
// to compute a derivative, adjoint, or kinetic image.
internal static class Kinetic
{
 public static readonly int[] Horizontal=[0,7,8,9];
 public static readonly int[] Traceless=[1,2,3,4,5,6,11,12,13];
 public static long ComputeCalls{get;private set;}
 public static long ConnectionActions{get;private set;}
 public static long DerivativeSlots{get;private set;}
 public static FT Read(JsonElement e)
 {
  var t=new FT();foreach(var q in e.EnumerateArray())
  {var k=(Form:q.GetProperty("form").GetInt32(),Blade:q.GetProperty("blade").GetInt32(),K0:q.GetProperty("k0").GetInt32(),K1:q.GetProperty("k1").GetInt32());string real=q.GetProperty("real").GetString()!,imaginary=q.GetProperty("imaginary").GetString()!;var z=new Scalar(SpinGeometry.Parse(real),SpinGeometry.Parse(imaginary));if(k.Form<0||k.Form>Full||k.Blade<0||k.Blade>Full||k.K0!=0||k.K1!=0||z.IsZero||t.ContainsKey(k)||z.Real.ToString()!=real||z.Imaginary.ToString()!=imaginary)throw new ArgumentException("canonical full tensor input");Put(t,k,z);}return t;
 }
 public static Matrix Projector(IEnumerable<int> axes){var p=new Matrix(14);foreach(int a in axes)p[a,a]=1;return p;}
 public static FT Vector(Matrix p){var f=new FT();for(int a=0;a<14;a++)for(int b=0;b<14;b++)Put(f,(1<<a,1<<b,0,0),new Scalar(p[b,a],0));return f;}
 public static FT WedgeCl(FT a,FT b)
 {var t=new FT();foreach(var x in a)foreach(var y in b)if((x.Key.Form&y.Key.Form)==0&&(x.Key.Blade&y.Key.Blade)==0)Put(t,(x.Key.Form|y.Key.Form,x.Key.Blade|y.Key.Blade,0,0),x.Value*y.Value*(Shuffle(x.Key.Form,y.Key.Form)*Shuffle(x.Key.Blade,y.Key.Blade)));return t;}
 public static FT[] Fields(Matrix[] lambda)
 {
  var ph=Projector(Horizontal);var pt=Projector(Traceless);var pr=Projector([10]);var j=new FT();var b=new FT();var c=new FT();
  for(int a=0;a<14;a++)
  {var u=ph*lambda[a]*pt+pt*lambda[a]*ph;var r=ph*lambda[a]*pr+pr*lambda[a]*ph;j=Add(j,Product(One(1<<a,0,1),Scale(Homogeneous.SpinGenerator(u),-4)));b=Add(b,Product(One(1<<a,0,1),Scale(Homogeneous.SpinGenerator(r),-4)));}
  foreach(int a in Traceless)c=Add(c,WedgeCl(One(1<<a,1<<a,1),One(0,1<<10,1)));
  return[Vector(ph),Vector(pt),Vector(pr),j,b,c,Add(Scale(b,-1),c)];
 }
 // Independent ordered-slot replacement for covectors. The Clifford part
 // is an actual commutator with the spin lift, not a bivector-only formula.
 public static FT FormAction(Matrix l,FT input)
 {
  var t=new FT();foreach(var q in input)
  {var slots=Enumerable.Range(0,14).Where(a=>(q.Key.Form&(1<<a))!=0).ToArray();for(int s=0;s<slots.Length;s++)for(int b=0;b<14;b++)if(l[slots[s],b]!=0)
   {int mask=0,sign=1;bool zero=false;for(int k=0;k<slots.Length;k++){int bit=1<<(k==s?b:slots[k]);if((mask&bit)!=0){zero=true;break;}sign*=Shuffle(mask,bit);mask|=bit;}if(!zero)Put(t,(mask,q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*new Scalar(l[slots[s],b]*-sign,0));}}
  return t;
 }
 public static FT SpinAction(Matrix l,FT input)=>Product(Homogeneous.SpinGenerator(l),input,'C');
 public static FT Action(Matrix l,FT input){ConnectionActions++;return Add(FormAction(l,input),SpinAction(l,input));}
 public static FT Contract(FT input,int a)
 {var t=new FT();foreach(var q in input)if((q.Key.Form&(1<<a))!=0)Put(t,(q.Key.Form^(1<<a),q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*(Degree(q.Key.Form&((1<<a)-1))%2==0?1:-1));return t;}
 public static FT Exterior(FT[] derivatives)
 {var t=new FT();for(int a=0;a<14;a++)t=Add(t,Product(One(1<<a,0,1),derivatives[a]));return t;}
 public static FT Codifferential(FT[] derivatives)
 {var t=new FT();for(int a=0;a<14;a++)t=Add(t,Scale(Contract(derivatives[a],a),-Sigma(a)));return t;}
 public static FT Cyclic(FT input)
 {var t=new FT();foreach(var q in input)if((q.Key.Form&q.Key.Blade)==0)Put(t,(0,q.Key.Form|q.Key.Blade,0,0),q.Value*((Degree(q.Key.Form&0x3f80)%2==0?1:-1)*Shuffle(q.Key.Form,q.Key.Blade)));return t;}
 public static bool Grades(FT input,params int[] grades)=>input.Keys.All(k=>grades.Contains(Degree(k.Blade))&&k.K0==0&&k.K1==0);
 public sealed record Result(FT Input,FT[] Derivatives,FT[] DerivativeOracles,FT ExteriorDerivative,FT ExteriorOracle,FT AdjointFirst,FT AdjointSecond,FT Adjoint,FT SimplifiedAdjoint,FT[] AdjointDerivatives,FT[] ParallelAdjoints,FT[][] Stages,FT Reverse,FT ParallelReverse,FT Forward,FT Full);
 public static Result Compute(Matrix[] lambda,FT input)
 {
  ComputeCalls++;var legs=Caa.AdjointLegs(input);var adjoint=Add(legs.First,legs.Second);var simplified=Caa.AdjointSimplified(input);var derivative=new FT[14];var oracle=new FT[14];var ad=new FT[14];var parallel=new FT[14];
  for(int a=0;a<14;a++){DerivativeSlots++;derivative[a]=Action(lambda[a],input);oracle[a]=Homogeneous.Action(lambda[a],input);ad[a]=Action(lambda[a],adjoint);parallel[a]=Caa.AdjointLiteral(derivative[a]);}
  var exterior=Exterior(derivative);var exteriorOracle=Exterior(oracle);FT[][] stages=[Caa.ForwardStages(exterior),Caa.ForwardStages(exterior,true)];var reverse=Codifferential(ad);var parallelReverse=Codifferential(parallel);var forward=stages[0][7];return new(input,derivative,oracle,exterior,exteriorOracle,legs.First,legs.Second,adjoint,simplified,ad,parallel,stages,reverse,parallelReverse,forward,Scale(Add(forward,reverse),Fourier.Half));
 }
 public static Rational[] Current(FT y,FT variation)=>Enumerable.Range(0,14).Select(a=>Pair(Contract(y,a),variation)*Sigma(a)).ToArray();
 public static Rational Divergence(Matrix[] lambda,Rational[] current)
 {Rational sum=0;for(int a=0;a<14;a++)for(int b=0;b<14;b++)sum+=lambda[a][a,b]*current[b];return sum;}
 public static FT LinearCombination(FT[] basis,params Rational[] coefficients)
 {var t=new FT();for(int i=0;i<coefficients.Length;i++)t=Add(t,Scale(basis[i],new Scalar(coefficients[i],0)));return t;}
 public static object Evidence(Result r)=>new{input=Terms(r.Input),derivatives=r.Derivatives.Select(Terms),derivativeOracles=r.DerivativeOracles.Select(Terms),exteriorDerivative=Terms(r.ExteriorDerivative),exteriorOracle=Terms(r.ExteriorOracle),adjointFirst=Terms(r.AdjointFirst),adjointSecond=Terms(r.AdjointSecond),adjoint=Terms(r.Adjoint),simplifiedAdjoint=Terms(r.SimplifiedAdjoint),adjointDerivatives=r.AdjointDerivatives.Select(Terms),parallelAdjoints=r.ParallelAdjoints.Select(Terms),stages=r.Stages.Select(c=>c.Select(Terms)),reverse=Terms(r.Reverse),parallelReverse=Terms(r.ParallelReverse),forward=Terms(r.Forward),full=Terms(r.Full)};
}
