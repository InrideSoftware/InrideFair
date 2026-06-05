using InrideFair.Utils;

namespace InrideFair.Tests;

public class SignatureMatcherTests
{
    [Theory]
    [InlineData("injector.exe", "injector.exe", true)]
    [InlineData("my-injector.exe", "injector", false)]
    [InlineData("unix-tool.exe", "nix", false)]
    [InlineData("xone.exe", "xone", true)]
    [InlineData("xone_loader.exe", "xone", true)]
    public void MatchesFileName_UsesWordBoundariesForShortSignatures(string fileName, string signature, bool expected)
    {
        var result = SignatureMatcher.MatchesFileName(fileName, signature);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("cheatengine", "cheatengine", true)]
    [InlineData("some-cheatengine-host", "cheatengine", true)]
    [InlineData("legitimate-process", "nix", false)]
    public void MatchesText_HandlesProcessNames(string text, string signature, bool expected)
    {
        var result = SignatureMatcher.MatchesText(text, signature);

        Assert.Equal(expected, result);
    }
}
