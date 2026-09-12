using System.Text.Json;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Owned implementation: homogeneous full covariant derivatives are constructed
// from passed618 matrices, never from expected invariant response coefficients.
internal static class Fifth
{
 public static long SlotProducts{get;private set;}
 public static Rational[,] Matrix(JsonElement e)
 {if(e.GetArrayLength()!=14||e.EnumerateArray().Any(r=>r.GetArrayLength()!=14))throw new ArgumentException("14 matrix");var m=new Rational[14,14];for(int a=0;a<14;a++)for(int b=0;b<14;b++)m[a,b]=Feedback.Parse(e[a][b].GetString()!);return m;}
 public static object MatrixText(Rational[,] m)=>Enumerable.Range(0,14).Select(a=>Enumerable.Range(0,14).Select(b=>m[a,b].ToString()).ToArray()).ToArray();
 public static FT Diagonal(Rational h,Rational e,Rational trace)
 {var t=new FT();for(int a=0;a<14;a++)Put(t,(1<<a,1<<a,0,0),new Scalar(a==10?trace:a is 0 or 7 or 8 or 9?h:e,0));return t;}
 public static FT B()
 {var t=new FT();foreach(int a in new[]{0,7,8,9})Put(t,(1<<a,(1<<a)|(1<<10),0,0),new Scalar(new Rational(-1,2)*BladeSign(1<<a,1<<10),0));return t;}
 public static FT C()
 {var t=new FT();foreach(int a in new[]{1,2,3,4,5,6,11,12,13})Put(t,(1<<a,(1<<a)|(1<<10),0,0),BladeSign(1<<a,1<<10));return t;}
 public static FT Reflect(FT t)
 {var r=new FT();foreach(var q in t)Put(r,q.Key,q.Value*((((q.Key.Form>>10)&1)+((q.Key.Blade>>10)&1))%2==0?1:-1));return r;}
 public static FT Spin(Rational[,] m)
 {var t=new FT();for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)Put(t,(0,(1<<a)|(1<<b),0,0),new Scalar(new Rational(Sigma(b),2)*m[a,b],0));return t;}
 // Primary covector route: replace an entry in an explicit ordered index list,
 // reject duplicate entries, then count inversions without the mask shuffle.
 public static FT Covector(Rational[,] m,FT t)
 {
  var result=new FT();foreach(var q in t){int[] indices=Feedback.Bits(q.Key.Form);for(int slot=0;slot<indices.Length;slot++)for(int b=0;b<14;b++)
  {int a=indices[slot];if(m[a,b]==0||indices.Where((_,i)=>i!=slot).Contains(b))continue;int[] output=(int[])indices.Clone();output[slot]=b;int inversions=0;for(int i=0;i<output.Length;i++)for(int j=i+1;j<output.Length;j++)if(output[i]>output[j])inversions++;int mask=output.Aggregate(0,(x,y)=>x|(1<<y));SlotProducts++;Put(result,(mask,q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*new Scalar(m[a,b]*(inversions%2==0?-1:1),0));}}
  return result;
 }
 public static FT Derivative(Rational[,] m,FT t)=>Add(Product(Spin(m),t,'C'),Covector(m,t));
 // Independent full exterior-slot route: metric-skew Clifford commutators
 // equal the induced exterior representation on blades of EVERY grade.
 public static FT ExteriorSlots(Rational[,] m,FT t)
 {
  var r=new FT();foreach(var q in t)for(int a=0;a<14;a++)
  {
   if((q.Key.Form&(1<<a))!=0){int rest=q.Key.Form^(1<<a);int sign=Degree(rest&((1<<a)-1))%2==0?1:-1;for(int b=0;b<14;b++)if(m[a,b]!=0&&(rest&(1<<b))==0){SlotProducts++;Put(r,(rest|(1<<b),q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*new Scalar(-sign*Shuffle(1<<b,rest)*m[a,b],0));}}
   if((q.Key.Blade&(1<<a))!=0){int rest=q.Key.Blade^(1<<a);int sign=Degree(rest&((1<<a)-1))%2==0?1:-1;for(int b=0;b<14;b++)if(m[b,a]!=0&&(rest&(1<<b))==0){SlotProducts++;Put(r,(q.Key.Form,rest|(1<<b),q.Key.K0,q.Key.K1),q.Value*new Scalar(sign*Shuffle(1<<b,rest)*m[b,a],0));}}
  }return r;
 }
 public static FT Contract(FT t,int a)
 {var r=new FT();foreach(var q in t)if((q.Key.Form&(1<<a))!=0)Put(r,(q.Key.Form^(1<<a),q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*(Degree(q.Key.Form&((1<<a)-1))%2==0?1:-1));return r;}
 public static FT Divergence(FT[] derivatives)
 {var t=new FT();for(int a=0;a<14;a++)t=Add(t,Scale(Contract(derivatives[a],a),-Sigma(a)));return t;}
 public static Kinetic KineticOperator(Rational[][,] lambda,FT t)
 {
  var derivatives=lambda.Select(m=>Derivative(m,t)).ToArray();var alternate=lambda.Select(m=>ExteriorSlots(m,t)).ToArray();var d=new FT();
  for(int a=0;a<14;a++)d=Add(d,Product(One(1<<a,0,1),derivatives[a]));
  var stages=Caa.ForwardStages(d);var naive=Caa.ForwardStages(d,true);var legs=Caa.AdjointLegs(t);var kad=Add(legs.First,legs.Second);
  var reverse=lambda.Select(m=>Derivative(m,kad)).ToArray();var reverseAlternate=derivatives.Select(Caa.AdjointLiteral).ToArray();var divergence=Divergence(reverse);
  return new(t,derivatives,alternate,d,stages,naive,legs.First,legs.Second,kad,reverse,reverseAlternate,divergence,Scale(Add(stages[7],divergence),Fourier.Half));
 }
 public static Bilinear Polarize(FT x,FT y)
 {
  var xy=Product(x,y);var yx=Product(y,x);var q=Scale(Add(xy,yx),Fourier.Half);
  var naiveQ=Scale(Add(NaiveProduct(x,y,'W'),NaiveProduct(y,x,'W')),Fourier.Half);
  var ax=Caa.AdjointLiteral(x);var ay=Caa.AdjointLiteral(y);var dx=Adjoint.DQAdjoint(x,ay);var dy=Adjoint.DQAdjoint(y,ax);var dq=Scale(Add(dx,dy),Fourier.Half);
  var stages=Caa.ForwardStages(q);var naive=Caa.ForwardStages(q,true);
  return new(x,y,xy,yx,q,naiveQ,ax,ay,dx,dy,dq,stages,naive,Scale(Add(stages[7],dq),new Scalar(new Rational(1,3),0)));
 }
 public static FT CrossQ(FT x,FT y)=>Add(Product(x,y),Product(y,x));
 public static FT Evaluate(FT[] coefficients,Rational gamma)
 {var t=new FT();Rational power=1;foreach(var c in coefficients){t=Add(t,Scale(c,new Scalar(power,0)));power*=gamma;}return t;}
 public static FT[] Zeros(int n)=>Enumerable.Range(0,n).Select(_=>new FT()).ToArray();
 public static FT DiagonalPolarization(FT x,FT y)
 {
  Rational tauX=0,tauY=0,dot=0;for(int a=0;a<14;a++){Rational u=x.GetValueOrDefault((1<<a,1<<a,0,0)).Real,v=y.GetValueOrDefault((1<<a,1<<a,0,0)).Real;tauX+=u;tauY+=v;dot+=u*v;}
  var t=new FT();for(int a=0;a<14;a++){Rational u=x.GetValueOrDefault((1<<a,1<<a,0,0)).Real,v=y.GetValueOrDefault((1<<a,1<<a,0,0)).Real;Put(t,(1<<a,1<<a,0,0),new Scalar(2*((tauX-u)*(tauY-v)-(dot-u*v)),0));}return t;
 }
 public static bool Constant(FT t)=>t.Keys.All(k=>k.K0==0&&k.K1==0);
 public static bool RealType(FT t,int degree)=>Constant(t)&&Typed(t,degree)&&HAnti(t);
}
internal sealed record Kinetic(FT Input,FT[] Derivatives,FT[] Alternate,FT Exterior,FT[] Stages,FT[] NaiveStages,FT AdjointFirst,FT AdjointSecond,FT Adjoint,FT[] ReverseDerivatives,FT[] AlternateReverse,FT Reverse,FT Full)
{
 public object Evidence()=>new{input=Terms(Input),derivatives=Derivatives.Select(Terms).ToArray(),alternateDerivatives=Alternate.Select(Terms).ToArray(),exteriorDerivative=Terms(Exterior),stages=Stages.Select(Terms).ToArray(),naiveStages=NaiveStages.Select(Terms).ToArray(),adjointFirst=Terms(AdjointFirst),adjointSecond=Terms(AdjointSecond),adjoint=Terms(Adjoint),reverseDerivatives=ReverseDerivatives.Select(Terms).ToArray(),alternateReverse=AlternateReverse.Select(Terms).ToArray(),forward=Terms(Stages[7]),reverse=Terms(Reverse),full=Terms(Full)};
}
internal sealed record Bilinear(FT X,FT Y,FT XY,FT YX,FT Q,FT NaiveQ,FT AdjointX,FT AdjointY,FT DqX,FT DqY,FT Dq,FT[] Stages,FT[] NaiveStages,FT Full)
{
 public object Evidence()=>new{x=Terms(X),y=Terms(Y),xy=Terms(XY),yx=Terms(YX),halfCrossQ=Terms(Q),naiveHalfCrossQ=Terms(NaiveQ),adjointX=Terms(AdjointX),adjointY=Terms(AdjointY),firstAdjointComposite=Terms(DqX),secondAdjointComposite=Terms(DqY),halfAdjointComposite=Terms(Dq),stages=Stages.Select(Terms).ToArray(),naiveStages=NaiveStages.Select(Terms).ToArray(),full=Terms(Full)};
}
