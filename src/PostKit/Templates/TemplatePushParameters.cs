using JetBrains.Annotations;

namespace PostKit.Templates;

/// <summary>Represents parameters for pushing template changes between Postmark servers.</summary>
public sealed record TemplatePushParameters
{
    /// <summary>Gets the source server ID.</summary>
    public required long SourceServerId { [UsedImplicitly] get; init; }

    /// <summary>Gets the destination server ID.</summary>
    public required long DestinationServerId { [UsedImplicitly] get; init; }

    /// <summary>Gets whether Postmark should perform the changes instead of doing a dry run.</summary>
    public required bool PerformChanges { [UsedImplicitly] get; init; }
}
