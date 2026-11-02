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
| `Services/` | Lógica del portal, configuración, autenticación, avisos emergentes y almacenamiento de imágenes |
| `Views/Admin/` | Vistas de los catálogos administrativos |
| `QuickLinks.Tests/` | Pruebas unitarias (xUnit) |

## Avisos emergentes

Toda pantalla puede mostrar avisos arriba a la derecha (debajo de la barra superior; en el teléfono, a todo el ancho). Funcionan también cuando la acción redirige a otra página (Post/Redirect/Get): el aviso viaja en `TempData` y lo pinta `Views/Shared/_Feedback.cshtml`, incluida en los dos layouts.

| Tipo | Título | Cuándo usarlo | Se cierra solo |
|---|---|---|---|
| `success` | Listo | La acción se completó: creado, guardado, activado, desactivado, sesión iniciada. | 5 s |
| `info` | Información | No hubo cambio, pero el usuario debe saberlo. | 7 s |
| `warning` | Atención | Se guardó, pero hay una consecuencia o algo pendiente (por ejemplo, una categoría desactivada con aplicaciones que dejan de verse en ella). | 9 s |
| `error` | Error | No se pudo hacer: regla de negocio, conflicto, formulario inválido o error inesperado (mensaje amable, sin detalles técnicos). | Nunca: se cierra con la X |

Varios avisos se apilan con los errores primero; el temporizador se pausa al pasar el cursor o enfocar el aviso.

**En un controlador** (`Services/Feedback.cs`):

```csharp
this.Success("Categoría creada.");
this.Info("No había cambios que guardar.");
this.Warning("Categoría desactivada. 3 aplicaciones ya no se muestran en esta categoría.");
this.Error("No se puede desactivar: debe quedar al menos un administrador activo.");
return RedirectToAction(nameof(Index));
```

- Un formulario devuelto con `ModelState` inválido avisa solo: los errores generales (`ModelState.AddModelError(string.Empty, ...)`) aparecen tal cual y, si solo hay errores de campo, el aviso dice «Revisa los datos marcados en el formulario.». El detalle de cada campo sigue junto al campo.
- Un error inesperado en un POST no muestra la página de error: `Infrastructure/UiExceptionFilter.cs` lo registra en el log y vuelve a la pantalla anterior con un aviso de error.
- Las claves anteriores `TempData["Success"]`, `["Info"]`, `["Warning"]` y `["Error"]` se siguen mostrando, pero el código nuevo debe usar los métodos de arriba.

**En JavaScript** (`wwwroot/js/site.js`, cargado en ambos layouts):

```js
QuickLinks.toast('error', 'No se pudo actualizar la búsqueda. Revisa tu conexión e intenta de nuevo.');
// Si la página se va a recargar, el aviso se guarda y aparece después de la recarga:
QuickLinks.toastAfterReload('success', 'Aplicación fijada en inicio.');
location.reload();
```

No uses `alert()`. Escribe textos cortos y claros, sin flechas.

## Pruebas

```bash
dotnet test QuickLinks.slnx
```

## Operación

- Salud: `GET /health` revisa la conexión a la base de datos.
- Logs: `Logs/quicklinks-AAAAMMDD.log`, con rotación diaria y por tamaño.
- Las fechas se guardan en UTC y se muestran en la zona horaria configurada.

Estándares del equipo: ver `Estandares_de_Desarrollo.md`.
