using System.Text;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
Console.WriteLine(builder.Configuration["Jwt:Key"]);
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        //ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("user_email", policy =>
        policy
            .RequireClaim("Email"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var cart = app.MapGroup("/cart");

cart.MapGet("/", (context) =>
{
    var email = context.User.FindFirst("Email")?.Value;
    if (email is not null)
    {
        return context.Response.WriteAsync(email);
    }
    return context.Response.WriteAsync("Hello World!");
}).RequireAuthorization("user_email");

static async Task<IResult> getCart()
{
    return TypedResults.Ok("Hello World");
}

static async Task<IResult> createCart()
{
    return TypedResults.Ok("Hello World");
}

static async Task<IResult> addToCart()
{
    return TypedResults.Ok("Hello World");
}

static async Task<IResult> cancelCart()
{
    return TypedResults.Ok("Hello World");
}

static async Task<IResult> checkOutCart()
{
    return TypedResults.Ok("Hello World");
}

app.Run();