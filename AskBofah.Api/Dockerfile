# ============================================================
# BUILD STAGE
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore (cached layer)
COPY ["AskBofah.Api/AskBofah.Api.csproj", "AskBofah.Api/"]
RUN dotnet restore "AskBofah.Api/AskBofah.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/AskBofah.Api"
RUN dotnet publish "AskBofah.Api.csproj" -c Release -o /app/publish

# ============================================================
# RUNTIME STAGE
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AskBofah.Api.dll"]