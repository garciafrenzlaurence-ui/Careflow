using CareFlow.Application.DTOs;

namespace CareFlow.Application.Interfaces;

public interface IPatientRepository
{
    Task<bool> ExistsAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PatientOptionDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
