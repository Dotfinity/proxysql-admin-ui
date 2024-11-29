# This stage is used to build the service project
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH=amd64
ARG RUNTIME=linux-musl-x64
ARG BUILD_VERSION=1.1.1.1
ARG BUILD_SUFFIX=RC
EXPOSE 8000
EXPOSE 7040
ENV BUILD_VERSION=$BUILD_VERSION
WORKDIR /src

COPY ["ProxysqlAdminUi.Web.csproj", "./"]
RUN dotnet restore --runtime $RUNTIME "./ProxysqlAdminUi.Web.csproj"
COPY . .
WORKDIR "/src"

RUN dotnet build "./ProxysqlAdminUi.Web.csproj" -c $BUILD_CONFIGURATION --runtime $RUNTIME -o /app/build

RUN dotnet publish "./ProxysqlAdminUi.Web.csproj" --runtime $RUNTIME --no-restore -c $BUILD_CONFIGURATION --self-contained -o /app/publish /p:UseAppHost=true 


# Enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md
# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine


ENV \
  DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
  LC_ALL=en_US.UTF-8 \
  LANG=en_US.UTF-8
RUN apk add --no-cache \
  icu-data-full \
  icu-libs


WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./ProxysqlAdminUi.Web"]