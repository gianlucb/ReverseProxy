#FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
FROM microsoft/dotnet:latest AS base
WORKDIR /app
EXPOSE 8080


#FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
FROM microsoft/dotnet:latest AS build
WORKDIR /src
COPY ["ReverseProxy/ReverseProxy.csproj", "ReverseProxy/"]
RUN dotnet restore "ReverseProxy/ReverseProxy.csproj"
COPY . .
WORKDIR "/src/ReverseProxy"
RUN dotnet build "ReverseProxy.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ReverseProxy.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
COPY --from=publish /app/appsettings.yml ./appsettings.yml
ENTRYPOINT ["dotnet", "ReverseProxy.dll"]