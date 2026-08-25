# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# It's important to keep lines from here down to 'COPY . .' or 'COPY ["src/", "src/"]' identical in all Dockerfiles
# to take advantage of Docker's build cache, to speed up local container builds

COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY "BookFast.slnx" "BookFast.slnx"

COPY "src/Common/BookFast.Common.Api/BookFast.Common.Api.csproj" "src/Common/BookFast.Common.Api/BookFast.Common.Api.csproj"
COPY "src/Common/BookFast.Common.Application/BookFast.Common.Application.csproj" "src/Common/BookFast.Common.Application/BookFast.Common.Application.csproj"
COPY "src/Common/BookFast.Common.Domain/BookFast.Common.Domain.csproj" "src/Common/BookFast.Common.Domain/BookFast.Common.Domain.csproj"
COPY "src/Common/BookFast.Common.Infrastructure/BookFast.Common.Infrastructure.csproj" "src/Common/BookFast.Common.Infrastructure/BookFast.Common.Infrastructure.csproj"
COPY "src/Common/BookFast.Common.Presentation/BookFast.Common.Presentation.csproj" "src/Common/BookFast.Common.Presentation/BookFast.Common.Presentation.csproj"
COPY "src/Common/BookFast.Common.SeedWork/BookFast.Common.SeedWork.csproj" "src/Common/BookFast.Common.SeedWork/BookFast.Common.SeedWork.csproj"

COPY "src/Services/Identity/BookFast.Identity/BookFast.Identity.csproj" "src/Services/Identity/BookFast.Identity/BookFast.Identity.csproj"
COPY "src/Services/Identity/BookFast.Identity.Core/BookFast.Identity.Core.csproj" "src/Services/Identity/BookFast.Identity.Core/BookFast.Identity.Core.csproj"
COPY "src/Services/Identity/BookFast.Identity.Infrastructure/BookFast.Identity.Infrastructure.csproj" "src/Services/Identity/BookFast.Identity.Infrastructure/BookFast.Identity.Infrastructure.csproj"

RUN dotnet restore "BookFast.slnx"

COPY ["src/", "src/"]

WORKDIR "/src/src/Services/Identity/BookFast.Identity"

RUN dotnet build "./BookFast.Identity.csproj" --no-restore -c $BUILD_CONFIGURATION

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BookFast.Identity.csproj" --no-build -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookFast.Identity.dll"]