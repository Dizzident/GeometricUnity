using System.Reflection;
using System.Text.Json;
using Json.Schema;

namespace Gu.Validation;

/// <summary>
/// Validates JSON documents against GU JSON schemas (draft 2020-12).
/// Schemas are loaded from embedded resources or from file paths.
/// </summary>
public static class SchemaValidator
{
    private static readonly Dictionary<string, JsonSchema> SchemaCache = new();
    private static readonly object Lock = new();

    /// <summary>
    /// Known schema names that can be loaded from embedded resources.
    /// </summary>
    public static IReadOnlyList<string> KnownSchemas { get; } = new[]
    {
        "branch",
        "geometry",
        "artifact",
        "observed",
        "validation",
    };

    /// <summary>
    /// Validate a JSON string against a named schema (e.g. "branch", "geometry").
    /// </summary>
    public static SchemaValidationResult Validate(string json, string schemaName)
    {
        var schema = LoadEmbeddedSchema(schemaName);
        return ValidateAgainstSchema(json, schema, schemaName);
    }

    /// <summary>
    /// Validate a JSON string against a schema loaded from a file path.
    /// </summary>
    public static SchemaValidationResult ValidateWithSchemaFile(string json, string schemaFilePath)
    {
        var schema = LoadSchemaFromFile(schemaFilePath);
        return ValidateAgainstSchema(json, schema, Path.GetFileName(schemaFilePath));
    }

    /// <summary>
    /// Load a named schema from embedded resources.
    /// </summary>
    public static JsonSchema LoadEmbeddedSchema(string schemaName)
    {
        lock (Lock)
        {
            if (SchemaCache.TryGetValue(schemaName, out var cached))
                return cached;

            var resourceName = $"Gu.Validation.Schemas.{schemaName}.schema.json";
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new ArgumentException($"Unknown schema '{schemaName}'. Known schemas: {string.Join(", ", KnownSchemas)}");

            using var reader = new StreamReader(stream);
            var schemaText = reader.ReadToEnd();
            var schema = JsonSchema.FromText(schemaText);

            SchemaCache[schemaName] = schema;
            return schema;
        }
    }

    /// <summary>
    /// Load a schema from a file path. If a schema with the same $id is already
    /// registered (e.g. from embedded resources), the cached version is returned.
    /// </summary>
    public static JsonSchema LoadSchemaFromFile(string filePath)
    {
        var schemaText = File.ReadAllText(filePath);
        try
        {
            return JsonSchema.FromText(schemaText);
        }
        catch (Json.Schema.JsonSchemaException)
        {
            // Schema with this $id is already registered globally (e.g. from embedded resources).
            // Parse the $id and retrieve the registered schema.
            using var doc = JsonDocument.Parse(schemaText);
            if (doc.RootElement.TryGetProperty("$id", out var idElement))
            {
                var uri = new Uri(idElement.GetString()!);
                var existing = SchemaRegistry.Global.Get(uri);
                if (existing is JsonSchema existingSchema)
                    return existingSchema;
            }
            throw;
        }
    }

    private static SchemaValidationResult ValidateAgainstSchema(string json, JsonSchema schema, string schemaName)
    {
        JsonElement document;
        try
        {
            document = JsonDocument.Parse(json).RootElement;
        }
        catch (JsonException ex)
        {
            return new SchemaValidationResult(
                IsValid: false,
                SchemaName: schemaName,
                Errors: [$"Invalid JSON: {ex.Message}"]);
        }

        var options = new EvaluationOptions
        {
            OutputFormat = OutputFormat.List,
        };

        var result = schema.Evaluate(document, options);

        if (result.IsValid)
        {
            return new SchemaValidationResult(
                IsValid: true,
                SchemaName: schemaName,
                Errors: []);
        }

        var errors = new List<string>();
        CollectErrors(result, errors);

        return new SchemaValidationResult(
            IsValid: false,
            SchemaName: schemaName,
            Errors: errors);
    }

    private static void CollectErrors(EvaluationResults results, List<string> errors)
    {
        if (results.Errors is not null)
        {
            foreach (var error in results.Errors)
            {
                var location = results.InstanceLocation.ToString();
                errors.Add(string.IsNullOrEmpty(location)
                    ? $"{error.Key}: {error.Value}"
                    : $"{location}: {error.Key}: {error.Value}");
            }
        }

        if (results.Details is not null)
        {
            foreach (var detail in results.Details)
            {
                CollectErrors(detail, errors);
            }
        }
    }
}

/// <summary>
/// Result of a JSON schema validation.
/// </summary>
public sealed record SchemaValidationResult(
    bool IsValid,
    string SchemaName,
    IReadOnlyList<string> Errors);
