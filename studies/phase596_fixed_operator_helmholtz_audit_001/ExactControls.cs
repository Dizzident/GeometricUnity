using System.Security.Cryptography;
using System.Text;

internal static class Linear
{
 public static Rational Inv(Rational x)=>x.Numerator==0?throw new DivideByZeroException():new(x.Denominator,x.Numerator);
 public static Rational[][] Zeros(int rows,int columns)=>Enumerable.Range(0,rows).Select(_=>new Rational[columns]).ToArray();
 public static Rational[] Product(Rational[][] a,Rational[] b)=>a.Select(row=>row.Zip(b,(x,y)=>x*y).Aggregate(new Rational(0),(s,x)=>s+x)).ToArray();
 public static bool IsZero(Rational[] a)=>a.All(x=>x==0);
 public static Rational Maximum(Rational[] a)=>a.Select(x=>x.Numerator<0?x*-1:x).Aggregate(new Rational(0),(m,x)=>checked(x.Numerator*m.Denominator)>checked(m.Numerator*x.Denominator)?x:m);
 public static string[][] Text(Rational[][] a)=>a.Select(row=>row.Select(x=>x.ToString()).ToArray()).ToArray();
 public static string Digest(Rational[][] a)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n",a.Select(row=>string.Join(",",row.Select(x=>x.ToString()))))+"\n"))).ToLowerInvariant();
 public static Echelon Solve(Rational[][] input)
 {
  var a=input.Select(x=>x.ToArray()).ToArray();int n=a[0].Length,row=0;var pivots=new List<int>();
  for(int col=0;col<n&&row<a.Length;col++)
  {
   int pivot=Enumerable.Range(row,a.Length-row).FirstOrDefault(i=>a[i][col]!=0,-1);if(pivot<0)continue;
   (a[row],a[pivot])=(a[pivot],a[row]);Rational scale=Inv(a[row][col]);
   for(int j=0;j<n;j++)a[row][j]=a[row][j]*scale;
   for(int i=0;i<a.Length;i++)if(i!=row&&a[i][col]!=0)
   {Rational factor=a[i][col];for(int j=0;j<n;j++)if(a[row][j]!=0)a[i][j]=a[i][j]-factor*a[row][j];}
   pivots.Add(col);row++;
  }
  var kernel=new List<Rational[]>();foreach(int free in Enumerable.Range(0,n).Where(i=>!pivots.Contains(i)))
  {var v=new Rational[n];v[free]=1;for(int i=0;i<pivots.Count;i++)v[pivots[i]]=a[i][free]*-1;kernel.Add(v);}
  return new(pivots.Count,pivots.ToArray(),a,kernel.ToArray());
 }
 public const long Prime=1000003;
 // Independent integer finite-field elimination: rank here is a lower bound on rational rank.
 public static int ModularRank(Rational[][] original)
 {
  static long Mod(long x)=>(x%Prime+Prime)%Prime;
  static long Power(long x,long exponent){long y=1;while(exponent>0){if((exponent&1)!=0)y=checked(y*x)%Prime;x=checked(x*x)%Prime;exponent>>=1;}return y;}
  var a=original.Select(row=>row.Select(x=>Mod(x.Denominator)==0?throw new InvalidOperationException("Noninvertible modular denominator"):
   checked(Mod(x.Numerator)*Power(Mod(x.Denominator),Prime-2))%Prime).ToArray()).ToArray();
  int rank=0;
  for(int col=0;col<a[0].Length&&rank<a.Length;col++)
  {
   int pivot=rank;while(pivot<a.Length&&a[pivot][col]==0)pivot++;if(pivot==a.Length)continue;
   (a[rank],a[pivot])=(a[pivot],a[rank]);long inverse=Power(a[rank][col],Prime-2);
   for(int i=rank+1;i<a.Length;i++)if(a[i][col]!=0)
   {long factor=checked(a[i][col]*inverse)%Prime;for(int j=col;j<a[0].Length;j++)a[i][j]=Mod(a[i][j]-checked(factor*a[rank][j]));}
   rank++;
  }
  return rank;
 }
 public static bool PrimeKnownAnswer()=>Enumerable.Range(2,999).All(d=>Prime%d!=0);
 public static bool ModularKnownAnswers()
 {
  bool rejected=false;try{ModularRank([[new Rational(1,Prime)]]);}catch(InvalidOperationException){rejected=true;}
  return rejected&&ModularRank([[new Rational(1,2),new Rational(1,3)],[new Rational(3,2),1]])==1
   &&ModularRank([[new Rational(-1,2),0],[0,new Rational(2,3)]])==2&&ModularRank([[0,0],[0,0]])==0;
 }
 public static bool KernelRrefConsistency(Rational[][] original,Echelon r)=>r.Kernel.All(v=>IsZero(Product(original,v)))
  &&r.Kernel.Length+r.Rank==original[0].Length
  &&r.Pivots.Select((col,i)=>r.Rows[i][col]==1&&Enumerable.Range(0,r.Rows.Length).All(j=>j==i||r.Rows[j][col]==0)).All(x=>x)
  &&r.Rows.Skip(r.Rank).All(IsZero);
}
sealed record Echelon(int Rank,int[] Pivots,Rational[][] Rows,Rational[][] Kernel);

