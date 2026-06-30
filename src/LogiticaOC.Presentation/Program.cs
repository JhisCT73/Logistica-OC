using LogiticaOC.Infrastructure.Data;
using LogiticaOC.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

// ── Licencia QuestPDF (Community = gratis para proyectos internos) ──
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ──────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── EF Core + SQL Server ─────────────────────────────────────────────
builder.Services.AddDbContext<OcLogisticaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repositorios y servicios de Infrastructure ───────────────────────
builder.Services.AddInfrastructureServices();

var app = builder.Build();

// ── Aplicar migraciones y seed automáticamente al iniciar ────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OcLogisticaDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();

