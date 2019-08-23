# Stripe 3D Secure 2 demo with .net core
## Pre-requisite
* Stripe test account
* ngrok account (optional if you want to deploy this to some public hosting). I am using it just to make my locally hosted application public since Stripe needs a callback endpoint to send some info.
## Config variables
* SecretKey : secret key of your stripe account 
* RedirectUrl : the url where this application will be hosted publicly so that stripe can use it as callback url
## Development
* Let's start listening on 5000 port to ngrok endpoint (you can change it in settings files to listen on diffrent port)
* ```./ngrok http 5000```
* Make sure you update Stripe settings appropriately in appsettings.json
## Build and run
* Now you just need to run following command to see the application in action
* ```dotnet build && dotnet run```