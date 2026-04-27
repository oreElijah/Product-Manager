using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDBContext : IdentityDbContext<AppUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
        : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole 
                {
                    Id = "6f9f18a8-5e69-42ab-94bc-29c4f58cd001",
                    ConcurrencyStamp = "79d74bcd-e2a4-48b2-93ff-281f33714911",
                    Name = "Admin", 
                    NormalizedName = "ADMIN"
                     },
                new IdentityRole 
                {
                    Id = "6f9f18a8-5e69-42ab-94bc-29c4f58cd002",
                    ConcurrencyStamp = "fb26b18d-b35a-4ccf-8be4-30d31ce3d70a",
                    Name = "User",
                    NormalizedName = "USER" 
                    }
            };

            modelBuilder.Entity<IdentityRole>().HasData(roles);
        }
    }
}