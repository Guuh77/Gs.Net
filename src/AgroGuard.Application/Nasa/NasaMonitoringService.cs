using AgroGuard.Application.Abstractions;
using AgroGuard.Application.Alerts;
using AgroGuard.Application.Common.Exceptions;
using AgroGuard.Application.Common.Security;
using AgroGuard.Application.Readings;
using AgroGuard.Domain.Entities;
using AgroGuard.Domain.Services;

namespace AgroGuard.Application.Nasa;

public sealed class NasaMonitoringService : INasaMonitoringService
{
    private static readonly GlobalFarmProfile[] GlobalFarmProfiles =
    [
        new("BR-MT-SOY", "Fazenda Primavera do Cerrado", "Brasil", "Sorriso, Mato Grosso", "Soybean", "Glycine max", 0.82m, 0.78m, -12.544m, -55.721m, 2800m, "Latossolo"),
        new("BR-PR-CORN", "Cooperativa Campos Gerais", "Brasil", "Ponta Grossa, Parana", "Corn", "Zea mays", 0.80m, 0.82m, -25.095m, -50.161m, 1420m, "Nitossolo"),
        new("BR-BA-COTTON", "Oeste Baiano Cotton Block", "Brasil", "Luis Eduardo Magalhaes, Bahia", "Cotton", "Gossypium hirsutum", 0.72m, 0.76m, -12.095m, -45.786m, 3100m, "Latossolo"),
        new("AR-BA-SOY", "Pampas Soybean Estate", "Argentina", "Pergamino, Buenos Aires", "Soybean", "Glycine max", 0.82m, 0.76m, -33.890m, -60.573m, 2450m, "Mollisol"),
        new("CL-ML-VINE", "Maule Valley Vineyard", "Chile", "Talca, Maule", "Grapevine", "Vitis vinifera", 0.68m, 0.54m, -35.426m, -71.665m, 520m, "Aluvial"),
        new("CO-HU-COFFEE", "Huila Mountain Coffee Farm", "Colombia", "Pitalito, Huila", "Coffee", "Coffea arabica", 0.74m, 0.66m, 1.853m, -76.051m, 410m, "Vulcanico"),
        new("MX-SI-MAIZE", "Sinaloa Maize Irrigation District", "Mexico", "Culiacan, Sinaloa", "Corn", "Zea mays", 0.80m, 0.84m, 24.809m, -107.394m, 1880m, "Aluvial"),
        new("US-IA-CORN", "Heartland Corn Research Farm", "Estados Unidos", "Ames, Iowa", "Corn", "Zea mays", 0.80m, 0.84m, 42.030m, -93.630m, 1650m, "Mollisol"),
        new("US-CA-ALMOND", "Central Valley Almond Block", "Estados Unidos", "Fresno, California", "Almond", "Prunus dulcis", 0.70m, 0.72m, 36.737m, -119.787m, 740m, "Aluvial"),
        new("CA-SK-CANOLA", "Prairie Canola Rotation", "Canada", "Saskatoon, Saskatchewan", "Canola", "Brassica napus", 0.74m, 0.68m, 52.157m, -106.670m, 2200m, "Chernozem"),
        new("FR-CV-WHEAT", "Beauce Wheat Cooperative", "Franca", "Chartres, Centre-Val de Loire", "Wheat", "Triticum aestivum", 0.76m, 0.70m, 48.446m, 1.489m, 1320m, "Calcario"),
        new("IT-PO-RICE", "Po Valley Rice Farm", "Italia", "Vercelli, Piemonte", "Rice", "Oryza sativa", 0.78m, 0.90m, 45.323m, 8.423m, 610m, "Hidromorfico"),
        new("UA-CK-WHEAT", "Cherkasy Black Soil Wheat", "Ucrania", "Cherkasy Oblast", "Wheat", "Triticum aestivum", 0.76m, 0.72m, 49.444m, 32.059m, 3400m, "Chernozem"),
        new("TR-KO-WHEAT", "Konya Anatolian Wheat Field", "Turquia", "Konya, Anatolia", "Wheat", "Triticum aestivum", 0.76m, 0.68m, 37.874m, 32.493m, 1750m, "Calcario"),
        new("EG-NI-COTTON", "Nile Delta Cotton Farm", "Egito", "Mansoura, Nile Delta", "Cotton", "Gossypium hirsutum", 0.72m, 0.78m, 31.041m, 31.378m, 860m, "Aluvial"),
        new("MA-MK-OLIVE", "Meknes Olive Grove", "Marrocos", "Meknes, Fes-Meknes", "Olive", "Olea europaea", 0.62m, 0.45m, 33.895m, -5.554m, 450m, "Calcario"),
        new("NG-KD-MAIZE", "Kaduna Maize Belt", "Nigeria", "Kaduna State", "Corn", "Zea mays", 0.80m, 0.80m, 10.510m, 7.416m, 1180m, "Ferruginoso"),
        new("ZA-FS-MAIZE", "Free State Maize Farm", "Africa do Sul", "Bothaville, Free State", "Corn", "Zea mays", 0.80m, 0.78m, -27.388m, 26.620m, 2300m, "Vertissolo"),
        new("ET-OR-COFFEE", "Oromia Coffee Highlands", "Etiopia", "Jimma, Oromia", "Coffee", "Coffea arabica", 0.74m, 0.66m, 7.675m, 36.835m, 520m, "Vulcanico"),
        new("IN-PB-WHEAT", "Punjab Wheat Belt Field", "India", "Ludhiana, Punjab", "Wheat", "Triticum aestivum", 0.76m, 0.72m, 30.901m, 75.857m, 980m, "Aluvial"),
        new("CN-HL-SOY", "Heilongjiang Soybean Farm", "China", "Harbin, Heilongjiang", "Soybean", "Glycine max", 0.82m, 0.74m, 45.803m, 126.535m, 2650m, "Chernozem"),
        new("JP-NI-RICE", "Niigata Rice Paddy", "Japao", "Niigata Prefecture", "Rice", "Oryza sativa", 0.78m, 0.90m, 37.916m, 139.036m, 380m, "Hidromorfico"),
        new("TH-NE-RICE", "Isan Rainfed Rice Field", "Tailandia", "Khon Kaen, Isan", "Rice", "Oryza sativa", 0.78m, 0.88m, 16.432m, 102.823m, 1250m, "Hidromorfico"),
        new("ID-JV-RICE", "Java Rice Terrace Block", "Indonesia", "Subang, West Java", "Rice", "Oryza sativa", 0.78m, 0.90m, -6.570m, 107.760m, 760m, "Vulcanico"),
        new("KE-RV-COFFEE", "Rift Valley Coffee Estate", "Quenia", "Kericho, Rift Valley", "Coffee", "Coffea arabica", 0.74m, 0.66m, -0.367m, 35.283m, 620m, "Vulcanico"),
        new("VN-MK-RICE", "Mekong Delta Rice Cooperative", "Vietname", "Can Tho, Mekong Delta", "Rice", "Oryza sativa", 0.78m, 0.90m, 10.045m, 105.746m, 1450m, "Hidromorfico"),
        new("AU-QLD-WHEAT", "Darling Downs Wheat Station", "Australia", "Toowoomba, Queensland", "Wheat", "Triticum aestivum", 0.76m, 0.70m, -27.560m, 151.950m, 2100m, "Vertissolo"),
        new("NZ-CB-DAIRY", "Canterbury Pasture Platform", "Nova Zelandia", "Canterbury Plains", "Pasture", "Lolium perenne", 0.76m, 0.78m, -43.532m, 172.637m, 910m, "Aluvial"),
        new("ES-AR-VINE", "Ebro Valley Vineyard Block", "Espanha", "Zaragoza, Aragon", "Grapevine", "Vitis vinifera", 0.68m, 0.55m, 41.648m, -0.889m, 340m, "Calcario")
    ];

