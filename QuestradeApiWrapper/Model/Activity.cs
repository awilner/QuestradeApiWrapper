namespace QuestradeApiWrapper.Model;

// https://www.questrade.com/api/documentation/rest-operations/account-calls/accounts-id-activities
public class Activity
{
    public required DateTime tradeDate { get; set; }
    public required DateTime transactionDate { get; set; }
    public required DateTime settlementDate { get; set; }
    public required string action { get; set; }
    public required string symbol { get; set; }
    public required ulong symbolId { get; set; }
    public required string description { get; set; }
    public required Currency currency { get; set; }
    public required double quantity { get; set; }
    public required double price { get; set; }
    public required double grossAmount { get; set; }
    public required double commission { get; set; }
    public required double netAmount { get; set; }
    public required string type { get; set; }
}

public class Activities
{
    public required List<Activity> activities { get; set; }
}