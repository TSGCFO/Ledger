using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace Ledger.Models
{
    [Table("vendor_payment_allocations")]
    public class VendorPaymentAllocation : BaseModel
    {
        [PrimaryKey("allocation_id")]
        public int AllocationId { get; set; }

        [Column("payment_id")]
        public int PaymentId { get; set; }

        [Column("transaction_id")]
        public int TransactionId { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navigation properties will be handled through methods rather than direct properties
    }
}