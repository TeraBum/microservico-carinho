using System.Text;
using Carrinho;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<CartDb>(opt => opt.UseNpgsql("Host=db.smjdaavxsnbmrdrvejsu.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=P@uc2025"));
builder.Services.AddDbContext<UserDb>(opt => opt.UseNpgsql("Host=db.smjdaavxsnbmrdrvejsu.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=P@uc2025"));
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

var cartMapping = app.MapGroup("/cart");

cartMapping.MapGet("/", async (HttpContext context, UserDb userDb, CartDb cartDb) =>
{
    var email = context.User.FindFirst("Email")?.Value;
    if (string.IsNullOrEmpty(email))
        return Results.Unauthorized();
    var user = await userDb.Users.Where(u => u.Email == email).FirstAsync();
    if (user is null)
        return Results.NotFound();
    var cart = await cartDb.Cart.Where(c => c.Status == "Active").FirstOrDefaultAsync();
    if (cart is null)
        return Results.NoContent();
    return Results.Ok(cart);
}).RequireAuthorization("user_email");

cartMapping.MapGet("/me", getCart);

static async Task<IResult> getCart(CartDb db)
{
    var carts = await db.Cart.ToListAsync();
    return TypedResults.Ok(carts);
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