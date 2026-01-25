namespace TicketFlow.Domain.ValueObjects;

public record Seat(string Sector, string Row, string Number)
{
    public const int MaxSectorLength = 20;
    public const int MaxRowLength = 10;
    public const int MaxNumberLength = 10;
    
    public override string ToString() => $"{Sector} - {Row}{Number}";
}