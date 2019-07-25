FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["BookFast.Facility/BookFast.Facility.csproj", "BookFast.Facility/"]
COPY ["Common/BookFast.ServiceBus/BookFast.ServiceBus.csproj", "Common/BookFast.ServiceBus/"]
COPY ["Common/BookFast.SeedWork/BookFast.SeedWork.csproj", "Common/BookFast.SeedWork/"]
COPY ["Common/BookFast.Security/BookFast.Security.csproj", "Common/BookFast.Security/"]
COPY ["Common/BookFast.Api/BookFast.Api.csproj", "Common/BookFast.Api/"]
COPY ["BookFast.Facility.Data/BookFast.Facility.Data.csproj", "BookFast.Facility.Data/"]
COPY ["BookFast.Facility.QueryStack/BookFast.Facility.QueryStack.csproj", "BookFast.Facility.QueryStack/"]
COPY ["BookFast.Facility.CommandStack/BookFast.Facility.CommandStack.csproj", "BookFast.Facility.CommandStack/"]
COPY ["BookFast.Facility.Domain/BookFast.Facility.Domain.csproj", "BookFast.Facility.Domain/"]
COPY ["Common/BookFast.ReliableEvents/BookFast.ReliableEvents.csproj", "Common/BookFast.ReliableEvents/"]
COPY ["Common/BookFast.DistributedMutex/BookFast.DistributedMutex.csproj", "Common/BookFast.DistributedMutex/"]
COPY ["Common/BookFast.Configuration/BookFast.Configuration.csproj", "Common/BookFast.Configuration/"]
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
