
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 05/20/2013 12:44:41
-- Generated from EDMX file: D:\Work\SmsSurvey\smsSurvey.dbInterface\smsSurvey.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [smsSurvey];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO


-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------
-- Creating table 'QuestionSet'
CREATE TABLE [dbo].[QuestionSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Text] nvarchar(160)  NOT NULL,
    [Order] int  NOT NULL,
    [Type] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'SurveyPlanSet'
CREATE TABLE [dbo].[SurveyPlanSet] (
    [Id] int IDENTITY(1,1) NOT NULL
);
GO

-- Creating table 'CustomerSet'
CREATE TABLE [dbo].[CustomerSet] (
    [PhoneNumber] nvarchar(50)  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [Surname] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'SurveyResultSet'
CREATE TABLE [dbo].[SurveyResultSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DateRan] datetime  NOT NULL,
    [CustomerPhoneNumber] nvarchar(50)  NOT NULL,
    [SurveyPlanId] int  NOT NULL
);
GO

-- Creating table 'ResultSet'
CREATE TABLE [dbo].[ResultSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Answer] nvarchar(500)  NOT NULL,
    [QuestionId] int  NOT NULL,
    [SurveyResultId] int  NOT NULL
);
GO

-- Creating table 'SurveyPlanQuestion'
CREATE TABLE [dbo].[SurveyPlanQuestion] (
    [SurveyPlan_Id] int  NOT NULL,
    [Questions_Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'QuestionSet'
ALTER TABLE [dbo].[QuestionSet]
ADD CONSTRAINT [PK_QuestionSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'SurveyPlanSet'
ALTER TABLE [dbo].[SurveyPlanSet]
ADD CONSTRAINT [PK_SurveyPlanSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [PhoneNumber] in table 'CustomerSet'
ALTER TABLE [dbo].[CustomerSet]
ADD CONSTRAINT [PK_CustomerSet]
    PRIMARY KEY CLUSTERED ([PhoneNumber] ASC);
GO

-- Creating primary key on [Id] in table 'SurveyResultSet'
ALTER TABLE [dbo].[SurveyResultSet]
ADD CONSTRAINT [PK_SurveyResultSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ResultSet'
ALTER TABLE [dbo].[ResultSet]
ADD CONSTRAINT [PK_ResultSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [SurveyPlan_Id], [Questions_Id] in table 'SurveyPlanQuestion'
ALTER TABLE [dbo].[SurveyPlanQuestion]
ADD CONSTRAINT [PK_SurveyPlanQuestion]
    PRIMARY KEY NONCLUSTERED ([SurveyPlan_Id], [Questions_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [SurveyPlan_Id] in table 'SurveyPlanQuestion'
ALTER TABLE [dbo].[SurveyPlanQuestion]
ADD CONSTRAINT [FK_SurveyPlanQuestion_SurveyPlan]
    FOREIGN KEY ([SurveyPlan_Id])
    REFERENCES [dbo].[SurveyPlanSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Questions_Id] in table 'SurveyPlanQuestion'
ALTER TABLE [dbo].[SurveyPlanQuestion]
ADD CONSTRAINT [FK_SurveyPlanQuestion_Question]
    FOREIGN KEY ([Questions_Id])
    REFERENCES [dbo].[QuestionSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SurveyPlanQuestion_Question'
CREATE INDEX [IX_FK_SurveyPlanQuestion_Question]
ON [dbo].[SurveyPlanQuestion]
    ([Questions_Id]);
GO

-- Creating foreign key on [CustomerPhoneNumber] in table 'SurveyResultSet'
ALTER TABLE [dbo].[SurveyResultSet]
ADD CONSTRAINT [FK_CustomerSurveyResult]
    FOREIGN KEY ([CustomerPhoneNumber])
    REFERENCES [dbo].[CustomerSet]
        ([PhoneNumber])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CustomerSurveyResult'
CREATE INDEX [IX_FK_CustomerSurveyResult]
ON [dbo].[SurveyResultSet]
    ([CustomerPhoneNumber]);
GO

-- Creating foreign key on [SurveyPlanId] in table 'SurveyResultSet'
ALTER TABLE [dbo].[SurveyResultSet]
ADD CONSTRAINT [FK_SurveyPlanSurveyResult]
    FOREIGN KEY ([SurveyPlanId])
    REFERENCES [dbo].[SurveyPlanSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SurveyPlanSurveyResult'
CREATE INDEX [IX_FK_SurveyPlanSurveyResult]
ON [dbo].[SurveyResultSet]
    ([SurveyPlanId]);
GO

-- Creating foreign key on [QuestionId] in table 'ResultSet'
ALTER TABLE [dbo].[ResultSet]
ADD CONSTRAINT [FK_QuestionResult]
    FOREIGN KEY ([QuestionId])
    REFERENCES [dbo].[QuestionSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_QuestionResult'
CREATE INDEX [IX_FK_QuestionResult]
ON [dbo].[ResultSet]
    ([QuestionId]);
GO

-- Creating foreign key on [SurveyResultId] in table 'ResultSet'
ALTER TABLE [dbo].[ResultSet]
ADD CONSTRAINT [FK_SurveyResultResult]
    FOREIGN KEY ([SurveyResultId])
    REFERENCES [dbo].[SurveyResultSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SurveyResultResult'
CREATE INDEX [IX_FK_SurveyResultResult]
ON [dbo].[ResultSet]
    ([SurveyResultId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------