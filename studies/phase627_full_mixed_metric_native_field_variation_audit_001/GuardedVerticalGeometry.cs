#if A68_GUARDED_VERTICAL_GEOMETRY
// Phase627-local guarded derivative of the COMPLETE89-line source:
// ../phase607_source_induced_vertical_curvature_audit_001/VerticalGeometry.cs
// SHA256435614a4871b169133c580ce88c67d3abd40b1c5c72e79d43bd84a77cc0fe92c
// Exact numeric expressions and their order are unchanged. Reservations are
// logical dense uppercharges, not actual allocator bytes or whole-RSS bounds.
// External Ambient/MetricData/ConnectionData/spin arrays and caller-owned
// Data mutation are NOT intercepted by this local file. Their stages remain
// separately admitted. Matrix headers exist before constructor-body guards.
// Reviewed access model: one logical source indexer/array read or write is
// one touch; raw/output initialization is one touch per slot; bulk copies and
// LINQ results use equivalent logical touches. Guards' own repeated loads,
// index checks, library internals and VM traffic are NOT a measured census.
// Dense branches reserve their upper envelope even when zeros skip arithmetic.
internal sealed class Matrix
{
 static MixedProducerAdmission.Scope Admission=>MixedProducerAdmission.Current;
 public int N{get;} public Rational[,] Data{get;} public static long Products{get;private set;}
 public Matrix(int n){Admission.MatrixNew(n);Admission.MatrixLoop(n,2);N=n;Data=new Rational[n,n];}
 static void Factor(Rational value)=>Admission.RationalInputs(value.Numerator,value.Denominator);
 void Index(int i,int j)
 {Admission.MatrixDimension(N);if((uint)i>=(uint)N||(uint)j>=(uint)N){Admission.Poison();throw new IndexOutOfRangeException();}}
 public Rational this[int i,int j]{get{Index(i,j);Factor(Data[i,j]);return Data[i,j];}set{Index(i,j);Factor(value);Data[i,j]=value;}}
 void Input()
 {Admission.MatrixDimension(N);Admission.MatrixLoop(N,2);foreach(var value in Data)Factor(value);}
 static void Domain(bool condition,string message)
 {if(!condition){Admission.Poison();throw new ArgumentException(message);}}
 public static Rational Inv(Rational x){Factor(x);return new(x.Denominator,x.Numerator);}
 public static Rational Mul(Rational a,Rational b)
 {Factor(a);Factor(b);if(Products==long.MaxValue)Admission.Reject("matrix product counter overflow");Products++;return a*b;}
 public static Matrix Identity(int n){Admission.MatrixLoop(n,1);var r=new Matrix(n);for(int i=0;i<n;i++)r[i,i]=1;return r;}
 public static Matrix Diagonal(params long[] a){Admission.MatrixLoop(a.Length,1,2);var r=new Matrix(a.Length);for(int i=0;i<a.Length;i++)r[i,i]=a[i];return r;}
 public Matrix Copy(){Input();Admission.MatrixLoop(N,2,2);var r=new Matrix(N);Array.Copy(Data,r.Data,Data.Length);return r;}
 public Matrix Scale(Rational s){Factor(s);Input();Admission.MatrixLoop(N,2,3);var r=new Matrix(N);for(int i=0;i<N;i++)for(int j=0;j<N;j++)if(this[i,j]!=0)r[i,j]=Mul(this[i,j],s);return r;}
 public static Matrix operator +(Matrix a,Matrix b){Check(a,b);Admission.MatrixLoop(a.N,2,3);var r=new Matrix(a.N);for(int i=0;i<a.N;i++)for(int j=0;j<a.N;j++)r[i,j]=a[i,j]+b[i,j];return r;}
 public static Matrix operator -(Matrix a,Matrix b)=>a+b.Scale(-1);
 public static Matrix operator *(Matrix a,Matrix b)
 {Check(a,b);Admission.MatrixLoop(a.N,2);Admission.MatrixLoop(a.N,3,5);var r=new Matrix(a.N);for(int i=0;i<a.N;i++)for(int k=0;k<a.N;k++)if(a[i,k]!=0)for(int j=0;j<a.N;j++)if(b[k,j]!=0)r[i,j]+=Mul(a[i,k],b[k,j]);return r;}
 private static void Check(Matrix a,Matrix b){Domain(a.N==b.N,"matrix dimensions");a.Input();b.Input();}
 public Rational Trace(){Input();Admission.MatrixLoop(N,1);Rational r=0;for(int i=0;i<N;i++)r+=this[i,i];return r;}
 public static Rational TraceProduct(Matrix a,Matrix b){Check(a,b);Admission.MatrixLoop(a.N,2,4);Rational r=0;for(int i=0;i<a.N;i++)for(int j=0;j<a.N;j++)if(a[i,j]!=0&&b[j,i]!=0)r+=Mul(a[i,j],b[j,i]);return r;}
 public bool Same(Matrix b){Input();b.Input();Admission.MatrixLoop(N,2,2);return N==b.N&&Enumerable.Range(0,N).All(i=>Enumerable.Range(0,N).All(j=>this[i,j]==b[i,j]));}
 public bool Zero=>Nonzero==0;
 public int Nonzero{get{Input();Admission.MatrixLoop(N,2);return Data.Cast<Rational>().Count(v=>v!=0);}}
 public bool Symmetric{get{Input();Admission.MatrixLoop(N,2,2);return Enumerable.Range(0,N).All(i=>Enumerable.Range(0,N).All(j=>this[i,j]==this[j,i]));}}
 public string[][] Text()
 {Input();Admission.MatrixLoop(N,2,3);Admission.MatrixLoop(N,1,2);Admission.RawArray((long)N*N+N,(long)N+1);return Enumerable.Range(0,N).Select(i=>Enumerable.Range(0,N).Select(j=>this[i,j].ToString()).ToArray()).ToArray();}
 void EliminationEnvelope()
 {
  // Full pivots, row/column and elimination scans, even if zeros skip work.
  // Deliberately loose source-touch envelope; nested Copy/Swap guards charge
  // separately. Inverse fits6n3+6n2+n; Determinant3n3+4n2+n.
  // Inertia has <=n iterations, each <=n2 off-diagonal search reads and <=8n2
  // update touches, plus <=n2 diagonal search reads and <=n pivot reads.
  // Thus9n3+n2+n suffices for that body;10n3+8n2+8n covers all three.
  // This excludes library-internal/guard traffic, not skipped source branches.
  Admission.MatrixLoop(N,3,10);Admission.MatrixLoop(N,2,8);Admission.MatrixLoop(N,1,8);
 }
 public Matrix Inverse()
 {
  Input();EliminationEnvelope();var a=Copy();var r=Identity(N);
  for(int c=0;c<N;c++){int pivot=Enumerable.Range(c,N-c).FirstOrDefault(i=>a[i,c]!=0,-1);if(pivot<0){Admission.Poison();throw new InvalidOperationException("singular matrix");}SwapRows(a,c,pivot);SwapRows(r,c,pivot);Rational inv=Inv(a[c,c]);for(int j=0;j<N;j++){a[c,j]=Mul(a[c,j],inv);r[c,j]=Mul(r[c,j],inv);}for(int i=0;i<N;i++)if(i!=c){Rational f=a[i,c];if(f==0)continue;for(int j=0;j<N;j++){a[i,j]-=Mul(f,a[c,j]);r[i,j]-=Mul(f,r[c,j]);}}}return r;
 }
 public Rational Determinant()
 {
  Input();EliminationEnvelope();var a=Copy();Rational det=1;
  for(int c=0;c<N;c++){int p=Enumerable.Range(c,N-c).FirstOrDefault(i=>a[i,c]!=0,-1);if(p<0)return 0;if(p!=c){SwapRows(a,c,p);det=Mul(det,-1);}Rational q=a[c,c];det=Mul(det,q);for(int i=c+1;i<N;i++){Rational f=Mul(a[i,c],Inv(q));if(f==0)continue;for(int j=c+1;j<N;j++)a[i,j]-=Mul(f,a[c,j]);a[i,c]=0;}}return det;
 }
 public (int Positive,int Negative,int Zero) Inertia()
 {
  Input();EliminationEnvelope();Domain(Symmetric,"inertia requires symmetric matrix");var a=Copy();int pos=0,neg=0,k=0;
  while(k<N)
  {
   int p=Enumerable.Range(k,N-k).FirstOrDefault(i=>a[i,i]!=0,-1);
   if(p>=0){SwapCongruence(a,k,p);Rational d=a[k,k];if(d.Numerator.Sign>0)pos++;else neg++;for(int i=k+1;i<N;i++)for(int j=i;j<N;j++){a[i,j]-=Mul(Mul(a[i,k],a[j,k]),Inv(d));a[j,i]=a[i,j];}k++;continue;}
   int u=-1,v=-1;for(int i=k;i<N&&u<0;i++)for(int j=i+1;j<N;j++)if(a[i,j]!=0){u=i;v=j;break;}if(u<0)break;
   SwapCongruence(a,k,u);SwapCongruence(a,k+1,v);Rational off=a[k,k+1];pos++;neg++;
   for(int i=k+2;i<N;i++)for(int j=i;j<N;j++){a[i,j]-=Mul(Mul(a[i,k],a[j,k+1])+Mul(a[i,k+1],a[j,k]),Inv(off));a[j,i]=a[i,j];}k+=2;
  }return(pos,neg,N-pos-neg);
 }
 private static void SwapRows(Matrix a,int i,int j){Admission.MatrixLoop(a.N,1,4);for(int k=0;k<a.N;k++)(a[i,k],a[j,k])=(a[j,k],a[i,k]);}
 private static void SwapCongruence(Matrix a,int i,int j){Admission.MatrixLoop(a.N,1,4);SwapRows(a,i,j);for(int k=0;k<a.N;k++)(a[k,i],a[k,j])=(a[k,j],a[k,i]);}
}

