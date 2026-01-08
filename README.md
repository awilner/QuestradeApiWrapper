# Questrade API Wrapper for CSharp
A simple wrapper around the [Questrade API](https://www.questrade.com/api/documentation/getting-started). I implemented only the GET API endpoints documented in the link above, which are read-only. Other endpoints, available only to Questrade partners, are not covered.

So far only the account requests are implemented, market requests should be implemented at a later date. Since I don't intend to use these, this is low priority to me. Feel free to contribute and open a PR.


## Getting Started
In order to use the Questrade API, you need to create and authorize an app, following instructions in https://www.questrade.com/api/documentation/getting-started. Write down the token generated for you. Note that this token expires in 7 days. If it expires, you need to go into the Questrade API Centre and generate a new one.

To use this wrapper, just add the `QuestradeApiWrapper` NuGet package to your VSCode project.


## Authentication
The token you generated during onboarding to the Questrade API is what's called a refresh token. When you pass that token to the wrapper object by calling init(), the wrapper places a call to the Questrade API login server, which returns both an access token and a new refresh token.

Note that when "trading in" a refresh token for an access token, the refresh token is rendered invalid, and a subsequent call to request a new access token needs to use the new refresh token that was returned by the API together with the access token.

The wrapper stores that refresh token to be used again when the access token expires (it checks for expiry at the beginning of every method invoked, and automatically places another request to the login server if necessary). But in case your program shuts down or crashes, that refresh token will be lost, and you're going to have to generate another token on the API Centre.

In order to avoid this, you should call `refreshToken()` after every other call, and persist the current refresh token in a database or file.

## Usage
Instantiate an object, and initialize it with a valid refresh token (don't forget to persist the new refresh token):
```
HttpClient httpClient = new HttpClient();
Questrade questrade = new Questrade(httpClient);
questrade.init("my_refresh_token");
string saveThisRefreshToken = questrade.refreshToken();
```

Now you're ready to make calls to the API. I recommend retrieving the refresh token after every call and persisting if it changes, since any call may trigger a refresh:
```
Accounts accounts = questrade.GetAccounts();
string newRefreshToken = questrade.refreshToken();
if(!newRefreshToken.Equals(refreshToken))
{
    // Persist the new refresh token
}

// Now do something with the list of accounts you received from the API
```

## Feedback
If you notice a bug, feel free to open an issue.

## Disclaimer
NOTE - This library is not endorsed or supported by Questrade in any way, shape or form. This library is released under the MIT License.
