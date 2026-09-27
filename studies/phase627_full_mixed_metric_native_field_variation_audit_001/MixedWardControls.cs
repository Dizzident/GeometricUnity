using static Fourier;
using static Mixed;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Finite diagnostics supplement the arbitrary-eta structural proof in
// MixedWard. They are never advertised as an enumeration of gauge parameters.
internal interface IMixedWardEvidence
{
 void Check(string name,bool passed);
 void Ward(string name,WardActionResult result);
 void Acceleration(string name,WardAccelerationResult result);
 void Original(string name,OriginalMixedResult result);
 void SecondJet(string name,FT value);
}

internal static class MixedWardControls
{
 public static bool InMenu(MixedMetricGerm g)=>g.MetricBasis==0&&
  (g.Multiindex.SequenceEqual(new[]{1,0,0,0})||g.Multiindex.SequenceEqual(new[]{0,2,0,0})||g.Multiindex.SequenceEqual(new[]{0,3,0,0}));
 static bool Same(BiTensor a,BiTensor b)=>Mixed.Equal(a.Value,b.Value)&&Mixed.Equal(a.H,b.H)&&Mixed.Equal(a.U,b.U)&&Mixed.Equal(a.HU,b.HU);
 static bool Same(Rational[] a,Rational[] b)=>a.SequenceEqual(b);
 static WardPrimitive Primitive(WardActionResult result,string name)=>result.Primitives.Single(p=>p.Name==name);
 public static void Evaluate(MixedBackground b,MixedMetricGerm g,IMixedWardEvidence evidence)
 {
  if(!InMenu(g))throw new ArgumentException("outside frozen twelve-context Ward menu");
  FT[] parameters=[Unit(0,1<<4,1),Unit(0,3,1)];var nativeDeta=Unit(1,1<<1,1);
  for(int parameter=0;parameter<2;parameter++)
  {
   var eta=parameters[parameter];string prefix=$"p{b.Point}_j{g.JetIndex}_eta{parameter}";
   WardActionResult? literalEpsilon=null,literalCompensated=null;WardAccelerationResult? literalAcceleration=null;OriginalMixedResult? literalField=null;
   for(int route=0;route<2;route++)
   {
    bool oracle=route==1;string name=prefix+"_route"+route;
    var epsilon=MixedWard.Evaluate(b,g,eta,nativeDeta,false,oracle);
    var compensated=MixedWard.Evaluate(b,g,eta,nativeDeta,true,oracle);
    var acceleration=MixedWard.Acceleration(b,g,eta,nativeDeta,oracle);
    evidence.Ward(name+"_epsilon",epsilon);evidence.Ward(name+"_compensated",compensated);evidence.Acceleration(name,acceleration);
    var nativeTangent=Pullback(b.InverseFrame,compensated.Tangent,oracle);
    var nativeDTangent=Pullback(b.InverseFrame,Sum(compensated.DBTangent,Times(P(b.B,compensated.Tangent,'C',oracle),-1)),oracle);
    var field=OriginalMixedAction.Evaluate(b,g,nativeTangent,nativeDTangent,oracle);evidence.Original(name+"_fixedNativeTangent",field);
    foreach(var row in new[]{epsilon,compensated})
    {
     string tag=name+(row.Compensated?"_compensated":"_epsilon");
     evidence.Check(tag+"_densityValue",Same(row.Literal.Value,row.Descended.Value));
     evidence.Check(tag+"_densityMetric",Same(row.Literal.Metric,row.Descended.Metric));
     evidence.Check(tag+"_densityField",Same(row.Literal.Field,row.Descended.Field));
     evidence.Check(tag+"_densityMixed",Same(row.Literal.Mixed,row.Descended.Mixed));
     evidence.Check(tag+"_descendedDerivative",Same(Primitive(row,"descendedDerivative").Tensor,Primitive(row,"descendedDerivativeOracle").Tensor));
     evidence.Check(tag+"_curvatureSquare",Same(Primitive(row,"DBSquaredEpsilon").Tensor,Primitive(row,"DBSquaredEpsilonExpanded").Tensor));
     evidence.Check(tag+"_curvatureVariation",Mixed.Equal(Primitive(row,"DBSquaredEpsilon").Tensor.HU,Primitive(row,"curvatureVariationCommutator").Tensor.HU));
     foreach(string suffix in new[]{"First","Outer","Inner"})evidence.Check(tag+"_solder"+suffix,Same(Primitive(row,"descendedPhi"+suffix).Tensor,Primitive(row,"unrotatedPhi"+suffix).Tensor));
    }
    evidence.Check(name+"_accelerationExteriorJet",Mixed.Equal(acceleration.Dw,acceleration.DwFromJet)&&Mixed.Equal(acceleration.DBw,acceleration.DBwFromJet));
    for(int piece=0;piece<4;piece++)
    {
     evidence.Check(name+"_compensatedWard_"+piece,compensated.Literal.Mixed[piece]+acceleration.Original[piece]==0);
     evidence.Check(name+"_epsilonMixedWard_"+piece,epsilon.Literal.Mixed[piece]+field.Mixed[piece]+acceleration.Original[piece]==0);
     evidence.Check(name+"_accelerationGreen_"+piece,acceleration.Original[piece]==acceleration.Euler[piece]+(piece==1?acceleration.Divergence:(Rational)0));
    }
    if(route==0){literalEpsilon=epsilon;literalCompensated=compensated;literalAcceleration=acceleration;literalField=field;}
    else
    {
     foreach(var pair in new[]{(literalEpsilon!,epsilon),(literalCompensated!,compensated)})
     {
      evidence.Check(name+"_primitiveMenu"+(pair.Item2.Compensated?"C":"E"),pair.Item1.Primitives.Select(p=>p.Name).SequenceEqual(pair.Item2.Primitives.Select(p=>p.Name)));
      for(int p=0;p<pair.Item1.Primitives.Length;p++)evidence.Check(name+"_wordPrimitive_"+(pair.Item2.Compensated?"C":"E")+p,Same(pair.Item1.Primitives[p].Tensor,pair.Item2.Primitives[p].Tensor));
      string tag=name+"_wordDensity_"+(pair.Item2.Compensated?"C":"E");
      var firstDensities=new[]{pair.Item1.Literal,pair.Item1.Descended};var secondDensities=new[]{pair.Item2.Literal,pair.Item2.Descended};
      for(int density=0;density<2;density++)
      {
       var first=firstDensities[density];var second=secondDensities[density];
       for(int piece=0;piece<3;piece++)for(int stage=0;stage<8;stage++)evidence.Check($"{tag}_{density}_p{piece}_s{stage}",Same(first.Stages[piece][stage],second.Stages[piece][stage]));
       for(int piece=0;piece<4;piece++)evidence.Check($"{tag}_{density}_top{piece}",Same(first.TopForms[piece],second.TopForms[piece]));
       evidence.Check($"{tag}_{density}_scalars",Same(first.Value,second.Value)&&Same(first.Metric,second.Metric)&&Same(first.Field,second.Field)&&Same(first.Mixed,second.Mixed));
      }
     }
     for(int piece=0;piece<3;piece++)for(int stage=0;stage<8;stage++)evidence.Check($"{name}_wordField_p{piece}_s{stage}",Same(literalField!.Stages[piece][stage],field.Stages[piece][stage]));
     evidence.Check(name+"_wordFieldScalars",Same(literalField!.Value,field.Value)&&Same(literalField.Metric,field.Metric)&&Same(literalField.Field,field.Field)&&Same(literalField.Mixed,field.Mixed));
     evidence.Check(name+"_wordAcceleration",Same(literalAcceleration!.Original,acceleration.Original)&&Same(literalAcceleration.Euler,acceleration.Euler)&&Same(literalAcceleration.Current,acceleration.Current)&&literalAcceleration.Divergence==acceleration.Divergence);
     evidence.Check(name+"_wordAccelerationTensors",Mixed.Equal(literalAcceleration.W,acceleration.W)&&Mixed.Equal(literalAcceleration.Dw,acceleration.Dw)&&Mixed.Equal(literalAcceleration.DBw,acceleration.DBw)&&Mixed.Equal(literalAcceleration.CrossQ,acceleration.CrossQ));
     for(int mu=0;mu<14;mu++)evidence.Check($"{name}_wordAccelerationJet{mu}",Mixed.Equal(literalAcceleration.PartialW[mu],acceleration.PartialW[mu])&&Mixed.Equal(literalAcceleration.CovariantW[mu],acceleration.CovariantW[mu])&&Mixed.Equal(literalAcceleration.DeltaBPartialInFrame[mu],acceleration.DeltaBPartialInFrame[mu])&&Mixed.Equal(literalAcceleration.CovariantAdjointX[mu],acceleration.CovariantAdjointX[mu])&&Same(literalAcceleration.CurrentCovariantDerivative[mu],acceleration.CurrentCovariantDerivative[mu]));
     for(int piece=0;piece<2;piece++)for(int stage=0;stage<8;stage++)evidence.Check($"{name}_wordAcceleration_p{piece}_s{stage}",Mixed.Equal(literalAcceleration.Stages[piece][stage],acceleration.Stages[piece][stage]));
    }
   }
  }
 }
 public static void SymmetricSecondJets(IMixedWardEvidence evidence)
 {
  // Two fixed noncentral basis controls. Universality is the separately bound
  // antisymmetry proof, not inferred from these210 tensor examples.
  FT[] coefficients=[Unit(0,1<<4,1),Unit(0,3,1)];
  for(int c=0;c<2;c++)for(int first=0;first<14;first++)for(int second=first;second<14;second++)for(int route=0;route<2;route++)
  {
   string name=$"eta{c}_second_{first}_{second}_route{route}";var value=MixedWard.SymmetricSecondJetImage(first,second,coefficients[c],route==1);evidence.SecondJet(name,value);evidence.Check(name,value.Count==0);
  }
 }
}
