using CampusPulse.Hubs;
using CampusPulse.Services;

Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "1");

var builder = WebApplication.CreateBuilder(args);

// ── Services ──
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR for real-time location updates
builder.Services.AddSignalR();

// HttpClient for Nominatim Geocoding API
builder.Services.AddHttpClient("Nominatim", client =>
{
    client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
    client.DefaultRequestHeaders.Add("User-Agent", "CampusPulse/1.0");
    client.DefaultRequestHeaders.Add("Accept-Language", "en");
});

// App services
builder.Services.AddSingleton<StudentService>();
builder.Services.AddScoped<GeocodingService>();

// CORS — allow frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:5500",
            "https://render-deploy-krrm.onrender.com"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// ── Middleware ──
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// SignalR Hub route
app.MapHub<LocationHub>("/locationHub");

// Serve index.html as default
app.MapFallbackToFile("index.html");

app.Run();
