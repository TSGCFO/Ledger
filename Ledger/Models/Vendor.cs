using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace Ledger.Models
{
    [Table("vendors")]
    public class Vendor : BaseModel
    {
        [PrimaryKey("vendor_id")]
        public string VendorId { get; set; } = string.Empty;

        [Column("vendor_name")]
        public string VendorName { get; set; } = string.Empty;

        [Column("fee_percentage")]
        public decimal FeePercentage { get; set; }

        [Column("contact_info")]
        public string? ContactInfo { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties will be handled through methods rather than direct properties
    }
}