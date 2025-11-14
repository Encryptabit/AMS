using Ams.Web.Dtos;
using Ams.Web.Requests;
using Ams.Web.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ams.Web.Endpoints;

public static class WorkspaceEndpoints
{
    public static IEndpointRouteBuilder MapWorkspaceApi(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/workspace");

        group.MapGet("/", (WorkspaceState state) =>
        {
            var snapshot = state.Snapshot;
            return TypedResults.Ok(new WorkspaceStateDto(snapshot.WorkspaceRoot, snapshot.BookIndexPath, snapshot.CrxTemplatePath));
        });

        group.MapPost("/", async Task<Results<Ok<WorkspaceStateDto>, ProblemHttpResult>> (
            WorkspaceUpdateRequest request,
            WorkspaceState state,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await state.UpdateAsync(request, cancellationToken).ConfigureAwait(false);
                var snapshot = state.Snapshot;
                return TypedResults.Ok(new WorkspaceStateDto(snapshot.WorkspaceRoot, snapshot.BookIndexPath, snapshot.CrxTemplatePath));
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(ex.Message);
            }
        });

        return builder;
    }
}
