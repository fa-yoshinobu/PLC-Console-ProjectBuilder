using System.Text.Json;
using PlcIoCheckerQr.Wpf;

namespace PlcIoCheckerQr.Wpf.Tests;

public sealed class ProjectJsonReaderTests
{
    [Fact]
    public void RequiresCurrentSchemaIdentifierWithoutLegacyAlias()
    {
        using var current = JsonDocument.Parse(
            """{"schema":"plc-console-project","schemaVersion":2}""");
        ProjectJsonReader.RequireProjectJsonV2(current.RootElement);

        using var legacy = JsonDocument.Parse(
            """{"schema":"plc-io-checker-project","schemaVersion":2}""");
        var exception = Assert.Throws<ProjectJsonException>(
            () => ProjectJsonReader.RequireProjectJsonV2(legacy.RootElement));

        Assert.Equal("error.jsonSchemaInvalid", exception.LocalizationKey);
    }

    [Fact]
    public void ReadsConditionalDeviceMetaShapeWithoutInferringDataType()
    {
        using var document = JsonDocument.Parse(
            """{"deviceMeta":[{"address":"D100","dataType":"INT16"},{"address":"M0","comment":"Comment only"}]}""");

        var result = ProjectJsonReader.ReadDeviceMetaByAddress(
            document.RootElement,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "D100" },
            NormalizeAddress);

        Assert.Equal("INT16", result["D100"].DataType);
        Assert.Null(result["M0"].DataType);
        Assert.Equal("Comment only", result["M0"].Comment);
    }

    [Theory]
    [InlineData(
        """{"deviceMeta":[{"address":"D100","comment":"Referenced"}]}""",
        "D100",
        "is required for referenced")]
    [InlineData(
        """{"deviceMeta":[{"address":"D100","dataType":null,"comment":"Referenced"}]}""",
        "D100",
        "must be a string")]
    [InlineData(
        """{"deviceMeta":[{"address":"D100","dataType":"INT16","comment":"Comment only"}]}""",
        "",
        "must be omitted for comment-only")]
    [InlineData(
        """{"deviceMeta":[{"address":"D100","dataType":null,"comment":"Comment only"}]}""",
        "",
        "must be omitted for comment-only")]
    [InlineData(
        """{"deviceMeta":[{"address":"D100","comment":"   "}]}""",
        "",
        "must not be empty for comment-only")]
    public void RejectsWrongConditionalDeviceMetaShape(
        string json,
        string referencedAddress,
        string expectedMessage)
    {
        using var document = JsonDocument.Parse(json);
        var referenced = string.IsNullOrEmpty(referencedAddress)
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase) { referencedAddress };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ProjectJsonReader.ReadDeviceMetaByAddress(
                document.RootElement,
                referenced,
                NormalizeAddress));

        Assert.Contains(expectedMessage, exception.Message);
    }

    private static string NormalizeAddress(string value) => value.Trim().ToUpperInvariant();
}
