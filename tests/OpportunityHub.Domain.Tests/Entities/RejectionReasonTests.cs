using OpportunityHub.Domain.Entities;
namespace OpportunityHub.Domain.Tests.Entities;

public sealed class RejectionReasonTests
{
    private const string Code = "INVALID_DATA";
    private const string Name = "Invalid data";
    private const int DisplayOrder = 1;

    #region Creation

    /// <summary>
    /// Verifies that a rejection reason is created with the supplied
    /// values and starts in the active state.
    /// </summary>
    [Fact]
    public void Create_ShouldCreateRejectionReason()
    {
        // Act
        var rejectionReason = new RejectionReason(
            Code,
            Name,
            DisplayOrder);

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            rejectionReason.Id);

        Assert.Equal(
            Code,
            rejectionReason.Code);

        Assert.Equal(
            Name,
            rejectionReason.Name);

        Assert.Equal(
            DisplayOrder,
            rejectionReason.DisplayOrder);

        Assert.True(
            rejectionReason.IsActive);
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
            new RejectionReason(
                string.Empty,
                Name,
                DisplayOrder));

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
            new RejectionReason(
                "   ",
                Name,
                DisplayOrder));

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
            new RejectionReason(
                null!,
                Name,
                DisplayOrder));

        Assert.Equal(
            "code",
            exception.ParamName);
    }

    /// <summary>
    /// Verifies that creation rejects an empty name.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectEmptyName()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new RejectionReason(
                Code,
                string.Empty,
                DisplayOrder));

        Assert.Equal(
            "name",
            exception.ParamName);
    }

    /// <summary>
    /// Verifies that creation rejects a whitespace name.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectWhitespaceName()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new RejectionReason(
                Code,
                "   ",
                DisplayOrder));

        Assert.Equal(
            "name",
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
            new RejectionReason(
                Code,
                null!,
                DisplayOrder));

        Assert.Equal(
            "name",
            exception.ParamName);
    }

    /// <summary>
    /// Verifies that creation rejects a negative display order.
    /// </summary>
    [Fact]
    public void Create_ShouldRejectNegativeDisplayOrder()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RejectionReason(
                Code,
                Name,
                -1));

        Assert.StartsWith(
            "Specified argument was out of the range",
            exception.Message);

        Assert.Equal(
            "displayOrder",
            exception.ParamName);
    }

    /// <summary>
    /// Verifies that zero is accepted as a valid display order.
    /// </summary>
    [Fact]
    public void Create_ShouldAllowZeroDisplayOrder()
    {
        // Act
        var rejectionReason = new RejectionReason(
            Code,
            Name,
            0);

        // Assert
        Assert.Equal(
            0,
            rejectionReason.DisplayOrder);
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

        rejectionReason.Deactivate();

        // Act
        rejectionReason.Activate();

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
        rejectionReason.Activate();

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
        rejectionReason.Deactivate();

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

        rejectionReason.Deactivate();

        // Act
        rejectionReason.Deactivate();

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
        rejectionReason.Deactivate();

        // Assert
        Assert.False(
            rejectionReason.IsActive);

        // Act
        rejectionReason.Activate();

        // Assert
        Assert.True(
            rejectionReason.IsActive);
    }

    #endregion

    #region Helpers

    private static RejectionReason CreateRejectionReason()
    {
        return new RejectionReason(
            Code,
            Name,
            DisplayOrder);
    }

    #endregion
}
