﻿// The MIT License (MIT)
//
// Copyright (c) 2015 Rasmus Mikkelsen
// https://github.com/rasmus/EventFlow
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Commands;
using EventFlow.Configuration;
using EventFlow.Core;
using EventFlow.Core.RetryStrategies;
using EventFlow.EventStores;
using EventFlow.Exceptions;
using EventFlow.Extensions;
using EventFlow.Logs;
using EventFlow.Subscribers;

namespace EventFlow
{
    public class CommandBus : ICommandBus
    {
        private readonly ILog _log;
        private readonly IResolver _resolver;
        private readonly IEventStore _eventStore;
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly ITransientFaultHandler<IOptimisticConcurrencyRetryStrategy> _transientFaultHandler;

        public CommandBus(
            ILog log,
            IResolver resolver,
            IEventStore eventStore,
            IDomainEventPublisher domainEventPublisher,
            ITransientFaultHandler<IOptimisticConcurrencyRetryStrategy> transientFaultHandler)
        {
            _log = log;
            _resolver = resolver;
            _eventStore = eventStore;
            _domainEventPublisher = domainEventPublisher;
            _transientFaultHandler = transientFaultHandler;
        }

        public async Task<ICommandId> PublishAsync<TAggregate, TIdentity>(
            ICommand<TAggregate, TIdentity> command,
            CancellationToken cancellationToken)
            where TAggregate : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var commandTypeName = command.GetType().PrettyPrint();
            var aggregateType = typeof (TAggregate);
            _log.Verbose(
                "Executing command '{0}' with ID '{1}' on aggregate '{2}'",
                commandTypeName,
                command.CommandId,
                aggregateType);

            IReadOnlyCollection<IDomainEvent> domainEvents;
            try
            {
                domainEvents = await ExecuteCommandAsync(command, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _log.Debug(
                    exception,
                    "Excution of command '{0}' with ID '{1}' on aggregate '{2}' failed due to exception '{3}' with message: {4}",
                    commandTypeName,
                    command.CommandId,
                    aggregateType,
                    exception.GetType().PrettyPrint(),
                    exception.Message);
                throw;
            }

            if (!domainEvents.Any())
            {
                _log.Verbose(
                    "Execution command '{0}' with ID '{1}' on aggregate '{2}' did NOT result in any domain events",
                    commandTypeName,
                    command.CommandId,
                    aggregateType);
                return command.CommandId;
            }

            _log.Verbose(() => string.Format(
                "Execution command '{0}' with ID '{1}' on aggregate '{2}' resulted in these events: {3}",
                commandTypeName,
                command.CommandId,
                aggregateType,
                string.Join(", ", domainEvents.Select(d => d.EventType.PrettyPrint()))));

            await _domainEventPublisher.PublishAsync<TAggregate, TIdentity>(
                command.AggregateId,
                domainEvents,
                cancellationToken)
                .ConfigureAwait(false);

            return command.CommandId;
        }

        public ICommandId Publish<TAggregate, TIdentity>(
            ICommand<TAggregate, TIdentity> command,
			CancellationToken cancellationToken)
            where TAggregate : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity
        {
            ICommandId commandId = null;

            using (var a = AsyncHelper.Wait)
            {
                a.Run(PublishAsync(command, cancellationToken), id => commandId = id);
            }

            return commandId;
        }

        private Task<IReadOnlyCollection<IDomainEvent>> ExecuteCommandAsync<TAggregate, TIdentity>(
            ICommand<TAggregate, TIdentity> command,
            CancellationToken cancellationToken)
            where TAggregate : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity
        {
            var aggregateType = typeof (TAggregate);
            var identityType = typeof (TIdentity);
            var commandType = command.GetType();
            var commandHandlerType = typeof (ICommandHandler<,,>).MakeGenericType(aggregateType, identityType, commandType);

            var commandHandlers = _resolver.ResolveAll(commandHandlerType).ToList();
            if (!commandHandlers.Any())
            {
                throw new NoCommandHandlersException(string.Format(
                    "No command handlers registered for the command '{0}' on aggregate '{1}'",
                    commandType.PrettyPrint(),
                    aggregateType.PrettyPrint()));
            }
            if (commandHandlers.Count > 1)
            {
                throw new InvalidOperationException(string.Format(
                    "Too many command handlers the command '{0}' on aggregate '{1}'. These were found: {2}",
                    commandType.PrettyPrint(),
                    aggregateType.PrettyPrint(),
                    string.Join(", ", commandHandlers.Select(h => h.GetType().PrettyPrint()))));
            }

            var commandHandler = commandHandlers.Single();
            var commandInvoker = commandHandlerType.GetMethod("ExecuteAsync", new []{ aggregateType, commandType, typeof(CancellationToken) });

            return _transientFaultHandler.TryAsync(
                async c =>
                    {
                        var aggregate = await _eventStore.LoadAggregateAsync<TAggregate, TIdentity>(command.AggregateId, c).ConfigureAwait(false);

                        var invokeTask = (Task) commandInvoker.Invoke(commandHandler, new object[] {aggregate, command, c});
                        await invokeTask.ConfigureAwait(false);

                        return await aggregate.CommitAsync(_eventStore, command.CommandId, c).ConfigureAwait(false);
                    },
                Label.Named($"command-execution-{commandType.Name.ToLowerInvariant()}"), 
                cancellationToken);
        }
    }
}
