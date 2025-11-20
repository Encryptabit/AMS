using AdsPush;
using AdsPush.Abstraction;
using System.Collections.Concurrent;
using Hangfire.Server;

namespace Ams.Web.Server.Api.Services.Jobs;

public partial class PushNotificationJobRunner
{
    [AutoInject] private AppDbContext dbContext = default!;
    [AutoInject] private IAdsPushSender adsPushSender = default!;
    [AutoInject] private ServerExceptionHandler serverExceptionHandler = default!;

    public async Task RequestPush(int[] pushNotificationSubscriptionIds,
        PushNotificationRequest request,
        PerformContext context = null!,
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await dbContext.PushNotificationSubscriptions
            .Where(pns => pushNotificationSubscriptionIds.Contains(pns.Id))
            .ToArrayAsync(cancellationToken);

        var payload = new AdsPushBasicSendPayload()
        {
            Title = AdsPushText.CreateUsingString(request.Title ?? "Ams.Web push"),
            Detail = AdsPushText.CreateUsingString(request.Message ?? string.Empty)
        };

        if (string.IsNullOrEmpty(request.Action) is false)
        {
            payload.Parameters.Add("action", request.Action);
        }
        if (string.IsNullOrEmpty(request.PageUrl) is false)
        {
            payload.Parameters.Add("pageUrl", request.PageUrl);
        }


        ConcurrentBag<Exception> exceptions = [];
        ConcurrentBag<int> problematicSubscriptionIds = [];

        await Parallel.ForEachAsync(subscriptions, parallelOptions: new()
        {
            MaxDegreeOfParallelism = 10,
            CancellationToken = cancellationToken
        }, async (subscription, cancellationToken) =>
        {
            try
            {
                var target = subscription.Platform is "browser" ? AdsPushTarget.BrowserAndPwa
                                    : subscription.Platform is "fcmV1" ? AdsPushTarget.Android
                                    : subscription.Platform is "apns" ? AdsPushTarget.Ios
                                    : throw new NotImplementedException();

                await adsPushSender.BasicSendAsync(target, subscription.PushChannel, payload, default);

            }
            catch (Exception exp)
            {
                exceptions.Add(exp);
                problematicSubscriptionIds.Add(subscription.Id);
            }
        });

        if (exceptions.IsEmpty is false)
        {
            serverExceptionHandler.Handle(new AggregateException("Failed to send push notifications", exceptions)
                .WithData(new()
                {
                    { "UserRelatedPush", request.UserRelatedPush },
                    { "JobId", context.BackgroundJob.Id  },
                    { "ProblematicSubscriptionIds", problematicSubscriptionIds }
                }));
        }
    }
}
