﻿using System.Diagnostics.CodeAnalysis;

namespace SandloDb.Core.Entities;

[ExcludeFromCodeCoverage]
public sealed class DbSet<T> where T : IEntity
{
    public T? Content { get; set; }
    public int CurrentSizeInBytes { get; set; }
    public long LastUpdateTime { get; set; }
}