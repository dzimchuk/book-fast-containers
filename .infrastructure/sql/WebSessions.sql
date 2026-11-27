-- The web apps' session stores: one ASP.NET Core SQL Server distributed cache table per app.
-- They hold the users' server-side tokens (encrypted with the app's Data Protection keys)
-- and the persisted state of paused Blazor circuits, so every replica of an app sees the same sessions.

IF SCHEMA_ID(N'web') IS NULL
    EXEC(N'CREATE SCHEMA [web] AUTHORIZATION [dbo]');

IF OBJECT_ID(N'[web].[GuestAppCache]', N'U') IS NULL
BEGIN
    CREATE TABLE [web].[GuestAppCache] (
        [Id] nvarchar(449) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
        [Value] varbinary(max) NOT NULL,
        [ExpiresAtTime] datetimeoffset NOT NULL,
        [SlidingExpirationInSeconds] bigint NULL,
        [AbsoluteExpiration] datetimeoffset NULL,
        CONSTRAINT [PK_GuestAppCache] PRIMARY KEY ([Id])
    );
    CREATE NONCLUSTERED INDEX [IX_GuestAppCache_ExpiresAtTime] ON [web].[GuestAppCache] ([ExpiresAtTime]);
END;

IF OBJECT_ID(N'[web].[ProviderAppCache]', N'U') IS NULL
BEGIN
    CREATE TABLE [web].[ProviderAppCache] (
        [Id] nvarchar(449) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
        [Value] varbinary(max) NOT NULL,
        [ExpiresAtTime] datetimeoffset NOT NULL,
        [SlidingExpirationInSeconds] bigint NULL,
        [AbsoluteExpiration] datetimeoffset NULL,
        CONSTRAINT [PK_ProviderAppCache] PRIMARY KEY ([Id])
    );
    CREATE NONCLUSTERED INDEX [IX_ProviderAppCache_ExpiresAtTime] ON [web].[ProviderAppCache] ([ExpiresAtTime]);
END;
