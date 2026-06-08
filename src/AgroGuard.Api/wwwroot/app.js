const state = {
    token: localStorage.getItem("agroguard.token"),
    user: JSON.parse(localStorage.getItem("agroguard.user") || "null"),
    latitude: -15.78,
    longitude: -47.93,
    cropName: "Soybean",
    climate: null,
    events: [],
    globalFarms: [],
    chartType: "temperature",
    chatOpen: false,
    chatStarted: false
};

const elements = {
    toast: document.querySelector("#toast"),
    authScreen: document.querySelector("#authScreen"),
    appScreen: document.querySelector("#appScreen"),
    loginTab: document.querySelector("#loginTab"),
    registerTab: document.querySelector("#registerTab"),
    authFeedback: document.querySelector("#authFeedback"),
    loginForm: document.querySelector("#loginForm"),
    registerForm: document.querySelector("#registerForm"),
    logoutButton: document.querySelector("#logoutButton"),
    refreshButton: document.querySelector("#refreshButton"),
    locationInput: document.querySelector("#locationInput"),
    locationBadge: document.querySelector("#locationBadge"),
    searchButton: document.querySelector("#searchButton"),
    geoButton: document.querySelector("#geoButton"),
    tempValue: document.querySelector("#tempValue"),
    tempDetail: document.querySelector("#tempDetail"),
    rainValue: document.querySelector("#rainValue"),
    rainDetail: document.querySelector("#rainDetail"),
    humidityValue: document.querySelector("#humidityValue"),
    humidityDetail: document.querySelector("#humidityDetail"),
    solarValue: document.querySelector("#solarValue"),
    solarDetail: document.querySelector("#solarDetail"),
    ndviValue: document.querySelector("#ndviValue"),
    ndviDetail: document.querySelector("#ndviDetail"),
    ndviFill: document.querySelector("#ndviFill"),
    eventsBadge: document.querySelector("#eventsBadge"),
    naturalEventsList: document.querySelector("#naturalEventsList"),
    coordinateAnalysis: document.querySelector("#coordinateAnalysis"),
    analysisSource: document.querySelector("#analysisSource"),
    worldFarmList: document.querySelector("#worldFarmList"),
    climateChart: document.querySelector("#climateChart"),
    loadingOverlay: document.querySelector("#loadingOverlay"),
    chatToggle: document.querySelector("#chatToggle"),
    chatPanel: document.querySelector("#chatPanel"),
    chatClose: document.querySelector("#chatClose"),
    chatMessages: document.querySelector("#chatMessages"),
    chatForm: document.querySelector("#chatForm"),
    chatInput: document.querySelector("#chatInput")
};

let worldMap;
let selectedMarker;
let eventLayer;
let farmLayer;
let resizeTimer;

const riskLevelLabels = {
    Low: { label: "Risco baixo", className: "low" },
    Moderate: { label: "Atencao", className: "moderate" },
    High: { label: "Risco alto", className: "high" },
    Critical: { label: "Risco critico", className: "critical" }
};

const cropLabels = {
    Almond: "Amendoa",
    Canola: "Canola",
    Coffee: "Cafe",
    Corn: "Milho",
    Cotton: "Algodao",
    Grapevine: "Uva",
    Olive: "Oliva",
    Pasture: "Pastagem",
    Rice: "Arroz",
    Soybean: "Soja",
    Sugarcane: "Cana-de-acucar",
    Wheat: "Trigo"
};

const eventCategoryClasses = {
    drought: "drought",
    wildfires: "wildfires",
    floods: "floods",
    severeStorms: "severeStorms"
};

document.addEventListener("DOMContentLoaded", () => {
    bindEvents();
    renderShell();

    if (state.token) {
        bootApplication();
    }
});

function bindEvents() {
    elements.loginTab.addEventListener("click", () => setAuthMode("login"));
    elements.registerTab.addEventListener("click", () => setAuthMode("register"));
    elements.loginForm.addEventListener("submit", submitLogin);
    elements.registerForm.addEventListener("submit", submitRegister);
    elements.logoutButton.addEventListener("click", logout);
    elements.refreshButton.addEventListener("click", () => loadAllData(true));
    elements.searchButton.addEventListener("click", handleCoordinateSearch);
    elements.locationInput.addEventListener("keydown", (event) => {
        if (event.key === "Enter") {
            handleCoordinateSearch();
        }
    });
    elements.geoButton.addEventListener("click", useBrowserLocation);

    document.querySelectorAll(".chart-tab").forEach((button) => {
        button.addEventListener("click", () => {
            state.chartType = button.dataset.chart;
            document.querySelectorAll(".chart-tab").forEach((tab) => tab.classList.toggle("active", tab === button));
            renderChart();
        });
    });

    elements.chatToggle.addEventListener("click", toggleChat);
    elements.chatClose.addEventListener("click", toggleChat);
    elements.chatForm.addEventListener("submit", submitChatMessage);
    document.querySelectorAll(".chat-suggestions button").forEach((button) => {
        button.addEventListener("click", () => {
            elements.chatInput.value = button.dataset.message || "";
            elements.chatInput.focus();
        });
    });

    window.addEventListener("resize", () => {
        window.clearTimeout(resizeTimer);
        resizeTimer = window.setTimeout(renderChart, 150);
    });
}

