using Applications.Application.Domain.Application;
using Microsoft.EntityFrameworkCore;

namespace Applications.Application.Infrastructure.Database;

internal static class CreditApplicationDbContextExtensions
{
    private static readonly Func<CreditApplicationDbContext, string, Task<CreditApplication?>> GetApplication =
        EF.CompileAsyncQuery(
            (CreditApplicationDbContext dbContext, string applicationId) =>
                dbContext.Applications.FirstOrDefault(application => application.ApplicationId == applicationId));

    public static async Task<CreditApplication?> GetCreditApplicationAsync(this CreditApplicationDbContext dbContext, string applicationId)
    {
        return await GetApplication(dbContext, applicationId);
    }

    private static readonly Func<CreditApplicationDbContext, string, Task<bool>> HasApplication =
        EF.CompileAsyncQuery(
            (CreditApplicationDbContext dbContext, string applicationId) =>
                dbContext.Applications.Any(application => application.ApplicationId == applicationId));

    public static async Task<bool> HasCreditApplicationAsync(this CreditApplicationDbContext dbContext, string applicationId)
    {
        return await HasApplication(dbContext, applicationId);
    }
}