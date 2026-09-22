namespace OutSystems.ExternalLibraries.RuntimeContext.Internal;

/// Pure URL host/subdomain extraction.
internal static class HostParser
{
    /// ODC supplies OUTSYSTEMS_RUNTIME_URL as a bare host ("acme-dev.outsystems.app").
    /// Uri.TryCreate with UriKind.Absolute rejects a schemeless string outright, and — worse —
    /// parses "acme-dev.outsystems.app:443" as scheme + path, yielding an EMPTY host while
    /// still returning true. Both cases are why the scheme is normalized before parsing.
    internal static string HostOf(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "";

        var trimmed = url.Trim();
        var candidate = trimmed.Contains("://", StringComparison.Ordinal)
            ? trimmed
            : "https://" + trimmed;

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri)) return "";

        var host = uri.Host;

        // An IPv6 literal is bracketed and has no meaningful leading label.
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
