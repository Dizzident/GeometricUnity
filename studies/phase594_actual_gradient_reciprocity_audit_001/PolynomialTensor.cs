using System.Numerics;
using static Algebra;
using PT=System.Collections.Generic.Dictionary<(int Form,int Blade),Polynomial>;

// Phase593 polynomial/tensor operations copied with public wrapper and coefficient access.
internal static class ExactTools
{
 const int Omega=(1<<14)-1;
public static void Pput(PT t,(int Form,int Blade) key,Polynomial value)
{Polynomial sum=t.TryGetValue(key,out var old)?old+value:value;if(sum.IsZero)t.Remove(key);else t[key]=sum;}
public static PT PSingle(int form,int blade,Polynomial value){var t=new PT();Pput(t,(form,blade),value);return t;}
public static PT PAdd(PT a,PT b){var r=new PT(a);foreach(var q in b)Pput(r,q.Key,q.Value);return r;}
public static PT PScale(PT a,Scalar s){var r=new PT();foreach(var q in a)Pput(r,q.Key,q.Value*s);return r;}
public static PT POmega(PT t){var r=new PT();foreach(var q in t)Pput(r,(q.Key.Form,Omega^q.Key.Blade),q.Value*BladeSign(Omega,q.Key.Blade));return r;}
public static PT PStar(PT t){var r=new PT();foreach(var q in t)Pput(r,(Omega^q.Key.Form,q.Key.Blade),q.Value*HodgeSign(q.Key.Form));return r;}
public static PT PWedge(PT a,PT b)
{
 var r=new PT();foreach(var q in a)foreach(var s in b)if((q.Key.Form&s.Key.Form)==0)
  Pput(r,(q.Key.Form|s.Key.Form,q.Key.Blade^s.Key.Blade),q.Value*s.Value*(Shuffle(q.Key.Form,s.Key.Form)*BladeSign(q.Key.Blade,s.Key.Blade)));return r;
}
public static PT PBracket(PT a,PT b,char bracket)
{
 var r=new PT();foreach(var q in a)foreach(var s in b)if((q.Key.Form&s.Key.Form)==0)
 {
  int ab=BladeSign(q.Key.Blade,s.Key.Blade),ba=BladeSign(s.Key.Blade,q.Key.Blade);
  Scalar sign=new(Shuffle(q.Key.Form,s.Key.Form)*(bracket=='C'?ab-ba:ab+ba));if(bracket=='A')sign*=Scalar.I;
  Pput(r,(q.Key.Form|s.Key.Form,q.Key.Blade^s.Key.Blade),q.Value*s.Value*sign);
 }return r;
}
public static PT PDerivative(PT a,int variable){var r=new PT();foreach(var q in a)Pput(r,q.Key,q.Value.Derivative(variable));return r;}
public static PT PAtZZero(PT a){var r=new PT();foreach(var q in a)Pput(r,q.Key,q.Value.AtZZero());return r;}
public static bool PSame(PT a,PT b)=>a.Count==b.Count&&a.All(q=>b.TryGetValue(q.Key,out var v)&&q.Value.Same(v));
public static bool HasDegree(PT a,int degree)=>a.Keys.All(q=>Degree(q.Form)==degree);
public static bool Hanti(PT a)=>a.All(q=>q.Value.Coefficients.All(v=>
 ((Degree(q.Key.Blade)*(Degree(q.Key.Blade)+1)/2)%2==1?v.Imaginary.Numerator==0:v.Real.Numerator==0)));
public static Polynomial TopTrace(PT t)
{if(!HasDegree(t,14))throw new InvalidOperationException("Top trace requires degree14");return t.TryGetValue((Omega,0),out var p)?p.RealPart()*-1:Polynomial.Constant(0);}
public static Polynomial PairOne(PT a,PT b)
{
 if(!HasDegree(a,1)||!HasDegree(b,1))throw new InvalidOperationException("One-form pairing expected");
 Polynomial p=Polynomial.Constant(0);foreach(var q in a)foreach(var s in b)if(q.Key==s.Key)
 {int axis=BitOperations.TrailingZeroCount((uint)q.Key.Form);p+= (q.Value*s.Value).RealPart()*(-Sigma(axis)*BladeSign(q.Key.Blade,s.Key.Blade));}return p;
}
public static object[] PTerms(PT t)=>t.OrderBy(q=>q.Key.Form).ThenBy(q=>q.Key.Blade).Select(q=>(object)new{formMask=q.Key.Form,cliffordMask=q.Key.Blade,polynomial=q.Value.Terms()}).ToArray();
public static object ScalarText(Scalar q)=>new{real=q.Real.ToString(),imaginary=q.Imaginary.ToString()};
}

