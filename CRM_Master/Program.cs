var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.MaxDepth = 512;
        options.JsonSerializerOptions.DefaultBufferSize = 4096 * 10;
    });

// Configurer HttpClient avec un timeout plus long
builder.Services.AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName, client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});

// Ajouter et configurer la session
builder.Services.AddDistributedMemoryCache(); // Utilise la mémoire pour stocker la session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Durée d'expiration de la session
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Rend le cookie sécurisé
    options.Cookie.SameSite = SameSiteMode.None; // Nécessite SecurePolicy = Always
    options.Cookie.HttpOnly = true; // Sécuriser le cookie
    options.Cookie.IsEssential = true; // Le cookie est essentiel pour le bon fonctionnement de l'application
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpringBoot",
        policy =>
        {
            policy.WithOrigins("http://localhost:8080") // Adresse de ton Spring Boot
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseCors("AllowSpringBoot");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=LoginForm}/{id?}")
    .WithStaticAssets();


app.Run();
