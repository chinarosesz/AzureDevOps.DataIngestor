
After changes to schema, need to create migration scripts so Entity Framework (EF) knows how to build up the database objets. Generaed files need to be checked in.

1. Run from  AzureDevOps.DataIngestor.Sdk directory where the  AzureDevOps.DataIngestor.Sdk.csproj file is located

	dotnet ef migrations add "YOURMIGRATION-NAME"

