using System.IO.Pipes;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using Moq;
using Moq.Protected;
using QuestradeApiWrapper.Model;
using Xunit.Abstractions;

namespace QuestradeApiWrapper;

public class QuestradeUnitTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private static readonly HttpClient httpClient = new HttpClient();

    public QuestradeUnitTest(ITestOutputHelper output)
    {
        _testOutputHelper = output;
    }

    [Fact]
    public void Init_ShouldUpdateRefreshToken()
    {
        // Arrange
        string accessResponse = "{\"access_token\":\"my_access_token\",\"api_server\":\"https:\\/\\/api.example.com\\/\",\"expires_in\":1800,\"refresh_token\":\"my_refresh_token\",\"token_type\":\"Bearer\"}";

        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        // Setup the protected SendAsync method
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.Equals("https://login.questrade.com/oauth2/token?grant_type=refresh_token&refresh_token=some_refresh_token")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(accessResponse, System.Text.Encoding.UTF8, "application/json"),
            })
            .Verifiable();

        var httpClient = new HttpClient(mockHandler.Object);

        Questrade questrade = new Questrade(httpClient);

        // Act
        questrade.init("some_refresh_token");

        // Assert
        questrade.refreshToken().Should().Be("my_refresh_token");
    }

    [Fact]
    public void GetAccounts_ShouldReturnValidResult()
    {
        // Arrange
        string accessResponse = "{\"access_token\":\"my_access_token\",\"api_server\":\"https:\\/\\/api.example.com\\/\",\"expires_in\":1800,\"refresh_token\":\"my_refresh_token\",\"token_type\":\"Bearer\"}";
        string accountsResponse = "{ \"accounts\":[{\"type\":\"RRSP\",\"number\":\"1234\",\"status\":\"Active\",\"isPrimary\":false,\"isBilling\":false,\"clientAccountType\":\"Individual\"}],\"userId\":4242}";

        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        // Setup the protected SendAsync method
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.StartsWith("https://login.questrade.com")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(accessResponse, System.Text.Encoding.UTF8, "application/json"),
            })
            .Verifiable();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.StartsWith("https://api.example.com")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(accountsResponse, System.Text.Encoding.UTF8, "application/json"),
            })
            .Verifiable();

        var httpClient = new HttpClient(mockHandler.Object);

        Questrade questrade = new Questrade(httpClient);
        questrade.init("some_refresh_token");

        // Act
        var accounts = questrade.GetAccounts();

        // Assert
        Account expected = new Account
        {
            type = AccountType.RRSP,
            number = "1234",
            status = AccountStatus.Active,
            isPrimary = false,
            isBilling = false,
            clientAccountType = ClientAccountType.Individual
        };

        accounts.Should().NotBeNull();
        accounts.Count.Should().Be(1);
        accounts[0].Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetAccountActivities_WhenParametersAreValid_ShouldReturnValidResult()
    {
        // Arrange
        string accessResponse = "{\"access_token\":\"my_access_token\",\"api_server\":\"https:\\/\\/api.example.com\\/\",\"expires_in\":1800,\"refresh_token\":\"my_refresh_token\",\"token_type\":\"Bearer\"}";
        string accountActivitiesResponse = "{\"activities\":[{\"tradeDate\":\"2025-02-04T00:00:00.000000-05:00\",\"transactionDate\":\"2025-02-04T00:00:00.000000-05:00\",\"settlementDate\":\"2025-02-04T00:00:00.000000-05:00\",\"action\":\"some action\",\"symbol\":\"BOG.US\",\"symbolId\":12345,\"description\":\"This is a long-winded description of an activity\",\"currency\":\"CAD\",\"quantity\":5,\"price\":3.14,\"grossAmount\":3.16,\"commission\":0.02,\"netAmount\":1.75,\"type\":\"some type\"}]}";

        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        // Setup the protected SendAsync method
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.StartsWith("https://login.questrade.com")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(accessResponse, System.Text.Encoding.UTF8, "application/json"),
            })
            .Verifiable();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.StartsWith("https://api.example.com/v1/accounts/1234/activities")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(accountActivitiesResponse, System.Text.Encoding.UTF8, "application/json"),
            })
            .Verifiable();

        var httpClient = new HttpClient(mockHandler.Object);

        Questrade questrade = new Questrade(httpClient);
        questrade.init("some_refresh_token");

        // Act
        var activities = questrade.GetAccountActivities("1234", DateTime.Now.AddDays(-5), DateTime.Now);

        // Assert
        Activity expected = new Activity
        {
            tradeDate = DateTime.Parse("2025-02-04T00:00:00.000000-05:00"),
            transactionDate = DateTime.Parse("2025-02-04T00:00:00.000000-05:00"),
            settlementDate = DateTime.Parse("2025-02-04T00:00:00.000000-05:00"),
            action = "some action",
            symbol = "BOG.US",
            symbolId = 12345,
            description = "This is a long-winded description of an activity",
            currency = Currency.CAD,
            quantity = 5,
            price = 3.14,
            grossAmount = 3.16,
            commission = 0.02,
            netAmount = 1.75,
            type = "some type"
        };

        activities.Should().NotBeNull();
        activities.Count.Should().Be(1);
        activities[0].Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetAccountActivity_WhenIntervalTooLarge_ShouldThrow()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(mockHandler.Object);

        Questrade questrade = new Questrade(httpClient);

        // Act
        Assert.Throws<ArgumentException>(() => questrade.GetAccountActivities("1234", DateTime.Now.AddDays(-32), DateTime.Now));

        // Assert
        mockHandler
            .Protected()
            .Verify("SendAsync", Times.Never(), [ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()]);
    }

    [Fact]
    public void GetAccountActivity_WhenStartDateAfterEndDate_ShouldThrow()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(mockHandler.Object);

        Questrade questrade = new Questrade(httpClient);

        // Act
        Assert.Throws<ArgumentException>(() => questrade.GetAccountActivities("1234", DateTime.Now, DateTime.Now.AddDays(-1)));

        // Assert
        mockHandler
            .Protected()
            .Verify("SendAsync", Times.Never(), [ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()]);
    }

    [Fact]
    public void GetAccountOrders_WhenParametersAreValid_ShouldReturnValidResult()
    {
        // Arrange
        string accessResponse = "{\"access_token\":\"my_access_token\",\"api_server\":\"https:\\/\\/api.example.com\\/\",\"expires_in\":1800,\"refresh_token\":\"my_refresh_token\",\"token_type\":\"Bearer\"}";
        string accountOrdersResponse = @"{""orders"":[
            {""id"": 4242,
            ""symbol"": ""ABCD"",
            ""symbolId"": 123456,
            ""totalQuantity"": 10,
            ""openQuantity"": 4,
            ""filledQuantity"": 5,
            ""canceledQuantity"": 1,
            ""side"": ""Short"",
            ""orderType"": ""StopLimit"",
            ""limitPrice"": 3.14,
            ""stopPrice"": 2.19,
            ""isAllOrNone"": false,
            ""isAnonymous"": true,
            ""icebergQuantity"": 0,
            ""minQuantity"": 3,
            ""avgExecPrice"": 3.15,
            ""lastExecPrice"": 3.16,
            ""source"": ""some source"",
            ""timeInForce"": ""GoodTillDate"",
            ""gtdDate"": ""2025-06-01T00:00:00"",
            ""state"": ""Partial"",
            ""clientReasonStr"": ""some reason"",
            ""chainId"": 2468,
            ""creationTime"": ""2025-05-25T00:00:00"",
            ""updateTime"": ""2025-05-29T00:00:00"",
            ""notes"": ""some notes"",
            ""primaryRoute"": ""some route"",
            ""secondaryRoute"": ""other route"",
            ""orderRoute"": ""order route"",
            ""venueHoldingOrder"": ""some venue"",
            ""commissionCharged"": 0.50,
            ""exchangeOrderId"": ""exchange order id"",
            ""isSignificantShareholder"": false,
            ""isInsider"": false,
            ""isLimitOffsetInDollar"": false,
            ""userId"": 12345678,
            ""placementCommission"": 0.20,
            ""legs"": [],
            ""strategyType"": ""SingleLeg"",
            ""triggerStopPrice"": 0.30,
            ""orderGroupId"": 87654321,
            ""orderClass"": ""Limit""}]}";

        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        // Setup the protected SendAsync method
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.StartsWith("https://login.questrade.com")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(accessResponse, System.Text.Encoding.UTF8, "application/json"),
            })
            .Verifiable();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.StartsWith("https://api.example.com/v1/accounts/1234/orders")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(accountOrdersResponse, System.Text.Encoding.UTF8, "application/json"),
            })
            .Verifiable();

        var httpClient = new HttpClient(mockHandler.Object);

        Questrade questrade = new Questrade(httpClient);
        questrade.init("some_refresh_token");

        // Act
        var orders = questrade.GetAccountOrders("1234");

        // Assert
        Order expected = new Order
        {
            id = 4242,
            symbol = "ABCD",
            symbolId = 123456,
            totalQuantity = 10,
            openQuantity = 4,
            filledQuantity = 5,
            canceledQuantity = 1,
            side = OrderSide.Short,
            orderType = OrderType.StopLimit,
            limitPrice = 3.14,
            stopPrice = 2.19,
            isAllOrNone = false,
            isAnonymous = true,
            icebergQuantity = 0,
            minQuantity = 3,
            avgExecPrice = 3.15,
            lastExecPrice = 3.16,
            source = "some source",
            timeInForce = OrderTimeInForce.GoodTillDate,
            gtdDate = DateTime.Parse("2025-06-01T00:00:00"),
            state = OrderState.Partial,
            clientReasonStr = "some reason",
            chainId = 2468,
            creationTime = DateTime.Parse("2025-05-25T00:00:00"),
            updateTime = DateTime.Parse("2025-05-29T00:00:00"),
            notes = "some notes",
            primaryRoute = "some route",
            secondaryRoute = "other route",
            orderRoute = "order route",
            venueHoldingOrder = "some venue",
            commissionCharged = 0.50,
            exchangeOrderId = "exchange order id",
            isSignificantShareholder = false,
            isInsider = false,
            isLimitOffsetInDollar = false,
            userId = 12345678,
            placementCommission = 0.20,
            legs = new List<OrderLeg>(),
            strategyType = StrategyType.SingleLeg,
            triggerStopPrice = 0.30,
            orderGroupId = 87654321,
            orderClass = OrderClass.Limit
        };

        orders.Should().NotBeNull();
        orders.Count.Should().Be(1);
        orders[0].Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetAccountOrders_WhenStartDateAfterEndDate_ShouldThrow()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(mockHandler.Object);

        Questrade questrade = new Questrade(httpClient);

        // Act
        Assert.Throws<ArgumentException>(() => questrade.GetAccountOrders("1234", startTime: DateTime.Now, endTime: DateTime.Now.AddDays(-1)));

        // Assert
        mockHandler
            .Protected()
            .Verify("SendAsync", Times.Never(), [ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()]);
    }

    [Fact]
    public void GetAccountExecutions_WhenParametersAreValid_ShouldReturnValidResult()
    {
        // Arrange
        string accessResponse = "{\"access_token\":\"my_access_token\",\"api_server\":\"https:\\/\\/api.example.com\\/\",\"expires_in\":1800,\"refresh_token\":\"my_refresh_token\",\"token_type\":\"Bearer\"}";
        string accountExecutionsResponse = @"{
            ""executions"":
            [
                {
                    ""id"": 4242,
                    ""symbol"": ""BOG.US"",
                    ""symbolId"": 123456,
                    ""quantity"": 10,
                    ""side"": ""BTO"",
                    ""price"": 1.23,
                    ""orderId"": 4321,
                    ""orderChainId"": 5678,
                    ""exchangeExecId"": ""some exec id"",
                    ""timestamp"": ""2025-12-01T00:00:00"",
                    ""notes"": ""some execution notes"",
                    ""venue"": ""some venue"",
                    ""totalCost"": 3.14,
                    ""orderPlacementCommission"": 2.13,
                    ""commission"": 1.23,
                    ""executionFee"": 0.25,
                    ""secFee"": 0.11,
                    ""canadianExecutionFee"": 0.01,
                    ""parentId"": 9876
                }
            ]
        }";

        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        // Setup the protected SendAsync method
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.StartsWith("https://login.questrade.com")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(accessResponse, System.Text.Encoding.UTF8, "application/json"),
            })
            .Verifiable();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.Equals("https://api.example.com/v1/accounts/1234/executions")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(accountExecutionsResponse, System.Text.Encoding.UTF8, "application/json"),
            })
            .Verifiable();

        var httpClient = new HttpClient(mockHandler.Object);

        Questrade questrade = new Questrade(httpClient);
        questrade.init("some_refresh_token");

        // Act
        var executions = questrade.GetAccountExecutions("1234");

        // Assert
        Execution expected = new Execution
        {
            id = 4242,
            symbol = "BOG.US",
            symbolId = 123456,
            quantity = 10,
            side = OrderSide.BTO,
            price = 1.23,
            orderId = 4321,
            orderChainId = 5678,
            exchangeExecId = "some exec id",
            timestamp = DateTime.Parse("2025-12-01T00:00:00"),
            notes = "some execution notes",
            venue = "some venue",
            totalCost = 3.14,
            orderPlacementCommission = 2.13,
            commission = 1.23,
            executionFee = 0.25,
            secFee = 0.11,
            canadianExecutionFee = 0.01,
            parentId = 9876
        };

        executions.Should().NotBeNull();
        executions.Count.Should().Be(1);
        executions[0].Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetAccountExecutions_WhenStartDateAfterEndDate_ShouldThrow()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(mockHandler.Object);

        Questrade questrade = new Questrade(httpClient);

        // Act
        Assert.Throws<ArgumentException>(() => questrade.GetAccountExecutions("1234", startTime: DateTime.Now, endTime: DateTime.Now.AddDays(-1)));

        // Assert
        mockHandler
            .Protected()
            .Verify("SendAsync", Times.Never(), [ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()]);
    }
    
    [Fact]
    public void GetAccountBalances_WhenParametersAreValid_ShouldReturnValidResult()
    {
        // Arrange
        string accessResponse = "{\"access_token\":\"my_access_token\",\"api_server\":\"https:\\/\\/api.example.com\\/\",\"expires_in\":1800,\"refresh_token\":\"my_refresh_token\",\"token_type\":\"Bearer\"}";
        string accountBalancesResponse =
        @"{
            ""perCurrencyBalances"":
            [
                {
                    ""currency"": ""CAD"",
                    ""cash"": 1.23,
                    ""marketValue"": 4.56,
                    ""totalEquity"": 7.89,
                    ""buyingPower"": 0.12,
                    ""maintenanceExcess"": 3.45,
                    ""isRealTime"": false
                }
            ],
            ""combinedBalances"":
            [
                {
                    ""currency"": ""USD"",
                    ""cash"": 9.87,
                    ""marketValue"": 6.54,
                    ""totalEquity"": 3.21,
                    ""buyingPower"": 0.98,
                    ""maintenanceExcess"": 7.65,
                    ""isRealTime"": true
                }
            ],
            ""sodPerCurrencyBalances"":
            [
                {
                    ""currency"": ""CAD"",
                    ""cash"": 1.23,
                    ""marketValue"": 6.54,
                    ""totalEquity"": 7.89,
                    ""buyingPower"": 2.10,
                    ""maintenanceExcess"": 3.45,
                    ""isRealTime"": false
                }
            ],
            ""sodCombinedBalances"":
            [
                {
                    ""currency"": ""USD"",
                    ""cash"": 3.21,
                    ""marketValue"": 4.56,
                    ""totalEquity"": 9.87,
                    ""buyingPower"": 0.12,
                    ""maintenanceExcess"": 5.43,
                    ""isRealTime"": true
                }
            ]
        }";

        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        // Setup the protected SendAsync method
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.StartsWith("https://login.questrade.com")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(accessResponse, System.Text.Encoding.UTF8, "application/json"),
            })
            .Verifiable();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.Equals("https://api.example.com/v1/accounts/1234/balances")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(accountBalancesResponse, System.Text.Encoding.UTF8, "application/json"),
            })
            .Verifiable();

        var httpClient = new HttpClient(mockHandler.Object);

        Questrade questrade = new Questrade(httpClient);
        questrade.init("some_refresh_token");

        // Act
        var balances = questrade.GetAccountBalances("1234");

        // Assert
        Balances expected = new Balances
        {
            perCurrencyBalances = new List<Balance>
            {
                new Balance
                {
                    currency = Currency.CAD,
                    cash = 1.23,
                    marketValue = 4.56,
                    totalEquity = 7.89,
                    buyingPower = 0.12,
                    maintenanceExcess = 3.45,
                    isRealTime = false
                }
            },
            combinedBalances = new List<Balance>
            {
                new Balance
                {
                    currency = Currency.USD,
                    cash = 9.87,
                    marketValue = 6.54,
                    totalEquity = 3.21,
                    buyingPower = 0.98,
                    maintenanceExcess = 7.65,
                    isRealTime = true
                }
            },
            sodPerCurrencyBalances = new List<Balance>
            {
                new Balance
                {
                    currency = Currency.CAD,
                    cash = 1.23,
                    marketValue = 6.54,
                    totalEquity = 7.89,
                    buyingPower = 2.10,
                    maintenanceExcess = 3.45,
                    isRealTime = false
                }
            },
            sodCombinedBalances = new List<Balance>
            {
                new Balance
                {
                    currency = Currency.USD,
                    cash = 3.21,
                    marketValue = 4.56,
                    totalEquity = 9.87,
                    buyingPower = 0.12,
                    maintenanceExcess = 5.43,
                    isRealTime = true
                }
            }
        };

        balances.Should().NotBeNull();
        balances.Should().BeEquivalentTo(expected);
    }
}
