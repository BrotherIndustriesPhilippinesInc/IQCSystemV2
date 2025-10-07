using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IQC_API.Data;
var builder = WebApplication.CreateBuilder(args);



// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod(); // important for DELETE/PUT
    });
});

builder.Services.AddDbContext<IQC_API_PG_Context>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("IQCPostgresDB") ?? throw new InvalidOperationException("Connection string 'IQCPostgresDB' not found.")));

builder.Services.AddDbContext<IQC_APIContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
