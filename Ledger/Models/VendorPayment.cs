using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace Ledger.Models
{
    [Table("vendor_payments")]
    public class VendorPayment : BaseModel
    {
        [PrimaryKey("payment_id")]
        public int PaymentId { get; set; }

        [Column("vendor_id")]
        public string VendorId { get; set; } = string.Empty;

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