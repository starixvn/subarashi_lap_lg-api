using System;
using System.Collections.Generic;
using CitiXchangeLibrary;

namespace Fanex.Athena.LGAPI.Core.Constants
{
    public static class ExchangeEnvironment
    {
        public static IDictionary<string, Enviro> EnvironmentMap { get; } = new Dictionary<string, Enviro>(StringComparer.OrdinalIgnoreCase)
        {
            { "PRO", Enviro.PRD },
            { "UAT", Enviro.PRD },
            { "SIT", Enviro.SIT },
            { "LOCAL", Enviro.POC },
            { "DEV", Enviro.POC },
            { "BACKUPMNL", Enviro.PRD}
        };
    }
}