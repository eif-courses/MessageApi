using MessageApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
       builder =>
       {
           builder.WithOrigins("http://localhost:4040")
                  .WithHeaders("Authorization");
       });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
     .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
     {
         c.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
         c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
         {
             ValidAudience = builder.Configuration["Auth0:Audience"],
             ValidIssuer = $"{builder.Configuration["Auth0:Domain"]}"
         };
     });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("Admin", p => p.    
    RequireAuthenticatedUser().
        RequireClaim("scope", "read:admin-messages"));
});


var app = builder.Build();
app.MapControllers();
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();


string publicMessage = "viešai matoma";
string protectedMessage = "autorizuotas vartotojas";
string adminMessage = "Administratoriaus zona";

app.MapGet("/api/messages/public", () => new ApiResponse(publicMessage));
app.MapGet("/api/messages/protected", () => new ApiResponse(protectedMessage)).RequireAuthorization();
app.MapGet("/api/messages/admin", () => new ApiResponse(adminMessage)).RequireAuthorization("Admin"); 

app.Run();
