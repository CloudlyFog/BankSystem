using BankSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BankSystem.AppContext
{
    sealed internal class BankAccountContext : DbContext
    {
        private readonly string queryConnection = @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=CabManagementSystem;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False";
        public BankAccountContext(string queryConnection)
        {
            this.queryConnection = queryConnection;
            Database.EnsureCreated();
        }
        public BankAccountContext() => Database.EnsureCreated();
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseSqlServer(queryConnection);
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<BankAccountModel> BankAccounts { get; set; }
    }
}
