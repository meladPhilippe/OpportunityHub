using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.ValueObjects;

namespace OpportunityHub.Domain.Tests.Entities;

public sealed class ChannelTests
{
    private const int Code = 100;
    private const int SortOrder = 1;

    private const string CreatedBy = "user-1";
    private const string UpdatedBy = "user-2";

    private static readonly DateTime CreatedAtUtc =
        new(
            2026,
            8,
            21,
            10,
            0,
            0,
            DateTimeKind.Utc);

    private static readonly DateTime UpdatedAtUtc =
        new(
            2026,
            8,
            21,
            11,
            0,
            0,
            DateTimeKind.Utc);

    #region Creation

    /// <summary>
    /// Verifies that a channel is created with the supplied values
    /// and is active by default.
    /// </summary>
    [Fact]
    public void Create_ShouldCreateChannel()
    {
        // Arrange
        var name = new LocalizedText(
            "Website",
            "الموقع الإلكتروني");

        // Act
        var channel = Channel.Create(
            Code,
            name,
            SortOrder,
            CreatedBy,
            CreatedAtUtc);

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            channel.Id);

        Assert.Equal(
            Code,
            channel.Code);

        Assert.Same(
            name,
            channel.Name);

        Assert.Equal(
            SortOrder,
            channel.SortOrder);

        Assert.True(channel.IsActive);

        Assert.Equal(
            CreatedBy,
            channel.CreatedBy);

        Assert.Equal(
            CreatedAtUtc,
            channel.CreatedAtUtc);

        Assert.Null(channel.UpdatedBy);
        Assert.Null(channel.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that channel creation uses the current UTC time
    /// when no creation timestamp is provided.
    /// </summary>
    [Fact]
    public void Create_ShouldUseCurrentUtcTime_WhenTimestampIsNotProvided()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var channel = Channel.Create(
            Code,
            new LocalizedText("Website"),
            SortOrder,
            CreatedBy);

        var after = DateTime.UtcNow;

        // Assert
        Assert.InRange(
            channel.CreatedAtUtc,
            before,
            after);
    }

