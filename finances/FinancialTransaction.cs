using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;

namespace finances
{

    public class FinancialTransaction
    {
        public int Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } // Poprawiono atrybut
        public double Amount { get; set; }
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