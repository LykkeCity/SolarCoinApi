SolarCoinApi.CashInJobRunner
================
### Runs a cash-in job

In order to work properly, **appsettings.json** file similar to this should be provided:

	{
	  "periodMs": 10000,

	  "hotWalletAddress": "",

	  "minimumTxAmount": 0.2,

	  "explorer": {
		"host": "",
		"port": "",
		"dbname": ""
	  },

	  "existingTxesStorage": {
		"name": "",
		"connectionString": ""
	  },

	  "generatedWalletsStorage": {
		"name": "",
		"connectionString": ""
	  },

	  "queue": {
		"name": "",
		"connectionString": ""
	  },

	  "logger": {
		"connectionString": "",
		"errorTableName": "",
		"infoTableName": "",
		"warningTableName": ""
	  },

	  "rpc": {
		"endpoint": "",
		"username": "",
		"password": ""
	  }
	}