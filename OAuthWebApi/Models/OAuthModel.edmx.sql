
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 09/07/2012 16:50:06
-- Generated from EDMX file: c:\users\steven\documents\visual studio 2010\Projects\OAuthWebApi\OAuthWebApi\Models\OAuthModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [OAuthWebAPI];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_UserClientAuthorization]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ClientAuthorizations] DROP CONSTRAINT [FK_UserClientAuthorization];
GO
IF OBJECT_ID(N'[dbo].[FK_ClientClientAuthorization]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ClientAuthorizations] DROP CONSTRAINT [FK_ClientClientAuthorization];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO
IF OBJECT_ID(N'[dbo].[ClientAuthorizations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ClientAuthorizations];
GO
IF OBJECT_ID(N'[dbo].[Clients]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Clients];
GO
IF OBJECT_ID(N'[dbo].[Nonces]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Nonces];
GO
IF OBJECT_ID(N'[dbo].[SymmetricCryptoKeys]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SymmetricCryptoKeys];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Username] nvarchar(max)  NOT NULL,
    [Password] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'ClientAuthorizations'
CREATE TABLE [dbo].[ClientAuthorizations] (
    [AuthorizationId] int IDENTITY(1,1) NOT NULL,
    [CreatedOnUtc] datetime  NOT NULL,
    [ClientId] int  NOT NULL,
    [UserId] int  NOT NULL,
    [Scope] nvarchar(max)  NOT NULL,
    [ExpirationDateUtc] datetime  NULL,
    [User_Id] int  NOT NULL,
    [Client_ClientId] int  NOT NULL
);
GO

-- Creating table 'Clients'
CREATE TABLE [dbo].[Clients] (
    [ClientId] int IDENTITY(1,1) NOT NULL,
    [ClientIdentifier] nvarchar(50)  NOT NULL,
    [ClientSecret] nvarchar(50)  NULL,
    [Callback] nvarchar(max)  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [ClientType] int  NOT NULL
);
GO

-- Creating table 'Nonces'
CREATE TABLE [dbo].[Nonces] (
    [Context] nvarchar(4000)  NOT NULL,
    [Code] nvarchar(4000)  NOT NULL,
    [Timestamp] datetime  NOT NULL
);
GO

-- Creating table 'SymmetricCryptoKeys'
CREATE TABLE [dbo].[SymmetricCryptoKeys] (
    [Bucket] nvarchar(4000)  NOT NULL,
    [Handle] nvarchar(4000)  NOT NULL,
    [ExpiresUtc] datetime  NOT NULL,
    [Secret] varbinary(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [AuthorizationId] in table 'ClientAuthorizations'
ALTER TABLE [dbo].[ClientAuthorizations]
ADD CONSTRAINT [PK_ClientAuthorizations]
    PRIMARY KEY CLUSTERED ([AuthorizationId] ASC);
GO

-- Creating primary key on [ClientId] in table 'Clients'
ALTER TABLE [dbo].[Clients]
ADD CONSTRAINT [PK_Clients]
    PRIMARY KEY CLUSTERED ([ClientId] ASC);
GO

-- Creating primary key on [Context], [Code], [Timestamp] in table 'Nonces'
ALTER TABLE [dbo].[Nonces]
ADD CONSTRAINT [PK_Nonces]
    PRIMARY KEY CLUSTERED ([Context], [Code], [Timestamp] ASC);
GO

-- Creating primary key on [Bucket], [Handle] in table 'SymmetricCryptoKeys'
ALTER TABLE [dbo].[SymmetricCryptoKeys]
ADD CONSTRAINT [PK_SymmetricCryptoKeys]
    PRIMARY KEY CLUSTERED ([Bucket], [Handle] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [User_Id] in table 'ClientAuthorizations'
ALTER TABLE [dbo].[ClientAuthorizations]
ADD CONSTRAINT [FK_UserClientAuthorization]
    FOREIGN KEY ([User_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserClientAuthorization'
CREATE INDEX [IX_FK_UserClientAuthorization]
ON [dbo].[ClientAuthorizations]
    ([User_Id]);
GO

-- Creating foreign key on [Client_ClientId] in table 'ClientAuthorizations'
ALTER TABLE [dbo].[ClientAuthorizations]
ADD CONSTRAINT [FK_ClientClientAuthorization]
    FOREIGN KEY ([Client_ClientId])
    REFERENCES [dbo].[Clients]
        ([ClientId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ClientClientAuthorization'
CREATE INDEX [IX_FK_ClientClientAuthorization]
ON [dbo].[ClientAuthorizations]
    ([Client_ClientId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------