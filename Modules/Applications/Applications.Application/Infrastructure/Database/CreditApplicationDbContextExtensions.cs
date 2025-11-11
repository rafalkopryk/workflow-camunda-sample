using Applications.Application.Domain.Application;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.EntityFrameworkCore;

namespace Applications.Application.Infrastructure.Database;

internal static class CreditApplicationDbContextExtensions
{
    extension(CreditApplicationDbContext dbContext)
    {
        public async Task<CreditApplication?> GetCreditApplicationAsync(string applicationId)
        {
            return await dbContext.Applications.FirstOrDefaultAsync(x => x.Id == applicationId);
        }
        
        public async Task<bool> HasCreditApplicationAsync(string applicationId)
        {
            return (await dbContext.Applications.CountAsync(x => x.Id == applicationId)) > 0;
        }
    }
    
    private static readonly Func<CreditApplicationDbContext, string, Task<CreditApplication?>> GetApplication =
        EF.CompileAsyncQuery(
            (CreditApplicationDbContext dbContext, string applicationId) =>
                dbContext.Applications.FirstOrDefault(application => application.Id == applicationId));

    private static readonly Func<CreditApplicationDbContext, string, Task<bool>> HasApplication =
        EF.CompileAsyncQuery(
            (CreditApplicationDbContext dbContext, string applicationId) =>
                dbContext.Applications.Any(application => application.Id == applicationId));
}