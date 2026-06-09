using HoaTheoMua.Extentions;
using HoaTheoMua.Middleware;
using HoaTheoMua.Repository;
using Microsoft.EntityFrameworkCore;
using MailService = HoaTheoMua.Service.MailService;
using JwtService = HoaTheoMua.Service.JwtService;
using UserService = HoaTheoMua.Service.User;
using AuthService = HoaTheoMua.Service.Auth;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
    
var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "HoaTheoMua";
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.ConfigureRateLimiter();
var app = builder.Build();
builder.Services.AddJwtServices(builder.Configuration);
builder.Services.AddSwaggerServices();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<MailService.IService, MailService.Service>();
builder.Services.AddScoped<JwtService.IService, JwtService.Service>();
builder.Services.AddScoped<JwtService.IService, JwtService.Service>();
builder.Services.AddScoped<UserService.IService, UserService.Service>();
builder.Services.AddScoped<AuthService.IService, AuthService.Service>();


var app = builder.Build();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    app.UseSwaggerAPI();
}

app.UseRateLimiter();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();