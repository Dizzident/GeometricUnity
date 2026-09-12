using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

// Prospective implementation. The entry gate remains closed until the complete
// proof, fixture, provenance contract and both independent reviews are frozen.
internal static partial class Program
{
 const string Root="studies/phase626_fixed_cubic_full_stationary_residual_certificate_001";
 const string Stem="fixed_cubic_full_stationary_residual_certificate";
 const string ContractId="phase626-a67-fixed-cubic-full-stationary-residual-certificate-v1";
 const int Q=907712;
 static readonly Rational Lambda=new(1,Q),Radius=new(120,Q),Third=new(1,3);
 static readonly Dictionary<string,long> Counts=[];
 static bool geometryPassed=true,operatorPassed=true,polynomialPassed=true,certificatePassed=true,actionPassed=true,domainPassed=true;
 static int largestTensor;
 static void Inc(string key,long amount=1)=>Counts[key]=Counts.GetValueOrDefault(key)+amount;
 static bool Equal(FT x,FT y)=>x.Count==y.Count&&x.All(q=>y.TryGetValue(q.Key,out var z)&&q.Value==z);
 static FT ScaleR(FT x,Rational r)=>Scale(x,new Scalar(r,0));
 static bool Le(Rational x,Rational y)=>(x-y).Numerator.Sign<=0;
 static FT Grade(FT t,int grade)=>t.Where(q=>Degree(q.Key.Blade)==grade).ToDictionary(q=>q.Key,q=>q.Value);
 static void Check(FT t,int degree,bool realType=true)
 {
  Inc("tensorChecks");largestTensor=Math.Max(largestTensor,t.Count);
  domainPassed&=t.Keys.All(k=>k.Form>=0&&k.Form<16384&&k.Blade>=0&&k.Blade<16384&&Degree(k.Form)==degree&&k.K0==0&&k.K1==0)&&t.Values.All(z=>!z.IsZero)&&(!realType||HAnti(t));
  if(t.Count>600000)throw new ResourceLimitException("live single tensor support ceiling");
  CheckResources();
 }
 static void CheckGrades(FT t,params int[] allowed){Inc("gradeEnvelopeChecks");domainPassed&=t.Keys.All(k=>allowed.Contains(Degree(k.Blade)));}
 static Rational PairValue(FT x,FT y){Inc("signedPairings");return Pair(x,y);}
 static string Fingerprint(FT t){Inc("transportFingerprints");return Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(Terms(t)))).ToLowerInvariant();}
 static void ValidateStages(FT[] actual,FT[] oracle)
 {
  // Every retained stage is complete. Grade envelopes are checks, never filters.
  int[] input=actual[0].Keys.Select(k=>Degree(k.Blade)).Distinct().ToArray();
  int[] first=input.Select(g=>g%2==0?g-1:g+1).Where(g=>g>=0&&g<=14).Distinct().ToArray();
  int[] inner=input.SelectMany(g=>new[]{g-2,g+2}).Where(g=>g>=0&&g<=14).Distinct().ToArray();
  int[] outer=inner.Select(g=>g%2==0?g+1:g-1).Where(g=>g>=0&&g<=14).Distinct().ToArray();
  int[][] permitted=[input,input,first,inner,inner,outer,first.Concat(outer).Distinct().ToArray(),first.Concat(outer).Distinct().ToArray()];
  for(int s=0;s<8;s++){Inc("forwardStageEqualities");operatorPassed&=Equal(actual[s],oracle[s]);CheckGrades(actual[s],permitted[s]);CheckGrades(oracle[s],permitted[s]);Check(actual[s],CertificateLayout.StageDegrees[s],false);Check(oracle[s],CertificateLayout.StageDegrees[s],false);}
 }
 static void ValidateKinetic(CertifiedKinetic row)
 {
  Check(row.Input,1);ValidateStages(row.Stages,row.NaiveStages);
  operatorPassed&=Equal(row.Exterior,row.ExteriorOracle)&&Equal(row.Adjoint,row.SimplifiedAdjoint)&&Equal(row.Reverse,row.ReverseOracle)&&Equal(row.Full,ScaleR(Add(row.Stages[7],row.Reverse),new Rational(1,2)));
  Check(row.Exterior,2);Check(row.Adjoint,2);Check(row.Full,1);Check(row.Reverse,1);
  for(int a=0;a<14;a++)
  {
   Inc("covariantDerivativeEqualities");operatorPassed&=Equal(row.Derivatives[a],row.DerivativeOracles[a]);
   Inc("parallelAdjointEqualities",2);operatorPassed&=Equal(row.ReverseDerivatives[a],row.ParallelReverse[a])&&Equal(row.ReverseDerivatives[a],row.ParallelSimplified[a]);
   Check(row.Derivatives[a],1);Check(row.ReverseDerivatives[a],2);
  }
 }
 static (FT K,FT N) FeedbackRow(StreamingEvidence store,string name,FT q,FT naiveQ,FT dq,FT wordDq,object metadata,FT? linearResidual=null,bool evaluatedResidual=false)
 {
  Inc("nonlinearRows");operatorPassed&=Equal(q,naiveQ)&&Equal(dq,wordDq);CheckGrades(q,1,2,5,6,10);Check(q,2);Check(dq,1);
  var stages=Certified.Forward(q);var naive=Certified.Forward(naiveQ,true);ValidateStages(stages,naive);
  var n=ScaleR(Add(stages[7],dq),Third);var oracle=ScaleR(Add(naive[7],wordDq),Third);CheckGrades(n,name.EndsWith("_central",StringComparison.Ordinal)?[3,4,8]:[1,2,5,6,9,13]);operatorPassed&=Equal(n,oracle);Check(n,1);
  CertificateLayout.WriteStages(store,name,stages,naive,metadata);
  store.WriteCollection(name+"_transpose",new Dictionary<string,FT>{{"actual",dq},{"word",wordDq}},metadata);
  if(linearResidual is null)store.WriteCollection(name+"_result",new Dictionary<string,FT>{{"nonlinear",n},{"oracle",oracle}},metadata);
  else
  {
   var residual=Add(linearResidual,n);Check(residual,1);
   store.WriteCollection(name+"_result",new Dictionary<string,FT>{{"nonlinear",n},{"residual",residual},{"scaledDefect",evaluatedResidual?ScaleR(residual,Lambda):residual}},metadata);
  }
  return(stages[7],n);
 }
 static (FT K,FT N) Self(StreamingEvidence store,string name,FT x,FT adjoint,object metadata,FT? linearResidual=null)
 {
  Inc("selfProducts");var q=Product(x,x);var naive=Certified.Naive(x,x);
  Inc("selfTransposes");var dq=Certified.Transpose(x,adjoint,'C');var word=Certified.WordTranspose(x,adjoint);
  return FeedbackRow(store,name,q,naive,dq,word,metadata,linearResidual,linearResidual is not null);
 }
 static (FT K,FT N) Cross(StreamingEvidence store,string name,FT x,FT y,FT ax,FT ay,object metadata)
 {
  Inc("crossProducts",2);var q=Add(Product(x,y),Product(y,x));var naive=Add(Certified.Naive(x,y),Certified.Naive(y,x));
  Inc("crossTransposes",2);var dq=Add(Certified.Transpose(x,ay,'C'),Certified.Transpose(y,ax,'C'));var word=Add(Certified.WordTranspose(x,ay),Certified.WordTranspose(y,ax));
  return FeedbackRow(store,name,q,naive,dq,word,metadata);
 }
 static Rational[] Current(FT adjoint,FT variation)=>Enumerable.Range(0,14).Select(a=>PairValue(Fifth.Contract(adjoint,a),variation)*Sigma(a)).ToArray();
 static Rational Divergence(Rational[][,] lambda,Rational[] current)
 {Rational value=0;for(int a=0;a<14;a++)for(int b=0;b<14;b++){Inc("greenConnectionSlots");value+=lambda[a][a,b]*current[b];}return value;}
 static object ScientificPoint(int point,JsonElement prior,JsonElement connection,StreamingEvidence store)
 {
  Inc("points");string prefix=CertificateLayout.Prefix(point);
  var lambda=connection.GetProperty("frameNomizu").EnumerateArray().Select(Fifth.Matrix).ToArray();
  if(lambda.Length!=14)throw new ArgumentException("fourteen full connection directions");
  for(int a=0;a<14;a++)for(int i=0;i<14;i++)for(int j=0;j<14;j++)
  {Inc("connectionEntries");geometryPassed&=lambda[a][i,j]*Sigma(i)+lambda[a][j,i]*Sigma(j)==0;if(!CertificateLayout.ActiveDirections.Contains(a))geometryPassed&=lambda[a][i,j]==0;}
  var source=Certified.Read(prior.GetProperty("source"));Check(source,1);Inc("sourceLineageChecks");geometryPassed&=Equal(source,Certified.Read(connection.GetProperty("source")))&&Equal(source,Fifth.Diagonal(new Rational(-21,4),new Rational(-15,4),new Rational(-21,4)))&&Certified.Norm(source)==60;
  var coefficients=Certified.Zeros(6);var coefficientRows=new List<object>();
  for(int n=1;n<=5;n++)
  {
   var selected=prior.GetProperty("coefficientRows").EnumerateArray().Where(r=>r.GetProperty("order").GetInt32()==n).OrderBy(r=>r.GetProperty("gammaPower").GetInt32()).ToArray();
   if(selected.Length!=3||!selected.Select(r=>r.GetProperty("gammaPower").GetInt32()).SequenceEqual(new[]{0,1,2}))throw new ArgumentException("exact gamma coefficient menu");
   foreach(var row in selected){Inc("upstreamCoefficientRows");coefficients[n]=Add(coefficients[n],Certified.Read(row.GetProperty("actual")));}
   var upstreamEvaluation=prior.GetProperty("evaluationRows").EnumerateArray().Single(r=>r.GetProperty("gamma").GetInt32()==1&&r.GetProperty("order").GetInt32()==n);
   Inc("gammaOneReconstructionChecks");polynomialPassed&=Equal(coefficients[n],Certified.Read(upstreamEvaluation.GetProperty("tensor")));Check(coefficients[n],1);
   coefficientRows.Add(new{order=n,norm=Certified.Norm(coefficients[n]).ToString(),terms=coefficients[n].Count});
  }
  var x=Certified.Evaluate(coefficients,Lambda);
  var isotropyRows=new List<object>();var isotropy=connection.GetProperty("isotropyRows").EnumerateArray().ToArray();if(isotropy.Length!=6)throw new ArgumentException("six Lorentz isotropy generators");
  var reflection=Fifth.Matrix(connection.GetProperty("disconnected"));for(int i=0;i<14;i++)for(int j=0;j<14;j++){Inc("reflectionMatrixEntries");geometryPassed&=reflection[i,j]==(i==j?(CertificateLayout.ActiveDirections.Contains(i)?-1:1):0);}
  FT[] invariantFields=[..coefficients.Skip(1),x];
  for(int f=0;f<6;f++)
  {
   for(int g=0;g<6;g++){Inc("isotropyFieldChecks");var generator=Fifth.Matrix(isotropy[g].GetProperty("frameGenerator"));var actual=Fifth.Derivative(generator,invariantFields[f]);var oracle=Fifth.ExteriorSlots(generator,invariantFields[f]);geometryPassed&=actual.Count==0&&Equal(actual,oracle);isotropyRows.Add(new{field=CertificateLayout.Fields[f],generator=g,actual=Terms(actual),oracle=Terms(oracle)});}
   var transformed=new FT();foreach(var row in invariantFields[f]){Rational sign=1;for(int a=0;a<14;a++)if(((row.Key.Form^row.Key.Blade)&(1<<a))!=0)sign*=reflection[a,a];Put(transformed,row.Key,row.Value*new Scalar(sign,0));}Inc("disconnectedFieldChecks");geometryPassed&=Equal(transformed,invariantFields[f]);
  }
  FT[] probes=[Fifth.Diagonal(1,0,0),Fifth.Diagonal(0,1,0),Fifth.Diagonal(0,0,1),Certified.Read(prior.GetProperty("J")),Fifth.B(),Fifth.C(),One(8,157,1),One(1,0,Scalar.I)];
  FT[] fields=[..coefficients.Skip(1),x,..probes];
  int[][] inputGrades=[[1],[2],[1],[2],[1,5],[1,2,5],[1],[1],[1],[2],[2],[2],[5],[0]];
  int[][] responseGrades=[[2],[1],[2],[1],[2,6],[1,2,6],[2],[2],[2],[1],[1],[1],[2,6],[3]];
  int[][] adjointGrades=[[2],[1],[2],[1],[2,6],[1,2,6],[2],[2],[2],[1],[1],[1],[2,6],[3]];
  var cyclicRows=new List<object>();foreach(int f in new[]{1,3,5,9,10,11})
  {
   var cyclic=new FT();foreach(var row in Grade(fields[f],2)){int a=Feedback.Bits(row.Key.Form).Single();if((row.Key.Blade&(1<<a))==0)Put(cyclic,(0,row.Key.Blade|(1<<a),0,0),row.Value*(Sigma(a)*Shuffle(1<<a,row.Key.Blade)));}
   Inc("cyclicBivectorChecks");geometryPassed&=cyclic.Count==0;cyclicRows.Add(new{field=CertificateLayout.Fields[f],cyclic=Terms(cyclic)});
  }
  var h=new FT[14];var forward=new FT[14];var adjoints=new FT[14];
  // Deliberately retain only these small response arrays. The full 70-array
  // kinetic result is serialized before the next field is constructed.
  for(int f=0;f<fields.Length;f++)
  {
   Inc("kineticRows");CheckGrades(fields[f],inputGrades[f]);CertifiedKinetic? kinetic=Certified.Kinetic(lambda,fields[f]);CheckGrades(kinetic.Full,responseGrades[f]);CheckGrades(kinetic.Adjoint,adjointGrades[f]);CheckGrades(kinetic.SimplifiedAdjoint,adjointGrades[f]);CheckGrades(kinetic.Exterior,inputGrades[f]);CheckGrades(kinetic.ExteriorOracle,inputGrades[f]);for(int a=0;a<14;a++){CheckGrades(kinetic.Derivatives[a],inputGrades[f]);CheckGrades(kinetic.DerivativeOracles[a],inputGrades[f]);CheckGrades(kinetic.ReverseDerivatives[a],adjointGrades[f]);CheckGrades(kinetic.ParallelReverse[a],adjointGrades[f]);CheckGrades(kinetic.ParallelSimplified[a],adjointGrades[f]);}ValidateKinetic(kinetic);
   CertificateLayout.WriteKinetic(store,prefix+"_kinetic_"+CertificateLayout.Fields[f],kinetic,new{point,field=CertificateLayout.Fields[f]});
   h[f]=kinetic.Full;forward[f]=kinetic.Stages[7];adjoints[f]=kinetic.Adjoint;kinetic=null;GC.Collect();CheckResources();
  }
  var residuals=Certified.Zeros(11);var nonlinear=Certified.Zeros(11);var polynomialK=Certified.Zeros(11);
  residuals[0]=Add(source,coefficients[1]);residuals[1]=Add(h[0],coefficients[2]);
  store.WriteCollection(prefix+"_lowResidual",new Dictionary<string,FT>{{"G0",residuals[0]},{"G1",residuals[1]},{"defect0",new FT()},{"defect1",residuals[0]},{"defect2",residuals[1]}},new{point,meaning="unscaled coefficient shift: defect[n+1]=G[n]"});
  var polynomialRows=new List<object>();
  for(int n=2;n<=10;n++)
  {
   var q=new FT();var naiveQ=new FT();var dq=new FT();var wordDq=new FT();var pairs=new List<int[]>();
   for(int i=1;i<=5;i++)for(int j=1;j<=5;j++)if(i+j==n)
   {
    Inc("orderedPolynomialProducts");q=Add(q,Product(coefficients[i],coefficients[j]));naiveQ=Add(naiveQ,Certified.Naive(coefficients[i],coefficients[j]));
    Inc("orderedPolynomialTransposes");dq=Add(dq,Certified.Transpose(coefficients[i],adjoints[j-1],'C'));wordDq=Add(wordDq,Certified.WordTranspose(coefficients[i],adjoints[j-1]));pairs.Add([i,j]);
   }
   var linear=n<=5?h[n-1]:new FT();if(n<5)linear=Add(linear,coefficients[n+1]);
   var row=FeedbackRow(store,prefix+"_polynomial"+n,q,naiveQ,dq,wordDq,new{point,residualOrder=n,scaledDefectOrder=n+1,orderedPairs=pairs,meaning="scaledDefect is the coefficient of lambda^(n+1) in lambda G(lambda), hence equals the full G_n coefficient without an extra lambda0 factor"},linear);
   nonlinear[n]=row.N;polynomialK[n]=row.K;residuals[n]=Add(linear,row.N);
   polynomialRows.Add(new{order=n,orderedPairs=pairs,norm=Certified.Norm(residuals[n]).ToString(),terms=residuals[n].Count,grades=residuals[n].Keys.Select(k=>Degree(k.Blade)).Distinct().Order()});
  }
  int[][] residualGrades=[[],[],[],[],[],[2,6],[1,5,9],[2,6],[1,5,9],[2,6],[1,5,9,13]];
  for(int n=0;n<=10;n++){Inc("residualCoefficients");CheckGrades(residuals[n],residualGrades[n]);if(n<5){Inc("zeroResidualCoefficients");polynomialPassed&=residuals[n].Count==0;}Check(residuals[n],1);}
  var directLinear=Add(source,Add(h[5],ScaleR(x,Q)));var direct=Self(store,prefix+"_direct",x,adjoints[5],new{point,lambda=Lambda.ToString(),kappa=Q,meaning="direct evaluated residual G(X); scaledDefect is lambda0 G(X), not a polynomial coefficient"},directLinear);
  var directResidual=Add(directLinear,direct.N);var evaluated=Certified.Evaluate(residuals,Lambda);var evaluatedNonlinear=Certified.Evaluate(nonlinear,Lambda);var evaluatedK=Certified.Evaluate(polynomialK,Lambda);var evaluatedH=Certified.Evaluate([new FT(),..h.Take(5)],Lambda);
  CheckGrades(directResidual,1,2,5,6,9,13);
  Inc("directEvaluationEqualities",4);polynomialPassed&=Equal(directResidual,evaluated)&&Equal(direct.N,evaluatedNonlinear)&&Equal(direct.K,evaluatedK)&&Equal(h[5],evaluatedH);
  var defectCoefficients=Certified.Zeros(12);for(int n=0;n<=10;n++){Inc("defectCoefficientShifts");defectCoefficients[n+1]=residuals[n];}
  var defect=ScaleR(directResidual,Lambda);Inc("directDefectEquality");polynomialPassed&=Equal(defect,Certified.Evaluate(defectCoefficients,Lambda));
  Rational normX=Certified.Norm(x),normG=Certified.Norm(directResidual),normDefect=Certified.Norm(defect),lipschitz=Lambda*51520+2*Lambda*2576*Radius,selfMap=Lambda*(60+51520*Radius+2576*Radius*Radius),denominator=1-lipschitz;
  certificatePassed&=denominator.Numerator.Sign>0;if(denominator.Numerator.Sign<=0)throw new ArgumentException("nonpositive contraction denominator before reciprocal");
  Rational error=Lambda*normG*new Rational(denominator.Denominator,denominator.Numerator),taylor=Radius*new Rational(1,768);
  Inc("certificateRows");certificatePassed&=Le(normX,Radius)&&Le(selfMap,Radius*new Rational(7,8))&&Le(lipschitz,new Rational(1,2))&&denominator.Numerator.Sign>0&&normDefect==Lambda*normG&&Le(normDefect,Radius*new Rational(1,512))&&error.Numerator.Sign>=0;
  // This planted invalid truncation is computed only AFTER the complete G.
  var missing=Grade(coefficients[5],5);var truncatedG4=Add(residuals[4],ScaleR(missing,-1));Inc("gradeFiveTruncationDecoys");polynomialPassed&=missing.Count==600&&truncatedG4.Count==600&&truncatedG4.GetValueOrDefault((8,157,0,0)).Real==new Rational(3,4);
  var selfRows=new (FT K,FT N)[8];for(int v=0;v<8;v++)selfRows[v]=Self(store,prefix+"_self_"+CertificateLayout.Fields[v+6],probes[v],adjoints[v+6],new{point,probe=CertificateLayout.Fields[v+6]});
  Rational[] expectedNorms=[-4,-9,-1,9,-1,-9,1,1];var actionRows=new List<object>();
  for(int v=0;v<8;v++)
  {
   var probe=probes[v];var cross=Cross(store,prefix+"_cross_"+CertificateLayout.Fields[v+6],x,probe,adjoints[5],adjoints[v+6],new{point,probe=CertificateLayout.Fields[v+6]});
   Rational[] original=[Third*PairValue(x,direct.K),Third*(PairValue(probe,direct.K)+PairValue(x,cross.K)),Third*(PairValue(probe,cross.K)+PairValue(x,selfRows[v].K)),Third*PairValue(probe,selfRows[v].K)];
   var gradients=new[]{PairValue(probe,direct.N),PairValue(probe,cross.N),PairValue(probe,selfRows[v].N)};
   for(int z=0;z<4;z++)Inc("originalCubicCoefficients");for(int z=0;z<3;z++){Inc("originalCubicDerivativeEqualities");actionPassed&=(z+1)*original[z+1]==gradients[z];}
   Inc("probeNormControls");var norm=PairValue(probe,probe);actionPassed&=norm==expectedNorms[v];Inc("cubicThirdOrderControls");actionPassed&=original[3]==(v==0?-16:v==1?-336:0);
   Rational sourceFirst=PairValue(probe,source),kineticFirst=(PairValue(probe,forward[5])+PairValue(x,forward[v+6]))*new Rational(1,2),massFirst=Q*PairValue(probe,x);var current=Current(adjoints[5],probe);var div=Divergence(lambda,current);var local=sourceFirst+kineticFirst+original[1]+massFirst;var euler=PairValue(probe,directResidual);Inc("originalFirstVariationGreenEqualities");actionPassed&=local==euler+div*new Rational(1,2);
   actionRows.Add(new{probe=CertificateLayout.Fields[v+6],signedNorm=norm.ToString(),originalCubic=original.Select(z=>z.ToString()),derivativePairings=gradients.Select(z=>z.ToString()),source=sourceFirst.ToString(),kinetic=kineticFirst.ToString(),cubic=original[1].ToString(),mass=massFirst.ToString(),localTotal=local.ToString(),eulerPairing=euler.ToString(),current=current.Select(z=>z.ToString()),divergence=div.ToString()});
  }
  var offCross=Cross(store,prefix+"_offRoot",probes[4],probes[0],adjoints[10],adjoints[6],new{point,field="B",probe="PHGamma",gamma=1,kappa=0});
  var offGradient=Add(source,Add(h[10],selfRows[4].N));var offSource=PairValue(probes[0],source);var offKinetic=(PairValue(probes[0],forward[10])+PairValue(probes[4],forward[6]))*new Rational(1,2);var offCubic=Third*(PairValue(probes[0],selfRows[4].K)+PairValue(probes[4],offCross.K));var offEuler=PairValue(probes[0],offGradient);var offCurrent=Current(adjoints[10],probes[0]);var offDiv=Divergence(lambda,offCurrent);Rational[] restricted=[PairValue(probes[4],source),PairValue(probes[4],forward[10])*new Rational(1,2),PairValue(probes[4],selfRows[4].K)*Third];
  Inc("offRootDecoys");actionPassed&=offSource==21&&offKinetic==5&&offCubic==-4&&offSource+offKinetic+offCubic==22&&offEuler==20&&offDiv==4&&restricted.All(z=>z==0)&&offGradient.Count!=0;
  var transport=new Dictionary<string,string>();for(int f=0;f<14;f++){transport.Add("field_"+CertificateLayout.Fields[f],Fingerprint(fields[f]));transport.Add("H_"+CertificateLayout.Fields[f],Fingerprint(h[f]));}for(int n=2;n<=10;n++){transport.Add("KQ_"+n,Fingerprint(polynomialK[n]));transport.Add("N_"+n,Fingerprint(nonlinear[n]));}for(int n=0;n<=10;n++)transport.Add("G_"+n,Fingerprint(residuals[n]));transport.Add("directKQ",Fingerprint(direct.K));transport.Add("directN",Fingerprint(direct.N));transport.Add("directG",Fingerprint(directResidual));
  return new{schemaVersion=1,phase=626,point,gamma=1,kappa=Q,lambda=Lambda.ToString(),frame=connection.GetProperty("frame").Clone(),source=Terms(source),isotropyRows,cyclicRows,transportFingerprints=transport,coefficientRows,polynomialRows,certificate=new{radius=Radius.ToString(),candidateNorm=normX.ToString(),residualNorm=normG.ToString(),defectNorm=normDefect.ToString(),selfMapBound=selfMap.ToString(),lipschitz=lipschitz.ToString(),posterioriError=error.ToString(),taylorError=taylor.ToString(),certifiedError=(Le(error,taylor)?error:taylor).ToString()},actionRows,offRoot=new{source=offSource.ToString(),kinetic=offKinetic.ToString(),cubic=offCubic.ToString(),localTotal=(offSource+offKinetic+offCubic).ToString(),eulerPairing=offEuler.ToString(),current=offCurrent.Select(z=>z.ToString()),divergence=offDiv.ToString(),restricted=restricted.Select(z=>z.ToString())},truncationDecoy=new{omittedGrade=5,terms=truncatedG4.Count,witness=truncatedG4.GetValueOrDefault((8,157,0,0)).Real.ToString()}};
 }
 const string FixtureJson="""
{
 "dimension": 14,
 "signature": [
  1,
  1,
  1,
  1,
  1,
  1,
  1,
  -1,
  -1,
  -1,
  -1,
  -1,
  -1,
  -1
 ],
 "points": [
  0,
  1
 ],
 "fullDiracModuleDimension": 128,
 "chiralHalfDimension": 64,
 "normalizedTraceDenominator": 128,
 "domain": "full real u(64,64), all16384 Clifford blades and229376 one-form coordinates; no final projection",
 "operator": "canonical untied CAA: firstC outerA innerA; Phi1=Gamma1 Phi2=Gamma2",
 "coreFileCount": 726,
 "exactTolerance": 0,
 "bindingCount": 67,
 "compiledFileCount": 9,
 "parameters": {
  "gamma": 1,
  "kappa": 907712,
  "lambda": "1/907712",
  "analyticDisk": "1/226928",
  "sourceNorm": 60,
  "hBound": 51520,
  "nonlinearBound": 2576,
  "radius": "15/113464",
  "sourceNormalization": "conditional source-fixed cubic coefficient; displayed kappa1/2 versus expanded kappa1 ambiguity remains"
 },
 "fields": [
  "S1",
  "S2",
  "S3",
  "S4",
  "S5",
  "X",
  "PHGamma",
  "PTGamma",
  "PtrGamma",
  "J",
  "B",
  "C",
  "W5",
  "central"
 ],
 "probeNorms": [
  "-4",
  "-9",
  "-1",
  "9",
  "-1",
  "-9",
  "1",
  "1"
 ],
 "cubicZ3": [
  "-16",
  "-336",
  "0",
  "0",
  "0",
  "0",
  "0",
  "0"
 ],
 "residualOrders": [
  0,
  1,
  2,
  3,
  4,
  5,
  6,
  7,
  8,
  9,
  10
 ],
 "scaledDefectOrders": [
  0,
  1,
  2,
  3,
  4,
  5,
  6,
  7,
  8,
  9,
  10,
  11
 ],
 "zeroResidualOrders": [
  0,
  1,
  2,
  3,
  4
 ],
 "residualGrades": [
  [],
  [],
  [],
  [],
  [],
  [
   2,
   6
  ],
  [
   1,
   5,
   9
  ],
  [
   2,
   6
  ],
  [
   1,
   5,
   9
  ],
  [
   2,
   6
  ],
  [
   1,
   5,
   9,
   13
  ]
 ],
 "negativeControls": {
  "truncatedG4Terms": 600,
  "truncatedG4Form": 8,
  "truncatedG4Blade": 157,
  "truncatedG4Witness": "3/4",
  "offRoot": {
   "source": "21",
   "kinetic": "5",
   "cubic": "-4",
   "local": "22",
   "euler": "20",
   "divergence": "4",
   "restricted": [
    "0",
    "0",
    "0"
   ]
  }
 },
 "expectedCounts": {
  "arithmeticControls": 4,
  "wordCases": 507904,
  "hodgeCases": 16384,
  "fullRealDomainMasks": 16384,
  "centralDomainControls": 2,
  "tensorChecks": 2452,
  "gradeEnvelopeChecks": 3572,
  "signedPairings": 510,
  "transportFingerprints": 120,
  "forwardStageEqualities": 656,
  "covariantDerivativeEqualities": 392,
  "parallelAdjointEqualities": 784,
  "nonlinearRows": 54,
  "selfProducts": 18,
  "selfTransposes": 18,
  "crossProducts": 36,
  "crossTransposes": 36,
  "greenConnectionSlots": 3528,
  "points": 2,
  "connectionEntries": 5488,
  "sourceLineageChecks": 2,
  "upstreamCoefficientRows": 30,
  "gammaOneReconstructionChecks": 10,
  "reflectionMatrixEntries": 392,
  "isotropyFieldChecks": 72,
  "disconnectedFieldChecks": 12,
  "cyclicBivectorChecks": 12,
  "kineticRows": 28,
  "orderedPolynomialProducts": 50,
  "orderedPolynomialTransposes": 50,
  "residualCoefficients": 22,
  "zeroResidualCoefficients": 10,
  "directEvaluationEqualities": 8,
  "defectCoefficientShifts": 22,
  "directDefectEquality": 2,
  "certificateRows": 2,
  "gradeFiveTruncationDecoys": 2,
  "originalCubicCoefficients": 64,
  "originalCubicDerivativeEqualities": 48,
  "probeNormControls": 16,
  "cubicThirdOrderControls": 16,
  "originalFirstVariationGreenEqualities": 16,
  "offRootDecoys": 2,
  "pointContexts": 2,
  "transportComparisons": 60,
  "evidenceChunks": 3914
 },
 "resourceCounts": {
  "kineticCalls": 28,
  "derivativeSlots": 392,
  "forwardCalls": 164,
  "adjointCalls": 420,
  "transposeCalls": 2624,
  "wordTransposeCalls": 104,
  "naiveProductCalls": 350
 },
 "resources": {
  "maximumCoefficientProducts": 500000000,
  "maximumSlotProducts": 200000000,
  "maximumTransposePairVisits": 300000000,
  "maximumWordTransposePairVisits": 250000000,
  "maximumNaiveProductPairVisits": 300000000,
  "maximumTensorSupport": 600000,
  "maximumTensorRationalCharacters": 256,
  "maximumScalarRationalCharacters": 512,
  "maximumMeasuredManagedBytes": 8589934592
 },
 "storage": {
  "schema": "expanded-rational-tensor-chunks-v1",
  "planBinding": "chunk-plan",
  "chunks": 3914,
  "pointContexts": [
   "studies/phase626_fixed_cubic_full_stationary_residual_certificate_001/output/point0_context.json",
   "studies/phase626_fixed_cubic_full_stationary_residual_certificate_001/output/point1_context.json"
  ],
  "maximumRecordsPerChunk": 100000,
  "maximumMetadataBytes": 1048576,
  "maximumChunkBytes": 67108864,
  "maximumAggregateChunkBytes": 2147483648,
  "maximumContextBytes": 1048576,
  "maximumManifestBytes": 16777216,
  "aggregateGuardGuaranteesSuccess": false
 },
 "scope": {
  "sourcePreferredOperatorEstablished": false,
  "sourceKappaNormalizationResolved": false,
  "physicalCouplingsSelected": false,
  "physicalUnitsSelected": false,
  "fullUnrestrictedPdeConvergenceProved": false,
  "globalSpinBundleEstablished": false,
  "naturalFibreBoundaryConditionsSelected": false,
  "metricEpsilonMixedHessianComputed": false,
  "physicalHessianConstructed": false,
  "propagatorPolesComputed": false,
  "bosonMassesPredicted": false,
  "externalReviewReplaced": false,
  "centralDirectionRemoved": false,
  "higherCliffordGradesDiscarded": false,
  "pointwiseKineticSelfAdjointnessAssumed": false,
  "diagnosticAlgebraicGammaUsed": false
 }
}
""";
 static readonly Dictionary<string,string> Paths=new(StringComparer.Ordinal)
 {
  ["program"]="studies/phase626_fixed_cubic_full_stationary_residual_certificate_001/Program.cs",
  ["project"]="studies/phase626_fixed_cubic_full_stationary_residual_certificate_001/Phase626FixedCubicFullStationaryResidualCertificate.csproj",
  ["study"]="studies/phase626_fixed_cubic_full_stationary_residual_certificate_001/STUDY.md",
  ["phase600-program"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/Program.cs",
  ["phase600-project"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/Phase600FullTraceAdjointPeriodicGradientNormAudit.csproj",
  ["phase600-study"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/STUDY.md",
  ["phase600-contract"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/preregistration/contract_v1.json",
  ["phase600-summary"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",
  ["phase600-exactarithmetic-helper"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/ExactArithmetic.cs",
  ["phase600-fouriertensor-helper"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/FourierTensor.cs",
  ["phase600-traceadjoint-helper"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/TraceAdjoint.cs",
  ["phase607-program"]="studies/phase607_source_induced_vertical_curvature_audit_001/Program.cs",
  ["phase607-project"]="studies/phase607_source_induced_vertical_curvature_audit_001/Phase607SourceInducedVerticalCurvatureAudit.csproj",
  ["phase607-study"]="studies/phase607_source_induced_vertical_curvature_audit_001/STUDY.md",
  ["phase607-contract"]="studies/phase607_source_induced_vertical_curvature_audit_001/preregistration/contract_v1.json",
  ["phase607-summary"]="studies/phase607_source_induced_vertical_curvature_audit_001/output/source_induced_vertical_curvature_audit_summary.json",
  ["phase607-verticalgeometry-helper"]="studies/phase607_source_induced_vertical_curvature_audit_001/VerticalGeometry.cs",
  ["phase608-program"]="studies/phase608_source_induced_ambient_ricci_audit_001/Program.cs",
  ["phase608-project"]="studies/phase608_source_induced_ambient_ricci_audit_001/Phase608SourceInducedAmbientRicciAudit.csproj",
  ["phase608-study"]="studies/phase608_source_induced_ambient_ricci_audit_001/STUDY.md",
  ["phase608-contract"]="studies/phase608_source_induced_ambient_ricci_audit_001/preregistration/contract_v1.json",
  ["phase608-summary"]="studies/phase608_source_induced_ambient_ricci_audit_001/output/source_induced_ambient_ricci_audit_summary.json",
  ["phase608-ambientgeometry-helper"]="studies/phase608_source_induced_ambient_ricci_audit_001/AmbientGeometry.cs",
  ["phase610-program"]="studies/phase610_induced_spin_curvature_contraction_audit_001/Program.cs",
  ["phase610-project"]="studies/phase610_induced_spin_curvature_contraction_audit_001/Phase610InducedSpinCurvatureContractionAudit.csproj",
  ["phase610-study"]="studies/phase610_induced_spin_curvature_contraction_audit_001/STUDY.md",
  ["phase610-contract"]="studies/phase610_induced_spin_curvature_contraction_audit_001/preregistration/contract_v1.json",
  ["phase610-summary"]="studies/phase610_induced_spin_curvature_contraction_audit_001/output/induced_spin_curvature_contraction_audit_summary.json",
  ["phase610-spingeometry-helper"]="studies/phase610_induced_spin_curvature_contraction_audit_001/SpinGeometry.cs",
  ["phase611-program"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/Program.cs",
  ["phase611-project"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/Phase611UntiedCaaResponseJointGaugeAudit.csproj",
  ["phase611-study"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/STUDY.md",
  ["phase611-contract"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/preregistration/contract_v1.json",
  ["phase611-summary"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/output/untied_caa_response_joint_gauge_audit_summary.json",
  ["phase611-caaoperator-helper"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/CaaOperator.cs",
  ["phase618-program"]="studies/phase618_homogeneous_covariant_connection_audit_001/Program.cs",
  ["phase618-project"]="studies/phase618_homogeneous_covariant_connection_audit_001/Phase618HomogeneousCovariantConnectionAudit.csproj",
  ["phase618-study"]="studies/phase618_homogeneous_covariant_connection_audit_001/STUDY.md",
  ["phase618-contract"]="studies/phase618_homogeneous_covariant_connection_audit_001/preregistration/contract_v1.json",
  ["phase618-summary"]="studies/phase618_homogeneous_covariant_connection_audit_001/output/homogeneous_covariant_connection_audit_summary.json",
  ["phase618-homogeneousconnection-helper"]="studies/phase618_homogeneous_covariant_connection_audit_001/HomogeneousConnection.cs",
  ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
  ["core-source-manifest"]="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json",
  ["build-props"]="Directory.Build.props",
  ["phase622-program"]="studies/phase622_full_homogeneous_kinetic_carrier_audit_001/Program.cs",
  ["phase622-project"]="studies/phase622_full_homogeneous_kinetic_carrier_audit_001/Phase622FullHomogeneousKineticCarrierAudit.csproj",
  ["phase622-study"]="studies/phase622_full_homogeneous_kinetic_carrier_audit_001/STUDY.md",
  ["phase622-contract"]="studies/phase622_full_homogeneous_kinetic_carrier_audit_001/preregistration/contract_v1.json",
  ["phase622-summary"]="studies/phase622_full_homogeneous_kinetic_carrier_audit_001/output/full_homogeneous_kinetic_carrier_audit_summary.json",
  ["phase622-kinetic-helper"]="studies/phase622_full_homogeneous_kinetic_carrier_audit_001/KineticCarrier.cs",
  ["phase623-program"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/Program.cs",
  ["phase623-project"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/Phase623FullInverseKappaFifthOrderFeedbackAudit.csproj",
  ["phase623-study"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/STUDY.md",
  ["phase623-contract"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/preregistration/contract_v1.json",
  ["phase623-summary"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/output/full_inverse_kappa_fifth_order_feedback_audit_summary.json",
  ["phase623-feedback-helper"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/InverseFeedback.cs",
  ["phase623-point0"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/output/points/point0.json",
  ["phase623-point1"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/output/points/point1.json",
  ["certified-operator-helper"]="studies/phase626_fixed_cubic_full_stationary_residual_certificate_001/CertifiedOperators.cs",
  ["streaming-evidence-helper"]="studies/phase626_fixed_cubic_full_stationary_residual_certificate_001/StreamingEvidence.cs",
  ["chunk-plan"]="studies/phase626_fixed_cubic_full_stationary_residual_certificate_001/preregistration/chunk_plan_v1.json",
  ["phase619-program"]="studies/phase619_invariant_bivector_nonlinear_feedback_audit_001/Program.cs",
  ["phase619-project"]="studies/phase619_invariant_bivector_nonlinear_feedback_audit_001/Phase619InvariantBivectorNonlinearFeedbackAudit.csproj",
  ["phase619-study"]="studies/phase619_invariant_bivector_nonlinear_feedback_audit_001/STUDY.md",
  ["phase619-contract"]="studies/phase619_invariant_bivector_nonlinear_feedback_audit_001/preregistration/contract_v1.json",
  ["phase619-summary"]="studies/phase619_invariant_bivector_nonlinear_feedback_audit_001/output/invariant_bivector_nonlinear_feedback_audit_summary.json",
  ["phase619-feedback-helper"]="studies/phase619_invariant_bivector_nonlinear_feedback_audit_001/BivectorFeedback.cs"
 };
 const string OutputDirectory="studies/phase626_fixed_cubic_full_stationary_residual_certificate_001/output";
 const string ContractPath=Root+"/preregistration/contract_v1.json";
 const string Success="fixed-cubic-full-residual-certified-conditional-stationary-branch";
 static readonly string[] Firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
 static readonly string[] Precedence=["invalid-or-drifted-input","known-answer-control-failed","full-domain-control-failed","homogeneous-connection-control-failed","full-operator-control-failed","full-polynomial-control-failed","original-action-control-failed","contraction-certificate-control-failed","resource-census-control-failed",Success];
 static bool contractValid,exactBindingsValid,coreSourceTreeValid,knownAnswerPassed=true;
 static int coreFileCount,maximumScalarRationalCharacters=1;
 static long maximumMeasuredManagedBytes;
 static Binding[] bindings=[];
 static StreamingEvidence? evidenceStore;
 static readonly List<ContextPin> contexts=[];
 static readonly List<JsonElement> pointSummaries=[];
 static string Sha(string path)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
 static string Hash(string text)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
 static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(s=>s is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
 static bool Preflight()
 {
  using var document=JsonDocument.Parse(File.ReadAllBytes(ContractPath));var c=document.RootElement;
  contractValid=c.GetProperty("schemaVersion").GetInt32()==1&&c.GetProperty("phase").GetInt32()==626&&c.GetProperty("contractId").GetString()==ContractId&&c.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&c.GetProperty("deterministicZeroSampling").GetBoolean()&&c.GetProperty("externalReviewPending").GetBoolean()&&c.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(c.GetProperty("fixtures").GetRawText()))&&c.GetProperty("terminalPrecedence").EnumerateArray().Select(x=>x.GetString()).SequenceEqual(Precedence)&&c.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&Firewalls.All(k=>c.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
  bindings=c.GetProperty("exactBindings").EnumerateArray().Select(b=>new Binding(b.GetProperty("id").GetString()!,b.GetProperty("path").GetString()!,b.GetProperty("sha256").GetString()!)).ToArray();
  exactBindingsValid=bindings.Length==67&&Paths.Count==67&&bindings.Select(b=>b.id).Distinct().Count()==67&&bindings.Select(b=>b.path).Distinct().Count()==67&&bindings.All(b=>Paths.TryGetValue(b.id,out var p)&&p==b.path&&b.hashMatches);
  var compiled=Directory.EnumerateFiles(Root,"*.cs",SearchOption.AllDirectories).Where(p=>!p.Split('/').Any(s=>s is "bin" or "obj")).Concat(Regex.Matches(File.ReadAllText(Paths["project"]),"<Compile Include=\"([^\"]+)\"").Select(m=>Path.GetRelativePath(".",Path.GetFullPath(Path.Combine(Root,m.Groups[1].Value))))).ToArray();
  exactBindingsValid&=compiled.Length==9&&compiled.Distinct().Count()==9&&compiled.All(p=>bindings.Any(b=>b.path==p));
  if(!contractValid||!exactBindingsValid)return false;
  using var manifest=JsonDocument.Parse(File.ReadAllBytes(Paths["core-source-manifest"]));var m=manifest.RootElement;var live=CorePaths();coreFileCount=live.Length;
  coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&m.GetProperty("files").EnumerateArray().Select(x=>x.GetProperty("path").GetString()).SequenceEqual(live)&&m.GetProperty("files").EnumerateArray().All(x=>Sha(x.GetProperty("path").GetString()!)==x.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();
  if(!coreSourceTreeValid)return false;
  foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase607","source-induced-vertical-curvature-controls-pass-flat-reference-not-induced"),("phase608","source-induced-ambient-ricci-controls-pass-conditional-curved-background"),("phase610","induced-spin-curvature-contraction-controls-pass-nonzero-conditional-source"),("phase611","untied-caa-response-controls-pass-source-choice-open"),("phase618","homogeneous-connection-controls-pass-conditional-local-existence"),("phase619","invariant-bivector-controls-pass-nonlinear-grade-five-required"),("phase622","full-homogeneous-kinetic-carrier-controls-pass-conditional-linear-branch"),("phase623","full-inverse-kappa-fifth-order-controls-pass-grade-five-branch-required")})
  {
   using var upstream=JsonDocument.Parse(File.ReadAllBytes(Paths[id+"-summary"]));var u=upstream.RootElement;
   if(!u.GetProperty("auditPassed").GetBoolean()||!u.GetProperty("contractValid").GetBoolean()||!u.GetProperty("exactBindingsValid").GetBoolean()||!u.GetProperty("coreSourceTreeValid").GetBoolean()||u.GetProperty("contractSha256").GetString()!=Sha(Paths[id+"-contract"])||u.GetProperty("verdictKind").GetString()!=terminal||!u.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()||!u.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()||u.GetProperty("authorityFirewalls").EnumerateObject().Count()!=14||!Firewalls.All(k=>u.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)||!u.GetProperty("externalReviewPending").GetBoolean()||u.GetProperty("promotedPhysicalMassClaimCount").GetInt32()!=0)return false;
  }
  string source=File.ReadAllText(Paths["primary-source"]);if(!new[]{"(3.7)","(3.8)","(3.10)","(3.15)","(3.17)","(3.27)","(3.34)","(9.1)","(9.4)","(12.26)","(12.27)"}.All(source.Contains))return false;
  using var plan=JsonDocument.Parse(File.ReadAllBytes(Paths["chunk-plan"]));var p0=plan.RootElement;var generated=CertificateLayout.Plan();
  return p0.GetProperty("schemaVersion").GetInt32()==1&&p0.GetProperty("phase").GetInt32()==626&&p0.GetProperty("encoding").GetString()=="expanded-rational-tensor-chunks-v1"&&p0.GetProperty("chunkCount").GetInt32()==3914&&generated.Length==3914&&p0.GetProperty("chunks").GetRawText()==JsonSerializer.Serialize(generated);
 }
 static string ScientificVerdict()=>!knownAnswerPassed?Precedence[1]:!domainPassed?Precedence[2]:!geometryPassed?Precedence[3]:!operatorPassed?Precedence[4]:!polynomialPassed?Precedence[5]:!actionPassed?Precedence[6]:!certificatePassed?Precedence[7]:Success;
 static void CheckResources()
 {
  maximumMeasuredManagedBytes=Math.Max(maximumMeasuredManagedBytes,GC.GetTotalMemory(false));
  if(CoefficientProducts>500000000||Fifth.SlotProducts>200000000||LargestTensor>600000||maximumMeasuredManagedBytes>8589934592L)throw new ResourceLimitException("grouped/slot/support/measured managed-memory ceiling");
 }
 static void ScanScalars(JsonElement e)
 {
  if(e.ValueKind==JsonValueKind.Object)foreach(var p in e.EnumerateObject())ScanScalars(p.Value);
  else if(e.ValueKind==JsonValueKind.Array)foreach(var q in e.EnumerateArray())ScanScalars(q);
  else if(e.ValueKind==JsonValueKind.String){string s=e.GetString()!;if(Regex.IsMatch(s,"^-?[0-9]+(?:/[0-9]+)?$")){maximumScalarRationalCharacters=Math.Max(maximumScalarRationalCharacters,s.Length);if(s.Length>512)throw new ResourceLimitException("scalar rational-character ceiling");}}
 }
 static void WriteContext(int point,object value)
 {
  var element=JsonSerializer.SerializeToElement(value,StreamingEvidence.JsonOptions);ScanScalars(element);byte[] bytes=Encoding.UTF8.GetBytes(element.GetRawText()+"\n");if(bytes.Length>1048576)throw new ResourceLimitException("point-context byte ceiling");
  string path=Root+"/output/point"+point+"_context.json";Directory.CreateDirectory(OutputDirectory);File.WriteAllBytes(path,bytes);contexts.Add(new(point,path,Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),bytes.Length));pointSummaries.Add(element);Inc("pointContexts");
 }
 public static int Main()
 {
  using var fixture=JsonDocument.Parse(FixtureJson);var expected=fixture.RootElement.GetProperty("expectedCounts");foreach(var p in expected.EnumerateObject())Counts.Add(p.Name,0);
  try{if(!Preflight()){Emit(Precedence[0],false,false,false,"preflight rejected");return 1;}}
  catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or ArgumentException or KeyNotFoundException or FormatException){Emit(Precedence[0],false,false,false,ex.GetType().Name);return 1;}
  try
  {
   foreach(bool pass in new[]{new Rational(2,4)==new Rational(1,2),new Rational(-3,-6)==new Rational(1,2),new Rational(2,3)*new Rational(3,2)==1,Scalar.I*Scalar.I==new Scalar(-1)}){Inc("arithmeticControls");knownAnswerPassed&=pass;}
   for(int a=0;a<=Full;a++)
   {
    foreach(int b in Enumerable.Range(0,14).Select(i=>1<<i).Append(Full)){Inc("wordCases",2);knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b)&&BladeSign(b,a)==WordSign(b,a);}Inc("wordCases");knownAnswerPassed&=BladeSign(a,a)==WordSign(a,a);
    Inc("hodgeCases");int d=Degree(a);knownAnswerPassed&=HodgeSign(a)*HodgeSign(Full^a)==((d*(14-d)+7)%2==0?1:-1);Inc("fullRealDomainMasks");knownAnswerPassed&=HAnti(One(0,a,AdjointSign(a)==-1?1:Scalar.I));
   }
   foreach(bool pass in new[]{HAnti(One(0,0,Scalar.I)),Equal(Product(One(0,1,1),One(0,1,1),'A'),One(0,0,Scalar.I*2))}){Inc("centralDomainControls");knownAnswerPassed&=pass;}
   if(!knownAnswerPassed){Emit(Precedence[1],false,false,false,null);return 1;}
   evidenceStore=new StreamingEvidence(CertificateLayout.Plan());using var connection=JsonDocument.Parse(File.ReadAllBytes(Paths["phase618-summary"]));
   for(int point=0;point<2;point++){using var prior=JsonDocument.Parse(File.ReadAllBytes(Paths["phase623-point"+point]));var context=ScientificPoint(point,prior.RootElement,connection.RootElement.GetProperty("evidence").GetProperty("rows")[point],evidenceStore);WriteContext(point,context);CheckResources();}
   var left=pointSummaries[0].GetProperty("transportFingerprints");var right=pointSummaries[1].GetProperty("transportFingerprints");foreach(var p in left.EnumerateObject()){Inc("transportComparisons");geometryPassed&=p.Value.GetString()==right.GetProperty(p.Name).GetString();}
   Counts["evidenceChunks"]=evidenceStore.Pins.Count;
   var allowed=evidenceStore.Pins.Select(p=>p.path).Concat(contexts.Select(p=>p.path)).Concat(new[]{OutputDirectory+"/"+Stem+".json",OutputDirectory+"/"+Stem+"_summary.json"}).ToHashSet(StringComparer.Ordinal);
   bool shardSetPassed=evidenceStore.Complete()&&contexts.Count==2&&Directory.EnumerateFiles(OutputDirectory,"*",SearchOption.AllDirectories).All(allowed.Contains);
   bool countsPassed=Counts.Count==expected.EnumerateObject().Count()&&Counts.All(p=>p.Value==expected.GetProperty(p.Key).GetInt64());CheckResources();
   bool resourcesPassed=shardSetPassed&&Certified.KineticCalls==28&&Certified.DerivativeSlots==392&&Certified.ForwardCalls==164&&Certified.AdjointCalls==420&&Certified.TransposeCalls==2624&&Certified.WordTransposeCalls==104&&Certified.NaiveProductCalls==350;
   string terminal=ScientificVerdict();if(terminal==Success&&(!countsPassed||!resourcesPassed))terminal=Precedence[8];return Emit(terminal,countsPassed,resourcesPassed,shardSetPassed,null)==Success?0:1;
  }
  catch(Exception ex) when(ex is ResourceLimitException or IOException or JsonException or InvalidOperationException or ArgumentException or OverflowException or KeyNotFoundException or FormatException)
  {string terminal=ScientificVerdict();if(terminal==Success)terminal=Precedence[8];Emit(terminal,false,false,false,ex.GetType().Name+": "+ex.Message);return 1;}
 }
 static string Emit(string terminal,bool countsPassed,bool resourcesPassed,bool shardSetPassed,string? resourceFailure)
 {
  Console.Error.WriteLine("Phase626 unpinned run telemetry: sampled managed-memory maximum bytes="+maximumMeasuredManagedBytes);
  using var fixture=JsonDocument.Parse(FixtureJson);
  object Evidence(bool compact)=>new{knownAnswerPassed=terminal==Precedence[0]?false:knownAnswerPassed,geometryPassed,operatorPassed,polynomialPassed,certificatePassed,actionPassed,domainPassed,controlsPassed=terminal==Success,countsPassed,resourcesPassed,shardSetPassed,resourceFailure,counts=Counts,pointSummaries=compact?[]:pointSummaries.ToArray(),shards=evidenceStore?.Pins.ToArray()??[],contexts=contexts.ToArray(),trackedCoefficientProducts=CoefficientProducts,trackedSlotProducts=Fifth.SlotProducts,trackedTransposePairVisits=Certified.TransposePairVisits,trackedWordTransposePairVisits=Certified.WordTransposePairVisits,trackedNaiveProductPairVisits=Certified.NaiveProductPairVisits,kineticCalls=Certified.KineticCalls,derivativeSlots=Certified.DerivativeSlots,forwardCalls=Certified.ForwardCalls,adjointCalls=Certified.AdjointCalls,transposeCalls=Certified.TransposeCalls,wordTransposeCalls=Certified.WordTransposeCalls,naiveProductCalls=Certified.NaiveProductCalls,largestTensor=LargestTensor,largestRetainedTensor=evidenceStore?.LargestTensor??0,maximumTensorRationalCharacters=evidenceStore?.LargestRationalCharacters??0,maximumScalarRationalCharacters,managedMemoryGuardPassed=maximumMeasuredManagedBytes<=8589934592L,managedMemoryCeilingBytes=8589934592L,totalShardBytes=evidenceStore?.TotalBytes??0,maximumShardBytes=evidenceStore?.LargestFileBytes??0,retainedRecords=evidenceStore?.RetainedRecords??0,scope=fixture.RootElement.GetProperty("scope").Clone()};
  object Result(object evidence)=>new{schemaVersion=1,phase=626,phaseId="phase626-fixed-cubic-full-stationary-residual-certificate",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(b=>b.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(b=>b.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=Firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
  byte[] bytes=Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Result(Evidence(false)),StreamingEvidence.JsonOptions)+"\n");
  if(bytes.Length>16777216){if(terminal==Success)terminal=Precedence[8];resourceFailure="manifest byte ceiling; complete point summaries remain in their context files";countsPassed=false;resourcesPassed=false;bytes=Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Result(Evidence(true)),StreamingEvidence.JsonOptions)+"\n");}
  if(bytes.Length>16777216)throw new ResourceLimitException("bounded failure manifest unexpectedly exceeds cap");
  Directory.CreateDirectory(OutputDirectory);File.WriteAllBytes(OutputDirectory+"/"+Stem+".json",bytes);File.WriteAllBytes(OutputDirectory+"/"+Stem+"_summary.json",bytes);Console.WriteLine("Phase626 verdict: "+terminal);return terminal;
 }
}
internal sealed record ContextPin(int point,string path,string sha256,long bytes);
