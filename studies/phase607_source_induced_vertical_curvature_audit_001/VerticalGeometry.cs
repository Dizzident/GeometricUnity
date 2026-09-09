// Exact dense coordinate algebra. Metric jets are derived from inverse-matrix
// differentiation, independently of the candidate connection/commutator laws.
internal sealed class Matrix
{
 public int N{get;} public Rational[,] Data{get;} public static long Products{get;private set;}
 public Matrix(int n){N=n;Data=new Rational[n,n];}
 public Rational this[int i,int j]{get=>Data[i,j];set=>Data[i,j]=value;}
 public static Rational Inv(Rational x)=>new(x.Denominator,x.Numerator);
 public static Rational Mul(Rational a,Rational b){Products++;return a*b;}
 public static Matrix Identity(int n){var r=new Matrix(n);for(int i=0;i<n;i++)r[i,i]=1;return r;}
 public static Matrix Diagonal(params long[] a){var r=new Matrix(a.Length);for(int i=0;i<a.Length;i++)r[i,i]=a[i];return r;}
 public Matrix Copy(){var r=new Matrix(N);Array.Copy(Data,r.Data,Data.Length);return r;}
 public Matrix Scale(Rational s){var r=new Matrix(N);for(int i=0;i<N;i++)for(int j=0;j<N;j++)if(this[i,j]!=0)r[i,j]=Mul(this[i,j],s);return r;}
 public static Matrix operator +(Matrix a,Matrix b){Check(a,b);var r=new Matrix(a.N);for(int i=0;i<a.N;i++)for(int j=0;j<a.N;j++)r[i,j]=a[i,j]+b[i,j];return r;}
 public static Matrix operator -(Matrix a,Matrix b)=>a+b.Scale(-1);
 public static Matrix operator *(Matrix a,Matrix b){Check(a,b);var r=new Matrix(a.N);for(int i=0;i<a.N;i++)for(int k=0;k<a.N;k++)if(a[i,k]!=0)for(int j=0;j<a.N;j++)if(b[k,j]!=0)r[i,j]+=Mul(a[i,k],b[k,j]);return r;}
 private static void Check(Matrix a,Matrix b){if(a.N!=b.N)throw new ArgumentException("matrix dimensions");}
 public Rational Trace(){Rational r=0;for(int i=0;i<N;i++)r+=this[i,i];return r;}
 public static Rational TraceProduct(Matrix a,Matrix b){Check(a,b);Rational r=0;for(int i=0;i<a.N;i++)for(int j=0;j<a.N;j++)if(a[i,j]!=0&&b[j,i]!=0)r+=Mul(a[i,j],b[j,i]);return r;}
 public bool Same(Matrix b)=>N==b.N&&Enumerable.Range(0,N).All(i=>Enumerable.Range(0,N).All(j=>this[i,j]==b[i,j]));
 public bool Zero=>Nonzero==0;
 public int Nonzero=>Data.Cast<Rational>().Count(v=>v!=0);
 public bool Symmetric=>Enumerable.Range(0,N).All(i=>Enumerable.Range(0,N).All(j=>this[i,j]==this[j,i]));
 public string[][] Text()=>Enumerable.Range(0,N).Select(i=>Enumerable.Range(0,N).Select(j=>this[i,j].ToString()).ToArray()).ToArray();
 public Matrix Inverse()
 {
  var a=Copy();var r=Identity(N);
  for(int c=0;c<N;c++){int pivot=Enumerable.Range(c,N-c).FirstOrDefault(i=>a[i,c]!=0,-1);if(pivot<0)throw new InvalidOperationException("singular matrix");SwapRows(a,c,pivot);SwapRows(r,c,pivot);Rational inv=Inv(a[c,c]);for(int j=0;j<N;j++){a[c,j]=Mul(a[c,j],inv);r[c,j]=Mul(r[c,j],inv);}for(int i=0;i<N;i++)if(i!=c){Rational f=a[i,c];if(f==0)continue;for(int j=0;j<N;j++){a[i,j]-=Mul(f,a[c,j]);r[i,j]-=Mul(f,r[c,j]);}}}return r;
 }
 public Rational Determinant()
 {
  var a=Copy();Rational det=1;
  for(int c=0;c<N;c++){int p=Enumerable.Range(c,N-c).FirstOrDefault(i=>a[i,c]!=0,-1);if(p<0)return 0;if(p!=c){SwapRows(a,c,p);det=Mul(det,-1);}Rational q=a[c,c];det=Mul(det,q);for(int i=c+1;i<N;i++){Rational f=Mul(a[i,c],Inv(q));if(f==0)continue;for(int j=c+1;j<N;j++)a[i,j]-=Mul(f,a[c,j]);a[i,c]=0;}}return det;
 }
 public (int Positive,int Negative,int Zero) Inertia()
 {
  if(!Symmetric)throw new ArgumentException("inertia requires symmetric matrix");var a=Copy();int pos=0,neg=0,k=0;
  while(k<N)
  {
   int p=Enumerable.Range(k,N-k).FirstOrDefault(i=>a[i,i]!=0,-1);
   if(p>=0){SwapCongruence(a,k,p);Rational d=a[k,k];if(d.Numerator.Sign>0)pos++;else neg++;for(int i=k+1;i<N;i++)for(int j=i;j<N;j++){a[i,j]-=Mul(Mul(a[i,k],a[j,k]),Inv(d));a[j,i]=a[i,j];}k++;continue;}
   int u=-1,v=-1;for(int i=k;i<N&&u<0;i++)for(int j=i+1;j<N;j++)if(a[i,j]!=0){u=i;v=j;break;}if(u<0)break;
   SwapCongruence(a,k,u);SwapCongruence(a,k+1,v);Rational off=a[k,k+1];pos++;neg++;
   for(int i=k+2;i<N;i++)for(int j=i;j<N;j++){a[i,j]-=Mul(Mul(a[i,k],a[j,k+1])+Mul(a[i,k+1],a[j,k]),Inv(off));a[j,i]=a[i,j];}k+=2;
  }return(pos,neg,N-pos-neg);
 }
 private static void SwapRows(Matrix a,int i,int j){for(int k=0;k<a.N;k++)(a[i,k],a[j,k])=(a[j,k],a[i,k]);}
 private static void SwapCongruence(Matrix a,int i,int j){SwapRows(a,i,j);for(int k=0;k<a.N;k++)(a[k,i],a[k,j])=(a[k,j],a[k,i]);}
}

