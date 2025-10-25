using System;
using System.Collections.Generic;

namespace CarRental.Models;

public partial class ViewCarClass
{
    public int CarId { get; set; }

    public string Brand { get; set; } = null!;

    public string Model { get; set; } = null!;

    public string LicensePlate { get; set; } = null!;

    public int Year { get; set; }

    public string ClassName { get; set; } = null!;

    public decimal RentalCostPerDay { get; set; }

    public string Status { get; set; } = null!;
}
