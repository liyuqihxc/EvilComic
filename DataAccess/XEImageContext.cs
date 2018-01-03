using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EvilComic.DataAccess
{
    public partial class XEImageContext : DbContext
    {
        public virtual DbSet<XEImages> Xeimages { get; set; }

        /// <summary>
        /// 单幅图片平均约为223.6KB，再加上XEImages记录中其它字段给它算一条记录为224KB
        /// </summary>
        public const int RecordAvgSize = 0x38000;

        /// <summary>
        /// 数据文件最大1.9GB
        /// </summary>
        public const int DataFileMaxSize = 0x7999999A;

        /// <summary>
        /// 磁盘最小剩余空间2GB
        /// </summary>
        public const uint DiskMinFreeSpace = 0x80000000;

        public XEImageContext(DbContextOptions<XEImageContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<XEImages>(entity =>
            {
                entity.HasKey(e => e.ImageId);

                entity.ToTable("XEImages");

                entity.HasIndex(e => e.ImageId)
                    .IsUnique();

                entity.Property(e => e.ImageId).ValueGeneratedOnAdd();

                entity.Property(e => e.Order).HasColumnType("SMALLINT");

                entity.Property(e => e.Xedata)
                    .IsRequired()
                    .HasColumnName("XEData");
            });
        }
    }
}
