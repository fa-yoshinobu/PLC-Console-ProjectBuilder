using System.Text.Json;
using PLCConsoleProjectBuilder.Core;

namespace PLCConsoleProjectBuilder.Core.Tests;

public sealed class ProjectFactoryTests
{
    [Theory]
    [InlineData("Melsec", "Normal", "MELSEC iQ-R (built-in)", " d001 ", "D1")]
    [InlineData("Melsec", "Normal", "MELSEC iQ-R (built-in)", "x00f", "XF")]
    [InlineData("Melsec", "Normal", "MELSEC iQ-F (built-in)", "x010", "X10")]
    [InlineData("Keyence", "Normal", "KEYENCE KV-8000", "r001", "R001")]
    [InlineData("Keyence", "Xym", "KEYENCE KV-8000 (XYM)", "x03f", "X3F")]
    public void NormalizeDeviceAddressFormatsAddressLikeMobileApps(
        string vendor,
        string keyenceDeviceMode,
        string machineLabel,
        string input,
        string expected)
    {
        Assert.Equal(expected, ProjectFactory.NormalizeDeviceAddress(input, vendor, keyenceDeviceMode, machineLabel));
    }

    [Fact]
    public void MakeProjectNormalizesAddressesInAllInputSections()
    {
        var project = ProjectFactory.MakeProject(ProjectInputBuilder.MakeInput(
            DevicesText: "d001,Int16",
            WatchText: "x00f,Bit",
            TrapsText: "stc000,Bit,Rise,,true",
            CommentsText: "sd000,,System word"));

        Assert.Equal("D1", project.Devices.Single().Address);
        Assert.Equal("XF", project.TimeChart.Single().Address);
        Assert.Equal("STC0", project.Traps.Single().Address);
        Assert.Equal("SD0", project.Comments.Single().Address);
        Assert.Equal("", project.Comments.Single().DataType);
    }

    [Theory]
    [InlineData("Melsec", "Normal", "MELSEC iQ-R (built-in)", "XFF", true)]
    [InlineData("Melsec", "Normal", "MELSEC iQ-R (built-in)", "STC0", true)]
    [InlineData("Melsec", "Normal", "MELSEC iQ-R (built-in)", "SD0", false)]
    [InlineData("Melsec", "Normal", "MELSEC iQ-R (built-in)", "SWFF", false)]
    [InlineData("Melsec", "Normal", "MELSEC iQ-F (built-in)", "X77", true)]
    [InlineData("Keyence", "Normal", "KEYENCE KV-8000", "R015", true)]
    [InlineData("Keyence", "Normal", "KEYENCE KV-8000", "CM100", false)]
    [InlineData("Keyence", "Normal", "KEYENCE KV-8000", "DM100", false)]
    [InlineData("Keyence", "Xym", "KEYENCE KV-8000 (XYM)", "X39F", true)]
    [InlineData("Keyence", "Xym", "KEYENCE KV-8000 (XYM)", "M100", true)]
    [InlineData("Keyence", "Xym", "KEYENCE KV-8000 (XYM)", "D100", false)]
    public void BitDetectionIsVendorModeAndModelAware(
        string vendor,
        string keyenceDeviceMode,
        string machineLabel,
        string address,
        bool expectedIsBit)
    {
        Assert.Equal(expectedIsBit, ProjectFactory.IsBitAddress(address, vendor, keyenceDeviceMode, machineLabel));
    }

