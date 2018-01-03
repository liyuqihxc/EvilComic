using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace EvilComic.DataAccess
{
    public partial class XEDbContext : DbContext
    {
        public virtual DbSet<Unfinished> Unfinished { get; set; }
        public virtual DbSet<Xecategory> Xecategory { get; set; }
        public virtual DbSet<Xeconnection> Xeconnection { get; set; }
        public virtual DbSet<XedbPath> XedbPath { get; set; }
        public virtual DbSet<Xeinformation> Xeinformation { get; set; }

        public XEDbContext(DbContextOptions<XEDbContext> options) : base(options)
        {

        }

        public static XEDbContext CreateContext()
        {
            var option = new DbContextOptionsBuilder<XEDbContext>();
            option.UseSqlite(Common.Utility.Configuration.GetConnectionString("EvilComic"));
            return new XEDbContext(option.Options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Unfinished>(entity =>
            {
                entity.HasIndex(e => e.UnfinishedId)
                    .IsUnique();

                entity.Property(e => e.UnfinishedId).ValueGeneratedOnAdd();

                entity.HasOne(d => d.Information)
                    .WithMany(p => p.Unfinished)
                    .HasForeignKey(d => d.InformationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Xecategory>(entity =>
            {
                entity.HasKey(e => e.CategoryId);

                entity.ToTable("XECategory");

                entity.HasIndex(e => e.CategoryId)
                    .IsUnique();

                entity.Property(e => e.CategoryId).ValueGeneratedOnAdd();

                entity.Property(e => e.FrontPage)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(300)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(50)");
            });

            modelBuilder.Entity<Xeconnection>(entity =>
            {
                entity.HasKey(e => e.ConnectionId);

                entity.ToTable("XEConnection");

                entity.HasIndex(e => e.ConnectionId)
                    .IsUnique();

                entity.HasIndex(e => e.DbGuid)
                    .IsUnique();

                entity.Property(e => e.ConnectionId)
                    .HasColumnName("ConnectionID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.DbGuid)
                    .IsRequired()
                    .HasColumnType("NCHAR(36)");

                entity.HasOne(d => d.DbPath)
                    .WithMany(p => p.Xeconnection)
                    .HasForeignKey(d => d.DbPathId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<XedbPath>(entity =>
            {
                entity.HasKey(e => e.DbPathId);

                entity.ToTable("XEDbPath");

                entity.HasIndex(e => e.DbPathId)
                    .IsUnique();

                entity.HasIndex(e => e.Path)
                    .IsUnique();

                entity.Property(e => e.DbPathId).ValueGeneratedOnAdd();

                entity.Property(e => e.Path)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(360)");
            });

            modelBuilder.Entity<Xeinformation>(entity =>
            {
                entity.HasKey(e => e.InformationId);

                entity.ToTable("XEInformation");

                entity.HasIndex(e => e.InformationId)
                    .IsUnique();

                entity.Property(e => e.InformationId).ValueGeneratedOnAdd();

                entity.Property(e => e.ImageCount).HasColumnType("SMALLINT");

                entity.Property(e => e.ImageUrlTemplate)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(300)");

                entity.Property(e => e.Order).HasColumnType("INT");

                entity.Property(e => e.OrginUrl)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(300)");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(100)");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Xeinformation)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Connection)
                    .WithMany(p => p.Xeinformation)
                    .HasForeignKey(d => d.ConnectionId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });
        }
    }
}
