using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Byndyusoft.Messaging.RabbitMq;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Byndyusoft.Net.RabbitMq.HostedServices
{
    public class SubscribeExchangeExample : BackgroundService
    {
        private readonly IRabbitMqClient _rabbitMqClient;

        public SubscribeExchangeExample(IRabbitMqClient rabbitMqClient)
        {
            _rabbitMqClient = rabbitMqClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _rabbitMqClient.CreateExchangeIfNotExistsAsync("exchange", ex => ex.AsAutoDelete(true),
                stoppingToken);

            using var consumer = _rabbitMqClient.Subscribe("exchange", "routingKey",
                    async (queueMessage, cancellationToken) =>
                    {
                        var model = await queueMessage.Content.ReadAsAsync<Message>(cancellationToken);
                        Console.WriteLine(JsonConvert.SerializeObject(model));
                        return ConsumeResult.Ack;
                    })
                .WithPrefetchCount(20)
                .WithDeclareSubscribingQueue(option => option.AsAutoDelete(true))
                .WithDeclareErrorQueue(option => option.AsAutoDelete(true))
                .Start();

            await Task.Run(async () =>
            {
                var rand = new Random();
                while (stoppingToken.IsCancellationRequested == false)
                {
                    var message = new Message {Property = "exchange-example"};
                    await _rabbitMqClient.PublishAsJsonAsync("exchange", "routingKey", message, stoppingToken);

                    await Task.Delay(TimeSpan.FromSeconds(rand.NextDouble()), stoppingToken);
                }
            }, stoppingToken);
        }
    }
}