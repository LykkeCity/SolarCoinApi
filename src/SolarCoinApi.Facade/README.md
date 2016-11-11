SolarCoinApi.Facade
================
### Runs an API for generating wallets.

To see available methods, see `http://localhost:<port>/swagger/ui`

In order to work properly, **appsettings.json** file similar to this should be provided:

	{
	  "Logging": {
		"IncludeScopes": false,
		"LogLevel": {
		  "Default": "",
		  "System": "",
		  "Microsoft": ""
		}
	  },


	  "RpcServer": {
		"Endpoint": "",
		"Username": "",
		"Password": ""
	  },

	  "logging": {
		"connectionString": "",
		"infoTableName": "",
		"errorTableName": "",
		"warningTableName": ""
	  },

	  "generatedWallets": {
		"connectionString": "",
		"tableName": ""
	  },

	  "ApplicationInsights": {
		"InstrumentationKey": ""
	  }
	}