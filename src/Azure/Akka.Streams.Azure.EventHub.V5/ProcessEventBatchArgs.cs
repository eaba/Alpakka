﻿//-----------------------------------------------------------------------
// <copyright file="ProcessEventBatchArgs.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Microsoft.Azure.Amqp.Framing;

namespace Akka.Streams.Azure.EventHub
{
    /// <summary>
    ///   Contains information about a partition that has attempted to receive an event from the Azure Event Hub
    ///   service in an <c>EventProcessorClient</c> context, as well as the received event, if any.  It
    ///   allows creating a checkpoint based on the associated event.
    /// </summary>
    ///
    /// <seealso href="https://www.nuget.org/packages/Azure.Messaging.EventHubs.Processor">Azure.Messaging.EventHubs.Processor (NuGet)</seealso>
    ///
    public readonly struct ProcessEventBatchArgs
    {
        /// <summary>
        ///   Indicates whether or not the arguments contain an event to be processed.  In
        ///   the case where no event is contained, then the creation of checkpoints and reading the last
        ///   enqueued event properties are unavailable.
        /// </summary>
        ///
        /// <value><c>true</c> if the arguments contain an event to be processed; otherwise, <c>false</c>.</value>
        ///
        public bool HasEvents => Events != null && Partition != null;

        /// <summary>
        ///   The context of the Event Hub partition this instance is associated with.
        /// </summary>
        ///
        public PartitionContext Partition { get; }

        /// <summary>
        ///   The received event to be processed.  Expected to be <c>null</c> if the receive call has timed out.
        /// </summary>
        ///
        /// <remarks>
        ///   Ownership of this data, including the memory that holds its <see cref="EventData.EventBody" />,
        ///   is assumed to transfer to consumers of the <see cref="ProcessEventArgs" />.  It may be considered
        ///   immutable and is safe to access so long as the reference is held.
        /// </remarks>
        ///
        public List<EventData>? Events { get; }

        /// <summary>
        ///   A <see cref="System.Threading.CancellationToken"/> to indicate that the processor is requesting that the
        ///   handler stop its activities.  If this token is requesting cancellation, then either the processor is
        ///   attempting to shutdown or ownership of the partition has changed.
        /// </summary>
        ///
        /// <remarks>
        ///   The handler processing events has responsibility for deciding whether or not to honor
        ///   the cancellation request.  If the application chooses not to do so, the processor will wait for the
        ///   handler to complete before taking further action.
        /// </remarks>
        ///
        public CancellationToken CancellationToken { get; }

        /// <summary>
        ///   The callback to be called upon <see cref="UpdateCheckpointAsync" /> call.
        /// </summary>
        ///
        private Func<CancellationToken, Task> UpdateCheckpointAsyncImplementation { get; }

        /// <summary>
        ///   Initializes a new instance of the <see cref="ProcessEventArgs"/> structure.
        /// </summary>
        ///
        /// <param name="partition">The context of the Event Hub partition this instance is associated with.</param>
        /// <param name="events">The received event to be processed.  Expected to be <c>null</c> if the receive call has timed out.</param>
        /// <param name="updateCheckpointImplementation">The callback to be called upon <see cref="UpdateCheckpointAsync" /> call.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        public ProcessEventBatchArgs(
            PartitionContext partition,
            IEnumerable<EventData>? events,
            Func<CancellationToken, Task> updateCheckpointImplementation,
            CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNull(updateCheckpointImplementation, nameof(updateCheckpointImplementation));

            Partition = partition;
            Events = events?.ToList();
            UpdateCheckpointAsyncImplementation = updateCheckpointImplementation;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        ///   Updates the checkpoint for the <see cref="PartitionContext" /> and <see cref="EventData" /> associated with
        ///   this event.
        /// </summary>
        ///
        /// <param name="cancellationToken">An optional <see cref="System.Threading.CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <exception cref="InvalidOperationException">Occurs when <see cref="Partition" /> and <see cref="Data" /> are <c>null</c>.</exception>
        ///
        public Task UpdateCheckpointAsync(CancellationToken cancellationToken = default) => UpdateCheckpointAsyncImplementation(cancellationToken);        
    }
}