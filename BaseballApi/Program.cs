using Microsoft.EntityFrameworkCore;
using BaseballApi.Models;
using BaseballApi;
using Microsoft.AspNetCore.Identity;
using BaseballApi.Import;

var corsLocal = "_corsLocalPolicy";

var builder = WebApplication.CreateBuilder(args);

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
