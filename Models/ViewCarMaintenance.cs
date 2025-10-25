using System;
using System.Collections.Generic;

namespace CarRental.Models;

public partial class ViewCarMaintenance
{
    public int Id { get; set; }

    public int CarId { get; set; }

    public string Brand { get; set; } = null!;

    public string Model { get; set; } = null!;

    public string ClassName { get; set; } = null!;

    public DateOnly MaintenanceDate { get; set; }

    public string Description { get; set; } = null!;

    public decimal Cost { get; set; }
}
