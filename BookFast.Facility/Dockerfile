FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["BookFast.Facility/BookFast.Facility.csproj", "BookFast.Facility/"]
RUN dotnet restore "BookFast.Facility/BookFast.Facility.csproj"
COPY . .
WORKDIR "/src/BookFast.Facility"
RUN dotnet build "BookFast.Facility.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BookFast.Facility.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BookFast.Facility.dll"]