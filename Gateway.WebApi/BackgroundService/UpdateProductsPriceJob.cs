using Quartz;
using Services.ParseService;

namespace Gateway.WebApi.BackgroundService;

public class UpdateProductsPriceJob : IJob
{
    private readonly ILogger<UpdateProductsPriceJob> _logger;
    private readonly Parse _parseService;

    public UpdateProductsPriceJob(ILogger<UpdateProductsPriceJob> logger, Parse parseService)
    {
        _logger = logger;
        _parseService = parseService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation(
                "Начало выполнения UpdateProductsPriceJob в {time}",
                DateTime.Now
            );
            await _parseService.UpdateProductsPrice();
            _logger.LogInformation("UpdateProductsPriceJob завершен");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в UpdateProductsPriceJob");
        }
    }
}
