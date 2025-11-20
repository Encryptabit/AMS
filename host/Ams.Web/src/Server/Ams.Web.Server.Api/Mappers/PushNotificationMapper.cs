using Ams.Web.Server.Api.Models.PushNotification;
using Ams.Web.Shared.Dtos.PushNotification;
using Riok.Mapperly.Abstractions;

namespace Ams.Web.Server.Api.Mappers;

/// <summary>
/// More info at Server/Mappers/README.md
/// </summary>
[Mapper]
public static partial class PushNotificationMapper
{
    public static partial void Patch(this PushNotificationSubscriptionDto source, PushNotificationSubscription destination);
}
