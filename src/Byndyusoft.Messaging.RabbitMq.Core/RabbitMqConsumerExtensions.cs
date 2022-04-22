using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Byndyusoft.Messaging.RabbitMq.Topology;
using Byndyusoft.Messaging.RabbitMq.Utils;

namespace Byndyusoft.Messaging.RabbitMq
{
    public static class RabbitMqConsumerExtensions
    {
        public static IRabbitMqConsumer Start(this IRabbitMqConsumer consumer)
        {
            Preconditions.CheckNotNull(consumer, nameof(consumer));

            return consumer.StartAsync().GetAwaiter().GetResult();
        }

        public static IRabbitMqConsumer Stop(this IRabbitMqConsumer consumer)
        {
            Preconditions.CheckNotNull(consumer, nameof(consumer));

            return consumer.StopAsync().GetAwaiter().GetResult();
        }

        public static IRabbitMqConsumer WithQueueBinding(this IRabbitMqConsumer consumer,
            string exchangeName,
            string routingKey)
        {
            Preconditions.CheckNotNull(consumer, nameof(consumer));
            Preconditions.CheckNotNull(exchangeName, nameof(exchangeName));
            Preconditions.CheckNotNull(routingKey, nameof(routingKey));

            consumer.OnStarting += async (_, cancellationToken) =>
            {
                await consumer.Client.BindQueueAsync(exchangeName, routingKey, consumer.QueueName, cancellationToken)
                    .ConfigureAwait(false);
            };

            return consumer;
        }

        public static IRabbitMqConsumer WithPrefetchCount(this IRabbitMqConsumer consumer,
            ushort prefetchCount)
        {
            Preconditions.CheckNotNull(consumer, nameof(consumer));

            consumer.PrefetchCount = prefetchCount;

            return consumer;
        }

        public static IRabbitMqConsumer WithExclusive(this IRabbitMqConsumer consumer,
            bool exclusive)
        {
            Preconditions.CheckNotNull(consumer, nameof(consumer));

            consumer.Exclusive = exclusive;

            return consumer;
        }

        public static IRabbitMqConsumer WithDeclareQueue(this IRabbitMqConsumer consumer, string queueName,
            Action<QueueOptions> optionsSetup)
        {
            Preconditions.CheckNotNull(consumer, nameof(consumer));
            Preconditions.CheckNotNull(optionsSetup, nameof(optionsSetup));

            var options = QueueOptions.Default;
            optionsSetup(options);

            return consumer.WithDeclareQueue(queueName, options);
        }

        public static IRabbitMqConsumer WithDeclareQueue(this IRabbitMqConsumer consumer, string queueName,
            QueueOptions options)
        {
            Preconditions.CheckNotNull(consumer, nameof(consumer));
            Preconditions.CheckNotNull(options, nameof(options));

            async Task EventHandler(IRabbitMqConsumer _, CancellationToken cancellationToken)
            {
                await consumer.Client.CreateQueueIfNotExistsAsync(consumer.QueueName, options, cancellationToken)
                    .ConfigureAwait(false);
            }

            //consumer.OnStarting += EventHandler;

            // queue declaring event handler should be first
            var field = consumer.GetType().GetTypeInfo().GetDeclaredField(nameof(IRabbitMqConsumer.OnStarting));
            var current = (Delegate) field.GetValue(consumer);
            var dlg = Delegate.Combine(new BeforeRabbitQueueConsumerStartEventHandler(EventHandler), current);
            field.SetValue(consumer, dlg);

            return consumer;
        }

        public static IRabbitMqConsumer WithDeclareSubscribingQueue(this IRabbitMqConsumer consumer,
            Action<QueueOptions> optionsSetup)
        {
            Preconditions.CheckNotNull(consumer, nameof(consumer));
            Preconditions.CheckNotNull(optionsSetup, nameof(optionsSetup));

            var options = QueueOptions.Default;
            optionsSetup(options);

            return consumer.WithDeclareSubscribingQueue(options);
        }

        public static IRabbitMqConsumer WithDeclareSubscribingQueue(this IRabbitMqConsumer consumer,
            QueueOptions options)
        {
            Preconditions.CheckNotNull(consumer, nameof(consumer));
            Preconditions.CheckNotNull(options, nameof(options));

            return consumer.WithDeclareQueue(consumer.QueueName, options);
        }

        public static IRabbitMqConsumer WithDeclareErrorQueue(this IRabbitMqConsumer consumer, QueueOptions options)
        {
            Preconditions.CheckNotNull(consumer, nameof(consumer));
            Preconditions.CheckNotNull(options, nameof(options));

            var errorQueueName = consumer.Client.Options.NamingConventions.ErrorQueueName(consumer.QueueName);
            return consumer.WithDeclareQueue(errorQueueName, options);
        }

        public static IRabbitMqConsumer WithDeclareErrorQueue(this IRabbitMqConsumer consumer,
            Action<QueueOptions> optionsSetup)
        {
            Preconditions.CheckNotNull(consumer, nameof(consumer));

            var options = QueueOptions.Default;
            optionsSetup(options);

            return WithDeclareErrorQueue(consumer, options);
        }
    }
}