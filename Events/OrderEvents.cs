
namespace Shared.Events;

public record OrderPlacedEvent(Guid UserId, Guid GameId, decimal Price);

public record PaymentProcessedEvent(Guid UserId, Guid GameId, bool Approved);