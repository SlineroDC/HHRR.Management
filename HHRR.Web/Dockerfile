FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HHRR.Web/HHRR.Web.csproj", "HHRR.Web/"]
COPY ["HHRR.Infrastructure/HHRR.Infrastructure.csproj", "HHRR.Infrastructure/"]
COPY ["HHRR.Application/HHRR.Application.csproj", "HHRR.Application/"]
COPY ["HHRR.Core/HHRR.Core.csproj", "HHRR.Core/"]
RUN dotnet restore "HHRR.Web/HHRR.Web.csproj"
COPY . .
WORKDIR "/src/HHRR.Web"
RUN dotnet build "HHRR.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HHRR.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HHRR.Web.dll"]
