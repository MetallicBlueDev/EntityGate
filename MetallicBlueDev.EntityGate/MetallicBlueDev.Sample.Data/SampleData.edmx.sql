
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 10/26/2019 16:10:40
-- Generated from EDMX file: L:\Repos\MetallicBlueDev\EntityGate\MetallicBlueDev.EntityGate\MetallicBlueDev.Sample.Data\SampleData.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [SampleDbTest];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------


-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'SampleEntity1Set'
CREATE TABLE [dbo].[SampleEntity1Set] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Value] int  NOT NULL
);
GO

-- Creating table 'OtherEntitySet'
CREATE TABLE [dbo].[OtherEntitySet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Description] nvarchar(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'SampleEntity1Set'
ALTER TABLE [dbo].[SampleEntity1Set]
ADD CONSTRAINT [PK_SampleEntity1Set]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'OtherEntitySet'
ALTER TABLE [dbo].[OtherEntitySet]
ADD CONSTRAINT [PK_OtherEntitySet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------