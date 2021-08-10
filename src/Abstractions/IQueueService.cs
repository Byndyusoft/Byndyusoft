﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;

namespace Byndyusoft.Net.RabbitMq.Abstractions
{
    /// <summary>
    ///     RabbitMq messaging service
    /// </summary>
    public interface IQueueService : IDisposable
    {
        /// <summary>
        ///     Initializes topology of queues
        /// </summary>
        Task Initialize(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Publishes new outgoing message into bus
        /// </summary>
        /// <typeparam name="TMessage">Outgoing message type</typeparam>
        /// <param name="message">Message itself</param>
        /// <param name="key">Message unique key</param>
        /// <param name="headers">Additional message headers</param>
        /// <param name="returnedHandled">Returned messages handler</param>
        /// <param name="cancellationToken">Token for cancelling operation</param>
        Task Publish<TMessage>(
            TMessage message,
            string key,
            Dictionary<string, string>? headers = null,
            Action<MessageReturnedEventArgs>? returnedHandled = null,
            CancellationToken cancellationToken = default) where TMessage : class;

        /// <summary>
        ///     Subscribes on incoming messages from bus
        /// </summary>
        /// <typeparam name="TMessage">Incoming message type</typeparam>
        /// <param name="processMessage">Incoming message handler</param>
        /// <param name="cancellationToken">Token for cancelling operation</param>
        public void SubscribeAsync<TMessage>(Func<TMessage, Task> processMessage, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Resends messages from all errors queues
        /// </summary>
        /// <param name="cancellationToken">Token for cancelling operation</param>
        Task ResendErrorMessages(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Resends messages from particular error queue
        /// </summary>
        /// <param name="routingKey">Routing key of incoming queue</param>
        /// <param name="cancellationToken">Token for cancelling operation</param>
        Task ResendErrorMessages(string routingKey, CancellationToken cancellationToken = default);
    }
}
