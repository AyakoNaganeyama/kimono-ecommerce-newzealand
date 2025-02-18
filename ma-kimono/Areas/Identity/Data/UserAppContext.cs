using ma_kimono.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ma_kimono.Areas.Identity.Data;

//Db context for identity and inserted role
public class UserAppContext : IdentityDbContext<AppUser>
{
    public UserAppContext(DbContextOptions<UserAppContext> options): base(options) { }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // insert role whne migrate and update database 
        base.OnModelCreating(builder);
        var admin = new IdentityRole("admin");
        admin.NormalizedName = "admin";
        var client = new IdentityRole("client");
        admin.NormalizedName = "client";
        var manager = new IdentityRole("manager");
        admin.NormalizedName = "manager";
        builder.Entity<IdentityRole>().HasData(admin, client, manager);
    }
}
