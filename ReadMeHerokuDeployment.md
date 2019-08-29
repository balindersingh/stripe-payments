# Overview
* We will build and publish .net core app locally
* Create a docker image for the published files.
* We only need asp.net core run time here instead of the whole image of microsoft/aspnetcore since we are using a published version of our app instead building it on heroku.
* then we will register that docker image on heroku and then just publish that container as is.

## Let's start with building and publishing our app to a published folder
dotnet publish -c Release
* It will publish under bin folder with all dlls etc
* Make sure you have provided Dockerfile in same working directory under publish
## Then build docker image using this command
docker build -t stripeappimage ./bin/release/netcoreapp2.2/publish

## login to heroku
heroku login
heroku container:login

## create a app on heroku i.e. my-heroku-app

## We need to tag the heroku target image
docker tag stripeappimage registry.heroku.com/my-heroku-app/web

## Push the docker image
docker push registry.heroku.com/my-heroku-app/web

## Finally make the container live
heroku container:release web -a my-heroku-app