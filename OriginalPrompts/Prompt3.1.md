## Observerse Formalization

This section formalizes the observerse construction as the first major replacement of ordinary space-time fundamentality in the draft. The source draft explicitly introduces the observerse in order to replace a single-space starting point by a pair of spaces linked by observation maps, and it ties this move to the claim that space-time should be recovered rather than assumed fundamental. It also explicitly defines native and invasive fields using pullback along the observation map.  The present completion section retains that architecture, but imposes stricter domain, regularity, and status discipline so that later bundle, spinor, connection, and field-content constructions can depend on it without hidden assumptions. This is consistent with the completion framework, which designates observerse, pullback geometry, native versus invasive fields, and open questions on existence and regularity as core elements of the formal foundation.  

### 8.1 Setup and purpose

The observerse is introduced in the draft as a two-space framework ((X,Y,\iota)) intended to support a more fundamental role for observation and measurement, and to allow different classes of fields to live natively on different spaces. The draft’s guiding idea is that physics may occur primarily on (Y), while observed structures on (X) arise through pullback and are liable to be misinterpreted as native to (X).  This section does not treat those interpretive claims as proved mathematics. It extracts the formal core needed later: the triple structure, the admissible class of observation maps, the pullback mechanism, and the native/invasive field distinction. Any strengthening beyond the draft is labeled as an inserted assumption or inserted convention, as required by the completion methodology.  

### 8.2 Primitive data

We retain the minimal base-space posture of the draft.

**Axiom 8.2.1.**
(X) is a smooth, oriented, spin 4-manifold. No metric on (X) is assumed at the primitive stage. 

This axiom is upstream of the observerse rather than part of its definition, but it is included here because the draft builds the observerse specifically as a replacement for treating (X) as already endowed with ordinary space-time geometry. 

### 8.3 Formal definition of observerse

The draft’s Definition 3.1 gives the observerse as a triple ((X^n,Y^d,{\iota})) with local maps (\iota:U_x^n\to Y^d) that are local Riemannian embeddings into a Riemannian manifold (Y), inducing a pullback metric on (X) and a normal bundle. It also distinguishes the trivial, Einsteinian, and ambient cases.  The present completion document adopts the following normalized form.

**Definition 8.3.1 (Observerse).**
An observerse is a triple
[
\mathcal O=(X,Y,\mathcal I),
]
where:

1. (X) is the fixed base manifold from Axiom 8.2.1.
2. (Y) is a smooth manifold, or more generally a smooth fibered space over (X) once the Einsteinian case is specialized.
3. (\mathcal I) is a specified admissible class of local observation maps
   [
   \iota:U\to Y,
   ]
   with (U\subseteq X) open.

Each admissible (\iota) is understood to supply the local observational bridge between (X) and (Y). When the target (Y) carries appropriate geometric data, (\iota) also induces corresponding pullback data on (U).

This definition is deliberately broader than a single fixed map. The draft writes ((X^n,Y^d,{\iota})) rather than ((X^n,Y^d,\iota)), which indicates that a family or class of admissible observations is intended rather than a unique map. 

**Inserted Convention 8.3.2.**
We write (\mathcal I) for the admissible class of observation maps and (\iota\in\mathcal I) for an individual observation map. This replaces the draft’s mixed use of set-braced and single-map notation.

This convention is added because later statements about “different observations via different sections (\iota)” require a clean distinction between the class of maps and a chosen representative. 

### 8.4 Admissible observation maps

The draft definition imposes several conditions on (\iota): locality, openness of the domain, embedding character, metric pullback, and normal-bundle production. But it does not globally separate which of these are definitional across all observerse and which are specific to the metricized cases.  The completion document therefore normalizes admissibility as follows.

**Definition 8.4.1 (Admissible observation map, weak form).**
Let (\mathcal O=(X,Y,\mathcal I)) be an observerse. A map (\iota:U\to Y), with (U\subseteq X) open, is called an admissible observation map if:

1. (\iota) is smooth on (U).
2. (\iota) is an immersion on (U).
3. the image (\iota(U)\subset Y) lies in a portion of (Y) on which the geometric objects intended for pullback are defined.

This weak form is the minimum needed across all cases.

**Definition 8.4.2 (Admissible observation map, metricized form).**
If (Y) carries a metric (g_Y) on an open neighborhood of (\iota(U)), then (\iota) is metrically admissible if in addition:

1. (\iota) is a local Riemannian or semi-Riemannian embedding on (U),
2. the pullback tensor
   [
   g_X^{(\iota)}:=\iota^*(g_Y)
   ]
   is nondegenerate on (U),
3. the normal bundle (N_\iota) along (\iota(U)) is defined as a smooth subbundle of (TY|_{\iota(U)}), with the induced metric from (g_Y).

