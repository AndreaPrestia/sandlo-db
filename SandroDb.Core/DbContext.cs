using SandloDb.Core.Builders;
using SandloDb.Core.Configurations;
using SandloDb.Core.Entities;

namespace SandloDb.Core;

public sealed class DbContext
{
    private Dictionary<Type, List<object>>? _collections = new();

    private long CurrentTimestamp => new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
    private readonly object _lock = new();

    /// <summary>
    /// The entity ttl in minutes
    /// </summary>
    public int? EntityTtlMinutes { get; set; }
    
    /// <summary>
    /// The memory cleanup policy to use
    /// </summary>
    public MemoryCleanUpPolicy? MemoryCleanUpPolicy { get; set; }
    
    /// <summary>
    /// The max memory allocation in bytes for the storage
    /// </summary>
    public double? MaxMemoryAllocationInBytes { get; set; }
    
    /// <summary>
    /// Current types stored in DbContext
    /// </summary>
    public IList<Type> CurrentTypes
    {
        get
        {
            lock (_lock)
            {
                return _collections != null && _collections.Count != 0
                    ? _collections.Select(x => x.Key).ToList()
                    : new List<Type>();
            }
        }
    }

    // Static method to create an instance of the builder
    public static DbContextBuilder CreateBuilder()
    {
        return DbContextBuilder.Initialize();
    }
    