    [Theory]
    [InlineData("Keyence", "Normal", "KEYENCE KV-8000", "DM65535", "Int16")]
    [InlineData("Keyence", "Normal", "KEYENCE KV-8000", "B8000", "Bit")]
    [InlineData("Keyence", "Normal", "KEYENCE KV-8000", "R200000", "Bit")]
    [InlineData("Keyence", "Xym", "KEYENCE KV-8000 (XYM)", "X20000", "Bit")]
    [InlineData("Keyence", "Xym", "KEYENCE KV-8000 (XYM)", "M64000", "Bit")]
    [InlineData("Keyence", "Xym", "KEYENCE KV-8000 (XYM)", "L16000", "Bit")]
    public void MakeProjectAllowsAddressesPastOfflineCpuProfileRanges(
        string vendor,
        string keyenceDeviceMode,
        string machineLabel,
        string address,
        string dataType)
    {
        var project = ProjectFactory.MakeProject(ProjectInputBuilder.MakeInput(
            Vendor: vendor,
            KeyenceDeviceMode: keyenceDeviceMode,
            MachineLabel: machineLabel,
            DevicesText: $"{address},{dataType}",
            WatchText: "",
            TrapsText: ""));

        Assert.Equal(address, project.Devices.Single().Address);
    }

    [Theory]
    [InlineData("Melsec", "Normal", "MELSEC iQ-R (built-in)", "D2147483648")]
    [InlineData("Melsec", "Normal", "MELSEC iQ-R (built-in)", "W80000000")]
    [InlineData("Keyence", "Normal", "KEYENCE KV-8000", "R2147483700")]
    [InlineData("Keyence", "Xym", "KEYENCE KV-8000 (XYM)", "X1342177280")]
    public void MakeProjectRejectsAddressesPastCommonTechnicalLimit(
        string vendor,
        string keyenceDeviceMode,
        string machineLabel,
        string address)
    {
        Assert.Throws<ArgumentException>(() =>
            ProjectFactory.ValidateDeviceAddress(address, vendor, keyenceDeviceMode, machineLabel));
    }

    [Theory]
    [InlineData("D2147483647,UInt32")]
    [InlineData("D2147483647,Float32")]
    [InlineData("W7FFFFFFF,Int32")]
    public void MakeProjectRejectsMultiwordDataPastCommonTechnicalLimit(string devicesText)
    {
        var exception = Assert.Throws<ArgumentException>(() => ProjectFactory.MakeProject(ProjectInputBuilder.MakeInput(
            DevicesText: devicesText,
            WatchText: "",
            TrapsText: "")));

        Assert.Contains("technical device address limit", exception.Message);
    }

    [Fact]
    public void MakeProjectAllowsMultiwordDataEndingAtCommonTechnicalLimit()
    {
        var project = ProjectFactory.MakeProject(ProjectInputBuilder.MakeInput(
            DevicesText: "D2147483646,UInt32",
            WatchText: "",
            TrapsText: ""));

        Assert.Equal("D2147483646", project.Devices.Single().Address);
        Assert.Equal("UInt32", project.Devices.Single().DataType);
    }

    [Fact]
    public void DataTypeChoicesExcludeMultiwordTypesAtCommonTechnicalLimit()
    {
        Assert.Equal(
            ["Int16", "UInt16"],
            ProjectFactory.DeviceDataTypesForAddress("D2147483647", "Melsec"));
    }

    [Fact]
    public void UnknownAddressFamilyKeepsDataTypeChoicesOpenUntilValidated()
    {
        Assert.False(ProjectFactory.IsBitAddress("DFFFF", "Melsec", "Normal", "MELSEC iQ-R (built-in)"));
        Assert.Equal(ProjectFactory.DeviceDataTypes, ProjectFactory.DeviceDataTypesForAddress("DFFFF", "Melsec"));
        Assert.Equal(ProjectFactory.DeviceDataTypes, ProjectFactory.DeviceDataTypesForAddress("", "Melsec"));
    }

    [Fact]
    public void TrapConditionListsFollowAddressKind()
    {
        Assert.Equal(ProjectFactory.TrapConditions, ProjectFactory.TrapConditionsForAddress("", "Melsec"));

        Assert.Equal(ProjectFactory.BitTrapConditions, ProjectFactory.TrapConditionsForAddress("X0", "Melsec"));

        Assert.Equal(ProjectFactory.WordTrapConditions, ProjectFactory.TrapConditionsForAddress("D0", "Melsec"));
    }

