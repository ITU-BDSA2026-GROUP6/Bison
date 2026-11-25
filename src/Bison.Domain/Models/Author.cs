using Microsoft.AspNetCore.Identity;

namespace Bison.Domain.Models;

public class Author : IdentityUser
{
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
