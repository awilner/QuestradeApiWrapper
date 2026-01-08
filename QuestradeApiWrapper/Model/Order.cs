namespace QuestradeApiWrapper.Model;

// https://www.questrade.com/api/documentation/rest-operations/account-calls/accounts-id-orders
public class OrderLeg
{
    public required uint legId { get; set; }
    public required string symbol { get; set; }
    public required ulong symbolId { get; set; }
    public required uint legRatioQuantity { get; set; }
    public required OrderSide side { get; set; }
    public required double avgExecPrice { get; set; }
    public required double lastExecPrice { get; set; }
}

public class Order
{
    public required ulong id { get; set; }
    public required string symbol { get; set; }
    public required ulong symbolId { get; set; }
    public required int totalQuantity { get; set; }
    public required int openQuantity { get; set; }
    public required int filledQuantity { get; set; }
    public required int canceledQuantity { get; set; }
    public required OrderSide side { get; set; }
    public required OrderType orderType { get; set; }
    public required double limitPrice { get; set; }
    public required double stopPrice { get; set; }
    public required bool isAllOrNone { get; set; }
    public required bool isAnonymous { get; set; }
    public required int icebergQuantity { get; set; }
    public required int minQuantity { get; set; }
    public required double avgExecPrice { get; set; }
    public required double lastExecPrice { get; set; }
    public required string source { get; set; }
    public required OrderTimeInForce timeInForce { get; set; }
    public required DateTime gtdDate { get; set; }
    public required OrderState state { get; set; }
    public required string clientReasonStr { get; set; }
    public required ulong chainId { get; set; }
    public required DateTime creationTime { get; set; }
    public required DateTime updateTime { get; set; }
    public required string notes { get; set; }
    public required string primaryRoute { get; set; }
    public required string secondaryRoute { get; set; }
    public required string orderRoute { get; set; }
    public required string venueHoldingOrder { get; set; }
    public required double commissionCharged { get; set; }
    public required string exchangeOrderId { get; set; }
    public required bool isSignificantShareholder { get; set; }
    public required bool isInsider { get; set; }
    public required bool isLimitOffsetInDollar { get; set; }
    public required ulong userId { get; set; }
    public required double placementCommission { get; set; }
    public required List<OrderLeg> legs { get; set; }
    public required StrategyType strategyType { get; set; }
    public required double triggerStopPrice { get; set; }
    public required ulong orderGroupId { get; set; }
    public required OrderClass orderClass { get; set; }
}

public class Orders
{
    public required List<Order> orders { get; set; }
}