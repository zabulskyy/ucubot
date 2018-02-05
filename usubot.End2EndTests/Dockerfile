FROM microsoft/aspnetcore-build:2.0 AS build-env

ADD . /app
WORKDIR /app

RUN dotnet restore

ENTRYPOINT ["dotnet", "test", "-v", "m", "--filter", "TestCategory=Assignment1"]
