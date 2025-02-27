// <copyright file="KafkaTraceExporter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Beacon
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using Confluent.Kafka;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using OpenTelemetry;
    using OpenTelemetry.Resources;

    public class KafkaTraceExporter: BaseExporter<Activity>
    {
        private readonly IProducer<Confluent.Kafka.Null, string> _producer;
        private readonly string _topic;

        public KafkaTraceExporter(ProducerConfig config, string topic)
        {
            this._producer = new ProducerBuilder<Confluent.Kafka.Null, string>(config).Build();
            this._topic = topic;
        }

        public override ExportResult Export(in Batch<Activity> batch)
        {
            using (var scope = SuppressInstrumentationScope.Begin())
            {
                foreach (Activity activity in batch)
                {
                    string traceData = JsonConvert.SerializeObject(activity);

                    Console.WriteLine($"data: {traceData}");

                    // Produce the trace data to the Kafka topic
                    try
                    {
                        Message<Null, string> msg = new Message<Null, string> { Value = traceData };

                        msg.Headers = new Headers();
                        msg.Headers.Add("content-type", Encoding.UTF8.GetBytes("application/json"));

                        this._producer.Produce(this._topic, msg);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to produce message: {ex.Message}");
                        return ExportResult.Failure;
                    }
                }

                return ExportResult.Success;
            }
        }

        protected override bool OnShutdown(int timeoutMilliseconds)
        {
            return base.OnShutdown(timeoutMilliseconds);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
