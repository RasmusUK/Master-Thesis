FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SpotQuoteBooking.EventSource.Web/SpotQuoteBooking.EventSource.Web.csproj", "SpotQuoteBooking.EventSource.Web/"]
RUN dotnet restore "SpotQuoteBooking.EventSource.Web/SpotQuoteBooking.EventSource.Web.csproj"
COPY . .
WORKDIR "/src/SpotQuoteBooking.EventSource.Web"
RUN dotnet build "SpotQuoteBooking.EventSource.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SpotQuoteBooking.EventSource.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SpotQuoteBooking.EventSource.Web.dll"]
