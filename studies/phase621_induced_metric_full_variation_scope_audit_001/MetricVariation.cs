using static Geometry;
using static SpinGeometry;

// A genuine first-order scalar dual number. Values are unrestricted exact rationals.
internal readonly record struct Dual(Rational Value,Rational Delta)
{
 public static long Products{get;private set;}
 public static implicit operator Dual(long n)=>new(n,0);
 public static Dual operator +(Dual a,Dual b)=>new(a.Value+b.Value,a.Delta+b.Delta);
 public static Dual operator -(Dual a,Dual b)=>a+b*(Rational)(-1);
 public static Dual operator *(Dual a,Dual b)
 {Products++;return new(a.Value*b.Value,a.Delta*b.Value+a.Value*b.Delta);}
 public static Dual operator *(Dual a,Rational b)=>new(a.Value*b,a.Delta*b);
 public static bool Nonzero(Dual a)=>a.Value!=0||a.Delta!=0;
}

internal sealed class MetricData
{
 public Matrix G{get;init;}=new(14);
 public Matrix[] D{get;}=Enumerable.Range(0,14).Select(_=>new Matrix(14)).ToArray();
 public Matrix[,] DD{get;}=new Matrix[14,14];
 public MetricData(){for(int i=0;i<14;i++)for(int j=0;j<14;j++)DD[i,j]=new(14);}
 public static MetricData Baseline(Ambient g)
 {var r=new MetricData{G=g.Gram};for(int i=0;i<14;i++)for(int a=0;a<14;a++)for(int b=0;b<14;b++){r.D[i][a,b]=g.D[i,a,b];for(int j=0;j<14;j++)r.DD[i,j][a,b]=g.DD[i,j,a,b];}return r;}
}

internal sealed class ConnectionData
{
 public Matrix[] Gamma{get;}=Enumerable.Range(0,14).Select(_=>new Matrix(14)).ToArray();
 public Matrix[,] DGamma{get;}=new Matrix[14,14];
 public Rational[,,,] R{get;}=new Rational[14,14,14,14];
 public ConnectionData(){for(int i=0;i<14;i++)for(int j=0;j<14;j++)DGamma[i,j]=new(14);}
}

