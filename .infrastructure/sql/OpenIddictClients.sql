-- Applies the environment-specific OpenIddict clients and redirect URIs.
-- Invariant client/scope data is seeded by the Identity EF Core migration.
--
-- Run with sqlcmd, supplying the four service base URLs without a trailing slash, e.g.
--   sqlcmd -b -i OpenIddictClients.sql \
--     -v IdentityUrl="https://bookfast-identity-dev.example.azurecontainerapps.io" \
--        PropertyManagementUrl="https://bookfast-property-management-dev.example.azurecontainerapps.io" \
--        SearchUrl="https://bookfast-search-dev.example.azurecontainerapps.io" \
--        BookingUrl="https://bookfast-booking-dev.example.azurecontainerapps.io" \
--        ProviderAppUrl="https://bookfast-provider-app-dev.example.azurecontainerapps.io" \
--        ProviderAppSecretHash="<hash, see .infrastructure/README.md>" \
--        GuestAppUrl="https://bookfast-guest-app-dev.example.azurecontainerapps.io" \
--        GuestAppSecretHash="<hash, see .infrastructure/README.md>"
--
-- Each app's URL and secret hash may both be empty: that app's client is then skipped.

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @identityUrl            nvarchar(400) = N'$(IdentityUrl)';
DECLARE @propertyManagementUrl  nvarchar(400) = N'$(PropertyManagementUrl)';
DECLARE @searchUrl              nvarchar(400) = N'$(SearchUrl)';
DECLARE @bookingUrl             nvarchar(400) = N'$(BookingUrl)';
DECLARE @providerAppUrl         nvarchar(400) = N'$(ProviderAppUrl)';
DECLARE @providerAppSecretHash  nvarchar(max)  = N'$(ProviderAppSecretHash)';
DECLARE @guestAppUrl            nvarchar(400) = N'$(GuestAppUrl)';
DECLARE @guestAppSecretHash     nvarchar(max)  = N'$(GuestAppSecretHash)';

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

-- The apps' BFF clients: confidential, authorization code + PKCE, refresh tokens.
DECLARE @apps TABLE (
    client_id    nvarchar(100),
    id           uniqueidentifier,
    display_name nvarchar(100),
    app_scope    nvarchar(50),
    app_url      nvarchar(400),
    secret_hash  nvarchar(max));

INSERT INTO @apps VALUES
    ('provider-app', '019b0a1e-4d51-7e3b-9f62-7a5c8b0d2e14', 'BookFast Provider app', 'provider', @providerAppUrl, @providerAppSecretHash),
    ('guest-app',    '019b1c2f-6a83-7d14-8b05-3e9f4a7c1d28', 'BookFast Guest app',    'customer', @guestAppUrl,    @guestAppSecretHash);

DECLARE @clientId nvarchar(100), @id uniqueidentifier, @displayName nvarchar(100), @appScope nvarchar(50), @appUrl nvarchar(400), @secretHash nvarchar(max);

DECLARE app_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT client_id, id, display_name, app_scope, app_url, secret_hash FROM @apps;

OPEN app_cursor;
FETCH NEXT FROM app_cursor INTO @clientId, @id, @displayName, @appScope, @appUrl, @secretHash;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF @appUrl = N'' AND @secretHash = N''
    BEGIN
        PRINT CONCAT(@clientId, ' not configured (its URL and secret hash are empty), skipped.');
    END
    ELSE
    BEGIN
        IF @appUrl = N'' OR @secretHash = N''
        BEGIN
            DECLARE @message nvarchar(200) = CONCAT(@clientId, ': the app URL and the secret hash must be supplied together.');
            THROW 50000, @message, 1;
        END

        DECLARE @appRedirectUris nvarchar(max) = N'["' + @appUrl + N'/signin-oidc"]';
        DECLARE @appPostLogoutRedirectUris nvarchar(max) = N'["' + @appUrl + N'/signout-callback-oidc"]';

        IF NOT EXISTS (SELECT 1 FROM [identity].[openiddict_applications] WHERE client_id = @clientId)
        BEGIN
            INSERT INTO [identity].[openiddict_applications]
                (id, client_id, client_secret, client_type, consent_type, display_name, concurrency_token, permissions, requirements, redirect_uris, post_logout_redirect_uris)
            VALUES
                (@id, @clientId, @secretHash, 'confidential', 'implicit', @displayName, NEWID(),
                 N'["ept:authorization","ept:end_session","ept:token","gt:authorization_code","gt:refresh_token","rst:code","scp:email","scp:profile","scp:roles","scp:offline_access","scp:' + @appScope + N'"]',
                 N'["ft:pkce"]',
                 @appRedirectUris, @appPostLogoutRedirectUris);

            PRINT CONCAT(@clientId, ' client created.');
        END
        ELSE
        BEGIN
            UPDATE [identity].[openiddict_applications]
            SET client_secret = @secretHash,
                redirect_uris = @appRedirectUris,
                post_logout_redirect_uris = @appPostLogoutRedirectUris,
                concurrency_token = CONVERT(nvarchar(50), NEWID())
            WHERE client_id = @clientId
              AND (client_secret IS NULL
                   OR client_secret <> @secretHash
                   OR redirect_uris IS NULL
                   OR redirect_uris <> @appRedirectUris
                   OR post_logout_redirect_uris IS NULL
                   OR post_logout_redirect_uris <> @appPostLogoutRedirectUris);

            PRINT CONCAT(@clientId, ' client up to date (', @@ROWCOUNT, ' row(s) updated).');
        END
    END

    FETCH NEXT FROM app_cursor INTO @clientId, @id, @displayName, @appScope, @appUrl, @secretHash;
END

CLOSE app_cursor;
DEALLOCATE app_cursor;