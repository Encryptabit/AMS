using Hangfire.Dashboard;
using Hangfire.Annotations;

namespace Ams.Web.Server.Api.RequestPipeline;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize([NotNull] DashboardContext context)
    {
        return context.GetHttpContext().User.HasClaim(AppClaimTypes.FEATURES, AppFeatures.System.ManageJobs);
    }
}
