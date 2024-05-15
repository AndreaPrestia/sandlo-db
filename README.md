# sandlo-db
An in-memory database totally written in .NET

**How can I use it?**

You should reference the project and just the following lines in your startup part (Program.cs or whatever you prefer):

```
var dbContextBuilder = DbContextBuilder
                .Initialize()
                .WithEntityTtlMinutes(5)
                .WithMaxMemoryAllocationInBytes(2000);

builder.AddInMemoryDbContext(dbContextBuilder);
```

You can create the **DbContextBuilder** instance with the **DbContextBuilder.Initialize()** static method and the **AddInMemoryDbContext** extension will register it.

It helps you to apply the **EntityTtlMinutes**, **MaxMemoryAllocationInBytes** with a **FluentBuilder** pattern.

As you can see the **AddInMemoryDbContext** takes as input parameter an object of type **DbContextBuilder**. It is mandatory.

**DbContextBuilder**

This class exposes the methods to build in a **FluentBuilder** pattern way the **DbContext** class.

Method | Parameter                  | Type   | Context                                                     | Default value |
--- |----------------------------|--------|-------------------------------------------------------------|---------------|
WithEntityTtlMinutes | entityTtlMinutes           | int    | The TTL of the in-memory entities from their creation date. | 5             |  
WithMaxMemoryAllocationInBytes | maxMemoryAllocationInBytes | double | The max size in bytes of the database that can be reached.  | 5e+6          |  

See the Tests directory to find the unit tests and how to use it :-P.

**Conclusion**

This is just a test project, I aim to expand it and make it usable :)

**TODO**

- improving code quality
- expanding code coverage with unit testing
- safety tests
- benchmarking
- try to design clusterization of it

  

