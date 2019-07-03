***********************************************************
** Project :  SQLSpatialTools.UnitTests.DDD
***********************************************************

********************
*** Introduction ***
********************
This project attempts to data drive the test modules with inputs from flat formatted dataset.
Dataset\LRS Folder contains .data files; which are nothing but a flat file representation of test set for each LRS functions.
This attempts to test each functions with 'n' no. of datasets in quick time with no code changes in unit test files.
Here user\tester\developer can add new lines with different cases and get the required function tested from MS UnitTest framework under SQLSpatialTools.UnitTests.DDD namespace.
As a developer; one can take advantage of this framework to datadrive new functions being added as part of this library with minimal changes.

This project also creates the dataset in SQL compact DB file; which can be exported for each test run.
It also publishes the obtained results and error info (on execution failures) to appropriate table columns.

22-Mar-2019 
------------
Added initial to compare the test results against Oracle DB.

********************
***   Dev Notes  ***
********************
To add a new function to this framework;

1. First update the DB schema file for your new function
To change DB schema refer this file( File : Dataset\CreateDBSchema.sql)

Note: Append the file from end;

To add a new function named "double GetStartPointMeasure(string geomWkt)"

Here the variable inputs are geomWkt and expected start point measure; so totally two variables; so create a table for this dataset

------ GetStartPointMeasure
CREATE TABLE [GetStartPointMeasureData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [ExpectedMeasure] float NOT NULL
, [ObtainedMeasure] nvarchar(1000)
, [Result] nvarchar(50)
, [Error] nvarchar(1000)
);
GO
ALTER TABLE [GetStartPointMeasureData] ADD CONSTRAINT [PK_GetStartPointMeasureData] PRIMARY KEY ([Id]);
GO

In the above; Id is an identity column for table
[Result] to mark the test result of test set
[Error]  to capture any test exection failures

2. Update the dataset Mapping file; ref LRSDataSet.cs

Update select, insert query contents accordingly to select and load variable inputs to the function.

public class GetStartPointMeasureData : BaseDataSet
        {
            public const short ParamCount = 4;
            public const string TableName = "LRS_GetStartPointMeasureData";
            public const string DataFile = "Dataset\\LRS\\GetStartPointMeasure.data";
            public static readonly string SelectQuery = string.Format("SELECT [Id], [InputGeom], [ExpectedMeasure] FROM [{0}];", TableName);
            public static readonly string InsertQuery = string.Format("INSERT INTO [{0}] ([InputGeom], [ExpectedMeasure]) VALUES (N'[0]', [1]);", TableName);
            public override string ResultUpdateQuery => string.Format(UpdateResultQuery, TableName, Result, Id);
            public override string ErrorUpdateQuery => string.Format(UpdateErrorQuery, TableName, Error, Id);
            public string GetTargetUpdateQuery(string fieldName, object fieldValue) { return GetTargetUpdateQuery(TableName, fieldName, fieldValue); }

            public string InputGeom { get; set; }
            public double ExpectedMeasure { get; set; }
            public double ObtainedMeasure { get; set; }
        }

3. Create a data file for this function [Path should match the value in [DataFile] property] 
Creating GetStartPointMeasure.data file
--->file contents

## [InputGeom]								            ||[ExpectedStartMeasure]
LINESTRING ( 1 1 NULL  1, 25 1 NULL 25)		|| 1
LINESTRING ( 10 1 NULL  10, 25 1 NULL 25)		|| 10

-- In the above file; we have added two cases.

4. Load the dataset into db []REf: DataManipulator.cs file]

Add an entry to load the new dataset function[DataManipulator.LoadSet()]; which should be invoked from ClassInitialize() module of UnitTest framework

-- sample code
ExecuteQuery(ParseDataSet(LRSDataSet.GetStartPointMeasureData.DataFile, LRSDataSet.GetStartPointMeasureData.ParamCount, LRSDataSet.GetStartPointMeasureData.InsertQuery))

5. Post above configuration add an entry in UnitTest and get the data
-- sample code
var testCases = dbConnection.Query<LRSDataSet.ClipGeometrySegmentData>(LRSDataSet.ClipGeometrySegmentData.SelectQuery);
Loop through each cases and test the function and save the test_result and obtained_result to table.
Ref: SQLSpatialTools.UnitTests.DDD.LRSFunctionTests.ClipGeometrySegmentTest() function to update obtained values and test results. 