namespace Web_Banhngot_DoAnck.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Web_Banhngot_DoAnck.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<Web_Banhngot_DoAnck.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(Web_Banhngot_DoAnck.Models.ApplicationDbContext context)
        {
            var roleStore = new RoleStore<IdentityRole>(context);
            var roleManager = new RoleManager<IdentityRole>(roleStore);

            if (!roleManager.RoleExists("Admin"))
            {
                roleManager.Create(new IdentityRole("Admin"));
            }

            if (!roleManager.RoleExists("NhanVien"))
            {
                roleManager.Create(new IdentityRole("NhanVien"));
            }

            if (!roleManager.RoleExists("User"))
            {
                roleManager.Create(new IdentityRole("User"));
            }

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            if (!context.Users.Any(u => u.UserName == "admin"))
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@gmail.com"
                };

                userManager.Create(adminUser, "123456");
                userManager.AddToRole(adminUser.Id, "Admin");
            }

            if (!context.Users.Any(u => u.UserName == "nhanvien"))
            {
                var staffUser = new ApplicationUser
                {
                    UserName = "nhanvien",
                    Email = "nhanvien@gmail.com"
                };

                userManager.Create(staffUser, "123456");
                userManager.AddToRole(staffUser.Id, "NhanVien");
            }
        }
    }
}