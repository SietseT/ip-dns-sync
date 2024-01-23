FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS publish

WORKDIR /src
COPY app/IpDnsSync/IpDnsSync.csproj ./
RUN dotnet restore "./IpDnsSync.csproj"
COPY app/IpDnsSync .
RUN dotnet publish "IpDnsSync.csproj" -c Release -o /app/publish \
  --no-restore \
  --self-contained true \
  /p:PublishTrimmed=true \
  /p:PublishSingleFile=true


FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS final
WORKDIR /app

RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app

USER dotnetuser
COPY --chown=dotnetuser --from=publish /app/publish .
RUN rm -f appsettings.Development.json

ENTRYPOINT ["./IpDnsSync"]