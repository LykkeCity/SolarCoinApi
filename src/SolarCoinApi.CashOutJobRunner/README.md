SolarCoinApi.CashOutJobRunner
================
### Runs a cash-out job

In order to work properly, **appsettings.json** file similar to this should be provided:

	{
	  "periodMs": 10000,

	  "hotWalletPrivKey": "",

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
