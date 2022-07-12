using System.Security.Claims;
using System.Threading.Tasks;
using Aiia.Sample.Data;
using Aiia.Sample.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Aiia.Sample.Extensions;

public static class UserExtensions
{
    public static async Task<ApplicationUser> GetCurrentUser(this ClaimsPrincipal principal, ApplicationDbContext dbContext)
    {
        var currentUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null) throw new UserNotFoundException();
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);
        if (user == null) throw new UserNotFoundException();
        return user;
    }
}