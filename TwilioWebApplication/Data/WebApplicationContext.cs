
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwilioWebApplication.Models;
namespace TwilioWebApplication.Data
{
    public class WebApplicationContext : IdentityDbContext<User>
    {

        public WebApplicationContext(DbContextOptions<WebApplicationContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new UserEntityConfiguration());
        }



        public DbSet<Employee> Employees { get; set; }
        public DbSet<Call> Calls { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<TwilioPhoneNumber> TwilioPhoneNumbers { get; set; }
        public DbSet<Contact> Contacts { get; set; }
    }

    internal class UserEntityConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.FirstName).HasMaxLength(255);
            builder.Property(u => u.LastName).HasMaxLength(255);
            builder.Property(u => u.TwilioAccountSid).HasMaxLength(34);
            builder.Property(u => u.TwilioAuthToken).HasMaxLength(34);

        }
    }
}
