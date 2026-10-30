// Administración: confirmación antes de desactivar y vista previa de color.
(() => {
    'use strict';

    document.addEventListener('submit', e => {
        const button = e.submitter;
        if (button?.classList.contains('btn-outline-danger') && !confirm('¿Seguro que quieres desactivar este registro?')) {
            e.preventDefault();
        }
    });

    document.querySelectorAll('[data-color-input]').forEach(input => {
        const text = document.querySelector(input.dataset.colorInput);
        if (!text) {
            return;
        }
        input.addEventListener('input', () => { text.value = input.value; });
        text.addEventListener('input', () => {
            if (/^#[0-9a-fA-F]{6}$/.test(text.value)) {
                input.value = text.value;
            }
        });
    });
})();
