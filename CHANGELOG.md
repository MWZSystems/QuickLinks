# Changelog

Formato: [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/). Versionado: [SemVer](https://semver.org/lang/es/).

## [Desarrollo 1.0.0] - 2026-11-12 (segunda revisión)

- Se quitaron las flechas de «Volver al portal» y de las tarjetas de configuración.
- Ningún texto baja de 16 px: etiqueta de versión, insignias, rótulos de los filtros del portal y encabezados de administración. Los iconos pequeños (enlace externo, orden, separador) conservan su tamaño.
- Se retiraron los estilos en línea de medidas fijas en administración; el color de cada categoría se dibuja en SVG. Quedan tres estilos en línea que llevan valores configurados por quien administra (color de la marca en la página, color y velocidad de la franja de avisos, y color de cada sección del portal).
- Se conservó la flecha doble de ordenar columnas, que es un control con función.

Verificación: compilación sin advertencias y 16 pruebas; portal y acceso revisados en local. Las páginas de administración exigen sesión y no se vieron en un navegador.

## [Desarrollo 1.0.0] - 2026-11-11 (resumen de administración)

- Administración abre con una página nueva, «Resumen»: aplicaciones, aplicaciones en mantenimiento ahora, avisos activos, mantenimientos programados, usuarios y aplicaciones por categoría. Cada tarjeta abre su catálogo. «Activo» y «en mantenimiento» usan la misma definición que el portal.
- El portal de los empleados no cambia.
- `MWZSystem.Ui` 1.4.0 (antes 1.0.0).

Verificación: compilación sin advertencias y 16 pruebas (cinco nuevas del armado del resumen). El portal y el acceso se revisaron en local a 1280 y 375 px con el paquete nuevo, sin desborde ni errores en consola. La página «Resumen» exige sesión de administración y no se vio en un navegador: queda para revisión del usuario. Sus consultas a la base no tienen prueba automática porque el repositorio no tiene arnés con base de datos.

Cierre: fuente `29996b7` en `Releases/v1.0.0` y `main`, publicada en site7 con `dotnet publish` y el perfil Web Deploy propio (la contraseña de despliegue se toma de los secretos locales del proyecto). Comprobado de forma anónima: el portal y el acceso responden 200 y `/Summary` redirige al acceso.

## [1.0.0] - 2026-11-11 (interfaz común)

### Cambiado
- QuickLinks adopta `MWZSystem.Ui` 1.0.0, la base de interfaz común de las aplicaciones de MWZSystems: misma estructura de página, letra base de 18 px (antes 15 px) con Atkinson Hyperlegible, campos y botones más altos para usarse con el dedo, indicador de carga al navegar y al guardar, y un solo envío por formulario.
- Los avisos emergentes los muestra el paquete (mismos cuatro tipos y duraciones); `QuickLinks.toast` y `this.Success/Info/Warning/Error` siguen funcionando igual.
- La confirmación al desactivar un registro es una ventana de la aplicación en lugar del cuadro del navegador.
- El menú de administración y la barra superior son los de la estructura común; en teléfono el menú se abre con un botón.
- Página de error común para 403, 404 y 500, con número de referencia.

### Eliminado
- Copias locales de Bootstrap, Bootstrap Icons y jQuery en `wwwroot/lib` y `libman.json`: llegan con el paquete.
- Estilos y script propios de avisos y de la estructura de administración, sustituidos por los del paquete.

### Sin verificar
- Las pantallas de administración tras iniciar sesión no se revisaron en el navegador.

## [1.0.0] - 2026-11-11

### Documentación
- `LICENSE` deja de ser MIT: derechos reservados a nombre de MWZSystems, por decisión del usuario, para poder comercializar la aplicación. Los componentes de terceros conservan sus licencias (`THIRD-PARTY-NOTICES.md`).
- Nuevo `THIRD-PARTY-NOTICES.md`: componentes de terceros y sus licencias (todas permisivas, sin plantillas ni librerías comerciales).

## [1.0.0] - 2026-10-30

| Componente | Versión |
|---|---|
| Web | 1.0.0 |
| Base de datos | 1.0.0 (migración `InitialSchema`) |

### Agregado
- Réplica genérica de QuickLinks en ASP.NET Core MVC sobre .NET 10.
- Portal público que siempre muestra todas las aplicaciones disponibles (tarjetas por categoría o grupo), con fijadas y recientes como accesos rápidos arriba.
- Búsqueda en vivo con espera de 300 ms, filtros por categoría, sitio, edificio y grupo.
- Detalle de aplicación con carrusel; fijadas y recientes guardadas en cookies del navegador.
- Franja de avisos y bloqueo de aplicaciones por mantenimiento programado.
- Administración de aplicaciones, categorías, grupos, sitios, edificios, avisos, mantenimientos, usuarios y configuración.
- Inicio de sesión con cuentas locales (hash PBKDF2, bloqueo por intentos fallidos).
- Menú de configuración por cliente con seis apartados: Marca (nombre, empresa, color, logo), Portal (texto del buscador, filtros, contador, fijadas y recientes, pestaña nueva, espera de búsqueda), Avisos (etiqueta, color y velocidad de la franja), Seguridad (intentos y minutos de bloqueo, duración de sesión, longitud mínima de contraseña), Imágenes (peso máximo y tamaño del carrusel) y Regional (zona horaria y validación de enlaces). Las claves nuevas de `CT_Settings` se crean al guardar por primera vez; sin fila usan su valor por defecto.
- Esquema con los nombres del original (`CT_`, `SC_`, `SY_`) más índices únicos y fechas en UTC.
- Hospedaje `OutOfProcess` sin `apphost.exe`, como las demás aplicaciones .NET 10 del mismo hosting.
- El acceso de administradores está en un menú discreto (⋮) del encabezado del portal, para que los usuarios no lo confundan con un inicio de sesión propio.
- Íconos (Bootstrap Icons, servidos localmente) en el portal, el login y la administración: botones, menú, encabezados, estados y acciones.
- El detalle de una aplicación sin carrusel muestra su logo.
- La versión de la aplicación se muestra siempre junto a la marca (portal, login y administración) y en el pie de página.
- Avisos emergentes de éxito, información, advertencia y error en todo el sistema (`this.Success(...)` en los controladores y `QuickLinks.toast(...)` en JavaScript), con pila arriba a la derecha, cierre automático salvo los errores y pausa al pasar el cursor. Sustituyen la franja fija de la administración y el resumen de validación de los formularios. Desactivar una categoría, grupo, edificio o sitio que todavía se usa avisa como advertencia; no poder desactivar al último administrador avisa como error; iniciar y cerrar sesión, fijar y quitar fijadas también avisan. Un error inesperado al guardar vuelve a la pantalla anterior con un aviso amable.
- Pruebas unitarias en `QuickLinks.Tests` (servicio de avisos).
- En el teléfono, la administración ya no se ensancha más que la pantalla (encabezado y tablas).
- Guía de uso en PowerPoint (`wwwroot/docs/QuickLinks-Guia-de-uso.pptx`) con capturas y explicación de cada botón para usuarios y administradores; se descarga desde el menú ⋮ del portal y desde el menú de administración.
- Los formularios solo exigen los campos marcados como obligatorios; editar una aplicación ya no pide imágenes de carrusel.
- Las imágenes de `App_Data/media` se incluyen en el publish; un archivo de medios inexistente responde 404 y el portal muestra solo el nombre si un logo no carga.

### Cambiado respecto al sistema original
- La autenticación con el servicio corporativo se sustituyó por cuentas locales.
- La sesión se revalida en cada petición: desactivar a un usuario revoca su acceso de inmediato.
- Las imágenes se guardan con nombre GUID, se valida su tipo y tamaño, y se sirven desde una carpeta configurable.
- Los avisos se muestran como texto codificado (sin `Html.Raw`).
- Las aplicaciones recientes conservan el orden de uso.
- El detalle público solo expone aplicaciones activas.
