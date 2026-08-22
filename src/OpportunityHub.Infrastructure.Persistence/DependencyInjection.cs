using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpportunityHub.Infrastructure.Persistence;

public static class DependencyInjection
{
   public static IServiceCollection AddPersistence(
    this IServiceCollection services,
    string connectionString)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

    services.AddDbContext<OpportunityHubDbContext>(
        options =>
        {
            options.UseSqlServer(connectionString);
        });

    return services;
}
}