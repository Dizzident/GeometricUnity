namespace Gu.Branching.Conventions;

/// <summary>
/// Registry of all basis, ordering, adjoint, pairing, and norm conventions (Section 11).
/// Conventions must be declared before a run can start.
/// </summary>
public sealed class ConventionRegistry
{
    private readonly Dictionary<string, ConventionDescriptor> _conventions = new();

    /// <summary>All registered conventions.</summary>
    public IReadOnlyDictionary<string, ConventionDescriptor> All => _conventions;

    /// <summary>
    /// Register a convention. Throws if ID is already registered.
    /// </summary>
    public void Register(ConventionDescriptor convention)
    {
        ArgumentNullException.ThrowIfNull(convention);
        if (!_conventions.TryAdd(convention.ConventionId, convention))
        {
            throw new InvalidOperationException(
                $"Convention '{convention.ConventionId}' is already registered.");
        }
    }

    /// <summary>
    /// Look up a convention by ID.
    /// </summary>
    public ConventionDescriptor Get(string conventionId)
    {
        if (_conventions.TryGetValue(conventionId, out var convention))
            return convention;
        throw new KeyNotFoundException(
            $"Convention '{conventionId}' is not registered.");
    }

    /// <summary>
    /// Check whether a convention ID is registered.
    /// </summary>
    public bool Contains(string conventionId) => _conventions.ContainsKey(conventionId);

    /// <summary>
    /// Get all conventions in a given category.
    /// </summary>
    public IEnumerable<ConventionDescriptor> GetByCategory(string category) =>
        _conventions.Values.Where(c => c.Category == category);

    /// <summary>
    /// Creates a registry pre-populated with the default Minimal GU v1 conventions.
    /// </summary>
    public static ConventionRegistry CreateDefault()
    {
        var registry = new ConventionRegistry();

        registry.Register(new ConventionDescriptor
        {
            ConventionId = "basis-standard",
            Category = "basis",
            Description = "Standard ordered basis for finite-dimensional real Lie algebras.",
        });

        registry.Register(new ConventionDescriptor
        {
            ConventionId = "order-row-major",
            Category = "componentOrder",
            Description = "Row-major component ordering for tensor coefficients.",
        });

        registry.Register(new ConventionDescriptor
        {
            ConventionId = "adjoint-explicit",
            Category = "adjoint",
            Description = "Explicit adjoint representation. Transpose-equals-adjoint only when declared admissible.",
        });

        registry.Register(new ConventionDescriptor
        {
            ConventionId = "pairing-killing",
            Category = "pairing",
            Description = "Killing form pairing for Lie algebra inner product.",
        });

        registry.Register(new ConventionDescriptor
        {
            ConventionId = "pairing-trace",
            Category = "pairing",
            Description = "Trace pairing for Lie algebra inner product.",
        });

        registry.Register(new ConventionDescriptor
        {
            ConventionId = "norm-l2-quadrature",
            Category = "norm",
            Description = "L2 norm computed via quadrature weights and Lie algebra pairing.",
        });

        return registry;
    }
}