internal static class Geometry
{
 static MixedProducerAdmission.Scope Admission=>MixedProducerAdmission.Current;
 static void Factors(Rational alpha,Rational beta)
 {Admission.RationalInputs(alpha.Numerator,alpha.Denominator);Admission.RationalInputs(beta.Numerator,beta.Denominator);}
 // Explicit cctor prohibits beforefieldinit from allocating Pairs before a
 // caller establishes the mandatory scope. Shared static storage thereafter
 // remains an outer retained-input/lifetime obligation, not recharged per call.
 static Geometry(){}
 public static readonly (int I,int J)[] Pairs=MakePairs();
 static (int I,int J)[] MakePairs()
 {Admission.RawArray(40,4);Admission.MatrixVisits(32);return Enumerable.Range(0,4).Select(i=>(i,i)).Concat(from i in Enumerable.Range(0,4) from j in Enumerable.Range(i+1,3-i) select(i,j)).ToArray();}
 public static Matrix[] Basis()
 {Admission.RawArray(Pairs.Length);Admission.MatrixVisits(5L*Pairs.Length);return Pairs.Select(p=>{var m=new Matrix(4);m[p.I,p.J]=1;m[p.J,p.I]=1;return m;}).ToArray();}
 public static Matrix From(Rational[] coefficients)
 {if(coefficients.Length!=10){Admission.Poison();throw new ArgumentException("Sym4 coordinates");}Admission.MatrixVisits(50);var m=new Matrix(4);for(int i=0;i<10;i++){var p=Pairs[i];m[p.I,p.J]=coefficients[i];m[p.J,p.I]=coefficients[i];}return m;}
 public static Rational[] Coordinates(Matrix m)
 {if(m.N!=4||!m.Symmetric){Admission.Poison();throw new ArgumentException("symmetric4 required");}Admission.RawArray(Pairs.Length);Admission.MatrixVisits(4L*Pairs.Length);return Pairs.Select(p=>m[p.I,p.J]).ToArray();}
 public static Matrix Commutator(Matrix a,Matrix b)=>a*b-b*a;
 public static Matrix Gamma(Matrix p,Matrix a,Matrix b)=>(a*p*b+b*p*a).Scale(new Rational(-1,2));
 public static Matrix DGamma(Matrix p,Matrix x,Matrix a,Matrix b)=>(a*p*x*p*b+b*p*x*p*a).Scale(new Rational(1,2));
 public static Matrix Curvature(Matrix y,Matrix p,Matrix a,Matrix b,Matrix c)=>(y*Commutator(Commutator(p*a,p*b),p*c)).Scale(new Rational(-1,4));
 public static Rational Metric(Matrix p,Matrix a,Matrix b,Rational alpha,Rational beta)
 {Factors(alpha,beta);return alpha*Matrix.TraceProduct(p*a,p*b)+beta*(p*a).Trace()*(p*b).Trace();}
 public static Rational DMetric(Matrix p,Matrix x,Matrix a,Matrix b,Rational alpha,Rational beta)
 {Factors(alpha,beta);var dp=(p*x*p).Scale(-1);return alpha*(Matrix.TraceProduct(dp*a,p*b)+Matrix.TraceProduct(p*a,dp*b))+beta*((dp*a).Trace()*(p*b).Trace()+(p*a).Trace()*(dp*b).Trace());}
 public static Rational DDMetric(Matrix p,Matrix x,Matrix z,Matrix a,Matrix b,Rational alpha,Rational beta)
 {
  Factors(alpha,beta);var px=(p*x*p).Scale(-1);var pz=(p*z*p).Scale(-1);var pxz=p*x*p*z*p+p*z*p*x*p;
  return alpha*(Matrix.TraceProduct(pxz*a,p*b)+Matrix.TraceProduct(p*a,pxz*b)+Matrix.TraceProduct(px*a,pz*b)+Matrix.TraceProduct(pz*a,px*b))
   +beta*((pxz*a).Trace()*(p*b).Trace()+(p*a).Trace()*(pxz*b).Trace()+(px*a).Trace()*(pz*b).Trace()+(pz*a).Trace()*(px*b).Trace());
 }
 public static Rational[] Solve(Matrix inverse,Rational[] covector)
 {if(covector.Length<inverse.N){Admission.Poison();throw new IndexOutOfRangeException();}Admission.MatrixLoop(inverse.N,2,2);Admission.MatrixLoop(inverse.N,1,2);Admission.RawArray(inverse.N);return Enumerable.Range(0,inverse.N).Select(i=>Enumerable.Range(0,inverse.N).Aggregate((Rational)0,(s,j)=>s+Matrix.Mul(inverse[i,j],covector[j]))).ToArray();}
 public static Rational Pow(Rational a,int p)
 {Admission.RationalInputs(a.Numerator,a.Denominator);Admission.MatrixVisits(Math.Max(0L,p));Rational r=1;for(int i=0;i<p;i++)r*=a;return r;}
}

