# This stage is used to build the service project
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH=amd64
ARG RUNTIME=linux-musl-x64
ARG BUILD_VERSION="1.1.1.1"
ARG BUILD_SUFFIX="RC"

ENV BUILD_VERSION=$BUILD_VERSION
ENV BUILD_SUFFIX=$BUILD_SUFFIX

EXPOSE 8000
EXPOSE 7040

WORKDIR /src
# #11 [build 7/7] RUN dotnet publish  -p:Version=1.1.1.1 -p:InformationalVersion=1.1.1.1 -p:VersionSuffix=RC -a amd64  --no-restore -o /app

# Copy project file and restore as distinct layers
COPY --link ./*.csproj .
RUN dotnet restore -a $TARGETARCH

# Copy source code and publish app
COPY --link ./ .
RUN dotnet build  /p:Version=${BUILD_VERSION} /p:InformationalVersion=${BUILD_VERSION} /p:VersionSuffix=${BUILD_SUFFIX} -a $TARGETARCH  --no-restore
RUN dotnet publish  /p:Version=${BUILD_VERSION} /p:InformationalVersion=${BUILD_VERSION} /p:VersionSuffix=${BUILD_SUFFIX} -a $TARGETARCH  --no-restore -o /app


# Enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md
# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

ENV \
  DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
  LC_ALL=en_US.UTF-8 \
  LANG=en_US.UTF-8
RUN apk add --no-cache \
  icu-data-full \
  icu-libs

WORKDIR /app
COPY --link --from=build /app .
USER $APP_UID
ENTRYPOINT ["./ProxysqlAdminUi.Web"]