using Ledger.Interfaces;
using Ledger.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ledger.ViewModels
{
    public class TransactionEntryViewModel : ViewModelBase
    {
        private readonly IAiAssistantService _aiService;
        private readonly IDatabaseService _databaseService;
        private ChequeTransaction _transaction;
        private string _statusMessage = string.Empty;
        private bool _isSuccess;
        private bool _hasImage;
        private ImageSource _chequeImage;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Vendor> _vendors;
        private Customer _selectedCustomer;
        private Vendor _selectedVendor;

        public TransactionEntryViewModel(IAiAssistantService aiService, IDatabaseService databaseService)
        {
            _aiService = aiService;
            _databaseService = databaseService;
            Title = "New Transaction";

            // Initialize properties
            Transaction = new ChequeTransaction
            {
                Date = DateTime.Today,
                ChequeAmount = 0
            };

            Customers = new ObservableCollection<Customer>();
            Vendors = new ObservableCollection<Vendor>();

            // Initialize commands
            CaptureImageCommand = new Command(async () => await CaptureImageAsync());
            SelectImageCommand = new Command(async () => await SelectImageAsync());
            ExtractDataCommand = new Command(async () => await ExtractDataAsync(), () => HasImage);
            SaveCommand = new Command(async () => await SaveTransactionAsync(), () => CanSaveTransaction());
            ClearCommand = new Command(ClearTransaction);
        }

        public ChequeTransaction Transaction
        {
            get => _transaction;
            set => SetProperty(ref _transaction, value);
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public ObservableCollection<Vendor> Vendors
        {
            get => _vendors;
            set => SetProperty(ref _vendors, value);
        }

        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value) && value != null)
                {
                    Transaction.CustomerId = value.CustomerId;
                    (SaveCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        public Vendor SelectedVendor
        {
            get => _selectedVendor;
            set
            {
                if (SetProperty(ref _selectedVendor, value) && value != null)
                {
                    Transaction.VendorId = value.VendorId;
                    (SaveCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsSuccess
        {
            get => _isSuccess;
            set => SetProperty(ref _isSuccess, value);
        }

        public bool HasImage
        {
            get => _hasImage;
            set
            {
                if (SetProperty(ref _hasImage, value))
                {
                    (ExtractDataCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        public ImageSource ChequeImage
        {
            get => _chequeImage;
            set => SetProperty(ref _chequeImage, value);
        }

        public ICommand CaptureImageCommand { get; }
        public ICommand SelectImageCommand { get; }
        public ICommand ExtractDataCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ClearCommand { get; }

        private async Task CaptureImageAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                if (!MediaPicker.IsCaptureSupported)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Camera is not supported on this device.", "OK");
                    return;
                }

                var photo = await MediaPicker.CapturePhotoAsync();

                if (photo != null)
                {
                    // Save the image to local storage
                    var localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
                    using (var sourceStream = await photo.OpenReadAsync())
                    using (var destinationStream = File.Create(localFilePath))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }

                    // Display the captured image
                    ChequeImage = ImageSource.FromFile(localFilePath);
                    HasImage = true;

                    // Automatically extract data from the image
                    await ExtractDataAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error capturing image: {ex.Message}";
                IsSuccess = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SelectImageAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                var photo = await MediaPicker.PickPhotoAsync();

                if (photo != null)
                {
                    // Save the image to local storage
                    var localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
                    using (var sourceStream = await photo.OpenReadAsync())
                    using (var destinationStream = File.Create(localFilePath))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }

                    // Display the selected image
                    ChequeImage = ImageSource.FromFile(localFilePath);
                    HasImage = true;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error selecting image: {ex.Message}";
                IsSuccess = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExtractDataAsync()
        {
            if (IsBusy || !HasImage || ChequeImage == null)
                return;

            try
            {
                IsBusy = true;
                StatusMessage = "Extracting data from image...";

                // Get the file path from the image source
                var imageFile = (ChequeImage as FileImageSource)?.File;
                if (string.IsNullOrEmpty(imageFile))
                {
                    StatusMessage = "Error: Invalid image file.";
                    IsSuccess = false;
                    return;
                }

                // Open the image file as a stream
                using var stream = File.OpenRead(imageFile);

                // Use the AI service to extract data from the image
                var extractedTransaction = await _aiService.ExtractDataFromImageAsync(stream);

                // Update the transaction with extracted data
                if (extractedTransaction != null)
                {
                    // Keep the current customer and vendor selections if they exist
                    var customerId = SelectedCustomer?.CustomerId ?? extractedTransaction.CustomerId;
                    var vendorId = SelectedVendor?.VendorId ?? extractedTransaction.VendorId;

                    // Update the transaction properties
                    Transaction.ChequeNumber = extractedTransaction.ChequeNumber;
                    Transaction.ChequeAmount = extractedTransaction.ChequeAmount;
                    Transaction.Date = extractedTransaction.Date;
                    Transaction.CustomerId = customerId;
                    Transaction.VendorId = vendorId;

                    // Update the selected customer and vendor if needed
                    if (SelectedCustomer == null || SelectedCustomer.CustomerId != customerId)
                    {
                        SelectedCustomer = Customers.FirstOrDefault(c => c.CustomerId == customerId);
                    }

                    if (SelectedVendor == null || SelectedVendor.VendorId != vendorId)
                    {
                        SelectedVendor = Vendors.FirstOrDefault(v => v.VendorId == vendorId);
                    }

                    StatusMessage = "Data extracted successfully!";
                    IsSuccess = true;
                }
                else
                {
                    StatusMessage = "Could not extract data from the image.";
                    IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error extracting data: {ex.Message}";
                IsSuccess = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveTransactionAsync()
        {
            if (IsBusy || !CanSaveTransaction())
                return;

            try
            {
                IsBusy = true;
                StatusMessage = "Saving transaction...";

                // Ensure required fields are set
                Transaction.CustomerId = SelectedCustomer?.CustomerId ?? Transaction.CustomerId;
                Transaction.VendorId = SelectedVendor?.VendorId ?? Transaction.VendorId;

                // Save the transaction to the database
                var savedTransaction = await _databaseService.AddTransactionAsync(Transaction);

                if (savedTransaction != null)
                {
                    StatusMessage = "Transaction saved successfully!";
                    IsSuccess = true;

                    // Clear the form for a new entry
                    ClearTransaction();
                }
                else
                {
                    StatusMessage = "Error saving transaction.";
                    IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving transaction: {ex.Message}";
                IsSuccess = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ClearTransaction()
        {
            // Reset the transaction
            Transaction = new ChequeTransaction
            {
                Date = DateTime.Today,
                ChequeAmount = 0
            };

            // Clear selections
            SelectedCustomer = null;
            SelectedVendor = null;

            // Clear the image
            ChequeImage = null;
            HasImage = false;

            // Clear status message
            StatusMessage = string.Empty;
            IsSuccess = false;
        }

        private bool CanSaveTransaction()
        {
            return !string.IsNullOrEmpty(Transaction.ChequeNumber) &&
                   Transaction.ChequeAmount > 0 &&
                   Transaction.CustomerId > 0 &&
                   !string.IsNullOrEmpty(Transaction.VendorId);
        }

        public override async Task InitializeAsync(object? parameter)
        {
            try
            {
                IsBusy = true;

                // Load customers and vendors
                var customers = await _databaseService.GetCustomersAsync();
                var vendors = await _databaseService.GetVendorsAsync();

                // Clear and add to observable collections
                Customers.Clear();
                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }

                Vendors.Clear();
                foreach (var vendor in vendors)
                {
                    Vendors.Add(vendor);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading data: {ex.Message}";
                IsSuccess = false;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}