using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace Ledger.Models
{
    [Table("customers")]
    public class Customer : BaseModel
    {
        [PrimaryKey("customer_id")]
        public int CustomerId { get; set; }

        [Column("customer_name")]
        public string CustomerName { get; set; } = string.Empty;

        [Column("contact_info")]
        public string? ContactInfo { get; set; }

        [Column("fee_percentage")]
        public decimal FeePercentage { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<ChequeTransaction> Transactions { get; set; }
        public virtual ICollection<CustomerDeposit> Deposits { get; set; }
    }
}