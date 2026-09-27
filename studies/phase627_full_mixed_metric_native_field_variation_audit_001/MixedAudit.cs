using static Algebra;
using static Fourier;
using static Mixed;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Uncalled prospective orchestration. This is NOT an entry authorization or a
// replacement for the frozen provenance/resource/semantic-replay contract.
// The recorder must open a point/germ session before callbacks that calculate
// tensors and finish it synchronously; retaining an entire 700-row run is not
// permitted by this interface. Each callback's menu is data-independent.
internal interface IMixedAuditEvidence:IMixedWardEvidence
{
 void BeginPoint(int point,FT certifiedPolynomial);
 void Background(MixedBackground background);
 // Complete numerical background replay precedes all child germs; this does
 // not complete the point traversal or waive its350 children and EndPoint.
 void SealPointBackground();
 void BeginGerm(MixedBackground background,int metricBasis,int jetIndex);
 void Geometry(MixedBackground background,MixedMetricGerm germ);
 void GeometryLeaf(string name,int degree,FT value);
 void Tensor(string name,int degree,FT value,bool expanded);
 void Error(MixedError error);
 void EndGerm();
 void EndPoint();
}

internal sealed record MixedAuditCensus(int Points,int Germs,int[] GermOrders,
 int JetSlotIdentities,int DiagonalNullControls,int SymmetricNullControls,
 int ThirdGermLowerPieceChecks,int PureDiffeomorphismControls);

