using Ams.Web.Client.Core.Components;

namespace Ams.Web.Client.Core.Services;

public partial class ClientAppMessages
{
    /// <summary>
    /// A publisher that publishes this message wants a snack bar to be shown.
    /// </summary>
    public const string SHOW_SNACK = nameof(SHOW_SNACK);

    /// <summary>
    /// A publisher that publishes this message wants a modal to be shown.
    /// </summary>
    public const string SHOW_MODAL = nameof(SHOW_MODAL);

    /// <summary>
    /// A publisher that publishes this message wants a modal to be closed.
    /// </summary>
    public const string CLOSE_MODAL = nameof(CLOSE_MODAL);

    /// <summary>
    /// This message gets published when the app theme is changed.
    /// </summary>
    public const string THEME_CHANGED = nameof(THEME_CHANGED);

    /// <summary>
    /// A publisher that publishes this message wants the navigation panel to be opened.
    /// </summary>
    public const string OPEN_NAV_PANEL = nameof(OPEN_NAV_PANEL);

    /// <summary>
    /// A publisher that publishes this message wants the navigation panel to be closed.
    /// </summary>
    public const string CLOSE_NAV_PANEL = nameof(CLOSE_NAV_PANEL);

    /// <summary>
    /// When the culture is changed, this message is published.
    /// </summary>
    public const string CULTURE_CHANGED = nameof(CULTURE_CHANGED);

    /// <summary>
    /// <inheritdoc cref="Parameters.IsOnline"/>
    /// </summary>
    public const string IS_ONLINE_CHANGED = nameof(IS_ONLINE_CHANGED);

    /// <summary>
    /// When the data of the current page is changed, this message is published.
    /// </summary>
    public const string PAGE_DATA_CHANGED = nameof(PAGE_DATA_CHANGED);

    /// <summary>
    /// When the route data is updated, this message is published.
    /// </summary>
    public const string ROUTE_DATA_UPDATED = nameof(ROUTE_DATA_UPDATED);

    /// <summary>
    /// Supposed to be called using JavaScript to navigate between pages without reloading the app.
    /// </summary>
    public const string NAVIGATE_TO = nameof(NAVIGATE_TO);

    /// <summary>
    /// A publisher that publishes this message wants the diagnostic modal to be shown.
    /// </summary>
    public const string SHOW_DIAGNOSTIC_MODAL = nameof(SHOW_DIAGNOSTIC_MODAL);

    /// <summary>
    /// When the user profile is updated, this message is published.
    /// </summary>
    public const string PROFILE_UPDATED = nameof(PROFILE_UPDATED);

    /// <summary>
    /// This would let the client know about the progress of a job (Typically a long-running hangfire background job).
    /// </summary>
    public const string BACKGROUND_JOB_PROGRESS = nameof(BACKGROUND_JOB_PROGRESS);



    /// <summary>
    /// When a user completes social sign-in in a separate window, this message is published to notify the app.
    /// </summary>
    public const string SOCIAL_SIGN_IN_CALLBACK = nameof(SOCIAL_SIGN_IN_CALLBACK);

    /// <summary>
    /// A publisher that publishes this message wants to force the app to check for updates and install them.
    /// </summary>
    public const string FORCE_UPDATE = nameof(FORCE_UPDATE);
}
