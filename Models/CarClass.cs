using System;
using System.Collections.Generic;

namespace CarRental.Models;

public partial class CarClass
{
    public int ClassId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
}
