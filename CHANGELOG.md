# Changelog

Formato: [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/). Versionado: [SemVer](https://semver.org/lang/es/).

## [1.0.0] - 2026-10-30

| Componente | Versión |
|---|---|
| Web | 1.0.0 |
| Base de datos | 1.0.0 (migración `InitialSchema`) |

### Agregado
- Réplica genérica de QuickLinks en ASP.NET Core MVC sobre .NET 10.
- Portal público con modo simple (fijadas, recientes y búsqueda) y modo completo (tarjetas por categoría o grupo).
- Búsqueda en vivo con espera de 300 ms, filtros por categoría, sitio, edificio y grupo.
- Detalle de aplicación con carrusel; fijadas y recientes guardadas en cookies del navegador.
- Franja de avisos y bloqueo de aplicaciones por mantenimiento programado.
- Administración de aplicaciones, categorías, grupos, sitios, edificios, avisos, mantenimientos, usuarios y configuración.
- Inicio de sesión con cuentas locales (hash PBKDF2, bloqueo tras 5 intentos fallidos).
- Marca configurable por cliente: nombre, logo, color principal y zona horaria.
- Esquema con los nombres del original (`CT_`, `SC_`, `SY_`) más índices únicos y fechas en UTC.
- Hospedaje `OutOfProcess` sin `apphost.exe`, como las demás aplicaciones .NET 10 del mismo hosting.

### Cambiado respecto al sistema original
- La autenticación con el servicio corporativo se sustituyó por cuentas locales.
- La sesión se revalida en cada petición: desactivar a un usuario revoca su acceso de inmediato.
- Las imágenes se guardan con nombre GUID, se valida su tipo y tamaño, y se sirven desde una carpeta configurable.
- Los avisos se muestran como texto codificado (sin `Html.Raw`).
- Las aplicaciones recientes conservan el orden de uso.
- El detalle público solo expone aplicaciones activas.
