FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
RUN apt-get update && \
    apt-get install -y tzdata && \
    rm -rf /var/lib/apt/lists/*
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["global.json", "./"]
COPY ["HoaTheoMua.API/HoaTheoMua.API.csproj", "HoaTheoMua.API/"]
COPY ["HoaTheoMua.Service/HoaTheoMua.Service.csproj", "HoaTheoMua.Service/"]
COPY ["HoaTheoMua.Repository/HoaTheoMua.Repository.csproj", "HoaTheoMua.Repository/"]
RUN dotnet restore "HoaTheoMua.API/HoaTheoMua.API.csproj"
COPY . .
WORKDIR "/src/HoaTheoMua.API"
RUN dotnet build "HoaTheoMua.API.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:TreatWarningsAsErrors=false

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HoaTheoMua.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:TreatWarningsAsErrors=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HoaTheoMua.API.dll"]
