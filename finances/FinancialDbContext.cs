using Microsoft.EntityFrameworkCore;
using System;

namespace finances
{
    internal class FinancialDbContext : DbContext
    {
        public DbSet<FinancialTransaction> FinancialTransactions { get; set; }

        public FinancialDbContext()
        {
            Database.EnsureCreated(); // Utwórz bazę danych jeśli nie istnieje
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=FinancialDatabase.db"); // Ustawienie bazy danych SQLite
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FinancialTransaction>().HasData(
                new FinancialTransaction { Id = 1, TransactionDate = DateTime.Now, Description = "Example Expense", Amount = 100.50m, Type = TransactionType.Expense },
                new FinancialTransaction { Id = 2, TransactionDate = DateTime.Now, Description = "Example Income", Amount = 500.75m, Type = TransactionType.Income }
            );
        }
    }

    internal class FinancialTransaction
    {
        public int Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } // Poprawiono atrybut
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, Date: {TransactionDate.ToShortDateString()}, Description: {Description}, Amount: {Amount}, Type: {Type}";
        }
    }

    public enum TransactionType
    {
        Expense, // Wydatek
        Income   // Dochód
    }
}
