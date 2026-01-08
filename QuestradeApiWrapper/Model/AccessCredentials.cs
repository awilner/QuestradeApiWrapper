namespace QuestradeApiWrapper.Model;

// https://www.questrade.com/api/documentation/authorization
public class AccessCredentials
{
    public required string access_token { get; set; }
    public required string api_server { get; set; }
    public required string refresh_token { get; set; }
    public required string token_type { get; set; }
    public required int expires_in { get; set; }
}