# aiia-sample

Sample ASP.NET Core web app apps showcasing a basic usage and feature of Aiia.

## Introduction

The sample app implements some very basic features of Aiia to get you familiar to how it works.

It's focused more on developers, and it's is still work in progress. Our goal is to showcase all of the Aiia features in this project.

You can find Aiia documentation at <https://docs.aiia.eu/>

## Web

### Running locally

1) You'll need to create an account at <https://portal.aiia.eu/> and generate a `ClientId` and `ClientSecret`.
  1) As a webhook url specify https://<hostname>/aiia/webhook if your website is publicly reachable. If this is not the case it can also be left empty, but you won't receive any webhook in that case.
  1) As a redirect url https://<hostname>/aiia/callback. You can also use localhost for this field (remember the port number!)

1) Download and install [.NET Core](https://dotnet.microsoft.com/download) 6.

1) Run the `initialize.sh` or `initialize.ps1` scripts in the repository root to setup the project. 

   - The script will ask you the `ClientId` and `ClientSecret` you previously configured.
   - The webhook secret is optional and can be left empty.
   - FOR SANDBOX ONLY: Note that the project is configured by default to use the production APIs for Aiia.
     If you are only enabled for sandbox you need to open `Web/appsettings.json` and change the `BaseApiUrl` into `https://api-sandbox.aiia.eu`.

1) Open the .sln project file in your favorite C# development environment and start the project, or run the following
   `cd Web; dotnet run`

### Basic Flow to add your account to the app:

1. Navigate to the sample app: <http://localhost:5020>
2. Register
3. Navigate to `Accounts` page using `Accounts` button in the navigation bar
4. Click the `Connect Accounts` link
5. Go through the Aiia flow, logging in into your bank.
6. At the end you'll be redirected to sample app's callback page with some explanation of what's happening behind the scenes
8. When you navigate back to the Accounts page will be not empty anymore
9. Press on one of the accounts in the list
10. You'll be shown the transactions list (if transaction page is empty, that's fine, we fetch transaction data asynchronously so it's not always there at the beginning, but we notify clients when that's done via [webhook](https://api.aiia.eu/docs/index.html#section/Webhooks)
11. If running the project not through our webpage you can also try out payments.
