using System.Text;
using Carrinho;
using Carrinho.DTO;
using Carrinho.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore;
using Swashbuckle.AspNetCore;
using Swashbuckle.AspNetCore.Swagger;

using Microsoft.OpenApi.Models;

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


builder.Services.AddScoped<CartService>();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("user_email", policy =>
        policy
            .RequireClaim("Email"));

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, ctx, ct) =>
    {
        doc.Components ??= new Microsoft.OpenApi.Models.OpenApiComponents();

        doc.Components.SecuritySchemes["Bearer"] = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        };

        doc.SecurityRequirements.Add(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        return Task.CompletedTask;
    });
});

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    //app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();

var cartMapping = app.MapGroup("/cart");

cartMapping.MapGet("/", getCart).RequireAuthorization("user_email");
cartMapping.MapPost("/", createCart).RequireAuthorization("user_email");
//You can see that it is not restful here
cartMapping.MapPatch("/cart-items", addToCart).RequireAuthorization("user_email");
cartMapping.MapPatch("/cancel", cancelCart).RequireAuthorization("user_email");

static async Task<IResult> getCart(HttpContext context, CartService cartService)
{
    var email = context.User.FindFirst("Email")?.Value;
    if (string.IsNullOrEmpty(email))
        return TypedResults.Unauthorized();
    try
    {
        var user = await cartService.getUser(email);
        var cart = await cartService.getCartIfExists(user);
        if (cart == null)
            return TypedResults.NoContent();
        return TypedResults.Ok(cart);
    }
    catch (Exception ex)
    {
        if (ex is KeyNotFoundException)
            return TypedResults.NotFound("User not found");
    }
    return TypedResults.InternalServerError();
}

//Two exceptions might happen inside which might be handled by a middleware
static async Task<IResult> createCart(HttpContext context, CartDto cartDto, CartService cartService)
{
    var email = context.User.FindFirst("Email")?.Value;
    if (string.IsNullOrEmpty(email))
        return TypedResults.Unauthorized();
    try
    {
        
        var newCart = await cartService.createCartIfNotExist(cartDto, email);
        return TypedResults.Ok(newCart);
    }
    catch (Exception ex)
    {
        if (ex is KeyNotFoundException)
            return TypedResults.NotFound("User not found");
        if (ex is InvalidOperationException)
            return TypedResults.BadRequest("User already has an valid cart");
        return TypedResults.InternalServerError(ex.Message);
    }
    return TypedResults.InternalServerError();
}
//TODO create exceptions for user not found or cart not found
static async Task<IResult> addToCart(HttpContext context, CartItemDto[] cartItems, CartService cartService)
{
    var email = context.User.FindFirst("Email")?.Value;
    if (string.IsNullOrEmpty(email))
        return TypedResults.Unauthorized();
    try
    {
        var newCart = await cartService.addItemsToCart(cartItems, email);
        return TypedResults.Ok(newCart);
    }
    catch (Exception ex)
    {
        if (ex is KeyNotFoundException)
            return TypedResults.NotFound("Cart or user not found");
        return TypedResults.InternalServerError(ex.Message);
    }
    return TypedResults.InternalServerError();
}

static async Task<IResult> cancelCart(HttpContext context, CartService cartService)
{
    var email = context.User.FindFirst("Email")?.Value;
    if (string.IsNullOrEmpty(email))
        return TypedResults.Unauthorized();
    try
    {
        var oldCart = await cartService.cancelCartIfExist(email);
        return TypedResults.Ok(oldCart);
    }
    catch (Exception ex)
    {
        return TypedResults.InternalServerError(ex.Message);
        if (ex is KeyNotFoundException)
            return TypedResults.NotFound("Cart or user not found");
    }
    return TypedResults.InternalServerError();
}

static async Task<IResult> checkOutCart()
{
    return TypedResults.Ok("Hello World");
}

app.Run();