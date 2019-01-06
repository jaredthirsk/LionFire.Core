﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Collections;
using LionFire.DependencyInjection;
using LionFire.Extensions.DefaultValues;
using LionFire.ObjectBus;
using LionFire.Persistence;
using LionFire.Structures;

namespace LionFire.Referencing
{
    

    public abstract class RCollectionBase<TCollection, TItem> : RBase<TCollection>, IReadOnlyCollection<TItem>
        where TCollection : IEnumerable<TItem>
    {
        public abstract int Count { get; }

        public abstract IEnumerator<TItem> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        public abstract void OnCollectionChangedEvent(INotifyCollectionChangedEventArgs<TItem> a);
    }

    //public abstract class HCollectionBase<ObjectType, T> : WBase<ObjectType>, ICollection<T>
    //    where ObjectType : IEnumerable<T>
    //{
    //    public abstract int Count { get; }
    //    public abstract bool IsReadOnly { get; }

    //    public abstract void Add(T item);
    //    public abstract bool Remove(T item);
    //    public abstract void Clear();

    //    public abstract bool Contains(T item);

    //    public abstract void CopyTo(T[] array, int arrayIndex);

    //    public abstract IEnumerator<T> GetEnumerator();
    //    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    //}

    //public class WritableCollectionExperiment<T> : HCollectionBase<List<T>, T>
    //{
    //    MultiBindableCollection<T> collection = new MultiBindableCollection<T>();
        
    //    public override int Count => collection.Count;

    //    public override bool IsReadOnly => collection.IsReadOnly;

    //    public override void Add(T item) => collection.Add(item);
    //    public override void Clear() => collection.Clear();
    //    public override bool Contains(T item) => collection.Contains(item);
    //    public override void CopyTo(T[] array, int arrayIndex) => collection.CopyTo(array, arrayIndex);
    //    public override Task DeleteObject(object persistenceContext = null) => collection.DeleteObject(persistenceContext);
    //    public override IEnumerator<T> GetEnumerator() => collection.GetEnumerator();
    //    public override bool Remove(T item) => collection.Remove();
    //    public override Task<bool> TryRetrieveObject() => collection.TryRetrieveObject();
    //    public override Task WriteObject(object persistenceContext = null) => collection.WriteObject(persistenceContext);
    //}

