using FluentValidation;
using HoaTheoMua.Extentions;
using HoaTheoMua.Filters;
using HoaTheoMua.Middleware;
using HoaTheoMua.Repository;
using Microsoft.EntityFrameworkCore;
using AssemblyReference = HoaTheoMua.Service.AssemblyReference;
using CategoryNewsService = HoaTheoMua.Service.CategoryNew;
using CompanyAddressService = HoaTheoMua.Service.CompanyAddress;
using NewService = HoaTheoMua.Service.New;
using JobDescriptionService = HoaTheoMua.Service.JobDescriptions;
using RecruitmentRequestService = HoaTheoMua.Service.RecruitmentRequests;
using OrderService = HoaTheoMua.Service.Order;
using WebhookService = HoaTheoMua.Service.Webhook;
using ProductService = HoaTheoMua.Service.Product;
using ConfigService = HoaTheoMua.Service.Config;
using AddressService = HoaTheoMua.Service.Address;
using IService = HoaTheoMua.Service.AhamoveService.IService;
using NhanhVnService = HoaTheoMua.Service.NhanhVnService;
using Service = HoaTheoMua.Service.AhamoveService.Service;
using BaoKimService = HoaTheoMua.Service.BaoKim;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationActionFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("PostgresSql");
var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource)
);

builder.Services.ConfigureRateLimiter();
builder.Services.AddJwtServices(builder.Configuration);
builder.Services.AddSwaggerServices();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<HoaTheoMua.Service.Common.ICurrentUserService, HoaTheoMua.Service.Common.CurrentUserService>();
builder.Services.AddScoped<CategoryNewsService.IService, CategoryNewsService.Service>();
builder.Services.AddScoped<CompanyAddressService.IService, CompanyAddressService.Service>();
builder.Services.AddScoped<NewService.IService, NewService.Service>();
builder.Services.AddScoped<OrderService.IService, OrderService.Service>();
builder.Services.AddScoped<WebhookService.IService, WebhookService.Service>();
builder.Services.AddScoped<JobDescriptionService.IService, JobDescriptionService.Service>();
builder.Services.AddScoped<RecruitmentRequestService.IService, RecruitmentRequestService.Service>();
builder.Services.AddHttpClient<IService, Service>();
builder.Services.AddHttpClient<BaoKimService.IService, BaoKimService.Service>();
builder.Services.AddScoped<ProductService.IService, ProductService.Service>();
builder.Services.AddScoped<ConfigService.IService, ConfigService.Service>();
builder.Services.AddScoped<AddressService.IService, AddressService.Service>();
builder.Services.AddScoped<NhanhVnService.IService, NhanhVnService.Service>();



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