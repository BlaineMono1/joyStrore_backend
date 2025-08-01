using Quartz;
using Services.ParseService;

namespace Gateway.WebApi.BackgroundService;

public class ParseGamesJob : IJob
{
    private readonly ILogger<ParseGamesJob> _logger;
    private readonly Parse _parseService;

    public ParseGamesJob(ILogger<ParseGamesJob> logger, Parse parseService)
    {
        _logger = logger;
        _parseService = parseService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("Начало выполнения ParseGamesJob в {time}", DateTime.Now);
            await _parseService.ParseGames(1, 1); // Укажите нужные параметры
            _logger.LogInformation("ParseGamesJob завершен");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в ParseGamesJob");
        }
    }
}
