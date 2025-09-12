# Security Overview

## HTTPS and HSTS
- **Development**: HTTPS is enabled; HSTS disabled to ease local testing.
- **Staging/Production**: HTTPS enforced and HSTS enabled via middleware.

## Security Headers
The API adds standard headers on every response:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- Content Security Policy: `default-src 'self'`

## JWT Configuration
Authentication uses JWT bearer tokens.  Issuer, audience and signing keys are
configured via `appsettings.json` and overridden with environment variables or
secrets from a vault.  Tokens are signed and encrypted using symmetric keys.

## CORS Policy
| Environment | Allowed Origins |
|-------------|----------------|
| Development | `https://localhost:5001` |
| QA | `https://qa.example.com` |
| Production | `https://api.example.com` |

## Threats & Mitigations
- **Input validation** – FluentValidation ensures incoming payloads are valid.
- **Serialization safety** – System.Text.Json with explicit DTOs prevents over-
  posting and limits attack surface.
- **Logging** – Serilog is configured not to log PII.
- **Rate limiting** – a fixed window policy protects hot paths from abuse and
  is configurable via `RateLimiting` in `appsettings.json`.
