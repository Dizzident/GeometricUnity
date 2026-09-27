using static Algebra;
using static Fourier;
using static Mixed;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Geometry is rebuilt from the induced metric, not from the expected mixed
// response. The associated frame first jet is the same one passed in618/621.
internal sealed class MixedBackground
{
 public int Point{get;}
 public Ambient Ambient{get;}
 public Matrix Frame{get;}
 public Matrix InverseFrame{get;}
 public MetricData Metric{get;}
 public ConnectionData Connection{get;}
 public Matrix[] FrameLift{get;}=MixedGeometryAdmission.Vector<Matrix>(14);
 public Matrix[] FramePartial{get;}=MixedGeometryAdmission.Vector<Matrix>(14);
 public Matrix[] Omega{get;}=MixedGeometryAdmission.Vector<Matrix>(14);
 public FT[] NativePartial{get;}=MixedGeometryAdmission.Vector<FT>(14);
 public FT[] NativePartialOracle{get;}=MixedGeometryAdmission.Vector<FT>(14);
 public FT[] CovariantCoordinate{get;}=MixedGeometryAdmission.Vector<FT>(14);
 public FT[] CovariantFrame{get;}=MixedGeometryAdmission.Vector<FT>(14);
 public FT X{get;}
 public FT NativeX{get;}
 public FT DX{get;}
 public FT DXOracle{get;}
 public FT NativeExterior{get;}
 public FT B{get;}
 public FT F{get;}
 public FT Q{get;}
 public FT AdjointX{get;}
 public FT[] KInputs{get;}
 public FT ReverseX{get;}
 public FT[] GradientPieces{get;}
 public FT Gradient{get;}
 public MixedBackground(int point,FT x,Action<string,int,FT>? geometryLeaf=null)
 {
  MixedProducerStages.Arrays(4);Point=point;X=x;var y=point==0?Matrix.Diagonal(-1,1,1,1):Matrix.Diagonal(-1,4,9,16);
  Ambient=MixedGeometryAdmission.CreateAmbient(y,1,new Rational(-1,2),-1);
  Metric=MixedGeometryAdmission.BaselineMetric(Ambient);
  MixedProducerStages.Arrays(12);Frame=SpinGeometry.Frame(point);InverseFrame=Frame.Inverse();
  MixedGeometryAdmission.Reserve(MixedGeometryAdmission.Stage.MetricContainer);
  Connection=MixedGeometryAdmission.DualConnection(Metric,new MetricData()).Value;
  B=MixedProducerStages.Empty();NativeX=Pullback(InverseFrame,x);
  MixedProducerStages.MatrixSlots(14*14);MixedProducerStages.Arrays(14*14*2+14*3,14*14+14);
  for(int mu=0;mu<14;mu++)
  {
   FrameLift[mu]=Homogeneous.Rho(Homogeneous.Lift(Ambient,mu));FramePartial[mu]=FrameLift[mu]*Frame;
   Omega[mu]=InverseFrame*(Connection.Gamma[mu]+FrameLift[mu])*Frame;
   var spin=Spin(Omega[mu]);for(int a=0;a<14;a++)if(Frame[mu,a]!=0)B=Sum(B,Times(P(Unit(1<<a,0,1),spin),Frame[mu,a]));
   // Direct differentiation of the native coframe, with X's associated-frame
   // components constant by the independently checked homogeneous equivariance.
   var inversePartial=(InverseFrame*FrameLift[mu]).Scale(-1);
   NativePartial[mu]=Pullback(inversePartial,x);
   CovariantCoordinate[mu]=Derivative(Omega[mu],x);
   var partialInFrame=Sum(CovariantCoordinate[mu],Motion(InverseFrame*Connection.Gamma[mu]*Frame,x),Times(P(spin,x,'C'),-1));
   NativePartialOracle[mu]=Pullback(InverseFrame,partialInFrame,true);
  }
  MixedProducerStages.Arrays(14*14,14);MixedProducerStages.Metadata(14*4);MixedProducerStages.Scan(14*14);
  for(int a=0;a<14;a++)CovariantFrame[a]=Sum(Enumerable.Range(0,14).Select(mu=>Times(CovariantCoordinate[mu],Frame[mu,a])).ToArray());
  DX=WedgeCoordinate(CovariantCoordinate,Frame);
  NativeExterior=WedgeCoordinate(NativePartial,Matrix.Identity(14));
  MixedProducerStages.Arrays(2);DXOracle=Sum(Pullback(Frame,NativeExterior,true),P(B,x,'C',true));
  var curvature=MixedGeometryAdmission.EndomorphismFrame(Connection.R,Frame,InverseFrame);
  F=MixedGeometryAdmission.Lift(MixedGeometryAdmission.Lower(curvature,SpinGeometry.Eta()));
  if(MixedTrace.Active is not null&&geometryLeaf is null)throw new InvalidOperationException("traced background requires independently bound geometry leaves");
  geometryLeaf?.Invoke("curvature",2,F);Q=P(x,x);
  AdjointX=FixedAdjoint(x);MixedProducerStages.Arrays(3);KInputs=[FixedForward(F)[7],FixedForward(DX)[7],FixedForward(Q)[7]];
  MixedProducerStages.Arrays(14+14+14*14,16);MixedProducerStages.Metadata(4+4+14*4);MixedProducerStages.Scan(14+14+14*14);
  var reverseCoordinate=Omega.Select(omega=>Derivative(omega,AdjointX)).ToArray();
  var reverseFrame=Enumerable.Range(0,14).Select(a=>Sum(Enumerable.Range(0,14).Select(mu=>Times(reverseCoordinate[mu],Frame[mu,a])).ToArray())).ToArray();
  ReverseX=Divergence(reverseFrame);
  MixedProducerStages.Arrays(8,3);GradientPieces=[KInputs[0],Times(Sum(KInputs[1],ReverseX),new Rational(1,2)),Times(Sum(KInputs[2],Transpose(x,AdjointX)),new Rational(1,3)),Times(x,907712)];
  Gradient=Sum(GradientPieces);
 }
}

