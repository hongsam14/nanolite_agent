using System;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

namespace Nanolite_agent.Beacon
{
    public class Beacon
    {
        public readonly string BeaconName = "nanolite_beacon";
        public Beacon()
        {
            var tracerProvider = Sdk.CreateTracerProviderBuilder().AddSource(this.BeaconName)
                .AddConsoleExporter()
                .AddOtlpExporter(
                options =>
                    {
                        options.Endpoint = new Uri("http://localhost:4317");
                    }
                )
                .Build();
        }
    }
}
