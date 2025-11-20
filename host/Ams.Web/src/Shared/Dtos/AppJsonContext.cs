using Ams.Web.Shared.Dtos.PushNotification;
using Ams.Web.Shared.Dtos.Identity;
using Ams.Web.Shared.Dtos.Statistics;
using Ams.Web.Shared.Dtos.Diagnostic;

namespace Ams.Web.Shared.Dtos;

/// <summary>
/// https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-source-generator/
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSerializable(typeof(Dictionary<string, string?>))]
[JsonSerializable(typeof(TimeSpan))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(Guid[]))]
[JsonSerializable(typeof(GitHubStats))]
[JsonSerializable(typeof(NugetStatsDto))]
[JsonSerializable(typeof(AppProblemDetails))]
[JsonSerializable(typeof(SendNotificationToRoleDto))]
[JsonSerializable(typeof(PushNotificationSubscriptionDto))]
[JsonSerializable(typeof(VerifyWebAuthnAndSignInRequestDto))]
[JsonSerializable(typeof(WebAuthnAssertionOptionsRequestDto))]

public partial class AppJsonContext : JsonSerializerContext
{
}
