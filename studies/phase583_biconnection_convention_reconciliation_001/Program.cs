using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase583_biconnection_convention_reconciliation_001";
const string ContractPath = Root + "/preregistration/contract_v1.json";
const string SourcePath = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt";
const string BridgePath = "studies/phase559_bounded_transformation_law_source_census_001/output/bounded_transformation_law_source_census_summary.json";
const string PriorPath = "studies/phase582_relative_transport_observable_control_001/output/relative_transport_observable_control_summary.json";
const string OutputPath = Root + "/output/biconnection_convention_reconciliation.json";
const string SummaryPath = Root + "/output/biconnection_convention_reconciliation_summary.json";
const string Success = "printed-sign-conflict-proved-two-compatible-families-registered-map-unresolved";
using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
string[] forbidden = ["authorIntentSelected", "registeredTransformationBridgeEstablished", "registeredActionChanged",
    "registeredMeasureSelected", "physicalHiggsIdentified", "sourceContractApplicationAllowed",
    "phase561Opened", "o4Discharged", "phase458Satisfied", "phase481Changed",
    "samplingPerformed", "samplingAuthorized", "productionAuthorized", "gevClaimAllowed"];
var bindings = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new
{
    id=x.GetProperty("id").GetString()!, path=x.GetProperty("path").GetString()!, sha256=x.GetProperty("sha256").GetString()!
}).Select(x => new {x.id,x.path,x.sha256,hashMatches=File.Exists(x.path) && Sha(x.path)==x.sha256}).ToArray();
bool contractValid = contract.GetProperty("schemaVersion").GetInt32()==1
    && contract.GetProperty("phase").GetInt32()==583
    && contract.GetProperty("contractId").GetString()=="phase583-a45-biconnection-signs-v1"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("arithmetic").GetString()=="checked-int64-exact"
    && forbidden.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
bool exactBindingsValid = bindings.Length==5 && bindings.Select(x=>x.id).Distinct().Count()==5
    && bindings.Select(x=>x.path).Distinct().Count()==5 && bindings.All(x=>x.hashMatches);
if(!contractValid || !exactBindingsValid) { Emit("invalid-or-drifted-input",new {}); return; }

// Exact algebra controls before reading scientific source or upstream results.
long[,] identity={{1,0,0},{0,1,0},{0,0,1}};
long[,] jx=Skew(1,0,0), jy=Skew(0,1,0), jz=Skew(0,0,1);
var rotations=Rotations();
bool knownAnswerPassed=Eq(Sub(Mul(jx,jy),Mul(jy,jx)),jz)
    && !Eq(Mul(jx,jy),Mul(jy,jx)) && rotations.Count==24
    && rotations.Select(Key).Distinct().Count()==24
    && rotations.All(h=>Det(h)==1 && Eq(Mul(Tr(h),h),identity))
    && rotations.All(h=>rotations.Any(k=>Eq(k,Tr(h))))
    && rotations.All(h=>rotations.All(k=>rotations.Any(l=>Eq(l,Mul(h,k)))));
if(!knownAnswerPassed) { Emit("known-answer-battery-failed",new {knownAnswerPassed}); return; }
string source=File.ReadAllText(SourcePath);
using var bridge=JsonDocument.Parse(File.ReadAllBytes(BridgePath));
using var prior=JsonDocument.Parse(File.ReadAllBytes(PriorPath));
bool sourceAnchorsPresent=new[]{"(6.2)","(6.11)","(6.13)","(6.18)","(6.20)","multiple sign conventions"}
    .All(x=>source.Contains(x,StringComparison.Ordinal));
bool upstreamValid=sourceAnchorsPresent
    && bridge.RootElement.GetProperty("verdictKind").GetString()=="bounded-source-census-finds-continuum-law-discrete-bridge-incomplete"
    && prior.RootElement.GetProperty("auditPassed").GetBoolean();
if(!upstreamValid) { Emit("invalid-or-drifted-input",new {knownAnswerPassed,sourceAnchorsPresent}); return; }

