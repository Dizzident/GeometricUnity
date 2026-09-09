using static Geometry;

internal static class Splitting
{
 public static Matrix Transpose(Matrix a){var r=new Matrix(a.N);for(int i=0;i<a.N;i++)for(int j=0;j<a.N;j++)r[i,j]=a[j,i];return r;}
 public static Matrix[] ZeroJet()=>Enumerable.Range(0,4).Select(_=>new Matrix(4)).ToArray();
 public static Matrix[] Jet(int index){var j=ZeroJet();j[index/10]=Basis()[index%10];return j;}
 public static Matrix[] Seed(){var j=ZeroJet();var basis=Basis();for(int i=0;i<4;i++)j[i]=basis[(3*i+1)%10].Scale(i+1);return j;}
 // C_i[k,j]=Gamma^k_ij; the two lower connection indices are symmetric.
 public static Matrix[] Koszul(Matrix h,Matrix[] jet)
 {var p=h.Inverse();var c=ZeroJet();for(int i=0;i<4;i++)for(int j=0;j<4;j++)for(int k=0;k<4;k++)for(int l=0;l<4;l++)c[i][k,j]+=p[k,l]*(jet[i][j,l]+jet[j][i,l]-jet[l][i,j])*new Rational(1,2);return c;}
 public static Matrix[] Lower(Matrix y,Matrix[] c)=>c.Select(a=>Transpose(a)*y+y*a).ToArray();
 public static Rational[] Flatten(Matrix[] jet)=>jet.SelectMany(Coordinates).ToArray();
 public static Matrix N(Matrix[] l){var r=new Matrix(14);for(int i=0;i<4;i++){var v=Coordinates(l[i]);for(int a=0;a<10;a++)r[a+4,i]=v[a];}return r;}
 public static Matrix Metric(Matrix d,Matrix n){var inv=Matrix.Identity(14)-n;return Transpose(inv)*d*inv;}
 public static Matrix Blocks(Matrix d,Matrix n)
 {var g=d.Copy();for(int i=0;i<4;i++)for(int j=0;j<4;j++)for(int a=4;a<14;a++)for(int b=4;b<14;b++)g[i,j]+=n[a,i]*d[a,b]*n[b,j];for(int i=0;i<4;i++)for(int a=4;a<14;a++){Rational v=0;for(int b=4;b<14;b++)v-=n[b,i]*d[b,a];g[i,a]=v;g[a,i]=v;}return g;}
 public static Matrix Tangent(Matrix d,Matrix n,Matrix dn)
 {var inv=Matrix.Identity(14)-n;return (Transpose(dn)*d*inv+Transpose(inv)*d*dn).Scale(-1);}
 public static Matrix TangentBlocks(Matrix d,Matrix n,Matrix dn)
 {var r=new Matrix(14);for(int i=0;i<4;i++)for(int j=0;j<4;j++)for(int a=4;a<14;a++)for(int b=4;b<14;b++)r[i,j]+=dn[a,i]*d[a,b]*n[b,j]+n[a,i]*d[a,b]*dn[b,j];for(int i=0;i<4;i++)for(int a=4;a<14;a++){Rational v=0;for(int b=4;b<14;b++)v-=dn[b,i]*d[b,a];r[i,a]=v;r[a,i]=v;}return r;}
 public static Rational ExpectedDeterminant(Matrix y,Rational beta)=>64*(1+4*beta)*Matrix.Inv(Pow(y.Determinant(),4));
 public static bool VerticalSame(Matrix a,Matrix b)=>Enumerable.Range(4,10).All(i=>Enumerable.Range(4,10).All(j=>a[i,j]==b[i,j]));
 public static bool HorizontalZero(Matrix a)=>Enumerable.Range(0,4).All(i=>Enumerable.Range(0,4).All(j=>a[i,j]==0));
 public static bool MixedNonzero(Matrix a)=>Enumerable.Range(0,4).Any(i=>Enumerable.Range(4,10).Any(j=>a[i,j]!=0));
 public static Matrix ConnectionMap(Matrix h,Matrix y)
 {var m=new Matrix(40);for(int j=0;j<40;j++){var v=Flatten(Lower(y,Koszul(h,Jet(j))));for(int i=0;i<40;i++)m[i,j]=v[i];}return m;}
}
