using System.Web;
using BlazorApplicationInsights.Interfaces;
using Microsoft.AspNetCore.Components.Routing;
using Ams.Web.Shared.Controllers.Identity;
using Ams.Web.Client.Core.Services.DiagnosticLog;

namespace Ams.Web.Client.Core.Components;

/// <summary>
/// Manages the initialization and coordination of core services and settings within the client application.
/// This includes authentication state handling, telemetry setup, culture configuration, and optional
/// services such as SignalR connections, push notifications, and application insights.
/// </summary>
public partial class AppClientCoordinator : AppComponentBase
{
    [AutoInject] private IApplicationInsights appInsights = default!;
    [AutoInject] private UserAgent userAgent = default!;
    [AutoInject] private IJSRuntime jsRuntime = default!;
    [AutoInject] private IUserController userController = default!;
    [AutoInject] private IStorageService storageService = default!;
    [AutoInject] private ILogger<AuthManager> authLogger = default!;
    [AutoInject] private ILogger<Navigator> navigatorLogger = default!;
    [AutoInject] private ILogger<AppClientCoordinator> logger = default!;
    [AutoInject] private IBitDeviceCoordinator bitDeviceCoordinator = default!;

    private List<Action> unsubscribes = [];

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();

        if (AppPlatform.IsBlazorHybrid)
        {
            await ConfigureUISetup();
        }

        if (InPrerenderSession is false)
        {
            unsubscribes.Add(PubSubService.Subscribe(ClientAppMessages.NAVIGATE_TO, async (uri) =>
            {
                var uriValue = uri?.ToString()!;
                var replace = uriValue.Contains("replace=true", StringComparison.InvariantCultureIgnoreCase);
                var forceLoad = uriValue.Contains("forceLoad=true", StringComparison.InvariantCultureIgnoreCase);
                NavigationManager.NavigateTo(uriValue.Replace("replace=true", "", StringComparison.InvariantCultureIgnoreCase).Replace("forceLoad=true", "", StringComparison.InvariantCultureIgnoreCase).TrimEnd('&'), forceLoad, replace);
            }));

            if (AppPlatform.IsBlazorHybrid is false)
            {
                try
                {
                    BitButil.UseFastInvoke(); // Ensures that `TelemetryContext.Platform` is available to components using this value in their `OnInitAsync` method, such as `SignInPage.razor.cs`.
                    var userAgentData = await userAgent.Extract();
                    TelemetryContext.Platform = string.Join(' ', [userAgentData.Manufacturer, userAgentData.OsName, userAgentData.Name, "browser"]);
                }
                finally
                {
                    BitButil.UseNormalInvoke();
                }
            }
            TelemetryContext.TimeZone = await jsRuntime.GetTimeZone();
            TelemetryContext.Culture = CultureInfo.CurrentCulture.Name;
            TelemetryContext.PageUrl = HttpUtility.UrlDecode(NavigationManager.Uri);

            _ = appInsights.AddTelemetryInitializer(new()
            {
                Data = new()
                {
                    ["ai.application.ver"] = TelemetryContext.AppVersion,
                    ["ai.session.id"] = TelemetryContext.AppSessionId,
                    ["ai.device.locale"] = TelemetryContext.Culture
                }
            });

            NavigationManager.LocationChanged += NavigationManager_LocationChanged;
            AuthManager.AuthenticationStateChanged += AuthenticationStateChanged;
            await PropagateAuthState(firstRun: true, AuthenticationStateTask);
        }
    }

    private void NavigationManager_LocationChanged(object? sender, LocationChangedEventArgs e)
    {
        TelemetryContext.PageUrl = HttpUtility.UrlDecode(e.Location);
        navigatorLogger.LogInformation("Navigator's location changed to {Location}", TelemetryContext.PageUrl);
    }

    private Guid? lastPropagatedUserId = Guid.Empty;
    /// <summary>
    /// This code manages the association of a user with sensitive services, such as SignalR, push notifications, App Insights, and others, 
    /// ensuring the user is correctly set or cleared as needed.
    /// </summary>
    public async Task PropagateAuthState(bool firstRun, Task<AuthenticationState> task)
    {
        try
        {
            var user = (await task).User;
            var isAuthenticated = user.IsAuthenticated();
            var userId = isAuthenticated ? user.GetUserId() : (Guid?)null;
            if (lastPropagatedUserId == userId)
                return;
            await Abort(); // Cancels ongoing user id propagation, because the new authentication state is available.
            TelemetryContext.UserId = userId;
            TelemetryContext.UserSessionId = isAuthenticated ? user.GetSessionId() : null;

            // Typically, we use the logger directly without utilizing logger.BeginScope.
            // While many loggers provide specific methods to set userId and other context-related information,
            // we use this method to propagate the user ID and other telemetry contexts via Microsoft.Extensions.Logging's Scope feature.
            // PropagateUserId method is invoked both during app startup and when the authentication state changes.
            // Additionally, this is a convenient place to manage user-specific contexts for services like:
            // - App Insights: Set or clear the user ID for tracking purposes.
            // - Push Notifications: Update subscriptions to ensure user-specific notifications are routed to the correct devices.
            // - SignalR: Map connection IDs to a user's group of connections for message targeting.
            // By leveraging this method during authentication state changes, we streamline the propagation of user-specific contexts across these systems.

            if (isAuthenticated)
            {
                _ = appInsights.SetAuthenticatedUserContext(user.GetUserId().ToString());
            }
            else
            {
                _ = appInsights.ClearAuthenticatedUserContext();
            }

            var data = TelemetryContext.ToDictionary();
            using var scope = authLogger.BeginScope(data);
            {
                authLogger.LogInformation("Propagating {AuthStateType} {AuthState} authentication state.", firstRun ? "Initial" : "Updated", user.IsAuthenticated() ? "Authenticated" : "Anonymous");
            }



            if (isAuthenticated)
            {
                await UpdateUserSession();
            }

            lastPropagatedUserId = userId;
        }
        catch (Exception exp)
        {
            ExceptionHandler.Handle(exp);
        }
    }

    private void AuthenticationStateChanged(Task<AuthenticationState> task)
    {
        _ = PropagateAuthState(firstRun: false, task);
    }


    private async Task UpdateUserSession()
    {
        await userController.UpdateSession(new()
        {
            AppVersion = TelemetryContext.AppVersion,
            DeviceInfo = TelemetryContext.Platform,
            CultureName = CultureInfoManager.InvariantGlobalization ? null : CultureInfo.CurrentUICulture.Name,
            PlatformType = AppPlatform.Type
        }, CurrentCancellationToken);
    }

    private async Task ConfigureUISetup()
    {
        if (CultureInfoManager.InvariantGlobalization is false)
        {
            CultureInfoManager.SetCurrentCulture(new Uri(NavigationManager.Uri).GetCulture() ??  // 1- Culture query string OR Route data request culture
                                                 await storageService.GetItem("Culture") ?? // 2- User settings
                                                 CultureInfo.CurrentUICulture.Name); // 3- OS settings
        }
    }

    private List<IDisposable> signalROnDisposables = [];
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        unsubscribes.ForEach(unsubscribe => unsubscribe());
        unsubscribes = [];

        NavigationManager.LocationChanged -= NavigationManager_LocationChanged;
        AuthManager.AuthenticationStateChanged -= AuthenticationStateChanged;


        await base.DisposeAsync(disposing);
    }
}
