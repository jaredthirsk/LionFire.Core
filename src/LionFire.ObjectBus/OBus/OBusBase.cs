﻿using System;
using System.Collections.Generic;
using System.Linq;
using LionFire.DependencyInjection;
using LionFire.Referencing;

namespace LionFire.ObjectBus
{
    public abstract class OBusBase : IOBus
    {
        public virtual IOBase SingleOBase => null;
        public virtual IOBase DefaultOBase => SingleOBase;

        //    public abstract bool IsValid(IReference reference);

        //    public virtual T InstantiateObject<T>(Func<T> createDefault = null)
        //    {
        //        T result;

        //        if (createDefault != null)
        //        {
        //            result = createDefault();
        //        }
        //        else
        //        {
        //            result = (T)Activator.CreateInstance(typeof(T));
        //        }
        //        return result;
        //    }

        public abstract IEnumerable<string> UriSchemes { get; }
        public abstract IEnumerable<Type> ReferenceTypes { get; }
        public abstract IEnumerable<Type> HandleTypes { get; }

        public abstract IOBase TryGetOBase(IReference reference);
        public abstract IReference TryGetReference(string referenceString, bool strictMode);
        public virtual bool IsValid(IReference reference) => reference != null && UriSchemes.Contains(reference.Scheme);
        bool ICompatibleWithSome<string>.IsCompatibleWith(string stringUri) => stringUri != null && UriSchemes.Where(scheme => stringUri.StartsWith(scheme)).Any();

        public virtual bool IsCompatibleWith(IReference obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (ReferenceTypes.Contains(obj.GetType()))
            {
                return true;
            }

            // TODO: If unbound reference type, resolve it here.

            return false;
        }

        public H<T> GetHandle<T>(IReference reference) => GetHandle<T>(reference, default(T));
        public virtual H<T> GetHandle<T>(IReference reference, T handleObject = default(T))
        {
            // TODO: If handle reuse is on, try to find existing handle.
            var h = new OBusHandle<T>(reference, handleObject);

            var obase = DefaultOBase;

            if (obase != null)
            {
                h.OBase = obase;
            }
            else
            {
                h.OBus = this;
            }

            return h;
        }

        public virtual RH<T> GetReadHandle<T>(IReference reference)
        {
            // TODO: If handle reuse is on, try to find existing handle.

            // TODO: create read-only handle
            var h = new OBusHandle<T>(reference);
            return h;
        }

        public virtual C<T> GetCollectionHandle<T>(IReference reference)
        {
            throw new NotImplementedException();
            //var oboc = new OBoc<T, OBaseCollectionEntry>();
        }
        public virtual RC<T> GetReadCollectionHandle<T>(IReference reference)
        {
            throw new NotImplementedException();
        }
    }

    //public abstract class OBaseProvider : IOBaseProvider
    ////where ReferenceType : IReference
    //{
    //    public abstract string[] UriSchemes { get; }

    //    public abstract IOBase GetOBase(string connectionString);

    //    public abstract string UriSchemeDefault
    //    {
    //        get;
    //    }

    //    public abstract IOBase DefaultOBase
    //    {
    //        get;
    //    }

    //    public abstract IEnumerable<IOBase> OBases
    //    {
    //        get;
    //    }

    //    public abstract IEnumerable<IOBase> GetOBases(IReference reference);


    //    public abstract IReference ToReference(string uri);

    //}
}
