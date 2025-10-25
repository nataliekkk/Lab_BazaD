using System;
using System.Collections.Generic;
using CarRental.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRental;

public partial class CarRentalContext : DbContext
{
    public CarRentalContext()
    {
    }

    public CarRentalContext(DbContextOptions<CarRentalContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Car> Cars { get; set; }

    public virtual DbSet<CarClass> CarClasses { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Maintenance> Maintenances { get; set; }

    public virtual DbSet<RentalAgreement> RentalAgreements { get; set; }

    public virtual DbSet<RentalHistory> RentalHistories { get; set; }

    public virtual DbSet<ViewCarClass> ViewCarClasses { get; set; }

    public virtual DbSet<ViewCarMaintenance> ViewCarMaintenances { get; set; }

    public virtual DbSet<ViewRentalHistory> ViewRentalHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\sqlexpress;Database=CarRental;Trusted_Connection=True;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.CarId).HasName("PK__Cars__68A0342E3CD29C66");

            entity.HasIndex(e => e.LicensePlate, "UQ__Cars__026BC15C6A8A6768").IsUnique();

            entity.Property(e => e.Brand).HasMaxLength(50);
            entity.Property(e => e.LicensePlate).HasMaxLength(20);
            entity.Property(e => e.Model).HasMaxLength(150);
            entity.Property(e => e.RentalCostPerDay).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Class).WithMany(p => p.Cars)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK_Cars_CarClasses");
        });

        modelBuilder.Entity<CarClass>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__CarClass__CB1927C0BB4C9EA1");

            entity.HasIndex(e => e.Name, "UQ__CarClass__737584F617741CE1").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(150);
            entity.Property(e => e.Name).HasMaxLength(30);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId).HasName("PK__Clients__E67E1A24FECB3822");

            entity.HasIndex(e => e.LicenseNumber, "UQ__Clients__E8890166C0131E01").IsUnique();

            entity.Property(e => e.FullName).HasMaxLength(40);
            entity.Property(e => e.LicenseNumber).HasMaxLength(20);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
        });

        modelBuilder.Entity<Maintenance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Maintena__3214EC0784FA34C4");

            entity.Property(e => e.Cost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Description).HasMaxLength(150);

            entity.HasOne(d => d.Car).WithMany(p => p.Maintenances)
                .HasForeignKey(d => d.CarId)
                .HasConstraintName("FK_Maintenances_Cars");
        });

        modelBuilder.Entity<RentalAgreement>(entity =>
        {
            entity.HasKey(e => e.RentalAgreementId).HasName("PK__RentalAg__071DDE6EB3B669EA");

            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Car).WithMany(p => p.RentalAgreements)
                .HasForeignKey(d => d.CarId)
                .HasConstraintName("FK_RentalAgreements_Cars");

            entity.HasOne(d => d.Client).WithMany(p => p.RentalAgreements)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("FK_RentalAgreements_Clients");
        });

        modelBuilder.Entity<RentalHistory>(entity =>
        {
            entity.HasKey(e => e.RentalHistoryId).HasName("PK__RentalHi__D71CE44469442268");

            entity.ToTable("RentalHistory");

            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Client).WithMany(p => p.RentalHistories)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("FK_RentalHistory_Clients");
        });

        modelBuilder.Entity<ViewCarClass>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("View_CarClasses");

            entity.Property(e => e.Brand).HasMaxLength(50);
            entity.Property(e => e.ClassName).HasMaxLength(30);
            entity.Property(e => e.LicensePlate).HasMaxLength(20);
            entity.Property(e => e.Model).HasMaxLength(150);
            entity.Property(e => e.RentalCostPerDay).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<ViewCarMaintenance>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("View_CarMaintenance");

            entity.Property(e => e.Brand).HasMaxLength(50);
            entity.Property(e => e.ClassName).HasMaxLength(30);
            entity.Property(e => e.Cost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Description).HasMaxLength(150);
            entity.Property(e => e.Model).HasMaxLength(150);
        });

        modelBuilder.Entity<ViewRentalHistory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("View_RentalHistory");

            entity.Property(e => e.Brand).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(40);
            entity.Property(e => e.LicenseNumber).HasMaxLength(20);
            entity.Property(e => e.LicensePlate).HasMaxLength(20);
            entity.Property(e => e.Model).HasMaxLength(150);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
