using OutSystems.ExternalLibraries.RuntimeContext.Internal;
using Xunit;

namespace OutSystems.ExternalLibraries.RuntimeContext.Tests;

public class HostParserTests
{
    [Fact]
    public void BareHost_IsParsed()
    {
        // The documented ODC format. Uri.TryCreate(UriKind.Absolute) rejects this outright
        // without scheme normalization, so this is the primary regression guard.
        Assert.Equal("acme-dev.outsystems.app", HostParser.HostOf("acme-dev.outsystems.app"));
        Assert.Equal("acme-dev", HostParser.SubdomainOf("acme-dev.outsystems.app"));
    }

    [Fact]
    public void BareHostWithPort_DoesNotParsePortAsScheme()
    {
        // Without normalization Uri.TryCreate returns TRUE here with an EMPTY host,
        // treating "acme-dev.outsystems.app" as the scheme. Silent wrong answer.
        Assert.Equal("acme-dev.outsystems.app", HostParser.HostOf("acme-dev.outsystems.app:443"));
        Assert.Equal("acme-dev", HostParser.SubdomainOf("acme-dev.outsystems.app:443"));
    }

    [Theory]
    [InlineData("https://acme-dev.outsystems.app", "acme-dev")]
    [InlineData("https://acme-dev.outsystems.app/", "acme-dev")]
    [InlineData("https://acme-dev.outsystems.app/some/path", "acme-dev")]
    [InlineData("https://acme-dev.outsystems.app:443/path?q=1", "acme-dev")]
    [InlineData("http://acme-prd.outsystems.app", "acme-prd")]
    public void SchemedUrls_YieldTheLeadingLabel(string url, string expected)
    {
        Assert.Equal(expected, HostParser.SubdomainOf(url));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyInput_YieldsEmpty(string? url)
    {
        Assert.Equal("", HostParser.HostOf(url));
        Assert.Equal("", HostParser.SubdomainOf(url));
    }

    [Fact]
    public void SingleLabelHost_IsReturnedWhole()
    {
        Assert.Equal("localhost", HostParser.SubdomainOf("localhost"));
    }

    [Fact]
    public void IPv6Literal_HasNoSubdomain()
    {
        Assert.Equal("", HostParser.SubdomainOf("https://[::1]/path"));
    }

    [Fact]
    public void SurroundingWhitespace_IsTolerated()
    {
        Assert.Equal("acme-dev", HostParser.SubdomainOf("  acme-dev.outsystems.app  "));
    }
}
