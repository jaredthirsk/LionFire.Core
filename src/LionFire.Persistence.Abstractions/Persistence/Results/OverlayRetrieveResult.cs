﻿namespace LionFire.Persistence
{

    public class OverlayRetrieveResult<T> : OverlayPersistenceResultBase<IRetrieveResult<T>>, IRetrieveResult<T>, ITieredPersistenceResult
    {
        public OverlayRetrieveResult(IRetrieveResult<T> underlyingResult) : base(underlyingResult)
        {
        }

        public T Value => underlyingResult.Value;


    }
}