    private readonly INasaPowerClient _nasaPowerClient;
    private readonly INasaEventClient _nasaEventClient;
    private readonly IFieldRepository _fields;
    private readonly ISatelliteReadingRepository _readings;
    private readonly IAlertRepository _alerts;
    private readonly ICurrentUser _currentUser;
    private readonly RiskAssessmentService _riskAssessmentService;
    private readonly IUnitOfWork _unitOfWork;

    public NasaMonitoringService(
        INasaPowerClient nasaPowerClient,
        INasaEventClient nasaEventClient,
        IFieldRepository fields,
        ISatelliteReadingRepository readings,
        IAlertRepository alerts,
        ICurrentUser currentUser,
        RiskAssessmentService riskAssessmentService,
        IUnitOfWork unitOfWork)
    {
        _nasaPowerClient = nasaPowerClient;
        _nasaEventClient = nasaEventClient;
        _fields = fields;
        _readings = readings;
        _alerts = alerts;
        _currentUser = currentUser;
        _riskAssessmentService = riskAssessmentService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<GlobalFarmAnalysisResponse>> AnalyzeGlobalFarmsAsync(CancellationToken cancellationToken)
    {
        using var throttle = new SemaphoreSlim(6);
        var tasks = GlobalFarmProfiles.Select(async profile =>
        {
            await throttle.WaitAsync(cancellationToken);
            try
            {
                return await AnalyzeGlobalFarmAsync(profile, cancellationToken);
            }
            finally
            {
                throttle.Release();
            }
        });

        var analyses = await Task.WhenAll(tasks);

        return analyses
            .OrderByDescending(analysis => analysis.Alerts.Max(alert => (decimal?)alert.Score) ?? 0m)
            .ThenBy(analysis => analysis.Country)
            .ToArray();
    }

    public Task<GlobalFarmAnalysisResponse> AnalyzeCoordinatesAsync(
        CoordinateAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        ValidateCoordinates(request.Latitude, request.Longitude);

        var crop = ResolveCropProfile(request.CropName);
        var profile = new GlobalFarmProfile(
            "CUSTOM-MAP-POINT",
            "Ponto selecionado no mapa",
            "Coordenada livre",
            $"{request.Latitude:0.000}, {request.Longitude:0.000}",
            crop.CropName,
            crop.ScientificName,
            crop.IdealNdvi,
            crop.WaterDemandIndex,
            request.Latitude,
            request.Longitude,
            request.AreaHectares <= 0 ? 100m : request.AreaHectares,
            "Solo informado pelo usuario");

        return AnalyzeGlobalFarmAsync(profile, cancellationToken);
    }

    public async Task<ReadingAnalysisResponse> AnalyzeOwnedFieldWithNasaAsync(Guid fieldId, CancellationToken cancellationToken)
    {
        var field = await _fields.GetOwnedByIdAsync(fieldId, RequireUserId(), cancellationToken);
        if (field is null)
        {
            throw new NotFoundException("Field", fieldId);
        }

        var climate = await _nasaPowerClient.GetAgroClimateAsync(field.Latitude, field.Longitude, cancellationToken);
        var previousReading = await _readings.GetLatestByFieldAsync(field.Id, cancellationToken);
        var reading = BuildReadingFromClimate(field, climate);

        var generatedAlerts = _riskAssessmentService
            .Assess(field, reading, previousReading)
            .Select(assessment => assessment.ToAlert(field.Id, reading.Id))
            .ToArray();

        await _readings.AddAsync(reading, cancellationToken);
        if (generatedAlerts.Length > 0)
        {
            await _alerts.AddRangeAsync(generatedAlerts, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReadingAnalysisResponse(
            ToReadingResponse(reading),
            generatedAlerts.Select(alert => AlertService.ToResponse(alert, field.Name)).ToArray());
    }

    public async Task<NasaClimateDashboardResponse> GetClimateDashboardAsync(
        NasaClimateDashboardRequest request,
        CancellationToken cancellationToken)
    {
        ValidateCoordinates(request.Latitude, request.Longitude);

        var crop = ResolveCropProfile(request.CropName ?? "Soybean");
        var series = await _nasaPowerClient.GetClimateSeriesAsync(
            request.Latitude,
            request.Longitude,
            Math.Clamp(request.Days, 7, 60),
            cancellationToken);

        var daily = series.Daily
            .Select(day => new NasaDailyClimateResponse(
                day.Date,
                day.TemperatureCelsius,
                day.MaximumTemperatureCelsius,
                day.MinimumTemperatureCelsius,
                day.RainfallMillimeters,
                day.RelativeHumidityPercent,
                day.SolarRadiationMjPerSquareMeter,
                decimal.Round(Math.Clamp(day.RootZoneSoilWetness * 100m, 0m, 100m), 1, MidpointRounding.AwayFromZero),
                day.CloudCoveragePercent))
            .ToArray();

        var summary = new NasaClimateSummaryResponse(
            series.Summary.AverageTemperatureCelsius,
            series.Summary.MaximumTemperatureCelsius,
            daily.Length == 0 ? 0m : daily.Min(day => day.MinimumTemperatureCelsius),
            series.Summary.TotalRainfallMillimeters,
            series.Summary.AverageRelativeHumidityPercent,
            Average(daily.Select(day => day.SolarRadiationMjPerSquareMeter)),
            decimal.Round(Math.Clamp(series.Summary.RootZoneSoilWetness * 100m, 0m, 100m), 1, MidpointRounding.AwayFromZero),
            series.Summary.AverageCloudCoveragePercent,
            daily.Count(day => day.RainfallMillimeters > 0.1m),
            daily.Count(day => day.RainfallMillimeters <= 0.1m));

        var ndvi = EstimateNdvi(crop.IdealNdvi, series.Summary);

        return new NasaClimateDashboardResponse(
            request.Latitude,
            request.Longitude,
            crop.CropName,
            series.Summary.Source,
            series.Summary.StartDate,
            series.Summary.EndDate,
            summary,
            ClassifyVegetation(ndvi),
            daily);
    }

    public async Task<NasaNaturalEventsResponse> GetNaturalEventsAsync(
        int days,
        string? category,
        CancellationToken cancellationToken)
    {
        var events = await _nasaEventClient.GetOpenEventsAsync(days, category, cancellationToken);
        var responseEvents = events
            .Select(eventData => new NasaNaturalEventResponse(
                eventData.Id,
                eventData.Title,
                eventData.CategoryId,
                eventData.CategoryName,
                TranslateEventCategory(eventData.CategoryId, eventData.CategoryName),
                eventData.Date,
                eventData.Latitude,
                eventData.Longitude,
                eventData.Magnitude,
                eventData.MagnitudeUnit,
                eventData.SourceUrl))
            .ToArray();

        return new NasaNaturalEventsResponse(responseEvents.Length, responseEvents);
    }

    public NasaAssistantChatResponse GenerateAssistantReply(NasaAssistantChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw ValidationException.For(nameof(request.Message), "Message is required.");
        }

        if (request.Climate is null)
        {
            return new NasaAssistantChatResponse(
                "Selecione uma coordenada no mapa ou use a busca por latitude e longitude para eu analisar dados reais da NASA antes de recomendar acoes.",
                DateTime.UtcNow);
        }

        var summary = request.Climate.Summary;
        var vegetation = request.Climate.Vegetation;
        var events = (request.Events ?? [])
            .Where(eventData => eventData.Date is not null)
            .Take(5)
            .ToArray();

        var riskSignals = new List<string>();
        if (summary.AverageSoilMoisturePercent < 35m || summary.TotalRainfallMillimeters < 10m)
        {
            riskSignals.Add("deficit hidrico");
        }

        if (summary.MaximumTemperatureCelsius >= 35m)
        {
            riskSignals.Add("calor elevado");
        }

        if (summary.TotalRainfallMillimeters > 120m)
        {
            riskSignals.Add("excesso de chuva");
        }

        if (events.Length > 0)
        {
            riskSignals.Add("eventos naturais abertos no EONET");
        }

        var riskSummary = riskSignals.Count == 0
            ? "Nao ha sinal critico imediato nos dados recebidos."
            : $"Pontos de atencao: {string.Join(", ", riskSignals)}.";

        var action = BuildAssistantAction(summary, vegetation);
        var eventLine = events.Length == 0
            ? "Eventos NASA EONET: nenhum evento aberto relevante foi carregado para o contexto atual."
            : $"Eventos NASA EONET: {string.Join("; ", events.Select(eventData => $"{eventData.CategoryLabel} - {eventData.Title}"))}.";

        var response =
            $"Analise AgroGuard para a coordenada {request.Climate.Latitude:0.000}, {request.Climate.Longitude:0.000}.\n\n" +
            $"Situacao da lavoura: NDVI estimado {vegetation.Ndvi:0.000} ({vegetation.Classification}). {vegetation.Description}\n\n" +
            $"Clima recente: temperatura media {summary.AverageTemperatureCelsius:0.0} C, maxima {summary.MaximumTemperatureCelsius:0.0} C, chuva acumulada {summary.TotalRainfallMillimeters:0.0} mm e umidade do solo {summary.AverageSoilMoisturePercent:0.0}%.\n\n" +
            $"{riskSummary} {eventLine}\n\n" +
            $"Acao recomendada: {action}";

        return new NasaAssistantChatResponse(response, DateTime.UtcNow);
    }

    private async Task<GlobalFarmAnalysisResponse> AnalyzeGlobalFarmAsync(
        GlobalFarmProfile profile,
        CancellationToken cancellationToken)
    {
        var crop = new Crop(profile.CropName, profile.ScientificName, profile.IdealNdvi, profile.WaterDemandIndex);
        var field = new Field(
            Guid.NewGuid(),
            crop,
            profile.Name,
            profile.AreaHectares,
            profile.Latitude,
            profile.Longitude,
            profile.SoilType,
            DateTime.UtcNow.AddMonths(-3),
            DateTime.UtcNow.AddMonths(2));

        var climate = await _nasaPowerClient.GetAgroClimateAsync(profile.Latitude, profile.Longitude, cancellationToken);
        var reading = BuildReadingFromClimate(field, climate);
        var alerts = _riskAssessmentService
            .Assess(field, reading)
            .Select(assessment => AlertService.ToResponse(assessment.ToAlert(field.Id, reading.Id), profile.Name))
            .ToArray();

        return new GlobalFarmAnalysisResponse(
            profile.Code,
            profile.Name,
            profile.Country,
            profile.Region,
            profile.CropName,
            profile.Latitude,
            profile.Longitude,
            profile.AreaHectares,
            climate.Source,
            climate.StartDate,
            climate.EndDate,
            reading.Ndvi,
            reading.SoilMoisturePercent,
            reading.SurfaceTemperatureCelsius,
            reading.RainfallMillimeters,
            climate.AverageRelativeHumidityPercent,
            reading.CloudCoveragePercent,
            alerts);
    }

    private static SatelliteReading BuildReadingFromClimate(Field field, NasaPowerClimateSummary climate)
    {
        var ndvi = EstimateNdvi(field.Crop.IdealNdvi, climate);
        var soilMoisturePercent = Math.Clamp(climate.RootZoneSoilWetness * 100m, 0m, 100m);

        return new SatelliteReading(
            field.Id,
            climate.EndDate,
            "NASA POWER agroclimatology + estimated NDVI",
            ndvi,
            soilMoisturePercent,
            climate.MaximumTemperatureCelsius,
            climate.TotalRainfallMillimeters,
            climate.AverageCloudCoveragePercent);
    }

    private static decimal EstimateNdvi(decimal idealNdvi, NasaPowerClimateSummary climate)
    {
        var soilPenalty = Math.Max(0m, 0.55m - climate.RootZoneSoilWetness) * 0.42m;
        var heatPenalty = Math.Max(0m, climate.MaximumTemperatureCelsius - 34m) * 0.012m;
        var droughtPenalty = climate.TotalRainfallMillimeters < 8m ? 0.06m : 0m;
        var humidityBonus = Math.Min(climate.AverageRelativeHumidityPercent, 85m) / 1000m;
        var rainBonus = Math.Min(climate.TotalRainfallMillimeters, 45m) / 900m;

        var estimated = idealNdvi - soilPenalty - heatPenalty - droughtPenalty + humidityBonus + rainBonus;
        return Math.Clamp(decimal.Round(estimated, 3, MidpointRounding.AwayFromZero), 0.15m, 0.95m);
    }

    private static NasaVegetationIndexResponse ClassifyVegetation(decimal ndvi)
    {
        var score = decimal.Round(((ndvi + 1m) / 2m) * 100m, 1, MidpointRounding.AwayFromZero);

        if (ndvi >= 0.75m)
        {
            return new NasaVegetationIndexResponse(
                ndvi,
                "Vegetacao densa",
                "Cobertura vegetal forte, com boa resposta climatica recente.",
                "#10b981",
                score);
        }

        if (ndvi >= 0.55m)
        {
            return new NasaVegetationIndexResponse(
                ndvi,
                "Vegetacao saudavel",
                "Condicao boa para a cultura, mantendo acompanhamento de clima e solo.",
                "#84cc16",
                score);
        }

        if (ndvi >= 0.35m)
        {
            return new NasaVegetationIndexResponse(
                ndvi,
                "Atencao vegetativa",
                "Vigor intermediario; vale conferir agua, solo e pressao de pragas em campo.",
                "#eab308",
                score);
        }

        return new NasaVegetationIndexResponse(
            ndvi,
            "Baixo vigor",
            "Possivel estresse da lavoura ou baixa cobertura vegetal na area analisada.",
            "#f97316",
            score);
    }

    private static string TranslateEventCategory(string categoryId, string categoryName)
    {
        return categoryId switch
        {
            "drought" => "Seca",
            "wildfires" => "Incendio",
            "floods" => "Enchente",
            "severeStorms" => "Tempestade",
            "volcanoes" => "Vulcao",
            "earthquakes" => "Terremoto",
            "landslides" => "Deslizamento",
            "snow" => "Neve",
            "seaLakeIce" => "Gelo",
            "tempExtremes" => "Temperatura extrema",
            _ => string.IsNullOrWhiteSpace(categoryName) ? "Evento natural" : categoryName
        };
    }

    private static string BuildAssistantAction(
        NasaClimateSummaryResponse summary,
        NasaVegetationIndexResponse vegetation)
    {
        if (summary.AverageSoilMoisturePercent < 30m || summary.TotalRainfallMillimeters < 8m)
        {
            return "priorize verificacao de irrigacao, observe folhas murchas e acompanhe a proxima leitura antes de aplicar insumos caros.";
        }

        if (summary.TotalRainfallMillimeters > 120m)
        {
            return "verifique drenagem, erosao e acesso de maquinas; evite operacao pesada ate o solo perder saturacao.";
        }

        if (summary.MaximumTemperatureCelsius >= 37m)
        {
            return "evite operacoes no horario mais quente, monitore risco de incendio e proteja areas com palhada seca.";
        }

        if (vegetation.Ndvi < 0.55m)
        {
            return "faca vistoria no talhao para comparar NDVI com falhas de plantio, pragas, compactacao ou deficit de nutrientes.";
        }

        return "mantenha o manejo planejado, registre a leitura atual como linha de base e compare com a proxima janela NASA.";
    }

    private static decimal Average(IEnumerable<decimal> values)
    {
        var validValues = values.ToArray();
        return validValues.Length == 0
            ? 0m
            : decimal.Round(validValues.Average(), 2, MidpointRounding.AwayFromZero);
    }

    private static void ValidateCoordinates(decimal latitude, decimal longitude)
    {
        if (latitude is < -90m or > 90m)
        {
            throw ValidationException.For(nameof(latitude), "Latitude must be between -90 and 90.");
        }

        if (longitude is < -180m or > 180m)
        {
            throw ValidationException.For(nameof(longitude), "Longitude must be between -180 and 180.");
        }
    }

    private Guid RequireUserId()
    {
        return _currentUser.UserId ?? throw new ForbiddenException("Authenticated user was not found.");
    }

    private static SatelliteReadingResponse ToReadingResponse(SatelliteReading reading)
    {
        return new SatelliteReadingResponse(
            reading.Id,
            reading.FieldId,
            reading.CapturedAt,
            reading.Source,
            reading.Ndvi,
            reading.SoilMoisturePercent,
            reading.SurfaceTemperatureCelsius,
            reading.RainfallMillimeters,
            reading.CloudCoveragePercent);
    }

    private static CropDescriptor ResolveCropProfile(string cropName)
    {
        var normalized = (cropName ?? string.Empty).Trim().ToLowerInvariant();

        return normalized switch
        {
            "corn" or "milho" => new CropDescriptor("Corn", "Zea mays", 0.80m, 0.84m),
            "coffee" or "cafe" => new CropDescriptor("Coffee", "Coffea arabica", 0.74m, 0.66m),
            "rice" or "arroz" => new CropDescriptor("Rice", "Oryza sativa", 0.78m, 0.90m),
            "wheat" or "trigo" => new CropDescriptor("Wheat", "Triticum aestivum", 0.76m, 0.72m),
            "grapevine" or "uva" => new CropDescriptor("Grapevine", "Vitis vinifera", 0.68m, 0.55m),
            "sugarcane" or "cana" => new CropDescriptor("Sugarcane", "Saccharum officinarum", 0.86m, 0.88m),
            _ => new CropDescriptor("Soybean", "Glycine max", 0.82m, 0.78m)
        };
    }

    private sealed record CropDescriptor(string CropName, string ScientificName, decimal IdealNdvi, decimal WaterDemandIndex);
}
