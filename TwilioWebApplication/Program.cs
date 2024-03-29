using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using TwilioWebApplication.Data;
using TwilioWebApplication.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// this segment of code optimizes the connection string so it isn't hard coded (it finds out the root of the project automatically at runtime)
string _connection = builder.Configuration.GetConnectionString("DefaultConnection");

if (_connection.Contains("%CONTENTROOTPATH%"))
{
    _connection = _connection.Replace("%CONTENTROOTPATH%", builder.Environment.ContentRootPath);
}
builder.Services.AddDbContext<WebApplicationContext>(options => options.UseSqlite(
    _connection
    ));
builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<WebApplicationContext>();
//end of db setup


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

//builder.Services.AddIdentity<User, IdentityRole>().AddUserStore<WebApplicationContext>().AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/");


app.Run();
