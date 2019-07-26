FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src

COPY ["Common/", "Common/"]
COPY ["BookFast.Facility/", "BookFast.Facility/"]
COPY ["BookFast.Facility.Domain/", "BookFast.Facility.Domain/"]
COPY ["BookFast.Facility.CommandStack/", "BookFast.Facility.CommandStack/"]
COPY ["BookFast.Facility.QueryStack/", "BookFast.Facility.QueryStack/"]
COPY ["BookFast.Facility.Data/", "BookFast.Facility.Data/"]

RUN dotnet restore "BookFast.Facility/BookFast.Facility.csproj"

WORKDIR "/src/BookFast.Facility"
RUN dotnet build "BookFast.Facility.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BookFast.Facility.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BookFast.Facility.dll"]
