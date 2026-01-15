using carton_caps_referral.Middlewares;
using carton_caps_referral.Seed;
using carton_caps_referral.Repositories;
using carton_caps_referral.Repositories.InMemory;
using carton_caps_referral.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Middleware
builder.Services.AddSingleton<ErrorHandlerMiddleware>();
builder.Services.AddSingleton<TraceIdHeaderMiddleware>();

//Repositories (InMemory for testing purpose)
builder.Services.AddSingleton<IReferralLinkRepository, InMemoryReferralLinkRepository>();
builder.Services.AddSingleton<IReferralRepository, InMemoryReferralRepository>();
builder.Services.AddSingleton<IRateLimiterRepository>(sp => new InMemoryRateLimiter(5, TimeSpan.FromMinutes(1)));
builder.Services.AddScoped<IDeferredLinkVendorRepository, DeferredLinkVendorRepository>();

//Services
builder.Services.AddScoped<IReferralLinkService, ReferralLinkService>();
builder.Services.AddScoped<IDeepLinkService, DeepLinkService>();
builder.Services.AddScoped<IReferralService, ReferralService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await SeedData.SeedAsync(app.Services);
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseMiddleware<TraceIdHeaderMiddleware>();

app.MapControllers();

app.Run();