This is the metricized version closest to the draft’s Definition 3.1. 

**Inserted Assumption 8.4.3 (Regularity of admissible maps).**
For the remainder of the completion document, all admissible observation maps are assumed (C^\infty).

The draft uses smooth manifold language but does not explicitly restate regularity at each later dependency point. This assumption is inserted so that pullback, bundle restriction, induced connection, and spinor constructions can proceed uniformly.

**Inserted Assumption 8.4.4 (Nondegeneracy on the observed domain).**
Whenever a later construction uses a pullback metric (g_X^{(\iota)}), it is assumed that the relevant (\iota) has been restricted to an open set (U\subseteq X) on which (g_X^{(\iota)}) is nondegenerate.

This is implicit in the draft’s use of local Riemannian embedding language, but it must be explicit for downstream formal work. 

**Inserted Assumption 8.4.5 (Locality-first formalism).**
Unless explicitly stated otherwise, all observerse constructions are local on (X).

This assumption is necessary because the draft defines (\iota) on local open neighborhoods (U_x\subset X), while several later bundle constructions are described globally or semi-globally without proving their global existence. 

### 8.5 Pullback geometry and observation

The draft makes pullback central to the observerse. In the metricized cases, the observation map induces a metric on (X) by (g_X=\iota^*(g_Y)), and it also induces a normal bundle and associated pullback structures.  Later, in the Einsteinian observerse, the draft goes further and states that the choice of section (\iota) induces a metric on (Y) in the portion lying above (U\subset X), and that observation has a strong geometric effect, including metric transfer, splitting data, and induced connection data.  

**Definition 8.5.1 (Observation / pullback operation).**
Given an admissible observation map (\iota:U\to Y), the associated observation operator is the pullback
[
\iota^*:\mathcal T(Y)|_{\iota(U)}\to \mathcal T(U),
]
defined on whatever class (\mathcal T) of tensorial, bundle-valued, or field objects is under consideration and for which pullback is well-defined.

This is intentionally abstract at first. It covers metrics, differential forms, sections of pulled-back bundles, and later observed fields.

**Inserted Convention 8.5.2.**
When ambiguity is possible, observed objects on (U\subseteq X) induced by (\iota) carry an upper index ((\iota)), for example (g_X^{(\iota)}), (\chi_X^{(\iota)}), or (N_\iota).

This notation is added because the draft explicitly allows different observation choices (\iota), and later statements would otherwise suppress that dependence. 

**Inserted Assumption 8.5.3 (Functorial admissibility domain).**
Whenever a field (\chi) on (Y) is said to be observed on (X), it is assumed either that (\chi) is defined on a neighborhood of (\iota(U)) or that (\chi) is first restricted to such a neighborhood.

This assumption is minor but necessary, because the draft uses pullback language freely without repeatedly specifying the neighborhood conditions for each field class.

### 8.6 The three principal cases

The draft identifies three main observerse cases: trivial, Einsteinian, and ambient.  The completion document preserves these, but separates clean extraction from inserted completion structure.

**Definition 8.6.1 (Trivial observerse).**
The trivial observerse is the case (Y=X) with (\iota=\mathrm{id}_X). It recovers the ordinary one-space viewpoint as a degenerate or baseline case. 

**Definition 8.6.2 (Einsteinian observerse).**
The Einsteinian observerse is the case in which (Y=\mathrm{Met}(X)), interpreted in the draft as the bundle of pointwise metric tensors over (X), and admissible observation maps are sections (\iota=g) of this bundle representing Riemannian or semi-Riemannian metric fields on (X). 

**Inserted Assumption 8.6.3 (Model of the metric bundle).**
For formal completion, (\mathrm{Met}(X)) is taken to mean the open subbundle of (\mathrm{Sym}^2(T^*X)) whose fibers consist of nondegenerate symmetric bilinear forms of the allowed signatures under consideration.

The draft clearly intends the bundle of pointwise metrics, but does not normalize its ambient model. This assumption makes the Einsteinian observerse mathematically usable downstream. 

**Inserted Assumption 8.6.4 (Signature-sector restriction).**
Unless explicitly stated otherwise, later constructions in the strong-form completion are restricted to a fixed admissible signature sector of (\mathrm{Met}(X)).

This is necessary because the draft discusses multiple signatures but repeatedly privileges the physically relevant four-dimensional Lorentzian-type sector in later developments. 

**Definition 8.6.5 (Ambient observerse).**
The ambient observerse is the case in which (Y) is otherwise unconstrained beyond the immersion character of admissible maps (\iota). 

**Remark 8.6.6.**
The completion document treats the Einsteinian observerse as the main working case for formal development, matching the draft’s statement that it adopts this “strongest assumption beyond those of Einstein” for the search for new physics. 

### 8.7 Native and invasive fields

