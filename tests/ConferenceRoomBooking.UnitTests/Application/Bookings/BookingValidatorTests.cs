using ConferenceRoomBooking.Application.Bookings;

namespace ConferenceRoomBooking.UnitTests.Application.Bookings;

public class BookingValidatorTests
{
    [Test]
    public void Validate_WhenBookingIsWithinAllowedHours_ReturnsNoErrors()
    {
        // Arrange
        var now = new DateTime(2026, 9, 26, 9, 0, 0);
        var validator = new BookingValidator(() => now);

        var start = new DateTime(2026, 9, 26, 10, 0, 0);
        var end = new DateTime(2026, 9, 26, 12, 0, 0);

        // Act
        var errors = validator.Validate(start, end);

        // Assert
        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Validate_WhenBookingStartsAtExactly06_ReturnsNoErrors()
    {
        // Arrange
        var now = new DateTime(2026, 9, 26, 9, 0, 0);
        var validator = new BookingValidator(() => now);

        var start = new DateTime(2026, 9, 27, 6, 0, 0);
        var end = new DateTime(2026, 9, 27, 7, 0, 0);

        // Act
        var errors = validator.Validate(start, end);

        // Assert
        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Validate_WhenBookingEndsAtExactly23_ReturnsNoErrors()
    {
        // Arrange
        var now = new DateTime(2026, 9, 26, 9, 0, 0);
        var validator = new BookingValidator(() => now);

        var start = new DateTime(2026, 9, 27, 22, 0, 0);
        var end = new DateTime(2026, 9, 27, 23, 0, 0);

        // Act
        var errors = validator.Validate(start, end);

        // Assert
        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Validate_WhenBookingIsWithinOneCalendarDayFrom06To23_ReturnsNoErrors()
    {
        // Arrange
        var now = new DateTime(2026, 9, 26, 9, 0, 0);
        var validator = new BookingValidator(() => now);

        var start = new DateTime(2026, 9, 27, 6, 0, 0);
        var end = new DateTime(2026, 9, 27, 23, 0, 0);

        // Act
        var errors = validator.Validate(start, end);

        // Assert
        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Validate_WhenBookingStartIsAfterEnd_ReturnsValidationError()
    {
        // Arrange
        var now = new DateTime(2026, 9, 26, 9, 0, 0);
        var validator = new BookingValidator(() => now);

        var start = new DateTime(2026, 9, 26, 12, 0, 0); // Booking start is after end
        var end = new DateTime(2026, 9, 26, 10, 0, 0);

        // Act
        var errors = validator.Validate(start, end);

        // Assert
        Assert.That(errors, Has.One.Matches<string>(error => error == "Booking start must be before booking end."));
    }

    [Test]
    public void Validate_WhenBookingStartIsInThePast_ReturnsValidationError()
    {
        // Arrange
        var now = new DateTime(2026, 9, 26, 12, 0, 1);
        var validator = new BookingValidator(() => now);

        var start = new DateTime(2026, 9, 26, 12, 0, 0); // Booking start is in the past
        var end = new DateTime(2026, 9, 26, 13, 0, 0);

        // Act
        var errors = validator.Validate(start, end);

        // Assert
        Assert.That(errors, Has.One.Matches<string>(error => error == "Booking cannot start in the past."));
    }

    [Test]
    public void Validate_WhenBookingStartIsNotFullHour_ReturnsValidationError()
    {
        // Arrange
        var now = new DateTime(2026, 9, 26, 9, 0, 0);
        var validator = new BookingValidator(() => now);

        var start = new DateTime(2026, 9, 26, 12, 30, 0); // Booking start is not on a full hour
        var end = new DateTime(2026, 9, 26, 14, 00, 0);

        // Act
        var errors = validator.Validate(start, end);

        // Assert
        Assert.That(errors, Has.One.Matches<string>(error => error == "Booking start and end must be on full hours."));
    }

    [Test]
    public void Validate_WhenBookingEndIsNotFullHour_ReturnsValidationError()
    {
        // Arrange
        var now = new DateTime(2026, 9, 26, 9, 0, 0);
        var validator = new BookingValidator(() => now);

        var start = new DateTime(2026, 9, 26, 12, 0, 0);
        var end = new DateTime(2026, 9, 26, 14, 30, 0); // Booking end is not on a full hour

        // Act
        var errors = validator.Validate(start, end);

        // Assert
        Assert.That(errors, Has.One.Matches<string>(error => error == "Booking start and end must be on full hours."));
    }

    [Test]
    public void Validate_WhenBookingStartIsNotWithinWorkingHours_ReturnsValidationError()
    {
        // Arrange
        var now = new DateTime(2026, 9, 26, 9, 0, 0);
        var validator = new BookingValidator(() => now);

        var start = new DateTime(2026, 9, 27, 05, 0, 0); // Booking start is not within working hours
        var end = new DateTime(2026, 9, 27, 11, 0, 0);

        // Act
        var errors = validator.Validate(start, end);

        // Assert
        Assert.That(errors, Has.One.Matches<string>(error => error == "Booking must be within 06:00–23:00."));
    }

    [Test]
    public void Validate_WhenBookingEndIsNotWithinWorkingHours_ReturnsValidationError()
    {
        // Arrange
        var now = new DateTime(2026, 9, 26, 9, 0, 0);
        var validator = new BookingValidator(() => now);

        var start = new DateTime(2026, 9, 27, 22, 0, 0);
        var end = new DateTime(2026, 9, 27, 23, 30, 0);  // Booking end is not within working hours

        // Act
        var errors = validator.Validate(start, end);

        // Assert
        Assert.That(errors,  Does.Contain("Booking must be within 06:00–23:00."));
    }

    [Test]
    public void Validate_WhenBookingCrossesCalendarDay_ReturnsValidationError()
    {
        // Arrange
        var now = new DateTime(2026, 9, 26, 9, 0, 0);
        var validator = new BookingValidator(() => now);

        var start = new DateTime(2026, 9, 27, 22, 0, 0);
        var end = new DateTime(2026, 9, 28, 0, 0, 0);

        // Act
        var errors = validator.Validate(start, end);

        // Assert
        Assert.That(errors, Has.One.Matches<string>(error => error == "Booking must be within 06:00–23:00."));
    }

    [Test]
    public void Validate_WhenBookingStartEqualsEnd_ReturnsValidationError()
    {
        // Arrange
        var now = new DateTime(2026, 9, 26, 9, 0, 0);
        var validator = new BookingValidator(() => now);

        var start = new DateTime(2026, 9, 26, 12, 0, 0);
        var end = new DateTime(2026, 9, 26, 12, 0, 0);

        // Act
        var errors = validator.Validate(start, end);

        // Assert
        Assert.That(
            errors,
            Has.One.Matches<string>(
                error => error == "Booking start must be before booking end."));
    }

    [Test]
    public void Validate_WhenBookingViolatesMultipleRules_ReturnsAllValidationErrors()
    {
        // Arrange
        var now = new DateTime(2026, 9, 26, 9, 0, 0);
        var validator = new BookingValidator(() => now);

        var start = new DateTime(2026, 9, 27, 5, 30, 0);
        var end = new DateTime(2026, 9, 27, 10, 30, 0);

        // Act
        var errors = validator.Validate(start, end);

        // Assert
        Assert.That(errors, Has.Count.EqualTo(2));
        Assert.That(errors, Does.Contain("Booking start and end must be on full hours."));
        Assert.That(errors, Does.Contain("Booking must be within 06:00–23:00."));
    }
}
