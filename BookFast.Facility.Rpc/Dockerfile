FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["Common/", "Common/"]
COPY ["BookFast.Facility.Rpc/", "BookFast.Facility.Rpc/"]
COPY ["BookFast.Facility.Domain/", "BookFast.Facility.Domain/"]
COPY ["BookFast.Facility.CommandStack/", "BookFast.Facility.CommandStack/"]
COPY ["BookFast.Facility.QueryStack/", "BookFast.Facility.QueryStack/"]
COPY ["BookFast.Facility.Data/", "BookFast.Facility.Data/"]

RUN dotnet restore "BookFast.Facility.Rpc/BookFast.Facility.Rpc.csproj"

WORKDIR "/src/BookFast.Facility.Rpc"
RUN dotnet build "BookFast.Facility.Rpc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookFast.Facility.Rpc.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookFast.Facility.Rpc.dll"]
