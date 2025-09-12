# Testing Guide

The solution includes unit tests and end-to-end tests.

## Running Tests
```bash
./scripts/dev-restore.sh
./scripts/dev-build.sh
./scripts/dev-test.sh
```

Coverage reports are generated under `reports/coverage`.

## Metrics Endpoint
When the API is running, Prometheus metrics are exposed at `http://localhost:5000/metrics` (or the configured port).
