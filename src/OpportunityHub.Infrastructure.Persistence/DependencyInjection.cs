using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpportunityHub.Domain.Repositories;
using OpportunityHub.Infrastructure.Persistence.Repositories;

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

        services.AddScoped<IOpportunityRepository, OpportunityRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}