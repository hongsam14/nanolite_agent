// <copyright file="TracerProviderBuilderExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Nanolite_agent.Beacon
{
    using Confluent.Kafka;
    using OpenTelemetry;
    using OpenTelemetry.Trace;
    using System;

    public static class TracerProviderBuilderExtensions
    {
        // TODO: ProvierBuilder -> Provider
        public static TracerProviderBuilder AddKafkaExporter(this TracerProviderBuilder builder, string broker, string topic)
        {
            ProducerConfig producerConfig = new ProducerConfig { BootstrapServers = broker };

            using (KafkaTraceExporter kafkaExporter = new KafkaTraceExporter(producerConfig, topic))
            {
                //using (BatchActivityExportProcessor processor = new BatchActivityExportProcessor(kafkaExporter))
                using (SimpleActivityExportProcessor processor = new SimpleActivityExportProcessor(kafkaExporter))
                {
                    return builder.AddProcessor(processor);
                }
            }
        }
    }
}
