using Microsoft.AspNetCore.Authorization;

namespace AcxiomCRM.Security;

public class ResourceOwnerRequirement : IAuthorizationRequirement { }

public class ResourceOwnerAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement, string>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerRequirement requirement,
        string resourceOwnerId)
    {
        if (context.User.IsInRole("Admin") || context.User.IsInRole("Manager"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == resourceOwnerId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
