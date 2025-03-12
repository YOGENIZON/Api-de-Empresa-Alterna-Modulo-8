using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var issuer = "TuIssuer";
var audience = "TuAudience";
var secretKey = "Clave-secreta-mi-demipropiedad-deYo-porque-no-es-tuya-y-no-la-vas-a-adivinar-weje-weje";

builder.Services.AddEndpointsApiExplorer();
// Configuración de Swagger
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "Como tu quieras que se llame",
        Version = "v1",
        Description = "Sistema para administrar los palomos de las empresas"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Aqui debes poner el token JWT de la siguiente manera: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
});
});


builder.Services.AddCors(options =>
{
   
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()    
              .AllowAnyMethod()    
              .AllowAnyHeader();   
    });


    options.AddPolicy("PermitirMiDominio", policy =>
    {
        policy.WithOrigins("http://localhost:5240")  
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});



builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("PermitirTodo");
app.UseCors("PermitirMiDominio");


app.Use(async (context, next) => {
   Console.WriteLine($"Middleware 1: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"Middleware 1: {context.Response.StatusCode}");
});

app.Use(async (context, next) => {

   context.Response.Headers.Append("X-Custom-Header", "Lo cambie y punto");
    await next();
    
});



    



Almacenamiento.Companias = new List<Compania>
{
    new Compania { 
        Id = 1, Nombre = "Tu papi Jhonson", FechaFundacion = new DateTime(2010, 1, 1) 
    },
    new Compania { 
        Id = 2, Nombre = "Mamones del caribe", FechaFundacion = new DateTime(2005, 5, 15) 
    },

    new Compania { 
        Id = 3, Nombre = "Pueblas RD", FechaFundacion = new DateTime(2015, 10, 20) 
    },
};

Almacenamiento.Empleados = new List<Empleado>
{
    new Empleado { 
        Id = 1, 
        Nombre = "Carlos", 
        Cargo = "Lava plato",
        Edad = 20, 
        CompaniaId = 1, 
        FechaContratacion = new DateTime(2020, 3, 10), 
        Salario = 30000m 
    },
    new Empleado { 
        Id = 2, 
        Nombre = "Juan", 
        Cargo = "Pela papas",
        Edad = 21, 
        CompaniaId = 1, 
        FechaContratacion = new DateTime(2019, 6, 20), 
        Salario = 32000m 
    },
    new Empleado { 
        Id = 3, 
        Nombre = "Pedro", 
        Cargo = "Gerente",
        Edad = 22, 
        CompaniaId = 2, 
        FechaContratacion = new DateTime(2021, 1, 15), 
        Salario = 35000m 
    },
    new Empleado { 
        Id = 4, 
        Nombre = "María", 
        Cargo = "Secretaria",
        Edad = 23, 
        CompaniaId = 2, 
        FechaContratacion = new DateTime(2018, 8, 5), 
        Salario = 40000m 
    },
    new Empleado { 
        Id = 5, 
        Nombre = "Luis", 
        Cargo = "Vigilante",
        Edad = 24, 
        CompaniaId = 2, 
        FechaContratacion = new DateTime(2022, 2, 28), 
        Salario = 38000m 
    },

    new Empleado { 
        Id = 6, 
        Nombre = "Ana", 
        Cargo = "Cajera",
        Edad = 25, 
        CompaniaId = 3, 
        FechaContratacion = new DateTime(2017, 4, 30), 
        Salario = 45000m 
    },

    new Empleado { 
        Id = 7, 
        Nombre = "Jose", 
        Cargo = "Guaremate",
        Edad = 26, 
        CompaniaId = 3, 
        FechaContratacion = new DateTime(2016, 7, 25), 
        Salario = 50000m 
    },
};


//Para las companias

app.MapGet("/api/companias", () => 
{
    app.Use(async (context, next) => {
        Console.WriteLine("Middleware para la companias");
        await next();
    
    });
    return Results.Ok(Almacenamiento.Companias);
})
.WithTags("Compañías")
.RequireAuthorization();

//Las companias por id

app.MapGet("/api/companias/{id}", (int id) => 
{
    var Compania = Almacenamiento.Companias.FirstOrDefault(c => c.Id == id);
    return Compania != null ? Results.Ok(Compania) : Results.NotFound();
})
.WithTags("Compañías")
.RequireAuthorization();

//Para agregar una compania

app.MapPost("/api/companias", (Compania Compania) => 
{
    Compania.Id = Almacenamiento.UltimoIdCompania + 1;
    Compania.FechaFundacion = DateTime.Now;
    Almacenamiento.Companias.Add(Compania);
    return Results.Created($"/api/companias/{Compania.Id}", Compania);
})
.WithTags("Compañías")
.RequireAuthorization();


//Para actualizar una compania

app.MapPut("/api/companias/{id}", (int id, Compania CompaniaActualizada) => 
{
    var Compania = Almacenamiento.Companias.FirstOrDefault(c => c.Id == id);
    if (Compania == null) return Results.NotFound();
    
    Compania.Nombre = CompaniaActualizada.Nombre;
    return Results.Ok(Compania);
})
.WithTags("Compañías")
.RequireAuthorization();

//Para eliminar una compania pero si tiene empleados no se puede eliminar

app.MapDelete("/api/companias/{id}", (int id) => 
{
    var Compania = Almacenamiento.Companias.FirstOrDefault(c => c.Id == id);
    if (Compania == null) return Results.NotFound();
    
    if (Almacenamiento.Empleados.Any(e => e.CompaniaId == id))
        return Results.BadRequest("No se puede eliminar: La compañía tiene empleados");
    
    Almacenamiento.Companias.Remove(Compania);
    return Results.NoContent();
})
.WithTags("Compañías")
.RequireAuthorization();

//Ahora si se puede eliminar una compania con empleados

app.MapDelete("/api/companias/{id}/forzar", (int id) => 
{
    var Compania = Almacenamiento.Companias.FirstOrDefault(c => c.Id == id);
    if (Compania == null) return Results.NotFound();
    
    Almacenamiento.Empleados.RemoveAll(e => e.CompaniaId == id);
    Almacenamiento.Companias.Remove(Compania);
    return Results.NoContent();
})
.WithTags("Compañías")
.RequireAuthorization();

//Para obtener los empleados

app.MapGet("/api/empleados", () => 
    Results.Ok(Almacenamiento.Empleados))
.WithTags("Empleados")
.RequireAuthorization();

//Para obtener los empleados por id

app.MapGet("/api/empleados/{id}", (int id) => 
{
    var empleado = Almacenamiento.Empleados.FirstOrDefault(e => e.Id == id);
    return empleado != null ? Results.Ok(empleado) : Results.NotFound();
})
.WithTags("Empleados")
.RequireAuthorization();

//Para agregar un empleado

app.MapPost("/api/empleados", (Empleado empleado) => 
{
    if (!Almacenamiento.Companias.Any(c => c.Id == empleado.CompaniaId))
        return Results.NotFound("Compañía no encontrada");
    
    empleado.Id = Almacenamiento.UltimoIdEmpleado + 1;
    empleado.FechaContratacion = DateTime.Now;
if(string.IsNullOrWhiteSpace(empleado.Cargo)){
}

    Almacenamiento.Empleados.Add(empleado);
    return Results.Created($"/api/empleados/{empleado.Id}", empleado);
})
.WithTags("Empleados")
.RequireAuthorization();

//Para actualizar un empleado

app.MapPut("/api/empleados/{id}", (int id, Empleado empleadoActualizado) => 
{
    var empleado = Almacenamiento.Empleados.FirstOrDefault(e => e.Id == id);
    if (empleado == null) return Results.NotFound();
    
    empleado.Nombre = empleadoActualizado.Nombre;
    empleado.Cargo = empleadoActualizado.Cargo;
    empleado.Salario = empleadoActualizado.Salario;
    return Results.Ok(empleado);
})
.WithTags("Empleados")
.RequireAuthorization();

//Para eliminar un empleado

app.MapDelete("/api/empleados/{id}", (int id) => 
{
    var empleado = Almacenamiento.Empleados.FirstOrDefault(e => e.Id == id);
    if (empleado == null) return Results.NotFound();
    
    Almacenamiento.Empleados.Remove(empleado);
    return Results.NoContent();
})
.WithTags("Empleados")
.RequireAuthorization();

//Para despedir un empleado

app.MapPost("/api/empleados/{id}/despedir", (int id) => 
{
    var empleado = Almacenamiento.Empleados.FirstOrDefault(e => e.Id == id);
    if (empleado == null) return Results.NotFound();
    
    var fechaDespido = DateTime.Now;
    var añosTrabajados = (fechaDespido - empleado.FechaContratacion).TotalDays / 365.25;
    
    decimal prestacion = (empleado.Salario / 23.83m) * 15 * (decimal)añosTrabajados;
    
    var empleadoDespedido = new EmpleadoDespedido {
        Id = empleado.Id,
        Nombre = empleado.Nombre,
        Cargo = empleado.Cargo,
        FechaContratacion = empleado.FechaContratacion,
        FechaDespido = fechaDespido,
        Prestacion = prestacion
    };
    
    // Agregar empleado despedido a la lista de empleados despedidos

    Almacenamiento.EmpleadosDespedidos.Add(empleadoDespedido);
    Almacenamiento.Empleados.Remove(empleado);
    return Results.Ok(empleadoDespedido);
})
.WithTags("Despidos")
.RequireAuthorization();
//Para obtener los empleados despedidos

app.MapGet("/api/despidos", () => 
{ app.Use(async (context, next) => {
        Console.WriteLine("Middleware para los despidos");
        await next();
    
    });
    return Results.Ok(Almacenamiento.EmpleadosDespedidos);
})
.WithTags("Despidos")
.RequireAuthorization();


app.Run();


public class Compania
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public DateTime FechaFundacion { get; set; }
}

public class Empleado
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Cargo { get; set; } = null!;
    public int Edad { get; set; }
    public int CompaniaId { get; set; }
    public DateTime FechaContratacion { get; set; }
    public decimal Salario { get; set; }
}

public class EmpleadoDespedido
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Cargo { get; set; } = null!;
    public DateTime FechaContratacion { get; set; }
    public DateTime FechaDespido { get; set; }
    public decimal Prestacion { get; set; }
}


public static class Almacenamiento
{
    public static List<Compania> Companias = new();
    public static List<Empleado> Empleados = new();
    public static List<EmpleadoDespedido> EmpleadosDespedidos = new();

    public static int UltimoIdCompania => Companias.Any() ? Companias.Max(c => c.Id) : 0;
    public static int UltimoIdEmpleado => Empleados.Any() ? Empleados.Max(e => e.Id) : 0; 
    
    private static readonly string[] Cargos = {
    "Lava plato",
    "Pela papas",
    "Guaremate",
    "Cuida carros",
    "Vigilante"
    };
    
}
