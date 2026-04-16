namespace PhysOn.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
