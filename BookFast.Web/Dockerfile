FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src

COPY ["Common/", "Common/"]
COPY ["BookFast.Web/", "BookFast.Web/"]
COPY ["BookFast.Web.Contracts/", "BookFast.Web.Contracts/"]
COPY ["BookFast.Web.Proxy/", "BookFast.Web.Proxy/"]

RUN dotnet restore "BookFast.Web/BookFast.Web.csproj"

WORKDIR "/src/BookFast.Web"
RUN dotnet build "BookFast.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookFast.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookFast.Web.dll"]