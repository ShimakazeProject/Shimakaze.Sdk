namespace Shimakaze.Sdk.Engine.Csf;

/// <summary>
/// Specifies the serialization format for CSF string table data.
/// </summary>
public enum CsfFormat
{
    /// <summary>
    /// Native binary CSF format.
    /// </summary>
    Csf,

    /// <summary>
    /// YAML (v1) text format.
    /// </summary>
    Yaml,

    /// <summary>
    /// JSON (v2) format.
    /// </summary>
    JsonV2,

    /// <summary>
    /// JSON (v1) format.
    /// </summary>
    JsonV1,

    /// <summary>
    /// XML (v1) text format.
    /// </summary>
    Xml,
}
