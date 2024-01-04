# sandlo-db
An in-memory database written totally in .NET

**How can i use it?**

You should reference the project and just the following line in your startup part (Program.cs or whatever you prefer):

```
builder.AddSandloDb();
```

See the Tests directory to find the unit tests and how to use it :-P.

**Configuration**

Add this part in appsettings.json. 

```
"SandloDb": {
    "EntityTtlMinutes": 50
  }
```
The configuration is composed of:

Property | Type | Context |
--- | --- | --- |
EntityTtlMinutes | int | The TTL of the in-memory entities from their creation date. |

**Conclusion**

This is a test project, i'd like to expand it and make it usable :)

**TODO**

- improve code quality
- check for thread safety in the dbcontext
- manage max memory allocation and memory cleanup of older entities, with specific policy, when reached threshold
- expand code coverage with unit testing
- make benchmarking
- make safety tests
- make robustness tests (a lot of concurrent requests shouldn't make it cry)

  

