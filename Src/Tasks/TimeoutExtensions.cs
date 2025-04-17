// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Async.Extensions.Tasks
{
    /// <summary>
    /// Provides extension methods for applying timeouts to tasks and value tasks.
    /// </summary>
    public static class TimeoutExtensions
    {
        /// <summary>
        /// Applies a timeout to a Task. If the timeout elapses before the task completes, a TimeoutException is thrown.
        /// </summary>
        /// <param name="task">The task to apply the timeout to.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <typeparam name="T">The type of the task result.</typeparam>
        /// <returns>The original task result if it completes within the timeout period.</returns>
        public static async Task<T> TimeoutAfterAsync<T>(
            this Task<T> task,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                timeoutCts.CancelAfter(timeout);

                var completedTask = await Task.WhenAny(task, Task.Delay(Timeout.Infinite, timeoutCts.Token));

                if (completedTask == task)
                {
                    return await task;
                }

                throw new TimeoutException("The operation has timed out.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException("The operation was canceled by the provided cancellation token.");
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("The operation has timed out.");
            }

            throw new TimeoutException("The operation has timed out.");
        }

        /// <summary>
        /// Applies a timeout to a Task. If the timeout elapses before the task completes, a TimeoutException is thrown.
        /// </summary>
        /// <param name="task">The task to apply the timeout to.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A completed Task if it finishes within the timeout period.</returns>
        public static async Task TimeoutAfterAsync(
            this Task task,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                timeoutCts.CancelAfter(timeout);
                var completedTask = await Task.WhenAny(task, Task.Delay(Timeout.Infinite, timeoutCts.Token));
                if (completedTask == task)
                {
                    await task;
                    return;
                }
                throw new TimeoutException("The operation has timed out.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException("The operation was canceled by the provided cancellation token.");
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("The operation has timed out.");
            }

            throw new TimeoutException("The operation has timed out.");
        }

        /// <summary>
        /// Applies a timeout to a ValueTask. If the timeout elapses before the task completes, a TimeoutException is thrown.
        /// </summary>
        /// <param name="valueTask">The ValueTask to apply the timeout to.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <typeparam name="T">The type of the task result.</typeparam>
        /// <returns>The original ValueTask result if it completes within the timeout period.</returns>
        public static async Task<T> TimeoutAfterAsync<T>(
            this ValueTask<T> valueTask,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            return await valueTask.AsTask().TimeoutAfterAsync(timeout, cancellationToken);
        }

        /// <summary>
        /// Applies a timeout to a ValueTask. If the timeout elapses before the task completes, a TimeoutException is thrown.
        /// </summary>
        /// <param name="valueTask">The ValueTask to apply the timeout to.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A completed ValueTask if it finishes within the timeout period.</returns>
        public static async Task TimeoutAfterAsync(
            this ValueTask valueTask,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            await valueTask.AsTask().TimeoutAfterAsync(timeout, cancellationToken);
        }
    }
}