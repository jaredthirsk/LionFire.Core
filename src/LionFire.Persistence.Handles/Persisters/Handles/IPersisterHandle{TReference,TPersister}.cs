﻿#nullable enable
using LionFire.Referencing;

namespace LionFire.Persistence.Persisters
{
    public interface IPersisterHandle<TReference, TPersister> : IPersisterHandle<TReference>
        where TReference : IReference
        where TPersister : IPersister<TReference>
    {
        new TPersister? Persister { get; }
    }
}
