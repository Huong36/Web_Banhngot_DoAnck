using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Web_Banhngot_DoAnck.Models
{
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("myCS", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ProductRecommendation> ProductRecommendations { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Cấu hình để tránh lỗi Multiple Cascade Paths khi có 2 khóa ngoại cùng trỏ về Product
            modelBuilder.Entity<ProductRecommendation>()
                .HasRequired(p => p.ProductA)
                .WithMany()
                .HasForeignKey(p => p.ProductId_A)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProductRecommendation>()
                .HasRequired(p => p.ProductB)
                .WithMany()
                .HasForeignKey(p => p.ProductId_B)
                .WillCascadeOnDelete(false);
        }
    }
}