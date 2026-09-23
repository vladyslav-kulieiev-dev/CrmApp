-- Tabele
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[CatalogItems](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Code] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CategoryId] [int] NOT NULL,
	[Type] [int] NOT NULL,
	[UnitId] [int] NOT NULL,
	[UnitName] [nvarchar](255) NOT NULL,
	[BillingUnitId] [int] NOT NULL,
	[BillingUnitName] [nvarchar](255) NOT NULL,
	[Price] [decimal](18, 4) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[VatRate] [decimal](5, 2) NOT NULL,
	[Currency] [nvarchar](10) NOT NULL,
	[ParentItemId] [int] NULL,
	[CreatedAt] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[TechnicalSupervisorId] [int] NULL,
	[ImplementationManagerId] [int] NULL,
 CONSTRAINT [PK_CatalogItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

ALTER TABLE [dbo].[CatalogItems] ADD  CONSTRAINT [DF_CatalogItems_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[CatalogItemSupportedSystems](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CatalogItemId] [int] NOT NULL,
	[SupportedSystemId] [int] NOT NULL,
 CONSTRAINT [PK_CatalogItemSupportedSystems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ContractorContacts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContractorId] [int] NOT NULL,
	[Firstname] [nvarchar](255) NOT NULL,
	[Lastname] [nvarchar](255) NOT NULL,
	[DisplayName] [nvarchar](255) NOT NULL,
	[Position] [nvarchar](255) NULL,
	[Email] [nvarchar](255) NULL,
	[PhoneNumber] [nvarchar](50) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
 CONSTRAINT [PK_ContractorContacts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ContractorContracts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContractorId] [int] NOT NULL,
	[EngagementType] [int] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[ValidFrom] [datetime] NULL,
	[ValidTo] [datetime] NULL,
	[HoursLimit] [decimal](18, 4) NULL,
	[ContractNumber] [nvarchar](100) NULL,
	[BillingType] [int] NOT NULL,
	[BillingAmount] [decimal](19, 5) NULL,
	[AllowOverLimit] [bit] NOT NULL,
	[RenewalType] [int] NOT NULL,
	[RenewalDate] [datetime] NULL,
	[ModifiedBy] [int] NULL,
	[ModifiedAt] [datetime] NULL,
 CONSTRAINT [PK_ContractorContracts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

ALTER TABLE [dbo].[ContractorContracts] ADD  CONSTRAINT [DF_ContractorContracts_BillingType]  DEFAULT ((0)) FOR [BillingType]
ALTER TABLE [dbo].[ContractorContracts] ADD  CONSTRAINT [DF_ContractorContracts_AllowOverLimit]  DEFAULT ((0)) FOR [AllowOverLimit]
ALTER TABLE [dbo].[ContractorContracts] ADD  CONSTRAINT [DF_ContractorContracts_RenewalType]  DEFAULT ((0)) FOR [RenewalType]
GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ContractorHoursSnapshots](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContractorContractId] [int] NOT NULL,
	[SnapshotDate] [datetime] NOT NULL,
	[HoursUsed] [decimal](18, 4) NOT NULL,
	[HoursRemaining] [decimal](18, 4) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
 CONSTRAINT [PK_ContractorHoursSnapshots] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ContractorLicenseHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContractorLicenseId] [int] NOT NULL,
	[ChangedAt] [datetime] NOT NULL,
	[ChangedBy] [int] NOT NULL,
	[FieldName] [nvarchar](255) NOT NULL,
	[OldValue] [nvarchar](255) NOT NULL,
	[NewValue] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_ContractorLicenseHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ContractorLicenses](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContractorId] [int] NOT NULL,
	[CatalogItemId] [int] NOT NULL,
	[LicenseType] [int] NOT NULL,
	[Quantity] [decimal](18, 4) NOT NULL,
	[ImplementationOwnerId] [int] NULL,
	[TechnicalOwnerId] [int] NULL,
	[SerialNumber] [nvarchar](255) NOT NULL,
	[IssuedAt] [datetime] NOT NULL,
	[ValidFrom] [datetime] NOT NULL,
	[ExpiresAt] [datetime] NULL,
	[UpgradeDate] [datetime] NULL,
	[WarrantyStatus] [int] NULL,
	[WarrantyEndDate] [datetime] NULL,
	[CreatedAt] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[ModifiedBy] [int] NULL,
	[ModifiedAt] [datetime] NULL,
 CONSTRAINT [PK_ContractorLicenses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Contractors](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ForeignSystemObjectId] [int] NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Code] [nvarchar](255) NOT NULL,
	[DisplayName] [nvarchar](255) NOT NULL,
	[Nip] [nvarchar](255) NULL,
	[EuVAT] [nvarchar](255) NULL,
	[IsXopero] [bit] NULL,
 CONSTRAINT [PK_Contractors] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ContractorsNips](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ContractorId] [int] NOT NULL,
	[Nip] [nvarchar](50) NOT NULL,
	[IsPrimary] [bit] NOT NULL,
 CONSTRAINT [PK_ContractorsNips] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
 CONSTRAINT [UQ_ContractorsNips_Nip_ContractorId] UNIQUE NONCLUSTERED 
