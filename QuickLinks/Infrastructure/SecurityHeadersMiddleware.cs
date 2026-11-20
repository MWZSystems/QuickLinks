namespace QuickLinks.Infrastructure;

/// <summary>Agrega las cabeceras de seguridad estándar a todas las respuestas.</summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    // Estilos: las hojas y etiquetas <style> solo pueden venir del propio sitio (style-src-elem); los atributos
    // style siguen permitidos (style-src-attr) porque llevan colores configurados en la administración y el color
    // principal que pone MWZSystem.Ui. style-src queda como respaldo para navegadores sin esas dos directivas.
    private const string ContentSecurityPolicy =
        "default-src 'self'; img-src 'self' data: https:; style-src 'self' 'unsafe-inline'; " +
        "style-src-elem 'self'; style-src-attr 'unsafe-inline'; " +
        "script-src 'self'; font-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";

    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers.ContentSecurityPolicy = ContentSecurityPolicy;
        // El portal no usa cámara, micrófono, ubicación ni pagos: se desactivan para cualquier script.
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), usb=()";
        // Aísla la ventana de las que abra otro origen (los enlaces externos ya usan noopener).
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
        return next(context);
    }
}
