using full.API.Data;
using full.API.Infrastructure;
using full.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
{
    document.Info.Title = "Employees API";
    document.Info.Description = "CRUD API for managing employees, backing the Full_UI Angular app.";
    return Task.CompletedTask;
}));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<FullDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FullConnectionString")));
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

builder.Services.AddHealthChecks().AddDbContextCheck<FullDbContext>("database");

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigins)
          .AllowAnyHeader()
          .AllowAnyMethod()
          .WithExposedHeaders("X-Total-Count", "Location")));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<FullDbContext>().Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Employees API v1"));
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Exposed so integration tests can host the app with WebApplicationFactory<Program>.
public partial class Program;
