using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

internal static class ParallelControls
{
 // A connection generator acts on Clifford coefficients AND on covectors.
 // R^a_b=sigma_b, R^b_a=-sigma_a is the vector action of gamma_ab/2.
 public static (FT Clifford,FT Covector,FT Total) Action(FT t,int a,int b)
 {
  var spin=One(0,(1<<a)|(1<<b),Fourier.Half);
  var clifford=Product(spin,t,'C');var covector=new FT();
  foreach(var q in t)
  {
   foreach(var (j,i,value) in new[]{(a,b,-Sigma(b)),(b,a,Sigma(a))})
   {
    if((q.Key.Form&(1<<j))==0)continue;int rest=q.Key.Form^(1<<j);
    if((rest&(1<<i))!=0)continue;
    int sign=Shuffle(1<<j,rest)*Shuffle(1<<i,rest);
    Put(covector,(rest|(1<<i),q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*(value*sign));
   }
  }
  return(clifford,covector,Add(clifford,covector));
 }
 public static FT ExteriorAt(FT derivative,int axis)=>Product(One(1<<axis,0,1),derivative);
 public static FT DaggerAt(FT derivative,int axis)
 {
  var r=new FT();foreach(var q in derivative)if((q.Key.Form&(1<<axis))!=0)
  {int sign=Shuffle(1<<axis,q.Key.Form^(1<<axis));Put(r,(q.Key.Form^(1<<axis),q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*(-Sigma(axis)*sign));}
  return r;
 }
 public static IEnumerable<(int S,int Gamma,int Kappa)> Monomials()
 {for(int total=0;total<=3;total++)for(int s=0;s<=total;s++)for(int gamma=0;gamma<=total-s;gamma++)yield return(s,gamma,total-s-gamma);}
 public static Rational Coefficient(FT t,int a,int b)
 {var v=t.GetValueOrDefault((1<<a,1<<b,0,0));if(v.Imaginary!=0)throw new InvalidOperationException("nonreal vector coefficient");return v.Real;}
}
