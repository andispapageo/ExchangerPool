using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Options
{
    public sealed class ExchangeOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
        public bool Enabled { get; set; } = true;
        public EndpointOptions Endpoints { get; set; } = new();
    }

    public sealed class EndpointOptions
    {
        public string Symbols { get; set; } = string.Empty;
        public string Ticker { get; set; } = string.Empty;
        public string AllTickers { get; set; } = string.Empty;
    }
}
