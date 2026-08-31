namespace SplaAtlas.Model.Json;

/// <summary>
/// A model file could not be read as the contract describes it.
/// </summary>
/// <remarks>
/// Raised only for damage the utility cannot work around — bytes that are not JSON, or a document
/// whose root is not an object. Anything the contract merely calls unusual (an unknown key, a
/// missing optional field, a <c>kind</c> nobody has seen before) is data, not an error: the registry
/// is hand-edited and the utility's job is to report drift, not to refuse to open the file.
/// </remarks>
public sealed class JsonModelException : Exception
{
    public JsonModelException(string message)
        : base(message)
    {
    }

    public JsonModelException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
