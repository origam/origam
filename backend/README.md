# Origam Backend

This section of the project consists of server-side libraries, server implementations, and the `origam-utils` command-line tool.

Linting is enforced by automated tests during the PR process. When compiling the code locally in debug mode, linting is applied automatically, so any pushed code should pass the automated tests without issues.

The automated tests use their own model, `model-tests`, which is separate from `model-root`. If new code depends on changes in the model, those changes must also be added to `model-tests`; otherwise, the automated tests will fail.
