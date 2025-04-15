FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Auth.WebApi/Auth.WebApi.csproj"
COPY . .
WORKDIR "/src/Auth.WebApi"
RUN dotnet build "Auth.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auth.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY Auth.WebApi/.env .
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.WebApi.dll"]
