using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace Ledger.Models
{
    [Table("profit_withdrawal_allocations")]
    public class ProfitWithdrawalAllocation : BaseModel
    {
        [PrimaryKey("allocation_id")]
        public int AllocationId { get; set; }

        [Column("withdrawal_id")]
        public int WithdrawalId { get; set; }

        [Column("transaction_id")]
        public int TransactionId { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navigation properties will be handled through methods rather than direct properties
    }
}