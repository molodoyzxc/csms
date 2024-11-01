using Itmo.Dev.Platform.Common.Extensions;
using System.Threading.Channels;

namespace Messages;

public class MessageProcessor : IMessageSender, IMessageProcessor
{
    private readonly Channel<Message> _channel;
    private readonly IMessageHandler _messageHandler;
    private readonly int _batchSize;
    private bool _completed;

    public MessageProcessor(
        IMessageHandler messageHandler,
        int capacity = 100,
        int batchSize = 10,
        TimeSpan? batchTimeout = null)
    {
        _channel = Channel.CreateBounded<Message>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
        });
        _messageHandler = messageHandler;
        _batchSize = batchSize;
    }

    public ValueTask SendAsync(Message message, CancellationToken cancellationToken)
    {
        if (_completed)
        {
            throw new InvalidOperationException("Cannot send messages after completion.");
        }

        return _channel.Writer.WriteAsync(message, cancellationToken);
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await foreach (IReadOnlyList<Message> chunk in _channel.Reader.ReadAllAsync(cancellationToken)
                           .ChunkAsync(_batchSize, TimeSpan.FromSeconds(2))
                           .WithCancellation(cancellationToken))
        {
            await _messageHandler.HandleAsync(chunk, cancellationToken).ConfigureAwait(false);
        }
    }

    public void Complete()
    {
        _channel.Writer.Complete();
        _completed = true;
    }
}