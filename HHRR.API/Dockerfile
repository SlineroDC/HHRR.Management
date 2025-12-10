FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HHRR.API/HHRR.API.csproj", "HHRR.API/"]
COPY ["HHRR.Infrastructure/HHRR.Infrastructure.csproj", "HHRR.Infrastructure/"]
COPY ["HHRR.Application/HHRR.Application.csproj", "HHRR.Application/"]
COPY ["HHRR.Core/HHRR.Core.csproj", "HHRR.Core/"]
RUN dotnet restore "HHRR.API/HHRR.API.csproj"
COPY . .
WORKDIR "/src/HHRR.API"
RUN dotnet build "HHRR.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HHRR.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HHRR.API.dll"]
