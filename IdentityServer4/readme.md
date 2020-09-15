# Including/upgrading IdentityServer4 in Origam

1. Clone IdentityServer4 from repository - https://github.com/IdentityServer/IdentityServer4
2. Switch to relevant branch, at the moment we're using `releases/3.1.x`.
3. Navigate to relevant subfolder of the repository `{identity-server4-repository}/src/IdentityServer4/src/` and copy its content into `{origam-source-repository}/IdentityServer4`.
4. Copy signing key from `{identity-server4-repository}/key.snk` to `{origam-source-repository}/IdentityServer4/key.snk`.
5. Update NuGet packages for IdentityServer4 project.
6. Update package version for IdentityServer4 project.
7. Update this document with information about the relevant branch.