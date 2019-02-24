using System.Data.Entity;
using System.Data.SQLite;
using NestedNavigationProperties.Models.Ef6;
using SQLite.CodeFirst;

namespace NestedNavigationProperties.DbContext.Ef6
{
    public class ApplicationDatabaseContext : System.Data.Entity.DbContext
    {
        public ApplicationDatabaseContext() : base(new SQLiteConnection(GetConnectionString()), true)
        {
        }

        public static string GetConnectionString()
        {
            var cs = new SQLiteConnectionStringBuilder()
            {
                DataSource = "PresetMagician.test.sqlite3", ForeignKeys = true, SyncMode = SynchronizationModes.Off,
                CacheSize = -10240
            };


            return cs.ConnectionString;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer =
                new SqliteCreateDatabaseIfNotExists<ApplicationDatabaseContext>(modelBuilder);

            modelBuilder.Entity<Plugin>().HasMany(p => p.DefaultModes).WithMany(q => q.Plugins).Map(mc =>
                mc.MapLeftKey("PluginId").MapRightKey("ModeId").ToTable("PluginModes"));

            modelBuilder.Entity<Plugin>().HasMany(p => p.DefaultTypes).WithMany(q => q.Plugins).Map(mc =>
                mc.MapLeftKey("PluginId").MapRightKey("TypeId").ToTable("PluginTypes"));


            modelBuilder.Entity<Preset>().HasMany(p => p.Types).WithMany(q => q.Presets).Map(mc =>
                mc.MapLeftKey("PresetId").MapRightKey("TypeId").ToTable("PresetTypes"));

            modelBuilder.Entity<Preset>().HasMany(p => p.Modes).WithMany(q => q.Presets).Map(mc =>
                mc.MapLeftKey("PresetId").MapRightKey("ModeId").ToTable("PresetModes"));
            Database.SetInitializer(sqliteConnectionInitializer);
        }

        public DbSet<Plugin> Plugins { get; set; }
        public DbSet<Preset> Presets { get; set; }
        public DbSet<Mode> Modes { get; set; }
        public DbSet<Type> Types { get; set; }
    }
}