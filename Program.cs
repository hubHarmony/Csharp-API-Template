using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Simple_API;
using Simple_API.Database;

//Builder configuration
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddDbContext<Database>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DATABASE"), 
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DATABASE"))));
builder.Services.AddScoped<UserService>();

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = jwtSettings["Key"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];
if (string.IsNullOrEmpty(key))
{
    return;
}
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });


// Build
var app = builder.Build();



// App configuration
app.MapControllers();

/* TODO : Uncomment if you're using nginx for https redirections.
 Also ensure that your configuration looks like this : 
 
 server {
       listen 443 ssl;
       server_name your_domain.com;
       
       ssl_certificate /path/to/localhost.pem;
       ssl_certificate_key /path/to/localhost-key.pem;
   
       location / {
           proxy_pass http://localhost:your_backend_port;  # Change this to your backend server
           proxy_set_header Host $host;
           proxy_set_header X-Real-IP $remote_addr;
           proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
           proxy_set_header X-Forwarded-Proto $scheme;
       }
   }
   
   
app.UseForwardedHeaders(new ForwardedHeadersOptions
{ ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });
*/

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<Middlewares.ClaimsValidationMiddleware>();
app.UseCors("AllowAllOrigins");



app.Run();