function setAuthMode(mode) {
    const isLogin = mode === "login";
    elements.loginTab.classList.toggle("active", isLogin);
    elements.registerTab.classList.toggle("active", !isLogin);
    elements.loginForm.classList.toggle("hidden", !isLogin);
    elements.registerForm.classList.toggle("hidden", isLogin);
    showAuthFeedback("", "");
}

async function submitLogin(event) {
    event.preventDefault();
    const form = event.currentTarget;
    const payload = formToObject(form);
    setAuthLoading(form, true, "Entrando...");
    showAuthFeedback("info", "Validando credenciais e preparando painel NASA...");

    try {
        const response = await request("/api/auth/login", {
            method: "POST",
            body: payload
        }, false);

        showAuthFeedback("success", "Login confirmado. Carregando dados de satelite...");
        setSession(response);
        showToast("Login realizado.", "success");
        await bootApplication();
    } catch (error) {
        const message = normalizeAuthError(error.message, "Nao foi possivel entrar.");
        showAuthFeedback("error", message);
        showToast(message, "error");
    } finally {
        setAuthLoading(form, false);
    }
}

async function submitRegister(event) {
    event.preventDefault();
    const form = event.currentTarget;
    const payload = formToObject(form);
    setAuthLoading(form, true, "Criando conta...");
    showAuthFeedback("info", "Criando usuario e emitindo token JWT...");

    try {
        const response = await request("/api/auth/register", {
            method: "POST",
            body: payload
        }, false);

        showAuthFeedback("success", "Conta criada. Carregando painel NASA...");
        setSession(response);
        showToast("Conta criada.", "success");
        await bootApplication();
    } catch (error) {
        const message = normalizeAuthError(error.message, "Nao foi possivel criar a conta.");
        showAuthFeedback("error", message);
        showToast(message, "error");
    } finally {
        setAuthLoading(form, false);
    }
}

function setSession(response) {
    state.token = response.token;
    state.user = {
        id: response.userId,
        name: response.name,
        email: response.email,
        role: response.role
    };

    localStorage.setItem("agroguard.token", state.token);
    localStorage.setItem("agroguard.user", JSON.stringify(state.user));
    renderShell();
}

function setAuthLoading(form, isLoading, loadingText = "Carregando...") {
    const submitButton = form.querySelector("button[type='submit']");
    const controls = [
        ...form.querySelectorAll("input, button"),
        elements.loginTab,
        elements.registerTab
    ];

    controls.forEach((control) => {
        control.disabled = isLoading;
    });

    if (!submitButton) {
        return;
    }

    if (!submitButton.dataset.defaultLabel) {
        submitButton.dataset.defaultLabel = submitButton.textContent.trim();
    }

    submitButton.classList.toggle("is-loading", isLoading);
    submitButton.innerHTML = isLoading
        ? `<span class="button-spinner" aria-hidden="true"></span><span>${escapeHtml(loadingText)}</span>`
        : escapeHtml(submitButton.dataset.defaultLabel);
}

function showAuthFeedback(type, message) {
    if (!elements.authFeedback) {
        return;
    }

    elements.authFeedback.textContent = message;
    elements.authFeedback.className = message
        ? `auth-feedback ${type}`
        : "auth-feedback hidden";
}

function normalizeAuthError(message, fallback) {
    if (/403|forbidden|proibido|invalid|unauthorized|senha|password|credential/i.test(message)) {
        return "Email ou senha invalidos. Verifique os dados ou crie uma nova conta.";
    }

    if (/oracle|database|connection|ora-|socket|network|banco/i.test(message)) {
        return "Banco Oracle indisponivel. Confirme se o container esta rodando antes de entrar.";
    }

    if (/already|exists|duplicate|unique|ja existe|duplic/i.test(message)) {
        return "Esse email ja esta cadastrado. Use Login ou informe outro email.";
    }

    return `${fallback} ${message || "Tente novamente em alguns segundos."}`;
}

function logout() {
    state.token = null;
    state.user = null;
    state.climate = null;
    state.events = [];
    state.globalFarms = [];
    localStorage.removeItem("agroguard.token");
    localStorage.removeItem("agroguard.user");
    renderShell();
}

function renderShell() {
    const authenticated = Boolean(state.token);
    elements.authScreen.classList.toggle("hidden", authenticated);
    elements.appScreen.classList.toggle("hidden", !authenticated);
}

async function bootApplication() {
    renderShell();
    initMap();
    startChat();
    await loadAllData(true);
}

