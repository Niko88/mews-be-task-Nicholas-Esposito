﻿using ExchangeRateUpdater.Application.Components.Consumers;
using ExchangeRateUpdater.Application.Components.Queries;
using ExchangeRateUpdater.Application.Services;
using ExchangeRateUpdater.Domain.Types;
using MassTransit;
using Moq;
using Serilog;

namespace ExchangeRateUpdater.Tests;

public class GetExchangeRatesQueryConsumerTests
{
    private readonly Mock<IExchangeRateProviderService> _mockExchangeRateProviderService;
    private readonly Mock<ConsumeContext<GetExchangeRatesQuery>> _mockConsumeContext;
    private readonly Mock<ILogger> _mockLogger;
    private readonly List<Currency> _currencies = new() { new Currency("USD"), new Currency("EUR") };

    public GetExchangeRatesQueryConsumerTests()
    {
        _mockExchangeRateProviderService = new Mock<IExchangeRateProviderService>();
        _mockConsumeContext = new Mock<ConsumeContext<GetExchangeRatesQuery>>();
        _mockLogger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Consume_Should_ReturnExchangeRates_When_Successful()
    {
        // Arrange
        var query = new GetExchangeRatesQuery(NonEmptyList<Currency>.Create(_currencies));
        var expectedRates = NonNullResponse<Dictionary<string, ExchangeRate>>.Success(new Dictionary<string, ExchangeRate>
        {
            {"USD",new ExchangeRate(new Currency("USD"),new Currency("CZK"),1m)},
            {"EUR",new ExchangeRate(new Currency("EUR"),new Currency("CZK"),1m)},
        });

        _mockExchangeRateProviderService.Setup(x => x.GetExchangeRates()).ReturnsAsync(expectedRates);
        _mockConsumeContext.Setup(x => x.Message).Returns(query);

        var consumer = new GetExchangeRatesQueryConsumer(_mockExchangeRateProviderService.Object, _mockLogger.Object);

        // Act
        await consumer.Consume(_mockConsumeContext.Object);


        // Assert
        _mockConsumeContext.Verify(x => x.RespondAsync(It.Is<NonNullResponse<IEnumerable<ExchangeRate>>>(r =>
                r.Content == expectedRates)),
            Times.Once);
    }

}