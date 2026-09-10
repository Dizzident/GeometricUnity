using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

internal static class Homogeneous
{
 public static Matrix Rho(Matrix z)
 {
  var result=new Matrix(14);for(int a=0;a<4;a++)for(int b=0;b<4;b++)result[a,b]=z[a,b];
  var basis=Geometry.Basis();for(int a=0;a<10;a++){var v=Geometry.Coordinates((SpinGeometry.Transpose(z)*basis[a]+basis[a]*z).Scale(-1));for(int b=0;b<10;b++)result[b+4,a+4]=v[b];}return result;
 }
 public static Matrix Lift(Ambient g,int axis)=>axis<4?new Matrix(4):(g.P*g.Vertical[axis-4]).Scale(new Rational(-1,2));
 public static Matrix LambdaTyped(Ambient g,int axis)
 {
  var result=new Matrix(14);if(axis>=4)return result;var u=Ambient.Unit(4,axis);
  for(int a=0;a<4;a++){var v=Ambient.V(g.W(u,Ambient.Unit(4,a)).Scale(g.Sigma*new Rational(-1,2)));for(int b=0;b<14;b++)result[b,a]=v[b];}
  for(int a=0;a<10;a++){var v=Ambient.Apply(g.P*g.Vertical[a],u);for(int b=0;b<4;b++)result[b,a+4]=v[b]*new Rational(1,2);}return result;
 }
 public static Matrix Combine(Matrix[] matrices,Rational[] coefficients)
 {var result=new Matrix(14);for(int a=0;a<14;a++)if(coefficients[a]!=0)result=result+matrices[a].Scale(coefficients[a]);return result;}
 public static Rational[] Column(Matrix m,int j)=>Enumerable.Range(0,m.N).Select(i=>m[i,j]).ToArray();
 public static Rational[] BracketM(Ambient g,int a,int b)
 {
  var result=new Rational[14];var za=Lift(g,a);var zb=Lift(g,b);for(int c=0;c<4;c++)result[c]=(b<4?za[c,b]:0)-(a<4?zb[c,a]:0);return result;
 }
 public static Matrix BracketH(Ambient g,int a,int b)=>Geometry.Commutator(Lift(g,a),Lift(g,b));
 public static Matrix Lorentz(int point,int i,int j)
 {
  var z=new Matrix(4);z[i,j]=1;z[j,i]=(i==0?-1:1)*(j==0?-1:1)*-1;
  if(point==0)return z;var d=Matrix.Diagonal(1,2,3,4);return d.Inverse()*z*d;
 }
 // Full exterior representations on all covector and Clifford slots.
 // Clifford multiplication is not truncated to any selected grade.
 public static FT Action(Matrix l,FT input)
 {
  var result=new FT();foreach(var q in input)
  {
   for(int a=0;a<14;a++)
   {
    if((q.Key.Form&(1<<a))!=0){int rest=q.Key.Form^(1<<a),remove=Degree(rest&((1<<a)-1))%2==0?1:-1;for(int b=0;b<14;b++)if((rest&(1<<b))==0&&l[a,b]!=0)Put(result,(rest|(1<<b),q.Key.Blade,q.Key.K0,q.Key.K1),q.Value*new Scalar(l[a,b]*(-remove)*Shuffle(1<<b,rest),0));}
    if((q.Key.Blade&(1<<a))!=0){int rest=q.Key.Blade^(1<<a),remove=Degree(rest&((1<<a)-1))%2==0?1:-1;for(int b=0;b<14;b++)if((rest&(1<<b))==0&&l[b,a]!=0)Put(result,(q.Key.Form,rest|(1<<b),q.Key.K0,q.Key.K1),q.Value*new Scalar(l[b,a]*remove*Shuffle(1<<b,rest),0));}
   }
  }return result;
 }
 public static FT SpinGenerator(Matrix l)
 {var t=new FT();for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)Put(t,(0,(1<<a)|(1<<b),0,0),new Scalar(new Rational(-1,2)*Sigma(a)*l[b,a],0));return t;}
 public static FT Disconnected(FT input)
 {const int horizontal=(1<<0)|(1<<7)|(1<<8)|(1<<9);var result=new FT();foreach(var q in input)Put(result,q.Key,q.Value*((Degree(q.Key.Form&horizontal)+Degree(q.Key.Blade&horizontal))%2==0?1:-1));return result;}
 public static Matrix DisconnectedMatrix(){var m=Matrix.Identity(14);foreach(int a in new[]{0,7,8,9})m[a,a]=-1;return m;}
}
