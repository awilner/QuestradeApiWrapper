namespace QuestradeApiWrapper.Model;

// https://www.questrade.com/api/documentation/rest-operations/account-calls/accounts-id-balances
public class Balance
{
    public required Currency currency { get; set; }
    public required double cash { get; set; }
    public required double marketValue { get; set; }
    public required double totalEquity { get; set; }
    public required double buyingPower { get; set; }
    public required double maintenanceExcess { get; set; }
    public required bool isRealTime { get; set; }
}

public class Balances
{
    public required List<Balance> perCurrencyBalances { get; set; }
    public required List<Balance> combinedBalances { get; set; }
    public required List<Balance> sodPerCurrencyBalances { get; set; }
    public required List<Balance> sodCombinedBalances { get; set; }
}