sealed class Polynomial
{
 private readonly Dictionary<(int X,int Y,int Z),Scalar> terms=new();
 public bool IsZero=>terms.Count==0;
 public Scalar Coefficient(int x,int y,int z)=>terms.GetValueOrDefault((x,y,z));
 public IEnumerable<Scalar> Coefficients=>terms.Values;
 public static Polynomial Constant(Scalar value){var p=new Polynomial();p.Put((0,0,0),value);return p;}
 public static Polynomial Variable(int axis){var p=new Polynomial();p.Put((axis==0?1:0,axis==1?1:0,axis==2?1:0),1);return p;}
 private void Put((int X,int Y,int Z) key,Scalar value){Scalar sum=terms.GetValueOrDefault(key)+value;if(sum.IsZero)terms.Remove(key);else terms[key]=sum;}
 public static Polynomial operator +(Polynomial a,Polynomial b){var p=new Polynomial();foreach(var q in a.terms)p.Put(q.Key,q.Value);foreach(var q in b.terms)p.Put(q.Key,q.Value);return p;}
 public static Polynomial operator *(Polynomial a,Polynomial b){var p=new Polynomial();foreach(var q in a.terms)foreach(var s in b.terms)p.Put((q.Key.X+s.Key.X,q.Key.Y+s.Key.Y,q.Key.Z+s.Key.Z),q.Value*s.Value);return p;}
 public static Polynomial operator *(Polynomial a,Scalar b){var p=new Polynomial();foreach(var q in a.terms)p.Put(q.Key,q.Value*b);return p;}
 public static Polynomial operator *(Polynomial a,int b)=>a*new Scalar(b);
 public Polynomial Derivative(int axis)
 {var p=new Polynomial();foreach(var q in terms){int n=axis==0?q.Key.X:axis==1?q.Key.Y:q.Key.Z;if(n>0)p.Put((q.Key.X-(axis==0?1:0),q.Key.Y-(axis==1?1:0),q.Key.Z-(axis==2?1:0)),q.Value*n);}return p;}
 public Polynomial AtZZero(){var p=new Polynomial();foreach(var q in terms)if(q.Key.Z==0)p.Put(q.Key,q.Value);return p;}
 public Polynomial RealPart(){var p=new Polynomial();foreach(var q in terms)p.Put(q.Key,new Scalar(q.Value.Real,0));return p;}
 public Scalar Evaluate(int x,int y,int z){Scalar s=0;foreach(var q in terms)s+=q.Value*Pow(x,q.Key.X)*Pow(y,q.Key.Y)*Pow(z,q.Key.Z);return s;}
 private static long Pow(int b,int exponent){long v=1;for(int i=0;i<exponent;i++)v=checked(v*b);return v;}
 public bool Same(Polynomial p)=>terms.Count==p.terms.Count&&terms.All(q=>p.terms.TryGetValue(q.Key,out var s)&&q.Value==s);
 public object[] Terms()=>terms.OrderBy(q=>q.Key).Select(q=>(object)new{x=q.Key.X,y=q.Key.Y,z=q.Key.Z,real=q.Value.Real.ToString(),imaginary=q.Value.Imaginary.ToString()}).ToArray();
}
