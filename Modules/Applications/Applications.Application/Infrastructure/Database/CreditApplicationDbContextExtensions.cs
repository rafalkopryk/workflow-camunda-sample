using Applications.Application.Domain.Application;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.EntityFrameworkCore;

namespace Applications.Application.Infrastructure.Database;

internal static class CreditApplicationDbContextExtensions
{
    private static readonly Func<CreditApplicationDbContext, string, Task<CreditApplication?>> GetApplication =
        EF.CompileAsyncQuery(
            (CreditApplicationDbContext dbContext, string applicationId) =>
                dbContext.Applications.FirstOrDefault(application => application.Id == applicationId));

    public static async Task<CreditApplication?> GetCreditApplicationAsync(this CreditApplicationDbContext dbContext, string applicationId)
    {
        try
        {
            return await dbContext.Applications.FirstOrDefaultAsync(x => x.Id == applicationId);

        }
        catch (Exception ex)
        {
            throw;
        }

        //return await GetApplication(dbContext, applicationId);
    }

    private static readonly Func<CreditApplicationDbContext, string, Task<bool>> HasApplication =
        EF.CompileAsyncQuery(
            (CreditApplicationDbContext dbContext, string applicationId) =>
                dbContext.Applications.Any(application => application.Id == applicationId));

    public static async Task<bool> HasCreditApplicationAsync(this CreditApplicationDbContext dbContext, string applicationId)
    {
        return (await dbContext.Applications.CountAsync(x => x.Id == applicationId)) > 0;

        //return await HasApplication(dbContext, applicationId);
    }
}