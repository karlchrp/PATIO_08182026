# PATIO_08182026 - CsvProcessing

## How to build, run

### Using docker compose

```bash
docker compose up --build
```

### Using docker build and run

```bash
docker build -t csvprocessing .
```

```bash
docker run --rm -p 8080:8080 csvprocessing
```

### In local

```bash
dotnet run --project src/CsvProcessing.Api
```

### For running unit tests

```bash
dotnet test tests/CsvProcessing.Api.Tests/CsvProcessing.Api.Tests.csproj
```

## How to test / API documentation and usage
depends on what you executed for build and run, if you ran dotnet in local, please use placeholder-key-for-development as the X-Api-Key

### POST /api/v1/files/process
Uploads a CSV file and returns an aggregate calculated over one column.
The file is assumed to be well-formed. A non-numeric value, a short row, or a file with no data rows will return 422 rather than being skipped

#### Parameters
file - in form-data, required, the csv file to process (must have .csv extension and under 10mb)
column - in query, required, the column to aggregate
operation - in query, not required (defaults to average), which aggregate to calculate (average, sum, and count are the available operations)

#### Sample
for this below sample, you may change the column and the operation to be used in the query parameters
for the test file to be used in this api I already uploaded a sample on the repository. you can edit the file path below in case another csv has to be tested
```bash
curl.exe -X POST -H "X-Api-Key: placeholder-key-for-production" -F "file=@sample/testfile.csv" "http://localhost:8080/api/v1/files/process?column=Amount&operation=average"
```

#### Successful Response
```json
{
    "processingId": "552edef7-ce1a-406b-9f22-4427e83a61f8", -Identifier generated for this request
    "fileName": "test.csv", -Uploaded file name
    "sizeInBytes": 335, -Uploaded file size
    "format": "csv", -Always csv
    "operation": "sum", -Operation passed in request
    "processDateTime": "2026-08-18T11:59:47.1428761+00:00", -UTC time at completion
    "durationMs": 4.744, -Total process time of request
    "result": {
        "column": "Amount", -Column passed in request
        "operation": "sum", -Operation passed in request
        "value": 29875.54 -Calculated aggregate
    }
}
```

#### Errors
400 Bad Request
401 Unauthorized
413 Payload Too Large
415 Unsupported Media Type
422 Unprocessable Content - in this case the detail field will contain the reason (e.g. column not found or operation not supported)

### GET /api/v1/reports
Gets the amount of files processed and some details for each of those files

#### Sample
```bash
curl.exe -H "X-Api-Key: placeholder-key-for-production" http://localhost:8080/api/v1/reports
```

#### Successful Response
```json
{
    "totalFilesProcessed": 2,
    "files": [
        {
            "fileName": "test.csv",
            "sizeInBytes": 278,
            "operation": "sum",
            "processDateTime": "2026-08-18T12:44:34.4502212+00:00",
            "durationMs": 4.9877
        },
        {
            "fileName": "testfile.csv",
            "sizeInBytes": 278,
            "operation": "sum",
            "processDateTime": "2026-08-18T12:44:40.2939375+00:00",
            "durationMs": 0.1126
        }
    ]
}
```

## File tracking feature
Every successful upload is recorded with:

- fileName - the uploaded file name
- sizeInBytes - payload size
- operation - which aggregate was requested
- processDateTime - UTC timestamp of completion
- durationMs - processing time, measured with Stopwatch

totalFilesProcessed tracks the total number of files

Records are stored in a singleton InMemoryFileProcessingTracker, lock was used for thread safety.
Since this is tracked through memory only (no database or external storage), the tracking resets on restart.

## Patterns and design decisions

- I used middleware for the exception handling and placed it before UseRouting and MapControllers so that it wraps everything
- For the API Key I stored it on appsettings but ideally I would store them in AWS Secrets
- I tried to keep the controllers thin and implemented layered architecture by separating the service which will process the request
- I implemented options pattern with data annotations
- I implemented dependency injection
- I added unit testing to demonstrate Xunit and Moq, but I didn't create unit tests for everything due to time constraint
- I used IOptionsMonitor (always gives current value) instead of IOptions to allow disabling of API keys that are leaked
- I used CryptographicOperations.FixedTimeEquals to avoid attacks where response time is measured

## Limitations (Mostly due to time and resource constraint)

- Partial test coverage (I would add tests for each operation and each error case. And add tests that call the API over HTTP so the authentication middleware is covered too)
- The csv has to be well-formed (I would add more error handling, specifically for stray commas in the csv. And add skip of row instead of failing the whole batch if one row has an issue)
- Tracking is in memory (I would persist the information in a database instead)
- API Keys are in configuration file (I would keep the API keys in a Secrets Manager)
- Only successful uploads are tracked