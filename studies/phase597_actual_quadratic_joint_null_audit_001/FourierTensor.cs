using static Algebra;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Exact Laurent polynomials in exp(i*x0), exp(i*x1), with matrix-valued forms.
// No sampled angles, finite differences, numerical integration or eigensolver.
internal static class FourierTools
{
 public const int Full=16383;
 public static Scalar Half=>new(new Rational(1,2),0);
 public static void PutF(FT t,(int Form,int Blade,int K0,int K1) key,Scalar value)
 {Scalar sum=t.GetValueOrDefault(key)+value;if(sum.IsZero)t.Remove(key);else t[key]=sum;}
 public static FT One(int form,int blade,Scalar c){var t=new FT();PutF(t,(form,blade,0,0),c);return t;}
 public static FT Trig(int axis,bool sine,int form=0,int blade=0)
 {var t=new FT();PutF(t,(form,blade,axis==0?1:0,axis==1?1:0),sine?Scalar.I*Half*-1:Half);PutF(t,(form,blade,axis==0?-1:0,axis==1?-1:0),sine?Scalar.I*Half:Half);return t;}
 public static FT Add(FT a,FT b){var t=new FT(a);foreach(var q in b)PutF(t,q.Key,q.Value);return t;}
 public static FT Scale(FT a,Scalar c){var t=new FT();foreach(var q in a)PutF(t,q.Key,q.Value*c);return t;}
 public static FT Wedge(FT a,FT b)
 {var t=new FT();foreach(var q in a)foreach(var r in b)if((q.Key.Form&r.Key.Form)==0)PutF(t,(q.Key.Form|r.Key.Form,q.Key.Blade^r.Key.Blade,q.Key.K0+r.Key.K0,q.Key.K1+r.Key.K1),q.Value*r.Value*(Shuffle(q.Key.Form,r.Key.Form)*BladeSign(q.Key.Blade,r.Key.Blade)));return t;}
 // Source bracket acts on coefficient matrices, then wedges the ordered forms.
 public static FT Bracket(FT a,FT b,char kind)
 {var t=new FT();foreach(var q in a)foreach(var r in b)if((q.Key.Form&r.Key.Form)==0){int ab=BladeSign(q.Key.Blade,r.Key.Blade),ba=BladeSign(r.Key.Blade,q.Key.Blade);Scalar c=q.Value*r.Value*(Shuffle(q.Key.Form,r.Key.Form)*(kind=='C'?ab-ba:ab+ba));if(kind=='A')c*=Scalar.I;PutF(t,(q.Key.Form|r.Key.Form,q.Key.Blade^r.Key.Blade,q.Key.K0+r.Key.K0,q.Key.K1+r.Key.K1),c);}return t;}
 public static FT StarF(FT a){var t=new FT();foreach(var q in a)PutF(t,(Full^q.Key.Form,q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*HodgeSign(q.Key.Form));return t;}
 public static FT OmegaF(FT a){var t=new FT();foreach(var q in a)PutF(t,(q.Key.Form,Full^q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*BladeSign(Full,q.Key.Blade));return t;}
 public static FT Partial(FT a,int axis)
 {var t=new FT();foreach(var q in a)PutF(t,q.Key,q.Value*Scalar.I*(axis==0?q.Key.K0:q.Key.K1));return t;}
 public static FT ExteriorD(FT a)=>Add(Wedge(One(1,0,1),Partial(a,0)),Wedge(One(2,0,1),Partial(a,1)));
 public static FT AdjointD(FT a)
 {var t=new FT();for(int axis=0;axis<2;axis++)foreach(var q in Partial(a,axis))if((q.Key.Form&(1<<axis))!=0)PutF(t,(q.Key.Form^(1<<axis),q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*(-Sigma(axis)*(Degree(q.Key.Form&((1<<axis)-1))%2==0?1:-1)));return t;}
 public static bool Typed(FT t,int degree)=>t.Keys.All(k=>Degree(k.Form)==degree);
 public static bool Equal(FT a,FT b)=>a.Count==b.Count&&a.All(q=>b.TryGetValue(q.Key,out var v)&&q.Value==v);
 public static bool HAnti(FT t)=>t.All(q=>
 {int sign=(Degree(q.Key.Blade)*(Degree(q.Key.Blade)+1)/2)%2==1?1:-1;Scalar opposite=t.GetValueOrDefault((q.Key.Form,q.Key.Blade,-q.Key.K0,-q.Key.K1));return opposite==new Scalar(q.Value.Real*sign,q.Value.Imaginary*-sign);});
 public static Rational Pair(FT a,FT b)
 {Scalar total=0;foreach(var q in a)foreach(var r in b)if(q.Key.Form==r.Key.Form&&q.Key.Blade==r.Key.Blade&&q.Key.K0+r.Key.K0==0&&q.Key.K1+r.Key.K1==0)total+=q.Value*r.Value*(-BladeSign(q.Key.Blade,r.Key.Blade)*(Degree(q.Key.Form&0x3f80)%2==0?1:-1));if(total.Imaginary!=0)throw new InvalidOperationException("Nonreal integrated pairing");return total.Real;}
 public static Rational Top(FT a)
 {if(!Typed(a,14))throw new InvalidOperationException("Expected top form");Scalar c=a.GetValueOrDefault((Full,0,0,0));if(c.Imaginary!=0)throw new InvalidOperationException("Nonreal top trace");return c.Real*-1;}
 public static (FT Upper,FT Lower,FT Inner,bool Types) Chain(FT f,FT p1,FT p2,bool first=true)
 {var a=first?Bracket(p1,StarF(f),'C'):new FT();var inner=Bracket(p2,StarF(f),'A');var low=StarF(inner);var outer=Bracket(p1,low,'C');var upper=Add(a,Scale(StarF(outer),Half*-1));var lower=StarF(upper);return(upper,lower,inner,Typed(f,2)&&Typed(a,13)&&Typed(inner,14)&&Typed(low,0)&&Typed(outer,1)&&Typed(upper,13)&&Typed(lower,1));}
 public static object[] TermsF(FT a)=>a.OrderBy(q=>q.Key).Select(q=>(object)new{form=q.Key.Form,blade=q.Key.Blade,k0=q.Key.K0,k1=q.Key.K1,real=q.Value.Real.ToString(),imaginary=q.Value.Imaginary.ToString()}).ToArray();
 public static Rational[][] Transpose(Rational[][] a)=>Enumerable.Range(0,a[0].Length).Select(j=>a.Select(row=>row[j]).ToArray()).ToArray();
 public static Rational[] MV(Rational[][] a,Rational[] b)=>a.Select(row=>row.Zip(b,(u,v)=>u*v).Aggregate(new Rational(0),(s,v)=>s+v)).ToArray();
 public static Rational[][] MM(Rational[][] a,Rational[][] b)=>Transpose(Transpose(b).Select(col=>MV(a,col)).ToArray());
 public static Rational[][] Plus(Rational[][] a,Rational[][] b)=>a.Select((r,i)=>r.Select((v,j)=>v+b[i][j]).ToArray()).ToArray();
 public static Rational[][] Times(Rational[][] a,Rational c)=>a.Select(r=>r.Select(v=>v*c).ToArray()).ToArray();
 public static bool EqualM(Rational[][] a,Rational[][] b)=>a.Length==b.Length&&a.Zip(b,(r,s)=>r.SequenceEqual(s)).All(v=>v);
 public static string[][] MatrixText(Rational[][] a)=>a.Select(r=>r.Select(v=>v.ToString()).ToArray()).ToArray();
 public static Polynomial Quad(Rational[][] a,Polynomial[] x)
 {var p=Polynomial.Constant(0);for(int i=0;i<a.Length;i++)for(int j=0;j<a[i].Length;j++)p+=x[i]*x[j]*new Scalar(a[i][j],0);return p;}
 public static Rational[][] Hessian(Polynomial p,int n)=>Enumerable.Range(0,n).Select(i=>Enumerable.Range(0,n).Select(j=>
 {var q=p.Derivative(i).Derivative(j);Scalar v=q.Evaluate(0,0,0);if(v.Imaginary!=0||!q.Same(Polynomial.Constant(v)))throw new InvalidOperationException("Expected constant real Hessian");return v.Real;}).ToArray()).ToArray();
}
