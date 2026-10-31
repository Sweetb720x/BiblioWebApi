using BiblioWebApi.Application.Abstractions;
using BiblioWebApi.Application.Services;
using BiblioWebApi.Infrastructure.Persistence;
using BiblioWebApi.Infrastructure.Repositories;
using BiblioWebApi.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BiblioWebApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BiblioDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<ILibraryRules, LibraryRules>();
        services.AddHostedService<ReservationExpirationService>();
        return services;
    }
}
