using Application.Common.DTOs;
using Application.Common.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace ExchangerPool.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LiquidityController : ControllerBase
{
    private readonly GetBestPriceUseCase _getBestPriceUseCase;
    private readonly GetArbitrageRiskUseCase _getArbitrageUseCase;
    private readonly GetAvailableSymbolsUseCase _getSymbolsUseCase;
    private readonly ILogger<LiquidityController> _logger;

    public LiquidityController(
        GetBestPriceUseCase getBestPriceUseCase,
        GetArbitrageRiskUseCase getArbitrageUseCase,
        GetAvailableSymbolsUseCase getSymbolsUseCase,
        ILogger<LiquidityController> logger)
    {
        _getBestPriceUseCase = getBestPriceUseCase;
        _getArbitrageUseCase = getArbitrageUseCase;
        _getSymbolsUseCase = getSymbolsUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Get best aggregated price for a symbol across all exchanges
    /// </summary>
    /// <param name="symbol">Trading pair symbol (e.g., BTCUSDT, ETHUSDT)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aggregated price with best bid/ask from all exchanges</returns>
    [HttpGet("price/{symbol}")]
    [ProducesResponseType(typeof(AggregatedPriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AggregatedPriceDto>> GetBestPrice(
        string symbol,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting best price for {Symbol}", symbol?.ToUpper());
            var result = await _getBestPriceUseCase.ExecuteAsync(symbol, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Symbol not found: {Symbol}", symbol);
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all available trading symbols that exist on multiple exchanges
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of symbols available on 2+ exchanges</returns>
    [HttpGet("symbols")]
    [ProducesResponseType(typeof(IEnumerable<CryptoSymbolDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CryptoSymbolDto>>> GetSymbols(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting available symbols");
        var result = await _getSymbolsUseCase.ExecuteAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get current arbitrage opportunities across exchanges
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of symbols with arbitrage opportunities, sorted by profit percentage</returns>
    [HttpGet("arbitrage")]
    [ProducesResponseType(typeof(IEnumerable<AggregatedPriceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AggregatedPriceDto>>> GetArbitrageOpportunities(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting arbitrage opportunities");
        var result = await _getArbitrageUseCase.ExecuteAsync(cancellationToken);
        return Ok(result);
    }
}