using QuestradeApiWrapper.Model;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuestradeApiWrapper;

public class Questrade
{
    // Login URL
    private const string LoginUrl = "https://login.questrade.com/oauth2/token?grant_type=refresh_token&refresh_token=";
    private const string GetAccountsApiEndpoint = "v1/accounts";
    private const string GetAccountActivitiesApiEndpoint = "/activities";
    private const string GetAccountOrdersApiEndpoint = "/orders";
    private const string GetAccountExecutionsApiEndpoint = "/executions";
    private const string GetAccountBalancesApiEndpoint = "/balances";
    private const string StartTimeToken = "startTime=";
    private const string EndTimeToken = "endTime=";
    private const string StateFilterToken = "stateFilter=";
    private const string OrderIdsToken = "ids=";

    private HttpClient _httpClient;

    private AccessCredentials _credentials;

    private DateTime _credentialsExpiry;


    private JsonSerializerOptions _jsonSerializeOptions;


    public Questrade(HttpClient httpClient)
    {
        _httpClient = httpClient;

        _jsonSerializeOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        _credentials = new AccessCredentials
        {
            access_token = "",
            api_server = "",
            refresh_token = "",
            token_type = "",
            expires_in = 0
        };

        _credentialsExpiry = DateTime.MinValue;
    }

    // Ideally call this to save the refresh token after every interaction.
    public string refreshToken()
    {
        return _credentials.refresh_token;
    }

    public void init(string refreshToken)
    {
        // Build URL
        string url = LoginUrl + refreshToken;

        // Send the refresh token request
        var task = Task.Run(() => _httpClient.GetFromJsonAsync<AccessCredentials>(url));
        task.Wait();
        if (task.Result == null)
        {
            throw new SystemException("Refresh token failed. Using previous token " + refreshToken);
        }

        // Save the credentials and their expiry timestamp.
        _credentials = task.Result;
        _credentialsExpiry = DateTime.Now.AddSeconds(_credentials.expires_in);

        // Update the authorization header for the HTTP client
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(_credentials.token_type, _credentials.access_token);
    }

    public List<Account> GetAccounts()
    {
        // We always check whether the credentials expired
        if (DateTime.Now.CompareTo(_credentialsExpiry) >= 0)
        {
            init(_credentials.refresh_token);
        }

        // Build URL
        string url = _credentials.api_server + GetAccountsApiEndpoint;

        // Send request
        var task = Task.Run(() => _httpClient.GetFromJsonAsync<Accounts>(url, _jsonSerializeOptions));
        task.Wait();
        if (task.Result == null)
        {
            throw new SystemException("Get Accounts failed");
        }

        return task.Result.accounts;
    }

    // Get account activities for the selected time span. The interval cannot be greater than 31 days.
    public List<Activity> GetAccountActivities(string accountId, DateTime startTime, DateTime endTime)
    {
        // Before anything, check if the interval is more than 31 days or if the start date is after the end date
        if (endTime.CompareTo(startTime) <= 0 ||
           endTime.Subtract(startTime).CompareTo(TimeSpan.FromDays(31)) > 0)
        {
            throw new ArgumentException("Invalid time interval");
        }

        // We always check whether the credentials expired
        if (DateTime.Now.CompareTo(_credentialsExpiry) >= 0)
        {
            init(_credentials.refresh_token);
        }

        // Build URL
        string url = _credentials.api_server + GetAccountsApiEndpoint + "/" +
            accountId + GetAccountActivitiesApiEndpoint + "?" +
            StartTimeToken + startTime.ToString("o") + "&" +
            EndTimeToken + endTime.ToString("o") + "&";

        // Send request
        var task = Task.Run(() => _httpClient.GetFromJsonAsync<Activities>(url, _jsonSerializeOptions));
        task.Wait();
        if (task.Result == null)
        {
            throw new SystemException("Get Account Activity failed");
        }

        return task.Result.activities;
    }

