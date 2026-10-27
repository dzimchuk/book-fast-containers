FROM mcr.microsoft.com/mssql-tools18:latest

WORKDIR /migrations

COPY Identity.sql PropertyManagement.sql Booking.sql OpenIddictClients.sql /migrations/
COPY entrypoint.sh /usr/local/bin/entrypoint.sh

RUN chmod +x /usr/local/bin/entrypoint.sh

USER 10001

ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
