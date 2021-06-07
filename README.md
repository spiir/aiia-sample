# aiia-sample

Sample ASP.NET Core web app and mobile Flutter apps showcasing a basic usage and feature of Aiia.

## Introduction

The sample app implements some very basic features of Aiia to get you familiar to how it works.

It's focused more on developers, and it's is still work in progress. Our goal is to showcase all of the Aiia features in this project.

You can find Aiia documentation at <https://docs.aiia.eu/>

## Web

### Running locally

For starters, you'll need to create an account at <https://portal.aiia.eu/> and generate a `ClientId` and `ClientSecret`.

Then update `Aiia` configuration section in `appsettings.json`:

```
...
  "Aiia": {
    ...
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "WebHookSecret": "your-webhook-secret-if-configured"
  }
...
```

Other sections are unnecessary to modify to run the project locally.

At this point you should be good to go as long as you have [.NET Core](https://dotnet.microsoft.com/download) installed.

### Basic Flow to go through:

1. Navigate to the sample app: <http://localhost:5020>
2. Register
3. Navigate to `Accounts` page using `Accounts` button in the navigation bar
4. Press `Connect More Accounts via redirect` it showcases the redirect-flow
5. Go through the Aiia flow
6. At the end you'll be redirected to sample app's callback page with some explanation of what's happening behind the scenes
7. Press the appropriate button for the flow you went through in the callback page
8. You'll be navigated back to the Accounts page, which will be not empty anymore
9. Press on one of the accounts in the list
10. You'll be navigated to transactions page (if transaction page is empty, that's fine, we fetch transaction data asynchronously so it's not always there at the beginning, but we notify clients when that's done via [webhook](https://api.aiia.eu/docs/index.html#section/Webhooks)

## Mobile

### Running locally

Having Web app configured correctly, mobile apps don't require additional configuration to run locally. For running Flutter locally, please refer to [Flutter's get started guide](https://flutter.dev/docs/get-started/install).

### Basic Flow to go through:

1. Open the app, you'll be presented with minimal welcome page
2. Press `Login via Aiia`, this will open a webview with Aiia web app inside
3. Go through Aiia flow
4. When finished you'll be navigated to Accounts page, where you can see a list of accounts you just consented in the Aiia flow
5. Click on one of the accounts and you'll be navigated to transaction page with a list of transactions within the account (if transaction page is empty, that's fine, we fetch transaction data asynchronously so it's not always there at the beginning, but we notify clients when that's done via [webhook](https://api.aiia.eu/docs/index.html#section/Webhooks)

## License

MIT License

Copyright (c) 2021 Aiia A/S

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
