using Domain.Core.Entities.Aggregates;
using Domain.Core.Interfaces;

namespace Application.Common.Specifications;

public class ArbitrageRiskSpecification(decimal minProfitPercent = 0.1m) : Specification<AggregatedPrice>
{
    public override bool IsSatisfiedBy(AggregatedPrice entity) => entity.HasArbitrageOpportunity &&
               entity.ArbitrageRisk >= minProfitPercent;
}

public class SymbolSpecification : Specification<AggregatedPrice>
{
    private readonly string _symbol;
    public SymbolSpecification(string symbol) => _symbol = symbol.ToUpperInvariant();
    public override bool IsSatisfiedBy(AggregatedPrice entity) => entity.Symbol.Equals(_symbol, StringComparison.OrdinalIgnoreCase);
}