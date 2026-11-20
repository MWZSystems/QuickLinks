// Portal público: búsqueda en vivo, filtros, fijadas, recientes y ventanas (<dialog> nativo).
(() => {
    'use strict';

    const portal = document.getElementById('portal');
    if (!portal) {
        return;
    }

    const form = document.getElementById('portal-form');
    const basePath = document.body.dataset.baseUrl || '/';
    // Valores de Configuración > Portal; los respaldos son los de fábrica.
    const setting = (name, fallback) => Number(portal.dataset[name]) || fallback;
    const MAX_RECENT = setting('recentMax', 5);
    const RECENT_DAYS = setting('recentDays', 30);
    const PINNED_DAYS = setting('pinnedDays', 150);
    const SEARCH_DELAY_MS = setting('searchDelay', 300);

    // ---------- Cookies ----------
    const cookies = {
        get(name) {
            const entry = document.cookie.split('; ').find(c => c.startsWith(`${name}=`));
            return entry ? decodeURIComponent(entry.substring(name.length + 1)) : null;
        },
        set(name, value, days) {
            const maxAge = Math.round(days * 24 * 60 * 60);
            document.cookie = `${name}=${encodeURIComponent(value)}; path=${basePath}; max-age=${maxAge}; SameSite=Lax`;
        },
        getIds(name) {
            return (this.get(name) || '').split(',').map(Number).filter(id => Number.isInteger(id) && id > 0);
        },
        setIds(name, ids, days) {
            this.set(name, ids.join(','), days);
        }
    };

    const PINNED = 'pinnedApps';
    const RECENT = 'recentlyUsedApps';

    // ---------- Búsqueda y filtros ----------
    const searchInput = portal.querySelector('[data-search]');
    let searchTimer;
    let pendingSearch;

    searchInput.addEventListener('input', () => {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(refresh, SEARCH_DELAY_MS);
    });

    form.addEventListener('submit', e => {
        e.preventDefault();
        clearTimeout(searchTimer);
        refresh();
    });

    const siteSelect = portal.querySelector('[data-site-select]');
    const buildingSelect = portal.querySelector('[data-building-select]');

    function syncBuildingOptions() {
        const siteId = siteSelect.value;
        [...buildingSelect.options].forEach(option => {
            if (option.value) {
                option.hidden = siteId !== '' && option.dataset.site !== siteId;
            }
        });
        if (buildingSelect.selectedOptions[0]?.hidden) {
            buildingSelect.value = '';
        }
    }

    siteSelect.addEventListener('change', syncBuildingOptions);
    buildingSelect.addEventListener('change', () => {
        const site = buildingSelect.selectedOptions[0]?.dataset.site;
        if (site) {
            siteSelect.value = site;
            syncBuildingOptions();
        }
    });
    syncBuildingOptions();

    portal.querySelectorAll('[data-filter]').forEach(el => el.addEventListener('change', refresh));

    portal.querySelector('[data-clear-filters]').addEventListener('click', () => {
        portal.querySelectorAll('[data-filter]').forEach(el => {
            if (el.type === 'checkbox') {
                el.checked = false;
            } else {
                el.value = '';
            }
        });
        syncBuildingOptions();
        refresh();
    });

    const filtersToggle = portal.querySelector('[data-toggle-filters]');
    const filtersPanel = document.getElementById('filters-panel');
    filtersToggle?.addEventListener('click', () => {
        filtersPanel.hidden = !filtersPanel.hidden;
        filtersToggle.setAttribute('aria-expanded', String(!filtersPanel.hidden));
    });

    function currentParams() {
        const params = new URLSearchParams();
        for (const [key, value] of new FormData(form)) {
            if (value !== '') {
                params.append(key, value);
            }
        }
        return params;
    }

    function updateUrl() {
        const query = currentParams().toString();
        history.replaceState(null, '', query ? `${form.action}?${query}` : form.action);
    }

    async function refresh() {
        const params = currentParams();
        // Una búsqueda nueva cancela la anterior: no se descargan resultados que ya no se van a mostrar.
        pendingSearch?.abort();
        const controller = new AbortController();
        pendingSearch = controller;
        updateUrl();

        try {
            const response = await fetch(`${portal.dataset.resultsUrl}?${params}`, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                signal: controller.signal
            });
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }
            const html = await response.text();
            const doc = new DOMParser().parseFromString(html, 'text/html');
            // Se mueven los nodos ya analizados en lugar de volver a interpretar HTML con innerHTML.
            document.getElementById('complete-results').replaceChildren(...doc.getElementById('complete-part').childNodes);
            document.getElementById('app-count').textContent = doc.getElementById('count-part').textContent;
        } catch (error) {
            if (error.name === 'AbortError') {
                return;
            }
            console.error('No se pudieron actualizar los resultados', error);
            QuickLinks.toast('error', 'No se pudo actualizar la búsqueda. Revisa tu conexión e intenta de nuevo.');
        }
    }

    // ---------- Franja de avisos: pausa con botón (teclado y pantallas táctiles) ----------
    document.querySelectorAll('[data-news-toggle]').forEach(button => button.addEventListener('click', () => {
        const paused = button.closest('.news-ticker').classList.toggle('is-paused');
        const label = paused ? 'Reanudar avisos' : 'Pausar avisos';
        button.setAttribute('aria-pressed', String(paused));
        button.setAttribute('aria-label', label);
        button.title = label;
        button.querySelector('i').className = `bi ${paused ? 'bi-play-fill' : 'bi-pause-fill'}`;
    }));

    // ---------- Logos que no cargan: la tarjeta queda solo con el nombre ----------
    // Los eventos error de <img> no burbujean; se escuchan en fase de captura.
    document.addEventListener('error', e => {
        const img = e.target;
        if (img instanceof HTMLImageElement && img.classList.contains('app-card-logo')) {
            img.nextElementSibling?.classList.add('app-card-name-only');
            const placeholder = document.createElement('i');
            placeholder.className = 'bi bi-window-stack app-card-placeholder';
            placeholder.setAttribute('aria-hidden', 'true');
            img.replaceWith(placeholder);
        } else if (img instanceof HTMLImageElement && img.classList.contains('app-pill-logo')) {
            img.remove();
        }
    }, true);

    // ---------- Ventanas ----------
    // <dialog> con showModal(): foco atrapado, Escape y fondo inerte los da el navegador.
    // Se cierran con los botones [data-dialog-close] o con un clic fuera, como antes.
    const openDialog = dialog => {
        if (!dialog.open) {
            dialog.showModal();
        }
    };
    document.querySelectorAll('dialog.ql-dialog').forEach(dialog => dialog.addEventListener('click', e => {
        if (e.target === dialog || e.target.closest('[data-dialog-close]')) {
            dialog.close();
        }
    }));

    // ---------- Apertura de aplicaciones, recientes y mantenimiento ----------
    const maintenanceDialog = document.getElementById('maintenance-modal');

    function registerRecent(appId) {
        const recent = cookies.getIds(RECENT).filter(id => id !== appId);
        recent.unshift(appId);
        cookies.setIds(RECENT, recent.slice(0, MAX_RECENT), RECENT_DAYS);
    }

    async function showMaintenance(link) {
        document.getElementById('maintenance-title').textContent = link.dataset.appName;
        const message = document.querySelector('[data-maintenance-message]');
        message.textContent = 'Esta aplicación está en mantenimiento.';
        openDialog(maintenanceDialog);
        try {
            const response = await fetch(`${portal.dataset.maintenanceUrl}/${link.dataset.appId}`);
            if (response.ok) {
                message.textContent = (await response.json()).message;
            }
        } catch (error) {
            console.error('No se pudo obtener el mensaje de mantenimiento', error);
        }
    }

    document.addEventListener('click', e => {
        if (e.target.closest('[data-details]')) {
            return; // lo atiende el manejador de detalle
        }
        const link = e.target.closest('.app-link');
        if (!link || !link.dataset.appId) {
            return;
        }
        if (link.classList.contains('is-blocked')) {
            e.preventDefault();
            showMaintenance(link);
            return;
        }
        registerRecent(Number(link.dataset.appId));
    });

    // ---------- Detalle y fijadas ----------
    const detailsModalEl = document.getElementById('details-modal');
    const pinButton = detailsModalEl.querySelector('[data-pin-toggle]');
    const openButton = detailsModalEl.querySelector('[data-open-app]');

    function renderPinButton(appId) {
        const pinned = cookies.getIds(PINNED).includes(appId);
        const icon = document.createElement('i');
        icon.className = `bi ${pinned ? 'bi-pin-angle' : 'bi-pin-angle-fill'} me-2`;
        icon.setAttribute('aria-hidden', 'true');
        pinButton.replaceChildren(icon, pinned ? 'Quitar de fijadas' : 'Fijar en inicio');
        pinButton.classList.toggle('btn-outline-danger', pinned);
        pinButton.classList.toggle('btn-brand-outline', !pinned);
        pinButton.dataset.appId = String(appId);
    }

    function renderCarousel(images, name, logo) {
        const carousel = document.getElementById('details-carousel');
        const indicators = carousel.querySelector('.carousel-indicators');
        const inner = carousel.querySelector('.carousel-inner');
        indicators.replaceChildren();
        inner.replaceChildren();

        // Sin carrusel se muestra el logo de la aplicación; sin logo, la imagen genérica.
        const fallback = logo || portal.dataset.placeholder;
        const sources = images.length > 0 ? images : [fallback];
        sources.forEach((src, index) => {
            const item = document.createElement('div');
            item.className = `carousel-item${index === 0 ? ' active' : ''}`;
            const img = document.createElement('img');
            img.src = src;
            img.className = images.length > 0 ? 'd-block w-100' : 'd-block mx-auto details-logo';
            img.alt = images.length > 0 ? `Imagen ${index + 1} de ${name}` : '';
            item.appendChild(img);
            inner.appendChild(item);

            if (sources.length > 1) {
                const dot = document.createElement('button');
                dot.type = 'button';
                dot.dataset.bsTarget = '#details-carousel';
                dot.dataset.bsSlideTo = String(index);
                dot.setAttribute('aria-label', `Imagen ${index + 1}`);
                if (index === 0) {
                    dot.className = 'active';
                    dot.setAttribute('aria-current', 'true');
                }
                indicators.appendChild(dot);
            }
        });
    }

    async function openDetails(appId) {
        try {
            const response = await fetch(`${portal.dataset.detailsUrl}/${appId}`);
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }
            const app = await response.json();
            document.getElementById('details-title').textContent = app.name;
            const field = name => detailsModalEl.querySelector(`[data-field="${name}"]`);
            field('description').textContent = app.description || 'Sin descripción.';
            field('sme').textContent = app.sme || 'No especificado';
            field('userManual').textContent = app.userManual || 'No especificado';
            field('version').textContent = app.version || 'No especificada';
            openButton.href = app.link;
            openButton.dataset.appId = String(app.id);
            openButton.dataset.appName = app.name;
            renderPinButton(app.id);
            renderCarousel(app.images, app.name, app.logo);
            openDialog(detailsModalEl);
        } catch (error) {
            console.error('No se pudo cargar el detalle de la aplicación', error);
            QuickLinks.toast('error', 'No se pudo abrir el detalle de la aplicación. Intenta de nuevo.');
        }
    }

    document.addEventListener('click', e => {
        const trigger = e.target.closest('[data-details]');
        if (trigger) {
            e.preventDefault();
            e.stopPropagation();
            openDetails(Number(trigger.dataset.details));
        }
    });

    pinButton.addEventListener('click', () => {
        const appId = Number(pinButton.dataset.appId);
        const pinned = cookies.getIds(PINNED);
        const wasPinned = pinned.includes(appId);
        const updated = wasPinned ? pinned.filter(id => id !== appId) : [...pinned, appId];
        cookies.setIds(PINNED, updated, PINNED_DAYS);
        QuickLinks.toastAfterReload('success', wasPinned ? 'Aplicación quitada de fijadas.' : 'Aplicación fijada en inicio.');
        location.reload();
    });

    const unpinDialog = document.getElementById('unpin-modal');
    const confirmUnpin = document.querySelector('[data-confirm-unpin]');

    document.addEventListener('click', e => {
        const unpin = e.target.closest('[data-unpin]');
        if (unpin) {
            confirmUnpin.dataset.appId = unpin.dataset.unpin;
            openDialog(unpinDialog);
        }
    });

    confirmUnpin.addEventListener('click', () => {
        const appId = Number(confirmUnpin.dataset.appId);
        cookies.setIds(PINNED, cookies.getIds(PINNED).filter(id => id !== appId), PINNED_DAYS);
        QuickLinks.toastAfterReload('success', 'Aplicación quitada de fijadas.');
        location.reload();
    });
})();
