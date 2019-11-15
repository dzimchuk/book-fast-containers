FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src

COPY ["Common/", "Common/"]
COPY ["BookFast.Search/", "BookFast.Search/"]
COPY ["BookFast.Search.Adapter/", "BookFast.Search.Adapter/"]
COPY ["BookFast.Search.Contracts/", "BookFast.Search.Contracts/"]

RUN dotnet restore "BookFast.Search/BookFast.Search.csproj"

WORKDIR "/src/BookFast.Search"
RUN dotnet build "BookFast.Search.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookFast.Search.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookFast.Search.dll"]