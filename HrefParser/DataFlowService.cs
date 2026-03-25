using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;
using System.Windows;

namespace HrefParser
{
    internal static class DataFlowService
    {
        public static ITargetBlock<TInput> CreatePipeline<TInput, TOutput>(
        Func<TInput, Task<TInput>> pause,
        Func<TInput, Task<TOutput>> processor,
        Action<TOutput> onItemProcessed,
        CancellationToken token,
        int parallelism = 8)
        {
            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = parallelism,
                CancellationToken = token,
                BoundedCapacity = parallelism * 2
            };
            var pauseBlock = new TransformBlock<TInput, TInput>(pause, options);
            var transformBlock = new TransformBlock<TInput, TOutput>(processor, options);
            var actionBlock = new ActionBlock<TOutput>(onItemProcessed, options);
            
            pauseBlock.LinkTo(transformBlock, new DataflowLinkOptions { PropagateCompletion = true });
            transformBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

            var completionSource = new WriteOnceBlock<TOutput>(x => x);
            _ = actionBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    // Поставь сюда точку останова (Breakpoint)
                    var error = t.Exception?.Flatten().InnerException;
                    System.Diagnostics.Debug.WriteLine($"ТРУБА СДОХЛА: {error?.Message}");
                }
                else completionSource.Complete();
            });

            return DataflowBlock.Encapsulate<TInput, TOutput>(pauseBlock, completionSource);
        }
    }
}
