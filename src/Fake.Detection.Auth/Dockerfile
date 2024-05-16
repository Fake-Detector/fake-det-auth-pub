FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5146;https://+:7146
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="123456"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/fakeauth.pfx
COPY ["fakeauth.pfx", "/https/fakeauth.pfx"]

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Fake.Detection.Auth/Fake.Detection.Auth.csproj", "src/Fake.Detection.Auth/"]
RUN dotnet nuget add source "https://nuget.pkg.github.com/Fake-Detector/index.json" -n github -u <USER> -p <PASSWORD> --store-password-in-clear-text
RUN dotnet restore "src/Fake.Detection.Auth/Fake.Detection.Auth.csproj"
COPY . .
WORKDIR "/src/src/Fake.Detection.Auth"
RUN dotnet build "Fake.Detection.Auth.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Fake.Detection.Auth.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fake.Detection.Auth.dll"]
