using Microsoft.AspNetCore.Identity;

namespace Viora.DataSeed
{
    public class ContextSeed
    {

        public static async Task SeedRoles(RoleManager<IdentityRole<int>> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole<int>("User"));
            }
            if (!await roleManager.RoleExistsAsync("Volunteer"))
            {
                await roleManager.CreateAsync(new IdentityRole<int>("Volunteer"));
            }
        }
    }
}