// Pure logical-source-access model; neither this metadata nor ReserveBody
// constructs geometry. Matrix kernels charge separately. This does not bound
// internal LINQ allocations, guard instrumentation or allocator/VM/RSS traffic.
internal static class MetricJetAdmission
{
 public static long BodyVisits(int basisLength)
 {
  if(basisLength<10)throw new ArgumentOutOfRangeException(nameof(basisLength));
  // pair/Gram900 + t3/t4 33000 + D7000 + DD170000 =210900;
  // pair/t2/t3/t4 zero initialization11200; u/t1 each need one input read,
  // one result zero-init and one result store per entry (6*basisLength).
  // D/DD property initialization is charged at the factories below, not here.
  return 222100+6L*basisLength;
 }
 public static void ReserveBody(int basisLength)
 {
  var s=MixedProducerAdmission.Current;
  long visits;try{visits=BodyVisits(basisLength);}catch{s.Poison();throw;}
  s.MatrixVisits(visits);
 }
}

internal sealed class MetricJet
{
 static MixedProducerAdmission.Scope Admission=>MixedProducerAdmission.Current;
 static Rational[,,] DArray(){Admission.RawArray(1000);Admission.MatrixVisits(1000);return new Rational[10,10,10];}
 static Rational[,,,] DDArray(){Admission.RawArray(10000);Admission.MatrixVisits(10000);return new Rational[10,10,10,10];}
 public Matrix Gram{get;} public Rational[,,] D{get;}=DArray();public Rational[,,,] DD{get;}=DDArray();
 public MetricJet(Matrix p,Matrix[] basis,Rational alpha,Rational beta)
 {
  // Initializer arrays above retain the historical pre-constructor order.
  // Body access envelope and initializer zeroing are admitted separately;
  // this preserves the historical property-before-constructor ordering.
  if(basis.Length<10){Admission.Poison();throw new IndexOutOfRangeException();}
  // Original generic code accepts extra basis entries: u/t1 evaluate all of
  // them, then fixed arrays use the first10. Preserve that successful domain;
  // the source-bound caller separately requires its exact10 basis menu.
  Admission.RationalInputs(alpha.Numerator,alpha.Denominator);Admission.RationalInputs(beta.Numerator,beta.Denominator);MetricJetAdmission.ReserveBody(basis.Length);
  Gram=new Matrix(10);Admission.RawArray(basis.Length);var u=basis.Select(a=>p*a).ToArray();Admission.RawArray(100);var pair=new Matrix[10,10];Admission.RawArray(u.Length);var t1=u.Select(a=>a.Trace()).ToArray();Admission.RawArray(100);var t2=new Rational[10,10];Admission.RawArray(1000);var t3=new Rational[10,10,10];Admission.RawArray(10000);var t4=new Rational[10,10,10,10];
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
#endif
