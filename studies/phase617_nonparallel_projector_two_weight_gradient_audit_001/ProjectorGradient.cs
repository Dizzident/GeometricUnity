using System.Text.Json;
using static Algebra;
using static Fourier;
using static Adjoint;
using static Caa;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

internal static class ProjectorGradient
{
 public static Matrix PartialProjector(Ambient g,int axis)
 {
  var result=new Matrix(14);if(axis<4)return result;var x=g.Vertical[axis-4];
  for(int j=0;j<10;j++){var a=g.Vertical[j];var column=(g.Y.Scale((g.P*x*g.P*a).Trace())-x.Scale((g.P*a).Trace())).Scale(new Rational(1,4));var v=Geometry.Coordinates(column);for(int i=0;i<10;i++)result[i+4,j+4]=v[i];}return result;
 }
 public static Matrix Connection(Ambient g,int axis)
 {var result=new Matrix(14);var inv=g.Gram.Inverse();for(int j=0;j<14;j++){var v=Ambient.Koszul(inv,g.D,axis,j);for(int i=0;i<14;i++)result[i,j]=v[i];}return result;}
 // Independent typed horizontal/vertical formula, not a commutator evaluation.
 public static Matrix Oracle(Ambient g,int axis)
 {
  var result=new Matrix(14);if(axis>=4)return result;var u=Ambient.Unit(4,axis);
  for(int j=0;j<4;j++){var v=Ambient.Unit(4,j);var a=(g.Y*Ambient.Sym(u,v)*g.Y-g.Y.Scale(g.HorizontalPair(u,v)*new Rational(1,4))).Scale(g.Sigma*new Rational(1,2));var column=Ambient.V(a);for(int i=0;i<14;i++)result[i,j]=column[i];}
  for(int j=0;j<10;j++){var a=g.Vertical[j]-g.Y.Scale((g.P*g.Vertical[j]).Trace()*new Rational(1,4));var column=Ambient.Apply(g.P*a,u);for(int i=0;i<4;i++)result[i,j+4]=column[i]*new Rational(1,2);}return result;
 }
 public static Matrix[] FrameDerivatives(Matrix[] coordinate,Matrix frame,Matrix inverse)
 {return Enumerable.Range(0,14).Select(a=>{var sum=new Matrix(14);for(int i=0;i<14;i++)if(frame[i,a]!=0)sum=sum+coordinate[i].Scale(frame[i,a]);return inverse*sum*frame;}).ToArray();}
 public static FT ExteriorDerivative(Matrix[] derivatives)
 {var result=new FT();for(int a=0;a<14;a++)result=Add(result,Product(One(1<<a,0,1),SpinGeometry.WeightedGamma(derivatives[a])));return result;}
 public static FT Contract(FT input,int axis)
 {var result=new FT();foreach(var q in input)if((q.Key.Form&(1<<axis))!=0){int sign=Degree(q.Key.Form&((1<<axis)-1))%2==0?1:-1;Put(result,(q.Key.Form^(1<<axis),q.Key.Blade,0,0),q.Value*sign);}return result;}
 public static FT Divergence(FT[] derivatives)
 {var result=new FT();for(int a=0;a<14;a++)result=Add(result,Scale(Contract(derivatives[a],a),-Sigma(a)));return result;}
 public static FT OracleJ(Matrix[] derivatives)
 {
  var result=new FT();for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++)if(c!=a&&derivatives[a][c,b]!=0)
   Put(result,(1<<b,(1<<a)|(1<<c),0,0),new Scalar(-2*Sigma(a)*BladeSign(1<<a,1<<c)*derivatives[a][c,b],0));return result;
 }
 public static FT DiagonalAdjoint(Matrix v)
 {
  var result=new FT();for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)Put(result,((1<<a)|(1<<b),(1<<a)|(1<<b),0,0),new Scalar(2*(v[a,a]+v[b,b]-v.Trace()),0));return result;
 }
 public static FT Evaluate(FT[] coefficients,Rational a,Rational b)=>Add(Add(Scale(coefficients[0],new Scalar(a*a,0)),Scale(coefficients[1],new Scalar(a*b,0))),Scale(coefficients[2],new Scalar(b*b,0)));
 public static bool ConstantRealType(FT t,int degree)=>Typed(t,degree)&&HAnti(t)&&t.Keys.All(k=>k.K0==0&&k.K1==0);
 public static bool VectorOnly(FT t)=>t.Keys.All(k=>Degree(k.Blade)==1);
 public static FT ReadTerms(JsonElement element)
 {var result=new FT();foreach(var q in element.EnumerateArray()){var key=(q.GetProperty("form").GetInt32(),q.GetProperty("blade").GetInt32(),q.GetProperty("k0").GetInt32(),q.GetProperty("k1").GetInt32());var value=new Scalar(SpinGeometry.Parse(q.GetProperty("real").GetString()!),SpinGeometry.Parse(q.GetProperty("imaginary").GetString()!));if(value.IsZero||result.ContainsKey(key))throw new ArgumentException("tensor record");Put(result,key,value);}return result;}
}
