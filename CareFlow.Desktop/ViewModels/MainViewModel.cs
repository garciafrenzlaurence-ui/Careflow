using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CareFlow.Application.DTOs;
using CareFlow.Application.Interfaces;
using CareFlow.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareFlow.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly BookingApplicationService? _bookingService;
    private readonly IAvailabilityService? _availabilityService;
    private readonly IPatientRepository? _patientRepository;
    private bool _isRevertingSelection;

    [ObservableProperty] private PatientOptionDto? selectedPatient;
    [ObservableProperty] private DateTimeOffset? bookingDate = DateTimeOffset.Now;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool hasResult;
    [ObservableProperty] private string resultMessage = string.Empty;
    [ObservableProperty] private bool isSuccessful;
    [ObservableProperty] private bool isError;
    [ObservableProperty] private string availabilityDateLabel = string.Empty;

    public ObservableCollection<PatientOptionDto> Patients { get; } = new();
    public ObservableCollection<DiagnosticServiceOption> DiagnosticServices { get; } = new();
    public ObservableCollection<BookingListItemDto> Bookings { get; } = new();
    public int SelectedServiceCount => DiagnosticServices.Count(x => x.IsSelected);

    public MainViewModel() { }

    public MainViewModel(BookingApplicationService bookingService, IAvailabilityService availabilityService, IPatientRepository patientRepository)
    {
        _bookingService = bookingService;
        _availabilityService = availabilityService;
        _patientRepository = patientRepository;
    }

    partial void OnBookingDateChanged(DateTimeOffset? value)
    {
        if (value is not null && _availabilityService is not null)
            _ = RefreshAvailabilityAsync();
    }

    public async Task InitializeAsync()
    {
        await LoadPatientsAsync();
        await RefreshAvailabilityAsync();
        await RefreshBookingsAsync();
    }

    private async Task LoadPatientsAsync()
    {
        // Patient selection is supplied by the database, not free-form patient entry.
        if (_patientRepository is null) return;
        var patients = await _patientRepository.GetAllAsync();
        Patients.Clear();
        foreach (var patient in patients) Patients.Add(patient);
        SelectedPatient = Patients.FirstOrDefault();
    }

    private void OnDiagnosticServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(DiagnosticServiceOption.IsSelected)) return;
        OnPropertyChanged(nameof(SelectedServiceCount));
        if (_isRevertingSelection) return;
        if (sender is DiagnosticServiceOption option && option.IsSelected && SelectedServiceCount > 3)
        {
            _isRevertingSelection = true;
            option.IsSelected = false;
            _isRevertingSelection = false;
            OnPropertyChanged(nameof(SelectedServiceCount));
            ShowResult("You can select a maximum of three diagnostic services.", false);
            return;
        }
        HasResult = false;
    }

    [RelayCommand]
    private async Task RefreshAvailabilityAsync()
    {
        if (_availabilityService is null || BookingDate is null) return;
        try
        {
            var date = DateOnly.FromDateTime(BookingDate.Value.DateTime);
            var rows = await _availabilityService.GetAvailabilityAsync(date);
            foreach (var row in rows)
            {
                var option = DiagnosticServices.FirstOrDefault(x => x.DiagnosticServiceId == row.DiagnosticServiceId);
                if (option is null)
                {
                    option = new DiagnosticServiceOption(row.ServiceName, row.DiagnosticServiceId);
                    option.PropertyChanged += OnDiagnosticServicePropertyChanged;
                    DiagnosticServices.Add(option);
                }
                option.DailyCapacity = row.DailyCapacity;
                option.BookedCount = row.BookedCount;
                option.RemainingCapacity = row.RemainingCapacity;
            }
            AvailabilityDateLabel = date.ToString("MMMM dd, yyyy");
        }
        catch (Exception ex)
        {
            ShowResult(ex.Message, false);
        }
    }

    [RelayCommand]
    private async Task RefreshBookingsAsync()
    {
        if (_bookingService is null) return;
        try
        {
            var rows = await _bookingService.GetConfirmedBookingsAsync();
            Bookings.Clear();
            foreach (var row in rows) Bookings.Add(row);
        }
        catch
        {
            ShowResult("Previously confirmed bookings could not be retrieved.", false);
        }
    }

    [RelayCommand]
    private async Task SubmitBookingAsync()
    {
        if (IsBusy) return;
        if (SelectedPatient is null)
        { ShowResult("Please select an existing patient.", false); return; }
        if (BookingDate is null)
        { ShowResult("Please select a booking date.", false); return; }

        DateOnly date = DateOnly.FromDateTime(BookingDate.Value.DateTime);
        if (date < DateOnly.FromDateTime(DateTime.Today))
        { ShowResult("The booking date cannot be earlier than today.", false); return; }

        var selected = DiagnosticServices.Where(x => x.IsSelected).ToList();
        if (selected.Count == 0)
        { ShowResult("Please select at least one diagnostic service.", false); return; }
        if (selected.Count > 3)
        { ShowResult("You can select a maximum of three diagnostic services.", false); return; }
        if (_bookingService is null)
        { ShowResult("The booking service has not been configured.", false); return; }

        try
        {
            IsBusy = true;
            var request = new CreateBookingRequest
            {
                PatientId = SelectedPatient.PatientId,
                BookingDate = date,
                DiagnosticServiceIds = selected.Select(x => x.DiagnosticServiceId).ToList()
            };
            BookingResultDto result = await _bookingService.CreateBookingAsync(request);
            if (result.Success && result.BookingId.HasValue)
            {
                foreach (var service in DiagnosticServices) service.IsSelected = false;
                await RefreshAvailabilityAsync();
                await RefreshBookingsAsync();
                ShowResult($"Booking confirmed successfully. Booking ID: {result.BookingId.Value}", true);
            }
            else ShowResult(result.Message, false);
        }
        catch
        {
            ShowResult("The booking could not be completed. No success was recorded. Please try again.", false);
        }
        finally { IsBusy = false; }
    }

    public void ShowStartupError(string message) => ShowResult(message, false);

    private void ShowResult(string message, bool success)
    {
        ResultMessage = message;
        IsSuccessful = success;
        IsError = !success;
        HasResult = true;
    }
}

public partial class DiagnosticServiceOption : ObservableObject
{
    public string ServiceName { get; }
    public int DiagnosticServiceId { get; }
    [ObservableProperty] private bool isSelected;
    [ObservableProperty] private int dailyCapacity;
    [ObservableProperty] private int bookedCount;
    [ObservableProperty] private int remainingCapacity;

    public DiagnosticServiceOption(string serviceName, int diagnosticServiceId)
    { ServiceName = serviceName; DiagnosticServiceId = diagnosticServiceId; }
}
