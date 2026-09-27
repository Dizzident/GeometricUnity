using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Full real-trace algebra. The independently ordered route never projects a
// Clifford grade or replaces a differential formal adjoint by a pointwise one.
internal static class Mixed
{
 static readonly System.Runtime.CompilerServices.ConditionalWeakTable<MixedTrace.Session,Dictionary<int,FT>> solderCache=new();
 public static FT Phi1=>Solder(1);
 public static FT Phi2=>Solder(2);
 static FT Solder(int degree)
 {
  var active=MixedTrace.Active;if(active is null){MixedProducerStages.ExternalDictionary();MixedProducerStages.Scan(105);return degree==1?Caa.Gamma1:Caa.Gamma2;}
  MixedProducerStages.Metadata(1);
  var cache=solderCache.GetOrCreateValue(active);if(cache.TryGetValue(degree,out var known))return known;
  MixedProducerStages.Metadata(2);MixedProducerStages.Arrays(455,9);
  var terms=new List<FT>();for(int a=0;a<14;a++)if(degree==1)terms.Add(Unit(1<<a,1<<a,1));else for(int b=a+1;b<14;b++)terms.Add(Unit((1<<a)|(1<<b),(1<<a)|(1<<b),1));
  var result=Sum(terms.ToArray());cache.Add(degree,result);return result;
 }
 public static FT Unit(int form,int blade,Scalar value){MixedProducerStages.Trace(0,true);return MixedTrace.Record("unit",Fourier.One(form,blade,value),[],new{form,blade,real=value.Real.ToString(),imaginary=value.Imaginary.ToString()});}
 public static FT Hodge(FT t){MixedProducerStages.Trace(1);return MixedTrace.Record("star",Fourier.Star(t),[t]);}
 public static FT HodgeAdjoint(FT t,int degree){MixedProducerStages.Trace(1,true);try{return MixedTrace.Record("starAdjoint",Adjoint.StarAdjoint(t,degree),[t],new{degree});}catch(ArgumentException){MixedProducerStages.Poison();throw;}}
 public static FT Spin(Matrix omega){MixedProducerStages.RecordedMatrix(omega);MixedProducerStages.MatrixSlots(91);MixedProducerStages.ExternalDictionary();MixedProducerStages.Trace(0,true);return MixedTrace.Record("spin",Homogeneous.SpinGenerator(omega),[],MixedTrace.MatrixArg(omega));}
 public static FT Raise(FT t)
 {MixedProducerStages.Input(t);MixedProducerStages.Scan(t.Count);MixedProducerStages.Trace(1);var result=MixedProducerStages.Empty();foreach(var q in t)Put(result,q.Key,q.Value*(Degree(q.Key.Form&0x3f80)%2==0?1:-1));return MixedTrace.Record("raise",result,[t]);}
 public static long ProductCalls{get;private set;}
 public static long ProductPairVisits{get;private set;}
 public static long NaiveCalls{get;private set;}
 public static long NaivePairVisits{get;private set;}
 public static long TransposeCalls{get;private set;}
 public static long TransposePairVisits{get;private set;}
 public static long WordTransposeCalls{get;private set;}
 public static long WordTransposePairVisits{get;private set;}
 public static long MotionSlotVisits{get;private set;}
 public static long OrderedMotionSlotVisits{get;private set;}
 public static long ForwardCalls{get;private set;}
 public static long ReverseCalls{get;private set;}
 public static long ComponentRecordVisits{get;private set;}
 public static long SliceRecordVisits{get;private set;}
 public static long SumRecordVisits{get;private set;}
 public static long ScaleRecordVisits{get;private set;}
 public static long PullbackFormBuilds{get;private set;}
 public static long PullbackRecordVisits{get;private set;}
 public static long PullbackCoefficientVisits{get;private set;}
 public static FT Sum(params FT[] a)
 {MixedProducerStages.Scan(a.Length);foreach(var t in a){MixedProducerStages.Input(t);MixedProducerStages.Scan(t.Count);}if(a.Length==0)MixedProducerStages.ExternalDictionary();var r=MixedProducerStages.Empty();foreach(var t in a)foreach(var q in t){SumRecordVisits=MixedProducerStages.Add(SumRecordVisits,1);Put(r,q.Key,q.Value);}return a.Length==0?MixedTrace.Zero():MixedTrace.Record("sum",r,a);}
 public static FT Times(FT a,Rational c)
 {MixedProducerStages.Trace(1,true);ScaleRecordVisits=MixedProducerStages.Add(ScaleRecordVisits,a.Count);return MixedTrace.Record("scale",Fourier.Scale(a,new Scalar(c,0)),[a],new{real=c.ToString(),imaginary="0"});}
 public static FT P(FT a,FT b,char kind='W',bool oracle=false)
 {
  MixedProducerStages.Input(a);MixedProducerStages.Input(b);MixedProducerStages.Trace(2,true);long visits=(long)a.Count*b.Count;
  if(oracle){NaiveCalls=MixedProducerStages.Add(NaiveCalls,1);NaivePairVisits=MixedProducerStages.Add(NaivePairVisits,visits);return MixedTrace.Record("product",NaiveProduct(a,b,kind),[a,b],new{kind=kind.ToString()});}
  ProductCalls=MixedProducerStages.Add(ProductCalls,1);ProductPairVisits=MixedProducerStages.Add(ProductPairVisits,visits);return MixedTrace.Record("product",Product(a,b,kind),[a,b],new{kind=kind.ToString()});
 }
 public static FT Transpose(FT a,FT y,char kind='C',bool oracle=false)
 {
  if(kind is not ('C' or 'A')){MixedProducerStages.Poison();throw new ArgumentException("adjoint kind");}
  MixedProducerStages.Transpose(a,y,oracle);MixedProducerStages.Trace(2,true);
  long visits=(long)a.Count*y.Count;
  if(!oracle){TransposeCalls=MixedProducerStages.Add(TransposeCalls,1);TransposePairVisits=MixedProducerStages.Add(TransposePairVisits,visits);return MixedTrace.Record("transpose",Adjoint.BracketAdjoint(a,y,kind),[a,y],new{kind=kind.ToString()});}
  WordTransposeCalls=MixedProducerStages.Add(WordTransposeCalls,1);WordTransposePairVisits=MixedProducerStages.Add(WordTransposePairVisits,visits);
  var r=MixedProducerStages.Empty();foreach(var x in a)foreach(var z in y)
  {
   if((x.Key.Form&z.Key.Form)!=x.Key.Form)continue;
   int input=z.Key.Form^x.Key.Form;int metric=Degree(x.Key.Form&0x3f80)%2==0?1:-1;
   // Explicit ordered wedge inversions, independent of Algebra.Shuffle.
   int inversions=0;foreach(int i in Feedback.Bits(x.Key.Form))foreach(int j in Feedback.Bits(input))if(i>j)inversions++;
   int xy=WordSign(x.Key.Blade,z.Key.Blade),yx=WordSign(z.Key.Blade,x.Key.Blade);
   var coefficient=x.Value*z.Value*(metric*(inversions%2==0?1:-1)*(kind=='C'?yx-xy:xy+yx));
   if(kind=='A')coefficient*=Scalar.I;
   Put(r,(input,x.Key.Blade^z.Key.Blade,x.Key.K0+z.Key.K0,x.Key.K1+z.Key.K1),coefficient);
  }return MixedTrace.Record("transpose",r,[a,y],new{kind=kind.ToString()});
 }
 // T_r is pullback motion, not the mixed two-form dual named M2.
 public static FT Motion(Matrix a,FT t,bool oracle=false)
 {
  MixedProducerStages.Motion(a,t,oracle);MixedProducerStages.Trace(1,true);var result=MixedProducerStages.Empty();foreach(var q in t)
  {
   var indices=Feedback.Bits(q.Key.Form);
   for(int slot=0;slot<indices.Length;slot++)for(int b=0;b<14;b++)
   {
    if(oracle)OrderedMotionSlotVisits=MixedProducerStages.Add(OrderedMotionSlotVisits,1);else MotionSlotVisits=MixedProducerStages.Add(MotionSlotVisits,1);
    int old=indices[slot];if(a[old,b]==0||indices.Where((_,j)=>j!=slot).Contains(b))continue;
    int mask,sign;
    if(oracle)
    {
     var changed=(int[])indices.Clone();changed[slot]=b;int inversions=0;
     for(int i=0;i<changed.Length;i++)for(int j=i+1;j<changed.Length;j++)if(changed[i]>changed[j])inversions++;
     mask=changed.Aggregate(0,(s,v)=>s|(1<<v));sign=inversions%2==0?1:-1;
    }
    else
    {
     int rest=q.Key.Form^(1<<old);mask=rest|(1<<b);
     sign=(Degree(rest&((1<<old)-1))%2==0?1:-1)*Shuffle(1<<b,rest);
    }
    Put(result,(mask,q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*new Scalar(a[old,b]*sign,0));
   }
  }return MixedTrace.Record("motion",result,[t],MixedTrace.MatrixArg(a));
 }
 public static Matrix MetricTranspose(Matrix a)
 {MixedProducerStages.Matrix(a);MixedProducerStages.MatrixSlots(392);var r=new Matrix(14);for(int i=0;i<14;i++)for(int j=0;j<14;j++)r[i,j]=a[j,i]*Sigma(i)*Sigma(j);return r;}
 public static FT MotionAdjoint(Matrix a,FT t,bool oracle=false)=>Motion(MetricTranspose(a),t,oracle);
 public static FT PairingMotion(Matrix a,FT t,bool oracle=false){MixedProducerStages.Arrays(3);return Sum(Motion(a,t,oracle),MotionAdjoint(a,t,oracle),Times(t,a.Trace()*-1));}
 public static FT StarDelta(Matrix a,FT t,bool oracle=false){MixedProducerStages.Arrays(2);return Sum(Hodge(Motion(a,t,oracle)),Times(Motion(a,Hodge(t),oracle),-1));}
 static Jet AddJ(params Jet[] a){MixedProducerStages.Arrays(2L*a.Length,2);MixedProducerStages.Metadata(4);MixedProducerStages.Scan(2L*a.Length);return new(Sum(a.Select(x=>x.Value).ToArray()),Sum(a.Select(x=>x.Delta).ToArray()));}
 static Jet ScaleJ(Jet a,Rational c)=>new(Times(a.Value,c),Times(a.Delta,c));
 static Jet ProductJ(Jet a,Jet b,char kind,bool oracle){MixedProducerStages.Arrays(2);return new(P(a.Value,b.Value,kind,oracle),Sum(P(a.Delta,b.Value,kind,oracle),P(a.Value,b.Delta,kind,oracle)));}
 static Jet StarJ(Jet t,Matrix a,bool oracle){MixedProducerStages.Arrays(2);return new(Hodge(t.Value),Sum(Hodge(t.Delta),StarDelta(a,t.Value,oracle)));}
 public static Jet[] Forward(Jet input,Matrix a,bool oracle=false)
 {
  MixedProducerStages.Arrays(10,2);ForwardCalls=MixedProducerStages.Add(ForwardCalls,1);
  var p1=new Jet(Phi1,Times(Motion(a,Phi1,oracle),-1));
  var p2=new Jet(Phi2,Times(Motion(a,Phi2,oracle),-1));
  var sf=StarJ(input,a,oracle);var first=ProductJ(p1,sf,'C',oracle);var inner=ProductJ(p2,sf,'A',oracle);
  var zero=StarJ(inner,a,oracle);var outer=ProductJ(p1,zero,'A',oracle);
  var upper=AddJ(first,ScaleJ(StarJ(outer,a,oracle),new Rational(-1,2)));var lower=StarJ(upper,a,oracle);
  return[input,sf,first,inner,zero,outer,upper,lower];
 }
 public static FT[] FixedForward(FT t,bool oracle=false)
 {
  MixedProducerStages.Arrays(10,2);ForwardCalls=MixedProducerStages.Add(ForwardCalls,1);var sf=Hodge(t);var first=P(Phi1,sf,'C',oracle);var inner=P(Phi2,sf,'A',oracle);
  var zero=Hodge(inner);var outer=P(Phi1,zero,'A',oracle);var upper=Sum(first,Times(Hodge(outer),new Rational(-1,2)));
  return[t,sf,first,inner,zero,outer,upper,Hodge(upper)];
 }
 // FIXED-baseline adjoint of the differentiated Hodge map. This is not the
 // derivative of a moving adjoint: (dot star)^dag=T^dag star^dag-star^dag T^dag.
 static Jet StarAdjointJ(Jet y,int degree,Matrix a,bool oracle)
 {
  FT Adj(FT z)=>HodgeAdjoint(z,degree);
  MixedProducerStages.Arrays(3);
  var value=Adj(y.Value);return new(value,Sum(Adj(y.Delta),MotionAdjoint(a,value,oracle),Times(Adj(MotionAdjoint(a,y.Value,oracle)),-1)));
 }
 static Jet TransposeJ(Jet phi,Jet y,char kind,bool oracle){MixedProducerStages.Arrays(2);return new(Transpose(phi.Value,y.Value,kind,oracle),Sum(Transpose(phi.Delta,y.Value,kind,oracle),Transpose(phi.Value,y.Delta,kind,oracle)));}
 public static Jet[] Reverse(Jet y,Matrix a,bool oracle=false)
 {
  MixedProducerStages.Arrays(11,2);ReverseCalls=MixedProducerStages.Add(ReverseCalls,1);
  var p1=new Jet(Phi1,Times(Motion(a,Phi1,oracle),-1));var p2=new Jet(Phi2,Times(Motion(a,Phi2,oracle),-1));
  var upper=StarAdjointJ(y,13,a,oracle);var first=StarAdjointJ(TransposeJ(p1,upper,'C',oracle),2,a,oracle);
  var outer=StarAdjointJ(upper,1,a,oracle);var zero=TransposeJ(p1,outer,'A',oracle);var top=StarAdjointJ(zero,14,a,oracle);
  var inner=TransposeJ(p2,top,'A',oracle);var second=ScaleJ(StarAdjointJ(inner,2,a,oracle),new Rational(-1,2));
  return[y,upper,first,outer,zero,top,inner,second,AddJ(first,second)];
 }
 public static FT FixedAdjoint(FT y,bool oracle=false)
 {
  MixedProducerStages.Arrays(2);ReverseCalls=MixedProducerStages.Add(ReverseCalls,1);var upper=HodgeAdjoint(y,13);var first=HodgeAdjoint(Transpose(Phi1,upper,'C',oracle),2);
  var one=HodgeAdjoint(upper,1);var zero=Transpose(Phi1,one,'A',oracle);var top=HodgeAdjoint(zero,14);
  var second=Times(HodgeAdjoint(Transpose(Phi2,top,'A',oracle),2),new Rational(-1,2));return Sum(first,second);
 }
 public static FT C(FT y,Matrix a,bool oracle=false)
 {MixedProducerStages.Arrays(3);var k=FixedForward(y,oracle)[7];return Sum(FixedForward(Motion(a,y,oracle),oracle)[7],MotionAdjoint(a,k,oracle),Times(k,a.Trace()*-1));}
 public static FT CAdjoint(FT x,Matrix a,bool oracle=false)
 {MixedProducerStages.Arrays(3);var k=FixedAdjoint(x,oracle);return Sum(MotionAdjoint(a,k,oracle),FixedAdjoint(Motion(a,x,oracle),oracle),Times(k,a.Trace()*-1));}
 public static FT Derivative(Matrix omega,FT y,bool oracle=false){MixedProducerStages.Arrays(2);return Sum(P(Spin(omega),y,'C',oracle),Times(Motion(omega,y,oracle),-1));}
 public static FT Component(FT x,int form)
 {MixedProducerStages.Input(x);MixedProducerStages.Scan(x.Count);MixedProducerStages.Trace(1,true);var r=MixedProducerStages.Empty();foreach(var q in x){ComponentRecordVisits=MixedProducerStages.Add(ComponentRecordVisits,1);if(q.Key.Form==form)Put(r,(0,q.Key.Blade,q.Key.K0,q.Key.K1),q.Value);}return MixedTrace.Record("component",r,[x],new{form});}
 public static FT[,] TwoFormSlices(FT t)
 {
  MixedProducerStages.Input(t);MixedProducerStages.Scan(t.Count);MixedProducerStages.Bits(t.Count);MixedProducerStages.Arrays(287,92);MixedProducerStages.Metadata(91);
  // Arrays covers196 grid zeroes+first stores and91 trace-input zeroes/stores.
  //105 mirrored triples need315 touches:119 beyond the196 first stores.
  //91 trace result reads complete210; each record adds2 index reads+grid read.
  MixedProducerStages.MatrixSlots(210+3L*t.Count);
  var r=new FT[14,14];for(int i=0;i<14;i++)for(int j=i;j<14;j++){r[i,j]=MixedProducerStages.Empty();r[j,i]=r[i,j];}
  foreach(var q in t){SliceRecordVisits=MixedProducerStages.Add(SliceRecordVisits,1);var indices=Feedback.Bits(q.Key.Form);if(indices.Length!=2){MixedProducerStages.Poison();throw new ArgumentException("two-form slice input");}Put(r[indices[0],indices[1]],(0,q.Key.Blade,q.Key.K0,q.Key.K1),q.Value);}
  for(int i=0;i<14;i++)for(int j=i+1;j<14;j++)MixedTrace.Record("component",r[i,j],[t],new{form=(1<<i)|(1<<j)});
  // Reversed slots share the canonical slice. Callers multiply by orientation.
  return r;
 }
 public static FT ExteriorComponent(FT x,int a,int b)
 {MixedProducerStages.Axis(a);MixedProducerStages.Axis(b);if(a==b)return MixedProducerStages.Empty();return Times(Component(x,(1<<a)|(1<<b)),a<b?1:-1);}
 public static FT Contract(FT x,int axis)
 {MixedProducerStages.Axis(axis);MixedProducerStages.Input(x);MixedProducerStages.Scan(x.Count);MixedProducerStages.Trace(1,true);var r=MixedProducerStages.Empty();foreach(var q in x)if((q.Key.Form&(1<<axis))!=0)Put(r,(q.Key.Form^(1<<axis),q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*(Degree(q.Key.Form&((1<<axis)-1))%2==0?1:-1));return MixedTrace.Record("contract",r,[x],new{axis});}
 public static FT Divergence(FT[] derivative){MixedProducerStages.Derivatives(derivative);MixedProducerStages.Arrays(14);MixedProducerStages.Metadata(4);MixedProducerStages.Scan(14);return Sum(Enumerable.Range(0,14).Select(a=>Times(Contract(derivative[a],a),-Sigma(a))).ToArray());}
 public static FT WedgeCoordinate(FT[] derivative,Matrix frame)
 {
  MixedProducerStages.Derivatives(derivative);MixedProducerStages.Matrix(frame);MixedProducerStages.MatrixSlots(588);MixedProducerStages.Arrays(392,196);
  var r=MixedProducerStages.Empty();for(int mu=0;mu<14;mu++)for(int a=0;a<14;a++)if(frame[mu,a]!=0)r=Sum(r,Times(P(Unit(1<<a,0,1),derivative[mu]),frame[mu,a]));return r;
 }
 public static FT Pullback(Matrix map,FT t,bool oracle=false)
 {
  MixedProducerStages.Pullback(map,t);MixedProducerStages.Trace(1,true);var forms=new Dictionary<int,FT>();foreach(int mask in t.Keys.Select(q=>q.Form).Distinct().Order())
  {
   // Pullback is one atomic recorded primitive; its independently replayed
   // form cache must not emit orphan, support-dependent unit nodes.
   PullbackFormBuilds=MixedProducerStages.Add(PullbackFormBuilds,1);var form=Fourier.One(0,0,1);foreach(int a in Feedback.Bits(mask))
   {
    // One Bits-result read plus condition/coefficient reads for all14 entries.
    MixedProducerStages.MatrixSlots(29);var row=MixedProducerStages.Empty();for(int b=0;b<14;b++)if(map[a,b]!=0)Put(row,(1<<b,0,0,0),new Scalar(map[a,b],0));long visits=(long)form.Count*row.Count;
    if(oracle){NaiveCalls=MixedProducerStages.Add(NaiveCalls,1);NaivePairVisits=MixedProducerStages.Add(NaivePairVisits,visits);form=NaiveProduct(form,row,'W');}
    else{ProductCalls=MixedProducerStages.Add(ProductCalls,1);ProductPairVisits=MixedProducerStages.Add(ProductPairVisits,visits);form=Product(form,row,'W');}
   }
   forms.Add(mask,form);
  }
  MixedProducerStages.Scan(t.Count);var result=MixedProducerStages.Empty();foreach(var q in t)
  {
   PullbackRecordVisits=MixedProducerStages.Add(PullbackRecordVisits,1);MixedProducerStages.Scan(forms[q.Key.Form].Count);foreach(var f in forms[q.Key.Form]){PullbackCoefficientVisits=MixedProducerStages.Add(PullbackCoefficientVisits,1);Put(result,(f.Key.Form,q.Key.Blade,q.Key.K0,q.Key.K1),f.Value*q.Value);}
  }return MixedTrace.Record("pullback",result,[t],MixedTrace.MatrixArg(map));
 }
 public static bool Equal(FT a,FT b){MixedProducerStages.Input(a);MixedProducerStages.Input(b);MixedProducerStages.Scan(a.Count);MixedProducerStages.Metadata(2);return a.Count==b.Count&&a.All(q=>b.TryGetValue(q.Key,out var z)&&q.Value==z);}
 public static Rational Norm(FT x){MixedProducerStages.Input(x);MixedProducerStages.Scan(x.Count);MixedProducerStages.Metadata(2);return x.Values.Aggregate((Rational)0,(s,z)=>s+Abs(z.Real)+Abs(z.Imaginary));}
 public static Rational Abs(Rational x){MixedProducerStages.Factor(x);return x.Numerator.Sign<0?x*-1:x;}
 public static Rational MatrixNorm(Matrix a){MixedProducerStages.Matrix(a,false);MixedProducerStages.MatrixSlots((long)a.N*a.N);Rational s=0;for(int i=0;i<a.N;i++)for(int j=0;j<a.N;j++)s+=Abs(a[i,j]);return s;}
}
