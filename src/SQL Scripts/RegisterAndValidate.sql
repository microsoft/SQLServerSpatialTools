-- Copyright (c) Microsoft Corporation.  All rights reserved.
-- Install the SQLSpatialTools assembly and all its functions into the current database
-- Runs all the LRS functions with sample examples.

-- First unregister should be called
-- To run sql script inside another SQLCMD Mode should be enabled
-- To enable SQLCMD Mode: Query -> SQLCMD Mode

-- First Unregister existing functions
:r "ScriptDirPath\Unregister.sql"

-- Register Script Path will be replaced in run time
:r "ScriptDirPath\Register.sql"

-- Run LRS Functions 
:r "ScriptDirPath\lrs_geometry_example.sql"

-- Run Util Functions 
:r "ScriptDirPath\util_geometry_example.sql"

-- Run Sample Projections
:r "ScriptDirPath\projection_example.sql"

-- Run Sample Transforms
:r "ScriptDirPath\transform_example.sql"