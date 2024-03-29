FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["Common/", "Common/"]
COPY ["BookFast.Booking/", "BookFast.Booking/"]
COPY ["BookFast.Booking.Domain/", "BookFast.Booking.Domain/"]
COPY ["BookFast.Booking.CommandStack/", "BookFast.Booking.CommandStack/"]
COPY ["BookFast.Booking.QueryStack/", "BookFast.Booking.QueryStack/"]
COPY ["BookFast.Booking.Data/", "BookFast.Booking.Data/"]
COPY ["BookFast.Facility.Rpc/Protos/facility.proto", "BookFast.Facility.Rpc/Protos/facility.proto"]

RUN dotnet restore "BookFast.Booking/BookFast.Booking.csproj"

WORKDIR "/src/BookFast.Booking"
RUN dotnet build "BookFast.Booking.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookFast.Booking.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookFast.Booking.dll"]