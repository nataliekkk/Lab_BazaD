using System;
using System.Collections.Generic;

namespace CarRental.Models;

public partial class Client
{
    public int ClientId { get; set; }

    public string FullName { get; set; } = null!;

    public string LicenseNumber { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public virtual ICollection<RentalAgreement> RentalAgreements { get; set; } = new List<RentalAgreement>();

    public virtual ICollection<RentalHistory> RentalHistories { get; set; } = new List<RentalHistory>();
}
