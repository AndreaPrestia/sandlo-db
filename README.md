# sandlo-db
An in-memory database totally written in .NET

**How can I use it?**

You should reference the project and just the following line in your startup part (Program.cs or whatever you prefer):

```
builder.AddSandloDbContext(new SandloDbOptions()
{
    EntityTtlMinutes = 10
});
```

As you can see the AddSandloDbContext takes as input parameter an object of type **SandloDbOptions**. It is not mandatory.

**SandloDbOptions**

Property | Type | Context                                                      | Default value |
--- | --- |--------------------------------------------------------------|--------------|
EntityTtlMinutes | int | The TTL of the in-memory entities from their creation date.  | 5            |  

See the Tests directory to find the unit tests and how to use it :-P.

**Conclusion**

This is just a test project, I aim to expand it and make it usable :)

**TODO**

- improving code quality
- managing max memory allocation and memory cleanup of older entities, with specific policy, when the threshold is reached
- expanding code coverage with unit testing
- safety tests
- robustness tests (the receipt of concurrent requests should not result in crash, corrupted or inconsistent data, race conditions not satisfied and so on)
- benchmarking
- try to design clusterization of it

  

