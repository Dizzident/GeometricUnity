using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text.Json;

// No placeholder SHA: these declarations freeze the identity/source/type;
// their actual coefficients are independently authenticated BEFORE Open.
internal sealed record MixedSinkLeafDeclaration(string Id,int Degree,string Source);
internal sealed record MixedTemplateResources(long TemplateBytes,long TotalSerializedBytes);
internal sealed record MixedTemplateUsage(int FactoryCalls,int FrozenContexts,int BoundContexts,long SerializedBytes,bool Frozen,bool Failed);

internal sealed class MixedSinkContextTemplate
{
 public string Id{get;}
 public string GraphPath{get;}
 public string MetadataPath{get;}
 public MixedSinkResources Resources{get;}
 public MixedPointCheckpointPlan? PointCheckpoint{get;}
 public IReadOnlyList<MixedSinkLeafDeclaration> Leaves{get;}
 public IReadOnlyDictionary<string,string> LeafRoles{get;}
 public IReadOnlyList<MixedSinkMark> Marks{get;}
 readonly string callbackId;readonly MixedPlannedCallback[] callbackEntries;
 public MixedContextPlan Callbacks=>new(callbackId,callbackEntries.ToArray());
 public MixedSinkContextTemplate(string id,string graphPath,string metadataPath,MixedSinkResources resources,
  IEnumerable<MixedSinkLeafDeclaration> leaves,IReadOnlyDictionary<string,string> leafRoles,
  IEnumerable<MixedSinkMark> marks,MixedContextPlan callbacks,MixedPointCheckpointPlan? pointCheckpoint=null)
 {
  ArgumentNullException.ThrowIfNull(resources);ArgumentNullException.ThrowIfNull(resources.Trace);MixedTrace.CheckLimits(resources.Trace);
  Need(resources.FileBytes>0&&resources.ContextBytes>0&&resources.MetadataBytes>0&&resources.FailureGraphBytes>0,"positive frozen context resources");
  Id=id;GraphPath=graphPath;MetadataPath=metadataPath;Resources=resources;PointCheckpoint=pointCheckpoint;
  Leaves=Array.AsReadOnly(leaves.ToArray());LeafRoles=new ReadOnlyDictionary<string,string>(new Dictionary<string,string>(leafRoles,StringComparer.Ordinal));
  Marks=Array.AsReadOnly(marks.ToArray());callbackId=callbacks.Id;callbackEntries=callbacks.Callbacks.ToArray();
  Need(Id==callbackId,"callback context identity");
  Need(Leaves.Count<=resources.Trace.Nodes&&Leaves.All(l=>l is not null&&Named(l.Id)&&Named(l.Source)&&l.Degree is >=0 and <=14)&&Leaves.Select(l=>l.Id).Distinct(StringComparer.Ordinal).Count()==Leaves.Count,"unique named typed leaf declarations");
  Need(LeafRoles.All(r=>Named(r.Key)&&Leaves.Any(l=>l.Id==r.Value)),"roles refer to declared leaves");
  Need(Marks.Count<=resources.Trace.Marks&&Marks.All(m=>m is not null&&Named(m.Name)&&m.Degree is >=0 and <=14&&m.Expanded==(m.RelativePath is not null))&&Marks.Select(m=>m.Name).Distinct(StringComparer.Ordinal).Count()==Marks.Count,"complete canonical mark declarations within frozen cap");
  Need(callbackEntries.Length>0&&callbackEntries.All(c=>c is not null&&Named(c.Category)&&Named(c.Name)),"named frozen callback declarations");
 }
 static bool Named(string value)=>!string.IsNullOrWhiteSpace(value)&&value.All(c=>c is >= '\x20' and <= '\x7e');
 static void Need(bool ok,string why){if(!ok)throw new InvalidOperationException("A68 context template: "+why);}
 internal object Wire()=>new{schema="phase627-context-structure-v1",id=Id,graphPath=GraphPath,metadataPath=MetadataPath,
  resources=Resources,pointCheckpoint=PointCheckpoint,leaves=Leaves,leafRoles=LeafRoles,marks=Marks,callbacks=new MixedContextPlan(callbackId,callbackEntries)};
 public MixedSinkContextPlan Bind(IReadOnlyList<MixedTrace.LeafSpec> actual)
 {
  Need(actual is not null&&actual.Count==Leaves.Count,"complete ordered actual leaf bindings");
  var copy=actual!.ToArray();
  Need(copy.Length==Leaves.Count,"actual enumerated leaf census matches frozen declaration");
  for(int i=0;i<copy.Length;i++)
  {
   var declared=Leaves[i];var leaf=copy[i];
   Need(leaf is not null&&leaf.Id==declared.Id&&leaf.Degree==declared.Degree&&leaf.Source==declared.Source,"actual leaf matches frozen identity/source/type");
   Need(leaf!.Sha256 is {Length:64}&&leaf.Sha256.All(c=>c is >= '0' and <= '9' or >= 'a' and <= 'f'),"actual canonical leaf hash required");
  }
  // Matching this shape is NOT source authentication. The sink MUST call its
  // reviewed ValidateContextPlan/ValidateLeaf implementation independently.
  return new(Id,GraphPath,MetadataPath,Resources,copy,LeafRoles,Marks,Callbacks,PointCheckpoint);
 }
}