The draft’s Definition 3.2 is one of the clearest formal distinctions in the observerse chapter: fields can be native to (X) or (Y), and fields on (X) obtained by pullback from (Y) are invasive.  This distinction is central and should be preserved verbatim in spirit but normalized in notation.

**Definition 8.7.1 (Native field).**
Let (E\to X) and (F\to Y) be smooth bundles. A section (\sigma_X\in\Gamma(E)) is native to (X). A section (\sigma_Y\in\Gamma(F)) is native to (Y).

**Definition 8.7.2 (Invasive field on (X)).**
Let (\iota:U\to Y) be an admissible observation map, and let (\sigma_Y) be a field on (Y) of a type for which pullback to (U) is defined. Then the field
[
\sigma_X^{(\iota)}:=\iota^*(\sigma_Y)
]
on (U) is called invasive on (X).

This matches the source distinction between native sections and pullback-generated invasive fields. 

**Inserted Convention 8.7.3.**
The phrases “native to (X),” “native to (Y),” and “invasive on (X)” are ontological labels inside the formalism, not statements about physical observability or fundamentality by themselves.

This convention is needed because the draft often moves quickly from formal origin statements to physical interpretation. The completion document separates those layers. 

**Remark 8.7.4.**
The draft explicitly suggests that, within the intended theory, most physics may happen on (Y), while (X) supports a much smaller set of truly native structures, possibly only one independent primary field in the strong Einsteinian setup. This is recorded here as motivation, not as a theorem of the formalism.  

### 8.8 Observerse in the Einsteinian case

The draft’s strongest geometric use of the observerse appears in the Einsteinian case. There (Y=\mathrm{Met}(X)), the total space carries tangent and cotangent geometry, a horizontal/vertical splitting structure, and later the chimeric bundle, topological spinors, and an induced connection mechanism.    This observerse section stops before fully formalizing those later constructions, but it must state the dependency clearly.

**Proposition 8.8.1 (Observerse dependency note).**
In the completion document, the following later constructions depend on the observerse formalism:

1. the bundle projection (\pi:Y\to X) in the Einsteinian case,
2. the horizontal and vertical subbundles (H) and (V),
3. the chimeric bundle (C=V\oplus H^*),
4. topological-to-metric spinor comparison under observation,
5. induced metric and connection transfer on (Y).

This proposition is organizational rather than substantive; it records the dependency chain already visible in the draft and in the completion outline.  

### 8.9 Added assumptions beyond the draft

The following items have been added in the completion document beyond what is fully fixed in the draft. They are collected here explicitly because the user requested that all added assumptions be noted.

**Inserted Assumption 8.9.1.** All admissible observation maps are (C^\infty).

**Inserted Assumption 8.9.2.** Observerse constructions are local on (X) unless global existence is separately proved.

**Inserted Assumption 8.9.3.** Whenever pullback metric data are used, the observation map has been restricted so that the pullback metric is nondegenerate on its domain.

**Inserted Assumption 8.9.4.** In the Einsteinian case, (\mathrm{Met}(X)) is modeled as an open subbundle of (\mathrm{Sym}^2(T^*X)) consisting of nondegenerate symmetric bilinear forms.

**Inserted Assumption 8.9.5.** A fixed admissible signature sector is chosen whenever downstream constructions require one.

**Inserted Convention 8.9.6.** (\mathcal I) denotes the admissible class of observation maps, while (\iota) denotes a chosen map.

**Inserted Convention 8.9.7.** Observed objects induced by (\iota) carry explicit dependence notation when needed, such as (g_X^{(\iota)}).

**Inserted Convention 8.9.8.** “Native” and “invasive” are formal origin labels, not automatically physical conclusions.

These insertions are not arbitrary embellishments. They repair exactly the kinds of gaps the completion document is meant to expose: undefined globality, hidden regularity requirements, suppressed dependence on the chosen observation, and unnormalized modeling of the metric bundle.  

### 8.10 Unresolved issues and proof obligations

Several important points remain unresolved at this stage and should be tracked as open obligations rather than silently assumed complete.

First, the draft does not yet provide a full global existence theory for admissible observation maps, especially in the ambient case. Second, the Einsteinian observerse requires a normalized treatment of the metric bundle, signature sectors, and possible global topological obstructions. Third, the exact scope of pullback admissibility for all later field classes is not fully formalized in the draft. Fourth, the later claims that observation induces not just pullback geometry on (X) but also metric and connection data on (Y) require a separate formal statement and proof in the induced-connection chapter.   

For the purposes of the completion document, however, the observerse formalism is now stable enough to support the next sections on proto-Riemannian geometry, horizontal and vertical structures, and the chimeric bundle construction. 

I can write the next section as **Proto-Riemannian Geometry** in the same style, with the exact sequences, (H), (V), and the Frobenius metric made explicit.
