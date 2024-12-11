# This stage is used to build the service project
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH=amd64
ARG RUNTIME=linux-musl-x64
ARG BUILD_VERSION="0.0.0.0"
ARG BUILD_SUFFIX="LOCAL"

ENV BUILD_VERSION=$BUILD_VERSION \ 
    BUILD_SUFFIX=$BUILD_SUFFIX

EXPOSE 8000
EXPOSE 7040

WORKDIR /src

# Copy project file and restore as distinct layers
COPY --link ./*.csproj .
RUN dotnet restore -a $TARGETARCH

# Copy source code and publish app
COPY --link ./ .
#RUN dotnet build -v d /p:Version=${BUILD_VERSION} /p:InformationalVersion=${BUILD_VERSION} /p:VersionSuffix=${BUILD_SUFFIX} -a $TARGETARCH  --no-restore
RUN dotnet publish -v d -a $TARGETARCH  --no-restore -o /app


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