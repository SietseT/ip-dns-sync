FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS publish
ARG Version
WORKDIR /src
COPY ["src/PublicDnsUpdater/PublicDnsUpdater.csproj", "PublicDnsUpdater/"]
RUN dotnet restore "PublicDnsUpdater/PublicDnsUpdater.csproj" --runtime linux-x64
COPY . .
WORKDIR "/src/PublicDnsUpdater"

RUN dotnet publish "PublicDnsUpdater.csproj" -c Release -o /app/publish \
    --no-restore \
    --runtime linux-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:PublishSingleFile=true \
    /p:Version=$Version \
    /p:InformationalVersion=$Version 

# use different image
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS final

# create a new user and change directory ownership
RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app

USER dotnetuser
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["./PublicDnsUpdater"]
