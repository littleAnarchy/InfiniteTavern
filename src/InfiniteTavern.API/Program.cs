using InfiniteTavern.Application.Services;
using InfiniteTavern.Infrastructure.Data;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use PORT environment variable (for Render, Railway, etc.)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(int.Parse(port));
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Infinite Tavern API", 
        Version = "v1",
        Description = "AI-powered text RPG backend"
    });
});

// MongoDB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "mongodb://localhost:27017";
var databaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "InfiniteTavern";

builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString));
builder.Services.AddScoped<IMongoDatabase>(sp => 
    sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
builder.Services.AddScoped<MongoDbContext>();
builder.Services.AddScoped<IGameRepository, GameRepository>();

Console.WriteLine($"✓ MongoDB configured: {databaseName}");

// AI Service Selection
var aiProvider = builder.Configuration["AI:Provider"] ?? "OpenAI";

if (aiProvider.Equals("Claude", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpClient<IAIService, ClaudeService>();
    Console.WriteLine("✓ Using Claude AI Service (Sonnet 3.5)");
}
else if (aiProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpClient<IAIService, OpenAIService>();
    Console.WriteLine("✓ Using OpenAI Service (GPT-4o mini)");
}
else
{
    throw new InvalidOperationException($"Unknown AI provider: {aiProvider}. Valid options: Claude, OpenAI");
}

// Application Services
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IPromptBuilderService, PromptBuilderService>();
builder.Services.AddSingleton<IDiceService, DiceService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Infinite Tavern API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

// Disable HTTPS redirect in production (Render handles SSL)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");

app.UseAuthorization();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

app.MapControllers();

// Log startup
app.Logger.LogInformation("Infinite Tavern API starting...");
app.Logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();
