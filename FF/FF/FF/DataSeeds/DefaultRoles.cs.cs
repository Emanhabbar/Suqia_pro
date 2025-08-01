using Microsoft.AspNetCore.Identity;

namespace FF.DataSeeds
{
    public class DefaultRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            // إذا لم يكن دور "Admin" موجوداً، قم بإنشائه
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // إذا لم يكن دور "TankOwner" موجوداً، قم بإنشائه
            if (!await roleManager.RoleExistsAsync("TankOwner"))
            {
                await roleManager.CreateAsync(new IdentityRole("TankOwner"));
            }

            // إذا لم يكن دور "Customer" موجوداً، قم بإنشائه
            if (!await roleManager.RoleExistsAsync("Customer"))
            {
                await roleManager.CreateAsync(new IdentityRole("Customer"));
            }
        }
    }
}