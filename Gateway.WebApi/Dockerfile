FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Gateway.WebApi/Gateway.WebApi.csproj"
COPY . .
WORKDIR "/src/Gateway.WebApi"
RUN dotnet build "Gateway.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gateway.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gateway.WebApi.dll"]
