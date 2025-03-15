using System;

namespace RepoSteamNetworking.API.VersionCompat;

[Serializable]
public enum VersionCompatibility
{
    /// <summary>
    /// The versions must match, This is the default behaviour and does not need to be specified.
    /// </summary>
    Strict,
    
    /// <summary>
    /// The major, minor, and patch versions can be different. This is assuming versioning follows (Major.Minor.Patch)
    /// <example>
    /// '1.0.1' and '0.9.0' are compatible, when set.
    /// </example>
    /// </summary>
    Any,
    
    /// <summary>
    /// The minor and patch versions can be different, but not the major version. This is assuming versioning follows (Major.Minor.Patch)
    /// <example>
    /// '1.2.1' and '1.1.0' are compatible, but not '1.2.1' and '2.0.0'.
    /// </example>
    /// </summary>
    Minor,
    
    /// <summary>
    /// The patch version can be different, but not the major or minor version. This is assuming versioning follows (Major.Minor.Patch)
    /// <example>
    /// '1.2.0' and '1.2.3' are compatible, but not '1.2.1' and '1.3.0' or '1.2.1' and '2.0.0'.
    /// </example>
    /// </summary>
    Patch
}

public static class VersionCompatibilityExtensions
{
    public static string CreateVersionRequirementString(this VersionCompatibility compatibility, System.Version version)
    {
        return compatibility switch
        {
            VersionCompatibility.Strict => $"version {version.Major}.{version.Minor}.{version.Build}",
            VersionCompatibility.Any => "any version",
            VersionCompatibility.Minor => $"a version greater than or equal to {version.Major}.0.0 and less than {version.Major + 1}.0.0",
            VersionCompatibility.Patch => $"a version greater than or equal to {version.Major}.{version.Minor}.0 and less than {version.Major}.{version.Minor + 1}.0",
            _ => throw new ArgumentOutOfRangeException(nameof(compatibility), compatibility, null)
        };
    }
}