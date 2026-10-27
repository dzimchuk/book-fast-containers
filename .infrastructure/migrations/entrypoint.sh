#!/bin/sh

set -eu

: "${SQL_CONNECTION_STRING:?SQL_CONNECTION_STRING must be set}"
: "${IDENTITY_URL:?IDENTITY_URL must be set}"
: "${PROPERTY_MANAGEMENT_URL:?PROPERTY_MANAGEMENT_URL must be set}"
: "${SEARCH_URL:?SEARCH_URL must be set}"
: "${BOOKING_URL:?BOOKING_URL must be set}"

SQLCMD=/opt/mssql-tools18/bin/sqlcmd

# sqlcmd takes discrete connection parameters, so split the ADO.NET connection string into the
# SQLCMD* variables it reads from the environment. Note this means the password cannot contain ';'.
connection_string_value() {
    printf '%s' "$SQL_CONNECTION_STRING" | tr ';' '\n' | awk -v key="$1" '
        {
            k = $0
            sub(/=.*$/, "", k)
            gsub(/^[ \t]+|[ \t]+$/, "", k)
            if (tolower(k) == tolower(key)) {
                v = $0
                sub(/^[^=]*=/, "", v)
                print v
                exit
            }
        }'
}

SQLCMDSERVER=$(connection_string_value "Server")
SQLCMDDBNAME=$(connection_string_value "Initial Catalog")
SQLCMDUSER=$(connection_string_value "User ID")
SQLCMDPASSWORD=$(connection_string_value "Password")
export SQLCMDSERVER SQLCMDDBNAME SQLCMDUSER SQLCMDPASSWORD

if [ -z "$SQLCMDSERVER" ] || [ -z "$SQLCMDDBNAME" ]; then
    echo "Could not parse Server/Initial Catalog out of SQL_CONNECTION_STRING" >&2
    exit 1
fi

echo "==> Target: ${SQLCMDSERVER}/${SQLCMDDBNAME}"

# -b aborts on the first error so a failed batch fails the job replica; -t 0 disables the query timeout
for script in Identity.sql PropertyManagement.sql Booking.sql; do
    echo "==> Applying ${script}"
    "$SQLCMD" -b -I -t 0 -i "/migrations/${script}"
done

echo "==> Applying OpenIddict client configuration"
"$SQLCMD" -b -I -t 0 -i /migrations/OpenIddictClients.sql \
    -v IdentityUrl="$IDENTITY_URL" \
       PropertyManagementUrl="$PROPERTY_MANAGEMENT_URL" \
       SearchUrl="$SEARCH_URL" \
       BookingUrl="$BOOKING_URL"

echo "==> Database migrations completed successfully"
