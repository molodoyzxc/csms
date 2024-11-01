namespace Messages;

public class ConsoleMessageHandler : IMessageHandler
{
    public ValueTask HandleAsync(IEnumerable<Message> messages, CancellationToken cancellationToken)
    {
        string output = string.Join(Environment.NewLine, messages.Select(m => $"{m.Title}: {m.Text}"));
        Console.WriteLine(output);
        return ValueTask.CompletedTask;
    }
}