internal static class Matrix
{
 public static Scalar[,] Add(Scalar[,] a,Scalar[,] b)=>new[,]{{a[0,0]+b[0,0],a[0,1]+b[0,1]},{a[1,0]+b[1,0],a[1,1]+b[1,1]}};
 public static Scalar[,] Scale(Scalar[,] a,Scalar s)=>new[,]{{a[0,0]*s,a[0,1]*s},{a[1,0]*s,a[1,1]*s}};
 public static Scalar[,] Multiply(Scalar[,] a,Scalar[,] b)
 {var c=new Scalar[2,2];for(int i=0;i<2;i++)for(int j=0;j<2;j++)for(int k=0;k<2;k++)c[i,j]+=a[i,k]*b[k,j];return c;}
 public static Scalar[,] Bracket(Scalar[,] a,Scalar[,] b)=>Add(Multiply(a,b),Scale(Multiply(b,a),-1));
 public static Scalar[,] Dagger(Scalar[,] a)
 {var b=new Scalar[2,2];for(int i=0;i<2;i++)for(int j=0;j<2;j++)b[i,j]=new(a[j,i].Real,a[j,i].Imaginary*-1);return b;}
 public static Scalar Trace(Scalar[,] a)=>a[0,0]+a[1,1];
 public static Rational Pair(Scalar[,] a,Scalar[,] b)
 {Scalar trace=Trace(Multiply(a,b));if(trace.Imaginary!=0)throw new InvalidOperationException("Real Lie trace expected");return trace.Real*new Rational(-1,2);}
 public static bool Zero(Scalar[,] a)=>a.Cast<Scalar>().All(x=>x.IsZero);
 public static bool Same(Scalar[,] a,Scalar[,] b)=>a.Cast<Scalar>().SequenceEqual(b.Cast<Scalar>());
 public static Scalar[,] Combine(Scalar[][,] basis,Rational[] coefficients)
 {var m=new Scalar[2,2];for(int i=0;i<basis.Length;i++)m=Add(m,Scale(basis[i],new Scalar(coefficients[i],0)));return m;}
 public static Polynomial[,] PScale(Scalar[,] a,Polynomial p)=>new[,]{{p*a[0,0],p*a[0,1]},{p*a[1,0],p*a[1,1]}};
 public static Polynomial[,] PAdd(Polynomial[,] a,Polynomial[,] b)=>new[,]{{a[0,0]+b[0,0],a[0,1]+b[0,1]},{a[1,0]+b[1,0],a[1,1]+b[1,1]}};
 public static Polynomial[,] PMultiply(Polynomial[,] a,Polynomial[,] b)
 {var c=new Polynomial[2,2];for(int i=0;i<2;i++)for(int j=0;j<2;j++){c[i,j]=Polynomial.Constant(0);for(int k=0;k<2;k++)c[i,j]+=a[i,k]*b[k,j];}return c;}
 public static Polynomial[,] PBracket(Polynomial[,] a,Polynomial[,] b)
 {var ab=PMultiply(a,b);var ba=PMultiply(b,a);return new[,]{{ab[0,0]+ba[0,0]*-1,ab[0,1]+ba[0,1]*-1},{ab[1,0]+ba[1,0]*-1,ab[1,1]+ba[1,1]*-1}};}
 public static Polynomial PPair(Polynomial[,] a,Polynomial[,] b)
 {var ab=PMultiply(a,b);return (ab[0,0]+ab[1,1]).RealPart()*new Scalar(new Rational(-1,2),0);}
}

// Exact finite Fourier series with polynomial amplitude coefficients; normalized circle average is mode zero.
sealed class Fourier
{
 private readonly Dictionary<int,Polynomial> modes=new();
 private void Put(int k,Polynomial value){var p=modes.TryGetValue(k,out var old)?old+value:value;if(p.IsZero)modes.Remove(k);else modes[k]=p;}
 public static Fourier Constant(Polynomial p){var f=new Fourier();f.Put(0,p);return f;}
 public static Fourier Cos(){var f=new Fourier();f.Put(-1,Polynomial.Constant(new Scalar(new Rational(1,2),0)));f.Put(1,Polynomial.Constant(new Scalar(new Rational(1,2),0)));return f;}
 public static Fourier Sin(){var f=new Fourier();f.Put(-1,Polynomial.Constant(new Scalar(0,new Rational(1,2))));f.Put(1,Polynomial.Constant(new Scalar(0,new Rational(-1,2))));return f;}
 public static Fourier operator +(Fourier a,Fourier b){var f=new Fourier();foreach(var x in a.modes)f.Put(x.Key,x.Value);foreach(var x in b.modes)f.Put(x.Key,x.Value);return f;}
 public static Fourier operator *(Fourier a,Fourier b){var f=new Fourier();foreach(var x in a.modes)foreach(var y in b.modes)f.Put(x.Key+y.Key,x.Value*y.Value);return f;}
 public Fourier Scale(Scalar s){var f=new Fourier();foreach(var x in modes)f.Put(x.Key,x.Value*s);return f;}
 public Fourier Amplitude(Polynomial p){var f=new Fourier();foreach(var x in modes)f.Put(x.Key,x.Value*p);return f;}
 public Fourier Derivative(){var f=new Fourier();foreach(var x in modes)f.Put(x.Key,x.Value*new Scalar(0,x.Key));return f;}
 public Polynomial Average()=>modes.GetValueOrDefault(0,Polynomial.Constant(0));
 public bool Same(Fourier f)=>modes.Count==f.modes.Count&&modes.All(x=>f.modes.TryGetValue(x.Key,out var p)&&x.Value.Same(p));
 public bool IsZero=>modes.Count==0;
}
