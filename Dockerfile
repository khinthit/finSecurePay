# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/FinSecurePay.Api/FinSecurePay.Api.csproj src/FinSecurePay.Api/
RUN dotnet restore src/FinSecurePay.Api/FinSecurePay.Api.csproj
COPY src/ src/
RUN dotnet publish src/FinSecurePay.Api/FinSecurePay.Api.csproj \
    -c Release -o /app/publish --no-restore

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
ARG APP_VERSION=dev
ENV APP_VERSION=${APP_VERSION}
WORKDIR /app
COPY --from=build /app/publish .
# Built-in non-root "app" user provided by the .NET images
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "FinSecurePay.Api.dll"]
