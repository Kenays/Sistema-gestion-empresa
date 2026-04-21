using Empresa.Api.Domain;
using System.Text;
using Empresa.Api.Application.Services;
using Empresa.Api.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Escribe: Bearer {tu token}",
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});

// DbContext
var dbProvider = builder.Configuration["DatabaseProvider"];

builder.Services.AddDbContext<EmpresaDbContext>(options =>
{
    if (dbProvider == "Postgres")
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"));
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("EmpresaDB"));
    }
}); // JWT
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder
    .Services.AddAuthentication(options =>
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

            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<VentaService>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EmpresaDbContext>();

    db.Database.EnsureCreated();

    if (!db.Roles.Any())
    {
        db.Roles.AddRange(
            new Rol { Nombre = "Administrador" },
            new Rol { Nombre = "Operador" }
        );
        db.SaveChanges();
    }

    if (!db.Usuarios.Any())
    {
        var admin = new Usuario
        {
            Nombre = "Admin",
            Email = "admin@empresa.com",
            PasswordHash = "$2a$11$PUI1T2oXyeBvYVp2nJZEruo9CchynqkWdO6/8XnrghEwQ.Tju9MY6",
            Activo = true
        };

        db.Usuarios.Add(admin);
        db.SaveChanges();

        var rolAdmin = db.Roles.First(r => r.Nombre == "Administrador");

        db.UsuarioRoles.Add(new UsuarioRol
        {
            IdUsuario = admin.IdUsuario,
            IdRol = rolAdmin.IdRol
        });

        db.SaveChanges();
    }

    if (!db.Clientes.Any())
    {
        db.Clientes.AddRange(
            new Cliente { Nombre = "Juan Pérez", Email = "juan@test.com", Telefono = "11111111", Activo = true },
            new Cliente { Nombre = "María López", Email = "maria@test.com", Telefono = "22222222", Activo = true },
            new Cliente { Nombre = "Carlos Gómez", Email = "carlos@test.com", Telefono = "33333333", Activo = true },
            new Cliente { Nombre = "Ana Martínez", Email = "ana@test.com", Telefono = "44444444", Activo = true },
            new Cliente { Nombre = "Luis Rodríguez", Email = "luis@test.com", Telefono = "55555555", Activo = true }
        );
        db.SaveChanges();
    }

    if (!db.Productos.Any())
    {
        db.Productos.AddRange(
            new Producto { Nombre = "Laptop", Precio = 5000, Costo = 4000, Stock = 10, Codigo = "LAP001", Impuesto = 0.12m, Activo = true },
            new Producto { Nombre = "Mouse", Precio = 150, Costo = 80, Stock = 50, Codigo = "MOU001", Impuesto = 0.12m, Activo = true },
            new Producto { Nombre = "Teclado", Precio = 300, Costo = 200, Stock = 30, Codigo = "TEC001", Impuesto = 0.12m, Activo = true },
            new Producto { Nombre = "Monitor", Precio = 1200, Costo = 900, Stock = 15, Codigo = "MON001", Impuesto = 0.12m, Activo = true },
            new Producto { Nombre = "USB", Precio = 80, Costo = 40, Stock = 100, Codigo = "USB001", Impuesto = 0.12m, Activo = true }
        );
        db.SaveChanges();
    }

    if (!db.Ventas.Any())
    {
        var clientes = db.Clientes.ToList();
        var productos = db.Productos.ToList();

        for (int i = 0; i < 5; i++)
        {
            var cliente = clientes[i];
            var producto = productos[i];

            var subtotal = producto.Precio;
            var impuesto = subtotal * (producto.Impuesto ?? 0);
            var total = subtotal + impuesto;

            var venta = new Venta
            {
                IdCliente = cliente.IdCliente,
                Subtotal = subtotal,
                Impuesto = impuesto,
                Total = total,
                Activo = true
            };

            db.Ventas.Add(venta);
            db.SaveChanges();

            db.DetalleVentas.Add(new DetalleVenta
            {
                IdVenta = venta.IdVenta,
                IdProducto = producto.IdProducto,
                Cantidad = 1,
                PrecioUnitario = producto.Precio,
                Impuesto = impuesto,
                Subtotal = subtotal
            });

            db.SaveChanges();
        }
    }
}

// Middleware
    app.UseSwagger();
    app.UseSwaggerUI();

app.UseAuthentication(); // ⚠️ SIEMPRE antes
app.UseAuthorization();

app.MapControllers();

app.Run();