async function loadAllData(reloadFarms = false) {
    if (!state.token) {
        return;
    }

    showLoading(true);
    try {
        const [climate, events] = await Promise.all([
            loadClimate(),
            loadEvents()
        ]);

        state.climate = climate;
        state.events = events.events || [];

        renderDashboard();
        renderEvents();
        renderAnalysis();
        renderMap();
        renderChart();

        if (reloadFarms || state.globalFarms.length === 0) {
            state.globalFarms = await request("/api/nasa/global-farms");
            renderGlobalFarms();
            renderMap();
        }
    } catch (error) {
        showToast(error.message, "error");
    } finally {
        showLoading(false);
    }
}

async function loadLocationOnly() {
    showLoading(true);
    try {
        state.climate = await loadClimate();
        renderDashboard();
        renderAnalysis();
        renderMap();
        renderChart();
    } catch (error) {
        showToast(error.message, "error");
    } finally {
        showLoading(false);
    }
}

function loadClimate() {
    const query = new URLSearchParams({
        latitude: state.latitude.toString(),
        longitude: state.longitude.toString(),
        cropName: state.cropName,
        days: "30"
    });

    return request(`/api/nasa/climate?${query}`);
}

function loadEvents() {
    return request("/api/nasa/events?days=60");
}

function handleCoordinateSearch() {
    const parsed = parseCoordinates(elements.locationInput.value);
    if (!parsed) {
        showToast("Use o formato: latitude, longitude. Exemplo: -23.55, -46.63", "error");
        return;
    }

    setLocation(parsed.latitude, parsed.longitude, state.cropName, true);
}

function useBrowserLocation() {
    if (!navigator.geolocation) {
        showToast("Geolocalizacao nao suportada neste navegador.", "error");
        return;
    }

    navigator.geolocation.getCurrentPosition(
        (position) => setLocation(position.coords.latitude, position.coords.longitude, state.cropName, true),
        () => showToast("Nao foi possivel obter sua localizacao.", "error"),
        { enableHighAccuracy: true, timeout: 10000 });
}

function setLocation(latitude, longitude, cropName = state.cropName, shouldLoad = false) {
    state.latitude = roundCoordinate(latitude);
    state.longitude = roundCoordinate(longitude);
    state.cropName = cropName || "Soybean";

    const locationText = `${state.latitude.toFixed(4)}, ${state.longitude.toFixed(4)}`;
    elements.locationInput.value = locationText;
    elements.locationBadge.textContent = locationText;

    if (worldMap) {
        worldMap.setView([state.latitude, state.longitude], Math.max(worldMap.getZoom(), 5));
    }

    renderMap();

    if (shouldLoad) {
        loadLocationOnly();
    }
}

function initMap() {
    if (worldMap || !window.L) {
        return;
    }

    worldMap = L.map("worldMap", {
        center: [state.latitude, state.longitude],
        zoom: 4,
        zoomControl: true,
        attributionControl: true
    });

    L.tileLayer("https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png", {
        maxZoom: 19,
        attribution: "&copy; OpenStreetMap | CARTO | NASA"
    }).addTo(worldMap);

    eventLayer = L.layerGroup().addTo(worldMap);
    farmLayer = L.layerGroup().addTo(worldMap);

    worldMap.on("click", (event) => {
        setLocation(event.latlng.lat, event.latlng.lng, state.cropName, true);
    });

    window.setTimeout(() => {
        worldMap.invalidateSize();
        renderMap();
    }, 50);
}

function renderMap() {
    if (!worldMap || !eventLayer || !farmLayer) {
        return;
    }

    eventLayer.clearLayers();
    farmLayer.clearLayers();

    if (!selectedMarker) {
        selectedMarker = L.marker([state.latitude, state.longitude], {
            icon: makeMapIcon("selected", "AG"),
            zIndexOffset: 1000
        }).addTo(worldMap);
    } else {
        selectedMarker.setLatLng([state.latitude, state.longitude]);
    }

    selectedMarker.bindPopup(`
        <strong>Area analisada</strong><br>
        ${state.latitude.toFixed(4)}, ${state.longitude.toFixed(4)}<br>
        Cultura: ${escapeHtml(translateCrop(state.cropName))}
    `);

    for (const eventData of state.events) {
        if (eventData.latitude === null || eventData.longitude === null) {
            continue;
        }

        const className = eventCategoryClasses[eventData.categoryId] || "default";
        L.marker([eventData.latitude, eventData.longitude], {
            icon: makeMapIcon(className, "E")
        })
            .addTo(eventLayer)
            .bindPopup(`
                <strong>${escapeHtml(eventData.categoryLabel)}</strong><br>
                ${escapeHtml(eventData.title)}<br>
                ${eventData.date ? formatDate(eventData.date) : "Data nao informada"}
            `);
    }

    for (const farm of state.globalFarms) {
        L.marker([farm.latitude, farm.longitude], {
            icon: makeMapIcon("farm", "F")
        })
            .addTo(farmLayer)
            .bindPopup(`
                <strong>${escapeHtml(farm.name)}</strong><br>
                ${escapeHtml(farm.country)} | ${escapeHtml(translateCrop(farm.cropName))}<br>
                NDVI ${formatNumber(farm.estimatedNdvi, 3)}
            `)
            .on("click", () => setLocation(farm.latitude, farm.longitude, farm.cropName, true));
    }
}

