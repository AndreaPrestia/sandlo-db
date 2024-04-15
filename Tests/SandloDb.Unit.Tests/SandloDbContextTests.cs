using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SandloDb.Core;
using SandloDb.Core.Entities;

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

        [Theory]
        [Trait("Category", "Concurrency")]
        [InlineData(20)]
        public void SandloDbContext_Add_MultiThread_Ok(int parallelTasks)
        {
            //arrange
            var tasks = new Task[parallelTasks];

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            for (var i = 0; i < parallelTasks; i++)
            {
                var index = i;
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    service.Add(new SandloDbTestEntity()
                    {
                        Name = $"name-{index}",
                        Description = $"description-{index}",
                        Index = index
                    });
                });
            }

            Task.WaitAll(tasks);

            //act
            var result = service.GetAll<SandloDbTestEntity>();

            //assert
            Assert.NotNull(result);
            Assert.Equal(20, result.Count);
            var orderedResult = result.OrderBy(x => x.Index).ToList();
            for (var i = 0; i < orderedResult.Count; i++)
            {
                Assert.NotEqual(Guid.Empty, orderedResult[i].Id);
                Assert.NotEqual(0, orderedResult[i].Created);
                Assert.NotEqual(0, orderedResult[i].Updated);
                Assert.Equal($"name-{i}", orderedResult[i].Name);
                Assert.Equal($"description-{i}", orderedResult[i].Description);
            }
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
        public void SandloDbContext_AddMany_Ok()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var entityTwo = new SandloDbTestEntity()
            {
                Name = "nameTwo",
                Description = "descriptionTwo"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var result = service.AddMany(new List<SandloDbTestEntity>()
            {
                entity, entityTwo
            });

            //assert
            Assert.NotNull(result);
            Assert.Collection(result, eOne =>
                {
                    Assert.NotEqual(Guid.Empty, eOne.Id);
                    Assert.NotEqual(0, eOne.Created);
                    Assert.NotEqual(0, eOne.Updated);
                    Assert.Equal(entity.Created, eOne.Created);
                    Assert.Equal(entity.Updated, eOne.Updated);
                    Assert.Equal(entity.Name, eOne.Name);
                    Assert.Equal(entity.Description, eOne.Description);
                },
                eTwo =>
                {
                    Assert.NotEqual(Guid.Empty, eTwo.Id);
                    Assert.NotEqual(0, eTwo.Created);
                    Assert.NotEqual(0, eTwo.Updated);
                    Assert.Equal(entityTwo.Created, eTwo.Created);
                    Assert.Equal(entityTwo.Updated, eTwo.Updated);
                    Assert.Equal(entityTwo.Name, eTwo.Name);
                    Assert.Equal(entityTwo.Description, eTwo.Description);
                });
        }

        [Fact]
        public void SandloDbContext_AddMany_Entity_Null_Ko()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.AddMany<SandloDbTestEntity>(null);

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Theory]
        [Trait("Category", "Concurrency")]
        [InlineData(20, 20)]
        public void SandloDbContext_AddMany_MultiThread_Ok(int parallelTasks, int chunkSize)
        {
            //arrange
            var elementsToAdd = Enumerable.Range(0, chunkSize * parallelTasks).Select((e, j) =>
                new SandloDbTestEntity()
                {
                    Description = $"description-{j}",
                    Name = $"name-{j}",
                    Index = j
                }).ToList();

            var chunkedEntities = elementsToAdd
                .Select((x, j) => new SandloDbTestEntity()
                {
                    Index = j,
                    Description = x.Description,
                    Name = x.Name
                })
                .GroupBy(x => x.Index / 20)
                .Select(x => x.Select(v => v).ToList())
                .ToList();

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            var tasks = new Task[parallelTasks];

            for (var i = 0; i < parallelTasks; i++)
            {
                var index = i;
                tasks[i] = Task.Factory.StartNew(() => { service.AddMany(chunkedEntities[index]); });
            }

            Task.WaitAll(tasks);

            //act
            var result = service.GetAll<SandloDbTestEntity>();

            //assert
            Assert.NotNull(result);
            Assert.Equal(chunkSize * parallelTasks, result.Count);
            var orderedResult = result.OrderBy(x => x.Index).ToList();
            for (var i = 0; i < orderedResult.Count; i++)
            {
                Assert.NotEqual(Guid.Empty, orderedResult[i].Id);
                Assert.NotEqual(0, orderedResult[i].Created);
                Assert.NotEqual(0, orderedResult[i].Updated);
                Assert.Equal($"name-{i}", orderedResult[i].Name);
                Assert.Equal($"description-{i}", orderedResult[i].Description);
            }
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

            var entityToUpdate = new SandloDbTestEntity()
            {
                Id = result.Id,
                Created = result.Created,
                Name = "Updated Name",
                Description = "Updated Description",
                Index = result.Index
            };

            var updateResult = service.Update(entityToUpdate);

            Assert.NotNull(updateResult);
            Assert.Equal(entityToUpdate.Id, updateResult.Id);
            Assert.Equal(entityToUpdate.Created, updateResult.Created);
            Assert.Equal(entityToUpdate.Updated, updateResult.Updated);
            Assert.Equal(entityToUpdate.Name, updateResult.Name);
            Assert.Equal(entityToUpdate.Description, updateResult.Description);
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
                Id = Guid.NewGuid(),
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
        public void SandloDbContext_UpdateMany_Ok()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description",
                Index = 1
            };

            var entityTwo = new SandloDbTestEntity()
            {
                Name = "nameTwo",
                Description = "descriptionTwo",
                Index = 2
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act

            var result = service.AddMany(new List<SandloDbTestEntity>()
            {
                entity, entityTwo
            });

            //assert
            Assert.NotNull(result);
            Assert.Collection(result, eOne =>
                {
                    Assert.NotEqual(Guid.Empty, eOne.Id);
                    Assert.NotEqual(0, eOne.Created);
                    Assert.NotEqual(0, eOne.Updated);
                    Assert.Equal(entity.Created, eOne.Created);
                    Assert.Equal(entity.Updated, eOne.Updated);
                    Assert.Equal(entity.Name, eOne.Name);
                    Assert.Equal(entity.Description, eOne.Description);
                },
                eTwo =>
                {
                    Assert.NotEqual(Guid.Empty, eTwo.Id);
                    Assert.NotEqual(0, eTwo.Created);
                    Assert.NotEqual(0, eTwo.Updated);
                    Assert.Equal(entityTwo.Created, eTwo.Created);
                    Assert.Equal(entityTwo.Updated, eTwo.Updated);
                    Assert.Equal(entityTwo.Name, eTwo.Name);
                    Assert.Equal(entityTwo.Description, eTwo.Description);
                });

            var entityUpdated = new SandloDbTestEntity()
            {
                Id = entity.Id,
                Created = entity.Created,
                Index = entity.Index,
                Name = "Updated name",
                Description = "Updated description"
            };

            var entityTwoUpdated = new SandloDbTestEntity()
            {
                Id = entityTwo.Id,
                Created = entityTwo.Created,
                Index = entityTwo.Index,
                Name = "Updated nameTwo",
                Description = "Updated descriptionTwo"
            };

            var updateResult = service.UpdateMany(new List<SandloDbTestEntity>()
            {
                entityUpdated, entityTwoUpdated
            });

            Assert.NotNull(updateResult);
            Assert.Collection(updateResult.OrderBy(x => x.Index).ToList(), eOne =>
                {
                    Assert.Equal(entityUpdated.Id, eOne.Id);
                    Assert.Equal(entityUpdated.Created, eOne.Created);
                    Assert.Equal(entityUpdated.Updated, eOne.Updated);
                    Assert.Equal(entityUpdated.Name, eOne.Name);
                    Assert.Equal(entityUpdated.Description, eOne.Description);
                },
                eTwo =>
                {
                    Assert.Equal(entityTwoUpdated.Id, eTwo.Id);
                    Assert.Equal(entityTwoUpdated.Created, eTwo.Created);
                    Assert.Equal(entityTwoUpdated.Updated, eTwo.Updated);
                    Assert.Equal(entityTwoUpdated.Name, eTwo.Name);
                    Assert.Equal(entityTwoUpdated.Description, eTwo.Description);
                });
        }

        [Fact]
        public void SandloDbContext_UpdateMany_Entity_Null_Ko()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.UpdateMany<SandloDbTestEntity>(null);

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Fact]
        public void SandloDbContext_UpdateMany_Collection_Not_Found_Ko()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var entityTwo = new SandloDbTestEntity()
            {
                Name = "nameTwo",
                Description = "descriptionTwo"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.UpdateMany(new List<SandloDbTestEntity>()
            {
                entity, entityTwo
            });

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
        public void SandloDbContext_RemoveMany_Ok()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var entityTwo = new SandloDbTestEntity()
            {
                Name = "nameTwo",
                Description = "descriptionTwo"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var result = service.AddMany(new List<SandloDbTestEntity>()
            {
                entity, entityTwo
            });

            //assert
            Assert.NotNull(result);
            Assert.Collection(result, eOne =>
                {
                    Assert.NotEqual(Guid.Empty, eOne.Id);
                    Assert.NotEqual(0, eOne.Created);
                    Assert.NotEqual(0, eOne.Updated);
                    Assert.Equal(entity.Created, eOne.Created);
                    Assert.Equal(entity.Updated, eOne.Updated);
                    Assert.Equal(entity.Name, eOne.Name);
                    Assert.Equal(entity.Description, eOne.Description);
                },
                eTwo =>
                {
                    Assert.NotEqual(Guid.Empty, eTwo.Id);
                    Assert.NotEqual(0, eTwo.Created);
                    Assert.NotEqual(0, eTwo.Updated);
                    Assert.Equal(entityTwo.Created, eTwo.Created);
                    Assert.Equal(entityTwo.Updated, eTwo.Updated);
                    Assert.Equal(entityTwo.Name, eTwo.Name);
                    Assert.Equal(entityTwo.Description, eTwo.Description);
                });

            var deleteResult = service.RemoveMany(result);

            Assert.Equal(2, deleteResult);
        }

        [Fact]
        public void SandloDbContext_RemoveMany_Entity_Null_Ko()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.RemoveMany<SandloDbTestEntity>(null);

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Fact]
        public void SandloDbContext_RemoveMany_Collection_Not_Found_Ko()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var entityTwo = new SandloDbTestEntity()
            {
                Name = "nameTwo",
                Description = "descriptionTwo"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.RemoveMany(new List<SandloDbTestEntity>()
            {
                entity, entityTwo
            });

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
        public void SandloDbContext_Get_By_Predicate_Null_Ko()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.GetBy<SandloDbTestEntity>(null);

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
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
        public void SandloDbContext_Get_Predicate_Null_Ko()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.Get<SandloDbTestEntity>(null);

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
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

        [Fact]
        public void SandloDbContext_Get_All_By_Type_Ok()
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

            var getAllResult = service.GetAll(typeof(SandloDbTestEntity));

            Assert.NotNull(getAllResult);
            Assert.Collection(getAllResult, e =>
            {
                var typedEntity = e as SandloDbTestEntity;
                Assert.NotNull(typedEntity);
                Assert.Equal(result.Id, typedEntity.Id);
                Assert.Equal(result.Created, typedEntity.Created);
                Assert.Equal(result.Updated, typedEntity.Updated);
                Assert.Equal(result.Name, typedEntity.Name);
                Assert.Equal(result.Description, typedEntity.Description);
            });
        }

        [Fact]
        public void SandloDbContext_Get_All_By_Type_Type_Null_Ko()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.GetAll(null);

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Fact]
        public void SandloDbContext_Get_All_By_Type_Collection_Not_Found_Ok()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var getAllResult = service.GetAll(typeof(SandloDbTestEntity));

            //assert
            Assert.NotNull(getAllResult);
            Assert.Empty(getAllResult);
        }

        [Fact]
        public void SandloDbContext_Get_By_Type_Ok()
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

            var getAllResult = service.GetBy(x => x.Created != 0, typeof(SandloDbTestEntity));

            Assert.NotNull(getAllResult);
            Assert.Collection(getAllResult, e =>
            {
                var typedEntity = e as SandloDbTestEntity;

                Assert.NotNull(typedEntity);
                Assert.Equal(result.Id, typedEntity.Id);
                Assert.Equal(result.Created, typedEntity.Created);
                Assert.Equal(result.Updated, typedEntity.Updated);
                Assert.Equal(result.Name, typedEntity.Name);
                Assert.Equal(result.Description, typedEntity.Description);
            });
        }

        [Fact]
        public void SandloDbContext_Get_By_Type_Predicate_Null_Ko()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.GetBy(null, typeof(SandloDbTestEntity));

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Fact]
        public void SandloDbContext_Get_By_Type_Type_Null_Ko()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.GetBy(x => x.Created != 0, null);

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Fact]
        public void SandloDbContext_Get_By_Type_Collection_Not_Found_Ok()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var result = service.GetBy(x => x.Created != 0, typeof(SandloDbTestEntity));

            //assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void SandloDbContext_Get_Type_Ok()
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

            var getResult = service.Get(x => x.Created != 0, typeof(SandloDbTestEntity));

            Assert.NotNull(getResult);
            var typedResult = getResult as SandloDbTestEntity;
            Assert.NotNull(typedResult);
            Assert.Equal(result.Id, typedResult.Id);
            Assert.Equal(result.Created, typedResult.Created);
            Assert.Equal(result.Updated, typedResult.Updated);
            Assert.Equal(result.Name, typedResult.Name);
            Assert.Equal(result.Description, typedResult.Description);
        }

        [Fact]
        public void SandloDbContext_Get_Type_Predicate_Null_Ko()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.Get(null, typeof(SandloDbTestEntity));

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Fact]
        public void SandloDbContext_Get_Type_Type_Null_Ko()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.Get(x => x.Created != 0, null);

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Fact]
        public void SandloDbContext_Get_By_Id_By_Type_Ok()
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

            var getResult = service.GetById(result.Id, typeof(SandloDbTestEntity));

            Assert.NotNull(getResult);
            var typedResult = getResult as SandloDbTestEntity;
            Assert.NotNull(typedResult);
            Assert.Equal(result.Id, typedResult.Id);
            Assert.Equal(result.Created, typedResult.Created);
            Assert.Equal(result.Updated, typedResult.Updated);
            Assert.Equal(result.Name, typedResult.Name);
            Assert.Equal(result.Description, typedResult.Description);
        }

        [Fact]
        public void SandloDbContext_Get_By_Id_By_Type_Type_Null_Ko()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.GetById(Guid.NewGuid(), null);

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Fact]
        public void SandloDbContext_Get_By_Id_By_Type_Collection_Not_Found_Ok()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var result = service.GetById(Guid.NewGuid(), typeof(SandloDbTestEntity));

            //assert
            Assert.Null(result);
        }

        [Fact]
        public void SandloDbContext_Remove_By_Type_Ok()
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

            var deleteResult = service.Remove(result, typeof(SandloDbTestEntity));

            Assert.Equal(1, deleteResult);
        }

        [Fact]
        public void SandloDbContext_Remove_By_Type_Entity_Null_Ko()
        {
            //arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.Remove(null, typeof(SandloDbTestEntity));

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Fact]
        public void SandloDbContext_Remove_By_Type_Type_Null_Ko()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.Remove(entity, null);

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Fact]
        public void SandloDbContext_Remove_By_Type_Collection_Not_Found_Ko()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.Remove(entity, typeof(SandloDbTestEntity));

            //assert
            Assert.Throws<InvalidOperationException>(() => action());
        }

        [Fact]
        public void SandloDbContext_RemoveMany_By_Type_Ok()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var entityTwo = new SandloDbTestEntity()
            {
                Name = "nameTwo",
                Description = "descriptionTwo"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var result = service.AddMany(new List<SandloDbTestEntity>()
            {
                entity, entityTwo
            });

            //assert
            Assert.NotNull(result);
            Assert.Collection(result, eOne =>
                {
                    Assert.NotEqual(Guid.Empty, eOne.Id);
                    Assert.NotEqual(0, eOne.Created);
                    Assert.NotEqual(0, eOne.Updated);
                    Assert.Equal(entity.Created, eOne.Created);
                    Assert.Equal(entity.Updated, eOne.Updated);
                    Assert.Equal(entity.Name, eOne.Name);
                    Assert.Equal(entity.Description, eOne.Description);
                },
                eTwo =>
                {
                    Assert.NotEqual(Guid.Empty, eTwo.Id);
                    Assert.NotEqual(0, eTwo.Created);
                    Assert.NotEqual(0, eTwo.Updated);
                    Assert.Equal(entityTwo.Created, eTwo.Created);
                    Assert.Equal(entityTwo.Updated, eTwo.Updated);
                    Assert.Equal(entityTwo.Name, eTwo.Name);
                    Assert.Equal(entityTwo.Description, eTwo.Description);
                });

            var deleteResult = service.RemoveMany(result.Select(x => x).ToList<IEntity>(), typeof(SandloDbTestEntity));

            Assert.Equal(2, deleteResult);
        }

        [Fact]
        public void SandloDbContext_RemoveMany_By_Type_Entities_Null_Ko()
        {
            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.RemoveMany(null, typeof(SandloDbTestEntity));

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Fact]
        public void SandloDbContext_RemoveMany_By_Type_Type_Null_Ko()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var entityTwo = new SandloDbTestEntity()
            {
                Name = "nameTwo",
                Description = "descriptionTwo"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.RemoveMany(new List<IEntity>()
            {
                entity, entityTwo
            }, null);

            //assert
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Fact]
        public void SandloDbContext_RemoveMany_By_Type_Collection_Not_Found_Ko()
        {
            //arrange
            var entity = new SandloDbTestEntity()
            {
                Name = "name",
                Description = "description"
            };

            var entityTwo = new SandloDbTestEntity()
            {
                Name = "nameTwo",
                Description = "descriptionTwo"
            };

            var service = _host.Services.GetRequiredService<SandloDbContext>();

            //act
            var action = () => service.RemoveMany(new List<IEntity>()
            {
                entity, entityTwo
            }, typeof(SandloDbTestEntity));

            //assert
            Assert.Throws<InvalidOperationException>(() => action());
        }

        [Theory]
        [Trait("Category", "Concurrency")]
        [InlineData(2, 20)]
        public void SandloDbContext_CompleteRun_MultiThread_Ok(int numThreads, int numIterations)
        {
            // Arrange
            var service = _host.Services.GetRequiredService<SandloDbContext>();
            var threads = new List<Thread>();

            // Act
            for (int i = 0; i < numThreads; i++)
            {
                var thread = new Thread(() =>
                {
                    var entitiesToAdd = GenerateUniqueEntities(numIterations).ToList();
                    var addedEntities = service.AddMany(entitiesToAdd);
                    
                    Assert.NotNull(addedEntities);
                    Assert.True(entitiesToAdd.Select(x => x.Name).SequenceEqual(addedEntities.Select(x => x.Name)));
                    foreach (var entity in addedEntities)
                    {
                        entity.Price += 5.0m;
                    }
                    var updatedEntities = service.UpdateMany(addedEntities);
                    Assert.NotNull(updatedEntities);
                    Assert.True(addedEntities.Select(x => x.Price).SequenceEqual(updatedEntities.Select(x => x.Price)));
                    var deleteResult = service.RemoveMany(updatedEntities);
                    Assert.Equal(updatedEntities.Count, deleteResult);
                });
                threads.Add(thread);
                thread.Start();
            }

            // Wait for all threads to complete
            foreach (var thread in threads)
            {
                thread.Join();
            }

            // Assert
            var remainingEntities = service.GetAll<SandloDbTestEntity>();
            Assert.Empty(remainingEntities);
        }
        
           [Theory]
        [Trait("Category", "Concurrency")]
        [InlineData(20, 20)]
        public void SandloDbContext_CompleteRun_MultiThread_Ok_Old(int parallelTasks, int chunkSize)
        {
            //arrange
            var elementsToAdd = Enumerable.Range(0, chunkSize * parallelTasks).Select((_, j) =>
                new SandloDbTestEntity()
                {
                    Description = $"description-{j}",
                    Name = $"name-{j}",
                    Index = j
                }).ToList();
        
            var chunkedEntities = elementsToAdd
                .Select((x, j) => new SandloDbTestEntity()
                {
                    Index = j,
                    Description = x.Description,
                    Name = x.Name
                })
                .GroupBy(x => x.Index / 20)
                .Select(x => x.Select(v => v).ToList())
                .ToList();
        
            var service = _host.Services.GetRequiredService<SandloDbContext>();
        
            var tasks = new Task[parallelTasks];
        
            for (var i = 0; i < parallelTasks; i++)
            {
                var index = i;
                tasks[i] = Task.Factory.StartNew(() => { service.AddMany(chunkedEntities[index]); });
            }
        
            Task.WaitAll(tasks);
        
            //act
            var result = service.GetAll<SandloDbTestEntity>();
        
            //assert
            Assert.NotNull(result);
            Assert.Equal(chunkSize * parallelTasks, result.Count);
            var orderedResult = result.OrderBy(x => x.Index).ToList();
            for (var i = 0; i < orderedResult.Count; i++)
            {
                Assert.NotEqual(Guid.Empty, orderedResult[i].Id);
                Assert.NotEqual(0, orderedResult[i].Created);
                Assert.NotEqual(0, orderedResult[i].Updated);
                Assert.Equal($"name-{i}", orderedResult[i].Name);
                Assert.Equal($"description-{i}", orderedResult[i].Description);
            }
        
            var toBeUpdatedOrderResult = orderedResult.Select(e => new SandloDbTestEntity()
            {
                Created = e.Created,
                Description = $"{e.Description}-updated",
                Id = e.Id,
                Index = e.Index,
                Name = $"{e.Name}-updated"
            });
            
            var updatedChunkedEntities = toBeUpdatedOrderResult
                .Select((x, j) => new SandloDbTestEntity()
                {
                    Index = j,
                    Description = x.Description,
                    Name = x.Name,
                    Id = x.Id,
                    Created = x.Created,
                    Updated = x.Updated
                })
                .GroupBy(x => x.Index / 20)
                .Select(x => x.Select(v => v).ToList())
                .ToList();
        
            tasks = new Task[parallelTasks];
        
            for (var i = 0; i < parallelTasks; i++)
            {
                var index = i;
                tasks[i] = Task.Factory.StartNew(() => { service.UpdateMany(updatedChunkedEntities[index]); });
            }
        
            Task.WaitAll(tasks);
        
            var updatedResult = service.GetAll<SandloDbTestEntity>();
        
            //assert
            Assert.NotNull(updatedResult);
            Assert.Equal(chunkSize * parallelTasks, result.Count);
            var updatedOrderedResult = updatedResult.OrderBy(x => x.Index).ToList();
            for (var i = 0; i < updatedOrderedResult.Count; i++)
            {
                Assert.NotEqual(Guid.Empty, updatedOrderedResult[i].Id);
                Assert.NotEqual(0, updatedOrderedResult[i].Created);
                Assert.NotEqual(0, updatedOrderedResult[i].Updated);
                Assert.Equal($"name-{i}-updated", updatedOrderedResult[i].Name);
                Assert.Equal($"description-{i}-updated", updatedOrderedResult[i].Description);
            }
            
            var toBeDeletedOrderResult = orderedResult.Select(e => new SandloDbTestEntity()
            {
                Created = e.Created,
                Description = $"{e.Description}-updated",
                Id = e.Id,
                Index = e.Index,
                Name = $"{e.Name}-updated"
            });
            
            var toBeDeletedChunkedEntities = toBeDeletedOrderResult
                .Select((x, j) => new SandloDbTestEntity()
                {
                    Index = x.Index,
                    Description = x.Description,
                    Name = x.Name,
                    Id = x.Id,
                    Created = x.Created,
                    Updated = x.Updated
                })
                .GroupBy(x => x.Index / 20)
                .Select(x => x.Select(v => v).ToList())
                .ToList();
            
            tasks = new Task[parallelTasks];
            
            for (var i = 0; i < parallelTasks; i++)
            {
                var index = i;
                tasks[i] = Task.Factory.StartNew(() => { service.RemoveMany(toBeDeletedChunkedEntities[index]); });
            }
            
            Task.WaitAll(tasks);
            
            result = service.GetAll<SandloDbTestEntity>();
            
            //assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
        
        private IEnumerable<SandloDbTestEntity> GenerateUniqueEntities(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new SandloDbTestEntity { Name = $"Entity {i}", Price = 10.0m };
            }
        }

        private class SandloDbTestEntity : IEntity
        {
            public Guid Id { get; set; }
            public long Created { get; set; }
            public long Updated { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
            public int Index { get; set; }
            public decimal Price { get; set; }
        }
    }
}