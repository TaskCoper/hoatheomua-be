using HoaTheoMua.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
   options.AddPolicy("CorsPolicy1", policyBuilder =>
   {
       policyBuilder.AllowAnyOrigin();
       policyBuilder.AllowAnyMethod();
       policyBuilder.AllowAnyHeader();
       policyBuilder.AllowCredentials();
   });
   
   options.AddPolicy("CorsPolicy2", policyBuilder =>
   {
       policyBuilder.WithOrigins("http://localhost:4200");
       policyBuilder.WithMethods("GET", "POST");
       policyBuilder.WithHeaders("Authorization");
       policyBuilder.AllowCredentials();
   });
});



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.UseCors("CorsPolicy1");

app.Run();