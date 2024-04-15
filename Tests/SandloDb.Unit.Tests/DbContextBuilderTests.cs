using SandloDb.Core.Builders;

namespace SandloDb.Unit.Tests;

public class DbContextBuilderTests
{
    [Fact]
    public void DbContextBuilder_BuildOk_Default_Values()
    {
        //arrange
        var expectedTtlMinutes = 5;
        var expectedMaxMemoryAllocationInBytes = 1e+7;
        
        //act
        var dbContext = DbContextBuilder.Initialize().Build();
        
        //act
        Assert.NotNull(dbContext);
        Assert.Equal(expectedTtlMinutes, dbContext.EntityTtlMinutes);
        Assert.Equal(expectedMaxMemoryAllocationInBytes, dbContext.MaxMemoryAllocationInBytes);
    }
    
    [Fact]
    public void DbContextBuilder_BuildOk_WithEntityTtlMinutes_MaxMemoryAllocationInBytes_Default_Value()
    {
        //arrange
        var entityTtlMinutes = 10;
        var expectedMaxMemoryAllocationInBytes = 1e+7;

        //act
        var dbContext = DbContextBuilder
            .Initialize()
            .WithEntityTtlMinutes(entityTtlMinutes)
            .Build();
        
        //act
        Assert.NotNull(dbContext);
        Assert.Equal(entityTtlMinutes, dbContext.EntityTtlMinutes);
        Assert.Equal(expectedMaxMemoryAllocationInBytes, dbContext.MaxMemoryAllocationInBytes);
    }
    
    [Fact]
    public void DbContextBuilder_BuildOk_MaxMemoryAllocationInBytes__WithEntityTtlMinutesDefault_Value()
    {
        //arrange
        var expectedTtlMinutes = 5;
        var maxMemoryAllocationInBytes = 20;

        //act
        var dbContext = DbContextBuilder
            .Initialize()
            .WithMaxMemoryAllocationInBytes(maxMemoryAllocationInBytes)
            .Build();
        
        //act
        Assert.NotNull(dbContext);
        Assert.Equal(expectedTtlMinutes, dbContext.EntityTtlMinutes);
        Assert.Equal(maxMemoryAllocationInBytes, dbContext.MaxMemoryAllocationInBytes);
    }
    
    [Fact]
    public void DbContextBuilder_BuildOk_WithEntityTtlMinutes_And_MaxMemoryAllocationInBytes()
    {
        //arrange
        var entityTtlMinutes = 10;
        var maxMemoryAllocationInBytes = 2e+8;
        
        //act
        var dbContext = DbContextBuilder
            .Initialize()
            .WithEntityTtlMinutes(entityTtlMinutes)
            .WithMaxMemoryAllocationInBytes(maxMemoryAllocationInBytes)
            .Build();
        
        //act
        Assert.NotNull(dbContext);
        Assert.Equal(entityTtlMinutes, dbContext.EntityTtlMinutes);
        Assert.Equal(maxMemoryAllocationInBytes, dbContext.MaxMemoryAllocationInBytes);
    }
    
    [Fact]
    public void DbContextBuilder_BuildOk_WithEntityTtlMinutes_LessThanZero()
    {
        //arrange
        var entityTtlMinutes = -1;
        var maxMemoryAllocationInBytes = 2e+8;
        
        //act
        var action = () => DbContextBuilder
            .Initialize()
            .WithEntityTtlMinutes(entityTtlMinutes)
            .WithMaxMemoryAllocationInBytes(maxMemoryAllocationInBytes)
            .Build();

        // Assert
        var caughtException = Assert.Throws<InvalidOperationException>(action);
        Assert.Equal("entityTtlMinutes cannot be equal or less than 0 minutes.", caughtException.Message);
    }
    
    [Fact]
    public void DbContextBuilder_BuildOk_WithEntityTtlMinutes_EqualsZero()
    {
        //arrange
        var entityTtlMinutes = 0;
        var maxMemoryAllocationInBytes = 2e+8;

        //act
        var action = () => DbContextBuilder
            .Initialize()
            .WithEntityTtlMinutes(entityTtlMinutes)
            .WithMaxMemoryAllocationInBytes(maxMemoryAllocationInBytes)
            .Build();

        // Assert
        var caughtException = Assert.Throws<InvalidOperationException>(action);
        Assert.Equal("entityTtlMinutes cannot be equal or less than 0 minutes.", caughtException.Message);
    }
    
    [Fact]
    public void DbContextBuilder_BuildOk_WithEntityTtlMinutes_MoreThan30Minutes()
    {
        //arrange
        var entityTtlMinutes = 31;
        var maxMemoryAllocationInBytes = 2e+8;

        //act
        var action = () => DbContextBuilder
            .Initialize()
            .WithEntityTtlMinutes(entityTtlMinutes)
            .WithMaxMemoryAllocationInBytes(maxMemoryAllocationInBytes)
            .Build();

        // Assert
        var caughtException = Assert.Throws<InvalidOperationException>(action);
        Assert.Equal("entityTtlMinutes cannot be more than 30 minutes.", caughtException.Message);
    }
    
    [Fact]
    public void DbContextBuilder_BuildOk_WithMaxMemoryAllocationInBytes_LessThanZero()
    {
        //arrange
        var entityTtlMinutes = 1;
        var maxMemoryAllocationInBytes = -1;

        //act
        var action = () => DbContextBuilder
            .Initialize()
            .WithEntityTtlMinutes(entityTtlMinutes)
            .WithMaxMemoryAllocationInBytes(maxMemoryAllocationInBytes)
            .Build();

        // Assert
        var caughtException = Assert.Throws<InvalidOperationException>(action);
        Assert.Equal("maxMemoryAllocationInBytes cannot be equal or less than 0 bytes.", caughtException.Message);
    }
    
    [Fact]
    public void DbContextBuilder_BuildOk_WithMaxMemoryAllocationInBytes_EqualsZero()
    {
        //arrange
        var entityTtlMinutes = 1;
        var maxMemoryAllocationInBytes = 0;

        //act
        var action = () => DbContextBuilder
            .Initialize()
            .WithEntityTtlMinutes(entityTtlMinutes)
            .WithMaxMemoryAllocationInBytes(maxMemoryAllocationInBytes)
            .Build();

        // Assert
        var caughtException = Assert.Throws<InvalidOperationException>(action);
        Assert.Equal("maxMemoryAllocationInBytes cannot be equal or less than 0 bytes.", caughtException.Message);
    }
    
    [Fact]
    public void DbContextBuilder_BuildOk_WithEntityTtlMinutes_MoreThan200MB()
    {
        //arrange
        var entityTtlMinutes = 10;
        var maxMemoryAllocationInBytes = 3e+8;

        //act
        var action = () => DbContextBuilder
            .Initialize()
            .WithEntityTtlMinutes(entityTtlMinutes)
            .WithMaxMemoryAllocationInBytes(maxMemoryAllocationInBytes)
            .Build();

        // Assert
        var caughtException = Assert.Throws<InvalidOperationException>(action);
        Assert.Equal("maxMemoryAllocationInBytes cannot be more than 200 megabytes.", caughtException.Message);
    }
}