    [Fact]
    public void TrapConditionValidationRejectsUnsupportedConditionForAddressKind()
    {
        Assert.Equal("Fall", ProjectFactory.ValidateTrapConditionForAddress("X0", "Fall", "Melsec"));
        Assert.Equal("LessOrEqual", ProjectFactory.ValidateTrapConditionForAddress("D0", "LessOrEqual", "Melsec"));

        var bitException = Assert.Throws<ArgumentException>(() =>
            ProjectFactory.ValidateTrapConditionForAddress("X0", "GreaterOrEqual", "Melsec"));
        Assert.Contains("Invalid trap condition for X0", bitException.Message);

        var wordException = Assert.Throws<ArgumentException>(() =>
            ProjectFactory.ValidateTrapConditionForAddress("D0", "Rise", "Melsec"));
        Assert.Contains("Invalid trap condition for D0", wordException.Message);
    }

    [Theory]
    [InlineData("Rise", false)]
    [InlineData("Fall", false)]
    [InlineData("Change", false)]
    [InlineData("GreaterOrEqual", true)]
    [InlineData("LessOrEqual", true)]
    [InlineData("Equal", true)]
    [InlineData("NotEqual", true)]
    public void TrapConditionThresholdRequirementIsConditionSpecific(string condition, bool requiresThreshold)
    {
        Assert.Equal(requiresThreshold, ProjectFactory.TrapConditionRequiresThreshold(condition));
    }

    [Fact]
    public void SlugifyLowercasesAndKeepsFallbackForEmptySlug()
    {
        Assert.Equal("plc-console-2026", ProjectFactory.Slugify(" PLC Console!! 2026 "));
        Assert.Equal("plc-project", ProjectFactory.Slugify("###"));
        Assert.Equal("custom", ProjectFactory.Slugify("   ", fallback: "custom"));
    }

    [Fact]
    public void MakeProjectNormalizesNameIdAndDistinctWatchAddresses()
    {
        var project = ProjectFactory.MakeProject(ProjectInputBuilder.MakeInput(
            Name: "  ",
            DevicesText: "d100,uint16,Speed\r\nx0,Bit,Start",
            WatchText: "D100,UInt16\r\nd100,UInt16\r\nX0,Bit",
            TrapsText: "D100,UInt16,GreaterOrEqual,12.5,false"),
            nowEpochMs: 456);

        Assert.Equal("PLC QR Project", project.Name);
        Assert.Equal("plc-qr-project-456", project.Id);
        Assert.Equal(["D100", "X0"], project.TimeChart.Select(target => target.Address).ToArray());
        Assert.Equal("UInt16", project.Devices[0].DataType);
        Assert.Equal(12.5, project.Traps.Single().Threshold);
        Assert.False(project.Traps.Single().Enabled);
    }

    [Fact]
    public void MakeProjectPreservesExplicitProjectAndTrapIds()
    {
        var project = ProjectFactory.MakeProject(ProjectInputBuilder.MakeInput(
            ProjectId: "project-imported",
            TrapsText: "D100,Int16,GreaterOrEqual,12,true",
            TrapIds: ["trap-imported"]),
            nowEpochMs: 456);

        Assert.Equal("project-imported", project.Id);
        Assert.Equal("trap-imported", project.Traps.Single().Id);
    }

    [Fact]
    public void TimingRangesDoNotRequireTimeoutToExceedPollingInterval()
    {
        var project = ProjectFactory.MakeProject(ProjectInputBuilder.MakeInput(
            MonitorIntervalMs: ProjectFactory.MaxPollingIntervalMs,
            TimeoutMs: ProjectFactory.MinTimeoutMs));

        Assert.Equal(ProjectFactory.MaxPollingIntervalMs, project.Connection.MonitorIntervalMs);
        Assert.Equal(ProjectFactory.MinTimeoutMs, project.Connection.TimeoutMs);
    }

