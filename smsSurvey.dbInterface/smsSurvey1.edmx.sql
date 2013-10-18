
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 10/17/2013 09:45:14
-- Generated from EDMX file: D:\Work\Txtfeedback\Repository Git\smsSurvey\smssurvey\smsSurvey.dbInterface\smsSurvey.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [smsSurvey];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_CustomerSurveyResult]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[SurveyResultSet] DROP CONSTRAINT [FK_CustomerSurveyResult];
GO
IF OBJECT_ID(N'[dbo].[FK_SurveyPlanSurveyResult]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[SurveyResultSet] DROP CONSTRAINT [FK_SurveyPlanSurveyResult];
GO
IF OBJECT_ID(N'[dbo].[FK_QuestionResult]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ResultSet] DROP CONSTRAINT [FK_QuestionResult];
GO
IF OBJECT_ID(N'[dbo].[FK_SurveyResultResult]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ResultSet] DROP CONSTRAINT [FK_SurveyResultResult];
GO
IF OBJECT_ID(N'[dbo].[FK_webpages_UsersInRoles_webpages_Roles]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[webpages_UsersInRoles] DROP CONSTRAINT [FK_webpages_UsersInRoles_webpages_Roles];
GO
IF OBJECT_ID(N'[dbo].[FK_webpages_UsersInRoles_UserProfile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[webpages_UsersInRoles] DROP CONSTRAINT [FK_webpages_UsersInRoles_UserProfile];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfileSurveyPlan_SurveyPlan]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserProfileSurveyPlan] DROP CONSTRAINT [FK_UserProfileSurveyPlan_SurveyPlan];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfileSurveyPlan_UserProfile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserProfileSurveyPlan] DROP CONSTRAINT [FK_UserProfileSurveyPlan_UserProfile];
GO
IF OBJECT_ID(N'[dbo].[FK_SurveyPlanQuestion]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[QuestionSet] DROP CONSTRAINT [FK_SurveyPlanQuestion];
GO
IF OBJECT_ID(N'[dbo].[FK_RunningSurvey]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CustomerSet] DROP CONSTRAINT [FK_RunningSurvey];
GO
IF OBJECT_ID(N'[dbo].[FK_SurveyResultQuestion]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[SurveyResultSet] DROP CONSTRAINT [FK_SurveyResultQuestion];
GO
IF OBJECT_ID(N'[dbo].[FK_CompanyTag]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Tags] DROP CONSTRAINT [FK_CompanyTag];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfileCompanies]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserProfile] DROP CONSTRAINT [FK_UserProfileCompanies];
GO
IF OBJECT_ID(N'[dbo].[FK_QuestionAlertAlertNotification]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AlertNotificationSet] DROP CONSTRAINT [FK_QuestionAlertAlertNotification];
GO
IF OBJECT_ID(N'[dbo].[FK_QuestionQuestionAlert]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[QuestionAlertSet] DROP CONSTRAINT [FK_QuestionQuestionAlert];
GO
IF OBJECT_ID(N'[dbo].[FK_SurveyResultTags_SurveyResult]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[SurveyResultTags] DROP CONSTRAINT [FK_SurveyResultTags_SurveyResult];
GO
IF OBJECT_ID(N'[dbo].[FK_SurveyResultTags_Tags]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[SurveyResultTags] DROP CONSTRAINT [FK_SurveyResultTags_Tags];
GO
IF OBJECT_ID(N'[dbo].[FK_TagsTagTypes_Tags]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TagsTagTypes] DROP CONSTRAINT [FK_TagsTagTypes_Tags];
GO
IF OBJECT_ID(N'[dbo].[FK_TagsTagTypes_TagTypes]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TagsTagTypes] DROP CONSTRAINT [FK_TagsTagTypes_TagTypes];
GO
IF OBJECT_ID(N'[dbo].[FK_SurveyTemplateTags]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Tags] DROP CONSTRAINT [FK_SurveyTemplateTags];
GO
IF OBJECT_ID(N'[dbo].[FK_QuestionAlertSetTags]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[QuestionAlertSet] DROP CONSTRAINT [FK_QuestionAlertSetTags];
GO
IF OBJECT_ID(N'[dbo].[FK_TagsTags_Tags]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TagsTags] DROP CONSTRAINT [FK_TagsTags_Tags];
GO
IF OBJECT_ID(N'[dbo].[FK_TagsTags_Tags1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TagsTags] DROP CONSTRAINT [FK_TagsTags_Tags1];
GO
IF OBJECT_ID(N'[dbo].[FK_CompaniesDevice]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DeviceSet] DROP CONSTRAINT [FK_CompaniesDevice];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[QuestionSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[QuestionSet];
GO
IF OBJECT_ID(N'[dbo].[SurveyTemplateSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SurveyTemplateSet];
GO
IF OBJECT_ID(N'[dbo].[CustomerSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CustomerSet];
GO
IF OBJECT_ID(N'[dbo].[SurveyResultSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SurveyResultSet];
GO
IF OBJECT_ID(N'[dbo].[ResultSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ResultSet];
GO
IF OBJECT_ID(N'[dbo].[UserProfile]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserProfile];
GO
IF OBJECT_ID(N'[dbo].[webpages_Membership]', 'U') IS NOT NULL
    DROP TABLE [dbo].[webpages_Membership];
GO
IF OBJECT_ID(N'[dbo].[webpages_OAuthMembership]', 'U') IS NOT NULL
    DROP TABLE [dbo].[webpages_OAuthMembership];
GO
IF OBJECT_ID(N'[dbo].[webpages_Roles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[webpages_Roles];
GO
IF OBJECT_ID(N'[dbo].[Companies]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Companies];
GO
IF OBJECT_ID(N'[dbo].[Tags]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Tags];
GO
IF OBJECT_ID(N'[dbo].[TagTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TagTypes];
GO
IF OBJECT_ID(N'[dbo].[AlertNotificationSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AlertNotificationSet];
GO
IF OBJECT_ID(N'[dbo].[QuestionAlertSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[QuestionAlertSet];
GO
IF OBJECT_ID(N'[dbo].[DeviceSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DeviceSet];
GO
IF OBJECT_ID(N'[dbo].[webpages_UsersInRoles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[webpages_UsersInRoles];
GO
IF OBJECT_ID(N'[dbo].[UserProfileSurveyPlan]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserProfileSurveyPlan];
GO
IF OBJECT_ID(N'[dbo].[SurveyResultTags]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SurveyResultTags];
GO
IF OBJECT_ID(N'[dbo].[TagsTagTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TagsTagTypes];
GO
IF OBJECT_ID(N'[dbo].[TagsTags]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TagsTags];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'QuestionSet'
CREATE TABLE [dbo].[QuestionSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Text] nvarchar(160)  NOT NULL,
    [Order] int  NOT NULL,
    [Type] nvarchar(50)  NOT NULL,
    [ValidAnswers] nvarchar(1000)  NULL,
    [ValidAnswersDetails] nvarchar(1000)  NULL,
    [SurveyPlan_Id] int  NOT NULL
);
GO

-- Creating table 'SurveyTemplateSet'
CREATE TABLE [dbo].[SurveyTemplateSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Description] nvarchar(100)  NOT NULL,
    [ThankYouMessage] nvarchar(160)  NOT NULL,
    [DateStarted] datetime  NULL,
    [DateEnded] datetime  NULL,
    [IsRunning] bit  NOT NULL,
    [Provider] nvarchar(50)  NOT NULL,
    [IntroMessage] nvarchar(160)  NOT NULL,
    [DefaultLanguage] nvarchar(10)  NOT NULL
);
GO

-- Creating table 'CustomerSet'
CREATE TABLE [dbo].[CustomerSet] (
    [PhoneNumber] nvarchar(50)  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [Surname] nvarchar(50)  NOT NULL,
    [SurveyInProgress] bit  NOT NULL,
    [SurveyPlan_Id] int  NULL,
    [Email] nvarchar(100)  NULL
);
GO

-- Creating table 'SurveyResultSet'
CREATE TABLE [dbo].[SurveyResultSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DateRan] datetime  NOT NULL,
    [CustomerPhoneNumber] nvarchar(50)  NOT NULL,
    [SurveyPlanId] int  NOT NULL,
    [Terminated] bit  NOT NULL,
    [CurrentQuestion_Id] int  NULL,
    [PercentageComplete] float  NOT NULL,
    [LanguageChosenForSurvey] nvarchar(10)  NULL
);
GO

-- Creating table 'ResultSet'
CREATE TABLE [dbo].[ResultSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Answer] nvarchar(500)  NOT NULL,
    [QuestionId] int  NOT NULL,
    [SurveyResultId] int  NOT NULL,
    [AdditionalInfo] nvarchar(500)  NULL,
    [DateSubmitted] datetime  NULL,
    [SubmittedViaSMS] bit  NOT NULL
);
GO

-- Creating table 'UserProfile'
CREATE TABLE [dbo].[UserProfile] (
    [UserId] int IDENTITY(1,1) NOT NULL,
    [UserName] nvarchar(56)  NOT NULL,
    [DefaultProvider] nvarchar(50)  NULL,
    [DefaultTelNo] nvarchar(50)  NOT NULL,
    [Company_Name] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'webpages_Membership'
CREATE TABLE [dbo].[webpages_Membership] (
    [UserId] int  NOT NULL,
    [CreateDate] datetime  NULL,
    [ConfirmationToken] nvarchar(128)  NULL,
    [IsConfirmed] bit  NULL,
    [LastPasswordFailureDate] datetime  NULL,
    [PasswordFailuresSinceLastSuccess] int  NOT NULL,
    [Password] nvarchar(128)  NOT NULL,
    [PasswordChangedDate] datetime  NULL,
    [PasswordSalt] nvarchar(128)  NOT NULL,
    [PasswordVerificationToken] nvarchar(128)  NULL,
    [PasswordVerificationTokenExpirationDate] datetime  NULL
);
GO

-- Creating table 'webpages_OAuthMembership'
CREATE TABLE [dbo].[webpages_OAuthMembership] (
    [Provider] nvarchar(30)  NOT NULL,
    [ProviderUserId] nvarchar(100)  NOT NULL,
    [UserId] int  NOT NULL
);
GO

-- Creating table 'webpages_Roles'
CREATE TABLE [dbo].[webpages_Roles] (
    [RoleId] int IDENTITY(1,1) NOT NULL,
    [RoleName] nvarchar(256)  NOT NULL
);
GO

-- Creating table 'Companies'
CREATE TABLE [dbo].[Companies] (
    [Name] nvarchar(50)  NOT NULL,
    [Description] nvarchar(500)  NOT NULL,
    [Address] nvarchar(500)  NOT NULL,
    [RegistrationNumber] nvarchar(50)  NULL,
    [PostalCode] nvarchar(50)  NULL,
    [City] nvarchar(70)  NOT NULL,
    [Notes] nvarchar(500)  NULL,
    [VATID] nvarchar(50)  NOT NULL,
    [Bank] nvarchar(50)  NULL,
    [BankAccount] nvarchar(70)  NULL
);
GO

-- Creating table 'Tags'
CREATE TABLE [dbo].[Tags] (
    [Description] nvarchar(max)  NOT NULL,
    [CompanyName] nvarchar(50)  NOT NULL,
    [Name] nvarchar(50)  NOT NULL,
    [Id] int IDENTITY(1,1) NOT NULL,
    [ActiveSurveyTemplate_Id] int  NULL
);
GO

-- Creating table 'TagTypes'
CREATE TABLE [dbo].[TagTypes] (
    [Type] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'AlertNotificationSet'
CREATE TABLE [dbo].[AlertNotificationSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Type] nvarchar(50)  NOT NULL,
    [DistributionList] nvarchar(500)  NOT NULL,
    [QuestionAlertId] int  NOT NULL
);
GO

-- Creating table 'QuestionAlertSet'
CREATE TABLE [dbo].[QuestionAlertSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Description] nvarchar(200)  NOT NULL,
    [Operator] nvarchar(50)  NOT NULL,
    [TriggerAnswer] nvarchar(max)  NOT NULL,
    [QuestionId] int  NOT NULL,
    [Tags_CompanyName] nvarchar(50)  NULL,
    [Tags_Id] int  NULL
);
GO

-- Creating table 'DeviceSet'
CREATE TABLE [dbo].[DeviceSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DeviceId] nvarchar(max)  NOT NULL,
    [CompaniesName] nvarchar(50)  NOT NULL,
    [SurveyLink] nvarchar(max)  NULL,
    [FriendlyName] nvarchar(max)  NULL
);
GO

-- Creating table 'webpages_UsersInRoles'
CREATE TABLE [dbo].[webpages_UsersInRoles] (
    [webpages_Roles_RoleId] int  NOT NULL,
    [UserProfile_UserId] int  NOT NULL
);
GO

-- Creating table 'UserProfileSurveyPlan'
CREATE TABLE [dbo].[UserProfileSurveyPlan] (
    [SurveyTemplateSet_Id] int  NOT NULL,
    [UserProfile_UserId] int  NOT NULL
);
GO

-- Creating table 'SurveyResultTags'
CREATE TABLE [dbo].[SurveyResultTags] (
    [SurveyResultSet_Id] int  NOT NULL,
    [Tags_CompanyName] nvarchar(50)  NOT NULL,
    [Tags_Id] int  NOT NULL
);
GO

-- Creating table 'TagsTagTypes'
CREATE TABLE [dbo].[TagsTagTypes] (
    [Tags_CompanyName] nvarchar(50)  NOT NULL,
    [Tags_Id] int  NOT NULL,
    [TagTypes_Type] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'TagsTags'
CREATE TABLE [dbo].[TagsTags] (
    [Regions_CompanyName] nvarchar(50)  NOT NULL,
    [Regions_Id] int  NOT NULL,
    [Locations_CompanyName] nvarchar(50)  NOT NULL,
    [Locations_Id] int  NOT NULL
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

-- Creating primary key on [Id] in table 'SurveyTemplateSet'
ALTER TABLE [dbo].[SurveyTemplateSet]
ADD CONSTRAINT [PK_SurveyTemplateSet]
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

-- Creating primary key on [UserId] in table 'UserProfile'
ALTER TABLE [dbo].[UserProfile]
ADD CONSTRAINT [PK_UserProfile]
    PRIMARY KEY CLUSTERED ([UserId] ASC);
GO

-- Creating primary key on [UserId] in table 'webpages_Membership'
ALTER TABLE [dbo].[webpages_Membership]
ADD CONSTRAINT [PK_webpages_Membership]
    PRIMARY KEY CLUSTERED ([UserId] ASC);
GO

-- Creating primary key on [Provider], [ProviderUserId] in table 'webpages_OAuthMembership'
ALTER TABLE [dbo].[webpages_OAuthMembership]
ADD CONSTRAINT [PK_webpages_OAuthMembership]
    PRIMARY KEY CLUSTERED ([Provider], [ProviderUserId] ASC);
GO

-- Creating primary key on [RoleId] in table 'webpages_Roles'
ALTER TABLE [dbo].[webpages_Roles]
ADD CONSTRAINT [PK_webpages_Roles]
    PRIMARY KEY CLUSTERED ([RoleId] ASC);
GO

-- Creating primary key on [Name] in table 'Companies'
ALTER TABLE [dbo].[Companies]
ADD CONSTRAINT [PK_Companies]
    PRIMARY KEY CLUSTERED ([Name] ASC);
GO

-- Creating primary key on [CompanyName], [Id] in table 'Tags'
ALTER TABLE [dbo].[Tags]
ADD CONSTRAINT [PK_Tags]
    PRIMARY KEY CLUSTERED ([CompanyName], [Id] ASC);
GO

-- Creating primary key on [Type] in table 'TagTypes'
ALTER TABLE [dbo].[TagTypes]
ADD CONSTRAINT [PK_TagTypes]
    PRIMARY KEY CLUSTERED ([Type] ASC);
GO

-- Creating primary key on [Id] in table 'AlertNotificationSet'
ALTER TABLE [dbo].[AlertNotificationSet]
ADD CONSTRAINT [PK_AlertNotificationSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'QuestionAlertSet'
ALTER TABLE [dbo].[QuestionAlertSet]
ADD CONSTRAINT [PK_QuestionAlertSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'DeviceSet'
ALTER TABLE [dbo].[DeviceSet]
ADD CONSTRAINT [PK_DeviceSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [webpages_Roles_RoleId], [UserProfile_UserId] in table 'webpages_UsersInRoles'
ALTER TABLE [dbo].[webpages_UsersInRoles]
ADD CONSTRAINT [PK_webpages_UsersInRoles]
    PRIMARY KEY NONCLUSTERED ([webpages_Roles_RoleId], [UserProfile_UserId] ASC);
GO

-- Creating primary key on [SurveyTemplateSet_Id], [UserProfile_UserId] in table 'UserProfileSurveyPlan'
ALTER TABLE [dbo].[UserProfileSurveyPlan]
ADD CONSTRAINT [PK_UserProfileSurveyPlan]
    PRIMARY KEY NONCLUSTERED ([SurveyTemplateSet_Id], [UserProfile_UserId] ASC);
GO

-- Creating primary key on [SurveyResultSet_Id], [Tags_CompanyName], [Tags_Id] in table 'SurveyResultTags'
ALTER TABLE [dbo].[SurveyResultTags]
ADD CONSTRAINT [PK_SurveyResultTags]
    PRIMARY KEY NONCLUSTERED ([SurveyResultSet_Id], [Tags_CompanyName], [Tags_Id] ASC);
GO

-- Creating primary key on [Tags_CompanyName], [Tags_Id], [TagTypes_Type] in table 'TagsTagTypes'
ALTER TABLE [dbo].[TagsTagTypes]
ADD CONSTRAINT [PK_TagsTagTypes]
    PRIMARY KEY NONCLUSTERED ([Tags_CompanyName], [Tags_Id], [TagTypes_Type] ASC);
GO

-- Creating primary key on [Regions_CompanyName], [Regions_Id], [Locations_CompanyName], [Locations_Id] in table 'TagsTags'
ALTER TABLE [dbo].[TagsTags]
ADD CONSTRAINT [PK_TagsTags]
    PRIMARY KEY NONCLUSTERED ([Regions_CompanyName], [Regions_Id], [Locations_CompanyName], [Locations_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

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
    REFERENCES [dbo].[SurveyTemplateSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

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
    ON DELETE CASCADE ON UPDATE NO ACTION;

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

-- Creating foreign key on [webpages_Roles_RoleId] in table 'webpages_UsersInRoles'
ALTER TABLE [dbo].[webpages_UsersInRoles]
ADD CONSTRAINT [FK_webpages_UsersInRoles_webpages_Roles]
    FOREIGN KEY ([webpages_Roles_RoleId])
    REFERENCES [dbo].[webpages_Roles]
        ([RoleId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [UserProfile_UserId] in table 'webpages_UsersInRoles'
ALTER TABLE [dbo].[webpages_UsersInRoles]
ADD CONSTRAINT [FK_webpages_UsersInRoles_UserProfile]
    FOREIGN KEY ([UserProfile_UserId])
    REFERENCES [dbo].[UserProfile]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_webpages_UsersInRoles_UserProfile'
CREATE INDEX [IX_FK_webpages_UsersInRoles_UserProfile]
ON [dbo].[webpages_UsersInRoles]
    ([UserProfile_UserId]);
GO

-- Creating foreign key on [SurveyTemplateSet_Id] in table 'UserProfileSurveyPlan'
ALTER TABLE [dbo].[UserProfileSurveyPlan]
ADD CONSTRAINT [FK_UserProfileSurveyPlan_SurveyPlan]
    FOREIGN KEY ([SurveyTemplateSet_Id])
    REFERENCES [dbo].[SurveyTemplateSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [UserProfile_UserId] in table 'UserProfileSurveyPlan'
ALTER TABLE [dbo].[UserProfileSurveyPlan]
ADD CONSTRAINT [FK_UserProfileSurveyPlan_UserProfile]
    FOREIGN KEY ([UserProfile_UserId])
    REFERENCES [dbo].[UserProfile]
        ([UserId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfileSurveyPlan_UserProfile'
CREATE INDEX [IX_FK_UserProfileSurveyPlan_UserProfile]
ON [dbo].[UserProfileSurveyPlan]
    ([UserProfile_UserId]);
GO

-- Creating foreign key on [SurveyPlan_Id] in table 'QuestionSet'
ALTER TABLE [dbo].[QuestionSet]
ADD CONSTRAINT [FK_SurveyPlanQuestion]
    FOREIGN KEY ([SurveyPlan_Id])
    REFERENCES [dbo].[SurveyTemplateSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SurveyPlanQuestion'
CREATE INDEX [IX_FK_SurveyPlanQuestion]
ON [dbo].[QuestionSet]
    ([SurveyPlan_Id]);
GO

-- Creating foreign key on [SurveyPlan_Id] in table 'CustomerSet'
ALTER TABLE [dbo].[CustomerSet]
ADD CONSTRAINT [FK_RunningSurvey]
    FOREIGN KEY ([SurveyPlan_Id])
    REFERENCES [dbo].[SurveyTemplateSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_RunningSurvey'
CREATE INDEX [IX_FK_RunningSurvey]
ON [dbo].[CustomerSet]
    ([SurveyPlan_Id]);
GO

-- Creating foreign key on [CurrentQuestion_Id] in table 'SurveyResultSet'
ALTER TABLE [dbo].[SurveyResultSet]
ADD CONSTRAINT [FK_SurveyResultQuestion]
    FOREIGN KEY ([CurrentQuestion_Id])
    REFERENCES [dbo].[QuestionSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SurveyResultQuestion'
CREATE INDEX [IX_FK_SurveyResultQuestion]
ON [dbo].[SurveyResultSet]
    ([CurrentQuestion_Id]);
GO

-- Creating foreign key on [CompanyName] in table 'Tags'
ALTER TABLE [dbo].[Tags]
ADD CONSTRAINT [FK_CompanyTag]
    FOREIGN KEY ([CompanyName])
    REFERENCES [dbo].[Companies]
        ([Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Company_Name] in table 'UserProfile'
ALTER TABLE [dbo].[UserProfile]
ADD CONSTRAINT [FK_UserProfileCompanies]
    FOREIGN KEY ([Company_Name])
    REFERENCES [dbo].[Companies]
        ([Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfileCompanies'
CREATE INDEX [IX_FK_UserProfileCompanies]
ON [dbo].[UserProfile]
    ([Company_Name]);
GO

-- Creating foreign key on [QuestionAlertId] in table 'AlertNotificationSet'
ALTER TABLE [dbo].[AlertNotificationSet]
ADD CONSTRAINT [FK_QuestionAlertAlertNotification]
    FOREIGN KEY ([QuestionAlertId])
    REFERENCES [dbo].[QuestionAlertSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_QuestionAlertAlertNotification'
CREATE INDEX [IX_FK_QuestionAlertAlertNotification]
ON [dbo].[AlertNotificationSet]
    ([QuestionAlertId]);
GO

-- Creating foreign key on [QuestionId] in table 'QuestionAlertSet'
ALTER TABLE [dbo].[QuestionAlertSet]
ADD CONSTRAINT [FK_QuestionQuestionAlert]
    FOREIGN KEY ([QuestionId])
    REFERENCES [dbo].[QuestionSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_QuestionQuestionAlert'
CREATE INDEX [IX_FK_QuestionQuestionAlert]
ON [dbo].[QuestionAlertSet]
    ([QuestionId]);
GO

-- Creating foreign key on [SurveyResultSet_Id] in table 'SurveyResultTags'
ALTER TABLE [dbo].[SurveyResultTags]
ADD CONSTRAINT [FK_SurveyResultTags_SurveyResult]
    FOREIGN KEY ([SurveyResultSet_Id])
    REFERENCES [dbo].[SurveyResultSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Tags_CompanyName], [Tags_Id] in table 'SurveyResultTags'
ALTER TABLE [dbo].[SurveyResultTags]
ADD CONSTRAINT [FK_SurveyResultTags_Tags]
    FOREIGN KEY ([Tags_CompanyName], [Tags_Id])
    REFERENCES [dbo].[Tags]
        ([CompanyName], [Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SurveyResultTags_Tags'
CREATE INDEX [IX_FK_SurveyResultTags_Tags]
ON [dbo].[SurveyResultTags]
    ([Tags_CompanyName], [Tags_Id]);
GO

-- Creating foreign key on [Tags_CompanyName], [Tags_Id] in table 'TagsTagTypes'
ALTER TABLE [dbo].[TagsTagTypes]
ADD CONSTRAINT [FK_TagsTagTypes_Tags]
    FOREIGN KEY ([Tags_CompanyName], [Tags_Id])
    REFERENCES [dbo].[Tags]
        ([CompanyName], [Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [TagTypes_Type] in table 'TagsTagTypes'
ALTER TABLE [dbo].[TagsTagTypes]
ADD CONSTRAINT [FK_TagsTagTypes_TagTypes]
    FOREIGN KEY ([TagTypes_Type])
    REFERENCES [dbo].[TagTypes]
        ([Type])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TagsTagTypes_TagTypes'
CREATE INDEX [IX_FK_TagsTagTypes_TagTypes]
ON [dbo].[TagsTagTypes]
    ([TagTypes_Type]);
GO

-- Creating foreign key on [ActiveSurveyTemplate_Id] in table 'Tags'
ALTER TABLE [dbo].[Tags]
ADD CONSTRAINT [FK_SurveyTemplateTags]
    FOREIGN KEY ([ActiveSurveyTemplate_Id])
    REFERENCES [dbo].[SurveyTemplateSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SurveyTemplateTags'
CREATE INDEX [IX_FK_SurveyTemplateTags]
ON [dbo].[Tags]
    ([ActiveSurveyTemplate_Id]);
GO

-- Creating foreign key on [Tags_CompanyName], [Tags_Id] in table 'QuestionAlertSet'
ALTER TABLE [dbo].[QuestionAlertSet]
ADD CONSTRAINT [FK_QuestionAlertSetTags]
    FOREIGN KEY ([Tags_CompanyName], [Tags_Id])
    REFERENCES [dbo].[Tags]
        ([CompanyName], [Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_QuestionAlertSetTags'
CREATE INDEX [IX_FK_QuestionAlertSetTags]
ON [dbo].[QuestionAlertSet]
    ([Tags_CompanyName], [Tags_Id]);
GO

-- Creating foreign key on [Regions_CompanyName], [Regions_Id] in table 'TagsTags'
ALTER TABLE [dbo].[TagsTags]
ADD CONSTRAINT [FK_TagsTags_Tags]
    FOREIGN KEY ([Regions_CompanyName], [Regions_Id])
    REFERENCES [dbo].[Tags]
        ([CompanyName], [Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Locations_CompanyName], [Locations_Id] in table 'TagsTags'
ALTER TABLE [dbo].[TagsTags]
ADD CONSTRAINT [FK_TagsTags_Tags1]
    FOREIGN KEY ([Locations_CompanyName], [Locations_Id])
    REFERENCES [dbo].[Tags]
        ([CompanyName], [Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TagsTags_Tags1'
CREATE INDEX [IX_FK_TagsTags_Tags1]
ON [dbo].[TagsTags]
    ([Locations_CompanyName], [Locations_Id]);
GO

-- Creating foreign key on [CompaniesName] in table 'DeviceSet'
ALTER TABLE [dbo].[DeviceSet]
ADD CONSTRAINT [FK_CompaniesDevice]
    FOREIGN KEY ([CompaniesName])
    REFERENCES [dbo].[Companies]
        ([Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CompaniesDevice'
CREATE INDEX [IX_FK_CompaniesDevice]
ON [dbo].[DeviceSet]
    ([CompaniesName]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------