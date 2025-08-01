using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FF.Models; // تأكد أن هذا هو اسم مجلد النماذج الصحيح

namespace FF.Data // تأكد أن هذا هو اسم مجلد البيانات الصحيح
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Area> Areas { get; set; }
        public DbSet<Tank> Tanks { get; set; }
        public DbSet<Order> Orders { get; set; }
         public DbSet<ApplicationUser> applicationUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // مهم جداً لجداول Identity
            builder.Entity<Order>()
      .Property(o => o.TotalPrice)
      .HasColumnType("decimal(18, 2)");

            builder.Entity<Tank>()
                .Property(t => t.PricePerBarrel)
                .HasColumnType("decimal(18, 2)");
            // العلاقة بين الطلب والمستخدم (الزبون)
            builder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // العلاقة بين الطلب والصهريج
            builder.Entity<Order>()
                .HasOne(o => o.Tank)
                .WithMany()
                .HasForeignKey(o => o.TankId)
                .OnDelete(DeleteBehavior.Restrict);

            // العلاقة بين الصهاريج والمناطق (Many-to-Many)
            // هذا الكود الآن سيعمل بشكل صحيح بعد التعديلات على النماذج
            builder.Entity<Tank>()
                .HasMany(t => t.Areas)
                .WithMany(a => a.Tanks)
                .UsingEntity(j => j.ToTable("TankAreas")); // تحديد اسم جدول الربط بشكل صريح، وهي ممارسة ممتازة
        }
    }
}