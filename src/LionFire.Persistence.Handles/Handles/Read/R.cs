﻿using LionFire.Persistence.Handles;
using LionFire.Referencing;

namespace LionFire.Persistence.Handles
{
    public class R<TValue> : ReadHandlePassthrough<TValue, IReference>
    {
        public static implicit operator R<TValue>(string uri) => new R<TValue> { Reference = uri.ToReference() };
        public static implicit operator R<TValue>(TValue value)  => new R<TValue> { Reference = (value as IReferencable)?.Reference, Value = value };
    }

    public class R<TValue, TReference> : ReadHandlePassthrough<TValue, TReference>
       where TReference : class, IReference
    {
        public static implicit operator R<TValue, TReference>(TReference reference) => new R<TValue, TReference> { Reference = reference };
        public static implicit operator R<TValue, TReference>(string uri) => new R<TValue, TReference> { Reference = uri.ToReference<TReference>() };
        public static implicit operator R<TValue, TReference>(TValue value) => new R<TValue, TReference> { Reference = (value as IReferencable<TReference>)?.Reference, Value = value };
    }
}
