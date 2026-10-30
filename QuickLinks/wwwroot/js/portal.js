// Portal público: modos simple/completo, búsqueda en vivo, filtros, fijadas, recientes y modales.
(() => {
    'use strict';

    const portal = document.getElementById('portal');
    if (!portal) {
        return;
    }

    const form = document.getElementById('portal-form');
    const basePath = document.body.dataset.baseUrl || '/';
    const MAX_RECENT = 5;
    const SEARCH_DELAY_MS = 300;

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
    const SHOW_ALL = 'showAllDefault';

    // ---------- Modos ----------
    const showAllField = document.getElementById('show-all-field');

    function setMode(mode) {
        const complete = mode === 'complete';
        portal.classList.toggle('mode-complete', complete);
        portal.classList.toggle('mode-simple', !complete);
        showAllField.value = complete ? 'true' : 'false';
        updateUrl();
        const input = portal.querySelector(complete ? '.portal-toolbar [data-search]' : '[data-search-mirror]');
        input?.focus();
    }

    document.addEventListener('click', e => {
        const modeButton = e.target.closest('[data-mode]');
        if (modeButton) {
            setMode(modeButton.dataset.mode);
        }
    });

    const showAllDefault = portal.querySelector('[data-show-all-default]');
    showAllDefault.checked = cookies.get(SHOW_ALL) === 'true';
    showAllDefault.addEventListener('change', () => cookies.set(SHOW_ALL, showAllDefault.checked ? 'true' : 'false', 365));

    // ---------- Búsqueda y filtros ----------
    const searchInputs = [...portal.querySelectorAll('[data-search]')];
    let searchTimer;
    let lastRequest;

    searchInputs.forEach(input => {
        input.addEventListener('input', () => {
            searchInputs.filter(other => other !== input).forEach(other => { other.value = input.value; });
            clearTimeout(searchTimer);
            searchTimer = setTimeout(refresh, SEARCH_DELAY_MS);
        });
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
    filtersToggle.addEventListener('click', () => {
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
        const request = Symbol();
        lastRequest = request;
        updateUrl();

        try {
            const response = await fetch(`${portal.dataset.resultsUrl}?${params}`, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }
            const html = await response.text();
            if (request !== lastRequest) {
                return; // llegó una respuesta más nueva mientras tanto
            }
            const doc = new DOMParser().parseFromString(html, 'text/html');
            document.getElementById('simple-results').innerHTML = doc.getElementById('simple-part').innerHTML;
            document.getElementById('complete-results').innerHTML = doc.getElementById('complete-part').innerHTML;
            document.getElementById('app-count').textContent = doc.getElementById('count-part').textContent;
        } catch (error) {
            console.error('No se pudieron actualizar los resultados', error);
        }
    }

    // ---------- Logos que no cargan: la tarjeta queda solo con el nombre ----------
    // Los eventos error de <img> no burbujean; se escuchan en fase de captura.
    document.addEventListener('error', e => {
        const img = e.target;
        if (img instanceof HTMLImageElement && img.classList.contains('app-card-logo')) {
            img.nextElementSibling?.classList.add('app-card-name-only');
            img.remove();
        }
    }, true);

    // ---------- Apertura de aplicaciones, recientes y mantenimiento ----------
    const maintenanceModal = new bootstrap.Modal(document.getElementById('maintenance-modal'));

    function registerRecent(appId) {
        const recent = cookies.getIds(RECENT).filter(id => id !== appId);
        recent.unshift(appId);
        cookies.setIds(RECENT, recent.slice(0, MAX_RECENT), 30);
    }

    async function showMaintenance(link) {
        document.getElementById('maintenance-title').textContent = link.dataset.appName;
        const message = document.querySelector('[data-maintenance-message]');
        message.textContent = 'Esta aplicación está en mantenimiento.';
        maintenanceModal.show();
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
    const detailsModal = new bootstrap.Modal(detailsModalEl);
    const pinButton = detailsModalEl.querySelector('[data-pin-toggle]');
    const openButton = detailsModalEl.querySelector('[data-open-app]');

    function renderPinButton(appId) {
        const pinned = cookies.getIds(PINNED).includes(appId);
        pinButton.textContent = pinned ? 'Quitar de fijadas' : 'Fijar en inicio';
        pinButton.classList.toggle('btn-outline-danger', pinned);
        pinButton.classList.toggle('btn-brand-outline', !pinned);
        pinButton.dataset.appId = String(appId);
    }

    function renderCarousel(images, name) {
        const carousel = document.getElementById('details-carousel');
        const indicators = carousel.querySelector('.carousel-indicators');
        const inner = carousel.querySelector('.carousel-inner');
        indicators.replaceChildren();
        inner.replaceChildren();

        const sources = images.length > 0 ? images : [portal.dataset.placeholder];
        sources.forEach((src, index) => {
            const item = document.createElement('div');
            item.className = `carousel-item${index === 0 ? ' active' : ''}`;
            const img = document.createElement('img');
            img.src = src;
            img.className = 'd-block w-100';
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
            renderCarousel(app.images, app.name);
            detailsModal.show();
        } catch (error) {
            console.error('No se pudo cargar el detalle de la aplicación', error);
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

    document.addEventListener('keydown', e => {
        const trigger = e.target.closest?.('[data-details]');
        if (trigger && (e.key === 'Enter' || e.key === ' ')) {
            e.preventDefault();
            openDetails(Number(trigger.dataset.details));
        }
    });

    pinButton.addEventListener('click', () => {
        const appId = Number(pinButton.dataset.appId);
        const pinned = cookies.getIds(PINNED);
        const updated = pinned.includes(appId) ? pinned.filter(id => id !== appId) : [...pinned, appId];
        cookies.setIds(PINNED, updated, 150);
        location.reload();
    });

    const unpinModal = new bootstrap.Modal(document.getElementById('unpin-modal'));
    const confirmUnpin = document.querySelector('[data-confirm-unpin]');

    document.addEventListener('click', e => {
        const unpin = e.target.closest('[data-unpin]');
        if (unpin) {
            confirmUnpin.dataset.appId = unpin.dataset.unpin;
            unpinModal.show();
        }
    });

    confirmUnpin.addEventListener('click', () => {
        const appId = Number(confirmUnpin.dataset.appId);
        cookies.setIds(PINNED, cookies.getIds(PINNED).filter(id => id !== appId), 150);
        location.reload();
    });
})();
