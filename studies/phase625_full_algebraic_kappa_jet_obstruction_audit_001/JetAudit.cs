using static Algebra;
using static Fourier;

// Exact 3x3 algebraic linear algebra. No response forecast is an input.
internal sealed class JetMatrix
{
 public Cubic[,] Values{get;}=new Cubic[3,3];
 public Cubic this[int i,int j]{get=>Values[i,j];set=>Values[i,j]=value;}
 public static JetMatrix Columns(Cubic[][] columns){var m=new JetMatrix();for(int j=0;j<3;j++)for(int i=0;i<3;i++)m[i,j]=columns[j][i];return m;}
 public static JetMatrix Identity(){var m=new JetMatrix();for(int i=0;i<3;i++)m[i,i]=1;return m;}
 public Cubic Determinant()=>this[0,0]*(this[1,1]*this[2,2]-this[1,2]*this[2,1])-this[0,1]*(this[1,0]*this[2,2]-this[1,2]*this[2,0])+this[0,2]*(this[1,0]*this[2,1]-this[1,1]*this[2,0]);
 public JetMatrix AdjugateInverse()
 {
  var det=Determinant();var inverse=det.Inverse();var m=new JetMatrix();
  for(int i=0;i<3;i++)for(int j=0;j<3;j++){var rows=Enumerable.Range(0,3).Where(k=>k!=j).ToArray();var cols=Enumerable.Range(0,3).Where(k=>k!=i).ToArray();m[i,j]=(this[rows[0],cols[0]]*this[rows[1],cols[1]]-this[rows[0],cols[1]]*this[rows[1],cols[0]])*((i+j)%2==0?(Cubic)1:(Cubic)(-1))*inverse;}return m;
 }
 // Independent Gaussian elimination with Euclidean scalar inverses.
 public JetMatrix GaussianInverse()
 {
  var a=new Cubic[3,6];for(int i=0;i<3;i++)for(int j=0;j<3;j++){a[i,j]=this[i,j];a[i,j+3]=i==j?(Cubic)1:(Cubic)0;}
  for(int j=0;j<3;j++){int pivot=j;while(pivot<3&&a[pivot,j].Zero)pivot++;if(pivot==3)throw new InvalidOperationException("singular response block");if(pivot!=j)for(int k=0;k<6;k++)(a[pivot,k],a[j,k])=(a[j,k],a[pivot,k]);var inverse=a[j,j].EuclideanInverse();for(int k=0;k<6;k++)a[j,k]*=inverse;for(int i=0;i<3;i++)if(i!=j){var factor=a[i,j];for(int k=0;k<6;k++)a[i,k]-=factor*a[j,k];}}
  var m=new JetMatrix();for(int i=0;i<3;i++)for(int j=0;j<3;j++)m[i,j]=a[i,j+3];return m;
 }
 public static JetMatrix operator *(JetMatrix a,JetMatrix b){var m=new JetMatrix();for(int i=0;i<3;i++)for(int j=0;j<3;j++)for(int k=0;k<3;k++)m[i,j]+=a[i,k]*b[k,j];return m;}
 public Cubic[] Apply(Cubic[] x)=>Enumerable.Range(0,3).Select(i=>Enumerable.Range(0,3).Aggregate((Cubic)0,(s,j)=>s+this[i,j]*x[j])).ToArray();
 public object Text()=>Enumerable.Range(0,3).Select(i=>Enumerable.Range(0,3).Select(j=>this[i,j].Text()).ToArray()).ToArray();
}
internal sealed record JetField(string Id,AT Input,Kinetic.Result[] Coefficients,FeedbackResult Feedback)
{
 public AT Forward=>new(Coefficients.Select(x=>x.Forward).ToArray());
 public AT Reverse=>new(Coefficients.Select(x=>x.Reverse).ToArray());
 public AT H=>new(Coefficients.Select(x=>x.Full).ToArray());
 public AT Adjoint=>new(Coefficients.Select(x=>x.Adjoint).ToArray());
 public object KineticEvidence()=>new{id=Id,input=AT.Terms(Input),coefficients=Coefficients.Select(Kinetic.Evidence),forward=AT.Terms(Forward),reverse=AT.Terms(Reverse),full=AT.Terms(H),adjoint=AT.Terms(Adjoint)};
 public object FeedbackEvidence()=>new{id=Id,result=Feedback.Evidence()};
}
internal static class JetTools
{
 public static AT Grade(AT input,int grade)=>new(input.Coefficients.Select(t=>t.Where(q=>Degree(q.Key.Blade)==grade).ToDictionary(q=>q.Key,q=>q.Value)).ToArray());
 public static Cubic[] Coordinates(AT input,AT[] basis,Rational[] norms)=>Enumerable.Range(0,3).Select(i=>AT.Pair(basis[i],input)*(Cubic)Matrix.Inv(norms[i])).ToArray();
 public static AT From(AT[] basis,Cubic[] coefficients)=>AT.Combine(basis,coefficients);
 public static Cubic[] Current(AT adjoint,AT variation)=>Enumerable.Range(0,14).Select(a=>AT.Pair(AT.Linear(adjoint,x=>Kinetic.Contract(x,a)),variation)*(Cubic)Sigma(a)).ToArray();
 public static Cubic Divergence(Matrix[] lambda,Cubic[] current){Cubic value=0;for(int a=0;a<14;a++)for(int b=0;b<14;b++)value+=(Cubic)lambda[a][a,b]*current[b];return value;}
 public static Cubic Component(AT input,int form,int blade)=>new(input.Coefficients[0].GetValueOrDefault((form,blade,0,0)).Real,input.Coefficients[1].GetValueOrDefault((form,blade,0,0)).Real,input.Coefficients[2].GetValueOrDefault((form,blade,0,0)).Real);
}
