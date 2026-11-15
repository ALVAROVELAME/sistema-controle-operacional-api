FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY *.csproj .

RUN dotnet restore sistema-controle-operacional-api.csproj

COPY . .

RUN dotnet publish sistema-controle-operacional-api.csproj -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet","sistema-controle-operacional-api.dll"]