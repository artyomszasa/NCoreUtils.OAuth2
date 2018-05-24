# **********************************************************************************************************************
# BUILD IMAGE
FROM microsoft/dotnet:2.0-sdk AS build-env-oauth2
WORKDIR /app
# COPY NUGET config
COPY ./NuGet.Config ./
# COPY PROJECT FILES
COPY ./NCoreUtils.OAuth2.Abstractions/NCoreUtils.OAuth2.Abstractions.fsproj ./NCoreUtils.OAuth2.Abstractions/NCoreUtils.OAuth2.Abstractions.fsproj
COPY ./NCoreUtils.OAuth2.Authentication/NCoreUtils.OAuth2.Authentication.fsproj ./NCoreUtils.OAuth2.Authentication/NCoreUtils.OAuth2.Authentication.fsproj
COPY ./NCoreUtils.OAuth2.Authentication.Abstractions/NCoreUtils.OAuth2.Authentication.Abstractions.fsproj ./NCoreUtils.OAuth2.Authentication.Abstractions/NCoreUtils.OAuth2.Authentication.Abstractions.fsproj
COPY ./NCoreUtils.OAuth2.Core/NCoreUtils.OAuth2.Core.fsproj ./NCoreUtils.OAuth2.Core/NCoreUtils.OAuth2.Core.fsproj
COPY ./NCoreUtils.OAuth2.Core.Abstractions/NCoreUtils.OAuth2.Core.Abstractions.fsproj ./NCoreUtils.OAuth2.Core.Abstractions/NCoreUtils.OAuth2.Core.Abstractions.fsproj
COPY ./NCoreUtils.OAuth2.Data/NCoreUtils.OAuth2.Data.fsproj ./NCoreUtils.OAuth2.Data/NCoreUtils.OAuth2.Data.fsproj
COPY ./NCoreUtils.OAuth2.Data.EntityFrameworkCore/NCoreUtils.OAuth2.Data.EntityFrameworkCore.csproj ./NCoreUtils.OAuth2.Data.EntityFrameworkCore/NCoreUtils.OAuth2.Data.EntityFrameworkCore.csproj
COPY ./NCoreUtils.OAuth2.Encryption.Google/NCoreUtils.OAuth2.Encryption.Google.fsproj ./NCoreUtils.OAuth2.Encryption.Google/NCoreUtils.OAuth2.Encryption.Google.fsproj
COPY ./NCoreUtils.OAuth2.Middleware/NCoreUtils.OAuth2.Middleware.fsproj ./NCoreUtils.OAuth2.Middleware/NCoreUtils.OAuth2.Middleware.fsproj
COPY ./NCoreUtils.OAuth2.Rest/NCoreUtils.OAuth2.Rest.fsproj ./NCoreUtils.OAuth2.Rest/NCoreUtils.OAuth2.Rest.fsproj
COPY ./NCoreUtils.OAuth2.WebService/NCoreUtils.OAuth2.WebService.fsproj ./NCoreUtils.OAuth2.WebService/NCoreUtils.OAuth2.WebService.fsproj
# RESTORE PACKAGES
RUN dotnet restore ./NCoreUtils.OAuth2.WebService/NCoreUtils.OAuth2.WebService.fsproj -r linux-x64
# COPY SOURCES
COPY ./NCoreUtils.OAuth2.Abstractions/*.fs ./NCoreUtils.OAuth2.Abstractions/
COPY ./NCoreUtils.OAuth2.Authentication/*.fs ./NCoreUtils.OAuth2.Authentication/
COPY ./NCoreUtils.OAuth2.Authentication.Abstractions/*.fs ./NCoreUtils.OAuth2.Authentication.Abstractions/
COPY ./NCoreUtils.OAuth2.Core/*.fs ./NCoreUtils.OAuth2.Core/
COPY ./NCoreUtils.OAuth2.Core.Abstractions/*.fs ./NCoreUtils.OAuth2.Core.Abstractions/
COPY ./NCoreUtils.OAuth2.Data/*.fs ./NCoreUtils.OAuth2.Data/
COPY ./NCoreUtils.OAuth2.Data.EntityFrameworkCore/*.cs ./NCoreUtils.OAuth2.Data.EntityFrameworkCore/
COPY ./NCoreUtils.OAuth2.Data.EntityFrameworkCore/Migrations ./NCoreUtils.OAuth2.Data.EntityFrameworkCore/Migrations/
COPY ./NCoreUtils.OAuth2.Encryption.Google/*.fs ./NCoreUtils.OAuth2.Encryption.Google/
COPY ./NCoreUtils.OAuth2.Middleware/*.fs ./NCoreUtils.OAuth2.Middleware/
COPY ./NCoreUtils.OAuth2.Rest/*.fs ./NCoreUtils.OAuth2.Rest/
COPY ./NCoreUtils.OAuth2.WebService/*.fs ./NCoreUtils.OAuth2.WebService/
# PUBLISH APPLICATION
RUN dotnet publish ./NCoreUtils.OAuth2.WebService/NCoreUtils.OAuth2.WebService.fsproj -c Release -r linux-x64 --no-restore -o /app/out

# **********************************************************************************************************************
# RUNTIME IMAGE
FROM microsoft/dotnet:2.0-runtime-deps
WORKDIR /app
# INSTALL CURL
RUN apt-get update && apt-get install -y curl
# SETUP ENVIRONMENT
ENV ASPNETCORE_ENVIRONMENT=Production
# COPY APP
COPY --from=build-env-oauth2 /app/out ./
# ENTRY POINT
ENTRYPOINT ["./NCoreUtils.OAuth2.WebService", "--tcp=0.0.0.0:80"]

