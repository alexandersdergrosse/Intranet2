using Intranet2.Datenbank.Data;
using Intranet2.Datenbank.Models;
using Intranet2.Services.ActiveDirectory;
using Intranet2.Services.Jobs;
using Intranet2.Sicherheit;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5005", "https://localhost:5006");

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

    options.AddPolicy(Berechtigungen.UmfragenVerwalten, policy =>
    {
        policy.RequireRole(
            Rollen.Admin,
            Rollen.Redaktion);
    });

    // Nur Administratoren dürfen Benutzer verwalten.
    options.AddPolicy(Berechtigungen.BenutzerVerwalten, policy =>
        {
            policy.RequireRole(Rollen.Admin);
        });
});


// MITARBEITERFOTO-SERVICE
builder.Services.AddSingleton<Intranet2.Services.Fotos.MitarbeiterFotoService>();

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

// MITARBEITERFOTOS VOM FILESERVER EINBINDEN
string fotosPfad = builder.Configuration["Mitarbeiterfotos:Pfad"]
                   ?? @"\\fileserver\Volume_V\mitarbeiter_fotos";

if (Directory.Exists(fotosPfad))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(fotosPfad),
        RequestPath = "/mitarbeiterfotos"
    });
}

app.UseRouting();

// WINDOWS-BENUTZER AUTHENTIFIZIEREN
app.UseAuthentication();

app.UseMiddleware<BenutzerMiddleware>();

// BERECHTIGUNGEN PRÜFEN

app.UseAuthorization();

app.MapRazorPages();

app.Run();