// Full705 structure is inspected/fingerprinted before any source calculation.
// Store only commitments, not291199 mark descriptors. Regeneration may depend
// only on frozen source algorithms/menus; even a changed factory closure must
// reproduce the exact committed structure or fail BEFORE leaf reconstruction.
// Caller-owned factory state and serializer/construction scratch still need
// separate whole-memory/source-closure review; hashes alone do not prove it.
internal sealed class MixedContextTemplateCatalog
{
 readonly Func<string,MixedSinkContextTemplate> factory;readonly MixedTemplateResources limits;
 readonly Dictionary<string,(long Bytes,string Sha256)> commitments=new(StringComparer.Ordinal);
 readonly HashSet<string> bound=new(StringComparer.Ordinal);readonly HashSet<string> allowed;
 public IReadOnlyList<string> ContextIds{get;}
 bool frozen,failed,busy;int calls;long serializedBytes;
 MixedSinkContextTemplate? currentValidation,currentMaterialization;
 WeakReference<MixedSinkContextPlan>? latestBoundPlan;
 public MixedContextTemplateCatalog(Func<string,MixedSinkContextTemplate> factory,MixedTemplateResources limits)
 {
  this.factory=factory??throw new ArgumentNullException(nameof(factory));this.limits=limits??throw new ArgumentNullException(nameof(limits));
  Need(limits.TemplateBytes>0&&limits.TotalSerializedBytes>0,"positive explicit template byte limits");
  ContextIds=Array.AsReadOnly(MixedAuditPlan.RunContextIds());allowed=new(ContextIds,StringComparer.Ordinal);
 }
 void Need(bool ok,string why){if(!ok){failed=true;throw new InvalidOperationException("A68 template catalog: "+why);}}
 T Guard<T>(Func<T> action)
 {
  Need(!failed&&!busy,"failed or reentrant catalog");busy=true;
  try{var value=action();Need(!failed,"swallowed reentrant catalog failure");return value;}
  catch{failed=true;throw;}finally{busy=false;}
 }
 MixedSinkContextTemplate Create(string id,Action<string> admitConstruction)
 {
  Need(allowed.Contains(id)&&calls<2*MixedAuditPlan.RunContextCount,"exact declared context and prospective factory count");
  Need(admitConstruction is not null,"mandatory pre-construction admission");
  admitConstruction!(id);Need(!failed,"construction admission failed");
  calls++;var template=factory(id);Need(!failed&&template is not null&&template.Id==id,"exact independently frozen template context");return template!;
 }
 (long Bytes,string Sha256) Fingerprint(MixedSinkContextTemplate template)
 {
  using var stream=new TemplateHashStream(limits.TemplateBytes,n=>
  {Need(n<=limits.TotalSerializedBytes-serializedBytes,"prospective cumulative template serialization ceiling");serializedBytes=checked(serializedBytes+n);});
  JsonSerializer.Serialize(stream,template.Wire(),MixedTrace.JsonOptions);stream.WriteByte(10);return(stream.Length,stream.Finish());
 }
 public void Freeze(Action<string> admitConstruction,Action<MixedSinkContextTemplate> validateStructure)=>Guard(()=>
 {
  Need(!frozen&&commitments.Count==0&&validateStructure is not null,"one full pre-work structural freeze");
  foreach(string id in ContextIds)
  {
   var template=Create(id,admitConstruction);currentValidation=template;
   try{validateStructure!(template);Need(!failed,"structural validation failed");}
   finally{currentValidation=null;}
   commitments.Add(id,Fingerprint(template));
  }
  MixedAuditPlan.ValidateRunContextIds(commitments.Keys);frozen=true;return true;
 });
 // Bind an external validator to the exact object this catalog will fingerprint,
 // not merely an equal ID/count or a factory's self-submitted alternate template.
 public void RequireCurrentValidation(MixedSinkContextTemplate template)
 {Need(!failed&&busy&&currentValidation is not null&&ReferenceEquals(currentValidation,template),"exact catalog-owned validation callback object");}
 public void RequireCurrentMaterialization(MixedSinkContextTemplate template)
 {Need(!failed&&frozen&&busy&&currentMaterialization is not null&&ReferenceEquals(currentMaterialization,template),"exact fingerprint-checked materialization callback object");}
 // Weak identity only: the catalog does not retain another full bound plan.
 // A correct-looking surrogate cannot stand in for Materialize's actual result.
 public void RequireLatestBoundPlan(MixedSinkContextPlan plan)
 {Need(!failed&&frozen&&!busy&&latestBoundPlan is not null&&latestBoundPlan.TryGetTarget(out var actual)&&ReferenceEquals(actual,plan),"exact latest catalog-bound plan object");}
 public MixedSinkContextPlan Materialize(string id,Action<string> admitConstruction,Func<MixedSinkContextTemplate,IReadOnlyList<MixedTrace.LeafSpec>> bindLeaves)=>Guard(()=>
 {
  Need(frozen&&allowed.Contains(id)&&!bound.Contains(id)&&bindLeaves is not null,"frozen unique context before binding");
  var template=Create(id,admitConstruction);Need(Fingerprint(template)==commitments[id],"template changed after pre-work commitment");
  // No observed graph, coefficient support, checks or geometry is a factory
  // input. The ONLY late-bound fields are separately authenticated leaf SHA.
  IReadOnlyList<MixedTrace.LeafSpec> actual;currentMaterialization=template;
  try{actual=bindLeaves!(template);Need(!failed,"leaf binder failed or reentered");}
  finally{currentMaterialization=null;}
  var plan=template.Bind(actual);Need(bound.Add(id),"one context materialization");latestBoundPlan=new(plan);return plan;
 });
 public MixedTemplateUsage Snapshot()=>new(calls,commitments.Count,bound.Count,serializedBytes,frozen,failed);
 sealed class TemplateHashStream(long ceiling,Action<int> charge):Stream
 {
  readonly IncrementalHash hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);long count;bool finished;
  public override void Write(ReadOnlySpan<byte> bytes)
  {if(finished||bytes.Length>ceiling-count)throw new InvalidOperationException("A68 template serialization prewrite ceiling");charge(bytes.Length);hash.AppendData(bytes);count+=bytes.Length;}
  public string Finish(){if(finished)throw new InvalidOperationException("template hash already finished");finished=true;return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();}
  public override void Write(byte[] bytes,int offset,int length)=>Write(bytes.AsSpan(offset,length));
  public override bool CanRead=>false;public override bool CanSeek=>false;public override bool CanWrite=>true;public override long Length=>count;
  public override long Position{get=>count;set=>throw new NotSupportedException();}public override void Flush(){}
  public override int Read(byte[] b,int o,int c)=>throw new NotSupportedException();public override long Seek(long o,SeekOrigin s)=>throw new NotSupportedException();public override void SetLength(long l)=>throw new NotSupportedException();
  protected override void Dispose(bool disposing){if(disposing)hash.Dispose();base.Dispose(disposing);}
 }
}