    /// <summary>
    /// Verifies that creating a channel fails when the code is zero.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectZeroCode()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Channel.Create(
                0,
                new LocalizedText("Website"),
                SortOrder,
                CreatedBy,
                CreatedAtUtc));

        Assert.StartsWith(
  "Channel code must be greater than zero.",
  exception.Message);

        Assert.Equal(
            "code",
            exception.ParamName);
    }

    /// <summary>
    /// Verifies that creating a channel fails when the code is negative.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectNegativeCode()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Channel.Create(
                -1,
                new LocalizedText("Website"),
                SortOrder,
                CreatedBy,
                CreatedAtUtc));

        Assert.Equal(
            "code",
            exception.ParamName);
    }

    /// <summary>
    /// Verifies that creating a channel fails when the name is null.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectNullName()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            Channel.Create(
                Code,
                null!,
                SortOrder,
                CreatedBy,
                CreatedAtUtc));

        Assert.Equal(
            "name",
            exception.ParamName);
    }

    /// <summary>
    /// Verifies that creating a channel fails when the sort order is negative.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectNegativeSortOrder()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Channel.Create(
                Code,
                new LocalizedText("Website"),
                -1,
                CreatedBy,
                CreatedAtUtc));

        Assert.Equal(
            "sortOrder",
            exception.ParamName);
    }

    /// <summary>
    /// Verifies that creating a channel fails when the creator is empty.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectEmptyCreatedBy()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Channel.Create(
                Code,
                new LocalizedText("Website"),
                SortOrder,
                string.Empty,
                CreatedAtUtc));

        Assert.Equal(
            "createdBy",
            exception.ParamName);
    }

    #endregion

    #region State

    /// <summary>
    /// Verifies that an active channel can be deactivated
    /// and the update information is tracked.
    /// </summary>
    [Fact]
    public void Deactivate_ShouldDeactivateChannelAndTrackUpdate()
    {
        // Arrange
        var channel = CreateChannel();

        // Act
        channel.Deactivate(
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.False(channel.IsActive);

        Assert.Equal(
            UpdatedBy,
            channel.UpdatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            channel.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that an inactive channel can be activated
    /// and the update information is tracked.
    /// </summary>
    [Fact]
    public void Activate_ShouldActivateChannelAndTrackUpdate()
    {
        // Arrange
        var channel = CreateChannel();

        channel.Deactivate(
            UpdatedBy,
            UpdatedAtUtc);

        // Act
        channel.Activate(
            CreatedBy,
            CreatedAtUtc);

        // Assert
        Assert.True(channel.IsActive);

        Assert.Equal(
            CreatedBy,
            channel.UpdatedBy);

        Assert.Equal(
            CreatedAtUtc,
            channel.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that deactivating an already inactive channel
    /// still tracks the update.
    /// </summary>
    [Fact]
    public void Deactivate_ShouldTrackUpdate_WhenAlreadyInactive()
    {
        // Arrange
        var channel = CreateChannel();

        channel.Deactivate(
            CreatedBy,
            CreatedAtUtc);

        // Act
        channel.Deactivate(
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.False(channel.IsActive);

        Assert.Equal(
            UpdatedBy,
            channel.UpdatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            channel.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that activating an already active channel
    /// still tracks the update.
    /// </summary>
    [Fact]
    public void Activate_ShouldTrackUpdate_WhenAlreadyActive()
    {
        // Arrange
        var channel = CreateChannel();

        // Act
        channel.Activate(
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.True(channel.IsActive);

        Assert.Equal(
            UpdatedBy,
            channel.UpdatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            channel.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that deactivation fails when the updater is empty
    /// and leaves the channel unchanged.
    /// </summary>
    [Fact]
    public void Deactivate_ShouldRejectEmptyUpdatedBy()
    {
        // Arrange
        var channel = CreateChannel();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            channel.Deactivate(
                string.Empty,
                UpdatedAtUtc));

        Assert.Equal(
            "updatedBy",
            exception.ParamName);

        Assert.True(channel.IsActive);
        Assert.Null(channel.UpdatedBy);
        Assert.Null(channel.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that activation fails when the updater is empty
    /// and leaves the channel unchanged.
    /// </summary>
    [Fact]
    public void Activate_ShouldRejectEmptyUpdatedBy()
    {
        // Arrange
        var channel = CreateChannel();

        channel.Deactivate(
            CreatedBy,
            CreatedAtUtc);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            channel.Activate(
                string.Empty,
                UpdatedAtUtc));

        Assert.Equal(
            "updatedBy",
            exception.ParamName);

        Assert.False(channel.IsActive);
        Assert.Equal(
            CreatedBy,
            channel.UpdatedBy);

        Assert.Equal(
            CreatedAtUtc,
            channel.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that deactivation uses the current UTC time
    /// when no timestamp is provided.
    /// </summary>
    [Fact]
    public void Deactivate_ShouldUseCurrentUtcTime_WhenTimestampIsNotProvided()
    {
        // Arrange
        var channel = CreateChannel();

        var before = DateTime.UtcNow;

        // Act
        channel.Deactivate(UpdatedBy);

        var after = DateTime.UtcNow;

        // Assert
        Assert.False(channel.IsActive);

        Assert.InRange(
            channel.UpdatedAtUtc!.Value,
            before,
            after);

        Assert.Equal(
            UpdatedBy,
            channel.UpdatedBy);
    }

    /// <summary>
    /// Verifies that activation uses the current UTC time
    /// when no timestamp is provided.
    /// </summary>
    [Fact]
    public void Activate_ShouldUseCurrentUtcTime_WhenTimestampIsNotProvided()
    {
        // Arrange
        var channel = CreateChannel();

        channel.Deactivate(
            CreatedBy,
            CreatedAtUtc);

        var before = DateTime.UtcNow;

        // Act
        channel.Activate(UpdatedBy);

        var after = DateTime.UtcNow;

        // Assert
        Assert.True(channel.IsActive);

        Assert.InRange(
            channel.UpdatedAtUtc!.Value,
            before,
            after);

        Assert.Equal(
            UpdatedBy,
            channel.UpdatedBy);
    }

    #endregion

    #region Helpers

    private static Channel CreateChannel()
    {
        return Channel.Create(
            Code,
            new LocalizedText(
                "Website",
                "الموقع الإلكتروني"),
            SortOrder,
            CreatedBy,
            CreatedAtUtc);
    }

    #endregion
}