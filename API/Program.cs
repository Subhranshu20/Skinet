
using API.Extensions;
using API.Helpers;
using API.Middleware;
using core.Entities.Identity;
using infrastructure.Data;
using infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers();
builder.Services.AddApplicationServices();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionStringIdentity = builder.Configuration.GetConnectionString("IdentityConnection");
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<StoreContext>(x=> x.UseSqlite(connectionString));
builder.Services.AddDbContext<AppIdentityDbContext>(x=> x.UseSqlite(connectionStringIdentity));
builder.Services.AddSingleton<IConnectionMultiplexer>(c=> {
    var configuration=ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"),true);
    return ConnectionMultiplexer.Connect(configuration);
});
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
using var scope = app.Services.CreateScope();
var services =scope.ServiceProvider;
var loggerFactory = services.GetRequiredService<ILoggerFactory>();
var context = services.GetRequiredService<StoreContext>();
var identityContext = services.GetRequiredService<AppIdentityDbContext>();
var userManager = services.GetRequiredService<UserManager<AppUser>>();
try{
    
    await context.Database.MigrateAsync();
    await identityContext.Database.MigrateAsync();
    await StoreContextSeed.SeedAsync(context, loggerFactory);    
    await AppIdentityDbContextSeed.SeedUserAsync(userManager); 
}
catch(Exception ex)
{
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogError(ex,"An error occured during migration");
}
app.Run();
