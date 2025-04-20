using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace Ledger.Models
{
    [Table("profit_withdrawals")]
    public class ProfitWithdrawal : BaseModel
    {
        [PrimaryKey("withdrawal_id")]
        public int WithdrawalId { get; set; }

        [Column("date")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("fully_allocated")]
        public bool FullyAllocated { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties will be handled through methods rather than direct properties
    }
}