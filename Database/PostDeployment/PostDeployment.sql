/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/


IF ('$(BuildConfiguration)' = 'Debug')
BEGIN
    PRINT '***** Creating Test Data for Debug configuration *****';
    :r .\TestInserts\Account_Inserts.sql
    :r .\TestInserts\Role_Inserts.sql
    :r .\TestInserts\AccountRole_Inserts.sql
END