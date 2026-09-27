using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Closed, full-content traversal. Shapes are checked BEFORE indexing and
// against MixedAuditPlan's independent child menu; no reflection/summary path.
internal static class MixedStructuredFields
{
 static readonly int[] Degrees=[2,12,13,14,0,1,13,1];
 public sealed record Visitor(Action<string,int,FT> Tensor,Action<string,Rational[]> Array,Action<string,Rational> Scalar,Action<string,bool> Boolean);
 static void Length<T>(T[] array,int expected){if(array is null||array.Length!=expected)throw new InvalidOperationException("structured evidence exact array length");}
 static void Bi(string name,int degree,BiTensor t,Visitor visit)
 {visit.Tensor(name+"/Value",degree,t.Value);visit.Tensor(name+"/H",degree,t.H);visit.Tensor(name+"/U",degree,t.U);visit.Tensor(name+"/HU",degree,t.HU);}
 static void Stages(string name,BiTensor[][] stages,Visitor visit)
 {Length(stages,3);for(int p=0;p<3;p++){Length(stages[p],8);for(int s=0;s<8;s++)Bi($"{name}/{p}/{s}",Degrees[s],stages[p][s],visit);}}
 static void Array(string name,Rational[] values,int length,Visitor visit){Length(values,length);visit.Array(name,values);}
 static void Density(string name,WardDensity density,Visitor visit)
 {
  Stages(name+"/Stages",density.Stages,visit);Length(density.TopForms,4);for(int p=0;p<4;p++)Bi($"{name}/TopForms/{p}",14,density.TopForms[p],visit);
  Array(name+"/Value",density.Value,4,visit);Array(name+"/Metric",density.Metric,4,visit);Array(name+"/Field",density.Field,4,visit);Array(name+"/Mixed",density.Mixed,4,visit);
 }
 public static void Original(OriginalMixedResult result,Visitor visit)
 {Stages("Stages",result.Stages,visit);Array("Value",result.Value,4,visit);Array("Metric",result.Metric,4,visit);Array("Field",result.Field,4,visit);Array("Mixed",result.Mixed,4,visit);}
 public static void Ward(WardActionResult result,Visitor visit)
 {
  visit.Boolean("Compensated",result.Compensated);visit.Boolean("Oracle",result.Oracle);visit.Tensor("Eta",0,result.Eta);visit.Tensor("NativeDeta",1,result.NativeDeta);visit.Tensor("Deta",1,result.Deta);visit.Tensor("DBeta",1,result.DBeta);visit.Tensor("Tangent",1,result.Tangent);visit.Tensor("DBTangent",2,result.DBTangent);
  string[] names=["epsilon","inverse","epsilonInverse","inverseEpsilon","referenceCurvature","DBepsilon","DBinverse","DBSquaredEpsilon","DBSquaredEpsilonExpanded","curvatureVariationCommutator","gaugeDifference","DBgaugeDifference","varpi","DBvarpi","T","DBT","rotatedCurvature","covariantT","quadraticT","unrotatedPhiFirst","unrotatedPhiOuter","unrotatedPhiInner","phiFirst","phiOuter","phiInner","descendedT","descendedDerivative","descendedDerivativeOracle","descendedCurvature","descendedQuadratic","descendedPhiFirst","descendedPhiOuter","descendedPhiInner"];
  int[] degrees=[0,0,0,0,2,1,1,2,2,2,1,2,1,2,1,2,2,2,2,1,1,2,1,1,2,1,2,2,2,2,1,1,2];
  Length(result.Primitives,33);for(int p=0;p<33;p++){if(result.Primitives[p].Name!=names[p])throw new InvalidOperationException("fixed primitive name/order");Bi("Primitives/"+names[p],degrees[p],result.Primitives[p].Tensor,visit);}
  Density("Literal",result.Literal,visit);Density("Descended",result.Descended,visit);
 }
 public static void Acceleration(WardAccelerationResult result,Visitor visit)
 {
  visit.Boolean("Oracle",result.Oracle);visit.Tensor("Eta",0,result.Eta);visit.Tensor("NativeDeta",1,result.NativeDeta);visit.Tensor("W",1,result.W);visit.Tensor("Dw",2,result.Dw);visit.Tensor("DBw",2,result.DBw);visit.Tensor("DwFromJet",2,result.DwFromJet);visit.Tensor("DBwFromJet",2,result.DBwFromJet);visit.Tensor("CrossQ",2,result.CrossQ);
  var arrays=new[]{result.PartialW,result.CovariantW,result.DeltaBPartialInFrame,result.CovariantAdjointX};string[] names=["PartialW","CovariantW","DeltaBPartialInFrame","CovariantAdjointX"];
  for(int a=0;a<4;a++){Length(arrays[a],14);for(int mu=0;mu<14;mu++)visit.Tensor(names[a]+"/"+mu,a==3?2:1,arrays[a][mu]);}
  Length(result.Stages,2);for(int p=0;p<2;p++){Length(result.Stages[p],8);for(int s=0;s<8;s++)visit.Tensor($"Stages/{p}/{s}",Degrees[s],result.Stages[p][s]);}
  Array("Original",result.Original,4,visit);Array("Euler",result.Euler,4,visit);Array("Current",result.Current,14,visit);Array("CoordinateCurrent",result.CoordinateCurrent,14,visit);
  Length(result.CurrentCovariantDerivative,14);for(int mu=0;mu<14;mu++)Array("CurrentCovariantDerivative/"+mu,result.CurrentCovariantDerivative[mu],14,visit);visit.Scalar("Divergence",result.Divergence);
 }
}
