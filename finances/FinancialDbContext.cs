using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;

namespace finances
{
    class FinancialDbContext : DbContext
    {

        public DbSet<FinancialTransaction> FinancialTransactions { get; set; }

        public FinancialDbContext() : base()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Console.WriteLine);
            if (!optionsBuilder.IsConfigured)
            {
                string dbPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string dbName = "FinancialDatabase.db";
                optionsBuilder.UseSqlite($"Data Source={System.IO.Path.Combine(dbPath, dbName)}"); // Ustawienie bazy danych SQLite w lokalizacji danych aplikacji
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