// A first jet (h,dh) is unrestricted at a point subject to dh tangent to SO(3).
// d0h=dh+A0h-hA0. Compose dh by the ordinary product rule, not by the cocycle formula.
long[,] a0=Skew(1,-2,1), p=Skew(2,1,-1), d=Skew(-1,2,3);
long[][,] cs=[Skew(1,2,-1),Skew(-2,1,3),Skew(3,-1,2)];
var rows=new List<object>();
int compatibleCount=0, stabilizerCompatibleCount=0, jetCount=0;
long cocycleResidual=0, referenceRecoveryResidual=0, maxFormulaResidual=0, maxSignRelabelResidual=0;
foreach(int c in new[]{-1,1}) foreach(int s in new[]{-1,1}) foreach(int r in new[]{-1,1})
{
    long connectionResidual=0, quotientResidual=0, dictionaryResidual=0;
    foreach(var e in rotations) foreach(var h in rotations) foreach(var ch in cs)
    {
        jetCount++;
        var de=Sub(Add(Mul(e,d),Mul(e,a0)),Mul(a0,e));
        var dh=Sub(Add(Mul(h,ch),Mul(h,a0)),Mul(a0,h));
        var eh=Mul(e,h);
        var deh=Add(Mul(de,h),Mul(e,dh));
        var ce=Mul(Tr(e),D0(e,de,a0));
        var cH=Mul(Tr(h),D0(h,dh,a0));
        var ceh=Mul(Tr(eh),D0(eh,deh,a0));
        referenceRecoveryResidual=Max(referenceRecoveryResidual,Norm(Sub(ce,d)),Norm(Sub(cH,ch)));
        cocycleResidual=Max(cocycleResidual,Norm(Sub(ceh,Add(AdInv(h,ce),cH))));
        var pp=Add(AdInv(h,p),Scale(s,cH));
        var a=Add(a0,Scale(c,p)); var b=Add(a0,ce);
        var ap=Add(a0,Scale(c,pp)); var bp=Add(a0,ceh);
        var expectedA=Gauge(a,h,dh); var expectedB=Gauge(b,h,dh);
        var aResidual=Sub(ap,expectedA);
        var pi=Add(Mul(Mul(e,p),Tr(e)),Scale(r,Mul(D0(e,de,a0),Tr(e))));
        var pip=Add(Mul(Mul(eh,pp),Tr(eh)),Scale(r,Mul(D0(eh,deh,a0),Tr(eh))));
        var qResidual=Sub(pip,pi);
        var expectedQ=Scale(s+r,Mul(Mul(Mul(Mul(e,h),cH),Tr(h)),Tr(e)));
        connectionResidual=Max(connectionResidual,Norm(aResidual),Norm(Sub(bp,expectedB)));
        quotientResidual=Max(quotientResidual,Norm(qResidual));
        dictionaryResidual=Max(dictionaryResidual,Norm(Sub(Scale(c,pi),Mul(Mul(e,Sub(a,b)),Tr(e)))));
        maxFormulaResidual=Max(maxFormulaResidual,Norm(Sub(aResidual,Scale(c*s-1,cH))),Norm(Sub(qResidual,expectedQ)));
        // Simultaneously negate p,c,s,r: A,B,T and c*pi are unchanged.
        var relabeledP=Add(AdInv(h,Scale(-1,p)),Scale(-s,cH));
        var relabeledA=Add(a0,Scale(-c,relabeledP));
        var relabeledPi=Add(Mul(Mul(e,Scale(-1,p)),Tr(e)),Scale(-r,Mul(D0(e,de,a0),Tr(e))));
        maxSignRelabelResidual=Max(maxSignRelabelResidual,Norm(Sub(relabeledA,ap)),Norm(Sub(Scale(-c,relabeledPi),Scale(c,pi))));
    }
    bool compatible=c*s==1 && s+r==0;
    if(compatible) compatibleCount++;
    if(compatible && s==-1) stabilizerCompatibleCount++;
    bool rowPassed=(connectionResidual==0)==(c*s==1) && (quotientResidual==0)==(s+r==0)
        && (!compatible || dictionaryResidual==0);
    maxFormulaResidual=Max(maxFormulaResidual,rowPassed?0:1);
    rows.Add(new {c,s,r,connectionDefectCoefficient=c*s-1,quotientDefectCoefficient=s+r,
        stabilizerDefectCoefficient=1+s,connectionResidual,quotientResidual,dictionaryResidual,
        compatible,originalRightActionStabilizer=s==-1,rowPassed});
}

// Nonconstant h(x)=exp(x*C), evaluated at x=0: h=I, dh=C, A0=p=0, epsilon=I.
// The Lie-vector squared norm is half the Frobenius squared norm.
var witnessC=Skew(1,2,-1);
var printedMovedT=Scale(-2,witnessC);
long printedWitnessNormSquared=FrobeniusSquared(printedMovedT)/2;
bool witnessPassed=printedWitnessNormSquared==24 && Norm(printedMovedT)==4;
bool controlsPassed=compatibleCount==2 && stabilizerCompatibleCount==1 && jetCount==13824
    && cocycleResidual==0 && referenceRecoveryResidual==0 && maxFormulaResidual==0
    && maxSignRelabelResidual==0 && witnessPassed;
