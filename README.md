# viia-sample

Sample ASP.NET web app showcasing a basic usage and feature of Viia.

Note: More interested in using Viia in a mobile app? Check out our Flutter sample project - https://github.com/spiir/viia-sample-mobile

## Introduction

This sample (or demo if you prefer) app implement some very basic features of Viia to get you familiar to how it works.

It's focused more on developers, so you'll notice some things that are not very good UX or design choices that are made hoping to explain what's happening in some of the steps.

It is still work in progress, our goal is to showcase all of the Viia features in this project.

You can find Viia documentation at https://api.getviia.com/docs/index.html

## Running locally

For starters, you'll need to get access to Viia, currently this is possible only by contacting us at support@nordicapigateway.com .

When you have access, you'll need to update `Viia` configuration section in `appsettings.json`:

```
...
  "Viia": {
    ...
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "WebHookSecret": "your-webhook-secret-if-configured"
  }
...
```

Other sections are unnecessary to modify to run the project locally.

At this point you should be good to go as long as you have .NET Core ( https://dotnet.microsoft.com/download ) installed.

## Basic Flow to go through:

1. Navigate to the sample app ( http://localhost:5020 )
2. Register
3. Navigate to `Accounts` page using `Accounts` button in the navigation bar
4. Press either `Connect More Accounts via iframe` or `Connect More Accounts via redirect` it showcases 2 different types of flows
5. Go through the Viia flow
6. At the end you'll be redirected to sample app's callback page with some explanation of what's happening behind the scenes
7. Press the appropriate button for the flow you went through in the callback page
8. You'll be navigated back to the Accounts page, which will be not empty anymore
9. Press on one of the accounts in the list
10. You'll be navigated to transactions page (if transaction page is empty, that's fine, we fetch transaction data asynchronously so it's not always there at the beginning, but we notify clients when that's done via webhook https://api.getviia.com/docs/index.html#section/Webhooks )

## License

MIT License

Copyright (c) 2019 Spiir A/S

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