using Intranet2.Datenbank.Models;
using Microsoft.EntityFrameworkCore;

namespace Intranet2.Datenbank.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Benutzer> Benutzer { get; set; } = null!;

        public DbSet<NewsBeitrag> NewsBeitraege { get; set; } = null!;

        public DbSet<BenutzerProtokoll> BenutzerProtokolle { get; set; } = null!;

        public DbSet<Umfrage> Umfragen { get; set; } = null!;

        public DbSet<UmfrageOption> UmfrageOptionen { get; set; } = null!;

        public DbSet<UmfrageStimme> UmfrageStimmen { get; set; } = null!;

        public DbSet<MarktplatzBeitrag> MarktplatzBeitraege { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ein Windows-Benutzer darf nur einmal vorhanden sein.
            modelBuilder.Entity<Benutzer>().HasIndex(b => b.WindowsBenutzername).IsUnique();

            // ✅ EINFÜGEN in OnModelCreating():
            modelBuilder.Entity<NewsBeitrag>()
                .Property(n => n.Inhalt)
                .HasColumnType("nvarchar(max)");

            // PRO BENUTZER NUR EINE STIMME PRO UMFRAGE
            modelBuilder.Entity<UmfrageStimme>().HasIndex(s => new { s.UmfrageId, s.WindowsBenutzername }).IsUnique();

            // UMFRAGE -> OPTIONEN
            modelBuilder.Entity<UmfrageOption>()
                .HasOne(o => o.Umfrage)
                .WithMany(u => u.Optionen)
                .HasForeignKey(o => o.UmfrageId)
                .OnDelete(DeleteBehavior.Cascade);

            // UMFRAGE -> STIMMEN
            modelBuilder.Entity<UmfrageStimme>()
                .HasOne(s => s.Umfrage)
                .WithMany(u => u.Stimmen)
                .HasForeignKey(s => s.UmfrageId)
                .OnDelete(DeleteBehavior.Cascade);

            // OPTION -> STIMMEN
            modelBuilder.Entity<UmfrageStimme>()
                .HasOne(s => s.UmfrageOption)
                .WithMany(o => o.Stimmen)
                .HasForeignKey(s => s.UmfrageOptionId)
                .OnDelete(DeleteBehavior.NoAction);
            // MARKTPLATZBEITRAG -> BENUTZER
            modelBuilder.Entity<MarktplatzBeitrag>()
                .HasOne(m => m.Benutzer)
                .WithMany()
                .HasForeignKey(m => m.BenutzerId)
                .OnDelete(DeleteBehavior.Restrict);

            // MARKTPLATZBEITRAG -> KATEGORIE
            modelBuilder.Entity<MarktplatzBeitrag>()
                .Property(m => m.Preis)
                .HasPrecision(10, 2);
        }
    }
}