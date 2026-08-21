using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.ValueObjects;

namespace OpportunityHub.Domain.Tests.Entities;

public sealed class SectorTests
{
    private const int Code = 200;
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
    /// Verifies that a sector is created with the supplied values
    /// and is active by default.
    /// </summary>
    [Fact]
    public void Create_ShouldCreateSector()
    {
        // Arrange
        var name = new LocalizedText(
            "Technology",
            "التكنولوجيا");

        // Act
        var sector = Sector.Create(
            Code,
            name,
            SortOrder,
            CreatedBy,
            CreatedAtUtc);

        // Assert
        Assert.NotEqual(Guid.Empty, sector.Id);

        Assert.Equal(
            Code,
            sector.Code);

        Assert.Same(
            name,
            sector.Name);

        Assert.Equal(
            SortOrder,
            sector.SortOrder);

        Assert.True(sector.IsActive);

        Assert.Equal(
            CreatedBy,
            sector.CreatedBy);

        Assert.Equal(
            CreatedAtUtc,
            sector.CreatedAtUtc);

        Assert.Null(sector.UpdatedBy);
        Assert.Null(sector.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that sector creation uses the current UTC time
    /// when no creation timestamp is provided.
    /// </summary>
    [Fact]
    public void Create_ShouldUseCurrentUtcTime_WhenTimestampIsNotProvided()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var sector = Sector.Create(
            Code,
            new LocalizedText("Technology"),
            SortOrder,
            CreatedBy);

        var after = DateTime.UtcNow;

        // Assert
        Assert.InRange(
            sector.CreatedAtUtc,
            before,
            after);
    }

    /// <summary>
    /// Verifies that creating a sector fails when the code is zero.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectZeroCode()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Sector.Create(
                0,
                new LocalizedText("Technology"),
                SortOrder,
                CreatedBy,
                CreatedAtUtc));

        Assert.Equal(
            "code",
            exception.ParamName);

        Assert.StartsWith(
            "Sector code must be greater than zero.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that creating a sector fails when the code is negative.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectNegativeCode()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Sector.Create(
                -1,
                new LocalizedText("Technology"),
                SortOrder,
                CreatedBy,
                CreatedAtUtc));

        Assert.Equal(
            "code",
            exception.ParamName);
    }

    /// <summary>
    /// Verifies that creating a sector fails when the name is null.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectNullName()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            Sector.Create(
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
    /// Verifies that creating a sector fails when the sort order is negative.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectNegativeSortOrder()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Sector.Create(
                Code,
                new LocalizedText("Technology"),
                -1,
                CreatedBy,
                CreatedAtUtc));

        Assert.Equal(
            "sortOrder",
            exception.ParamName);

        Assert.StartsWith(
            "Sort order cannot be negative.",
            exception.Message);
    }

    /// <summary>
    /// Verifies that creating a sector fails when the creator is empty.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectEmptyCreatedBy()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Sector.Create(
                Code,
                new LocalizedText("Technology"),
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
    /// Verifies that an active sector can be deactivated
    /// and the update information is tracked.
    /// </summary>
    [Fact]
    public void Deactivate_ShouldDeactivateSectorAndTrackUpdate()
    {
        // Arrange
        var sector = CreateSector();

        // Act
        sector.Deactivate(
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.False(sector.IsActive);

        Assert.Equal(
            UpdatedBy,
            sector.UpdatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            sector.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that an inactive sector can be activated
    /// and the update information is tracked.
    /// </summary>
    [Fact]
    public void Activate_ShouldActivateSectorAndTrackUpdate()
    {
        // Arrange
        var sector = CreateSector();

        sector.Deactivate(
            UpdatedBy,
            UpdatedAtUtc);

        // Act
        sector.Activate(
            CreatedBy,
            CreatedAtUtc);

        // Assert
        Assert.True(sector.IsActive);

        Assert.Equal(
            CreatedBy,
            sector.UpdatedBy);

        Assert.Equal(
            CreatedAtUtc,
            sector.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that deactivation fails when the updater is empty
    /// and the sector remains unchanged.
    /// </summary>
    [Fact]
    public void Deactivate_ShouldRejectEmptyUpdatedBy()
    {
        // Arrange
        var sector = CreateSector();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            sector.Deactivate(
                string.Empty,
                UpdatedAtUtc));

        Assert.Equal(
            "updatedBy",
            exception.ParamName);

        Assert.True(sector.IsActive);
        Assert.Null(sector.UpdatedBy);
        Assert.Null(sector.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that deactivation fails when the updater is whitespace
    /// and the sector remains unchanged.
    /// </summary>
    [Fact]
    public void Deactivate_ShouldRejectWhitespaceUpdatedBy()
    {
        // Arrange
        var sector = CreateSector();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            sector.Deactivate(
                "   ",
                UpdatedAtUtc));

        Assert.Equal(
            "updatedBy",
            exception.ParamName);

        Assert.True(sector.IsActive);
        Assert.Null(sector.UpdatedBy);
        Assert.Null(sector.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that activation fails when the updater is empty
    /// and the sector remains unchanged.
    /// </summary>
    [Fact]
    public void Activate_ShouldRejectEmptyUpdatedBy()
    {
        // Arrange
        var sector = CreateSector();

        sector.Deactivate(
            UpdatedBy,
            UpdatedAtUtc);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            sector.Activate(
                string.Empty,
                CreatedAtUtc));

        Assert.Equal(
            "updatedBy",
            exception.ParamName);

        Assert.False(sector.IsActive);

        Assert.Equal(
            UpdatedBy,
            sector.UpdatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            sector.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that activation fails when the updater is whitespace
    /// and the sector remains unchanged.
    /// </summary>
    [Fact]
    public void Activate_ShouldRejectWhitespaceUpdatedBy()
    {
        // Arrange
        var sector = CreateSector();

        sector.Deactivate(
            UpdatedBy,
            UpdatedAtUtc);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            sector.Activate(
                "   ",
                CreatedAtUtc));

        Assert.Equal(
            "updatedBy",
            exception.ParamName);

        Assert.False(sector.IsActive);

        Assert.Equal(
            UpdatedBy,
            sector.UpdatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            sector.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that deactivating an already inactive sector
    /// still tracks the update.
    /// </summary>
    [Fact]
    public void Deactivate_ShouldTrackUpdate_WhenAlreadyInactive()
    {
        // Arrange
        var sector = CreateSector();

        sector.Deactivate(
            CreatedBy,
            CreatedAtUtc);

        // Act
        sector.Deactivate(
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.False(sector.IsActive);

        Assert.Equal(
            UpdatedBy,
            sector.UpdatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            sector.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that activating an already active sector
    /// still tracks the update.
    /// </summary>
    [Fact]
    public void Activate_ShouldTrackUpdate_WhenAlreadyActive()
    {
        // Arrange
        var sector = CreateSector();

        // Act
        sector.Activate(
            UpdatedBy,
            UpdatedAtUtc);

        // Assert
        Assert.True(sector.IsActive);

        Assert.Equal(
            UpdatedBy,
            sector.UpdatedBy);

        Assert.Equal(
            UpdatedAtUtc,
            sector.UpdatedAtUtc);
    }

    /// <summary>
    /// Verifies that deactivation uses the current UTC time
    /// when no timestamp is provided.
    /// </summary>
    [Fact]
    public void Deactivate_ShouldUseCurrentUtcTime_WhenTimestampIsNotProvided()
    {
        // Arrange
        var sector = CreateSector();

        var before = DateTime.UtcNow;

        // Act
        sector.Deactivate(UpdatedBy);

        var after = DateTime.UtcNow;

        // Assert
        Assert.False(sector.IsActive);

        Assert.InRange(
            sector.UpdatedAtUtc!.Value,
            before,
            after);

        Assert.Equal(
            UpdatedBy,
            sector.UpdatedBy);
    }

    /// <summary>
    /// Verifies that activation uses the current UTC time
    /// when no timestamp is provided.
    /// </summary>
    [Fact]
    public void Activate_ShouldUseCurrentUtcTime_WhenTimestampIsNotProvided()
    {
        // Arrange
        var sector = CreateSector();

        sector.Deactivate(
            CreatedBy,
            CreatedAtUtc);

        var before = DateTime.UtcNow;

        // Act
        sector.Activate(UpdatedBy);

        var after = DateTime.UtcNow;

        // Assert
        Assert.True(sector.IsActive);

        Assert.InRange(
            sector.UpdatedAtUtc!.Value,
            before,
            after);

        Assert.Equal(
            UpdatedBy,
            sector.UpdatedBy);
    }

    #endregion

    #region Helpers

    private static Sector CreateSector()
    {
        return Sector.Create(
            Code,
            new LocalizedText(
                "Technology",
                "التكنولوجيا"),
            SortOrder,
            CreatedBy,
            CreatedAtUtc);
    }

    #endregion
}