Emit(controlsPassed?Success:"mathematical-control-failed",new
{
    knownAnswerPassed,upstreamValid,controlsPassed,sourceAnchorsPresent,
    signCensus=new {rows,compatibleCount,stabilizerCompatibleCount,menuExhaustive=true,allPossibleSourceRepairsExhausted=false},
    exactJetControls=new {rotationCount=rotations.Count,jetCount,cocycleResidual,referenceRecoveryResidual,maxFormulaResidual,maxSignRelabelResidual,
        arithmetic="checked-int64-exact",nonzeroReferenceConnection=true,independentProductRule=true},
    printedWitness=new {witnessPassed,initialRelativeFieldZero=true,printedWitnessNormSquared,
        formula="At h=epsilon=I, A0=p=0, dh=C: printed A'=-C, B'=+C, T'=-2C",
        simultaneousPrintedDefinitionsIncompatible=true,wholeTheoryFalsified=false},
    dictionary=new {formula="For c*s=1 and r=-s: T=c*p-epsilon^-1*d0(epsilon); c*pi_r=epsilon*T*epsilon^-1",
        compatibleTriples=new[]{new[]{-1,-1,1},new[]{1,1,-1}},
        translation="(p,c,s,r) -> (-p,-c,-s,-r)",finiteCoefficientSignFlipAbsoluteJacobian=1,
        physicalActionEquivalenceEstablished=false,registeredOmegaIdentificationEstablished=false,
        interactingMeasureDerived=false,authorIntentSelected=false},
    nextRequirement="Derive the two-connection action in T and B, distinguish dropping its torsion term from constraining T=0, and compare with registered omega/curvature/measure. No sampling before that dictionary."
});

void Emit(string verdict,object evidence)
{
    var result=new {schemaVersion=1,phase=583,phaseId="phase583-biconnection-convention-reconciliation",
        contractId="phase583-a45-biconnection-signs-v1",contractSha256=Sha(ContractPath),contractValid,exactBindingsValid,bindings,
        verdictKind=verdict,terminalStatus=verdict,auditPassed=verdict==Success,evidence,deterministic=true,
        authorityFirewalls=forbidden.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
    string json=JsonSerializer.Serialize(result,new JsonSerializerOptions {WriteIndented=true})+Environment.NewLine;
    Directory.CreateDirectory(Root+"/output"); File.WriteAllText(OutputPath,json); File.WriteAllText(SummaryPath,json);
    Console.WriteLine($"Phase583 verdict: {verdict}"); if(verdict!=Success) Environment.ExitCode=1;
}
static string Sha(string path)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static long[,] Skew(long x,long y,long z)=>new long[,]{{0,-z,y},{z,0,-x},{-y,x,0}};
static long[,] Add(long[,] a,long[,] b) {var o=new long[3,3]; for(int i=0;i<3;i++) for(int j=0;j<3;j++) o[i,j]=checked(a[i,j]+b[i,j]); return o;}
static long[,] Scale(long s,long[,] a) {var o=new long[3,3]; for(int i=0;i<3;i++) for(int j=0;j<3;j++) o[i,j]=checked(s*a[i,j]); return o;}
static long[,] Sub(long[,] a,long[,] b)=>Add(a,Scale(-1,b));
static long[,] Mul(long[,] a,long[,] b) {var o=new long[3,3]; for(int i=0;i<3;i++) for(int j=0;j<3;j++) for(int k=0;k<3;k++) o[i,j]=checked(o[i,j]+a[i,k]*b[k,j]); return o;}
static long[,] Tr(long[,] a) {var o=new long[3,3]; for(int i=0;i<3;i++) for(int j=0;j<3;j++) o[i,j]=a[j,i]; return o;}
static long Norm(long[,] a)=>a.Cast<long>().Max(x=>System.Math.Abs(x));
static long FrobeniusSquared(long[,] a)=>a.Cast<long>().Sum(x=>checked(x*x));
static bool Eq(long[,] a,long[,] b)=>Norm(Sub(a,b))==0;
static string Key(long[,] a)=>string.Join(",",a.Cast<long>());
static long Max(params long[] values)=>values.Max();
static long Det(long[,] a)=>checked(a[0,0]*(a[1,1]*a[2,2]-a[1,2]*a[2,1])-a[0,1]*(a[1,0]*a[2,2]-a[1,2]*a[2,0])+a[0,2]*(a[1,0]*a[2,1]-a[1,1]*a[2,0]));
static long[,] AdInv(long[,] h,long[,] a)=>Mul(Mul(Tr(h),a),h);
static long[,] Gauge(long[,] a,long[,] h,long[,] dh)=>Add(AdInv(h,a),Mul(Tr(h),dh));
static long[,] D0(long[,] h,long[,] dh,long[,] a0)=>Sub(Add(dh,Mul(a0,h)),Mul(h,a0));
static List<long[,]> Rotations()
{
    var result=new List<long[,]>();
    for(int i=0;i<3;i++) for(int j=0;j<3;j++) for(int k=0;k<3;k++)
    {
        if(i==j || i==k || j==k) continue;
        foreach(int a in new[]{-1,1}) foreach(int b in new[]{-1,1}) foreach(int c in new[]{-1,1})
        {var m=new long[3,3]; m[0,i]=a; m[1,j]=b; m[2,k]=c; if(Det(m)==1) result.Add(m);}
    }
    return result;
}
