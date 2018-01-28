FROM microsoft/aspnetcore-build:2.0
ADD . /app
WORKDIR /app
RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]
EXPOSE 80:8081
ENTRYPOINT ["dotnet", "run", "--server.urls", "http://*:80"]
