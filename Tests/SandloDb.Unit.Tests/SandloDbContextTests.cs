using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using SandloDb.Core;

namespace SandloDb.Unit.Tests
{
    public class SandloDbContextTests
    {
        private readonly IHost _host;

        public SandloDbContextTests()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton<SandloDbContext>();
                })
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