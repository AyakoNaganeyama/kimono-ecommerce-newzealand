using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ma_kimono.Areas.Identity.Data;
using ma_kimono.Models.DB;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);




/////////////////test
///
//var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
//var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
/////////////////////
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
var connectionString = builder.Configuration.GetConnectionString("UserAppContextConnection") ?? throw new InvalidOperationException("Connection string 'UserAppContextConnection' not found.");
builder.Services.AddDbContext<UserAppContext>(options => options.UseSqlServer(connectionString));
//inject DB context 
builder.Services.AddDbContext<Fs2024s1Project01ProjectContext>(options => options.UseSqlServer(connectionString));
//Add roles for authorization 
builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true).AddRoles<IdentityRole>().AddEntityFrameworkStores<UserAppContext>();
// add services to the container.
builder.Services.AddControllersWithViews();
//sign in with Google

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCookiePolicy();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
