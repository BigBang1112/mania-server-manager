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

EXPOSE 2350/tcp
EXPOSE 2350/udp
EXPOSE 3450/tcp
EXPOSE 3450/udp

WORKDIR /app
COPY --from=build /app .
COPY --chown=$APP_UID --chmod=755 entrypoint.sh .

RUN mkdir ./data
RUN chown -R $APP_UID:$APP_UID ./data
RUN chmod -R u+w ./data

USER $APP_UID
ENTRYPOINT ["./entrypoint.sh"]
