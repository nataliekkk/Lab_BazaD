using System;
using System.Collections.Generic;

namespace CarRental.Models;

public partial class ViewRentalHistory
{
    public int ClientId { get; set; }

    public string FullName { get; set; } = null!;

    public string LicenseNumber { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public DateOnly StartDateTime { get; set; }

    public DateOnly PlannedEndDateTime { get; set; }

    public DateOnly? ActualEndDateTime { get; set; }

    public decimal TotalAmount { get; set; }

    public string Brand { get; set; } = null!;

    public string Model { get; set; } = null!;

    public string LicensePlate { get; set; } = null!;
}
