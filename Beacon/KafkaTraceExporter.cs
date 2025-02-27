// <copyright file="KafkaTraceExporter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Beacon
{
    using System;
    using System.Diagnostics;
    using Confluent.Kafka;
    using Newtonsoft.Json;
    using OpenTelemetry;

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
            foreach (Activity activity in batch)
            {
                string traceData = JsonConvert.SerializeObject(new
                {
                    Id = activity.Id,
                    TraceId = activity.TraceId,
                    SpanId = activity.SpanId,
                    ParentSpanId = activity.ParentSpanId,
                    Name = activity.DisplayName,
                    StartTime = activity.StartTimeUtc,
                    Status = activity.Status,
                    Tags = activity.Tags,
                    Events = activity.Events,
                    Links = activity.Links,
                    //Kind = activity.Kind,
                });

                // Produce the trace data to the Kafka topic
                try
                {
                    this._producer.Produce(this._topic, new Message<Null, string> { Value = traceData });
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
}
