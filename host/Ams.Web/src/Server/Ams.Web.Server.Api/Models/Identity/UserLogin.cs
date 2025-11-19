namespace Ams.Web.Server.Api.Models.Identity;

public class UserLogin : IdentityUserLogin<Guid>
{
    public User? User { get; set; }
}
