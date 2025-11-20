using Ams.Web.Shared.Dtos.PushNotification;

namespace Ams.Web.Shared.Controllers.PushNotification;

[Route("api/[controller]/[action]/")]
public interface IPushNotificationController : IAppController
{
    [HttpPost]
    Task Subscribe([Required] PushNotificationSubscriptionDto subscription, CancellationToken cancellationToken);
}
