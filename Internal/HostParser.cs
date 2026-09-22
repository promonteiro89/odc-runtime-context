namespace OutSystems.ExternalLibraries.RuntimeContext.Internal;

internal static class HostParser
{
    // ODC supplies OUTSYSTEMS_RUNTIME_URL as a bare host, and Uri.TryCreate rejects a schemeless
    // string in UriKind.Absolute. A bare host carrying a port is worse: it parses as scheme plus
    // path and returns true with an empty Host. Hence the scheme is normalized before parsing.
    internal static string HostOf(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "";

        var trimmed = url.Trim();
        var candidate = trimmed.Contains("://", StringComparison.Ordinal)
            ? trimmed
            : "https://" + trimmed;

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri)) return "";

        var host = uri.Host;

        // An IPv6 literal is bracketed and has no leading label to take.
        if (string.IsNullOrEmpty(host) || host.StartsWith('[')) return "";

        return host;
    }

    internal static string SubdomainOf(string? url)
    {
        var host = HostOf(url);
        if (host.Length == 0) return "";

        int dot = host.IndexOf('.');
        return dot > 0 ? host[..dot] : host;
    }
}