function makeMapIcon(className, text) {
    return L.divIcon({
        html: `<div class="map-pin ${className}">${escapeHtml(text)}</div>`,
        className: "",
        iconSize: [28, 28],
        iconAnchor: [14, 14]
    });
}

function renderDashboard() {
    const climate = state.climate;
    if (!climate) {
        return;
    }

    const summary = climate.summary;
    const vegetation = climate.vegetation;

    elements.tempValue.textContent = `${formatNumber(summary.averageTemperatureCelsius, 1)} C`;
    elements.tempDetail.textContent = `Max: ${formatNumber(summary.maximumTemperatureCelsius, 1)} C | Min: ${formatNumber(summary.minimumTemperatureCelsius, 1)} C`;

    elements.rainValue.textContent = `${formatNumber(summary.totalRainfallMillimeters, 1)} mm`;
    elements.rainDetail.textContent = `${summary.daysWithRain} dias com chuva`;

    elements.humidityValue.textContent = `${formatNumber(summary.averageRelativeHumidityPercent, 1)}%`;
    elements.humidityDetail.textContent = "Media dos ultimos 30 dias";

    elements.solarValue.textContent = `${formatNumber(summary.averageSolarRadiationMjPerSquareMeter, 1)} MJ/m2`;
    elements.solarDetail.textContent = "Radiacao media diaria";

    elements.ndviValue.textContent = formatNumber(vegetation.ndvi, 3);
    elements.ndviValue.style.color = vegetation.color;
    elements.ndviDetail.textContent = vegetation.classification;
    elements.ndviDetail.style.color = vegetation.color;
    elements.ndviFill.style.width = `${Math.max(0, Math.min(100, vegetation.scorePercent))}%`;
    elements.ndviFill.style.background = vegetation.color;
    elements.analysisSource.textContent = `${climate.source} | ${formatDate(climate.startDate)} - ${formatDate(climate.endDate)}`;
}

function renderEvents() {
    const events = state.events;
    elements.eventsBadge.textContent = `${events.length} alertas`;

    if (events.length === 0) {
        elements.naturalEventsList.innerHTML = emptyState("Nenhum evento natural aberto retornado pela NASA EONET.");
        return;
    }

    elements.naturalEventsList.innerHTML = events.slice(0, 16).map((eventData) => {
        const color = getEventColor(eventData.categoryId);
        return `
            <article class="event-item">
                <span class="event-dot" style="background:${color}"></span>
                <div>
                    <strong>${escapeHtml(eventData.title)}</strong>
                    <span>${escapeHtml(eventData.categoryLabel)} | ${eventData.date ? formatDate(eventData.date) : "Data nao informada"}</span>
                </div>
            </article>
        `;
    }).join("");
}

