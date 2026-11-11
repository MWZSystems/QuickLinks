// Administración: vista previa de color. La confirmación al desactivar la hace MWZSystem.Ui (data-mwz-confirmar).
(() => {
    'use strict';

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
