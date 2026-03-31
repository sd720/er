using ItemProcessorApp.Data;
using ItemProcessorApp.Filters;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add EF Core with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add MVC + Session
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AuthFilter>();
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Since SQL parsing on the remote host hit Foreign Key drop errors, we rely entirely on EF Core to build the DB
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        // 1. Seed User
        db.Database.ExecuteSqlRaw("INSERT INTO dbo.Users (FullName, Email, Password, CreatedAt, IsActive) VALUES ('Admin User', 'admin@test.com', '$2a$11$y4zc79EJySBsxY.vgOwFuumCmg4CMKIq/hoOcujmj/Uv.oX8MifWy', GETUTCDATE(), 1);");

        // 2. Seed Items
        db.Database.ExecuteSqlRaw(@"
            INSERT INTO dbo.Items (Name, Weight, Description, CreatedBy, CreatedAt, UpdatedAt) VALUES 
            ('Raw Steel Block', 500.0000, 'Unprocessed steel block', 1, GETUTCDATE(), GETUTCDATE()),
            ('Aluminium Sheet', 120.5000, 'Thin aluminium sheet', 1, GETUTCDATE(), GETUTCDATE()),
            ('Copper Rod', 75.2500, 'Copper rod 2m length', 1, GETUTCDATE(), GETUTCDATE()),
            ('Plastic Granules', 250.0000, 'Raw plastic material', 1, GETUTCDATE(), GETUTCDATE()),
            ('Glass Panel', 80.0000, 'Tempered glass panel', 1, GETUTCDATE(), GETUTCDATE());
        ");

        // 3. Seed ProcessedItems (using direct IDs)
        db.Database.ExecuteSqlRaw(@"
            INSERT INTO dbo.ProcessedItems (ItemId, ParentId, OutputWeight, Notes, ProcessedBy, ProcessedAt) VALUES 
            (1, NULL, 480.0000, 'Primary processing of steel block', 1, GETUTCDATE());
            
            DECLARE @rootId INT = SCOPE_IDENTITY();
            
            INSERT INTO dbo.ProcessedItems (ItemId, ParentId, OutputWeight, Notes, ProcessedBy, ProcessedAt) VALUES 
            (2, @rootId, 110.0000, 'Aluminium sheet cut from process', 1, GETUTCDATE()),
            (3, @rootId, 60.0000, 'Copper rod extracted from alloy', 1, GETUTCDATE());
        ");
    }
}

app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
