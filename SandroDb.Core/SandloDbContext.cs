using System.Collections.Concurrent;

namespace SandloDb.Core
{
    public sealed class SandloDbContext
    {
        private ConcurrentDictionary<Type, ConcurrentBag<object>>? _collections = new();
        private long CurrentTimestamp => new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

        /// <summary>
        /// Current types stored in SandloDbContext
        /// </summary>
        public IList<Type> CurrentTypes => _collections != null && _collections.Any()
            ? _collections.Select(x => x.Key).ToList()
            : new List<Type>();

        /// <summary>
        /// Add element to storage
        /// </summary>
        /// <param name="entity"></param>
        /// <exception cref="ArgumentNullException">if entity is null</exception>
        /// <returns></returns>
        public T Add<T>(T? entity) where T : class, IEntity
        {
            ArgumentNullException.ThrowIfNull(entity);

            var type = typeof(T);

            if (_collections == null || !_collections.ContainsKey(type))
            {
                _collections ??= new ConcurrentDictionary<Type, ConcurrentBag<object>>();

                _collections.TryAdd(type, new ConcurrentBag<object>());
            }

            var collection = _collections[type];

            entity.Id = Guid.NewGuid();
            entity.Created = CurrentTimestamp;
            entity.Updated = CurrentTimestamp;
            collection.Add(entity);

            _collections[type] = collection;

            return entity;
        }

        /// <summary>
        /// Adds elements to storage
        /// </summary>
        /// <param name="entities"></param>
        /// <exception cref="ArgumentNullException">if entities is null</exception>
        /// <returns></returns>
        public IList<T> AddMany<T>(List<T>? entities) where T : class, IEntity
        {
            ArgumentNullException.ThrowIfNull(entities);

            var type = typeof(T);

            if (_collections == null || !_collections.ContainsKey(type))
            {
                _collections ??= new ConcurrentDictionary<Type, ConcurrentBag<object>>();

                _collections.TryAdd(type, new ConcurrentBag<object>());
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

            var collection = new ConcurrentBag<T>(collectionContent.OfType<T>());

            if (collection == null)
            {
                throw new InvalidOperationException(nameof(collection));
            }

            var foundedElements = collection.Where(e => e.Id == entity.Id).ToList();

            if (foundedElements == null || !foundedElements.Any())
            {
                throw new InvalidOperationException("Entity not found.");
            }

            if (foundedElements.Count > 1)
            {
                throw new InvalidOperationException("More than one entity found.");
            }

            entity.Updated = CurrentTimestamp;

            return entity;
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

            var collection = new ConcurrentBag<T>(collectionContent.OfType<T>());

            if (collection == null)
            {
                throw new InvalidOperationException(nameof(collection));
            }

            var foundedElements = collection.Where(e => entities.Select(et => et.Id).Contains(e.Id)).ToList();

            if (foundedElements == null || !foundedElements.Any())
            {
                throw new InvalidOperationException("Entity not found.");
            }

            if (foundedElements.Count > entities.Count)
            {
                throw new InvalidOperationException("More than one entity found.");
            }

            foreach (var entity in entities)
            {
                entity.Updated = CurrentTimestamp;
            }

            return entities;
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

            var collection = new ConcurrentBag<T>(collectionContent.OfType<T>());

            if (collection == null)
            {
                throw new InvalidOperationException(nameof(collection));
            }

            var count = collection.Count(e => e.Id == entity.Id);

            if (count == 0)
            {
                throw new InvalidOperationException("Entity not found.");
            }

            collection = new ConcurrentBag<T>(collection.Where(e => e.Id != entity.Id));

            if (collection.IsEmpty)
            {
                _collections.TryRemove(type, out _);
            }

            return count;
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

            var collection = new ConcurrentBag<IEntity>(collectionContent.OfType<IEntity>());

            if (collection == null)
            {
                throw new InvalidOperationException(nameof(collection));
            }

            var count = collection.Count(e => e.Id == entity.Id);

            if (count == 0)
            {
                throw new InvalidOperationException("Entity not found.");
            }

            collection = new ConcurrentBag<IEntity>(collection.Where(e => e.Id != entity.Id));

            if (collection.IsEmpty)
            {
                _collections.TryRemove(type, out _);
            }

            return count;
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

            var collection = new ConcurrentBag<T>(collectionContent.OfType<T>());

            if (collection == null)
            {
                throw new InvalidOperationException(nameof(collection));
            }

            var count = collection.Count(e => entities.Select(et => et.Id).Contains(e.Id));

            if (count == 0)
            {
                throw new InvalidOperationException("Entities not found.");
            }

            collection = new ConcurrentBag<T>(collection.Where(e => !entities.Select(et => et.Id).Contains(e.Id)));

            if (collection.IsEmpty)
            {
                _collections.TryRemove(type, out _);
            }

            return count;
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

            var collection = new ConcurrentBag<IEntity>(collectionContent.OfType<IEntity>());

            if (collection == null)
            {
                throw new InvalidOperationException(nameof(collection));
            }

            var count = collection.Count(e => entities.Select(et => et.Id).Contains(e.Id));

            if (count == 0)
            {
                throw new InvalidOperationException("Entities not found.");
            }

            collection = new ConcurrentBag<IEntity>(collection.Where(e => !entities.Select(et => et.Id).Contains(e.Id)));

            if (collection.IsEmpty)
            {
                _collections.TryRemove(type, out _);
            }

            return count;
        }

        /// <summary>
        /// Get all entities
        /// </summary>
        /// <returns></returns>
        public IList<T> GetAll<T>() where T : class, IEntity
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

            var collection = new ConcurrentBag<T>(collectionContent.OfType<T>());

            return !collection.Any() ? new List<T>() : collection.ToList();
        }

        /// <summary>
        /// Get all entities of IEntity by type
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="ArgumentNullException">if type is null</exception>
        /// <returns></returns>
        public IList<IEntity> GetAll(Type? type)
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

            var collection = new ConcurrentBag<IEntity>(collectionContent.OfType<IEntity>());

            return !collection.Any() ? new List<IEntity>() : collection.ToList();
        }

        /// <summary>
        /// Get a subset of entities
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IList<T> GetBy<T>(Func<T, bool>? predicate) where T : class, IEntity
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

            var collection = new ConcurrentBag<T>(collectionContent.OfType<T>());

            return !collection.Any() ? new List<T>() : collection.Where(predicate).ToList();
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

            var collection = new ConcurrentBag<IEntity>(collectionContent.OfType<IEntity>());

            return !collection.Any() ? new List<IEntity>() : collection.Where(predicate).ToList();
        }

        /// <summary>
        /// Gets the first entity found by predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <exception cref="ArgumentNullException">if predicate is null</exception>
        /// <returns></returns>
        public T? Get<T>(Func<T, bool>? predicate) where T : class, IEntity
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

            var collection = new ConcurrentBag<T>(collectionContent.OfType<T>());

            return !collection.Any() ? null : collection.FirstOrDefault(predicate);
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

            var collection = new ConcurrentBag<IEntity>(collectionContent.OfType<IEntity>());

            return !collection.Any() ? null : collection.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Gets the first entity found by predicate
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T? GetById<T>(Guid id) where T : class, IEntity
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

            var collection = new ConcurrentBag<T>(collectionContent.OfType<T>());

            return !collection.Any() ? null : collection.FirstOrDefault(e => e.Id == id);
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

            var collection = new ConcurrentBag<IEntity>(collectionContent.OfType<IEntity>());

            return !collection.Any() ? null : collection.FirstOrDefault(e => e.Id == id);
        }
    }
}