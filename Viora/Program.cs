using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Viora.Data;
using Viora.DataSeed;
using Viora.Hubs;
using Viora.Models;
using Viora.Repositories;
using Viora.Services;






var builder = WebApplication.CreateBuilder(args);



// Configure the application to listen on all network interfaces (0.0.0.0) on port 5000
// This allows access from other devices on the same network (e.g., mobile, other PCs)
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient();

builder.Services.AddMemoryCache();

//Database Configuration
builder.Services.AddDbContext<VioraDBContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


//for swagger
builder.Services.AddSwaggerGen();

//Allow CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
         });
});


//Add SignalR
builder.Services.AddSignalR();


//Identity Configuration
builder.Services.AddIdentity<ApplicationUser,IdentityRole<int>>()
    .AddEntityFrameworkStores<VioraDBContext>()
    .AddDefaultTokenProviders();

//JWT Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        // Fix it here instead of the global static
        //to don't remap anything — keep claim names exactly as they appear in the token.

        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });



// Dependecy Injection for Repositories and Services
builder.Services.AddScoped(typeof(GenericRepository<>));

builder.Services.AddScoped<ChatRepository>();
builder.Services.AddScoped<UserFileRepository>();

builder.Services.AddScoped<JwtAuthenticationService>();
builder.Services.AddScoped<SpeechToTextService>();
builder.Services.AddScoped<IntentClassificationService>();
builder.Services.AddScoped<ChatHandlingService>();
builder.Services.AddScoped<MessageHandlingService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TextToSpeechService>();
builder.Services.AddScoped<DocumentHandlingService>();





var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Enable CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    await ContextSeed.SeedRoles(roleManager);
}

app.Run();
