// Avisos emergentes: los que manda el servidor (Views/Shared/_Feedback.cshtml) se activan al cargar,
// y QuickLinks.toast(tipo, texto) los crea desde JavaScript. Tipos: success, info, warning y error.
// Los errores se quedan hasta cerrarlos; los demás se ocultan solos y se pausan con el cursor o el foco.
// QuickLinks.toastAfterReload(tipo, texto) guarda el aviso para mostrarlo después de recargar la página.
window.QuickLinks = window.QuickLinks || {};
(() => {
    'use strict';

    const styles = {
        success: ['toast-success', 'bi-check-circle-fill', 'Listo', 5000],
        info: ['toast-info', 'bi-info-circle-fill', 'Información', 7000],
        warning: ['toast-warning', 'bi-exclamation-triangle-fill', 'Atención', 9000],
        error: ['toast-error', 'bi-x-circle-fill', 'Error', 0]
    };

    function stack() {
        let element = document.getElementById('toast-stack');
        if (!element) {
            element = Object.assign(document.createElement('div'), { id: 'toast-stack', className: 'toast-stack' });
            element.setAttribute('aria-live', 'polite');
            document.body.appendChild(element);
        }
        return element;
    }

    // Debajo de la barra superior, para no tapar el menú del usuario.
    function placeBelowHeader() {
        const header = document.querySelector('.admin-header, body > header');
        if (header) {
            stack().style.top = `${Math.max(header.getBoundingClientRect().bottom, 0) + 8}px`;
        }
    }

    function close(toast) {
        toast.classList.add('hiding');
        setTimeout(() => toast.remove(), 200);
    }

    function wire(toast) {
        toast.querySelector('.app-toast-close')?.addEventListener('click', () => close(toast));
        const timeout = Number(toast.dataset.timeout || 0);
        if (timeout <= 0) {
            return;
        }
        let remaining = timeout;
        let started = Date.now();
        let timer = setTimeout(() => close(toast), remaining);
        const pause = () => { clearTimeout(timer); remaining = Math.max(remaining - (Date.now() - started), 1500); };
        const resume = () => {
            if (toast.matches(':hover') || toast.contains(document.activeElement)) {
                return;
            }
            started = Date.now();
            timer = setTimeout(() => close(toast), remaining);
        };
        toast.addEventListener('mouseenter', pause);
        toast.addEventListener('focusin', pause);
        toast.addEventListener('mouseleave', resume);
        toast.addEventListener('focusout', () => setTimeout(resume));
    }

    window.QuickLinks.toast = (kind, text) => {
        const [css, icon, title, timeout] = styles[kind] || styles.info;
        // El mismo aviso no se repite mientras siga visible (por ejemplo, varias búsquedas fallidas seguidas).
        const repeated = [...stack().querySelectorAll('.app-toast')]
            .find(t => t.classList.contains(css) && t.querySelector('.app-toast-body > div:last-child')?.textContent === text);
        if (repeated) {
            return repeated;
        }
        const toast = document.createElement('div');
        toast.className = `app-toast ${css}`;
        toast.setAttribute('role', kind === 'error' || kind === 'warning' ? 'alert' : 'status');
        toast.dataset.timeout = timeout;

        const iconElement = Object.assign(document.createElement('i'), { className: `bi ${icon} app-toast-icon` });
        iconElement.setAttribute('aria-hidden', 'true');
        const body = Object.assign(document.createElement('div'), { className: 'app-toast-body' });
        body.append(
            Object.assign(document.createElement('div'), { className: 'app-toast-title', textContent: title }),
            Object.assign(document.createElement('div'), { textContent: text }));
        const button = Object.assign(document.createElement('button'), { type: 'button', className: 'app-toast-close', innerHTML: '<i class="bi bi-x-lg" aria-hidden="true"></i>' });
        button.setAttribute('aria-label', 'Cerrar aviso');
        toast.append(iconElement, body, button);

        // Los errores van primero en la pila.
        const target = stack();
        if (kind === 'error') {
            target.prepend(toast);
        } else {
            target.appendChild(toast);
        }
        placeBelowHeader();
        wire(toast);
        return toast;
    };

    const pendingKey = 'quicklinks.toast';
    window.QuickLinks.toastAfterReload = (kind, text) => {
        try {
            sessionStorage.setItem(pendingKey, JSON.stringify({ kind, text }));
        } catch {
            // Sin almacenamiento (modo privado o bloqueado): el aviso simplemente no se muestra.
        }
    };

    placeBelowHeader();
    try {
        const pending = JSON.parse(sessionStorage.getItem(pendingKey) || 'null');
        sessionStorage.removeItem(pendingKey);
        if (pending?.text) {
            window.QuickLinks.toast(pending.kind, pending.text);
        }
    } catch {
        // Sin almacenamiento: no hay avisos pendientes.
    }
    window.addEventListener('resize', placeBelowHeader);
    window.addEventListener('scroll', placeBelowHeader, { passive: true });
    document.querySelectorAll('#toast-stack .app-toast').forEach(wire);
})();