    /// <summary>
    /// Base class for read/write handles
    /// </summary>
    /// <remarks>
    ///  - Backing identity field: IReference
    ///  - PersistenceState
    ///  - ObjectReferenceChanged 
    ///  - ObjectChanged 
    /// </remarks>
    /// <typeparam name="ObjectType"></typeparam>
    public abstract class RBase<ObjectType> : RH<ObjectType>, IKeyed<string>
    //where ObjectType : class
    {
        /// <summary>
        /// Reference only allows types assignable to types in this Enumerable
        /// </summary>
        public virtual IEnumerable<Type> AllowedReferenceTypes => null;

        #region Identity

        #region Reference

        public IReference Reference
        {
            get => reference;
            protected set
            {
                if (reference == value)
                {
                    return;
                }

                if (reference != default(IReference))
                {
                    throw new AlreadySetException();
                }

                var art = AllowedReferenceTypes;
                if (art != null && value != null && !art.Where(type => type.IsAssignableFrom(value.GetType())).Any())
                {
                    throw new ArgumentException("This type does not support IReferences of that type.  See AllowedReferenceTypes for allowed types.");
                }

                reference = value;
            }
        }
        protected IReference reference;
        public ITypedReference TypedReference => Reference as ITypedReference;

        #endregion

        public string Key
        {
            get => Reference.Key;
            set => Reference = value.ToReference();
        }

        #endregion

        #region Construction

        protected RBase() { }

        /// <param name="reference">Can be null</param>
        protected RBase(IReference reference) { Reference = reference; }

        /// <param name="reference">(Can be null)</param>
        /// <param name="obj">Starting value for Object</param>
        public RBase(IReference reference, ObjectType obj = default(ObjectType)) : this(reference)
        {
            _object = obj;
        }

        #endregion

        #region State

        #region State

        public PersistenceState State
        {
            get => handleState;
            set
            {
                if (handleState == value)
                {
                    return;
                }

                var oldValue = handleState;
                handleState = value;

                RaiseEvent(LionFire.HandleEvents.StateChanged);
                StateChanged?.Invoke(oldValue, value);
            }
        }
        private PersistenceState handleState;

        public event PersistenceStateChangeHandler StateChanged;

        #region Derived - Convenience

        public bool IsPersisted
        {
            get => State.HasFlag(PersistenceState.Persisted);
            set
            {
                if (value)
                {
                    State |= PersistenceState.Persisted;
                }
                else
                {
                    State &= ~PersistenceState.Persisted;
                }
            }
        }

        #region Reachable

        public bool? IsReachable
        {
            get => State.HasFlag(PersistenceState.Reachable) ? true : (State.HasFlag(PersistenceState.Reachable) ? false : (bool?)null);
            set
            {
                if (value.HasValue)
                {
                    if (value.Value)
                    {
                        State |= PersistenceState.Reachable;
                        State &= ~PersistenceState.Unreachable;
                    }
                    else
                    {
                        State |= PersistenceState.Unreachable;
                        State &= ~PersistenceState.Reachable;
                    }
                }
                else
                {
                    State &= ~(PersistenceState.Reachable | PersistenceState.Unreachable);
                }
            }
        }

        #endregion

        #endregion

        #endregion

        #region Object

        public ObjectType Object
        {
            [Blocking]
            get
            {
                if (!IsPersisted)
                {
                    TryRetrieveObject().ConfigureAwait(false).GetAwaiter().GetResult();
                }
                return _object;
            }
            set
            {
                if (object.ReferenceEquals(_object, value))
                {
                    return;
                }

                var oldValue = _object;
                _object = value;
                ObjectReferenceChanged?.Invoke(this, oldValue, value);
                ObjectChanged?.Invoke(this);
            }
        }
        protected ObjectType _object;

        public async Task<ObjectType> GetObject()
        {
            if (!IsPersisted)
            {
                await TryRetrieveObject().ConfigureAwait(false);
            }
            // TOTEST NOTE / REVIEW: Consider case where IsPersisted is true and _object is null.  How can this happen?
            return _object;
        }
        public async Task<ObjectType> GetObjectOrInstantiate()
        {
            var result = await GetObject().ConfigureAwait(false);
            if (!HasObject)
            {
                Object = result = InstantiateDefault();
            }
            return result;
        }

        private static bool IsDefaultValue(ObjectType value) => EqualityComparer<ObjectType>.Default.Equals(value, default(ObjectType));

        #region Instantiation 

        // No persistence, just instantiating an ObjectType

        /// <summary>
        /// Returns null if ObjectType is object or interface and TypedReference?.Type is null
        /// TODO: If ObjectType is Interface, get create type from attribute on Interface type.
        /// </summary>
        public Type GetInstantiationType()
        {
            if (typeof(ObjectType) == typeof(object))
            {
                if (TypedReference?.Type == null)
                {
                    return null;
                }
                return TypedReference.Type;
            }
            else
            {
                return typeof(ObjectType);
            }
        }

        private ObjectType InstantiateDefault(bool applyDefaultValues = true)
        {
            ObjectType result = (ObjectType)Activator.CreateInstance(GetInstantiationType() ?? throw new ArgumentNullException("Reference.Type must be set when using non-generic Handle, or when the generic type is object."));

            if (applyDefaultValues) { DefaultValueUtils.ApplyDefaultValues(result); }

            return result;
        }

        public void InstantiateAndSet(bool applyDefaultValues = true) => Object = InstantiateDefault(applyDefaultValues);
        private void InstantiateAndSetWithoutEvents(bool applyDefaultValues = true) => _object = InstantiateDefault(applyDefaultValues);

        public void EnsureInstantiated() // REVIEW: What should be done here?
        {
            //RetrieveOrCreateDefault(); ??

            if (Object == null)
            {
                InstantiateAndSet();
            }
        }
        private void EnsureInstantiatedWithoutEvents() // REVIEW: What should be done here?
        {
            if (_object == null)
            {
                InstantiateAndSetWithoutEvents();
            }
        }

        #endregion

        //protected virtual async Task<bool> DoTryRetrieve()
        //{
        //    return 
        //    bool result;
        //    if (!(result = (await TryRetrieveObject().ConfigureAwait(false))))
        //    {
        //        OnRetrieveFailed();
        //    }
        //    return result;
        //}

        public event Action<RH<ObjectType>, ObjectType /*oldValue*/ , ObjectType /*newValue*/> ObjectReferenceChanged;
        public event Action<RH<ObjectType>> ObjectChanged;

        protected virtual void OnSavedObject() { }
        protected virtual void OnDeletedObject() { }

        protected void OnRetrievedObject(ObjectType obj) => Object = obj; // TODO FUTURE: Bypass events, or trigger different events (don't trigger "user changed", but instead "retrieved")
        protected void OnRetrievedObjectInPlace() => throw new NotImplementedException("TODO - raise Object changed events?"); 
        protected void OnRetrieveFailed(IRetrieveResult<ObjectType> retrieveResult)
        {
            // TODO: Events?
        }

        public bool HasObject => !IsDefaultValue(_object);

        public void ForgetObject()
        {
            Object = default(ObjectType);
            IsReachable = false;
        }

        #endregion


        #endregion

        #region Events

        public event Action<RH<ObjectType>, HandleEvents> HandleEvents;

        protected void RaiseEvent(HandleEvents eventType) => HandleEvents?.Invoke(this, eventType);

        #endregion

        #region Retrieve

        public abstract Task<bool> TryRetrieveObject();

        /// <summary>
        /// Invokes get_Object, forcing a lazy retrieve if it was null.  Override to avoid this.
        /// If the user has set the object, then this will return true even if the object is not committed back to the source yet.
        /// </summary>
        /// <seealso cref="Exists"/>
        /// <returns>True if an object was found after a retrieval or was manually set on the handle, false otherwise.</returns>
        public virtual async Task<bool> TryGetObject()
        {
            if (HasObject)
            {
                return true;
            }

            if (!IsPersisted)
            {
                //await DoTryRetrieve().ConfigureAwait(false);
                await TryRetrieveObject().ConfigureAwait(false);
            }

            return HasObject;
        }

        /// <summary>
        /// Invokes get_Object, forcing a lazy retrieve if it was null.  Override to avoid this.
        /// </summary>
        /// <seealso cref="TryGetObject"/>
        /// <returns>True if an object was found after a retrieval, false otherwise.</returns>
        public virtual async Task<bool> Exists(bool forceCheck = false)
        {
            if (forceCheck)
            {
                return await TryRetrieveObject().ConfigureAwait(false);
            }
            else if (IsPersisted)
            {
                // Note: if delete is pending, it should set IsPersisted to false after deleting
                return true;
            }
            else
            {
                await TryRetrieveObject().ConfigureAwait(false);
                return HasObject;
            }
        }

        #endregion

    }
}