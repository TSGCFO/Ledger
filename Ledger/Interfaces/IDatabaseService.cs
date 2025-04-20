using Ledger.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ledger.Interfaces
{
    public interface IDatabaseService
    {
        // Authentication
        Task<bool> InitializeAsync(string apiKey, string apiUrl);

        // Customers
        Task<List<Customer>> GetCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(int customerId);
        Task<Customer> AddCustomerAsync(Customer customer);
        Task<bool> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int customerId);

        // Vendors
        Task<List<Vendor>> GetVendorsAsync();
        Task<Vendor?> GetVendorByIdAsync(string vendorId);
        Task<Vendor> AddVendorAsync(Vendor vendor);
        Task<bool> UpdateVendorAsync(Vendor vendor);
        Task<bool> DeleteVendorAsync(string vendorId);

        // Transactions
        Task<List<ChequeTransaction>> GetTransactionsAsync();
        Task<ChequeTransaction?> GetTransactionByIdAsync(int transactionId);
        Task<List<ChequeTransaction>> GetTransactionsByCustomerAsync(int customerId);
        Task<List<ChequeTransaction>> GetTransactionsByVendorAsync(string vendorId);
        Task<ChequeTransaction> AddTransactionAsync(ChequeTransaction transaction);
        Task<bool> UpdateTransactionAsync(ChequeTransaction transaction);
        Task<bool> DeleteTransactionAsync(int transactionId);

        // Customer Deposits
        Task<List<CustomerDeposit>> GetCustomerDepositsAsync(int customerId);
        Task<CustomerDeposit> AddCustomerDepositAsync(CustomerDeposit deposit);

        // Vendor Payments
        Task<List<VendorPayment>> GetVendorPaymentsAsync(string vendorId);
        Task<VendorPayment> AddVendorPaymentAsync(VendorPayment payment);

        // Profit Withdrawals
        Task<List<ProfitWithdrawal>> GetProfitWithdrawalsAsync();
        Task<ProfitWithdrawal> AddProfitWithdrawalAsync(ProfitWithdrawal withdrawal);
    }
}