(
	[Nip] ASC,
	[ContractorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

CREATE NONCLUSTERED INDEX [IX_ContractorsNips_ContractorId] ON [dbo].[ContractorsNips]
(
	[ContractorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
ALTER TABLE [dbo].[ContractorsNips] ADD  CONSTRAINT [DF_ContractorsNips_IsPrimary]  DEFAULT ((0)) FOR [IsPrimary]
GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Dictionaries](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](2000) NULL,
	[DictionaryType] [int] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[CreatedBy] [int] NULL,
	[IsCustom] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_Dictionaries] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

SET ANSI_PADDING ON

CREATE UNIQUE NONCLUSTERED INDEX [IX_Dictionaries_Name] ON [dbo].[Dictionaries]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[DictionariesElements](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DictionaryId] [int] NOT NULL,
	[Key] [nvarchar](255) NOT NULL,
	[Value] [nvarchar](255) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[CreatedBy] [int] NULL,
	[IsCustom] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDefault] [bit] NOT NULL,
	[ParentId] [int] NULL,
	[OrdinalNumber] [int] NOT NULL,
	[AlternativeValuesForMapping] [nvarchar](255) NULL,
	[Icon] [nvarchar](255) NULL,
	[IconColor] [nvarchar](255) NULL,
 CONSTRAINT [PK_DictionariesElements] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

ALTER TABLE [dbo].[DictionariesElements] ADD  CONSTRAINT [DF_DictionariesElements_OrdinalNumber]  DEFAULT ((0)) FOR [OrdinalNumber]
GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ImportedTasks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Number] [nvarchar](255) NOT NULL,
	[State] [nvarchar](255) NOT NULL,
	[Subject] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Category] [nvarchar](255) NULL,
	[Url] [nvarchar](255) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[DueDate] [datetime] NULL,
	[Channel] [nvarchar](100) NULL,
	[Priority] [nvarchar](100) NULL,
	[ResponseDueDate] [datetime] NULL,
	[ProductName] [nvarchar](255) NULL,
	[ClosedAt] [datetime] NULL,
	[ContactFirstName] [nvarchar](255) NULL,
	[ContactLastName] [nvarchar](255) NULL,
	[ContactPhoneNumber] [nvarchar](40) NULL,
	[ContactEmail] [nvarchar](255) NULL,
	[AssigneeFirstName] [nvarchar](255) NULL,
	[AssigneeLastName] [nvarchar](255) NULL,
	[AssigneeEmail] [nvarchar](255) NULL,
	[IsDeleted] [bit] NOT NULL,
	[ExternalId] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_ImportedTasks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Projects](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[State] [int] NOT NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[CreatedAt] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[ModifiedBy] [int] NULL,
	[ProjectManagerId] [int] NOT NULL,
	[ContractorId] [int] NULL,
	[ModifiedAt] [datetime] NULL,
 CONSTRAINT [PK_Projects] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[ProjectsMembers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProjectId] [int] NOT NULL,
	[UserProfileId] [int] NOT NULL,
	[IsProjectManager] [bit] NOT NULL,
 CONSTRAINT [PK_ProjectsMembers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Settings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](255) NOT NULL,
	[Label] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[ValueType] [int] NOT NULL,
	[Value] [nvarchar](max) NULL,
	[IsMultiple] [bit] NOT NULL,
	[TableName] [nvarchar](255) NULL,
	[ValueFixedPrefix] [nvarchar](max) NULL,
	[ValueFixedSuffix] [nvarchar](max) NULL,
 CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

SET ANSI_PADDING ON

CREATE UNIQUE NONCLUSTERED INDEX [IX_Settings_Key] ON [dbo].[Settings]
(
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[SettingsValuesDictionary](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SettingId] [int] NOT NULL,
	[SettingKey] [nvarchar](255) NOT NULL,
	[ValueType] [int] NOT NULL,
	[Value] [nvarchar](255) NOT NULL,
	[Label] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_SettingsValuesDictionary] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[TablesAdditionalFields](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](255) NOT NULL,
	[FieldName] [nvarchar](255) NOT NULL,
	[FieldType] [int] NOT NULL,
	[DictionaryId] [int] NULL,
	[IsMultiple] [bit] NOT NULL,
	[IsShowOnLists] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[CreatedBy] [int] NULL,
	[SortOrder] [int] NOT NULL,
	[IsRequired] [bit] NOT NULL,
	[DefaultValue] [nvarchar](255) NULL,
	[RowId] [int] NULL,
	[PresentInNewRow] [bit] NOT NULL,
	[RowSpan] [smallint] NOT NULL,
	[ColSpan] [smallint] NOT NULL,
	[IsMappedFromInitObject] [bit] NOT NULL,
	[InitObjectPropertyName] [nvarchar](255) NULL,
 CONSTRAINT [PK_TablesAdditionalFields] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

ALTER TABLE [dbo].[TablesAdditionalFields] ADD  CONSTRAINT [DF_TablesAdditionalFields_SortOrder]  DEFAULT ((0)) FOR [SortOrder]
ALTER TABLE [dbo].[TablesAdditionalFields] ADD  CONSTRAINT [DF_TablesAdditionalFields_IsRequired]  DEFAULT ((0)) FOR [IsRequired]
ALTER TABLE [dbo].[TablesAdditionalFields] ADD  CONSTRAINT [DF_TablesAdditionalFields_PresentInNewRow]  DEFAULT ((0)) FOR [PresentInNewRow]
ALTER TABLE [dbo].[TablesAdditionalFields] ADD  CONSTRAINT [DF_TablesAdditionalFields_RowSpan]  DEFAULT ((1)) FOR [RowSpan]
ALTER TABLE [dbo].[TablesAdditionalFields] ADD  CONSTRAINT [DF_TablesAdditionalFields_ColSpan]  DEFAULT ((1)) FOR [ColSpan]
ALTER TABLE [dbo].[TablesAdditionalFields] ADD  CONSTRAINT [DF_TablesAdditionalFields_IsMappedFromInitObject]  DEFAULT ((0)) FOR [IsMappedFromInitObject]
GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[TablesAdditionalFieldsPermissions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TableAdditionalFieldId] [int] NOT NULL,
	[RoleId] [nvarchar](255) NULL,
	[UserId] [int] NULL,
	[CanView] [bit] NOT NULL,
	[CanEdit] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[CreatedBy] [int] NULL,
 CONSTRAINT [PK_TablesAdditionalFieldsPermissions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[TablesAdditionalFieldsValues](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TableAdditionalFieldId] [int] NOT NULL,
	[FieldName] [nvarchar](255) NOT NULL,
	[FieldValue] [nvarchar](max) NULL,
	[DictionaryElementId] [int] NULL,
	[CreatedAt] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[TableName] [nvarchar](255) NOT NULL,
	[RowId] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_TablesAdditionalFieldsValues] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Tasks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TaskNumber] [nvarchar](100) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[CreatedBy] [int] NULL,
	[Priority] [nvarchar](255) NOT NULL,
	[State] [nvarchar](255) NOT NULL,
	[AssignedTo] [int] NULL,
	[AssignedAt] [datetime] NULL,
	[Title] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Notes] [nvarchar](max) NULL,
	[ProjectId] [int] NULL,
	[ContractorId] [int] NULL,
	[ContractorContactId] [int] NULL,
	[ClosedAt] [datetime] NULL,
	[IsDeleted] [bit] NOT NULL,
	[ImportedTaskId] [int] NULL,
	[Progress] [int] NOT NULL,
	[DueDate] [datetime] NULL,
	[Autonumeration] [int] NOT NULL,
	[Source] [nvarchar](255) NULL,
 CONSTRAINT [PK_Tasks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

ALTER TABLE [dbo].[Tasks] ADD  CONSTRAINT [DF_Tasks_Progress]  DEFAULT ((0)) FOR [Progress]
GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[TasksComments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TaskId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[PostedAt] [datetime2](7) NOT NULL,
	[ModifiedAt] [datetime2](7) NULL,
	[Content] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_TasksComments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

GO
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[UsersProfiles](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DisplayName] [nvarchar](max) NOT NULL,
	[ForeignSystemType] [int] NULL,
	[ForeignSystemOperatorId] [nvarchar](450) NULL,
	[UserId] [nvarchar](450) NULL,
	[FirstName] [nvarchar](255) NOT NULL,
	[LastName] [nvarchar](255) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[AlternativeEmails] [nvarchar](255) NULL,
	[CalendarProvider] [nvarchar](100) NULL,
	[CalendarId] [nvarchar](255) NULL,
 CONSTRAINT [PK_UsersProfiles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

SET ANSI_PADDING ON

CREATE UNIQUE NONCLUSTERED INDEX [UX_UsersProfiles_UserId] ON [dbo].[UsersProfiles]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
ALTER TABLE [dbo].[UsersProfiles] ADD  CONSTRAINT [DF_UsersProfiles_FirstName]  DEFAULT (N'') FOR [FirstName]
ALTER TABLE [dbo].[UsersProfiles] ADD  CONSTRAINT [DF_UsersProfiles_LastName]  DEFAULT (N'') FOR [LastName]
ALTER TABLE [dbo].[UsersProfiles] ADD  CONSTRAINT [DF_UsersProfiles_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
-- Klucze obce
ALTER TABLE [dbo].[CatalogItems]  WITH CHECK ADD  CONSTRAINT [FK_CatalogItems_ToCatalogItems] FOREIGN KEY([ParentItemId])
REFERENCES [dbo].[CatalogItems] ([Id])
ALTER TABLE [dbo].[CatalogItems] CHECK CONSTRAINT [FK_CatalogItems_ToCatalogItems]
GO
ALTER TABLE [dbo].[CatalogItems]  WITH CHECK ADD  CONSTRAINT [FK_CatalogItems_ToDictionaryBillingUnits] FOREIGN KEY([BillingUnitId])
REFERENCES [dbo].[DictionariesElements] ([Id])
ALTER TABLE [dbo].[CatalogItems] CHECK CONSTRAINT [FK_CatalogItems_ToDictionaryBillingUnits]
GO
ALTER TABLE [dbo].[CatalogItems]  WITH CHECK ADD  CONSTRAINT [FK_CatalogItems_ToDictionaryProductCategory] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[DictionariesElements] ([Id])
ALTER TABLE [dbo].[CatalogItems] CHECK CONSTRAINT [FK_CatalogItems_ToDictionaryProductCategory]
GO
ALTER TABLE [dbo].[CatalogItems]  WITH CHECK ADD  CONSTRAINT [FK_CatalogItems_ToDictionaryProductTypes] FOREIGN KEY([Type])
REFERENCES [dbo].[DictionariesElements] ([Id])
ALTER TABLE [dbo].[CatalogItems] CHECK CONSTRAINT [FK_CatalogItems_ToDictionaryProductTypes]
GO
ALTER TABLE [dbo].[CatalogItems]  WITH CHECK ADD  CONSTRAINT [FK_CatalogItems_ToDictionaryUnits] FOREIGN KEY([UnitId])
REFERENCES [dbo].[DictionariesElements] ([Id])
ALTER TABLE [dbo].[CatalogItems] CHECK CONSTRAINT [FK_CatalogItems_ToDictionaryUnits]
GO
ALTER TABLE [dbo].[CatalogItems]  WITH CHECK ADD  CONSTRAINT [FK_CatalogItems_ToUserProfiles] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[CatalogItems] CHECK CONSTRAINT [FK_CatalogItems_ToUserProfiles]
GO
ALTER TABLE [dbo].[CatalogItems]  WITH CHECK ADD  CONSTRAINT [FK_CatalogItems_UsersProfiles_ImplementationManagerId] FOREIGN KEY([ImplementationManagerId])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[CatalogItems] CHECK CONSTRAINT [FK_CatalogItems_UsersProfiles_ImplementationManagerId]
GO
ALTER TABLE [dbo].[CatalogItems]  WITH CHECK ADD  CONSTRAINT [FK_CatalogItems_UsersProfiles_TechnicalSupervisorId] FOREIGN KEY([TechnicalSupervisorId])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[CatalogItems] CHECK CONSTRAINT [FK_CatalogItems_UsersProfiles_TechnicalSupervisorId]
GO
ALTER TABLE [dbo].[CatalogItemSupportedSystems]  WITH CHECK ADD  CONSTRAINT [FK_CatalogItemSupportedSystems_CatalogItem_SupportedSystemId] FOREIGN KEY([SupportedSystemId])
REFERENCES [dbo].[CatalogItems] ([Id])
ALTER TABLE [dbo].[CatalogItemSupportedSystems] CHECK CONSTRAINT [FK_CatalogItemSupportedSystems_CatalogItem_SupportedSystemId]
GO
ALTER TABLE [dbo].[CatalogItemSupportedSystems]  WITH CHECK ADD  CONSTRAINT [FK_CatalogItemSupportedSystems_CatalogItems_CatalogItemId] FOREIGN KEY([CatalogItemId])
REFERENCES [dbo].[CatalogItems] ([Id])
ALTER TABLE [dbo].[CatalogItemSupportedSystems] CHECK CONSTRAINT [FK_CatalogItemSupportedSystems_CatalogItems_CatalogItemId]
GO
ALTER TABLE [dbo].[ContractorContacts]  WITH CHECK ADD  CONSTRAINT [FK_ContractorsContacts_Contractors] FOREIGN KEY([ContractorId])
REFERENCES [dbo].[Contractors] ([Id])
ALTER TABLE [dbo].[ContractorContacts] CHECK CONSTRAINT [FK_ContractorsContacts_Contractors]
GO
ALTER TABLE [dbo].[ContractorContracts]  WITH CHECK ADD  CONSTRAINT [FK_ContractorContracts_ToModifiedUser] FOREIGN KEY([ModifiedBy])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[ContractorContracts] CHECK CONSTRAINT [FK_ContractorContracts_ToModifiedUser]
GO
ALTER TABLE [dbo].[ContractorContracts]  WITH CHECK ADD  CONSTRAINT [FK_ContractorContracts_ToUserProfiles] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[ContractorContracts] CHECK CONSTRAINT [FK_ContractorContracts_ToUserProfiles]
GO
ALTER TABLE [dbo].[ContractorContracts]  WITH CHECK ADD  CONSTRAINT [FK_ContractorLicenseHistory_ToContractors] FOREIGN KEY([ContractorId])
REFERENCES [dbo].[Contractors] ([Id])
ALTER TABLE [dbo].[ContractorContracts] CHECK CONSTRAINT [FK_ContractorLicenseHistory_ToContractors]
GO
ALTER TABLE [dbo].[ContractorHoursSnapshots]  WITH CHECK ADD  CONSTRAINT [FK_ContractorHoursSnapshots_ToContractorContracts] FOREIGN KEY([ContractorContractId])
REFERENCES [dbo].[ContractorContracts] ([Id])
ALTER TABLE [dbo].[ContractorHoursSnapshots] CHECK CONSTRAINT [FK_ContractorHoursSnapshots_ToContractorContracts]
GO
ALTER TABLE [dbo].[ContractorHoursSnapshots]  WITH CHECK ADD  CONSTRAINT [FK_ContractorHoursSnapshots_ToUserProfiles] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[ContractorHoursSnapshots] CHECK CONSTRAINT [FK_ContractorHoursSnapshots_ToUserProfiles]
GO
ALTER TABLE [dbo].[ContractorLicenseHistory]  WITH CHECK ADD  CONSTRAINT [FK_ContractorLicenseHistory_ToContractorLicenses] FOREIGN KEY([ContractorLicenseId])
REFERENCES [dbo].[ContractorLicenses] ([Id])
ALTER TABLE [dbo].[ContractorLicenseHistory] CHECK CONSTRAINT [FK_ContractorLicenseHistory_ToContractorLicenses]
GO
ALTER TABLE [dbo].[ContractorLicenseHistory]  WITH CHECK ADD  CONSTRAINT [FK_ContractorLicenseHistory_ToUserProfiles] FOREIGN KEY([ChangedBy])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[ContractorLicenseHistory] CHECK CONSTRAINT [FK_ContractorLicenseHistory_ToUserProfiles]
GO
ALTER TABLE [dbo].[ContractorLicenses]  WITH CHECK ADD  CONSTRAINT [FK_ContractorLicenses_ToCatalogItems] FOREIGN KEY([CatalogItemId])
REFERENCES [dbo].[CatalogItems] ([Id])
ALTER TABLE [dbo].[ContractorLicenses] CHECK CONSTRAINT [FK_ContractorLicenses_ToCatalogItems]
GO
ALTER TABLE [dbo].[ContractorLicenses]  WITH CHECK ADD  CONSTRAINT [FK_ContractorLicenses_ToContractors] FOREIGN KEY([ContractorId])
REFERENCES [dbo].[Contractors] ([Id])
ALTER TABLE [dbo].[ContractorLicenses] CHECK CONSTRAINT [FK_ContractorLicenses_ToContractors]
GO
ALTER TABLE [dbo].[ContractorLicenses]  WITH CHECK ADD  CONSTRAINT [FK_ContractorLicenses_ToModifiedUser] FOREIGN KEY([ModifiedBy])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[ContractorLicenses] CHECK CONSTRAINT [FK_ContractorLicenses_ToModifiedUser]
GO
ALTER TABLE [dbo].[ContractorLicenses]  WITH CHECK ADD  CONSTRAINT [FK_ContractorLicenses_ToUserProfiles] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[ContractorLicenses] CHECK CONSTRAINT [FK_ContractorLicenses_ToUserProfiles]
GO
ALTER TABLE [dbo].[ContractorLicenses]  WITH CHECK ADD  CONSTRAINT [FK_ContractorLicenses_ToUsersProfiles_ImplementationOwnerId] FOREIGN KEY([ImplementationOwnerId])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[ContractorLicenses] CHECK CONSTRAINT [FK_ContractorLicenses_ToUsersProfiles_ImplementationOwnerId]
GO
ALTER TABLE [dbo].[ContractorLicenses]  WITH CHECK ADD  CONSTRAINT [FK_ContractorLicenses_ToUsersProfiles_TechnicalOwnerId] FOREIGN KEY([TechnicalOwnerId])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[ContractorLicenses] CHECK CONSTRAINT [FK_ContractorLicenses_ToUsersProfiles_TechnicalOwnerId]
GO
ALTER TABLE [dbo].[ContractorsNips]  WITH CHECK ADD  CONSTRAINT [FK_ContractorsNips_ToContractors] FOREIGN KEY([ContractorId])
REFERENCES [dbo].[Contractors] ([Id])
ON DELETE CASCADE
ALTER TABLE [dbo].[ContractorsNips] CHECK CONSTRAINT [FK_ContractorsNips_ToContractors]
GO
ALTER TABLE [dbo].[Dictionaries]  WITH CHECK ADD  CONSTRAINT [FK_Dictionary_ToUserProfiles_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[Dictionaries] CHECK CONSTRAINT [FK_Dictionary_ToUserProfiles_CreatedBy]
GO
ALTER TABLE [dbo].[DictionariesElements]  WITH CHECK ADD  CONSTRAINT [FK_DictionariesElements_ToDictionaries] FOREIGN KEY([DictionaryId])
REFERENCES [dbo].[Dictionaries] ([Id])
ALTER TABLE [dbo].[DictionariesElements] CHECK CONSTRAINT [FK_DictionariesElements_ToDictionaries]
GO
ALTER TABLE [dbo].[DictionariesElements]  WITH CHECK ADD  CONSTRAINT [FK_DictionariesElements_ToDictionariesElements] FOREIGN KEY([ParentId])
REFERENCES [dbo].[DictionariesElements] ([Id])
ALTER TABLE [dbo].[DictionariesElements] CHECK CONSTRAINT [FK_DictionariesElements_ToDictionariesElements]
GO
ALTER TABLE [dbo].[DictionariesElements]  WITH CHECK ADD  CONSTRAINT [FK_DictionariesElements_ToUserProfiles_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[DictionariesElements] CHECK CONSTRAINT [FK_DictionariesElements_ToUserProfiles_CreatedBy]
GO
ALTER TABLE [dbo].[Projects]  WITH CHECK ADD  CONSTRAINT [FK_Projects_ToContractors] FOREIGN KEY([ContractorId])
REFERENCES [dbo].[Contractors] ([Id])
ALTER TABLE [dbo].[Projects] CHECK CONSTRAINT [FK_Projects_ToContractors]
GO
ALTER TABLE [dbo].[Projects]  WITH CHECK ADD  CONSTRAINT [FK_Projects_ToUsersProfiles_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[Projects] CHECK CONSTRAINT [FK_Projects_ToUsersProfiles_CreatedBy]
GO
ALTER TABLE [dbo].[Projects]  WITH CHECK ADD  CONSTRAINT [FK_Projects_ToUsersProfiles_ModifiedBy] FOREIGN KEY([ModifiedBy])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[Projects] CHECK CONSTRAINT [FK_Projects_ToUsersProfiles_ModifiedBy]
GO
ALTER TABLE [dbo].[Projects]  WITH CHECK ADD  CONSTRAINT [FK_Projects_ToUsersProfiles_ProjectManagerId] FOREIGN KEY([ProjectManagerId])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[Projects] CHECK CONSTRAINT [FK_Projects_ToUsersProfiles_ProjectManagerId]
GO
ALTER TABLE [dbo].[ProjectsMembers]  WITH CHECK ADD  CONSTRAINT [FK_ProjectsMembers_ToProjects] FOREIGN KEY([ProjectId])
REFERENCES [dbo].[Projects] ([Id])
ALTER TABLE [dbo].[ProjectsMembers] CHECK CONSTRAINT [FK_ProjectsMembers_ToProjects]
GO
ALTER TABLE [dbo].[ProjectsMembers]  WITH CHECK ADD  CONSTRAINT [FK_ProjectsMembers_ToUsersProfiles] FOREIGN KEY([UserProfileId])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[ProjectsMembers] CHECK CONSTRAINT [FK_ProjectsMembers_ToUsersProfiles]
GO
ALTER TABLE [dbo].[SettingsValuesDictionary]  WITH CHECK ADD  CONSTRAINT [FK_SettingsValuesDictionary_Settings] FOREIGN KEY([SettingId])
REFERENCES [dbo].[Settings] ([Id])
ALTER TABLE [dbo].[SettingsValuesDictionary] CHECK CONSTRAINT [FK_SettingsValuesDictionary_Settings]
GO
ALTER TABLE [dbo].[TablesAdditionalFields]  WITH CHECK ADD  CONSTRAINT [FK_CatalogItemAdditionalFields_ToDictionaries] FOREIGN KEY([DictionaryId])
REFERENCES [dbo].[Dictionaries] ([Id])
ALTER TABLE [dbo].[TablesAdditionalFields] CHECK CONSTRAINT [FK_CatalogItemAdditionalFields_ToDictionaries]
GO
ALTER TABLE [dbo].[TablesAdditionalFields]  WITH CHECK ADD  CONSTRAINT [FK_CatalogItemAdditionalFields_ToUserProfiles] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[TablesAdditionalFields] CHECK CONSTRAINT [FK_CatalogItemAdditionalFields_ToUserProfiles]
GO
ALTER TABLE [dbo].[TablesAdditionalFieldsPermissions]  WITH CHECK ADD  CONSTRAINT [FK_TablesAdditionalFieldsPermissions_ToAuthors] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[TablesAdditionalFieldsPermissions] CHECK CONSTRAINT [FK_TablesAdditionalFieldsPermissions_ToAuthors]
GO
ALTER TABLE [dbo].[TablesAdditionalFieldsPermissions]  WITH CHECK ADD  CONSTRAINT [FK_TablesAdditionalFieldsPermissions_ToTablesAdditionalFields] FOREIGN KEY([TableAdditionalFieldId])
REFERENCES [dbo].[TablesAdditionalFields] ([Id])
ALTER TABLE [dbo].[TablesAdditionalFieldsPermissions] CHECK CONSTRAINT [FK_TablesAdditionalFieldsPermissions_ToTablesAdditionalFields]
GO
ALTER TABLE [dbo].[TablesAdditionalFieldsPermissions]  WITH CHECK ADD  CONSTRAINT [FK_TablesAdditionalFieldsPermissions_ToUsersProfiles] FOREIGN KEY([UserId])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[TablesAdditionalFieldsPermissions] CHECK CONSTRAINT [FK_TablesAdditionalFieldsPermissions_ToUsersProfiles]
GO
ALTER TABLE [dbo].[TablesAdditionalFieldsValues]  WITH CHECK ADD  CONSTRAINT [FK_TablesAdditionalFieldsValues_ToDictionariesElements] FOREIGN KEY([DictionaryElementId])
REFERENCES [dbo].[DictionariesElements] ([Id])
ALTER TABLE [dbo].[TablesAdditionalFieldsValues] CHECK CONSTRAINT [FK_TablesAdditionalFieldsValues_ToDictionariesElements]
GO
ALTER TABLE [dbo].[TablesAdditionalFieldsValues]  WITH CHECK ADD  CONSTRAINT [FK_TablesAdditionalFieldsValues_ToTablesAdditionalFields] FOREIGN KEY([TableAdditionalFieldId])
REFERENCES [dbo].[TablesAdditionalFields] ([Id])
ALTER TABLE [dbo].[TablesAdditionalFieldsValues] CHECK CONSTRAINT [FK_TablesAdditionalFieldsValues_ToTablesAdditionalFields]
GO
ALTER TABLE [dbo].[TablesAdditionalFieldsValues]  WITH CHECK ADD  CONSTRAINT [FK_TablesAdditionalFieldsValues_ToUserProfiles_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[TablesAdditionalFieldsValues] CHECK CONSTRAINT [FK_TablesAdditionalFieldsValues_ToUserProfiles_CreatedBy]
GO
ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK_Tasks_AssignedTo_UsersProfiles] FOREIGN KEY([AssignedTo])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[Tasks] CHECK CONSTRAINT [FK_Tasks_AssignedTo_UsersProfiles]
GO
ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK_Tasks_ContractorContacts] FOREIGN KEY([ContractorContactId])
REFERENCES [dbo].[ContractorContacts] ([Id])
ALTER TABLE [dbo].[Tasks] CHECK CONSTRAINT [FK_Tasks_ContractorContacts]
GO
ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK_Tasks_Contractors] FOREIGN KEY([ContractorId])
REFERENCES [dbo].[Contractors] ([Id])
ALTER TABLE [dbo].[Tasks] CHECK CONSTRAINT [FK_Tasks_Contractors]
GO
ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK_Tasks_CreatedBy_UsersProfiles] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[Tasks] CHECK CONSTRAINT [FK_Tasks_CreatedBy_UsersProfiles]
GO
ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK_Tasks_ImportedTasks] FOREIGN KEY([ImportedTaskId])
REFERENCES [dbo].[ImportedTasks] ([Id])
ALTER TABLE [dbo].[Tasks] CHECK CONSTRAINT [FK_Tasks_ImportedTasks]
GO
ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK_Tasks_Projects] FOREIGN KEY([ProjectId])
REFERENCES [dbo].[Projects] ([Id])
ALTER TABLE [dbo].[Tasks] CHECK CONSTRAINT [FK_Tasks_Projects]
GO
ALTER TABLE [dbo].[TasksComments]  WITH CHECK ADD  CONSTRAINT [FK_TasksComments_ToTasks] FOREIGN KEY([TaskId])
REFERENCES [dbo].[Tasks] ([Id])
ALTER TABLE [dbo].[TasksComments] CHECK CONSTRAINT [FK_TasksComments_ToTasks]
GO
ALTER TABLE [dbo].[TasksComments]  WITH CHECK ADD  CONSTRAINT [FK_TasksComments_ToUsersProfiles] FOREIGN KEY([UserId])
REFERENCES [dbo].[UsersProfiles] ([Id])
ALTER TABLE [dbo].[TasksComments] CHECK CONSTRAINT [FK_TasksComments_ToUsersProfiles]
GO
-- Dane słownikowe i ustawienia
-- Dictionaries
SET IDENTITY_INSERT [dbo].[Dictionaries] ON 

INSERT [dbo].[Dictionaries] ([Id], [Name], [Description], [DictionaryType], [CreatedAt], [CreatedBy], [IsCustom], [IsActive]) VALUES (1, N'Waluty', N'Słownik walut', 1, CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1)
INSERT [dbo].[Dictionaries] ([Id], [Name], [Description], [DictionaryType], [CreatedAt], [CreatedBy], [IsCustom], [IsActive]) VALUES (2, N'Jednostki miary', N'Słownik jednostek miary wykorzystywanych w systemie', 2, CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1)
INSERT [dbo].[Dictionaries] ([Id], [Name], [Description], [DictionaryType], [CreatedAt], [CreatedBy], [IsCustom], [IsActive]) VALUES (3, N'Jednostki rozliczeniowe', N'Słownik jednostek rozliczeniowych wykorzystywanych w systemie', 3, CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1)
INSERT [dbo].[Dictionaries] ([Id], [Name], [Description], [DictionaryType], [CreatedAt], [CreatedBy], [IsCustom], [IsActive]) VALUES (4, N'Kategorie produktów', N'Słownik kategorii (grup) produktów', 4, CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1)
INSERT [dbo].[Dictionaries] ([Id], [Name], [Description], [DictionaryType], [CreatedAt], [CreatedBy], [IsCustom], [IsActive]) VALUES (5, N'Typy produktów', N'Słownik typów produktów', 5, CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1)
INSERT [dbo].[Dictionaries] ([Id], [Name], [Description], [DictionaryType], [CreatedAt], [CreatedBy], [IsCustom], [IsActive]) VALUES (6, N'Stawki VAT', N'Słownik stawek VAT', 6, CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1)
INSERT [dbo].[Dictionaries] ([Id], [Name], [Description], [DictionaryType], [CreatedAt], [CreatedBy], [IsCustom], [IsActive]) VALUES (7, N'Statusy płatności kontrahentów', N'Słownik statusów płatności kontrahentów', 7, CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1)
INSERT [dbo].[Dictionaries] ([Id], [Name], [Description], [DictionaryType], [CreatedAt], [CreatedBy], [IsCustom], [IsActive]) VALUES (8, N'Statusy zadań (CRM)', N'Słownik statusów zadań', 8, CAST(N'2026-09-23T23:26:42.000' AS DateTime), NULL, 0, 1)
INSERT [dbo].[Dictionaries] ([Id], [Name], [Description], [DictionaryType], [CreatedAt], [CreatedBy], [IsCustom], [IsActive]) VALUES (9, N'Priorytety zadań (CRM)', N'Słownik priorytetów zadań', 9, CAST(N'2026-09-23T23:26:42.000' AS DateTime), NULL, 0, 1)
INSERT [dbo].[Dictionaries] ([Id], [Name], [Description], [DictionaryType], [CreatedAt], [CreatedBy], [IsCustom], [IsActive]) VALUES (10, N'Źródło zadań (CRM)', N'Słownik źródeł zadań', 10, CAST(N'2026-05-11T11:05:34.567' AS DateTime), NULL, 0, 1)
SET IDENTITY_INSERT [dbo].[Dictionaries] OFF
GO
-- DictionariesElements
SET IDENTITY_INSERT [dbo].[DictionariesElements] ON 

INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (1, 2, N'Sztuka', N'szt', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 1, NULL, 1, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (2, 2, N'Usługa', N'usł', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 2, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (3, 2, N'Godzina', N'h', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 3, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (4, 2, N'Dzień', N'd', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 4, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (5, 2, N'Pakiet', N'pak', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 5, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (6, 2, N'Projekt', N'proj', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 6, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (7, 2, N'Licencja', N'lic', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 7, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (8, 2, N'Miesiąc', N'msc', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 8, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (9, 2, N'Miesiąc / użytkownik', N'msc/user', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 9, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (10, 2, N'Rok', N'rok', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 10, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (11, 2, N'Punkt sieciowy', N'pkt', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 11, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (12, 2, N'Komplet', N'kpl', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 12, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (13, 3, N'Sztuka', N'szt', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 1, NULL, 1, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (14, 3, N'Godzina', N'h', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 2, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (15, 3, N'Dzień', N'd', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 3, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (16, 3, N'Miesiąc', N'msc', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 4, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (17, 3, N'Rok', N'rok', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 5, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (18, 3, N'Pakiet', N'pak', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 6, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (19, 3, N'Usługa', N'usł', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 7, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (20, 3, N'Projekt', N'proj', CAST(N'2026-09-23T21:26:41.000' AS DateTime), NULL, 0, 1, 0, NULL, 8, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (21, 1, N'Polski Złoty', N'PLN', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 1, NULL, 1, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (22, 1, N'Euro', N'EUR', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 0, NULL, 2, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (23, 1, N'Dolar Amerykański', N'USD', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 0, NULL, 3, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (24, 5, N'Oprogramowanie', N'Oprogramowanie', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 1, NULL, 1, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (25, 5, N'Sprzęt', N'Sprzęt', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 0, NULL, 2, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (26, 5, N'Usługa', N'Usługa', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 0, NULL, 3, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (27, 5, N'Subskrypcja', N'Subskrypcja', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 0, NULL, 4, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (28, 5, N'Moduł oprogramowania', N'Moduł oprogramowania', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 0, 24, 5, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (29, 5, N'Komponent sprzętowy', N'Komponent sprzętowy', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 0, 25, 6, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (30, 5, N'Pakiet usług', N'Pakiet usług', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 0, NULL, 7, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (31, 5, N'Inne', N'Inne', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 0, NULL, 8, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (32, 6, N'23%', N'0.23', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 1, NULL, 1, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (33, 6, N'8%', N'0.08', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 0, NULL, 2, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (34, 6, N'5%', N'0.05', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 0, NULL, 3, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (35, 6, N'0%', N'0.00', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 0, NULL, 4, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (36, 7, N'Ok', N'Ok', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 1, NULL, 1, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (37, 7, N'Kontakt przed usługą', N'Kontakt przed usługą', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 0, NULL, 2, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (38, 7, N'Usługi wstrzymane', N'Usługi wstrzymane', CAST(N'2026-09-23T21:26:41.263' AS DateTime), NULL, 0, 1, 0, NULL, 3, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (39, 4, N'Systemy ERP', N'Systemy ERP', CAST(N'2026-09-23T21:26:42.180' AS DateTime), NULL, 0, 1, 1, NULL, 1, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (40, 8, N'Nieprzypisane', N'Nieprzypisane', CAST(N'2026-09-23T23:26:42.763' AS DateTime), NULL, 0, 1, 1, NULL, 1, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (41, 8, N'Nierozpoczęte', N'Nierozpoczęte', CAST(N'2026-09-23T23:26:42.763' AS DateTime), NULL, 0, 1, 0, NULL, 2, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (42, 8, N'W trakcie', N'W trakcie', CAST(N'2026-09-23T23:26:42.763' AS DateTime), NULL, 0, 1, 0, NULL, 3, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (43, 8, N'Wstrzymane', N'Wstrzymane', CAST(N'2026-09-23T23:26:42.763' AS DateTime), NULL, 0, 1, 0, NULL, 4, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (44, 8, N'Zakończone', N'Zakończone', CAST(N'2026-09-23T23:26:42.763' AS DateTime), NULL, 0, 1, 0, NULL, 5, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (45, 8, N'Anulowane', N'Anulowane', CAST(N'2026-09-23T23:26:42.763' AS DateTime), NULL, 0, 1, 0, NULL, 6, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (46, 9, N'Niski', N'Niski', CAST(N'2026-09-23T23:26:42.763' AS DateTime), NULL, 0, 1, 0, NULL, 1, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (47, 9, N'Normalny', N'Normalny', CAST(N'2026-09-23T23:26:42.763' AS DateTime), NULL, 0, 1, 1, NULL, 2, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (48, 9, N'Wysoki', N'Wysoki', CAST(N'2026-09-23T23:26:42.763' AS DateTime), NULL, 0, 1, 0, NULL, 3, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (49, 10, N'Wewnętrzne', N'Wewnętrzne', CAST(N'2026-09-23T23:26:43.440' AS DateTime), NULL, 0, 1, 1, NULL, 1, NULL, NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (50, 10, N'E-Mail', N'Mail', CAST(N'2026-09-23T23:26:43.440' AS DateTime), NULL, 0, 1, 0, NULL, 2, N'Mail', NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (51, 10, N'Zgłoszenie telefoniczne', N'Phone', CAST(N'2026-09-23T23:26:43.440' AS DateTime), NULL, 0, 1, 0, NULL, 3, N'Phone', NULL, NULL)
INSERT [dbo].[DictionariesElements] ([Id], [DictionaryId], [Key], [Value], [CreatedAt], [CreatedBy], [IsCustom], [IsActive], [IsDefault], [ParentId], [OrdinalNumber], [AlternativeValuesForMapping], [Icon], [IconColor]) VALUES (52, 10, N'Zewnętrzny CRM', N'ExternalCRM', CAST(N'2026-09-23T23:26:43.440' AS DateTime), NULL, 0, 1, 0, NULL, 4, NULL, NULL, NULL)
SET IDENTITY_INSERT [dbo].[DictionariesElements] OFF
GO
-- Settings
SET IDENTITY_INSERT [dbo].[Settings] ON 

INSERT [dbo].[Settings] ([Id], [Key], [Label], [Description], [ValueType], [Value], [IsMultiple], [TableName], [ValueFixedPrefix], [ValueFixedSuffix]) VALUES (1, N'ZohoDeskDepartmentId', N'Id oddziału w systemie Zoho Desk', N'', 2, NULL, 0, NULL, NULL, NULL)
INSERT [dbo].[Settings] ([Id], [Key], [Label], [Description], [ValueType], [Value], [IsMultiple], [TableName], [ValueFixedPrefix], [ValueFixedSuffix]) VALUES (2, N'ZohoDeskOrganizationId', N'Id organizacji w systemie Zoho Desk', N'', 2, NULL, 0, NULL, NULL, NULL)
INSERT [dbo].[Settings] ([Id], [Key], [Label], [Description], [ValueType], [Value], [IsMultiple], [TableName], [ValueFixedPrefix], [ValueFixedSuffix]) VALUES (3, N'ZohoDeskMcpUri', N'Adres serwera MCP Zoho Desk', N'', 2, NULL, 0, NULL, NULL, NULL)
INSERT [dbo].[Settings] ([Id], [Key], [Label], [Description], [ValueType], [Value], [IsMultiple], [TableName], [ValueFixedPrefix], [ValueFixedSuffix]) VALUES (4, N'ZohoDeskMcpName', N'Nazwa serwera MCP Zoho Desk', N'', 2, NULL, 0, NULL, NULL, NULL)
SET IDENTITY_INSERT [dbo].[Settings] OFF
GO
-- TablesAdditionalFields
SET IDENTITY_INSERT [dbo].[TablesAdditionalFields] ON 

INSERT [dbo].[TablesAdditionalFields] ([Id], [TableName], [FieldName], [FieldType], [DictionaryId], [IsMultiple], [IsShowOnLists], [CreatedAt], [CreatedBy], [SortOrder], [IsRequired], [DefaultValue], [RowId], [PresentInNewRow], [RowSpan], [ColSpan], [IsMappedFromInitObject], [InitObjectPropertyName]) VALUES (1, N'Contractors', N'Status płatności', 4, 7, 0, 1, CAST(N'2026-09-23T21:26:41.453' AS DateTime), NULL, 0, 0, NULL, NULL, 0, 1, 1, 0, NULL)
SET IDENTITY_INSERT [dbo].[TablesAdditionalFields] OFF
GO