internal static class MixedAudit
{
 static bool Equal(FT a,FT b)=>Mixed.Equal(a,b);
 public static readonly string[] Pieces=["source","kinetic","cubic","mass"];
 static int IntermediateDegree(string name)
 {
  if(name.StartsWith("fixed_p",StringComparison.Ordinal)){int start=name.IndexOf("_s",StringComparison.Ordinal)+2;int end=name.IndexOf('_',start);return new[]{2,12,13,14,0,1,13,1}[int.Parse(name[start..end])];}
  if(name.StartsWith("reverse_s",StringComparison.Ordinal)){int end=name.IndexOf('_',9);return new[]{1,13,2,1,0,14,12,2,2}[int.Parse(name[9..end])];}
  if(name.StartsWith("covariant",StringComparison.Ordinal)||name.StartsWith("partial_mu",StringComparison.Ordinal))return 2;
  throw new ArgumentException("unknown frozen intermediate name");
 }
 static void Need(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
 static void TensorDomain(FT t,int degree)
 {
  Need(Typed(t,degree)&&HAnti(t)&&t.All(q=>q.Key.Form is >=0 and <16384&&q.Key.Blade is >=0 and <16384&&q.Key.K0==0&&q.Key.K1==0&&!q.Value.IsZero),"complete local real-domain coefficient tensor");
 }
 public static MixedAuditCensus Evaluate(Func<int,FT> certifiedPolynomial,Rational epsilon,IMixedAuditEvidence evidence)
 {
  Need(epsilon.Numerator.Sign>0,"positive certified off-shell error");
  int germs=0,jets=0,diagonal=0,symmetric=0,thirdChecks=0,diffeomorphisms=0;var orders=new int[4];
  var multi=MetricVariation.Multiindices();Need(multi.Length==35&&Geometry.Basis().Length==10,"frozen full metric-jet menu");
  for(int point=0;point<2;point++)
  {
   var x=certifiedPolynomial(point);TensorDomain(x,1);evidence.BeginPoint(point,x);
   var b=new MixedBackground(point,x,evidence.GeometryLeaf);evidence.Background(b);
   for(int mu=0;mu<14;mu++)evidence.Check($"nativeFirstJet_{mu}",Equal(b.NativePartial[mu],b.NativePartialOracle[mu]));
   evidence.Check("covariantExterior",Equal(b.DX,b.DXOracle));
   if(point==0)
   {
    var aX=b.X.GetValueOrDefault((1,1,0,0));
    evidence.Check("nativeFirstJetHandAnchor",b.NativePartial[4].GetValueOrDefault((1,1,0,0))==aX*new Scalar(new Rational(-1,2),0));
    evidence.Check("nativeExteriorHandAnchor",b.NativeExterior.GetValueOrDefault((17,1,0,0))==aX*new Scalar(new Rational(1,2),0));
    evidence.Check("nativeVectorAnchorPositive",aX.Imaginary==0&&(aX.Real-new Rational(181,224*907712)).Numerator.Sign>0);
   }
   evidence.SealPointBackground();
   for(int metric=0;metric<10;metric++)for(int jet=0;jet<35;jet++)
   {
    evidence.BeginGerm(b,metric,jet);var g=new MixedMetricGerm(b,metric,jet,evidence.GeometryLeaf);evidence.Geometry(b,g);germs++;orders[g.Order]++;
    evidence.Check("traceFreeMotion",g.Motion.Trace()==0);
    evidence.Check("metricValueDualBlocks",g.DeltaMetric.G.Same(g.BlockMetric.G));
    for(int mu=0;mu<14;mu++)
    {
     evidence.Check($"metricFirstDualBlocks_{mu}",g.DeltaMetric.D[mu].Same(g.BlockMetric.D[mu]));
     evidence.Check($"connectionDualPalatini_{mu}",g.DeltaConnection.Gamma[mu].Same(g.Palatini.Gamma[mu]));
     for(int nu=0;nu<14;nu++)
     {
      evidence.Check($"metricSecondDualBlocks_{mu}_{nu}",g.DeltaMetric.DD[mu,nu].Same(g.BlockMetric.DD[mu,nu]));
      evidence.Check($"connectionFirstDualPalatini_{mu}_{nu}",g.DeltaConnection.DGamma[mu,nu].Same(g.Palatini.DGamma[mu,nu]));
     }
    }
    evidence.Check("curvatureDualPalatini",g.DeltaConnection.R.Cast<Rational>().SequenceEqual(g.Palatini.R.Cast<Rational>()));
    evidence.Check("spinCurvatureLowering",Equal(g.DeltaFAdapted,g.DeltaFOracle));
    evidence.Check("referenceCurvatureDerivative",Equal(g.DeltaFFixed,g.DeltaCurvatureFromConnection));
    var row=new MixedVariation(b,g,(name,t)=>evidence.Tensor("intermediate_"+name,IntermediateDegree(name),t,false));
    evidence.Check("nonInvariantCovariantDerivative",row.DerivativeIdentityPassed);
    evidence.Check("fixedAdjointIndependent",Equal(row.CAdjointFixed,row.CAdjointOracle));
    evidence.Check("movingAdjointPairingMotion",Equal(row.CAdjointFixed,Sum(row.MovingAdjointDelta,PairingMotion(g.Motion,b.AdjointX))));
    evidence.Check("ordinaryAdjointNativeDivergence",Equal(row.OrdinaryAdjoint,row.NativeAdjointAsFrame));
    void Mark(string name,int degree,FT t){TensorDomain(t,degree);evidence.Tensor(name,degree,t,true);}
    void Family(string name,int degree,FT[] tensors)
    {Need(tensors.Length==4,"four original action pieces");for(int p=0;p<4;p++)Mark(name+"_"+Pieces[p],degree,tensors[p]);Mark(name+"_total",degree,Sum(tensors));}
    Family("raw0",1,row.Raw0);Family("raw2",2,row.Raw2);
    Family("fieldFirst0",1,row.FieldFirst0);Family("fieldFirst2",2,row.FieldFirst2);
    Family("word0",1,row.Oracle0);Family("word2",2,row.Oracle2);
    Family("eulerCovariant",1,row.EulerCovariant);Family("eulerMoving",1,row.EulerMoving);
    Family("native0",1,row.Native0);Family("nativeEuler",1,row.NativeEuler);
    Family("nativeFieldFirst0",1,row.FieldFirst0.Select(t=>MixedVariation.CoordinateDual(b.Frame,t)).ToArray());
    Family("nativeMovingEuler",1,row.EulerMoving.Select(t=>MixedVariation.CoordinateDual(b.Frame,t)).ToArray());
    Mark("native2",2,row.Native2);Mark("nativeFieldFirst2",2,MixedVariation.CoordinateDual(b.Frame,row.FieldFirst2[1]));
    Mark("nativeDivergence",1,row.NativeDivergence);Mark("ordinaryAdjoint",1,row.OrdinaryAdjoint);
    for(int p=0;p<4;p++)
    {
     evidence.Check("rawFieldFirst0_"+Pieces[p],Equal(row.Raw0[p],row.FieldFirst0[p]));
     evidence.Check("rawFieldFirst2_"+Pieces[p],Equal(row.Raw2[p],row.FieldFirst2[p]));
     evidence.Check("rawWord0_"+Pieces[p],Equal(row.Raw0[p],row.Oracle0[p]));
     evidence.Check("rawWord2_"+Pieces[p],Equal(row.Raw2[p],row.Oracle2[p]));
     evidence.Check("eulerMoving_"+Pieces[p],Equal(row.EulerCovariant[p],row.EulerMoving[p]));
     evidence.Check("nativeEuler_"+Pieces[p],Equal(row.NativeEuler[p],MixedVariation.CoordinateDual(b.Frame,row.EulerCovariant[p])));
     if(g.Order==3&&p>0){thirdChecks++;evidence.Check("thirdGermLowerPiece_"+Pieces[p],row.Raw0[p].Count==0&&row.Raw2[p].Count==0&&row.EulerCovariant[p].Count==0);}
    }
    // Current is antisymmetric, its differentiated entries ARE NOT. All196
    // derivatives are named and fully replayed; only current storage quotients.
    var current=new FT();var green=new FT();
    for(int mu=0;mu<14;mu++)for(int nu=0;nu<14;nu++)
    {
     var coefficient=ExteriorComponent(row.Native2,mu,nu);jets++;
     evidence.Check($"nativeJetSlot_{mu}_{nu}",Equal(coefficient,row.CurrentCoefficients[mu,nu]));
     evidence.Check($"greenCurrent_{mu}_{nu}",Equal(row.CurrentCoefficients[mu,nu],row.GreenVariationCoefficients[mu,nu]));
     evidence.Tensor($"currentPartial_{mu}_{nu}",0,row.CurrentPartialCoefficients[mu,nu],false);
     if(mu==nu){diagonal++;evidence.Check($"diagonalNull_{mu}",coefficient.Count==0);}
     if(mu<nu)
     {
      symmetric++;evidence.Check($"symmetricNull_{mu}_{nu}",Sum(row.CurrentCoefficients[mu,nu],row.CurrentCoefficients[nu,mu]).Count==0);
      var form=Unit((1<<mu)|(1<<nu),0,1);current=Sum(current,P(form,row.CurrentCoefficients[mu,nu]));green=Sum(green,P(form,row.GreenVariationCoefficients[mu,nu]));
     }
    }
    Mark("current",2,current);Mark("greenCurrent",2,green);
    evidence.Check("completeCurrent",Equal(current,row.Native2)&&Equal(current,green));
    var error=MixedError.Create(b,g,epsilon);evidence.Error(error);
    if(g.Order==3)evidence.Check("thirdGermZeroBackgroundError",error.Raw0Error==0&&error.Raw2Error==0&&error.EulerError==0);
    if(point==1&&metric==0&&g.Multiindex.SequenceEqual(new[]{0,3,0,0}))
     evidence.Check("thirdSourceHandAnchor",Pair(Pullback(b.Frame,Unit(1<<8,1,1)),row.Raw0[0])==new Rational(3,16));
    if(point==0&&metric==0&&g.Multiindex.SequenceEqual(new[]{1,0,0,0}))
    {
     var nativeProbe=Sum(Unit(1,1,1),Unit(1<<4,1,1));
     evidence.Check("nonzeroMassHandAnchor",Pair(Pullback(b.Frame,nativeProbe),row.Raw0[3])==b.X.GetValueOrDefault((1,1,0,0)).Real*-907712);
    }
    int axis=Array.FindIndex(g.Multiindex,v=>v==3);var pair=Geometry.Pairs[metric];
    if(axis>=0&&(pair.I==axis||pair.J==axis))
    {diffeomorphisms++;evidence.Check("pureBaseDiffeomorphismPrincipalZero",g.DeltaConnection.R.Cast<Rational>().All(v=>v==0)&&g.DeltaFFixed.Count==0&&row.Raw0[0].Count==0&&row.EulerCovariant[0].Count==0);}
    if(MixedWardControls.InMenu(g))MixedWardControls.Evaluate(b,g,evidence);
    evidence.EndGerm();
   }
   evidence.EndPoint();
  }
  Need(germs==700&&orders.SequenceEqual(new[]{20,80,200,400})&&jets==137200&&diagonal==9800&&symmetric==63700&&thirdChecks==1200&&diffeomorphisms==32,"complete frozen germ/control census");
  return new(2,germs,orders,jets,diagonal,symmetric,thirdChecks,diffeomorphisms);
 }
}
