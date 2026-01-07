<<<<<<< HEAD
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ShareItems_WebApp/ShareItems_WebApp.csproj", "ShareItems_WebApp/"]
RUN dotnet restore "ShareItems_WebApp/ShareItems_WebApp.csproj"

COPY . .
WORKDIR "/src/ShareItems_WebApp"
RUN dotnet build "ShareItems_WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShareItems_WebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
=======
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ShareItems_WebApp/ShareItems_WebApp.csproj", "ShareItems_WebApp/"]
RUN dotnet restore "ShareItems_WebApp/ShareItems_WebApp.csproj"

COPY . .
WORKDIR "/src/ShareItems_WebApp"
RUN dotnet build "ShareItems_WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShareItems_WebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
>>>>>>> 925a86ee4a457b3fc262c4859f324e97d9027c80
ENTRYPOINT ["dotnet", "ShareItems_WebApp.dll"] 