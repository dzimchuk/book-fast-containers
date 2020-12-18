FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src

COPY ["Common/", "Common/"]
COPY ["BookFast.Search.Indexer/", "BookFast.Search.Indexer/"]
COPY ["BookFast.Search.Adapter/", "BookFast.Search.Adapter/"]
COPY ["BookFast.Search.Contracts/", "BookFast.Search.Contracts/"]
COPY ["BookFast.Facility.Client/", "BookFast.Facility.Client/"]

RUN dotnet restore "BookFast.Search.Indexer/BookFast.Search.Indexer.csproj"

WORKDIR "/src/BookFast.Search.Indexer"
RUN dotnet build "BookFast.Search.Indexer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookFast.Search.Indexer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookFast.Search.Indexer.dll"]