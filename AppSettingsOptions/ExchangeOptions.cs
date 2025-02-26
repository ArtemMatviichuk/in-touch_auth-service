﻿using Microsoft.Extensions.Options;

namespace AuthService.AppSettingsOptions
{
    public class ExchangeOptions
    {
        public string Exchange { get; set; } = string.Empty;
        public string RoutingKey { get; set; } = string.Empty;
        public string QueueName { get; set; } = string.Empty;

        public bool IsFanout => string.IsNullOrWhiteSpace(QueueName);
    }
}
