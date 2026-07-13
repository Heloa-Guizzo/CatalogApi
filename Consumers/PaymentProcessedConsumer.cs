using CatalogAPI.Models;
using CatalogAPI.Repositories;
using MassTransit;
using Shared.Events;

namespace CatalogAPI.Consumers;

public class PaymentProcessedConsumer: IConsumer<PaymentProcessedEvent>
{
    private readonly IUserLibraryRepository _libraryRepository;

    public PaymentProcessedConsumer(IUserLibraryRepository libraryRepository)
    {
        _libraryRepository = libraryRepository;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        if (!context.Message.Approved)
            return;

        await _libraryRepository.AddAsync(
            new UserLibrary
            {
                Id = Guid.NewGuid(),
                UserId = context.Message.UserId,
                GameId = context.Message.GameId,
                PurchasedAt = DateTime.UtcNow
            });
    }
}

