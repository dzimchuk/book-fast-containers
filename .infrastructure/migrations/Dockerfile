FROM ubuntu:24.04

# Microsoft does not publish an mssql-tools18 image, so install sqlcmd from the Microsoft package feed
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl \
    && curl -sSL -o /tmp/packages-microsoft-prod.deb https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb \
    && dpkg -i /tmp/packages-microsoft-prod.deb \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y --no-install-recommends mssql-tools18 libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/* /tmp/packages-microsoft-prod.deb

# Check if the ODBC driver is missing a shared library dependency
RUN ! ldd /opt/microsoft/msodbcsql18/lib64/libmsodbcsql-*.so.* | grep "not found"

WORKDIR /migrations

COPY Identity.sql PropertyManagement.sql Booking.sql OpenIddictClients.sql /migrations/
COPY entrypoint.sh /usr/local/bin/entrypoint.sh

RUN chmod +x /usr/local/bin/entrypoint.sh

USER 10001

ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
