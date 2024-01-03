using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SandloDb.Core;

namespace SandloDb.Unit.Tests
{
    public class SandloDbContextTests
    {
        private readonly IHost _host;

        public SandloDbContextTests()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) => { services.AddSingleton<SandloDbContext>(); })
                .Build();
        }

        [Fact]
        public void SandloDbContext_Add_Ok()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act

            var result = service.Add(entity);

            //assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.NotEqual(0, result.Created);
            Assert.NotEqual(0, result.Updated);
            Assert.Equal(entity.Created, result.Created);
            Assert.Equal(entity.Updated, result.Updated);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Description, result.Description);
        }

        [Fact]
        public void SandloDbContext_Add_Entity_Null_Ko()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.Add<SandloDbTestEntity>(null);

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Fact]
        public void SandloDbContext_Update_Ok()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act

            var result = service.Add(entity);

            //assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.NotEqual(0, result.Created);
            Assert.NotEqual(0, result.Updated);
            Assert.Equal(entity.Created, result.Created);
            Assert.Equal(entity.Updated, result.Updated);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Description, result.Description);

            result.Name = "Updated Name";
            result.Description = "Updated Description";

            var updateResult = service.Update(result);

            Assert.NotNull(updateResult);
            Assert.Equal(result.Id, updateResult.Id);
            Assert.Equal(result.Created, updateResult.Created);
            Assert.Equal(result.Updated, updateResult.Updated);
            Assert.Equal(entity.Name, updateResult.Name);
            Assert.Equal(entity.Description, updateResult.Description);

        }

        [Fact]
        public void SandloDbContext_Update_Entity_Null_Ko()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.Update<SandloDbTestEntity>(null);

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Fact]
        public void SandloDbContext_Update_Collection_Not_Found_Ko()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.Update(entity);

            //assert
            Assert.Throws<InvalidOperationException>(() => action());
        }

        [Fact]
        public void SandloDbContext_Remove_Ok()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act

            var result = service.Add(entity);

            //assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.NotEqual(0, result.Created);
            Assert.NotEqual(0, result.Updated);
            Assert.Equal(entity.Created, result.Created);
            Assert.Equal(entity.Updated, result.Updated);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Description, result.Description);

            result.Name = "Updated Name";
            result.Description = "Updated Description";

            var deleteResult = service.Remove(result);

            Assert.Equal(1, deleteResult);
        }

        [Fact]
        public void SandloDbContext_Remove_Entity_Null_Ko()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.Remove<SandloDbTestEntity>(null);

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Fact]
        public void SandloDbContext_Remove_Collection_Not_Found_Ko()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.Remove(entity);

            //assert
            Assert.Throws<InvalidOperationException>(() => action());
        }

        [Fact]
        public void SandloDbContext_Get_All_Ok()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act

            var result = service.Add(entity);

            //assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.NotEqual(0, result.Created);
            Assert.NotEqual(0, result.Updated);
            Assert.Equal(entity.Created, result.Created);
            Assert.Equal(entity.Updated, result.Updated);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Description, result.Description);

            var getAllResult = service.GetAll<SandloDbTestEntity>();

            Assert.NotNull(getAllResult);
            Assert.Collection(getAllResult, e =>
            {
                Assert.Equal(result.Id, e.Id);
                Assert.Equal(result.Created, e.Created);
                Assert.Equal(result.Updated, e.Updated);
                Assert.Equal(result.Name, e.Name);
                Assert.Equal(result.Description, e.Description);
            });
        }

        [Fact]
        public void SandloDbContext_Get_All_Collection_Not_Found_Ok()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var getAllResult = service.GetAll<SandloDbTestEntity>();

            //assert
            Assert.NotNull(getAllResult);
            Assert.Empty(getAllResult);
        }

        [Fact]
        public void SandloDbContext_Get_By_Ok()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act

            var result = service.Add(entity);

            //assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.NotEqual(0, result.Created);
            Assert.NotEqual(0, result.Updated);
            Assert.Equal(entity.Created, result.Created);
            Assert.Equal(entity.Updated, result.Updated);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Description, result.Description);

            var getAllResult = service.GetBy<SandloDbTestEntity>(x => x.Name == "name");

            Assert.NotNull(getAllResult);
            Assert.Collection(getAllResult, e =>
            {
                Assert.Equal(result.Id, e.Id);
                Assert.Equal(result.Created, e.Created);
                Assert.Equal(result.Updated, e.Updated);
                Assert.Equal(result.Name, e.Name);
                Assert.Equal(result.Description, e.Description);
            });
        }

        [Fact]
        public void SandloDbContext_Get_By_Collection_Not_Found_Ok()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var result = service.GetBy<SandloDbTestEntity>(x => x.Name == "Sandlo!");

            //assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void SandloDbContext_Get_Ok()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act

            var result = service.Add(entity);

            //assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.NotEqual(0, result.Created);
            Assert.NotEqual(0, result.Updated);
            Assert.Equal(entity.Created, result.Created);
            Assert.Equal(entity.Updated, result.Updated);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Description, result.Description);

            var getResult = service.Get<SandloDbTestEntity>(x => x.Name == "name");

            Assert.NotNull(getResult);
            Assert.Equal(result.Id, getResult.Id);
            Assert.Equal(result.Created, getResult.Created);
            Assert.Equal(result.Updated, getResult.Updated);
            Assert.Equal(result.Name, getResult.Name);
            Assert.Equal(result.Description, getResult.Description);
        }

        [Fact]
        public void SandloDbContext_Get_Collection_Not_Found_Ok()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var result = service.Get<SandloDbTestEntity>(x => x.Name == "Sandlo!");

            //assert
            Assert.Null(result);
        }
        
        [Fact]
        public void SandloDbContext_Get_By_Id_Ok()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act

            var result = service.Add(entity);

            //assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.NotEqual(0, result.Created);
            Assert.NotEqual(0, result.Updated);
            Assert.Equal(entity.Created, result.Created);
            Assert.Equal(entity.Updated, result.Updated);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Description, result.Description);

            var getResult = service.GetById<SandloDbTestEntity>(result.Id);

            Assert.NotNull(getResult);
            Assert.Equal(result.Id, getResult.Id);
            Assert.Equal(result.Created, getResult.Created);
            Assert.Equal(result.Updated, getResult.Updated);
            Assert.Equal(result.Name, getResult.Name);
            Assert.Equal(result.Description, getResult.Description);
        }

        [Fact]
        public void SandloDbContext_Get_By_Id_Collection_Not_Found_Ok()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var result = service.GetById<SandloDbTestEntity>(Guid.NewGuid());

            //assert
            Assert.Null(result);
        }

        public class SandloDbTestEntity : IEntity
        {
            public Guid Id { get; set; }
            public long Created { get; set; }
            public long Updated { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
        }
    }
}