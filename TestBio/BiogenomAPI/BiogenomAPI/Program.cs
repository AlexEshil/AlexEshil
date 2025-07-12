using BiogenomAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BiogenomAPI", Version = "v1" });
});

var app = builder.Build();

// Swagger работает всегда (если хочешь ограничить только дев, оставь как было)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BiogenomAPI v1");
});

app.UseHttpsRedirection();  // по желанию

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
