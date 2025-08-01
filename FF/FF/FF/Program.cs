using FF.Data;
using FF.Models; // Contains ApplicationUser
using FF.DataSeeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FF.Services;
using Microsoft.Extensions.Logging; // Ensure this using directive is present

// --- ≈÷«›… Using ·‹ Firebase ---
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.IO;
// --- ‰Â«Ì… ≈÷«›… Using ·‹ Firebase ---

var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection Setup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Add Identity services using ApplicationUser
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // Support for roles
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); // Important for password reset or verification

// 3. Add MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // To support Identity pages like /Identity/Account/Login

// --- ≈÷«›…  ÂÌ∆… Firebase ÊŒœ„… «·≈‘⁄«—«  Â‰« ---
// „”«— „·› „› «Õ «·Œœ„….  √ﬂœÌ „‰ √‰ «”„ «·„·› ’ÕÌÕ.
var pathToFirebaseKey = Path.Combine(Directory.GetCurrentDirectory(), "firebase-adminsdk.json");

//  ÂÌ∆… Firebase „—… Ê«Õœ… ⁄‰œ »œ¡  ‘€Ì· «· ÿ»Ìﬁ
if (File.Exists(pathToFirebaseKey))
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile(pathToFirebaseKey)
    });
    // ≈÷«›… Œœ„… «·≈‘⁄«—«  ≈·Ï Õ«ÊÌ… «·Œœ„«  ﬂ‹ Singleton
    builder.Services.AddSingleton<NotificationService>();
}
else
{
    // Ì„ﬂ‰ﬂ Â‰«  ”ÃÌ· Œÿ√ ≈–« ·„ Ì „ «·⁄ÀÊ— ⁄·Ï «·„·›
    // ··Õ’Ê· ⁄·Ï logger Â‰«° ” Õ «ÃÌ‰ ≈·Ï «·Õ’Ê· ⁄·ÌÂ „‰ builder.Services
    // √Ê Ì„ﬂ‰ﬂ »»”«ÿ… ÿ»«⁄… —”«·… ≈·Ï Console
    Console.WriteLine("Œÿ√: „·› „› «Õ Œœ„… Firebase (firebase-adminsdk.json) €Ì— „ÊÃÊœ ›Ì „”«— «·„‘—Ê⁄!");
    Console.WriteLine($"«·„”«— «·„ Êﬁ⁄: {pathToFirebaseKey}");
    Console.WriteLine("·‰ Ì „  ›⁄Ì· Œœ„… «·≈‘⁄«—«  »œÊ‰ Â–« «·„·›.");
}
// --- ‰Â«Ì… ≈÷«›…  ÂÌ∆… Firebase ÊŒœ„… «·≈‘⁄«—«  ---


// =================================================================
// Build the application
var app = builder.Build();
// =================================================================

// 4. Seed Roles and Default Users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    // Define logger here to be available outside the try block
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // First: Ensure basic roles exist (from DefaultRoles.cs)
        await DefaultRoles.SeedAsync(roleManager);

        // Second: Seed a default Admin user
        const string adminEmail = "admin@example.com";
        const string adminPassword = "AdminPassword123!";
        const string adminRoleName = "Admin";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "System Administrator", EmailConfirmed = true };
            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
                if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
                {
                    await userManager.AddToRoleAsync(adminUser, adminRoleName);
                }
            }
            else
            {
                // Use the logger defined in the outer scope
                logger.LogError($"Failed to create default admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
            {
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
            }
        }

        // --- Seed a default Customer user ---
        const string customerEmail = "customer@example.com";
        const string customerPassword = "CustomerPassword123!"; // Strong password
        const string customerRoleName = "Customer";

        var customerUser = await userManager.FindByEmailAsync(customerEmail);
        if (customerUser == null)
        {
            customerUser = new ApplicationUser
            {
                UserName = customerEmail,
                Email = customerEmail,
                FullName = "Regular Customer",
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(customerUser, customerPassword);

            if (createResult.Succeeded)
            {
                if (!await userManager.IsInRoleAsync(customerUser, customerRoleName))
                {
                    await userManager.AddToRoleAsync(customerUser, customerRoleName);
                }
            }
            else
            {
                // Use the logger defined in the outer scope
                logger.LogError($"Failed to create default customer user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(customerUser, customerRoleName))
            {
                await userManager.AddToRoleAsync(customerUser, customerRoleName);
            }
        }
        // --- End of default Customer user seeding ---

        // --- New: Seed a default TankOwner user ---
        const string tankOwnerEmail = "tankowner@example.com";
        const string tankOwnerPassword = "TankOwnerPassword123!"; // Strong password
        const string tankOwnerRoleName = "TankOwner";

        var tankOwnerUser = await userManager.FindByEmailAsync(tankOwnerEmail);
        if (tankOwnerUser == null)
        {
            tankOwnerUser = new ApplicationUser
            {
                UserName = tankOwnerEmail,
                Email = tankOwnerEmail,
                FullName = "Tank Owner User",
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(tankOwnerUser, tankOwnerPassword);

            if (createResult.Succeeded)
            {
                if (!await userManager.IsInRoleAsync(tankOwnerUser, tankOwnerRoleName))
                {
                    await userManager.AddToRoleAsync(tankOwnerUser, tankOwnerRoleName);
                }
            }
            else
            {
                // Use the logger defined in the outer scope
                logger.LogError($"Failed to create default tank owner user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(tankOwnerUser, tankOwnerRoleName))
            {
                await userManager.AddToRoleAsync(tankOwnerUser, tankOwnerRoleName);
            }
        }
        // --- End of new ---

    }
    catch (Exception ex)
    {
        // Use the logger defined in the outer scope
        logger.LogError(ex, "An error occurred while seeding the database with roles or default users.");
    }
}

// 5. Middleware Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 6. Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// 7. Routing Setup
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // Required for Identity pages

app.Run();