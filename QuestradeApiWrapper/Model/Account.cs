namespace QuestradeApiWrapper.Model;

// https://www.questrade.com/api/documentation/rest-operations/account-calls/accounts-id-activities
public class Account
{
    public required AccountType type { get; set; }
    public required string number { get; set; }
    public required AccountStatus status { get; set; }
    public required bool isPrimary { get; set; }
    public required bool isBilling { get; set; }
    public required ClientAccountType clientAccountType { get; set; }
}

public class Accounts
{
    public required List<Account> accounts { get; set; }
    public required ulong userId { get; set; }
 }