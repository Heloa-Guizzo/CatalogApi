using CatalogAPI.Models;
using CatalogAPI.Repositories;
using MassTransit;
using Shared.Events;

public class PaymentProcessedConsumer :
    IConsumer<PaymentProcessedEvent>
{
   
    public Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        Console.WriteLine(
            $"[CatalogAPI] PaymentProcessedEvent received | UserId: {context.Message.UserId} | GameId: {context.Message.GameId} | Approved: {context.Message.Approved}");

        if (!context.Message.Approved)
        {
            Console.WriteLine(
                $"[CatalogAPI] Payment was rejected. Game will not be added to user library.");

            return Task.CompletedTask;
        }

        UserLibraryStorage.Games.Add(
            new UserLibrary
            {
                Id = Guid.NewGuid(),
                UserId = context.Message.UserId,
                GameId = context.Message.GameId,
                PurchasedAt = DateTime.UtcNow
            });

        Console.WriteLine(
            $"[CatalogAPI] Game {context.Message.GameId} successfully added to library for user {context.Message.UserId}");

        Console.WriteLine(
            $"[CatalogAPI] Current library size: {UserLibraryStorage.Games.Count}");

        return Task.CompletedTask;
    }
}