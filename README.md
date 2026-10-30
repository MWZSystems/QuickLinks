# QuickLinks

Portal de accesos rápidos a las aplicaciones internas de una organización. Es un producto genérico: cada cliente tiene su propia instancia y su propia base de datos, y la marca se configura desde la administración.

## Requisitos

- .NET SDK 10
- SQL Server 2019 o posterior (o Azure SQL)
- Herramienta de EF Core: `dotnet tool install --global dotnet-ef --version 10.0.14`

## Levantar en local

1. Configura la cadena de conexión como secreto (no se guarda en el repositorio):

   ```bash
   dotnet user-secrets set "ConnectionStrings:QuickLinksDb" "Server=...;Database=...;User Id=...;Password=...;Encrypt=True;TrustServerCertificate=True;" --project QuickLinks/QuickLinks.csproj
   ```

2. Define la contraseña inicial del usuario `admin` (se aplica en el primer arranque si aún no tiene una):

   ```bash
   dotnet user-secrets set "Seed:AdminPassword" "<contraseña de al menos 12 caracteres>" --project QuickLinks/QuickLinks.csproj
   ```

3. Crea o actualiza la base de datos:

   ```bash
   dotnet ef database update --project QuickLinks/QuickLinks.csproj
   ```

4. Ejecuta:

   ```bash
   dotnet run --project QuickLinks/QuickLinks.csproj
   ```

   El portal queda en `http://localhost:5133` y la administración en `/Account/Login`.

## Alta de un cliente nuevo

1. Crea una base de datos vacía y aplica el script de migraciones (`dotnet ef migrations script --idempotent`).
2. Publica la aplicación y configura en el servidor, como variables de entorno:
   - `ConnectionStrings__QuickLinksDb`
   - `Seed__AdminPassword` (solo para el primer arranque; después se puede quitar)
   - `Media__RootPath`: carpeta donde se guardarán logos e imágenes, fuera de la carpeta publicada.
3. Entra como `admin` y en **Configuración** define nombre, logo, color y zona horaria.
4. Da de alta sitios, edificios, categorías y aplicaciones.

## Estructura

| Carpeta | Contenido |
|---|---|
| `Controllers/` | Portal público (`Home`), inicio de sesión (`Account`) y catálogos en `Admin/` |
| `Data/` | `AppDbContext`, configuraciones de EF por entidad, migraciones e inicialización |
| `Models/Entities/` | Entidades mapeadas a las tablas `CT_`, `SC_` y `SY_` |
| `Models/ViewModels/` | Modelos de las vistas del portal y de la administración |
| `Services/` | Lógica del portal, configuración, autenticación y almacenamiento de imágenes |
| `Views/Admin/` | Vistas de los catálogos administrativos |

## Operación

- Salud: `GET /health` revisa la conexión a la base de datos.
- Logs: `Logs/quicklinks-AAAAMMDD.log`, con rotación diaria y por tamaño.
- Las fechas se guardan en UTC y se muestran en la zona horaria configurada.

Estándares del equipo: ver `Estandares_de_Desarrollo.md`.
