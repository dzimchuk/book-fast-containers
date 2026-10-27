-- Applies the environment-specific OpenIddict redirect URIs.
-- Invariant client/scope data is seeded by the Identity EF Core migration.
--
-- Run with sqlcmd, supplying the four service base URLs without a trailing slash, e.g.
--   sqlcmd -b -i OpenIddictClients.sql \
--     -v IdentityUrl="https://bookfast-identity-dev.example.azurecontainerapps.io" \
--        PropertyManagementUrl="https://bookfast-property-management-dev.example.azurecontainerapps.io" \
--        SearchUrl="https://bookfast-search-dev.example.azurecontainerapps.io" \
--        BookingUrl="https://bookfast-booking-dev.example.azurecontainerapps.io"

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @identityUrl            nvarchar(400) = N'$(IdentityUrl)';
DECLARE @propertyManagementUrl  nvarchar(400) = N'$(PropertyManagementUrl)';
DECLARE @searchUrl              nvarchar(400) = N'$(SearchUrl)';
DECLARE @bookingUrl             nvarchar(400) = N'$(BookingUrl)';

IF @identityUrl = N'' OR @propertyManagementUrl = N'' OR @searchUrl = N'' OR @bookingUrl = N''
BEGIN
    THROW 50000, 'All four service URLs must be supplied via sqlcmd -v variables.', 1;
END

-- OpenIddict compares redirect URIs against Uri.AbsoluteUri, which keeps a trailing slash on authority-only URIs
DECLARE @redirectUris nvarchar(max) =
    N'["' + @identityUrl           + N'/swagger/oauth2-redirect.html","'
          + @propertyManagementUrl + N'/swagger/oauth2-redirect.html","'
          + @searchUrl             + N'/swagger/oauth2-redirect.html","'
          + @bookingUrl            + N'/swagger/oauth2-redirect.html"]';

DECLARE @postLogoutRedirectUris nvarchar(max) =
    N'["' + @identityUrl           + N'/","'
          + @propertyManagementUrl + N'/","'
          + @searchUrl             + N'/","'
          + @bookingUrl            + N'/"]';

IF NOT EXISTS (SELECT 1 FROM [identity].[openiddict_applications] WHERE client_id = 'swagger-ui')
BEGIN
    THROW 50000, 'The swagger-ui client is missing. Apply the Identity migrations before running this script.', 1;
END

UPDATE [identity].[openiddict_applications]
SET redirect_uris = @redirectUris,
    post_logout_redirect_uris = @postLogoutRedirectUris,
    concurrency_token = CONVERT(nvarchar(50), NEWID())
WHERE client_id = 'swagger-ui'
  AND (redirect_uris IS NULL
       OR redirect_uris <> @redirectUris
       OR post_logout_redirect_uris IS NULL
       OR post_logout_redirect_uris <> @postLogoutRedirectUris);

PRINT CONCAT('swagger-ui redirect URIs up to date (', @@ROWCOUNT, ' row(s) updated).');
