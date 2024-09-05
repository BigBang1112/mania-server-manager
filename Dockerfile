FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/nightly/sdk:9.0.100-preview.7-alpine3.20-aot AS build
ARG TARGETARCH
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# copy csproj and restore as distinct layers
COPY ManiaServerManager/*.csproj .
RUN dotnet restore -a $TARGETARCH

# copy and publish app and libraries
COPY ManiaServerManager/. .
RUN dotnet publish --no-restore -a $TARGETARCH -c $BUILD_CONFIGURATION -o /app

# Remove dbg files
RUN find /app -name '*.dbg' -delete

FROM mcr.microsoft.com/dotnet/nightly/runtime-deps:9.0-preview-alpine3.20-aot

EXPOSE 2350/tcp
EXPOSE 2350/udp
EXPOSE 3450/tcp
EXPOSE 3450/udp
EXPOSE 8080

WORKDIR /app
COPY --from=build /app .
# Uncomment to enable non-root user
# USER $APP_UID
ENTRYPOINT ["./ManiaServerManager"]