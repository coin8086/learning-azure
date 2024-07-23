param(
  [string] $workSpaceResId
)

$ErrorActionPreference = 'Stop'

$tableParams = @'
{
    "properties": {
        "schema": {
            "name": "MyTable_CL",
            "columns": [
                {
                    "name": "TimeGenerated",
                    "type": "datetime",
                    "description": "The time at which the data was generated"
                },
               {
                    "name": "Computer",
                    "type": "string",
                    "description": "The computer that generated the data"
                },
                {
                    "name": "AdditionalContext",
                    "type": "dynamic",
                    "description": "Additional message properties"
                },
                {
                    "name": "CounterName",
                    "type": "string",
                    "description": "Name of the counter"
                },
                {
                    "name": "CounterValue",
                    "type": "real",
                    "description": "Value collected for the counter"
                }
            ]
        }
    }
}
'@

Invoke-AzRestMethod -Path "$workSpaceResId/tables/MyTable_CL?api-version=2022-10-01" -Method PUT -payload $tableParams