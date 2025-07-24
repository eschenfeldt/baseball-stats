using Microsoft.EntityFrameworkCore;
using BaseballApi.Models;
using BaseballApi;
using Microsoft.AspNetCore.Identity;
using BaseballApi.Import;
using BaseballApi.Services;
using Microsoft.AspNetCore.Http.Features;

var corsLocal = "_corsLocalPolicy";

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = null; // unlimited
});

builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = long.MaxValue;
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();

// register db contexts
var ownerConnectionString = builder.Configuration["Baseball:OwnerConnectionString"];
builder.Services.AddDbContext<BaseballContext>(opt => opt.UseNpgsql(ownerConnectionString));

var identityConnectionString = builder.Configuration["Identity:OwnerConnectionString"];
builder.Services.AddDbContext<AppIdentityDbContext>(opt => opt.UseNpgsql(identityConnectionString));

builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AppIdentityDbContext>();

builder.Services.AddScoped<IRemoteFileManager, RemoteFileManager>();
builder.Services.AddSingleton<IMediaImportQueue, MediaImportQueue>();
builder.Services.AddHostedService<MediaImportBackgroundService>();
if (!builder.Environment.IsDevelopment())
{
    // Local dev won't see the prod files but will see the prod import task so don't try to process/clean up
    builder.Services.AddHostedService<MediaImportTaskRestarter>();
    builder.Services.AddHostedService<TempFileCleaner>();
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsLocal,
                      policy =>
                      {
                          policy.SetIsOriginAllowed(origin => new Uri(origin).IsLoopback)
                            .AllowAnyHeader()
                            .AllowCredentials();
                      });
});
builder.Host.UseSystemd();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Allow user setup endpoints only in development
    app.MapGroup("/api/Admin/Dev").MapIdentityApi<IdentityUser>();
    app.UseSwagger();
    app.UseSwaggerUI(options => options.EnableTryItOutByDefault());
}

app.UseHttpsRedirection();

app.UseCors(corsLocal);
app.UseAuthorization();

app.MapControllers();

app.Run();
