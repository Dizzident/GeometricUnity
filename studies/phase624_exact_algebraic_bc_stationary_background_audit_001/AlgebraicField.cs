// Exact real degree-three field. No floating point or root approximation.
// Coefficients are in the ordered basis (1,r,r^2), p(r)=0.
internal readonly record struct Cubic(Rational C0,Rational C1,Rational C2)
{
 public static long Products{get;private set;}
 public Rational this[int i]=>i switch{0=>C0,1=>C1,2=>C2,_=>throw new ArgumentOutOfRangeException(nameof(i))};
 public bool Zero=>C0==0&&C1==0&&C2==0;
 public static implicit operator Cubic(long n)=>new(n,0,0);
 public static implicit operator Cubic(Rational n)=>new(n,0,0);
 public static Cubic R=>new(0,1,0);
 public static Cubic Basis(int i)=>i switch{0=>new(1,0,0),1=>new(0,1,0),2=>new(0,0,1),_=>throw new ArgumentOutOfRangeException(nameof(i))};
 public static Cubic operator +(Cubic a,Cubic b)=>new(a.C0+b.C0,a.C1+b.C1,a.C2+b.C2);
 public static Cubic operator -(Cubic a,Cubic b)=>new(a.C0-b.C0,a.C1-b.C1,a.C2-b.C2);
 public static Cubic operator *(Cubic a,Cubic b)
 {Products++;var p=new Rational[5];for(int i=0;i<3;i++)for(int j=0;j<3;j++)p[i+j]+=a[i]*b[j];return Reduce(p);}
 public static Cubic Reduce(Rational[] input)
 {
  var p=new Rational[Math.Max(3,input.Length)];Array.Copy(input,p,input.Length);
  for(int k=p.Length-1;k>=3;k--){Rational a=p[k]*new Rational(1,693);p[k-3]+=17*a;p[k-2]+=64*a;p[k-1]+=511*a;p[k]=0;}return new(p[0],p[1],p[2]);
 }
 public static Cubic Monomial(int degree){var p=new Rational[degree+1];p[degree]=1;return Reduce(p);}
 // Independent multiplication by the companion map, not polynomial division.
 public static Cubic MultiplyCompanion(Cubic a,Cubic b)
 {
  Cubic Shift(Cubic x)=>new(new Rational(17,693)*x.C2,x.C0+new Rational(64,693)*x.C2,x.C1+new Rational(511,693)*x.C2);
  Cubic Scale(Cubic x,Rational q)=>new(x.C0*q,x.C1*q,x.C2*q);
  return Scale(a,b.C0)+Scale(Shift(a),b.C1)+Scale(Shift(Shift(a)),b.C2);
 }
 public Cubic Inverse()
 {
  if(Zero)throw new DivideByZeroException();var m=new Matrix(3);
  for(int j=0;j<3;j++){var q=this*Basis(j);for(int i=0;i<3;i++)m[i,j]=q[i];}
  var v=m.Inverse();return new(v[0,0],v[1,0],v[2,0]);
 }
 // Independent extended Euclidean inverse in Q[x], without a 3x3 solve.
 public Cubic EuclideanInverse()
 {
  if(Zero)throw new DivideByZeroException();Rational[] old=[-17,-64,-511,693],current=Trim([C0,C1,C2]),t0=[],t1=[1];int steps=0;
  while(current.Length>0)
  {if(++steps>4)throw new InvalidOperationException("cubic Euclidean step bound");var (q,remainder)=Divide(old,current);(old,current)=(current,remainder);(t0,t1)=(t1,Subtract(t0,Multiply(q,t1)));}
  if(old.Length!=1)throw new InvalidOperationException("irreducible-field inverse");return Reduce(t0.Select(x=>x*Matrix.Inv(old[0])).ToArray());
 }
 public string[] Text()=>[C0.ToString(),C1.ToString(),C2.ToString()];
 private static Rational[] Trim(Rational[] a){int n=a.Length;while(n>0&&a[n-1]==0)n--;return a.Take(n).ToArray();}
 private static Rational[] Subtract(Rational[] a,Rational[] b)=>Trim(Enumerable.Range(0,Math.Max(a.Length,b.Length)).Select(i=>(i<a.Length?a[i]:new Rational(0))-(i<b.Length?b[i]:new Rational(0))).ToArray());
 private static Rational[] Multiply(Rational[] a,Rational[] b)
 {if(a.Length==0||b.Length==0)return[];var p=new Rational[a.Length+b.Length-1];for(int i=0;i<a.Length;i++)for(int j=0;j<b.Length;j++)p[i+j]+=a[i]*b[j];return Trim(p);}
 private static (Rational[] Quotient,Rational[] Remainder) Divide(Rational[] a,Rational[] b)
 {if(b.Length==0)throw new DivideByZeroException();var r=(Rational[])a.Clone();var q=new Rational[Math.Max(0,a.Length-b.Length+1)];for(int k=r.Length-1;k>=b.Length-1;k--){Rational factor=r[k]*Matrix.Inv(b[^1]);q[k-b.Length+1]=factor;for(int j=0;j<b.Length;j++)r[k-b.Length+1+j]-=factor*b[j];}return(Trim(q),Trim(r));}
}
