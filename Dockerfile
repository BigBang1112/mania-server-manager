FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/nightly/sdk:9.0-noble-aot AS build
ARG TARGETARCH
WORKDIR /src

# Copy project file and restore as distinct layers
COPY ManiaServerManager/*.csproj .
RUN dotnet restore -r linux-$TARGETARCH

# Copy source code and publish app
COPY ManiaServerManager/. .
RUN dotnet publish --no-restore -o /app
RUN rm /app/*.dbg


# Final stage/image
FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-noble

WORKDIR /app
COPY --from=build /app .

RUN mkdir -p /app/data/archives /app/data/versions && \
    chown -R $APP_UID:$APP_UID /app/data/archives /app/data/versions && \
    chmod -R 775 /app/data/archives /app/data/versions

EXPOSE 2350/tcp 2350/udp 3450/tcp 3450/udp

USER $APP_UID

COPY --chmod=0755 entrypoint.sh .
ENTRYPOINT ["./entrypoint.sh"]
