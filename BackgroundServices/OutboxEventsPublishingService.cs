using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json;

namespace MovieRatingApp.BackgroundServices
{
    public class OutboxEventsPublishingService(IServiceScopeFactory _serviceScopeFactory)
        : BackgroundService
    {
        private readonly TimeSpan _delay = TimeSpan.FromSeconds(10);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();

                    var context = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
                    var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

                    var outboxEvents = await context.EventsOutbox
                        .Where(e => !e.IsHandled)
                        .ToListAsync(stoppingToken);

                    if (outboxEvents.Any())
                    {
                        foreach (var outboxEvent in outboxEvents)
                        {
                            try
                            {
                                var type = Assembly.GetAssembly(typeof(Program))?.GetType(outboxEvent.EventType);
                                if (type == null) continue;

                                var publishableEvent = JsonSerializer.Deserialize(outboxEvent.Notification, type);

                                if (publishableEvent != null)
                                {
                                    await publisher.Publish(publishableEvent, stoppingToken);
                                    outboxEvent.MarkAsHandled();
                                }
                            }
                            catch (Exception ex)
                            {
                                outboxEvent.MarkAsFailed(ex.Message);
                            }
                            finally
                            {
                                context.Update(outboxEvent);
                            }
                        }
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    // مهم جداً تعمل Log هنا عشان لو الداتا بيز وقعت السيرفس متقفش تماماً
                    Console.WriteLine($"Error in Background Service: {ex.Message}");
                }

                // الـ Delay مكانه هنا بره الـ IF عشان يريح البروسيسور في كل الأحوال
                await Task.Delay(_delay, stoppingToken);
            }
        }
    }
}

