
using API.Extensions;
using API.Helpers;
using API.Middleware;
using infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers();
builder.Services.AddApplicationServices();
builder.Services.AddSwaggerDocumentation();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<StoreContext>(x=> x.UseSqlite(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(opt =>
 {
    opt.AddPolicy(name: "AllowOrigin",policy => 
    {
        policy
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin();
        //.WithOrigins("https://localhost:7240");
    });
 });
var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwaggerDocumentation();
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
app.UseStatusCodePagesWithReExecute("/errors/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowOrigin");
app.UseAuthorization();

app.MapControllers();
using var scope = app.Services.CreateScope();
var services =scope.ServiceProvider;
var loggerFactory = services.GetRequiredService<ILoggerFactory>();
try{
    var context = services.GetRequiredService<StoreContext>();
    await context.Database.MigrateAsync();
    await StoreContextSeed.SeedAsync(context, loggerFactory);    
}
catch(Exception ex)
{
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogError(ex,"An error occured during migration");
}
app.Run();
