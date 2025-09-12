# Release Process

## Versioning
The project follows [SemVer](https://semver.org/).  Breaking changes bump the
major version, new features bump the minor version and patches increment the
patch number.

## Tagging & Changelog
Releases are tagged with the version number.  Conventional commit messages are
used to generate changelogs.

## CI Artifacts
The CI pipeline publishes:
- Test results and coverage reports under the `coverage` artifact.
- Swagger JSON describing the HTTP API.
- Docker images pushed to the registry for each commit.

Consumers can download artifacts from the workflow run summary and pull images
using the tagged version.