function renderAnalysis() {
    const climate = state.climate;
    if (!climate) {
        elements.coordinateAnalysis.innerHTML = emptyState("Selecione uma coordenada para iniciar a analise.");
        return;
    }

    const summary = climate.summary;
    const vegetation = climate.vegetation;
    const score = Math.max(0, Math.min(100, vegetation.scorePercent));
    const trend = getClimateTrend(climate.daily || []);
    const nearbyEvents = getNearbyEvents(climate.latitude, climate.longitude, 3);
    const riskItems = buildRiskItems(summary, vegetation, nearbyEvents);

    elements.coordinateAnalysis.innerHTML = `
        <div class="analysis-summary">
            <div class="analysis-score">
                <div class="score-ring" style="--score:${score}%; --score-color:${escapeHtml(vegetation.color)}">
                    <span>${formatNumber(vegetation.ndvi, 2)}</span>
                </div>
                <div>
                    <h3>${escapeHtml(vegetation.classification)}</h3>
                    <p>${escapeHtml(vegetation.description)}</p>
                </div>
            </div>

            <div class="analysis-section">
                <div class="analysis-heading">
                    <strong>Resumo da API NASA</strong>
                    <span>${formatDate(climate.startDate)} - ${formatDate(climate.endDate)}</span>
                </div>
                <div class="analysis-grid">
                    <div class="mini-stat"><span>Coordenada</span><strong>${formatNumber(climate.latitude, 4)}, ${formatNumber(climate.longitude, 4)}</strong></div>
                    <div class="mini-stat"><span>Cultura</span><strong>${escapeHtml(translateCrop(climate.cropName))}</strong></div>
                    <div class="mini-stat"><span>Temperatura media</span><strong>${formatNumber(summary.averageTemperatureCelsius, 1)} C</strong></div>
                    <div class="mini-stat"><span>Maxima / Minima</span><strong>${formatNumber(summary.maximumTemperatureCelsius, 1)} / ${formatNumber(summary.minimumTemperatureCelsius, 1)} C</strong></div>
                    <div class="mini-stat"><span>Chuva acumulada</span><strong>${formatNumber(summary.totalRainfallMillimeters, 1)} mm</strong></div>
                    <div class="mini-stat"><span>Dias com chuva</span><strong>${summary.daysWithRain} de ${summary.daysWithRain + summary.daysWithoutRain}</strong></div>
                    <div class="mini-stat"><span>Umidade relativa</span><strong>${formatNumber(summary.averageRelativeHumidityPercent, 1)}%</strong></div>
                    <div class="mini-stat"><span>Agua no solo</span><strong>${formatNumber(summary.averageSoilMoisturePercent, 1)}%</strong></div>
                    <div class="mini-stat"><span>Radiacao solar</span><strong>${formatNumber(summary.averageSolarRadiationMjPerSquareMeter, 1)} MJ/m2</strong></div>
                    <div class="mini-stat"><span>Nuvens</span><strong>${formatNumber(summary.averageCloudCoveragePercent, 1)}%</strong></div>
                </div>
            </div>

            <div class="analysis-section">
                <div class="analysis-heading">
                    <strong>Tendencia recente</strong>
                    <span>Ultimos 7 dias vs. 7 dias anteriores</span>
                </div>
                <div class="analysis-grid compact">
                    <div class="mini-stat"><span>Temperatura</span><strong>${formatSigned(trend.temperatureDelta)} C</strong></div>
                    <div class="mini-stat"><span>Chuva</span><strong>${formatSigned(trend.rainfallDelta)} mm</strong></div>
                    <div class="mini-stat"><span>Solo</span><strong>${formatSigned(trend.soilDelta)} p.p.</strong></div>
                    <div class="mini-stat"><span>Leituras</span><strong>${trend.currentDays} dias</strong></div>
                </div>
            </div>

            <div class="analysis-section">
                <div class="analysis-heading">
                    <strong>Riscos interpretados</strong>
                    <span>Calculado sobre clima, NDVI e EONET</span>
                </div>
                <div class="risk-list">
                    ${riskItems.map((item) => `
                        <span class="risk-chip ${item.className}">
                            <strong>${escapeHtml(item.label)}</strong>
                            ${escapeHtml(item.value)}
                        </span>
                    `).join("")}
                </div>
            </div>

            <div class="analysis-section">
                <div class="analysis-heading">
                    <strong>Eventos NASA EONET proximos</strong>
                    <span>${nearbyEvents.length ? "Mais relevantes para a coordenada" : "Nenhum evento proximo carregado"}</span>
                </div>
                ${nearbyEvents.length ? `
                    <div class="nearby-events">
                        ${nearbyEvents.map((eventData) => `
                            <article class="event-mini">
                                <strong>${escapeHtml(eventData.categoryLabel)}</strong>
                                <span>${escapeHtml(eventData.title)}</span>
                                <em>${formatNumber(eventData.distanceKm, 0)} km | ${eventData.date ? formatDate(eventData.date) : "Sem data"}</em>
                            </article>
                        `).join("")}
                    </div>
                ` : `<p class="analysis-note">A NASA EONET nao retornou evento aberto perto desta coordenada na janela atual.</p>`}
            </div>

            <p class="analysis-recommendation">${escapeHtml(buildOperationalHint(summary, vegetation, nearbyEvents))}</p>
        </div>
    `;
}

function renderGlobalFarms() {
    if (state.globalFarms.length === 0) {
        elements.worldFarmList.innerHTML = emptyState("Carregando catalogo global da NASA...");
        return;
    }

    elements.worldFarmList.innerHTML = state.globalFarms.map((farm, index) => {
        const topAlert = [...(farm.alerts || [])].sort((a, b) => b.score - a.score)[0];
        const level = riskLevelLabels[topAlert?.level || "Low"] || riskLevelLabels.Low;

        return `
            <article class="farm-card">
                <button type="button" data-farm-index="${index}">
                    <div class="pill-row">
                        <span class="pill ${level.className}">${escapeHtml(level.label)}</span>
                        <span class="pill">${escapeHtml(translateCrop(farm.cropName))}</span>
                        <span class="pill">${escapeHtml(farm.country)}</span>
                    </div>
                    <h3>${escapeHtml(farm.name)}</h3>
                    <p>${escapeHtml(farm.region)}</p>
                    <div class="farm-metrics">
                        <span>NDVI<strong>${formatNumber(farm.estimatedNdvi, 3)}</strong></span>
                        <span>Solo<strong>${formatNumber(farm.soilMoisturePercent, 1)}%</strong></span>
                        <span>Temp max<strong>${formatNumber(farm.maximumTemperatureCelsius, 1)} C</strong></span>
                        <span>Chuva<strong>${formatNumber(farm.totalRainfallMillimeters, 1)} mm</strong></span>
                    </div>
                </button>
            </article>
        `;
    }).join("");

    elements.worldFarmList.querySelectorAll("[data-farm-index]").forEach((button) => {
        button.addEventListener("click", () => {
            const farm = state.globalFarms[Number(button.dataset.farmIndex)];
            setLocation(farm.latitude, farm.longitude, farm.cropName, true);
        });
    });
}

