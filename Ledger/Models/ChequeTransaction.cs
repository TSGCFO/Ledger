using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace Ledger.Models
{
    [Table("cheque_transactions")]
    public class ChequeTransaction : BaseModel
    {
        [PrimaryKey("transaction_id")]
        public int TransactionId { get; set; }

        [Column("date")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Column("cheque_number")]
        public string ChequeNumber { get; set; } = string.Empty;

        [Column("cheque_amount")]
        public decimal ChequeAmount { get; set; }

        [Column("customer_fee")]
        public decimal? CustomerFee { get; set; }

        [Column("net_payable_to_customer")]
        public decimal? NetPayableToCustomer { get; set; }

        [Column("vendor_id")]
        public string VendorId { get; set; } = string.Empty;

        [Column("vendor_fee")]
        public decimal? VendorFee { get; set; }

        [Column("amount_to_receive_from_vendor")]
        public decimal? AmountToReceiveFromVendor { get; set; }

        [Column("profit")]
        public decimal? Profit { get; set; }

        [Column("paid_to_customer")]
        public decimal PaidToCustomer { get; set; } = 0;

        [Column("received_from_vendor")]
        public decimal ReceivedFromVendor { get; set; } = 0;

        [Column("profit_withdrawn")]
        public decimal ProfitWithdrawn { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties will be handled through methods rather than direct properties
    }
}