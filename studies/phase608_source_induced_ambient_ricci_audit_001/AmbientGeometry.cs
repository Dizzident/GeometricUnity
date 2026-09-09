using static Geometry;

internal sealed class Ambient
{
 public Matrix Y{get;} public Matrix P{get;} public Rational Alpha{get;} public Rational Beta{get;} public Rational Sigma{get;} public Rational Delta=>Alpha+4*Beta;
 public Matrix[] Vertical{get;}=Basis(); public Matrix Gram{get;} public Rational[,,] D{get;}=new Rational[14,14,14]; public Rational[,,,] DD{get;}=new Rational[14,14,14,14];
 public Ambient(Matrix y,Rational alpha,Rational beta,Rational sigma,bool inverseHorizontal=false)
 {
  Y=y;P=y.Inverse();Alpha=alpha;Beta=beta;Sigma=sigma;Gram=new Matrix(14);var j=new MetricJet(P,Vertical,alpha,beta);var horizontal=inverseHorizontal?P:Y;
  for(int a=0;a<4;a++)for(int b=0;b<4;b++)Gram[a,b]=sigma*horizontal[a,b];
  for(int a=0;a<10;a++)for(int b=0;b<10;b++)Gram[a+4,b+4]=j.Gram[a,b];
  for(int x=0;x<10;x++)
  {
   var dh=inverseHorizontal?(P*Vertical[x]*P).Scale(-1):Vertical[x];
   for(int a=0;a<4;a++)for(int b=0;b<4;b++)D[x+4,a,b]=sigma*dh[a,b];
   for(int a=0;a<10;a++)for(int b=0;b<10;b++)D[x+4,a+4,b+4]=j.D[x,a,b];
   for(int z=0;z<10;z++)
   {
    if(inverseHorizontal){var ddh=P*Vertical[x]*P*Vertical[z]*P+P*Vertical[z]*P*Vertical[x]*P;for(int a=0;a<4;a++)for(int b=0;b<4;b++)DD[x+4,z+4,a,b]=sigma*ddh[a,b];}
    for(int a=0;a<10;a++)for(int b=0;b<10;b++)DD[x+4,z+4,a+4,b+4]=j.DD[x,z,a,b];
   }
  }
 }
 public static Rational[] Unit(int n,int i){var r=new Rational[n];r[i]=1;return r;}
 public static Rational[] Add(Rational[] a,Rational[] b)=>a.Zip(b,(x,y)=>x+y).ToArray();
 public static Rational[] Scale(Rational[] a,Rational s)=>a.Select(x=>x*s).ToArray();
 public static Rational[] Apply(Matrix m,Rational[] v)=>Solve(m,v);
 public static Rational Dot(Rational[] a,Rational[] b)=>a.Zip(b,(x,y)=>x*y).Aggregate((Rational)0,(s,x)=>s+x);
 public static Rational[] H(Rational[] u)=>u.Concat(new Rational[10]).ToArray();
 public static Rational[] V(Matrix a)=>new Rational[4].Concat(Coordinates(a)).ToArray();
 public static Matrix Sym(Rational[] u,Rational[] v){var r=new Matrix(4);for(int i=0;i<4;i++)for(int j=0;j<4;j++)r[i,j]=(u[i]*v[j]+v[i]*u[j])*new Rational(1,2);return r;}
 public Rational Pair(Rational[] u,Rational[] v)=>Dot(u,Apply(Gram,v));
 public Rational HorizontalPair(Rational[] u,Rational[] v)=>Dot(u,Apply(Y,v));
 public Matrix W(Rational[] u,Rational[] v)=>(Y*Sym(u,v)*Y-Y.Scale(Beta*Matrix.Inv(Delta)*HorizontalPair(u,v))).Scale(Matrix.Inv(Alpha));
 public Matrix DW(Matrix x,Rational[] u,Rational[] v)=>(x*Sym(u,v)*Y+Y*Sym(u,v)*x-(Y.Scale(Dot(u,Apply(x,v)))+x.Scale(HorizontalPair(u,v))).Scale(Beta*Matrix.Inv(Delta))).Scale(Matrix.Inv(Alpha));
 public Rational MetricEntry(int a,int b)
 {if(a<4&&b<4)return Sigma*Y[a,b];if(a>=4&&b>=4)return Metric(P,Vertical[a-4],Vertical[b-4],Alpha,Beta);return 0;}
 public Rational FirstEntry(int x,int a,int b)
 {if(x<4)return 0;if(a<4&&b<4)return Sigma*Vertical[x-4][a,b];if(a>=4&&b>=4)return DMetric(P,Vertical[x-4],Vertical[a-4],Vertical[b-4],Alpha,Beta);return 0;}
 public Rational SecondEntry(int x,int z,int a,int b)
 {if(x<4||z<4||a<4||b<4)return 0;return DDMetric(P,Vertical[x-4],Vertical[z-4],Vertical[a-4],Vertical[b-4],Alpha,Beta);}
 public Rational[] GammaExpected(int a,int b)
 {
  if(a>=4&&b>=4)return V(Gamma(P,Vertical[a-4],Vertical[b-4]));
  if(a<4&&b<4)return V(W(Unit(4,a),Unit(4,b)).Scale(Sigma*new Rational(-1,2)));
  int ai=a>=4?a:b,ui=a<4?a:b;return H(Scale(Apply(P*Vertical[ai-4],Unit(4,ui)),new Rational(1,2)));
 }
 public Rational[] DerivativeExpected(int x,int a,int b)
 {
  if(x<4)return new Rational[14];var e=Vertical[x-4];
  if(a>=4&&b>=4)return V(DGamma(P,e,Vertical[a-4],Vertical[b-4]));
  if(a<4&&b<4)return V(DW(e,Unit(4,a),Unit(4,b)).Scale(Sigma*new Rational(-1,2)));
  int ai=a>=4?a:b,ui=a<4?a:b;return H(Scale(Apply(P*e*P*Vertical[ai-4],Unit(4,ui)),new Rational(-1,2)));
 }
 public Rational[] CurvatureExpected(int a,int b,int c)
 {
  if(a<4&&b>=4)return Scale(CurvatureExpected(b,a,c),-1);
  if(a>=4&&b>=4)
  {
   var aa=Vertical[a-4];var bb=Vertical[b-4];
   return c>=4?V(Curvature(Y,P,aa,bb,Vertical[c-4])):H(Scale(Apply(Commutator(P*aa,P*bb),Unit(4,c)),new Rational(-1,4)));
  }
  if(a>=4)
  {
   var aa=Vertical[a-4];var u=Unit(4,b);
   return c>=4?H(Scale(Apply(P*Vertical[c-4]*P*aa,u),new Rational(1,4))):V(W(Apply(P*aa,u),Unit(4,c)).Scale(Sigma*new Rational(-1,4)));
  }
  var v1=Unit(4,a);var v2=Unit(4,b);
  if(c>=4){var pa=P*Vertical[c-4];return V((W(v1,Apply(pa,v2))-W(v2,Apply(pa,v1))).Scale(Sigma*new Rational(-1,4)));}
  var w=Unit(4,c);return H(Scale(Add(Scale(v1,HorizontalPair(v2,w)),Scale(v2,HorizontalPair(v1,w)*-1)),Sigma*(Alpha+6*Beta)*Matrix.Inv(8*Alpha*Delta)));
 }
 public Rational RicciExpected(int a,int b)
 {if(a<4&&b<4)return Gram[a,b]*-1*Matrix.Inv(4*Delta);if(a<4||b<4)return 0;var u=P*Vertical[a-4];var v=P*Vertical[b-4];return new Rational(-5,4)*Matrix.TraceProduct(u,v)+new Rational(1,4)*u.Trace()*v.Trace();}
 public Matrix TracelessProjector()
 {
  var r=new Matrix(14);for(int a=0;a<10;a++){var v=Coordinates(Vertical[a]-Y.Scale((P*Vertical[a]).Trace()*new Rational(1,4)));for(int b=0;b<10;b++)r[b+4,a+4]=v[b];}return r;
 }
 public static int Rank(Matrix m)
 {
  var a=m.Copy();int row=0;for(int col=0;col<a.N&&row<a.N;col++){int p=Enumerable.Range(row,a.N-row).FirstOrDefault(i=>a[i,col]!=0,-1);if(p<0)continue;for(int j=col;j<a.N;j++)(a[row,j],a[p,j])=(a[p,j],a[row,j]);var q=Matrix.Inv(a[row,col]);for(int j=col;j<a.N;j++)a[row,j]*=q;for(int i=row+1;i<a.N;i++){var f=a[i,col];for(int j=col;j<a.N;j++)a[i,j]-=f*a[row,j];}row++;}return row;
 }
 public static Rational[] Koszul(Matrix inverse,Rational[,,] d,int a,int b)=>Solve(inverse,Enumerable.Range(0,14).Select(c=>(d[a,b,c]+d[b,a,c]-d[c,a,b])*new Rational(1,2)).ToArray());
}