function renderChart() {
    const canvas = elements.climateChart;
    const climate = state.climate;
    if (!canvas || !climate?.daily?.length) {
        return;
    }

    const rect = canvas.getBoundingClientRect();
    const width = Math.max(320, rect.width || 640);
    const height = Math.max(180, rect.height || 220);
    const ratio = window.devicePixelRatio || 1;
    canvas.width = width * ratio;
    canvas.height = height * ratio;

    const ctx = canvas.getContext("2d");
    ctx.setTransform(ratio, 0, 0, ratio, 0, 0);
    ctx.clearRect(0, 0, width, height);

    const daily = climate.daily;
    const isRain = state.chartType === "rainfall";
    const values = daily.map((day) => isRain ? day.rainfallMillimeters : day.temperatureCelsius);
    const color = isRain ? "#3b82f6" : "#f97316";
    const label = isRain ? "Chuva (mm)" : "Temperatura (C)";
    const padding = { top: 18, right: 16, bottom: 28, left: 42 };
    const chartWidth = width - padding.left - padding.right;
    const chartHeight = height - padding.top - padding.bottom;
    const minValue = isRain ? 0 : Math.floor(Math.min(...values) - 2);
    const maxValue = Math.max(isRain ? 10 : minValue + 5, Math.ceil(Math.max(...values) + (isRain ? 4 : 2)));

    ctx.font = "12px Inter, Segoe UI, Arial";
    ctx.fillStyle = "#9eb0a7";
    ctx.fillText(label, padding.left, 12);

    ctx.strokeStyle = "rgba(255,255,255,0.07)";
    ctx.lineWidth = 1;
    for (let i = 0; i <= 4; i++) {
        const y = padding.top + (chartHeight / 4) * i;
        ctx.beginPath();
        ctx.moveTo(padding.left, y);
        ctx.lineTo(width - padding.right, y);
        ctx.stroke();
    }

    ctx.fillStyle = "#60736a";
    ctx.textAlign = "right";
    for (let i = 0; i <= 4; i++) {
        const value = maxValue - ((maxValue - minValue) / 4) * i;
        const y = padding.top + (chartHeight / 4) * i + 4;
        ctx.fillText(formatNumber(value, 0), padding.left - 8, y);
    }

    if (isRain) {
        const barWidth = Math.max(3, chartWidth / values.length * 0.58);
        values.forEach((value, index) => {
            const x = padding.left + (chartWidth / Math.max(1, values.length - 1)) * index - barWidth / 2;
            const barHeight = ((value - minValue) / (maxValue - minValue)) * chartHeight;
            const y = padding.top + chartHeight - barHeight;
            ctx.fillStyle = "rgba(59,130,246,0.72)";
            roundRect(ctx, x, y, barWidth, barHeight, 4);
            ctx.fill();
        });
    } else {
        ctx.beginPath();
        values.forEach((value, index) => {
            const x = padding.left + (chartWidth / Math.max(1, values.length - 1)) * index;
            const y = padding.top + chartHeight - ((value - minValue) / (maxValue - minValue)) * chartHeight;
            if (index === 0) {
                ctx.moveTo(x, y);
            } else {
                ctx.lineTo(x, y);
            }
        });
        ctx.strokeStyle = color;
        ctx.lineWidth = 3;
        ctx.stroke();
    }

    ctx.textAlign = "center";
    ctx.fillStyle = "#60736a";
    daily.forEach((day, index) => {
        if (index % Math.ceil(daily.length / 6) !== 0 && index !== daily.length - 1) {
            return;
        }
        const x = padding.left + (chartWidth / Math.max(1, daily.length - 1)) * index;
        ctx.fillText(shortDate(day.date), x, height - 8);
    });
}

function startChat() {
    if (state.chatStarted) {
        return;
    }

    state.chatStarted = true;
    addChatMessage("ai", "Ola. Sou o AgroGuard AI. Selecione uma coordenada no mapa ou use a busca para que eu interprete clima, NDVI e alertas NASA em linguagem de manejo agricola.");
}

function toggleChat() {
    state.chatOpen = !state.chatOpen;
    elements.chatPanel.classList.toggle("open", state.chatOpen);
    if (state.chatOpen) {
        elements.chatInput.focus();
    }
}

