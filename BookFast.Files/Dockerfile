FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["Common/", "Common/"]
COPY ["BookFast.Files/", "BookFast.Files/"]
COPY ["BookFast.Files.Business/", "BookFast.Files.Business/"]
COPY ["BookFast.Files.Contracts/", "BookFast.Files.Contracts/"]
COPY ["BookFast.Files.Data/", "BookFast.Files.Data/"]
COPY ["BookFast.Facility.Rpc/Protos/facility.proto", "BookFast.Facility.Rpc/Protos/facility.proto"]

RUN dotnet restore "BookFast.Files/BookFast.Files.csproj"

WORKDIR "/src/BookFast.Files"
RUN dotnet build "BookFast.Files.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookFast.Files.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookFast.Files.dll"]