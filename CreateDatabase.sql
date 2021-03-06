USE [Experiments]
GO
/****** Object:  User [EXPERIMENT]    Script Date: 12.10.2016 11:57:27 ******/
CREATE USER [EXPERIMENT] FOR LOGIN [EXPERIMENT] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [EXPERIMENT]
GO
/****** Object:  Schema [Dynamic]    Script Date: 12.10.2016 11:57:27 ******/
CREATE SCHEMA [Dynamic]
GO
/****** Object:  Table [dbo].[DbDevice]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DbDevice](
	[Id] [varchar](128) NOT NULL,
	[Description] [varchar](4096) NOT NULL,
	[IpEndPoint] [varchar](64) NOT NULL,
 CONSTRAINT [PK_DbDevice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DbDeviceAttributes]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DbDeviceAttributes](
	[DeviceId] [varchar](128) NOT NULL,
	[LocationName] [varchar](128) NULL,
	[Latitude] [decimal](7, 4) NULL,
	[Longitude] [decimal](7, 4) NULL,
	[Elevation] [decimal](18, 2) NULL,
 CONSTRAINT [PK_DbDeviceAttributes] PRIMARY KEY CLUSTERED 
(
	[DeviceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DbSensor]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DbSensor](
	[Id] [varchar](128) NOT NULL,
	[DeviceId] [varchar](128) NOT NULL,
	[SensorId] [int] IDENTITY(1,1) NOT NULL,
	[Description] [varchar](4096) NOT NULL,
	[UnitSymbol] [varchar](10) NOT NULL,
	[SensorValueDataType] [int] NOT NULL CONSTRAINT [DF_DbSensor_SensorValueDataType]  DEFAULT ((0)),
	[SensorDataRetrievalMode] [int] NOT NULL CONSTRAINT [DF_DbSensor_SensorDataRetrievalMode]  DEFAULT ((0)),
	[ShallSensorDataBePersisted] [bit] NOT NULL CONSTRAINT [DF_DbSensor_ShallSensorDataBePersisted]  DEFAULT ((1)),
	[PersistDirectlyAfterChange] [bit] NOT NULL CONSTRAINT [DF_DbSensor_PersistDirectlyAfterChange]  DEFAULT ((0)),
	[IsVirtualSensor] [bit] NOT NULL,
	[SensorCategory] [varchar](128) NULL,
	[SensorDataCalculationMode] [int] NOT NULL CONSTRAINT [DF_DbSensor_SensorDataCalculationMode]  DEFAULT ((0)),
	[VirtualSensorDefinitionType] [int] NOT NULL CONSTRAINT [DF_DbSensor_VirtualSensorDefinitionType]  DEFAULT ((0)),
	[VirtualSensorDefininition] [varchar](max) NULL,
	[PullModeCommunicationType] [int] NOT NULL CONSTRAINT [DF_DbSensor_PullModeCommunicationType]  DEFAULT ((0)),
	[PullModeDotNetType] [varchar](256) NULL CONSTRAINT [DF_DbSensor_PullModeDotNetType]  DEFAULT ((0)),
	[PullFrequencyInSec] [int] NOT NULL CONSTRAINT [DF_DbSensor_SensorDataScanningInSecs]  DEFAULT ((0)),
	[DefaultValue] [varchar](max) NULL,
	[IsSynchronousPushToActuator] [bit] NOT NULL CONSTRAINT [DF_DbSensor_IsSynchronousPushToActuator]  DEFAULT ((0)),
	[IsActuator] [bit] NOT NULL CONSTRAINT [DF_DbSensor_IsActuator]  DEFAULT ((0)),
	[PushModeCommunicationType] [int] NOT NULL CONSTRAINT [DF_DbSensor_PullModeCommunicationType1]  DEFAULT ((0)),
	[LastUpdate] [datetime] NULL,
	[DataValidityThresholdInMsec] [int] NOT NULL CONSTRAINT [DF_DbSensor_DataValidityThresholdInMsec]  DEFAULT ((0)),
 CONSTRAINT [PK_DbSensor] PRIMARY KEY CLUSTERED 
(
	[SensorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_DbSensor] UNIQUE NONCLUSTERED 
(
	[Id] ASC,
	[DeviceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DbSensorData]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DbSensorData](
	[Identity] [bigint] IDENTITY(1,1) NOT NULL,
	[SensorId] [int] NOT NULL,
	[TakenWhen] [datetime2](7) NOT NULL,
	[Value] [varchar](max) NOT NULL,
	[CorrelationId] [varchar](max) NULL,
 CONSTRAINT [PK_DbSensorData] PRIMARY KEY CLUSTERED 
(
	[Identity] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DbSensorDependency]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DbSensorDependency](
	[BaseSensorId] [int] NOT NULL,
	[DependentSensorId] [int] NOT NULL,
 CONSTRAINT [PK_DbSensorDependency] PRIMARY KEY CLUSTERED 
(
	[BaseSensorId] ASC,
	[DependentSensorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Log]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Log](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[Thread] [varchar](255) NOT NULL,
	[Level] [varchar](50) NOT NULL,
	[Logger] [varchar](255) NOT NULL,
	[Message] [varchar](4000) NOT NULL,
	[Exception] [varchar](2000) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PerformanceTest]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PerformanceTest](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StringCol] [varchar](max) NULL,
	[BitCol] [bit] NULL,
	[IntCol] [int] NULL,
	[BigintCol] [bigint] NULL,
	[FloatCol] [float] NULL,
	[NumericCol] [numeric](24, 4) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PerformanceTestBaseData]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PerformanceTestBaseData](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StringCol] [varchar](max) NULL,
	[BitCol] [bit] NULL,
	[IntCol] [int] NULL,
	[BigintCol] [bigint] NULL,
	[FloatCol] [float] NULL,
	[NumericCol] [numeric](18, 4) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PerformanceTestTransform]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PerformanceTestTransform](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StringCol] [varchar](max) NULL,
	[BitCol] [bit] NULL,
	[IntCol] [int] NULL,
	[BigintCol] [bigint] NULL,
	[FloatCol] [float] NULL,
	[NumericCol] [numeric](24, 4) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TrackingPoint]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TrackingPoint](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TrackingPoint] [nvarchar](100) NOT NULL,
	[AdditionalData] [nvarchar](max) NULL,
	[Timestamp] [datetime2](7) NOT NULL,
	[Counter] [bigint] NOT NULL CONSTRAINT [DF_TrackingPoint_Counter]  DEFAULT ((0)),
	[CorrelationId] [varchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[DbDeviceAttributes]  WITH CHECK ADD  CONSTRAINT [FK_DbDeviceAttributes_DbDeviceAttributes] FOREIGN KEY([DeviceId])
REFERENCES [dbo].[DbDevice] ([Id])
GO
ALTER TABLE [dbo].[DbDeviceAttributes] CHECK CONSTRAINT [FK_DbDeviceAttributes_DbDeviceAttributes]
GO
ALTER TABLE [dbo].[DbSensor]  WITH CHECK ADD  CONSTRAINT [FK_DbSensor_DbDevice] FOREIGN KEY([DeviceId])
REFERENCES [dbo].[DbDevice] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DbSensor] CHECK CONSTRAINT [FK_DbSensor_DbDevice]
GO
ALTER TABLE [dbo].[DbSensorData]  WITH NOCHECK ADD  CONSTRAINT [FK_DbSensorData_DbSensor] FOREIGN KEY([SensorId])
REFERENCES [dbo].[DbSensor] ([SensorId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DbSensorData] CHECK CONSTRAINT [FK_DbSensorData_DbSensor]
GO
ALTER TABLE [dbo].[DbSensorDependency]  WITH CHECK ADD  CONSTRAINT [FK_DbSensorDependency_DbSensor] FOREIGN KEY([BaseSensorId])
REFERENCES [dbo].[DbSensor] ([SensorId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DbSensorDependency] CHECK CONSTRAINT [FK_DbSensorDependency_DbSensor]
GO
ALTER TABLE [dbo].[DbSensorDependency]  WITH CHECK ADD  CONSTRAINT [FK_DbSensorDependency_DbSensor1] FOREIGN KEY([DependentSensorId])
REFERENCES [dbo].[DbSensor] ([SensorId])
GO
ALTER TABLE [dbo].[DbSensorDependency] CHECK CONSTRAINT [FK_DbSensorDependency_DbSensor1]
GO
/****** Object:  StoredProcedure [dbo].[CreateDevice]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreateDevice]
    @Id [VARCHAR](128) ,
    @Description [VARCHAR](4096) ,
    @IpEndPoint [VARCHAR](64) ,
    @LocationName [VARCHAR](128) ,
    @Latitude [DECIMAL](7, 4) ,
    @Longitude [DECIMAL](7, 4) ,
    @Elevation [DECIMAL](18, 2)
AS
    BEGIN
        INSERT  INTO DbDevice
                ( Id, Description, IpEndPoint )
        VALUES  ( @Id, @Description, @IpEndPoint );

        INSERT  INTO dbo.DbDeviceAttributes
                ( DeviceId ,
                  LocationName ,
                  Latitude ,
                  Longitude ,
                  Elevation
	            )
        VALUES  ( @Id ,
                  @LocationName ,
                  @Latitude ,
                  @Longitude ,
                  @Elevation
                );
    END;

GO
/****** Object:  StoredProcedure [dbo].[CreateSensor]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreateSensor]
    @Id VARCHAR(128) ,
    @DeviceId VARCHAR(128) ,
    @Description VARCHAR(4096) ,
    @UnitSymbol VARCHAR(10) ,
    @SensorValueDataType INT ,
    @SensorDataRetrievalMode INT ,
    @ShallSensorDataBePersisted BIT ,
    @PersistDirectlyAfterChange BIT ,
    @IsVirtualSensor BIT ,
    @SensorCategory VARCHAR(128) ,
    @SensorDataCalculationMode INT ,
    @VirtualSensorDefinitionType INT ,
    @VirtualSensorDefininition VARCHAR(MAX) ,
    @PullModeCommunicationType INT ,
    @PullModeDotNetType VARCHAR(256) ,
    @PullFrequencyInSec INT ,
    @DefaultValue VARCHAR(MAX),
    @IsSynchronousPushToActuator bit,
    @IsActuator bit,
	@PushModeCommunicationType INT 
AS 
    BEGIN
        INSERT  INTO dbo.DbSensor
                ( Id ,
                  DeviceId ,
                  Description ,
                  UnitSymbol ,
                  SensorValueDataType ,
                  SensorDataRetrievalMode ,
                  ShallSensorDataBePersisted ,
                  PersistDirectlyAfterChange ,
                  IsVirtualSensor ,
                  SensorCategory ,
                  SensorDataCalculationMode ,
                  VirtualSensorDefinitionType ,
                  VirtualSensorDefininition ,
                  PullModeCommunicationType ,
                  PullModeDotNetType ,
                  PullFrequencyInSec ,
                  DefaultValue,
                  IsSynchronousPushToActuator,
                  IsActuator,
				  PushModeCommunicationType
                )
        VALUES  ( @Id ,
                  @DeviceId ,
                  @Description ,
                  @UnitSymbol ,
                  @SensorValueDataType ,
                  @SensorDataRetrievalMode ,
                  @ShallSensorDataBePersisted ,
                  @PersistDirectlyAfterChange ,
                  @IsVirtualSensor ,
                  @SensorCategory ,
                  @SensorDataCalculationMode ,
                  @VirtualSensorDefinitionType ,
                  @VirtualSensorDefininition ,
                  @PullModeCommunicationType ,
                  @PullModeDotNetType ,
                  @PullFrequencyInSec ,
                  @DefaultValue,
                  @IsSynchronousPushToActuator,
                  @IsActuator,
				  @PushModeCommunicationType
                );
		
        SELECT  SCOPE_IDENTITY();
    END

GO
/****** Object:  StoredProcedure [dbo].[CreateSensorDependency]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreateSensorDependency]
	@baseSensorId int,
	@dependentSensorId int
AS
BEGIN
	SET NOCOUNT ON;

    INSERT INTO DbSensorDependency (BaseSensorId, DependentSensorId) VALUES (@baseSensorId, @dependentSensorId);
END

GO
/****** Object:  StoredProcedure [dbo].[DeleteSensorDependency]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[DeleteSensorDependency]
	@baseSensorId int,
	@dependentSensorId int
AS
BEGIN
	SET NOCOUNT ON;

    DELETE FROM DbSensorDependency WHERE BaseSensorId = @baseSensorId AND DependentSensorId = @dependentSensorId;
END

GO
/****** Object:  StoredProcedure [dbo].[MultiplyBy3]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[MultiplyBy3] 
	@current_value varchar(200),
	@sensor_internal_id int,
	@return_value varchar(200) OUT,
	@is_cancelled bit OUT,
	@is_modified bit OUT,
	@is_sample_rate_adjusted bit OUT,
	@new_sample_rate_in_seconds int OUT
AS
BEGIN
	SET NOCOUNT ON;

    SET @return_value = CONVERT(varchar(200), 3 * CONVERT(decimal(10,0), @current_value))
    SET @is_cancelled = 0
    SET @is_modified = 1
    RETURN
END

GO
/****** Object:  StoredProcedure [dbo].[UpdateDevice]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[UpdateDevice]
    @Id [VARCHAR](128) ,
    @Description [VARCHAR](4096) ,
    @IpEndPoint [VARCHAR](64) ,
    @LocationName [VARCHAR](128) ,
    @Latitude [DECIMAL](7, 4) ,
    @Longitude [DECIMAL](7, 4) ,
    @Elevation [DECIMAL](18, 2)
AS
    BEGIN
        UPDATE  DbDevice
        SET     Description = @Description ,
                IpEndPoint = @IpEndPoint
        WHERE   Id = @Id;

        UPDATE  dbo.DbDeviceAttributes
        SET     LocationName = @LocationName ,
                Latitude = @Latitude ,
                Longitude = @Longitude ,
                Elevation = @Elevation
        WHERE   DeviceId = @Id;
    END;

GO
/****** Object:  StoredProcedure [dbo].[UpdateSensor]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[UpdateSensor]
    @SensorId INT ,
    @Description VARCHAR(4096) ,
    @UnitSymbol VARCHAR(10) ,
    @SensorValueDataType INT ,
    @SensorDataRetrievalMode INT ,
    @ShallSensorDataBePersisted BIT ,
    @PersistDirectlyAfterChange BIT ,
    @IsVirtualSensor BIT ,
    @SensorCategory VARCHAR(128) ,
    @SensorDataCalculationMode INT ,
    @VirtualSensorDefinitionType INT ,
    @VirtualSensorDefininition VARCHAR(MAX) ,
    @PullModeCommunicationType INT ,
    @PullModeDotNetType VARCHAR(256) ,
    @PullFrequencyInSec INT ,
    @DefaultValue VARCHAR(MAX),
    @IsSynchronousPushToActuator bit,
    @IsActuator bit,
	@PushModeCommunicationType INT 
AS 
    BEGIN
        UPDATE  dbo.DbSensor
        SET     Description = @Description ,
                UnitSymbol = @UnitSymbol ,
                SensorValueDataType = @SensorValueDataType ,
                SensorDataRetrievalMode = @SensorDataRetrievalMode ,
                ShallSensorDataBePersisted = @ShallSensorDataBePersisted ,
                PersistDirectlyAfterChange = @PersistDirectlyAfterChange ,
                IsVirtualSensor = @IsVirtualSensor ,
                SensorCategory = @SensorCategory ,
                SensorDataCalculationMode = @SensorDataCalculationMode ,
                VirtualSensorDefinitionType = @VirtualSensorDefinitionType ,
                VirtualSensorDefininition = @VirtualSensorDefininition ,
                PullModeCommunicationType = @PullModeCommunicationType ,
                PullModeDotNetType = @PullModeDotNetType ,
                PullFrequencyInSec = @PullFrequencyInSec ,
                DefaultValue = @DefaultValue,
                IsSynchronousPushToActuator = @IsSynchronousPushToActuator,
                IsActuator = @IsActuator,
				PushModeCommunicationType = @PushModeCommunicationType
        WHERE   SensorId = @SensorId;
    END

GO
/****** Object:  StoredProcedure [Dynamic].[CalcVirtualValueDemo]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [Dynamic].[CalcVirtualValueDemo] 
	@current_value varchar(200),
	@sensor_internal_id int
AS
BEGIN
	SET NOCOUNT ON;
    SELECT 100;
    RETURN;
END


GO
/****** Object:  StoredProcedure [Dynamic].[CalcVirtualValueDemoNullResult]    Script Date: 12.10.2016 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [Dynamic].[CalcVirtualValueDemoNullResult] 
	@current_value varchar(200),
	@sensor_internal_id int
AS
BEGIN
	SET NOCOUNT ON;
    SELECT NULL;
    RETURN;
END



GO