async function submitChatMessage(event) {
    event.preventDefault();
    const message = elements.chatInput.value.trim();
    if (!message) {
        return;
    }

    addChatMessage("user", message);
    elements.chatInput.value = "";
    addChatMessage("ai", "Analisando dados atuais da NASA...");

    try {
        const response = await request("/api/nasa/assistant/chat", {
            method: "POST",
            body: {
                message,
                climate: state.climate,
                events: state.events.slice(0, 10)
            }
        });

        replaceLastAiMessage(response.response);
    } catch (error) {
        replaceLastAiMessage(`Nao consegui gerar a analise agora. ${error.message}`);
    }
}

function addChatMessage(role, content) {
    const message = document.createElement("div");
    message.className = `message ${role}`;
    message.textContent = content;
    elements.chatMessages.appendChild(message);
    elements.chatMessages.scrollTop = elements.chatMessages.scrollHeight;
}

function replaceLastAiMessage(content) {
    const messages = elements.chatMessages.querySelectorAll(".message.ai");
    const last = messages[messages.length - 1];
    if (last) {
        last.textContent = content;
    } else {
        addChatMessage("ai", content);
    }
    elements.chatMessages.scrollTop = elements.chatMessages.scrollHeight;
}

async function request(path, options = {}, authenticated = true) {
    const headers = {
        Accept: "application/json",
        ...(options.headers || {})
    };

    if (options.body) {
        headers["Content-Type"] = "application/json";
    }

    if (authenticated && state.token) {
        headers.Authorization = `Bearer ${state.token}`;
    }

    const response = await fetch(path, {
        ...options,
        headers,
        body: options.body ? JSON.stringify(options.body) : undefined
    });

    const text = await response.text();
    const data = text ? safeJson(text) : null;

    if (!response.ok) {
        const message = data?.detail || data?.title || data?.message || text || `HTTP ${response.status}`;
        throw new Error(message);
    }

    return data;
}

function safeJson(text) {
    try {
        return JSON.parse(text);
    } catch {
        return null;
    }
}

function formToObject(form) {
    return Object.fromEntries(new FormData(form).entries());
}

function parseCoordinates(value) {
    const parts = String(value || "")
        .trim()
        .split(/[,\s]+/)
        .map(Number)
        .filter((valuePart) => Number.isFinite(valuePart));

    if (parts.length < 2 || parts[0] < -90 || parts[0] > 90 || parts[1] < -180 || parts[1] > 180) {
        return null;
    }

    return { latitude: parts[0], longitude: parts[1] };
}

function getClimateTrend(daily) {
    const ordered = [...daily].sort((a, b) => new Date(a.date) - new Date(b.date));
    const current = ordered.slice(-7);
    const previous = ordered.slice(-14, -7);

    return {
        currentDays: current.length,
        temperatureDelta: average(current.map((day) => day.temperatureCelsius)) - average(previous.map((day) => day.temperatureCelsius)),
        rainfallDelta: sum(current.map((day) => day.rainfallMillimeters)) - sum(previous.map((day) => day.rainfallMillimeters)),
        soilDelta: average(current.map((day) => day.soilMoisturePercent)) - average(previous.map((day) => day.soilMoisturePercent))
    };
}

function buildRiskItems(summary, vegetation, nearbyEvents) {
    return [
        {
            label: "Seca",
            value: summary.averageSoilMoisturePercent < 35 || summary.totalRainfallMillimeters < 10 ? "Atencao" : "Controlado",
            className: summary.averageSoilMoisturePercent < 35 || summary.totalRainfallMillimeters < 10 ? "warning" : "good"
        },
        {
            label: "Enchente",
            value: summary.totalRainfallMillimeters > 120 ? "Alto" : "Baixo",
            className: summary.totalRainfallMillimeters > 120 ? "danger" : "good"
        },
        {
            label: "Calor",
            value: summary.maximumTemperatureCelsius >= 37 ? "Critico" : summary.maximumTemperatureCelsius >= 34 ? "Atencao" : "Normal",
            className: summary.maximumTemperatureCelsius >= 37 ? "danger" : summary.maximumTemperatureCelsius >= 34 ? "warning" : "good"
        },
        {
            label: "Vegetacao",
            value: vegetation.ndvi < 0.55 ? "Baixo vigor" : "Saudavel",
            className: vegetation.ndvi < 0.55 ? "warning" : "good"
        },
        {
            label: "EONET",
            value: nearbyEvents.length ? `${nearbyEvents.length} evento(s)` : "Sem evento proximo",
            className: nearbyEvents.length ? "warning" : "good"
        }
    ];
}

