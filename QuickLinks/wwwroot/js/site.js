// Avisos emergentes: los muestra MWZSystem.Ui (mwz-ui.js). QuickLinks.toast(tipo, texto) se conserva para el código
// del portal. Tipos: success, info, warning y error. Los errores se quedan hasta cerrarlos.
// QuickLinks.toastAfterReload(tipo, texto) guarda el aviso para mostrarlo después de recargar la página.
window.QuickLinks = window.QuickLinks || {};
(() => {
    'use strict';

    const show = { success: mwz.aviso, info: mwz.info, warning: mwz.advertencia, error: mwz.error };
    window.QuickLinks.toast = (kind, text) => (show[kind] || mwz.info)(text);

    const pendingKey = 'quicklinks.toast';
    window.QuickLinks.toastAfterReload = (kind, text) => {
        try {
            sessionStorage.setItem(pendingKey, JSON.stringify({ kind, text }));
        } catch {
            // Sin almacenamiento (modo privado o bloqueado): el aviso simplemente no se muestra.
        }
    };

    try {
        const pending = JSON.parse(sessionStorage.getItem(pendingKey) || 'null');
        sessionStorage.removeItem(pendingKey);
        if (pending?.text) {
            window.QuickLinks.toast(pending.kind, pending.text);
        }
    } catch {
        // Sin almacenamiento: no hay avisos pendientes.
    }
})();
