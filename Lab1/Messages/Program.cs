namespace Messages;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var messageHandler = new ConsoleMessageHandler();
        var processor = new MessageProcessor(
            messageHandler,
            capacity: 100,
            batchSize: 5,
            batchTimeout: TimeSpan.FromSeconds(2));
#pragma warning disable CA1859
        IMessageProcessor messageProcessor = processor;
        IMessageSender messageSender = processor;
#pragma warning restore CA1859
        Task processingTask = messageProcessor.ProcessAsync(CancellationToken.None);
        IEnumerable<Message> messages = Enumerable.Range(1, 20).Select(i => new Message($"Title {i}", $"Message text {i}"));
        await Parallel.ForEachAsync(
            messages,
            new ParallelOptions { MaxDegreeOfParallelism = 1 },
            async (message, token) =>
                {
                    await messageSender.SendAsync(message, token).ConfigureAwait(false);
                })
            .ConfigureAwait(false);
        messageProcessor.Complete();
        await processingTask.ConfigureAwait(false);
        Console.WriteLine("All messages have been processed.");
    }
}