    /// <summary>
    /// Add element to storage
    /// </summary>
    /// <param name="entity"></param>
    /// <exception cref="ArgumentNullException">if entity is null</exception>
    /// <returns></returns>
    public T Add<T>(T? entity) where T : class, IEntity
    {
        lock (_lock)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var type = typeof(T);

            if (_collections == null || !_collections.ContainsKey(type))
            {
                _collections ??= new Dictionary<Type, List<object>>();

                _collections.TryAdd(type, new List<object>());
            }

            var collection = _collections[type];

            entity.Id = Guid.NewGuid();
            entity.Created = CurrentTimestamp;
            entity.Updated = CurrentTimestamp;
            collection.Add(entity);

            _collections[type] = collection;

            return entity;
        }
    }

    /// <summary>
    /// Adds elements to storage
    /// </summary>
    /// <param name="entities"></param>
    /// <exception cref="ArgumentNullException">if entities is null</exception>
    /// <returns></returns>
    public IList<T> AddMany<T>(List<T>? entities) where T : class, IEntity
    {
        lock (_lock)
        {
            ArgumentNullException.ThrowIfNull(entities);

            var type = typeof(T);

            if (_collections == null || !_collections.ContainsKey(type))
            {
                _collections ??= new Dictionary<Type, List<object>>();

                _collections.TryAdd(type, new List<object>());
            }

            var collection = _collections[type];

            foreach (var entity in entities)
            {
                entity.Id = Guid.NewGuid();
                entity.Created = CurrentTimestamp;
                entity.Updated = CurrentTimestamp;
                collection.Add(entity);
            }

            _collections[type] = collection;

            return entities;
        }
    }

    /// <summary>
    /// Update an element in the storage
    /// </summary>
    /// <param name="entity"></param>
    /// <exception cref="ArgumentNullException">if entity is null</exception>
    /// <exception cref="InvalidOperationException">if collection of type T is not found</exception>
    /// <exception cref="InvalidOperationException">if entity to update not found or we have more than one</exception>
    /// <returns></returns>
    public T Update<T>(T? entity) where T : class, IEntity
    {
        lock (_lock)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (entity.Id == Guid.Empty)
            {
                throw new ArgumentException("No id provided");
            }

            var type = typeof(T);

            if (_collections == null)
            {
                throw new InvalidOperationException($"Collection {type.Name} not found.");
            }

            var collectionExists = _collections.TryGetValue(type, out var collectionContent);

            if (!collectionExists || collectionContent == null)
            {
                throw new InvalidOperationException($"Collection {type.Name} not found.");
            }

            var collection = collectionContent.OfType<T>().ToList();

            if (collection == null)
            {
                throw new InvalidOperationException(nameof(collection));
            }

            var foundedElementIndex = collection.FindIndex(e => e.Id == entity.Id);

            if (foundedElementIndex < 0)
            {
                throw new InvalidOperationException("Entity not found.");
            }

            entity.Updated = CurrentTimestamp;

            collection[foundedElementIndex] = entity;

            _collections[type] = collection.OfType<object>().ToList();

            return entity;
        }
    }

    /// <summary>
    /// Updates a subset of entities in the storage
    /// </summary>
    /// <param name="entities"></param>
    /// <exception cref="ArgumentNullException">if entity is null</exception>
    /// <exception cref="InvalidOperationException">if collection of type T is not found</exception>
    /// <exception cref="InvalidOperationException">if entities to update not found or we have more than the provided ones</exception>
    /// <returns></returns>
    public IList<T> UpdateMany<T>(IList<T>? entities) where T : class, IEntity
    {
        lock (_lock)
        {
            ArgumentNullException.ThrowIfNull(entities);

            var type = typeof(T);

            if (_collections == null)
            {
                throw new InvalidOperationException($"Collection {type.Name} not found.");
            }

            var collectionExists = _collections.TryGetValue(type, out var collectionContent);

            if (!collectionExists || collectionContent == null)
            {
                throw new InvalidOperationException($"Collection {type.Name} not found.");
            }

            var collection = collectionContent.OfType<T>().ToList();

            if (collection == null)
            {
                throw new InvalidOperationException(nameof(collection));
            }

            foreach (var entity in entities)
            {
                if (entity.Id == Guid.Empty)
                {
                    throw new ArgumentException("No id provided");
                }

                var foundedElementIndex = collection.FindIndex(e => e.Id == entity.Id);

                if (foundedElementIndex < 0)
                {
                    throw new InvalidOperationException("Entity not found.");
                }

                entity.Updated = CurrentTimestamp;

                collection[foundedElementIndex] = entity;
            }
                
            _collections[type] = collection.OfType<object>().ToList();

            return entities;
        }
    }

    /// <summary>
    /// Remove entity from context
    /// </summary>
    /// <param name="entity"></param>
    /// <exception cref="ArgumentNullException">if entity is null</exception>
    /// <exception cref="InvalidOperationException">if entity to delete not found</exception>
    /// <returns></returns>
    public int Remove<T>(T? entity) where T : class, IEntity
    {
        lock (_lock)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var type = typeof(T);

            if (_collections == null)
            {
                throw new InvalidOperationException($"Collection {type.Name} not found.");
            }

            var collectionExists = _collections.TryGetValue(type, out var collectionContent);

            if (!collectionExists || collectionContent == null)
            {
                throw new InvalidOperationException($"Collection {type.Name} not found.");
            }

            var collection = collectionContent.OfType<T>().ToList();

            if (collection == null)
            {
                throw new InvalidOperationException(nameof(collection));
            }

            var index = collection.FindIndex(e => e.Id == entity.Id);

            if (index < 0)
            {
                throw new InvalidOperationException("Entity not found.");
            }

            collection.RemoveAt(index);

            _collections[type] = collection.OfType<object>().ToList();

            if (collection.Count == 0)
            {
                _collections.Remove(type);
            }

            return 1;
        }
    }

    /// <summary>
    /// Remove entity from context
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="type"></param>
    /// <exception cref="ArgumentNullException">if entity or type is null</exception>
    /// <exception cref="InvalidOperationException">if entity to delete not found</exception>
    /// <returns></returns>
    public int Remove(IEntity? entity, Type? type)
    {
        lock (_lock)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(type);

            if (_collections == null)
            {
                throw new InvalidOperationException($"Collection {type.Name} not found.");
            }

            var collectionExists = _collections.TryGetValue(type, out var collectionContent);

            if (!collectionExists || collectionContent == null)
            {
                throw new InvalidOperationException($"Collection {type.Name} not found.");
            }

            var collection = collectionContent.OfType<IEntity>().ToList();

            if (collection == null)
            {
                throw new InvalidOperationException(nameof(collection));
            }

            var index = collection.FindIndex(e => e.Id == entity.Id);

            if (index < 0)
            {
                throw new InvalidOperationException("Entity not found.");
            }

            collection.RemoveAt(index);

            _collections[type] = collection.OfType<object>().ToList();

            if (collection.Count == 0)
            {
                _collections.Remove(type);
            }

            return 1;
        }
    }

    /// <summary>
    /// Remove a subset of entities from context
    /// </summary>
    /// <param name="entities"></param>
    /// <exception cref="ArgumentNullException">if entities is null</exception>
    /// <exception cref="InvalidOperationException">if entities to delete not found</exception>
    /// <returns></returns>
    public int RemoveMany<T>(IList<T>? entities) where T : class, IEntity
    {
        lock (_lock)
        {
            ArgumentNullException.ThrowIfNull(entities);

            var type = typeof(T);

            if (_collections == null)
            {
                throw new InvalidOperationException($"Collection {type.Name} not found.");
            }

            var collectionExists = _collections.TryGetValue(type, out var collectionContent);

            if (!collectionExists || collectionContent == null)
            {
                throw new InvalidOperationException($"Collection {type.Name} not found.");
            }

            var collection = collectionContent.OfType<T>().ToList();

            if (collection == null)
            {
                throw new InvalidOperationException(nameof(collection));
            }

            foreach (var entity in entities)
            {
                var index = collection.FindIndex(e => e.Id == entity.Id);

                if (index < 0)
                {
                    throw new InvalidOperationException("Entity not found.");
                }

                collection.RemoveAt(index);
            }

            _collections[type] = collection.OfType<object>().ToList();

            if (collection.Count == 0)
            {
                _collections.Remove(type);
            }

            return entities.Count;
        }
    }

    /// <summary>
    /// Remove a subset of entities by type from context
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="type"></param>
    /// <exception cref="ArgumentNullException">if entities or type are null</exception>
    /// <exception cref="InvalidOperationException">if entities to delete not found</exception>
    /// <returns></returns>
    public int RemoveMany(IList<IEntity>? entities, Type? type)
    {
        lock (_lock)
        {
            ArgumentNullException.ThrowIfNull(entities);
            ArgumentNullException.ThrowIfNull(type);

            if (_collections == null)
            {
                throw new InvalidOperationException($"Collection {type.Name} not found.");
            }

            var collectionExists = _collections.TryGetValue(type, out var collectionContent);

            if (!collectionExists || collectionContent == null)
            {
                throw new InvalidOperationException($"Collection {type.Name} not found.");
            }

            var collection = collectionContent.OfType<IEntity>().ToList();

            if (collection == null)
            {
                throw new InvalidOperationException(nameof(collection));
            }

            foreach (var entity in entities)
            {
                var index = collection.FindIndex(e => e.Id == entity.Id);

                if (index < 0)
                {
                    throw new InvalidOperationException("Entity not found.");
                }

                collection.RemoveAt(index);
            }

            _collections[type] = collection.OfType<object>().ToList();

            if (collection.Count == 0)
            {
                _collections.Remove(type);
            }

            return entities.Count;
        }
    }

    /// <summary>
    /// Get all entities
    /// </summary>
    /// <returns></returns>
    public IList<T> GetAll<T>() where T : class, IEntity
    {
        lock (_lock)
        {
            var type = typeof(T);

            if (_collections == null || !_collections.ContainsKey(type))
            {
                return new List<T>();
            }

            var collectionContent = _collections[type];

            if (collectionContent == null! || !collectionContent.Any())
            {
                return new List<T>();
            }

            var collection = collectionContent.OfType<T>().ToList();

            var result = collection.Count == 0 ? new List<T>() : collection.ToList();
            return result;            }
    }

    /// <summary>
    /// Get all entities of IEntity by type
    /// </summary>
    /// <param name="type"></param>
    /// <exception cref="ArgumentNullException">if type is null</exception>
    /// <returns></returns>
    public IList<IEntity> GetAll(Type? type)
    {
        lock (_lock)
        {
            ArgumentNullException.ThrowIfNull(type);

            if (_collections == null || !_collections.ContainsKey(type))
            {
                return new List<IEntity>();
            }

            var collectionContent = _collections[type];

            if (collectionContent == null! || !collectionContent.Any())
            {
                return new List<IEntity>();
            }

            var collection = collectionContent.OfType<IEntity>().ToList();

            var result = collection.Count == 0 ? new List<IEntity>() : collection.ToList();
            return result;
        }
    }

    /// <summary>
    /// Get a subset of entities
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public IList<T> GetBy<T>(Func<T, bool>? predicate) where T : class, IEntity
    {
        lock (_lock)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            var type = typeof(T);

            if (_collections == null || !_collections.ContainsKey(type))
            {
                return new List<T>();
            }

            var collectionContent = _collections[type];

            if (collectionContent == null! || !collectionContent.Any())
            {
                return new List<T>();
            }

            var collection = collectionContent.OfType<T>().ToList();

            var result = collection.Count == 0 ? new List<T>() : collection.Where(predicate).AsQueryable().ToList();
            return result; 
        }
    }

    /// <summary>
    /// Get a subset of entities of IEntity passing the type
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="type"></param>
    /// <exception cref="ArgumentNullException">if predicate or type is null</exception>
    /// <returns></returns>
    public IList<IEntity> GetBy(Func<IEntity, bool>? predicate, Type? type)
    {
        lock (_lock)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(type);

            if (_collections == null || !_collections.ContainsKey(type))
            {
                return new List<IEntity>();
            }

            var collectionContent = _collections[type];

            if (collectionContent == null! || !collectionContent.Any())
            {
                return new List<IEntity>();
            }

            var collection = collectionContent.OfType<IEntity>().ToList();

            var result = collection.Count == 0 ? new List<IEntity>() : collection.Where(predicate).AsQueryable().ToList();
            return result; 
        }
    }

    /// <summary>
    /// Gets the first entity found by predicate
    /// </summary>
    /// <param name="predicate"></param>
    /// <exception cref="ArgumentNullException">if predicate is null</exception>
    /// <returns></returns>
    public T? Get<T>(Func<T, bool>? predicate) where T : class, IEntity
    {
        lock (_lock)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            var type = typeof(T);

            if (_collections == null || !_collections.ContainsKey(type))
            {
                return null;
            }

            var collectionContent = _collections[type];

            if (collectionContent == null! || !collectionContent.Any())
            {
                return null;
            }

            var collection = collectionContent.OfType<T>().ToList();

            var result =  collection.Count == 0 ? null : collection.FirstOrDefault(predicate);
            return result;
        }
    }

    /// <summary>
    /// Gets the first entity found by predicate
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="type"></param>
    /// <exception cref="ArgumentNullException">if predicate or type is null</exception>
    /// <returns></returns>
    public IEntity? Get(Func<IEntity, bool>? predicate, Type? type)
    {
        lock (_lock)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(type);

            if (_collections == null || !_collections.ContainsKey(type))
            {
                return null;
            }

            var collectionContent = _collections[type];

            if (collectionContent == null! || !collectionContent.Any())
            {
                return null;
            }

            var collection = collectionContent.OfType<IEntity>().ToList();
               
            var result =  collection.Count == 0 ? null : collection.FirstOrDefault(predicate);
            return result;
        }
    }

    /// <summary>
    /// Gets the first entity found by predicate
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public T? GetById<T>(Guid id) where T : class, IEntity
    {
        lock (_lock)
        {
            var type = typeof(T);

            if (_collections == null || !_collections.ContainsKey(type))
            {
                return null;
            }

            var collectionContent = _collections[type];

            if (collectionContent == null! || !collectionContent.Any())
            {
                return null;
            }

            var collection = collectionContent.OfType<T>().ToList();

            var result = collection.Count == 0 ? null : collection.FirstOrDefault(e => e.Id == id);
            return result;
        }
    }

    /// <summary>
    /// Gets the first entity found by predicate
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <exception cref="ArgumentNullException">if type is null</exception>
    /// <returns></returns>
    public IEntity? GetById(Guid id, Type? type)
    {
        lock (_lock)
        {
            ArgumentNullException.ThrowIfNull(type);

            if (_collections == null || !_collections.ContainsKey(type))
            {
                return null;
            }

            var collectionContent = _collections[type];

            if (collectionContent == null! || !collectionContent.Any())
            {
                return null;
            }

            var collection = collectionContent.OfType<IEntity>().ToList();

            var result = collection.Count == 0 ? null : collection.FirstOrDefault(e => e.Id == id);
            return result;            }
    }
}