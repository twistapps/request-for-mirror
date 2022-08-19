using RequestForMirror;

/// <summary>
///     Used as a mark for code generator.
/// </summary>
public interface IMarkedForCodeGen
{
    RequestStatus Ok { get; }
    RequestStatus Error { get; }
}