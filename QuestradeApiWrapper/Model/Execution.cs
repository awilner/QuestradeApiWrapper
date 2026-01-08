namespace QuestradeApiWrapper.Model;

// https://www.questrade.com/api/documentation/rest-operations/account-calls/accounts-id-executions
public class Execution
{
    public required string symbol { get; set; }
    public required ulong symbolId { get; set; }
    public required int quantity { get; set; }
    public required OrderSide side { get; set; }
    public required double price { get; set; }
    public required ulong id { get; set; }
    public required ulong orderId { get; set; }
    public required ulong orderChainId { get; set; }
    public required string exchangeExecId { get; set; }
    public required DateTime timestamp { get; set; }
    public required string notes { get; set; }
    public required string venue { get; set; }
    public required double totalCost { get; set; }
    public required double orderPlacementCommission { get; set; }
    public required double commission { get; set; }
    public required double executionFee { get; set; }
    public required double secFee { get; set; }
    public required double canadianExecutionFee { get; set; }
    public required ulong parentId { get; set; }
}

public class Executions
{
    public required List<Execution> executions { get; set; }
}