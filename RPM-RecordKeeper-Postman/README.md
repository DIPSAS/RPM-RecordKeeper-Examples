## Postman with JWK secret

This sample requires a client configuration from the [HelseID test-environment](https://selvbetjening.test.helseid.no) and [OpenDips](https://open.dips.no) before use.

Import the collection `RPM-RecordKeeper.postman_collection.json` into Postman. Insert the client configuration as Postman variables. Start `BearerTokenProvider` with `dotnet run` and call the API from within the Postman collection.

```javascript
  jwkPrivateKey = "Add your HelseID JWK private key here";
  clientId = "Add your HelseID client_id here";
  openDipsSubscriptionKey = "Add your OpenDips subscription key here";
```

## Attribution
BearerTokenProvider is based on the following example from [HelseID](https://github.com/NorskHelsenett/HelseID.Samples/tree/master/HelseId.Samples.ClientCredentials.Jwk).
