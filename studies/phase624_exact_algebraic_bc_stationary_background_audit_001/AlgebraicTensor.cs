using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Algebraic scalar extension only: each of the three coefficient tensors
// still ranges over the COMPLETE exterior/Clifford masks and real phases.
internal sealed class AT
{
 public FT[] Coefficients{get;}
 public AT(params FT[] coefficients){if(coefficients.Length!=3)throw new ArgumentException("three field coefficients");Coefficients=coefficients;}
 public static AT Zero()=>new(new FT(),new FT(),new FT());
 public static AT Lift(FT t)=>new(t,new FT(),new FT());
 public bool IsZero=>Coefficients.All(t=>t.Count==0);
 public static AT Add(AT a,AT b)=>new(Enumerable.Range(0,3).Select(i=>Fourier.Add(a.Coefficients[i],b.Coefficients[i])).ToArray());
 public static AT Scale(AT a,Cubic q)
 {var r=Zero();for(int i=0;i<3;i++)for(int j=0;j<3;j++){var weight=Cubic.Monomial(i+j);for(int k=0;k<3;k++)r.Coefficients[k]=Fourier.Add(r.Coefficients[k],Fourier.Scale(a.Coefficients[i],new Scalar(q[j]*weight[k],0)));}return r;}
 public static AT Combine(AT[] inputs,params Cubic[] weights)
 {if(inputs.Length!=weights.Length)throw new ArgumentException("coefficient menu");var r=Zero();for(int i=0;i<inputs.Length;i++)r=Add(r,Scale(inputs[i],weights[i]));return r;}
 public static AT Linear(AT t,Func<FT,FT> map)=>new(t.Coefficients.Select(map).ToArray());
 public static AT Bilinear(AT a,AT b,Func<FT,FT,FT> map)
 {var r=Zero();for(int i=0;i<3;i++)for(int j=0;j<3;j++){var tensor=map(a.Coefficients[i],b.Coefficients[j]);var weight=Cubic.Monomial(i+j);for(int k=0;k<3;k++)r.Coefficients[k]=Fourier.Add(r.Coefficients[k],Fourier.Scale(tensor,new Scalar(weight[k],0)));}return r;}
 public static AT Product(AT a,AT b,bool naive=false,char kind='W')=>Bilinear(a,b,(x,y)=>naive?NaiveProduct(x,y,kind):Fourier.Product(x,y,kind));
 public static AT DQAdjoint(AT a,AT b,bool word=false)=>Bilinear(a,b,word?WordAdjoint:global::Adjoint.DQAdjoint);
 private static FT WordAdjoint(FT phi,FT output)
 {
  var r=new FT();foreach(var x in phi)foreach(var y in output)if((x.Key.Form&y.Key.Form)==x.Key.Form)
  {int form=x.Key.Form^y.Key.Form,metric=Degree(x.Key.Form&0x3f80)%2==0?1:-1;int factor=WordSign(y.Key.Blade,x.Key.Blade)-WordSign(x.Key.Blade,y.Key.Blade);Put(r,(form,x.Key.Blade^y.Key.Blade,x.Key.K0+y.Key.K0,x.Key.K1+y.Key.K1),x.Value*y.Value*(metric*Shuffle(x.Key.Form,form)*factor));}return r;
 }
 public static Cubic Pair(AT a,AT b)
 {Cubic r=0;for(int i=0;i<3;i++)for(int j=0;j<3;j++)r+=Cubic.Monomial(i+j)*(Cubic)Fourier.Pair(a.Coefficients[i],b.Coefficients[j]);return r;}
 public static bool Equal(AT a,AT b)=>Enumerable.Range(0,3).All(i=>Fourier.Equal(a.Coefficients[i],b.Coefficients[i]));
 public static bool Valid(AT a,int degree)=>a.Coefficients.All(t=>Typed(t,degree)&&HAnti(t)&&t.Keys.All(k=>k.Form>=0&&k.Form<=Full&&k.Blade>=0&&k.Blade<=Full&&k.K0==0&&k.K1==0));
 public static bool Grades(AT a,params int[] degrees)=>a.Coefficients.All(t=>t.Keys.All(k=>degrees.Contains(Degree(k.Blade))));
 public static object[] Terms(AT a)=>a.Coefficients.SelectMany(t=>t.Keys).Distinct().Order().Select(k=>(object)new{form=k.Form,blade=k.Blade,k0=k.K0,k1=k.K1,real=a.Coefficients.Select(t=>t.GetValueOrDefault(k).Real.ToString()).ToArray(),imaginary=a.Coefficients.Select(t=>t.GetValueOrDefault(k).Imaginary.ToString()).ToArray()}).ToArray();
 public static AT[] Stages(AT t,bool naive=false)
 {var stages=t.Coefficients.Select(c=>Caa.ForwardStages(c,naive)).ToArray();return Enumerable.Range(0,8).Select(i=>new AT(stages.Select(s=>s[i]).ToArray())).ToArray();}
 public static (AT First,AT Second,AT Full,AT Simplified) Adjoint(AT t)
 {var legs=t.Coefficients.Select(Caa.AdjointLegs).ToArray();var first=new AT(legs.Select(x=>x.First).ToArray());var second=new AT(legs.Select(x=>x.Second).ToArray());return(first,second,Add(first,second),Linear(t,Caa.AdjointSimplified));}
 public static FeedbackResult Feedback(AT t)
 {
  var q=Product(t,t);var naiveQ=Product(t,t,true);var stages=Stages(q);var naive=Stages(q,true);var adjoint=Adjoint(t);var dq=DQAdjoint(t,adjoint.Full);var word=DQAdjoint(t,adjoint.Full,true);
  return new(t,q,naiveQ,stages,naive,adjoint.First,adjoint.Second,adjoint.Full,adjoint.Simplified,dq,word,Scale(Add(stages[7],dq),(Cubic)new Rational(1,3)));
 }
 public static CrossResult Cross(AT s,AT v,AT adjointS,AT adjointV)
 {
  var q=Add(Product(s,v),Product(v,s));var naiveQ=Add(Product(s,v,true),Product(v,s,true));var stages=Stages(q);var naive=Stages(q,true);
  var first=DQAdjoint(s,adjointV);var second=DQAdjoint(v,adjointS);var firstWord=DQAdjoint(s,adjointV,true);var secondWord=DQAdjoint(v,adjointS,true);
  return new(q,naiveQ,stages,naive,first,second,firstWord,secondWord,Scale(Add(stages[7],Add(first,second)),(Cubic)new Rational(1,3)));
 }
}
internal sealed record FeedbackResult(AT Input,AT Q,AT NaiveQ,AT[] Stages,AT[] NaiveStages,AT AdjointFirst,AT AdjointSecond,AT Adjoint,AT SimplifiedAdjoint,AT Dq,AT WordDq,AT Full)
{
 public object Evidence()=>new{input=AT.Terms(Input),q=AT.Terms(Q),naiveQ=AT.Terms(NaiveQ),stages=Stages.Select(AT.Terms),naiveStages=NaiveStages.Select(AT.Terms),adjointFirst=AT.Terms(AdjointFirst),adjointSecond=AT.Terms(AdjointSecond),adjoint=AT.Terms(Adjoint),simplifiedAdjoint=AT.Terms(SimplifiedAdjoint),dq=AT.Terms(Dq),wordDq=AT.Terms(WordDq),full=AT.Terms(Full)};
}
internal sealed record CrossResult(AT Q,AT NaiveQ,AT[] Stages,AT[] NaiveStages,AT First,AT Second,AT FirstWord,AT SecondWord,AT Full)
{
 public object Evidence()=>new{q=AT.Terms(Q),naiveQ=AT.Terms(NaiveQ),stages=Stages.Select(AT.Terms),naiveStages=NaiveStages.Select(AT.Terms),firstAdjoint=AT.Terms(First),secondAdjoint=AT.Terms(Second),firstWordAdjoint=AT.Terms(FirstWord),secondWordAdjoint=AT.Terms(SecondWord),full=AT.Terms(Full)};
}
