using ANG24.Sys.Domain.DBModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ANG24.Sys.Persistence
{
    public sealed class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Prefix>? Prefixes { get; set; }
        public DbSet<DeviceGroup>? DeviceGroups { get; set; }
        public DbSet<LogData>? LogDatas { get; set; }
        public DbSet<ModuleStage>? ModuleStages { get; set; }
        public DbSet<Session>? Sessions { get; set; }
        public DbSet<Customer>? Customers { get; set; }
        public DbSet<DeviceParameter>? DeviceParameters { get; set; }
        public DbSet<EnergyObject>? EnergyObjects { get; set; }
        public DbSet<Faze>? Fazes { get; set; }
        public DbSet<Module>? Modules { get; set; }
        public DbSet<Order>? Orders { get; set; }
        public DbSet<OrderType>? OrderTypes { get; set; }
        public DbSet<ParameterAddition>? ParameterAdditions { get; set; }
        public DbSet<ResultValue>? ResultValues { get; set; }
        public DbSet<TestObject>? TestObjects { get; set; }
        public DbSet<TestTarget>? TestTargets { get; set; }
        public DbSet<Unit>? Units { get; set; }
        public DbSet<FazeMeteringResult>? FazeMeteringResults { get; set; }
        public DbSet<Device>? Devices { get; set; }
    }
}
