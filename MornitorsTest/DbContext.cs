using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MornitorsTest
{
    public class AppDbContext : DbContext
    {
        public DbSet<EvacuationZones> EvacuationZone { get; set; }
        public DbSet<Vehicless> Vehicles { get; set; }
        public DbSet<EvacuationPlans> EvacuationPlan { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
    }

    public class EvacuationZones
    {
        [Key]
        public required string ZoneID { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? NumberOfPeople { get; set; }
        public int? UrgencyLevel { get; set; }
        public int? RemainingPeople { get; set; }
        public string? VehicleID { get; set; }
    }
    public class Vehicless
    {
        [Key]
        public required string VehicleID { get; set; }
        public int? Capacity { get; set; }
        public string? Type { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? Speed { get; set; }
    }
    public class EvacuationPlans
    {
        [Key]
        public required string GUID { get; set; }
        public string? ZoneID { get; set; }
        public string? VehicleID { get; set; }
        public string? ETA { get; set; }
        public int? NumberOfPeople { get; set; }
    }
}
