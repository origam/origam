---
name: license-headers
description: Add ORIGAM license headers when creating source files or fixing missing headers.
---

# License headers

- Follow the supported file types and exclusions in
  `.github/scripts/check-license-headers.ps1`.
- From the repository root, generate a header:
   - PowerShell: `./.github/scripts/get-license-header.ps1 -Extension cs`
   - Bash: `bash .github/scripts/get-license-header.sh cs`
     Replace `cs` with the target extension.
- Add the generated header where missing.
- Run `.github/scripts/check-license-headers.ps1` in PowerShell when available.
  Otherwise report local validation as skipped; GitHub CI runs the check.
