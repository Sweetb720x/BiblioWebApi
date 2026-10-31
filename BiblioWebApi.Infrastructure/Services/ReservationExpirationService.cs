using BiblioWebApi.Infrastructure.Persistence;
using BiblioWebApi.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace BiblioWebApi.Infrastructure.Services;

public sealed class ReservationExpirationService(IServiceScopeFactory scopeFactory, ILogger<ReservationExpirationService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<BiblioDbContext>();
                var expired = await db.Reservations.Include(x => x.Copy).Where(x => x.Status == ReservationStatus.Approved && x.PickupExpiresAt < DateTime.UtcNow).ToListAsync(stoppingToken);
                foreach (var reservation in expired)
                {
                    reservation.Status = ReservationStatus.Expired;
                    if (reservation.Copy is not null && reservation.Copy.Status == CopyStatus.Reserved) reservation.Copy.Status = CopyStatus.Available;
                }
                if (expired.Count > 0) await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error processing expired reservations");
            }
        }
    }
}


