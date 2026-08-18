# PATIO_08182026

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

```bash
dotnet test
```

## How to test

