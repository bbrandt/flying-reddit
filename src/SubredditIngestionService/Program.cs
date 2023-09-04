var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapHealthChecks("/ingestion/healthz");

app.Run();
