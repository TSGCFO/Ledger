using Ledger.Config;
using Ledger.Interfaces;
using Ledger.Models;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Postgrest = Supabase.Postgrest;

namespace Ledger.Infrastructure.Database
{
    public class SupabaseDatabaseService : IDatabaseService
    {
        private Client? _supabaseClient;
        private readonly SupabaseConfig _config;
        private bool _isInitialized = false;

        public SupabaseDatabaseService(SupabaseConfig config)
        {
            _config = config;
        }

        public async Task<bool> InitializeAsync(string apiKey, string apiUrl)
        {
            try
            {
                _config.ApiKey = apiKey;
                _config.ApiUrl = apiUrl;

                var options = new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = true
                };

                _supabaseClient = new Client(apiUrl, apiKey, options);
                await _supabaseClient.InitializeAsync();
                _isInitialized = true;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize Supabase: {ex.Message}");
                return false;
            }
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized || _supabaseClient == null)
                throw new InvalidOperationException("Supabase client is not initialized. Call InitializeAsync first.");
        }

        // Customers
        public async Task<List<Customer>> GetCustomersAsync()
        {
            EnsureInitialized();
            try
            {
                var response = await _supabaseClient!.From<Customer>().Get();
                return response.Models;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customers: {ex.Message}");
                return new List<Customer>();
            }
        }

        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            EnsureInitialized();
            try
            {
                var response = await _supabaseClient!.From<Customer>()
                    .Filter("customer_id", Postgrest.Constants.Operator.Equals, customerId)
                    .Single();
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customer by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<Customer> AddCustomerAsync(Customer customer)
        {
            EnsureInitialized();
            try
            {
                var response = await _supabaseClient!.From<Customer>().Insert(customer);
                return response.Models.FirstOrDefault() ?? customer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding customer: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            EnsureInitialized();
            try
            {
                var response = await _supabaseClient!.From<Customer>()
                    .Filter("customer_id", Postgrest.Constants.Operator.Equals, customer.CustomerId)
                    .Update(customer);
                return response.Models.Any();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating customer: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            EnsureInitialized();
            try
            {
                await _supabaseClient!.From<Customer>()
                    .Filter("customer_id", Postgrest.Constants.Operator.Equals, customerId)
                    .Delete();

                // Since we can't easily check if anything was deleted, we'll return true
                // if no exception was thrown
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting customer: {ex.Message}");
                return false;
            }
        }

        // Implement other methods from IDatabaseService
        // For brevity, I've only implemented the Customer-related methods
        // You would implement the remaining methods following the same pattern

        #region Not Implemented Yet

        public Task<List<Vendor>> GetVendorsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Vendor?> GetVendorByIdAsync(string vendorId)
        {
            throw new NotImplementedException();
        }

        public Task<Vendor> AddVendorAsync(Vendor vendor)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateVendorAsync(Vendor vendor)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteVendorAsync(string vendorId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ChequeTransaction>> GetTransactionsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ChequeTransaction?> GetTransactionByIdAsync(int transactionId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ChequeTransaction>> GetTransactionsByCustomerAsync(int customerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ChequeTransaction>> GetTransactionsByVendorAsync(string vendorId)
        {
            throw new NotImplementedException();
        }

        public Task<ChequeTransaction> AddTransactionAsync(ChequeTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateTransactionAsync(ChequeTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteTransactionAsync(int transactionId)
        {
            throw new NotImplementedException();
        }

        public Task<List<CustomerDeposit>> GetCustomerDepositsAsync(int customerId)
        {
            throw new NotImplementedException();
        }

        public Task<CustomerDeposit> AddCustomerDepositAsync(CustomerDeposit deposit)
        {
            throw new NotImplementedException();
        }

        public Task<List<VendorPayment>> GetVendorPaymentsAsync(string vendorId)
        {
            throw new NotImplementedException();
        }

        public Task<VendorPayment> AddVendorPaymentAsync(VendorPayment payment)
        {
            throw new NotImplementedException();
        }

        public Task<List<ProfitWithdrawal>> GetProfitWithdrawalsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProfitWithdrawal> AddProfitWithdrawalAsync(ProfitWithdrawal withdrawal)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}