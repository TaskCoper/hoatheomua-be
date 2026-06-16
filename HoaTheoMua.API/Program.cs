using FluentValidation;
using HoaTheoMua.Extension;
using HoaTheoMua.Extentions;
using HoaTheoMua.Middleware;
using HoaTheoMua.Repository;
using Microsoft.EntityFrameworkCore;
using AssemblyReference = HoaTheoMua.Service.AssemblyReference;
using MailService = HoaTheoMua.Service.MailService;
using JwtService = HoaTheoMua.Service.JwtService;
using UserService = HoaTheoMua.Service.User;
using AuthService = HoaTheoMua.Service.Auth;
using CategoryNewsService = HoaTheoMua.Service.CategoryNew;
using CompanyAddressService = HoaTheoMua.Service.CompanyAddress;
using NewService = HoaTheoMua.Service.New;
using JobDescriptionService = HoaTheoMua.Service.JobDescriptions;
using RecruitmentRequestService = HoaTheoMua.Service.RecruitmentRequests;
using OrderService = HoaTheoMua.Service.Order;
using WebhookService = HoaTheoMua.Service.Webhook;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PostgresSql")
    )
);

builder.Services.ConfigureRateLimiter();
builder.Services.AddJwtServices(builder.Configuration);
builder.Services.AddSwaggerServices();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<MailService.IService, MailService.Service>();
builder.Services.AddScoped<JwtService.IService, JwtService.Service>();
builder.Services.AddScoped<UserService.IService, UserService.Service>();
builder.Services.AddScoped<AuthService.IService, AuthService.Service>();
builder.Services.AddScoped<CategoryNewsService.IService, CategoryNewsService.Service>();
builder.Services.AddScoped<CompanyAddressService.IService, CompanyAddressService.Service>();
builder.Services.AddScoped<NewService.IService, NewService.Service>();
builder.Services.AddScoped<OrderService.IService, OrderService.Service>();
builder.Services.AddScoped<WebhookService.IService, WebhookService.Service>();
builder.Services.AddScoped<JobDescriptionService.IService, JobDescriptionService.Service>();
builder.Services.AddScoped<RecruitmentRequestService.IService, RecruitmentRequestService.Service>();

builder.Services.AddValidatorsFromAssembly(AssemblyReference.Assembly);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}


app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    app.UseSwaggerAPI();
}
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();