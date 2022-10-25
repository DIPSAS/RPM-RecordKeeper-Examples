## Client Credentials with JWK secret

This sample requires a client configuration from the [HelseID test-environment](https://selvbetjening.test.helseid.no) and [OpenDips](https://open.dips.no) before use.

Insert the client configuration into `Program.cs` in the manner below:

```csharp
  private const string _jwkPrivateKey = @"Add your HelseID JWK private key here";
  private const string _clientId = "Add your HelseID client_id here";
  private const string _openDipsSubscriptionKey = "Add your OpenDips subscription key here";
```

## Attribution
Based on the following example from [HelseID](https://github.com/NorskHelsenett/HelseID.Samples/tree/master/HelseId.Samples.ClientCredentials.Jwk).