function getNearbyEvents(latitude, longitude, limit) {
    return state.events
        .filter((eventData) => eventData.latitude !== null && eventData.longitude !== null)
        .map((eventData) => ({
            ...eventData,
            distanceKm: distanceInKm(latitude, longitude, eventData.latitude, eventData.longitude)
        }))
        .filter((eventData) => eventData.distanceKm <= 1800)
        .sort((a, b) => a.distanceKm - b.distanceKm)
        .slice(0, limit);
}

function buildOperationalHint(summary, vegetation, nearbyEvents = []) {
    if (nearbyEvents.some((eventData) => eventData.categoryId === "floods") || summary.totalRainfallMillimeters > 120) {
        return "Prioridade: verificar drenagem, compactacao e risco de operacao em solo saturado.";
    }

    if (nearbyEvents.some((eventData) => eventData.categoryId === "wildfires") || summary.maximumTemperatureCelsius >= 37) {
        return "Prioridade: evitar operacoes no periodo mais quente e monitorar risco de incendio.";
    }

    if (summary.averageSoilMoisturePercent < 30 || summary.totalRainfallMillimeters < 8) {
        return "Prioridade: revisar irrigacao e sinais de estresse hidrico antes de aplicar novos insumos.";
    }

    if (vegetation.ndvi < 0.55) {
        return "Prioridade: validar em campo possiveis falhas de plantio, pragas ou baixa fertilidade.";
    }

    return "Condicao operacional favoravel. Use esta leitura como base para comparar a proxima passagem NASA.";
}

function translateCrop(cropName) {
    return cropLabels[cropName] || cropName || "Cultura";
}

function getEventColor(categoryId) {
    return {
        drought: "#eab308",
        wildfires: "#f97316",
        floods: "#3b82f6",
        severeStorms: "#a78bfa"
    }[categoryId] || "#ef4444";
}

function average(values) {
    const validValues = values.filter((value) => Number.isFinite(Number(value)));
    if (validValues.length === 0) {
        return 0;
    }

    return validValues.reduce((total, value) => total + Number(value), 0) / validValues.length;
}

function sum(values) {
    return values
        .filter((value) => Number.isFinite(Number(value)))
        .reduce((total, value) => total + Number(value), 0);
}

function formatSigned(value) {
    const rounded = Math.round(Number(value || 0) * 10) / 10;
    return `${rounded > 0 ? "+" : ""}${formatNumber(rounded, 1)}`;
}

function distanceInKm(latitudeA, longitudeA, latitudeB, longitudeB) {
    const radiusKm = 6371;
    const deltaLatitude = toRadians(latitudeB - latitudeA);
    const deltaLongitude = toRadians(longitudeB - longitudeA);
    const latA = toRadians(latitudeA);
    const latB = toRadians(latitudeB);

    const a = Math.sin(deltaLatitude / 2) ** 2 +
        Math.cos(latA) * Math.cos(latB) * Math.sin(deltaLongitude / 2) ** 2;
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));

    return radiusKm * c;
}

function toRadians(value) {
    return Number(value) * Math.PI / 180;
}

function roundCoordinate(value) {
    return Math.round(Number(value) * 10000) / 10000;
}

function formatNumber(value, digits = 0) {
    return Number(value || 0).toLocaleString("pt-BR", {
        minimumFractionDigits: digits,
        maximumFractionDigits: digits
    });
}

function formatDate(value) {
    if (!value) {
        return "-";
    }
    return new Date(value).toLocaleDateString("pt-BR", { timeZone: "UTC" });
}

function shortDate(value) {
    const date = new Date(value);
    return date.toLocaleDateString("pt-BR", { day: "2-digit", month: "2-digit", timeZone: "UTC" });
}

function emptyState(message) {
    return `<div class="empty-state">${escapeHtml(message)}</div>`;
}

function showLoading(isVisible) {
    elements.loadingOverlay.classList.toggle("hidden", !isVisible);
}

function showToast(message, type = "") {
    elements.toast.textContent = message;
    elements.toast.className = `toast visible ${type}`;
    window.clearTimeout(showToast.timeout);
    showToast.timeout = window.setTimeout(() => {
        elements.toast.classList.remove("visible");
    }, 5200);
}

function escapeHtml(value) {
    return String(value ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

function roundRect(ctx, x, y, width, height, radius) {
    const safeRadius = Math.min(radius, Math.abs(height) / 2, width / 2);
    ctx.beginPath();
    ctx.moveTo(x + safeRadius, y);
    ctx.lineTo(x + width - safeRadius, y);
    ctx.quadraticCurveTo(x + width, y, x + width, y + safeRadius);
    ctx.lineTo(x + width, y + height - safeRadius);
    ctx.quadraticCurveTo(x + width, y + height, x + width - safeRadius, y + height);
    ctx.lineTo(x + safeRadius, y + height);
    ctx.quadraticCurveTo(x, y + height, x, y + height - safeRadius);
    ctx.lineTo(x, y + safeRadius);
    ctx.quadraticCurveTo(x, y, x + safeRadius, y);
}