internal static class MetricVariation
{
 public static int[][] Multiindices()
 {var r=new List<int[]>();for(int degree=0;degree<=3;degree++)for(int a=0;a<=degree;a++)for(int b=0;b<=degree-a;b++)for(int c=0;c<=degree-a-b;c++)r.Add([a,b,c,degree-a-b-c]);return r.ToArray();}
 public static bool Matches(int[] multi,params int[] derivative)
 {var counts=new int[4];foreach(int d in derivative){if(d<0||d>=4)return false;counts[d]++;}return multi.SequenceEqual(counts);}
 public static Matrix[] Downstairs(Matrix h,Matrix m,int[] multi,params int[] extra)
 {
  // Direct dual Koszul: the value h^{-1} varies for zero jets, but multiplies
  // the identically zero baseline first derivatives. No zero-jet rule is inserted.
  var inverse=h.Inverse();var dinverse=(inverse*m*inverse).Scale(Matches(multi)?-1:0);
  var r=Enumerable.Range(0,4).Select(_=>new Matrix(4)).ToArray();
  for(int i=0;i<4;i++)for(int j=0;j<4;j++)for(int k=0;k<4;k++)for(int l=0;l<4;l++)
  {
   Rational derivative=(Matches(multi,extra.Append(i).ToArray())?m[j,l]:0)+(Matches(multi,extra.Append(j).ToArray())?m[i,l]:0)-(Matches(multi,extra.Append(l).ToArray())?m[i,j]:0);
   var product=new Dual(inverse[k,l],dinverse[k,l])*new Dual(0,derivative);r[i][k,j]+=product.Delta*new Rational(1,2);
  }return r;
 }
 public static Matrix Shear(Matrix y,Matrix[] c)
 {var n=new Matrix(14);for(int i=0;i<4;i++){var v=Coordinates(Transpose(c[i])*y+y*c[i]);for(int a=0;a<10;a++)n[a+4,i]=v[a];}return n;}
 public static MetricData ShearJets(Ambient g,Matrix h,Matrix m,int[] multi)
 {
  var c=Downstairs(h,m,multi);var r=new MetricData{G=Shear(g.Y,c)};
  for(int z=0;z<14;z++)
  {
   r.D[z]=z<4?Shear(g.Y,Downstairs(h,m,multi,z)):Shear(g.Vertical[z-4],c);
   for(int w=0;w<14;w++)r.DD[z,w]=z<4&&w<4?Shear(g.Y,Downstairs(h,m,multi,z,w)):z<4?Shear(g.Vertical[w-4],Downstairs(h,m,multi,z)):w<4?Shear(g.Vertical[z-4],Downstairs(h,m,multi,w)):new Matrix(14);
  }return r;
 }
 private static Matrix SymProduct(Matrix n,Matrix g)=> (Transpose(n)*g+g*n).Scale(-1);
 public static MetricData MetricJets(MetricData g,MetricData n)
 {
  var r=new MetricData{G=SymProduct(n.G,g.G)};
  for(int z=0;z<14;z++)
  {
   r.D[z]=SymProduct(n.D[z],g.G)+SymProduct(n.G,g.D[z]);
   for(int w=0;w<14;w++)r.DD[z,w]=SymProduct(n.DD[z,w],g.G)+SymProduct(n.D[z],g.D[w])+SymProduct(n.D[w],g.D[z])+SymProduct(n.G,g.DD[z,w]);
  }return r;
 }
 // Independent mixed-block differentiation. G_HH/G_VV have zero first variation.
 public static MetricData MetricJetsBlocks(MetricData g,MetricData n)
 {
  var r=new MetricData();
  for(int i=0;i<4;i++)for(int a=4;a<14;a++)for(int b=4;b<14;b++)
  {
   r.G[i,a]-=n.G[b,i]*g.G[b,a];r.G[a,i]=r.G[i,a];
   for(int z=0;z<14;z++)
   {
    r.D[z][i,a]-=n.D[z][b,i]*g.G[b,a]+n.G[b,i]*g.D[z][b,a];r.D[z][a,i]=r.D[z][i,a];
    for(int w=0;w<14;w++){r.DD[z,w][i,a]-=n.DD[z,w][b,i]*g.G[b,a]+n.D[z][b,i]*g.D[w][b,a]+n.D[w][b,i]*g.D[z][b,a]+n.G[b,i]*g.DD[z,w][b,a];r.DD[z,w][a,i]=r.DD[z,w][i,a];}
   }
  }return r;
 }
 public static (ConnectionData Value,ConnectionData Delta) DualConnection(MetricData g,MetricData k)
 {
  var v=new ConnectionData();var d=new ConnectionData();var inverse=g.G.Inverse();var deltaInverse=(inverse*k.G*inverse).Scale(-1);
  var ip=new Dual[14,14];var dip=new Dual[14,14,14];
  for(int a=0;a<14;a++)for(int b=0;b<14;b++)ip[a,b]=new(inverse[a,b],deltaInverse[a,b]);
  for(int z=0;z<14;z++)for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int i=0;i<14;i++)if(Dual.Nonzero(ip[a,i]))for(int j=0;j<14;j++)if(Dual.Nonzero(ip[j,b]))
  {var dg=new Dual(g.D[z][i,j],k.D[z][i,j]);if(Dual.Nonzero(dg))dip[z,a,b]-=ip[a,i]*dg*ip[j,b];}
  for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++)
  {
   Dual gamma=0;var dg=new Dual[14];
   for(int l=0;l<14;l++)
   {
    var t=new Dual((g.D[a][b,l]+g.D[b][a,l]-g.D[l][a,b])*new Rational(1,2),(k.D[a][b,l]+k.D[b][a,l]-k.D[l][a,b])*new Rational(1,2));
    if(Dual.Nonzero(ip[c,l])&&Dual.Nonzero(t))gamma+=ip[c,l]*t;
    for(int z=0;z<14;z++)
    {
     var dt=new Dual((g.DD[z,a][b,l]+g.DD[z,b][a,l]-g.DD[z,l][a,b])*new Rational(1,2),(k.DD[z,a][b,l]+k.DD[z,b][a,l]-k.DD[z,l][a,b])*new Rational(1,2));
     if(Dual.Nonzero(dip[z,c,l])&&Dual.Nonzero(t))dg[z]+=dip[z,c,l]*t;if(Dual.Nonzero(ip[c,l])&&Dual.Nonzero(dt))dg[z]+=ip[c,l]*dt;
    }
   }
   v.Gamma[a][c,b]=gamma.Value;d.Gamma[a][c,b]=gamma.Delta;for(int z=0;z<14;z++){v.DGamma[z,a][c,b]=dg[z].Value;d.DGamma[z,a][c,b]=dg[z].Delta;}
  }
  for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++)for(int e=0;e<14;e++)
  {
   var r=new Dual(v.DGamma[a,b][e,c]-v.DGamma[b,a][e,c],d.DGamma[a,b][e,c]-d.DGamma[b,a][e,c]);
   for(int l=0;l<14;l++)
   {
    var x=new Dual(v.Gamma[a][e,l],d.Gamma[a][e,l]);var y=new Dual(v.Gamma[b][l,c],d.Gamma[b][l,c]);if(Dual.Nonzero(x)&&Dual.Nonzero(y))r+=x*y;
    x=new(v.Gamma[b][e,l],d.Gamma[b][e,l]);y=new(v.Gamma[a][l,c],d.Gamma[a][l,c]);if(Dual.Nonzero(x)&&Dual.Nonzero(y))r-=x*y;
   }v.R[a,b,c,e]=r.Value;d.R[a,b,c,e]=r.Delta;
  }return(v,d);
 }
 public static ConnectionData LinearizedConnection(MetricData g,MetricData k,ConnectionData baseline)
 {
  var r=new ConnectionData();var inv=g.G.Inverse();var di=(inv*k.G*inv).Scale(-1);
  var invd=g.D.Select(x=>(inv*x*inv).Scale(-1)).ToArray();var did=Enumerable.Range(0,14).Select(z=>(invd[z]*k.G*inv+inv*k.D[z]*inv+inv*k.G*invd[z]).Scale(-1)).ToArray();
  for(int a=0;a<14;a++)
  {
   var t=new Matrix(14);var dt=new Matrix(14);
   for(int b=0;b<14;b++)for(int l=0;l<14;l++){t[l,b]=(g.D[a][b,l]+g.D[b][a,l]-g.D[l][a,b])*new Rational(1,2);dt[l,b]=(k.D[a][b,l]+k.D[b][a,l]-k.D[l][a,b])*new Rational(1,2);}
   r.Gamma[a]=di*t+inv*dt;
   for(int z=0;z<14;z++)
   {
    var tz=new Matrix(14);var dtz=new Matrix(14);for(int b=0;b<14;b++)for(int l=0;l<14;l++){tz[l,b]=(g.DD[z,a][b,l]+g.DD[z,b][a,l]-g.DD[z,l][a,b])*new Rational(1,2);dtz[l,b]=(k.DD[z,a][b,l]+k.DD[z,b][a,l]-k.DD[z,l][a,b])*new Rational(1,2);}
    r.DGamma[z,a]=did[z]*t+di*tz+invd[z]*dt+inv*dtz;
   }
  }
  // Palatini identity: antisymmetrizing the derivative of the (1,2)-tensor
  // delta Gamma cancels the lower form-slot connection because torsion is zero.
  for(int a=0;a<14;a++)for(int b=0;b<14;b++)
  {var m=r.DGamma[a,b]-r.DGamma[b,a]+Commutator(baseline.Gamma[a],r.Gamma[b])-Commutator(baseline.Gamma[b],r.Gamma[a]);for(int c=0;c<14;c++)for(int d=0;d<14;d++)r.R[a,b,c,d]=m[d,c];}return r;
 }
 public static Matrix[] FrameConnectionVariation(ConnectionData baseline,ConnectionData delta,MetricData n,Matrix e,Matrix ei)
 {return Enumerable.Range(0,14).Select(a=>ei*(delta.Gamma[a]+n.D[a]+Commutator(baseline.Gamma[a],n.G))*e).ToArray();}
 public static Rational[,,,] CurvatureVariationFrame(Rational[,,,] baseline,Rational[,,,] delta,Matrix n,Matrix e,Matrix ei)
 {
  var effective=new Rational[14,14,14,14];for(int a=0;a<14;a++)for(int b=0;b<14;b++)
  {
   var m=new Matrix(14);var r=new Matrix(14);for(int c=0;c<14;c++)for(int d=0;d<14;d++){m[d,c]=delta[a,b,c,d];r[d,c]=baseline[a,b,c,d];}
   m=m+Commutator(r,n);
   for(int i=0;i<14;i++)for(int c=0;c<14;c++)for(int d=0;d<14;d++){if(n[i,a]!=0)m[d,c]+=n[i,a]*baseline[i,b,c,d];if(n[i,b]!=0)m[d,c]+=n[i,b]*baseline[a,i,c,d];}
   for(int c=0;c<14;c++)for(int d=0;d<14;d++)effective[a,b,c,d]=m[d,c];
  }return EndomorphismFrame(effective,e,ei);
 }
 public static Rational[,,,] CurvatureVariationLowered(Rational[,,,] baseline,Rational[,,,] delta,Matrix g,Matrix k,Matrix n,Matrix e)
 {
  var low=Lower(baseline,g);var changed=Lower(delta,g);var extra=Lower(baseline,k);
  for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++)for(int d=0;d<14;d++)
  {changed[a,b,c,d]+=extra[a,b,c,d];for(int i=0;i<14;i++){if(n[i,a]!=0)changed[a,b,c,d]+=n[i,a]*low[i,b,c,d];if(n[i,b]!=0)changed[a,b,c,d]+=n[i,b]*low[a,i,c,d];if(n[i,c]!=0)changed[a,b,c,d]+=n[i,c]*low[a,b,i,d];if(n[i,d]!=0)changed[a,b,c,d]+=n[i,d]*low[a,b,c,i];}}
  return LoweredFrame(changed,e);
 }
}
