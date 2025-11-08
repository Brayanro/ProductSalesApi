# Product Sales API

## Descripción
API RESTful para la gestión de productos y ventas desarrollada con ASP.NET Core 9.0. Implementa una arquitectura moderna y escalable, siguiendo principios SOLID, Clean Code y mejores prácticas de desarrollo backend.

## Características
- **Gestión de productos** (CRUD completo)
- **Registro y gestión de ventas** con ítems de venta
- **Autenticación y autorización** con JWT
- **Refresh tokens** para renovación de sesiones
- **Documentación interactiva** con Swagger y Scalar
- **Base de datos MySQL** con Entity Framework Core
- **Validaciones robustas** de datos
- **CORS configurado** para aplicaciones frontend

## Tecnologías Utilizadas
- **ASP.NET Core 9.0**
- **Entity Framework Core 9.0**
- **MySQL** (Pomelo.EntityFrameworkCore.MySql)
- **JWT Authentication** (Bearer tokens)
- **Swagger/OpenAPI** para documentación
- **Scalar** para documentación interactiva
- **Docker** para contenedorización

## Requisitos Previos
- **.NET SDK 9.0** o superior
- **MySQL Server 8.0** o superior
- **Docker** (opcional)

## Estructura del Proyecto
```
ProductSalesApi/
├── Auth/              # Helpers de autenticación (JWT)
├── Controllers/       # Controladores de la API
├── DTOs/             # Data Transfer Objects
├── Data/             # Contexto de base de datos
├── Entities/         # Modelos de dominio
├── Services/         # Lógica de negocio
├── Migrations/       # Migraciones de EF Core
└── Program.cs        # Punto de entrada y configuración
```

## Instalación y Configuración

### Variables de Entorno
1. Crea un archivo `appsettings.json` o modifica el existente con tu configuración:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProductSales;User=tu_usuario;Password=tu_contraseña;"
  },
  "Jwt": {
    "Key": "TuClaveSecretaSuperSeguraDeAlMenos32Caracteres",
    "Issuer": "ProductSalesApi",
    "Audience": "ProductSalesApi-Client",
    "ExpireMinutes": 120
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

2. Para desarrollo, puedes usar `appsettings.Development.json` para sobrescribir configuraciones.

### Configuración de Base de Datos
1. Asegúrate de tener MySQL instalado y en ejecución

2. Crea la base de datos:
```sql
CREATE DATABASE ProductSales;
```

3. Aplica las migraciones:
```bash
dotnet ef database update
```

### Instalación de Dependencias
```bash
dotnet restore
```

### Ejecución en Desarrollo
```bash
dotnet run
```

La API estará disponible en:
- **HTTP**: http://localhost:5058
- **HTTPS**: https://localhost:7207
- **Swagger UI**: https://localhost:7207 (raíz)
- **Scalar UI**: https://localhost:7207/scalar/v1

### Construcción para Producción
```bash
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

## Docker

### Construir la Imagen
```bash
docker build -t product-sales-api -f ProductSalesApi/Dockerfile .
```

### Ejecutar el Contenedor
```bash
docker run -d -p 8080:8080 -p 8081:8081 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=ProductSales;User=tu_usuario;Password=tu_contraseña;" \
  -e Jwt__Key="TuClaveSecretaSuperSeguraDeAlMenos32Caracteres" \
  --name product-sales-api \
  product-sales-api
```

## Endpoints Principales

### Autenticación
- `POST /api/auth/register` - Registrar nuevo usuario
- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/refresh` - Renovar token

### Productos
- `GET /api/products` - Listar todos los productos
- `GET /api/products/{id}` - Obtener producto por ID
- `POST /api/products` - Crear nuevo producto
- `PUT /api/products/{id}` - Actualizar producto
- `DELETE /api/products/{id}` - Eliminar producto

### Ventas
- `GET /api/sales/report` - Listar todas las ventas
- `POST /api/sales` - Crear nueva venta

## Buenas Prácticas Implementadas

### Clean Code
1. **Nombres Descriptivos**
   - Clases, métodos y variables con nombres que explican su propósito
   - Uso de convenciones de C# (PascalCase, camelCase)
   - Evitar abreviaciones confusas

2. **Métodos y Funciones**
   - Principio de Responsabilidad Única
   - Métodos pequeños y enfocados
   - Parámetros mínimos necesarios

### Principios SOLID

1. **Single Responsibility Principle (SRP)**
   - Cada clase tiene una única responsabilidad
   - Controladores solo manejan HTTP
   - Servicios contienen lógica de negocio

2. **Open/Closed Principle (OCP)**
   - Clases abiertas para extensión, cerradas para modificación
   - Uso de interfaces para abstracciones
   - DTOs para transferencia de datos

### Arquitectura y Organización

1. **Separación de Responsabilidades**
   - **Controllers**: Manejo de peticiones HTTP
   - **Services**: Lógica de negocio
   - **Entities**: Modelos de dominio
   - **DTOs**: Transferencia de datos
   - **Data**: Acceso a base de datos
   - **Auth**: Autenticación y autorización

2. **Gestión de Estado**
   - Entity Framework Core para persistencia
   - DbContext con scope por petición
   - Transacciones para operaciones complejas

3. **Manejo de Errores**
   - Try-catch en operaciones críticas
   - Respuestas HTTP apropiadas

4. **Validación**
   - Data Annotations en DTOs
   - Validación en servicios
   - ModelState en controladores

### Seguridad

1. **Autenticación JWT**
   - Tokens seguros con firma HMAC
   - Refresh tokens para renovación
   - Expiración configurable

2. **Autorización**
   - Protección de endpoints con `[Authorize]`
   - Validación de permisos por rol

3. **Validación de Datos**
   - Validación de entrada en DTOs
   - Sanitización de datos
   - Prevención de inyección SQL (EF Core)

4. **CORS**
   - Configuración restrictiva
   - Orígenes permitidos específicos

5. **Configuración Segura**
   - Secretos en variables de entorno
   - Claves JWT robustas
   - Conexiones seguras a base de datos

### Convenciones de Código

1. **C# Coding Conventions**
   - PascalCase para clases, métodos y propiedades
   - camelCase para variables locales y parámetros
   - Prefijo `I` para interfaces

2. **Organización de Archivos**
   - Un archivo por clase
   - Nombres de archivo coinciden con la clase
   - Agrupación lógica en carpetas

3. **Async/Await**
   - Operaciones asíncronas para I/O
   - Sufijo `Async` en métodos asíncronos
   - ConfigureAwait cuando es apropiado

4. **Dependency Injection**
   - Registro de servicios en `Program.cs`
   - Inyección por constructor
   - Scoped lifetime para servicios con estado

## Migraciones de Base de Datos

### Crear una Nueva Migración
```bash
dotnet ef migrations add NombreDeLaMigracion
```

### Aplicar Migraciones
```bash
dotnet ef database update
```

### Revertir Migración
```bash
dotnet ef database update NombreMigracionAnterior
```

### Eliminar Última Migración
```bash
dotnet ef migrations remove
```