    [Theory]
    [InlineData(99, 250)]
    [InlineData(10001, 250)]
    [InlineData(100, 249)]
    [InlineData(100, 10001)]
    public void MakeProjectRejectsTimingOutsideCommonRanges(int pollingIntervalMs, int timeoutMs)
    {
        Assert.Throws<ArgumentException>(() => ProjectFactory.MakeProject(ProjectInputBuilder.MakeInput(
            MonitorIntervalMs: pollingIntervalMs,
            TimeoutMs: timeoutMs)));
    }

    [Theory]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("32768")]
    public void MakeProjectRejectsInvalidInt16TrapThreshold(string threshold)
    {
        Assert.ThrowsAny<Exception>(() => ProjectFactory.MakeProject(ProjectInputBuilder.MakeInput(
            TrapsText: $"D100,Int16,GreaterOrEqual,{threshold},true")));
    }

    [Fact]
    public void MakeProjectRejectsDeviceCountAboveCommonLimit()
    {
        var devices = string.Join("\n", Enumerable.Range(0, ProjectFactory.MaxDevices + 1).Select(index => $"D{index},Int16"));
        Assert.Throws<ArgumentException>(() => ProjectFactory.MakeProject(ProjectInputBuilder.MakeInput(
            DevicesText: devices)));
    }

    [Fact]
    public void MakeProjectAllowsCommentOnlyMetadataBeyondReferencedLimit()
    {
        var comments = string.Join("\n", Enumerable.Range(0, ProjectFactory.MaxReferencedDeviceMeta + 1)
            .Select(index => $"D{index},,Comment {index}"));

        var project = ProjectFactory.MakeProject(ProjectInputBuilder.MakeInput(
            DevicesText: "",
            CommentsText: comments));

        Assert.Equal(ProjectFactory.MaxReferencedDeviceMeta + 1, project.Comments.Count);
        using var document = JsonDocument.Parse(ProjectQrPayload.ProjectQrJsonBytes(project));
        Assert.Equal(
            ProjectFactory.MaxReferencedDeviceMeta + 1,
            document.RootElement.GetProperty("deviceMeta").GetArrayLength());
        Assert.All(
            document.RootElement.GetProperty("deviceMeta").EnumerateArray(),
            meta => Assert.False(meta.TryGetProperty("dataType", out _)));
        Assert.NotEmpty(ProjectQrPayload.EncodeProjectChunks(project, chunkSize: 1_000));
    }

    [Fact]
    public void MakeProjectRejectsBlankCommentOnlyMetadata()
    {
        var exception = Assert.Throws<ArgumentException>(() => ProjectFactory.MakeProject(ProjectInputBuilder.MakeInput(
            DevicesText: "",
            CommentsText: "D100,,")));

        Assert.Contains("requires a non-empty comment", exception.Message);
    }

    [Fact]
    public void MakeProjectRejectsDataTypeOnCommentOnlyMetadata()
    {
        var exception = Assert.Throws<ArgumentException>(() => ProjectFactory.MakeProject(ProjectInputBuilder.MakeInput(
            DevicesText: "",
            CommentsText: "D100,Int16,Comment only")));

        Assert.Contains("must omit the data type", exception.Message);
    }

    [Fact]
    public void DeviceMetaCompositionUsesSeparateReferencedAndCommentOnlyLimits()
    {
        var referenced = Enumerable.Range(0, ProjectFactory.MaxReferencedDeviceMeta + 1)
            .Select(index => $"D{index}");
        Assert.Contains("Referenced device metadata", Assert.Throws<ArgumentException>(() =>
            ProjectFactory.ValidateDeviceMetaComposition(referenced, [])).Message);

        var commentOnly = Enumerable.Range(0, ProjectFactory.MaxCommentOnlyDeviceMeta + 1)
            .Select(index => new DeviceCommentDefinition($"D{index}", "", $"Comment {index}"));
        Assert.Contains("Comment-only device metadata", Assert.Throws<ArgumentException>(() =>
            ProjectFactory.ValidateDeviceMetaComposition([], commentOnly)).Message);
    }

}
