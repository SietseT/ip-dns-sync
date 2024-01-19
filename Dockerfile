FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS publish
ARG Version
WORKDIR /src

# COPY ["src/PublicDnsUpdater/PublicDnsUpdater.csproj", "PublicDnsUpdater/"]
# RUN dotnet restore "PublicDnsUpdater/PublicDnsUpdater.csproj" -r linux-x64
# COPY ["src/PublicDnsUpdater", "PublicDnsUpdater/"]
# WORKDIR "/src/PublicDnsUpdater"

# RUN dotnet publish "PublicDnsUpdater.csproj" -c Release -o /app/publish \
#     --no-restore \
#     -r linux-x64 \
#     --self-contained true \
#     /p:PublishTrimmed=true \
#     /p:PublishSingleFile=true \
#     /p:Version=$Version \
#     /p:InformationalVersion=$Version

COPY src/PublicDnsUpdater/PublicDnsUpdater.csproj ./
RUN dotnet restore "./PublicDnsUpdater.csproj"
COPY src/PublicDnsUpdater .
RUN dotnet publish "PublicDnsUpdater.csproj" -c Release -o /app/publish \
  --no-restore \
  --self-contained true \
  /p:PublishTrimmed=true \
  /p:PublishSingleFile=true

# use different image
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS final
WORKDIR /app

# create a new user and change directory ownership
RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app

USER dotnetuser
COPY --chown=dotnetuser --from=publish /app/publish .
RUN rm -f appsettings.Development.json


ENTRYPOINT ["./PublicDnsUpdater"]
