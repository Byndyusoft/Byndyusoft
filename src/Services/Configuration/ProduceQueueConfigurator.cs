﻿using System;
using Byndyusoft.Net.RabbitMq.Abstractions;
using Byndyusoft.Net.RabbitMq.Models;

namespace Byndyusoft.Net.RabbitMq.Services.Configuration
{
    /// <inheritdoc cref="IProduceMiddlewareConfigurator&lt;TMessage&gt;"/>
    internal sealed class ProduceQueueConfigurator<TMessage> : IProduceMiddlewareConfigurator<TMessage> where TMessage : class
    {
        /// <summary>
        ///     Configuration of queue
        /// </summary>
        private readonly QueueConfiguration _queue;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="queue">Configuration of queue</param>
        public ProduceQueueConfigurator(QueueConfiguration queue)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        /// <inheritdoc />
        public IProduceMiddlewareConfigurator<TMessage> Wrap<TWrapper>() where TWrapper : IProduceMiddleware<TMessage>
        {
            _queue.Middlewares.Add(typeof(TWrapper));
            return this;
        }

        /// <inheritdoc />
        public IProduceReturnedPipeConfigurator<TMessage> PipeReturned<TReturnedPipe>() where TReturnedPipe : IReturnedPipe<TMessage>
        {
            _queue.Pipes.Add(typeof(TReturnedPipe));
            return this;
        }
    }
}