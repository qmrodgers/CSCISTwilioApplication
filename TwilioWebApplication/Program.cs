using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TwilioWebApplication.Data;

var builder = WebApplication.CreateBuilder(args);
var _connection = builder.Configuration.GetConnectionString("DefaultConnection");
if (_connection.Contains("%CONTENTROOTPATH%"))
{
    _connection = _connection.Replace("%CONTENTROOTPATH%", builder.Environment.ContentRootPath);
}

builder.Services.AddDbContext<WebApplicationContext>(options => options.UseSqlServer(
    _connection
    ));

// Add services to the container.
builder.Services.AddControllersWithViews();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
