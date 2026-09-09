using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// One formal real variable; never evaluated at a parameter grid or truncated.
sealed class CP
{
 public Rational[] Coefficients{get;}
 public CP(params Rational[] values)
 {int length=values.Length;while(length>1&&values[length-1]==0)length--;Coefficients=length==0?[0]:values.Take(length).ToArray();}
 public static CP Variable=>new(0,1);
 public static implicit operator CP(long n)=>new(new Rational(n));
 public static implicit operator CP(Rational n)=>new(n);
 public Rational this[int i]=>i<Coefficients.Length?Coefficients[i]:0;
 public bool IsZero=>Coefficients.All(v=>v==0);
 public static CP operator +(CP a,CP b)=>new(Enumerable.Range(0,System.Math.Max(a.Coefficients.Length,b.Coefficients.Length)).Select(i=>a[i]+b[i]).ToArray());
 public static CP operator -(CP a,CP b)=>a+b.Scale(-1);
 public static CP operator *(CP a,CP b)
 {var r=new Rational[a.Coefficients.Length+b.Coefficients.Length-1];for(int i=0;i<a.Coefficients.Length;i++)for(int j=0;j<b.Coefficients.Length;j++)r[i+j]+=a[i]*b[j];return new(r);}
 public CP Scale(Rational a)=>new(Coefficients.Select(v=>v*a).ToArray());
 public bool Same(CP b)=>Coefficients.SequenceEqual(b.Coefficients);
 public string[] Text()=>Coefficients.Select(v=>v.ToString()).ToArray();
}

internal static class Cyclic
{
 public static Rational Inv(Rational a)=>a==0?throw new DivideByZeroException():new(a.Denominator,a.Numerator);
 public static FT[] Normalize(FT[] a)
 {int length=a.Length;while(length>1&&a[length-1].Count==0)length--;return length==0?[new FT()]:a.Take(length).ToArray();}
 public static FT[] Add(FT[] a,FT[] b)=>Normalize(Enumerable.Range(0,System.Math.Max(a.Length,b.Length)).Select(i=>Fourier.Add(i<a.Length?a[i]:new(),i<b.Length?b[i]:new())).ToArray());
 public static FT[] Scale(FT[] a,Rational s)=>Normalize(a.Select(v=>Fourier.Scale(v,new Scalar(s,0))).ToArray());
 public static bool Same(FT[] a,FT[] b)=>Enumerable.Range(0,System.Math.Max(a.Length,b.Length)).All(i=>Fourier.Equal(i<a.Length?a[i]:new(),i<b.Length?b[i]:new()));
 public static bool Zero(FT[] a)=>a.All(v=>v.Count==0);
 public static CP Pair(FT[] a,FT[] b)
 {var r=new Rational[a.Length+b.Length-1];for(int i=0;i<a.Length;i++)for(int j=0;j<b.Length;j++)r[i+j]+=Fourier.Pair(a[i],b[j]);return new(r);}
 public static object[] Terms(FT[] a)=>a.Select((v,i)=>(object)new{power=i,terms=Fourier.Terms(v)}).ToArray();
 public static Rational[][] Zeros(int n)=>Enumerable.Range(0,n).Select(_=>new Rational[n]).ToArray();
 public static Rational[][] Identity(int n){var a=Zeros(n);for(int i=0;i<n;i++)a[i][i]=1;return a;}
 public static Rational[][] Multiply(Rational[][] a,Rational[][] b)=>a.Select(row=>Enumerable.Range(0,b[0].Length).Select(j=>Enumerable.Range(0,b.Length).Select(k=>row[k]*b[k][j]).Aggregate(new Rational(0),(s,v)=>s+v)).ToArray()).ToArray();
 public static Rational[][] Transpose(Rational[][] a)=>Enumerable.Range(0,a[0].Length).Select(j=>a.Select(row=>row[j]).ToArray()).ToArray();
 public static bool SameMatrix(Rational[][] a,Rational[][] b)=>a.Length==b.Length&&a.Zip(b,(r,s)=>r.SequenceEqual(s)).All(v=>v);
 public static string[][] Text(Rational[][] a)=>a.Select(row=>row.Select(v=>v.ToString()).ToArray()).ToArray();
 public static int Rank(Rational[][] input)
 {var a=input.Select(row=>row.ToArray()).ToArray();int r=0;for(int j=0;j<a[0].Length&&r<a.Length;j++){int p=r;while(p<a.Length&&a[p][j]==0)p++;if(p==a.Length)continue;(a[r],a[p])=(a[p],a[r]);var inv=Inv(a[r][j]);for(int k=j;k<a[0].Length;k++)a[r][k]*=inv;for(int i=r+1;i<a.Length;i++){var c=a[i][j];for(int k=j;k<a[0].Length;k++)a[i][k]-=c*a[r][k];}r++;}return r;}
 public static Rational[][] Inverse(Rational[][] input)
 {int n=input.Length;var a=input.Select((row,i)=>row.Concat(Identity(n)[i]).ToArray()).ToArray();for(int j=0;j<n;j++){int p=j;while(p<n&&a[p][j]==0)p++;if(p==n)throw new InvalidOperationException("Degenerate Gram");(a[j],a[p])=(a[p],a[j]);var inv=Inv(a[j][j]);for(int k=0;k<2*n;k++)a[j][k]*=inv;for(int i=0;i<n;i++)if(i!=j){var c=a[i][j];for(int k=0;k<2*n;k++)a[i][k]-=c*a[j][k];}}return a.Select(row=>row.Skip(n).ToArray()).ToArray();}
 public static CP Determinant(CP[][] a)
 {int n=a.Length;CP sum=0;void Visit(int row,int used,int inversions,CP product){if(row==n){sum+=product.Scale(inversions%2==0?1:-1);return;}for(int j=0;j<n;j++)if((used&(1<<j))==0){int newInv=System.Numerics.BitOperations.PopCount((uint)(used>>(j+1)));Visit(row+1,used|(1<<j),inversions+newInv,product*a[row][j]);}}Visit(0,0,0,1);return sum;}
 public static CP Characteristic(Rational[][] a)=>Determinant(a.Select((row,i)=>row.Select((v,j)=>(i==j?CP.Variable:(CP)0)-(CP)v).ToArray()).ToArray());
 public static Rational[] PowerRanks(Rational[][] a){var a2=Multiply(a,a);var a3=Multiply(a2,a);return [Rank(a),Rank(a2),Rank(a3)];}
 public static CP[][] Gram(FT[][] basis)=>basis.Select(a=>basis.Select(b=>Pair(a,b)).ToArray()).ToArray();
 public static Rational[][] Gram(FT[] basis)=>basis.Select(a=>basis.Select(b=>Fourier.Pair(a,b)).ToArray()).ToArray();
 public static bool SameGram(CP[][] a,CP[][] b)=>a.Length==b.Length&&a.Zip(b,(r,s)=>r.Length==s.Length&&r.Zip(s,(x,y)=>x.Same(y)).All(v=>v)).All(v=>v);
 public static object GramText(CP[][] a)=>a.Select(row=>row.Select(v=>v.Text()).ToArray()).ToArray();
}
