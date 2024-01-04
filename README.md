# sandlo-db
An in-memory database totally written in .NET

**How can I use it?**

You should reference the project and just the following line in your startup part (Program.cs or whatever you prefer):

```
builder.AddSandloDb();
```

See the Tests directory to find the unit tests and how to use it :-P.

**Configuration**

Add this section to your appsettings.json. 

```
"SandloDb": {
    "EntityTtlMinutes": 50
  }
```
Property | Type | Context |
--- | --- | --- |
EntityTtlMinutes | int | The TTL of the in-memory entities from their creation date. |

**Conclusion**

This is just a test project, I aim to expand it and make it usable :)

**TODO**

- improving code quality
- managing max memory allocation and memory cleanup of older entities, with specific policy, when the threshold is reached
- expanding code coverage with unit testing
- safety tests
- robustness tests (the receipt of concurrent requests should not result in crash, corrupted or inconsistent data, race conditions not satisfied and so on)
- benchmarking

  

