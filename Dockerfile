# **********************************************************************************************************************
# BUILD IMAGE
FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine AS build-env-oauth2
# ADD BASH, see https://github.com/dotnet/dotnet-docker/issues/632
RUN apk update && apk add --no-cache bash
# PREFETCH FSharp.Core and System.Net.Http INTO SEPARATE LAYER
COPY ./NuGet.Config /tmp/prefetch/
RUN mkdir -p /tmp/prefetch \
    && cd /tmp/prefetch \
    && dotnet new web \
    && dotnet add package NCoreUtils.IO --version 3.0.0 --no-restore \
    && dotnet add package NCoreUtils.Data.Protocol --version 3.0.0 --no-restore \
    && dotnet add package System.Buffers --version 4.5.0 --no-restore \
    && dotnet add package System.Memory --version 4.5.3 --no-restore \
    && dotnet add package System.Numerics.Vectors --version 4.5.0 --no-restore \
    && dotnet add package System.Net.Http --version 4.3.4 --no-restore \
    && dotnet add package System.Runtime.CompilerServices.Unsafe --version 4.6.0 --no-restore \
    && dotnet add package System.Threading.Thread --version 4.3.0 --no-restore \
    && dotnet add package System.Threading.ThreadPool --version 4.3.0 --no-restore \
    && dotnet add package System.Threading.Tasks.Extensions --version 4.5.3 --no-restore \
    && dotnet add package System.Threading.Tasks.Parallel --version 4.3.0 --no-restore \
    && dotnet add package Microsoft.NETCore.Platforms --version 3.0.0 --no-restore \
    && dotnet add package Microsoft.NETCore.Targets --version 3.0.0 --no-restore \
    && dotnet add package Newtonsoft.Json --version 12.0.2 --no-restore \
    && dotnet add package NETStandard.Library --version 2.0.3 --no-restore \
    && dotnet add package NCoreUtils.Data.EntityFrameworkCore --version 3.0.0 --no-restore \
    && dotnet add package Antlr4.Runtime --version 4.6.6 --no-restore \
    && dotnet restore -r alpine-x64 -v n
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
COPY ./NCoreUtils.OAuth2.Encryption.Local/NCoreUtils.OAuth2.Encryption.Local.fsproj ./NCoreUtils.OAuth2.Encryption.Local/NCoreUtils.OAuth2.Encryption.Local.fsproj
COPY ./NCoreUtils.OAuth2.Shared/NCoreUtils.OAuth2.Shared.fsproj ./NCoreUtils.OAuth2.Shared/NCoreUtils.OAuth2.Shared.fsproj
COPY ./NCoreUtils.OAuth2.Middleware/NCoreUtils.OAuth2.Middleware.fsproj ./NCoreUtils.OAuth2.Middleware/NCoreUtils.OAuth2.Middleware.fsproj
COPY ./NCoreUtils.OAuth2.Rest/NCoreUtils.OAuth2.Rest.fsproj ./NCoreUtils.OAuth2.Rest/NCoreUtils.OAuth2.Rest.fsproj
COPY ./NCoreUtils.OAuth2.WebService/NCoreUtils.OAuth2.WebService.fsproj ./NCoreUtils.OAuth2.WebService/NCoreUtils.OAuth2.WebService.fsproj
# RESTORE PACKAGES
RUN dotnet restore ./NCoreUtils.OAuth2.WebService/NCoreUtils.OAuth2.WebService.fsproj -r alpine-x64 -v n
# COPY SOURCES
COPY ./NCoreUtils.OAuth2.Abstractions/*.fs ./NCoreUtils.OAuth2.Abstractions/
COPY ./NCoreUtils.OAuth2.Authentication/*.fs ./NCoreUtils.OAuth2.Authentication/
COPY ./NCoreUtils.OAuth2.Authentication.Abstractions/*.fs ./NCoreUtils.OAuth2.Authentication.Abstractions/
COPY ./NCoreUtils.OAuth2.Core/*.fs ./NCoreUtils.OAuth2.Core/
COPY ./NCoreUtils.OAuth2.Core.Abstractions/*.fs ./NCoreUtils.OAuth2.Core.Abstractions/
COPY ./NCoreUtils.OAuth2.Data/*.fs ./NCoreUtils.OAuth2.Data/
COPY ./NCoreUtils.OAuth2.Data.EntityFrameworkCore/*.cs ./NCoreUtils.OAuth2.Data.EntityFrameworkCore/
COPY ./NCoreUtils.OAuth2.Data.EntityFrameworkCore/Migrations ./NCoreUtils.OAuth2.Data.EntityFrameworkCore/Migrations/
COPY ./NCoreUtils.OAuth2.Encryption.Local/*.fs ./NCoreUtils.OAuth2.Encryption.Local/
COPY ./NCoreUtils.OAuth2.Shared/*.fs ./NCoreUtils.OAuth2.Shared/
COPY ./NCoreUtils.OAuth2.Middleware/*.fs ./NCoreUtils.OAuth2.Middleware/
COPY ./NCoreUtils.OAuth2.Rest/*.fs ./NCoreUtils.OAuth2.Rest/
COPY ./NCoreUtils.OAuth2.WebService/*.fs ./NCoreUtils.OAuth2.WebService/
# PUBLISH APPLICATION
RUN dotnet publish ./NCoreUtils.OAuth2.WebService/NCoreUtils.OAuth2.WebService.fsproj -c Release -r alpine-x64 --no-restore -o /app/out

# **********************************************************************************************************************
# RUNTIME IMAGE
FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.0-alpine
WORKDIR /app
# INSTALL GLOBALIZATION and CURL
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT false
# libgrpc_csharp_ext.x64.so miatt
RUN apk update && apk add --no-cache libc6-compat
RUN apk update && apk add --no-cache icu-libs curl
# SETUP ENVIRONMENT
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:80
# COPY APP
COPY --from=build-env-oauth2 /app/out ./
# ENTRY POINT
ENTRYPOINT ["./NCoreUtils.OAuth2.WebService"]

