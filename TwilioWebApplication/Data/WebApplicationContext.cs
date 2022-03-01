using Microsoft.EntityFrameworkCore;
using TwilioWebApplication.Models;
namespace TwilioWebApplication.Data
{
    public class WebApplicationContext : DbContext
    {
        public WebApplicationContext(DbContextOptions<WebApplicationContext> options) : base(options)
        {

        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<CallLog> CallLogs { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<User> Users { get; set; }

    }
}
