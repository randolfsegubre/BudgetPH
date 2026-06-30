using FinanceManager.Application.Features.Reports.DTOs;
using FinanceManager.Application.Common.Models;

namespace FinanceManager.Application.Interfaces;

public interface IReportService
{
    Task<Result<ReportResultDto>> GenerateReportAsync(Guid userId, ReportRequestDto request, CancellationToken cancellationToken = default);
    Task<byte[]> GeneratePdfAsync(Guid userId, ReportRequestDto request, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateExcelAsync(Guid userId, ReportRequestDto request, CancellationToken cancellationToken = default);
}