internal static class Geometry
{
 public static readonly (int I,int J)[] Pairs=Enumerable.Range(0,4).Select(i=>(i,i)).Concat(from i in Enumerable.Range(0,4) from j in Enumerable.Range(i+1,3-i) select(i,j)).ToArray();
 public static Matrix[] Basis()=>Pairs.Select(p=>{var m=new Matrix(4);m[p.I,p.J]=1;m[p.J,p.I]=1;return m;}).ToArray();
 public static Matrix From(Rational[] coefficients){if(coefficients.Length!=10)throw new ArgumentException("Sym4 coordinates");var m=new Matrix(4);for(int i=0;i<10;i++){var p=Pairs[i];m[p.I,p.J]=coefficients[i];m[p.J,p.I]=coefficients[i];}return m;}
 public static Rational[] Coordinates(Matrix m){if(m.N!=4||!m.Symmetric)throw new ArgumentException("symmetric4 required");return Pairs.Select(p=>m[p.I,p.J]).ToArray();}
 public static Matrix Commutator(Matrix a,Matrix b)=>a*b-b*a;
 public static Matrix Gamma(Matrix p,Matrix a,Matrix b)=>(a*p*b+b*p*a).Scale(new Rational(-1,2));
 public static Matrix DGamma(Matrix p,Matrix x,Matrix a,Matrix b)=>(a*p*x*p*b+b*p*x*p*a).Scale(new Rational(1,2));
 public static Matrix Curvature(Matrix y,Matrix p,Matrix a,Matrix b,Matrix c)=>(y*Commutator(Commutator(p*a,p*b),p*c)).Scale(new Rational(-1,4));
 public static Rational Metric(Matrix p,Matrix a,Matrix b,Rational alpha,Rational beta)=>alpha*Matrix.TraceProduct(p*a,p*b)+beta*(p*a).Trace()*(p*b).Trace();
 public static Rational DMetric(Matrix p,Matrix x,Matrix a,Matrix b,Rational alpha,Rational beta)
 {var dp=(p*x*p).Scale(-1);return alpha*(Matrix.TraceProduct(dp*a,p*b)+Matrix.TraceProduct(p*a,dp*b))+beta*((dp*a).Trace()*(p*b).Trace()+(p*a).Trace()*(dp*b).Trace());}
 public static Rational DDMetric(Matrix p,Matrix x,Matrix z,Matrix a,Matrix b,Rational alpha,Rational beta)
 {
  var px=(p*x*p).Scale(-1);var pz=(p*z*p).Scale(-1);var pxz=p*x*p*z*p+p*z*p*x*p;
  return alpha*(Matrix.TraceProduct(pxz*a,p*b)+Matrix.TraceProduct(p*a,pxz*b)+Matrix.TraceProduct(px*a,pz*b)+Matrix.TraceProduct(pz*a,px*b))
   +beta*((pxz*a).Trace()*(p*b).Trace()+(p*a).Trace()*(pxz*b).Trace()+(px*a).Trace()*(pz*b).Trace()+(pz*a).Trace()*(px*b).Trace());
 }
 public static Rational[] Solve(Matrix inverse,Rational[] covector)=>Enumerable.Range(0,inverse.N).Select(i=>Enumerable.Range(0,inverse.N).Aggregate((Rational)0,(s,j)=>s+Matrix.Mul(inverse[i,j],covector[j]))).ToArray();
 public static Rational Pow(Rational a,int p){Rational r=1;for(int i=0;i<p;i++)r*=a;return r;}
}

