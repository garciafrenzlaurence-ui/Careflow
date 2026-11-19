using CareFlow.Application.DTOs;
using CareFlow.Application.Interfaces;
using CareFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Repositories;

public sealed class EfPatientRepository(IDbContextFactory<CareFlowDbContext> contextFactory)
    : IPatientRepository
{
    public async Task<bool> ExistsAsync(int patientId, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Patients.AnyAsync(x => x.PatientId == patientId, cancellationToken);
    }

    public async Task<IReadOnlyList<PatientOptionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Patients.AsNoTracking().OrderBy(x => x.PatientNumber)
            .Select(x => new PatientOptionDto
            {
                PatientId = x.PatientId,
                PatientNumber = x.PatientNumber,
                FullName = x.FullName
            }).ToListAsync(cancellationToken);
    }
}