    // Get orders for the given account. Optional parameters may be used for filtering.
    public List<Order> GetAccountOrders(
        string accountId,
        List<ulong>? orderIds = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        OrderStateFilterType? stateFilter = null
    )
    {
        if (startTime.HasValue && endTime.HasValue)
        {
            // Before anything, check if the start date is after the end date
            if (endTime.Value.CompareTo(startTime.Value) <= 0)
            {
                throw new ArgumentException("Invalid time interval");
            }
        }

        // We always check whether the credentials expired
        if (DateTime.Now.CompareTo(_credentialsExpiry) >= 0)
        {
            init(_credentials.refresh_token);
        }

        // Build URL
        string url = _credentials.api_server + GetAccountsApiEndpoint + "/" +
            accountId + GetAccountOrdersApiEndpoint;

        // If we have a single order, add it to the end of the URL
        if (orderIds != null && orderIds.Count == 1)
        {
            url += "/" + orderIds[0];
        }

        // If we have parameters to pass, add them
        if ((orderIds != null && orderIds.Count > 1) ||
            startTime.HasValue ||
            endTime.HasValue ||
            stateFilter.HasValue)
        {
            url += "?";

            if (orderIds != null && orderIds.Count > 1)
            {
                url += OrderIdsToken + string.Join(",", orderIds) + "&";
            }

            if (startTime.HasValue)
            {
                url += StartTimeToken + startTime.Value.ToString("o") + "&";
            }

            if (endTime.HasValue)
            {
                url += EndTimeToken + endTime.Value.ToString("o") + "&";
            }

            if (stateFilter.HasValue)
            {
                url += StateFilterToken + stateFilter.Value.ToString() + "&";
            }
        }

        // Send request
        var task = Task.Run(() => _httpClient.GetFromJsonAsync<Orders>(url, _jsonSerializeOptions));
        task.Wait();
        if (task.Result == null)
        {
            throw new SystemException("Get Account Orders failed");
        }

        return task.Result.orders;
    }

    // Get executions for the account. By default, startTime is the beginning of the current day, and endTime is the end of the current day.
    public List<Execution> GetAccountExecutions(string accountId, DateTime? startTime = null, DateTime? endTime = null)
    {
        if (startTime.HasValue && endTime.HasValue)
        {
            // Before anything, check if the start date is after the end date
            if (endTime.Value.CompareTo(startTime.Value) <= 0)
            {
                throw new ArgumentException("Invalid time interval");
            }
        }

        // We always check whether the credentials expired
        if (DateTime.Now.CompareTo(_credentialsExpiry) >= 0)
        {
            init(_credentials.refresh_token);
        }

        // Build URL
        string url = _credentials.api_server + GetAccountsApiEndpoint + "/" +
            accountId + GetAccountExecutionsApiEndpoint;

        // If we have parameters to pass, add them
        if (startTime.HasValue || endTime.HasValue)
        {
            url += "?";

            if (startTime.HasValue)
            {
                url += StartTimeToken + startTime.Value.ToString("o") + "&";
            }

            if (endTime.HasValue)
            {
                url += EndTimeToken + endTime.Value.ToString("o") + "&";
            }
        }

        // Send request
        var task = Task.Run(() => _httpClient.GetFromJsonAsync<Executions>(url, _jsonSerializeOptions));
        task.Wait();
        if (task.Result == null)
        {
            throw new SystemException("Get Account Executions failed");
        }

        return task.Result.executions;
    }

    // Get balances for the account.
    public Balances GetAccountBalances(string accountId)
    {
        // We always check whether the credentials expired
        if (DateTime.Now.CompareTo(_credentialsExpiry) >= 0)
        {
            init(_credentials.refresh_token);
        }

        // Build URL
        string url = _credentials.api_server + GetAccountsApiEndpoint + "/" +
            accountId + GetAccountBalancesApiEndpoint;

        // Send request
        var task = Task.Run(() => _httpClient.GetFromJsonAsync<Balances>(url, _jsonSerializeOptions));
        task.Wait();
        if (task.Result == null)
        {
            throw new SystemException("Get Account Balances failed");
        }

        return task.Result;
    }
    
    // Get positions for the account.
    public Balances GetAccountPositions(string accountId)
    {
        // We always check whether the credentials expired
        if (DateTime.Now.CompareTo(_credentialsExpiry) >= 0)
        {
            init(_credentials.refresh_token);
        }

        // Build URL
        string url = _credentials.api_server + GetAccountsApiEndpoint + "/" +
            accountId + GetAccountExecutionsApiEndpoint;

        // Send request
        var task = Task.Run(() => _httpClient.GetFromJsonAsync<Balances>(url, _jsonSerializeOptions));
        task.Wait();
        if (task.Result == null)
        {
            throw new SystemException("Get Account Balances failed");
        }

        return task.Result;
    }
}
