namespace Task1.Models;

public record ConfigurationPageToken(long Id)
{
    public static readonly ConfigurationPageToken Empty = new(0);
}