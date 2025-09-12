# Final Verification Checklist

- [ ] Build succeeded.
- [ ] Tests passed. (See artifacts for `test-results.trx`).
- [ ] Coverage ≥ 75%. HTML report available under `reports/coverage`.
- [ ] Docker image tags published and runnable via `docker run`.
- [ ] Health checks `/health/live` and `/health/ready` return `200`.
- [ ] Metrics exposed at `/metrics` and OpenTelemetry exporters configured.
