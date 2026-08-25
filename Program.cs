using System.Security.Claims;
using Intranet2.Datenbank.Data;
using Intranet2.Datenbank.Models;
using Intranet2.Sicherheit;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Intranet2.Services.Jobs;
using Intranet2.Services.ActiveDirectory;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages
builder.Services.AddRazorPages();

// Cache für automatisch geladene Website-Inhalte
builder.Services.AddMemoryCache();

// Active Directory Mitarbeiter
builder.Services.AddScoped<MitarbeiterService>();

// Kreutzträger-Webseite automatisch auslesen
builder.Services.AddHttpClient<StellenService>(client =>
{
    client.BaseAddress = new Uri("https://kreutztraeger-kaeltetechnik.de/");

    client.Timeout = TimeSpan.FromSeconds(15);

    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 Kreutztraeger-Intranet/1.0");
});

// DATENBANK
builder.Services.AddDbContext<DataContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Die Verbindungszeichenfolge 'DefaultConnection' wurde nicht gefunden.");

    options.UseSqlServer(connectionString);
});

// WINDOWS-AUTHENTIFIZIERUNG
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

// AUTORISIERUNG/BERECHTIGUNGEN
builder.Services.AddAuthorization(options =>
{
    // Standardmäßig muss jeder Benutzer angemeldet sein.
    options.FallbackPolicy = options.DefaultPolicy;

    // Nur Administratoren dürfen den Admin-Bereich öffnen.
    options.AddPolicy(Berechtigungen.AdminBereich, policy =>
        {
            policy.RequireRole(Rollen.Admin);
        });

    // Nur Administratoren dürfen News verwalten.
    options.AddPolicy(Berechtigungen.NewsVerwalten, policy =>
        {
            policy.RequireRole(Rollen.Admin, Rollen.Redaktion);
        });

    options.AddPolicy(
    Berechtigungen.UmfragenVerwalten,
    policy =>
    {
        policy.RequireRole(
            Rollen.Admin,
            Rollen.Redaktion);
    });

    // Nur Administratoren dürfen Benutzer verwalten.
    options.AddPolicy(Berechtigungen.BenutzerVerwalten,
        policy =>
        {
            policy.RequireRole(Rollen.Admin);
        });
});


var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseStaticFiles();

// UPLOADS-ORDNER ALS STATISCHE DATEIEN EINBINDEN
string uploadPfad = builder.Configuration["Uploads:Pfad"] ?? Path.Combine(builder.Environment.WebRootPath, "Images", "Marktplatz");
Directory.CreateDirectory(uploadPfad);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadPfad),
    RequestPath = "/uploads"
});

app.UseRouting();


// WINDOWS-BENUTZER AUTHENTIFIZIEREN
app.UseAuthentication();

// WINDOWS-BENUTZER MIT DATENBANK VERBINDEN
// UND ROLLE ALS CLAIM HINZUFÜGEN
app.Use(async (context, next) =>
{
    // ? FIX 1: Statische Dateien komplett überspringen
    string path = context.Request.Path.Value ?? "";
    bool istStatischeDatei = path.StartsWith("/css") || path.StartsWith("/js") || path.StartsWith("/lib") || path.StartsWith("/Images") || path.StartsWith("/uploads") || path.StartsWith("/favicon");

    if (istStatischeDatei)
    {
        await next();
        return;
    }

    if (context.User.Identity?.IsAuthenticated == true)
    {
        string? windowsBenutzername = context.User.Identity.Name;

        if (!string.IsNullOrWhiteSpace(windowsBenutzername))
        {
            var db = context.RequestServices.GetRequiredService<DataContext>();

            Benutzer? benutzer = await db.Benutzer.FirstOrDefaultAsync(b => b.WindowsBenutzername == windowsBenutzername);

            if (benutzer == null)
            {
                string name = windowsBenutzername;
                if (windowsBenutzername.Contains("\\"))
                    name = windowsBenutzername.Split('\\').Last();

                benutzer = new Benutzer
                {
                    WindowsBenutzername = windowsBenutzername,
                    Name = name,
                    Email = null,
                    Rolle = Rollen.Benutzer,
                    IstAktiv = true,
                    RegisteredAt = DateTime.UtcNow,
                    LetzterMarktplatzBesuch = DateTime.Now,
                };

                db.Benutzer.Add(benutzer);

                // ? FIX 2: Race Condition absichern
                try
                {
                    await db.SaveChangesAsync();

                    // Nur protokollieren wenn INSERT wirklich geklappt hat
                    db.BenutzerProtokolle.Add(new BenutzerProtokoll
                    {
                        BenutzerId = benutzer.Id,
                        BenutzerName = benutzer.Name,
                        WindowsBenutzername = benutzer.WindowsBenutzername,
                        Aktion = "Neu angelegt",
                        Feld = null,
                        AlterWert = null,
                        NeuerWert = $"Rolle: {benutzer.Rolle}; Status: Aktiv",
                        AusgefuehrtVon = "System (erste Anmeldung)",
                        Zeitpunkt = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync();
                }
                catch (Exception)
                {
                    // ? Anderer Request war schneller – Benutzer nochmal laden
                    db.ChangeTracker.Clear();
                    benutzer = await db.Benutzer.FirstOrDefaultAsync(b => b.WindowsBenutzername == windowsBenutzername);
                }
            }

            // Benutzer konnte nicht geladen werden ? überspringen
            if (benutzer == null)
            {
                await next();
                return;
            }

            // NEUE MARKTPLATZ-BEITRÄGE
            DateTime letzterMarktplatzBesuch = benutzer.LetzterMarktplatzBesuch ?? benutzer.RegisteredAt;
            int neueMarktplatzBeitraege = await db.MarktplatzBeitraege
                .CountAsync(m => m.ErstelltAm > letzterMarktplatzBesuch && m.BenutzerId != benutzer.Id);
            context.Items["NeueMarktplatzBeitraege"] = neueMarktplatzBeitraege;

            // DEAKTIVIERTE BENUTZER SPERREN
            if (!benutzer.IstAktiv)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;

                await context.Response.WriteAsync("Ihr Benutzerkonto ist für das Intranet deaktiviert.");

                return;
            }

            // ROLLEN HINZUFÜGEN
            var rollenClaims = new List<Claim>();

            rollenClaims.Add(new Claim(ClaimTypes.Role, Rollen.Benutzer));

            if (benutzer.Rolle == Rollen.Admin) rollenClaims.Add(new Claim(ClaimTypes.Role, Rollen.Admin));

            if (benutzer.Rolle == Rollen.Redaktion) rollenClaims.Add(new Claim(ClaimTypes.Role, Rollen.Redaktion));

            var rollenIdentity = new ClaimsIdentity(rollenClaims, "IntranetRollen");

            context.User.AddIdentity(rollenIdentity);
        }
    }

    await next();
});


// BERECHTIGUNGEN PRÜFEN

app.UseAuthorization();

app.MapRazorPages();

app.Run();