internal sealed class MixedMetricGerm
{
 public int MetricBasis{get;}
 public int JetIndex{get;}
 public int[] Multiindex{get;}
 public int Order=>Multiindex.Sum();
 public MetricData Shear{get;}
 public MetricData DeltaMetric{get;}
 public MetricData BlockMetric{get;}
 public ConnectionData DeltaConnection{get;}
 public ConnectionData Palatini{get;}
 public Matrix Motion{get;}
 public Matrix[] MotionPartial{get;}=MixedGeometryAdmission.Vector<Matrix>(14);
 public Matrix[] MotionCovariant{get;}=MixedGeometryAdmission.Vector<Matrix>(14);
 public Matrix[] DeltaOmega{get;}
 public Matrix[,] DeltaOmegaPartial{get;}=MixedGeometryAdmission.Grid<Matrix>(14,14);
 public Matrix DeltaFrame{get;}
 public FT DeltaB{get;}
 public FT DeltaFAdapted{get;}
 public FT DeltaFOracle{get;}
 public FT DeltaFFixed{get;}
 public FT DeltaBExterior{get;}
 public FT DeltaCurvatureFromConnection{get;}
 public MixedMetricGerm(MixedBackground b,int metricBasis,int jetIndex,Action<string,int,FT>? geometryLeaf=null)
 {
  // The35 four-component multiindices plus conservative List growth and its
  // final reference array; no scientific coefficients are consulted.
  MixedProducerStages.Arrays(35*4+160,43);MixedProducerStages.Metadata(1);MixedProducerStages.Scan(200);
  MetricBasis=metricBasis;JetIndex=jetIndex;Multiindex=MetricVariation.Multiindices()[jetIndex];
  MixedProducerStages.Arrays(4);
  Shear=MixedGeometryAdmission.ShearJets(b.Ambient,Matrix.Diagonal(-1,1,1,1),Geometry.Basis()[metricBasis],Multiindex);
  DeltaMetric=MixedGeometryAdmission.MetricJets(b.Metric,Shear);
  BlockMetric=MixedGeometryAdmission.MetricJetsBlocks(b.Metric,Shear);
  DeltaConnection=MixedGeometryAdmission.DualConnection(b.Metric,DeltaMetric).Delta;
  Palatini=MixedGeometryAdmission.LinearizedConnection(b.Metric,BlockMetric,b.Connection);
  Motion=b.InverseFrame*Shear.G*b.Frame;DeltaFrame=Shear.G*b.Frame;
  MixedProducerStages.Arrays(14);MixedProducerStages.Metadata(4);MixedProducerStages.Scan(14);DeltaOmega=MetricVariation.FrameConnectionVariation(b.Connection,DeltaConnection,Shear,b.Frame,b.InverseFrame);
  MixedProducerStages.MatrixSlots(14*14);MixedProducerStages.Arrays(14*14*2,14*14);
  DeltaB=MixedProducerStages.Empty();for(int mu=0;mu<14;mu++)
  {
   var spin=Spin(DeltaOmega[mu]);for(int a=0;a<14;a++)if(b.Frame[mu,a]!=0)DeltaB=Sum(DeltaB,Times(P(Unit(1<<a,0,1),spin),b.Frame[mu,a]));
   MotionPartial[mu]=b.InverseFrame*(Shear.D[mu]+Geometry.Commutator(Shear.G,b.FrameLift[mu]))*b.Frame;
  }
  MixedProducerStages.Arrays(14+14*14,15);MixedProducerStages.Metadata(4+14*4);MixedProducerStages.Scan(14+14*14);
  var covariantCoordinate=Enumerable.Range(0,14).Select(mu=>b.InverseFrame*(Shear.D[mu]+Geometry.Commutator(b.Connection.Gamma[mu],Shear.G))*b.Frame).ToArray();
  for(int a=0;a<14;a++)MotionCovariant[a]=Homogeneous.Combine(covariantCoordinate,Enumerable.Range(0,14).Select(mu=>b.Frame[mu,a]).ToArray());
  // Differentiate the FIXED-coordinate spin-reference variation itself.
  // Only first baseline frame jets enter; no fictitious globally integrable
  // field of preferred GL lifts or missing symmetric frame Hessian is assumed.
  MixedProducerStages.MatrixSlots(14L*14*14+14L*14*14*14);MixedProducerStages.Arrays(2L*14*14*14*14,14L*14*14*14);
  DeltaBExterior=MixedProducerStages.Empty();for(int mu=0;mu<14;mu++)for(int nu=0;nu<14;nu++)
  {
   var z=DeltaConnection.Gamma[mu]+Shear.D[mu]+Geometry.Commutator(b.Connection.Gamma[mu],Shear.G);
   var dz=DeltaConnection.DGamma[nu,mu]+Shear.DD[nu,mu]+Geometry.Commutator(b.Connection.DGamma[nu,mu],Shear.G)+Geometry.Commutator(b.Connection.Gamma[mu],Shear.D[nu]);
   DeltaOmegaPartial[nu,mu]=b.InverseFrame*(dz+Geometry.Commutator(z,b.FrameLift[nu]))*b.Frame;
   var spin=Spin(DeltaOmegaPartial[nu,mu]);
   for(int i=0;i<14;i++)if(b.Frame[nu,i]!=0)for(int j=0;j<14;j++)if(b.Frame[mu,j]!=0)
    DeltaBExterior=Sum(DeltaBExterior,Times(P(P(Unit(1<<i,0,1),Unit(1<<j,0,1)),spin),b.Frame[nu,i]*b.Frame[mu,j]));
  }
  MixedProducerStages.Arrays(2);DeltaCurvatureFromConnection=Sum(DeltaBExterior,P(b.B,DeltaB,'C'));
  var frameR=MixedGeometryAdmission.VariationFrame(b.Connection.R,DeltaConnection.R,Shear.G,b.Frame,b.InverseFrame);
  DeltaFAdapted=MixedGeometryAdmission.Lift(MixedGeometryAdmission.Lower(frameR,SpinGeometry.Eta()));
  var lowered=MixedGeometryAdmission.VariationLowered(b.Connection.R,Palatini.R,b.Metric.G,BlockMetric.G,Shear.G,b.Frame);
  DeltaFOracle=MixedGeometryAdmission.Lift(lowered);
  if(MixedTrace.Active is not null&&geometryLeaf is null)throw new InvalidOperationException("traced metric germ requires independently bound geometry leaves");
  geometryLeaf?.Invoke("deltaCurvatureAdapted",2,DeltaFAdapted);geometryLeaf?.Invoke("deltaCurvatureOracle",2,DeltaFOracle);
  MixedProducerStages.Arrays(2);DeltaFFixed=Sum(DeltaFAdapted,Times(Mixed.Motion(Motion,b.F),-1));
 }
}
