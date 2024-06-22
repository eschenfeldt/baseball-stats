using Microsoft.EntityFrameworkCore;
using BaseballApi.Models;
using BaseballApi;
using Microsoft.AspNetCore.Identity;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGroup("/api/Admin").MapIdentityApi<IdentityUser>()
    .RequireAuthorization();

app.Run();
