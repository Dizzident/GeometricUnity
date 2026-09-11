using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

internal static class ActionVariation
{
 // Pullback derivative on every covector slot, with Clifford labels fixed.
 public static FT FormMotion(Matrix a,FT input)
 {
  if(input.Keys.Any(k=>k.K0!=0||k.K1!=0))throw new ArgumentException("metric audit form motion is a zero-frequency local tensor operation");
  var result=new FT();foreach(var q in input)for(int old=0;old<14;old++)if((q.Key.Form&(1<<old))!=0)
  {int rest=q.Key.Form^(1<<old),remove=Degree(rest&((1<<old)-1))%2==0?1:-1;for(int b=0;b<14;b++)if((rest&(1<<b))==0&&a[old,b]!=0)Put(result,(rest|(1<<b),q.Key.Blade,0,0),q.Value*new Scalar(a[old,b]*remove*Shuffle(1<<b,rest),0));}return result;
 }
 public static FT SpinOneForm(Matrix[] connection,Matrix e)
 {
  var result=new FT();for(int a=0;a<14;a++)
  {var m=new Matrix(14);for(int i=0;i<14;i++)if(e[i,a]!=0)m=m+connection[i].Scale(e[i,a]);var spin=Homogeneous.SpinGenerator(m);result=Add(result,Product(One(1<<a,0,1),spin));}return result;
 }
 public static FT CommutatorOneForms(FT b,FT t)=>Add(Product(b,t),Product(t,b));
 public static FT StarMotion(Matrix a,FT input)=>Add(Star(FormMotion(a,input)),Scale(FormMotion(a,Star(input)),-1));
 private static Jet MovingStar(Jet input,Matrix a)=>new(Star(input.Value),Add(Star(input.Delta),StarMotion(a,input.Value)));
 public static Jet[] FixedFrameChain(Jet f,Matrix a)
 {
  Jet StarJ(Jet t)=>MovingStar(t,a);
  var p1=new Jet(Caa.Gamma1,Scale(FormMotion(a,Caa.Gamma1),-1));var p2=new Jet(Caa.Gamma2,Scale(FormMotion(a,Caa.Gamma2),-1));
  var sf=StarJ(f);var one=Jet.Product(p1,sf,'C');var inner=Jet.Product(p2,sf,'A');var zero=StarJ(inner);var outer=Jet.Product(p1,zero,'A');var upper=Jet.Add(one,Jet.Scale(StarJ(outer),Fourier.Half*-1));var lower=StarJ(upper);
  Jet[] result=[f,sf,one,inner,zero,outer,upper,lower];int[] degrees=[2,12,13,14,0,1,13,1];for(int i=0;i<8;i++)if(!Typed(result[i].Value,degrees[i])||!Typed(result[i].Delta,degrees[i]))throw new InvalidOperationException("moving chain type");return result;
 }
 public static (Rational Value,Rational Delta) OriginalPair(Jet a,Jet b,Matrix motion)
 {var top=Jet.Product(a,MovingStar(b,motion));return(Top(top.Value),Top(top.Delta));}
 public static FT[] Fields(Matrix e)
 {
  var gamma=Caa.Gamma1;var u=One(1,(1<<0)|(1<<1),1);var mixed=Add(gamma,u);var central=Add(mixed,One(1<<1,0,Scalar.I));var mass=new FT();var third=new FT();for(int a=0;a<14;a++){Put(mass,(1<<a,1,0,0),new Scalar(e[0,a]+e[4,a],0));Put(third,(1<<a,1,0,0),new Scalar(e[8,a],0));}return[gamma,mixed,central,mass,third];
 }
 public static readonly Rational[][] BaselineForecasts=
 [
  [60,0,-1456,-7],
  [60,new(-13,4),new(-4372,3),new(-13,2)],
  [60,new(-13,4),new(-4372,3),-6],
  [new(21,4),0,0,new(-3,4)]
 ];
 public static Rational[] Forecast(int point,int field)=>field<4?BaselineForecasts[field]:[0,0,0,point==0?new Rational(1,4):1];
 public sealed record Result(Rational[] Values,Rational[] Direct,Rational[] Expanded,FT[] DeltaInputs,Jet[][] FixedStages,FT[][] AdaptedValueStages,FT[][] AdaptedDeltaStages);
 public static Result Evaluate(FT t,FT b,FT fb,FT deltaB,FT deltaFAdapted,Matrix motion)
 {
  var tdot=FormMotion(motion,t);var bdotAdapted=Add(deltaB,FormMotion(motion,b));var dt=CommutatorOneForms(b,t);var ddt=Add(CommutatorOneForms(bdotAdapted,t),CommutatorOneForms(b,tdot));
  var q=Product(t,t);var dq=Add(Product(tdot,t),Product(t,tdot));
  FT[] input=[fb,dt,q];FT[] variation=[deltaFAdapted,ddt,dq];
  // Actual original action in the fixed baseline frame. The native coordinate
  // field is fixed, while the spin reference, both solder tensors, every Hodge
  // occurrence, and final top form are differentiated by dual product rules.
  var fFixed=new Jet(fb,Add(deltaFAdapted,Scale(FormMotion(motion,fb),-1)));
  var dtFixed=new Jet(dt,CommutatorOneForms(deltaB,t));var qFixed=Jet.Fixed(q);
  Jet[][] stages=[FixedFrameChain(fFixed,motion),FixedFrameChain(dtFixed,motion),FixedFrameChain(qFixed,motion)];
  var direct=new Rational[4];var values=new Rational[4];Rational[] weights=[1,new(1,2),new(1,3),new(1,2)];
  for(int piece=0;piece<4;piece++){var pair=OriginalPair(Jet.Fixed(t),piece<3?stages[piece][7]:Jet.Fixed(t),motion);values[piece]=pair.Value*weights[piece];direct[piece]=pair.Delta*weights[piece];}
  // Independent expansion in the genuinely adapted frame: canonical solder
  // tensors and star are constant there, but ALL form inputs move.
  var baseStages=input.Select(x=>Caa.ForwardStages(x,true)).ToArray();var derivativeStages=variation.Select(x=>Caa.ForwardStages(x,true)).ToArray();var expanded=new Rational[4];
  for(int piece=0;piece<3;piece++)expanded[piece]=weights[piece]*(Pair(tdot,baseStages[piece][7])+Pair(t,derivativeStages[piece][7]));expanded[3]=Pair(t,tdot);
  return new(values,direct,expanded,variation,stages,baseStages,derivativeStages);
 }
}
