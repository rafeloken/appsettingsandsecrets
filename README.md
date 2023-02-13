# appsettingsandsecrets
Example of using appsettings and secrets in a .net Console application.

- In visual studio, go to Debug properties and add an environment variable `DOTNET_ENVIRONMENT` w/ the value `Development`;
- Add a user secret such as : ```{
  "AppSecrets": {
    "SuperSecret": "dev secrets FROM secrets.json!!!"
  }
}```
