FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 443
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["SpotQuoteApp/src/SpotQuoteApp.Web/SpotQuoteApp.Web.csproj", "SpotQuoteApp/src/SpotQuoteApp.Web/"]
COPY ["EventSourcingFramework/src/EventSourcingFramework.Infrastructure/EventSourcingFramework.Infrastructure.csproj", "EventSourcingFramework/src/EventSourcingFramework.Infrastructure/"]

RUN dotnet restore "SpotQuoteApp/src/SpotQuoteApp.Web/SpotQuoteApp.Web.csproj"
COPY . .
WORKDIR "/src/SpotQuoteApp/src/SpotQuoteApp.Web"
RUN dotnet build "SpotQuoteApp.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SpotQuoteApp.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SpotQuoteApp.Web.dll"]
