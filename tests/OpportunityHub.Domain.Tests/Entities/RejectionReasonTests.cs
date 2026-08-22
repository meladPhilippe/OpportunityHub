using OpportunityHub.Domain.Entities;
using OpportunityHub.Domain.ValueObjects;

namespace OpportunityHub.Domain.Tests.Entities;

public sealed class RejectionReasonTests
{
    private const string Code = "INVALID_DATA";
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

    #region Creation

    /// <summary>
    /// Verifies that a rejection reason is created with the supplied
    /// values and starts in the active state.
    /// </summary>
    [Fact]
    public void Create_ShouldCreateRejectionReason()
    {
        // Arrange
        var name = new LocalizedText("Invalid data");

        // Act
        var rejectionReason = RejectionReason.Create(
            Code,
            name,
            SortOrder,
            CreatedBy,
            CreatedAtUtc);

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            rejectionReason.Id);

        Assert.Equal(
            Code,
            rejectionReason.Code);

        Assert.Same(
            name,
            rejectionReason.Name);

        Assert.Equal(
            SortOrder,
            rejectionReason.SortOrder);

        Assert.True(
            rejectionReason.IsActive);

        Assert.Equal(
            CreatedBy,
            rejectionReason.CreatedBy);

        Assert.Equal(
            CreatedAtUtc,
            rejectionReason.CreatedAtUtc);
    }

    /// <summary>
    /// Verifies that a rejection reason starts with an active state
    /// when created.
    /// </summary>
    [Fact]
    public void Create_ShouldSetActiveByDefault()
    {
        // Act
        var rejectionReason = CreateRejectionReason();

        // Assert
        Assert.True(
            rejectionReason.IsActive);
    }

    #endregion

    #region Validation

    /// <summary>
    /// Verifies that creation rejects an empty code.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectEmptyCode()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            RejectionReason.Create(
                string.Empty,
                new LocalizedText("Invalid data"),
                SortOrder,
                CreatedBy,
                CreatedAtUtc));

        Assert.Equal(
            "code",
            exception.ParamName);
    }

    /// <summary>
    /// Verifies that creation rejects a whitespace code.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectWhitespaceCode()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            RejectionReason.Create(
                "   ",
                new LocalizedText("Invalid data"),
                SortOrder,
                CreatedBy,
                CreatedAtUtc));

        Assert.Equal(
            "code",
            exception.ParamName);
    }

    /// <summary>
    /// Verifies that creation rejects a null code.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectNullCode()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            RejectionReason.Create(
                null!,
                new LocalizedText("Invalid data"),
                SortOrder,
                CreatedBy,
                CreatedAtUtc));

        Assert.Equal(
            "code",
            exception.ParamName);
    }

    /// <summary>
    /// Verifies that creation rejects a null name.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectNullName()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            RejectionReason.Create(
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
    /// Verifies that creation rejects a negative sort order.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectNegativeSortOrder()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            RejectionReason.Create(
                Code,
                new LocalizedText("Invalid data"),
                -1,
                CreatedBy,
                CreatedAtUtc));

        Assert.Equal(
            "sortOrder",
            exception.ParamName);
    }

    /// <summary>
    /// Verifies that creation rejects an empty creator.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectEmptyCreatedBy()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            RejectionReason.Create(
                Code,
                new LocalizedText("Invalid data"),
                SortOrder,
                string.Empty,
                CreatedAtUtc));

        Assert.Equal(
            "createdBy",
            exception.ParamName);
    }

    /// <summary>
    /// Verifies that zero is accepted as a valid sort order.
    /// </summary>
    [Fact]
    public void Create_ShouldAllowZeroSortOrder()
    {
        // Act
        var rejectionReason = RejectionReason.Create(
            Code,
            new LocalizedText("Invalid data"),
            0,
            CreatedBy,
            CreatedAtUtc);

        // Assert
        Assert.Equal(
            0,
            rejectionReason.SortOrder);
    }

    #endregion

    #region Activation

    /// <summary>
    /// Verifies that activating a rejection reason changes it
    /// to the active state.
    /// </summary>
    [Fact]
    public void Activate_ShouldActivateRejectionReason()
    {
        // Arrange
        var rejectionReason = CreateRejectionReason();

        rejectionReason.Deactivate(UpdatedBy);

        // Act
        rejectionReason.Activate(UpdatedBy);

        // Assert
        Assert.True(
            rejectionReason.IsActive);
    }

    /// <summary>
    /// Verifies that activating an already active rejection reason
    /// keeps it active.
    /// </summary>
    [Fact]
    public void Activate_ShouldKeepRejectionReasonActive()
    {
        // Arrange
        var rejectionReason = CreateRejectionReason();

        // Act
        rejectionReason.Activate(UpdatedBy);

        // Assert
        Assert.True(
            rejectionReason.IsActive);
    }

    #endregion

    #region Deactivation

    /// <summary>
    /// Verifies that deactivating a rejection reason changes it
    /// to the inactive state.
    /// </summary>
    [Fact]
    public void Deactivate_ShouldDeactivateRejectionReason()
    {
        // Arrange
        var rejectionReason = CreateRejectionReason();

        // Act
        rejectionReason.Deactivate(UpdatedBy);

        // Assert
        Assert.False(
            rejectionReason.IsActive);
    }

    /// <summary>
    /// Verifies that deactivating an already inactive rejection reason
    /// keeps it inactive.
    /// </summary>
    [Fact]
    public void Deactivate_ShouldKeepRejectionReasonInactive()
    {
        // Arrange
        var rejectionReason = CreateRejectionReason();

        rejectionReason.Deactivate(UpdatedBy);

        // Act
        rejectionReason.Deactivate(UpdatedBy);

        // Assert
        Assert.False(
            rejectionReason.IsActive);
    }

    #endregion

    #region State Transitions

    /// <summary>
    /// Verifies that a rejection reason can transition from active
    /// to inactive and back to active.
    /// </summary>
    [Fact]
    public void State_ShouldSupportActivationAndDeactivation()
    {
        // Arrange
        var rejectionReason = CreateRejectionReason();

        // Act
        rejectionReason.Deactivate(UpdatedBy);

        // Assert
        Assert.False(
            rejectionReason.IsActive);

        // Act
        rejectionReason.Activate(UpdatedBy);

        // Assert
        Assert.True(
            rejectionReason.IsActive);
    }

    #endregion

    #region Helpers

    private static RejectionReason CreateRejectionReason()
    {
        return RejectionReason.Create(
            Code,
            new LocalizedText("Invalid data"),
            SortOrder,
            CreatedBy,
            CreatedAtUtc);
    }

    #endregion
}