internal sealed class MetricJet
{
 public Matrix Gram{get;} public Rational[,,] D{get;}=new Rational[10,10,10];public Rational[,,,] DD{get;}=new Rational[10,10,10,10];
 public MetricJet(Matrix p,Matrix[] basis,Rational alpha,Rational beta)
 {
  Gram=new Matrix(10);var u=basis.Select(a=>p*a).ToArray();var pair=new Matrix[10,10];var t1=u.Select(a=>a.Trace()).ToArray();var t2=new Rational[10,10];var t3=new Rational[10,10,10];var t4=new Rational[10,10,10,10];
  for(int a=0;a<10;a++)for(int b=0;b<10;b++){pair[a,b]=u[a]*u[b];t2[a,b]=pair[a,b].Trace();Gram[a,b]=alpha*t2[a,b]+beta*t1[a]*t1[b];}
  for(int a=0;a<10;a++)for(int b=0;b<10;b++)for(int c=0;c<10;c++){t3[a,b,c]=Matrix.TraceProduct(pair[a,b],u[c]);for(int d=0;d<10;d++)t4[a,b,c,d]=Matrix.TraceProduct(pair[a,b],pair[c,d]);}
  for(int x=0;x<10;x++)for(int a=0;a<10;a++)for(int b=0;b<10;b++)
  {
   D[x,a,b]=alpha*-1*(t3[x,a,b]+t3[a,x,b])-beta*(t2[x,a]*t1[b]+t1[a]*t2[x,b]);
   for(int z=0;z<10;z++)DD[x,z,a,b]=alpha*(t4[x,z,a,b]+t4[z,x,a,b]+t4[a,x,z,b]+t4[a,z,x,b]+t4[x,a,z,b]+t4[z,a,x,b])
    +beta*((t3[x,z,a]+t3[z,x,a])*t1[b]+t1[a]*(t3[x,z,b]+t3[z,x,b])+t2[x,a]*t2[z,b]+t2[z,a]*t2[x,b]);
  }
 }
}
