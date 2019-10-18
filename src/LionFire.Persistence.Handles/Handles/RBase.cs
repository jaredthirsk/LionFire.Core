﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Extensions.DefaultValues;
using LionFire.Ontology;
using LionFire.Referencing;
using LionFire.Resolvables;
using LionFire.Structures;
using LionFire.Threading;

namespace LionFire.Persistence.Handles
{

    /// <summary>
    /// Base class for read/write handles
    /// </summary>
    /// <remarks>
    ///  - Backing identity field: IReference
    ///  - PersistenceState
    ///  - ObjectReferenceChanged 
    ///  - ObjectChanged 
    /// </remarks>
    /// <typeparam name="TObject"></typeparam>
    public abstract class RBase<TObject> : Resolvable<IReference, TObject>, IReadHandleEx<TObject>, IKeyed<string>, IRetrievableImpl<TObject>, IHas<PersistenceResultFlags>
    //where ObjectType : class
    {
#error NEXT: Rename RBase to RBaseEx
#error NEXT: I just added base class of Resolvable.  Reconcile/refactor

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
            set => throw new NotImplementedException("TODO");
            //set => Reference = value.GetReference(); // TODO
        }

        #endregion

        #region Construction

        protected RBase() { }

        /// <param name="reference">Can be null</param>
        protected RBase(IReference reference)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            Reference = reference;
        }

        /// <param name="reference">Must not be null</param>
        ///// <param name="reference">If null, it should be set before the reference is used.</param>
        /// <param name="obj">Starting value for Object</param>
        protected RBase(IReference reference, TObject obj = default) : this(reference)
        {
            SetObjectFromConstructor(obj);
        }

        protected void SetObjectFromConstructor(TObject initialObject)
        {
            _object = initialObject;
            // FUTURE: In the future, we may want to do something special here, like set something along the lines of PersistenceFlags.SetByUser
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
            get => State.HasFlag(PersistenceState.UpToDate);
            set
            {
                if (value)
                {
                    State |= PersistenceState.UpToDate;
                }
                else
                {
                    State &= ~PersistenceState.UpToDate;
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

        public TObject Object
        {
            [Blocking]
            get
            {
                if (!IsPersisted)
                {
                    Retrieve().ConfigureAwait(false).GetAwaiter().GetResult();
                    //_ = Retrieve().Result;
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
        protected TObject _object;
        private object objectLock = new object();

        #region Instantiation 

        [ThreadSafe]
        public async Task<TObject> GetOrInstantiate()
        {
            await Get().ConfigureAwait(false);
            lock (objectLock)
            {
                if (!HasValue)
                {
                    Object = InstantiateDefault();
                }
                return _object;
            }
        }

        // No persistence, just instantiating an ObjectType

        /// <summary>
        /// Returns null if ObjectType is object or interface and TypedReference?.Type is null
        /// TODO: If ObjectType is Interface, get create type from attribute on Interface type.
        /// </summary>
        public Type GetInstantiationType()
        {
            if (typeof(TObject) == typeof(object))
            {
                if (TypedReference?.Type == null)
                {
                    return null;
                }
                return TypedReference.Type;
            }
            else
            {
                return typeof(TObject);
            }
        }

        private TObject InstantiateDefault(bool applyDefaultValues = true)
        {
            TObject result = (TObject)Activator.CreateInstance(GetInstantiationType() ?? throw new ArgumentNullException("Reference.Type must be set when using non-generic Handle, or when the generic type is object."));

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

        public event Action<RH<TObject>, TObject /*oldValue*/ , TObject /*newValue*/> ObjectReferenceChanged;
        public event Action<RH<TObject>> ObjectChanged;

        protected virtual void OnSavedObject() { }
        protected virtual void OnDeletedObject() { }

        protected TObject OnRetrievedObject(TObject obj)
        {
            Object = obj;
            RaiseRetrievedObject();
            this.State |= PersistenceState.UpToDate;
            return obj;
        }

        /// <summary>
        /// Reused existing Object instance, applied new retrieval results to it.
        /// </summary>
        protected void OnRetrievedObjectInPlace() => RaiseRetrievedObject();

        protected void RaiseRetrievedObject() { } // TODO

        protected void OnRetrieveFailed(IRetrieveResult<TObject> retrieveResult)
        {
            // TODO: Events?
        }

        public void DiscardObject()
        {
            Object = default;
            IsReachable = false;
            PersistenceResultFlags = PersistenceResultFlags.None;
            this.State &= ~PersistenceState.UpToDate;
        }

        #endregion

        /// <summary>
        /// Default: false for reference types, true for value types
        /// Extract to wrapper interface?
        /// If false, set_Object(default) is the same as DiscardObject(), which sets PersistenceFlags to None, and if true, it sets PersistenceFlags to HasUnpersistedObject (unless it was already Retrieved, and Object already is set to default).  TODO - Implement this
        /// </summary>
        public virtual bool CanObjectBeDefault => canObjectBeDefault;
        private static readonly bool canObjectBeDefault = typeof(TObject).IsValueType;

        //public virtual bool HasObject => CanObjectBeDefault ? (!IsDefaultValue(_object) && !this.RetrievedNullOrDefault()) : (!IsDefaultValue(_object));
        public virtual bool HasValue => State & (PersistenceState.UpToDate | PersistenceState.IncomingUpdateAvailable);

        public PersistenceResultFlags PersistenceResultFlags
        {
            get => persistenceResultFlags;
            protected set
            {
                if (persistenceResultFlags == value) return;
                persistenceResultFlags = value;
            }
        }
        private PersistenceResultFlags persistenceResultFlags;
        PersistenceResultFlags IHas<PersistenceResultFlags>.Object => PersistenceResultFlags;

        #endregion

        #region Events

        public event Action<RH<TObject>, HandleEvents> HandleEvents;

        protected void RaiseEvent(HandleEvents eventType) => HandleEvents?.Invoke(this, eventType);

        #endregion

        #region Get

        public abstract Task<IRetrieveResult<TObject>> RetrieveImpl();

        
        //async Task<IRetrieveResult<ObjectType>> IRetrievableImpl<ObjectType>.RetrieveObject() => await RetrieveObject().ConfigureAwait(false);
        public async Task<bool> Retrieve()
        {
            var result = await RetrieveImpl().ConfigureAwait(false);

            //var retrievableState = result.ToRetrievableState<TValue>(CanObjectBeDefault);
            //this.RetrievableState = retrievableState;

            this.PersistenceResultFlags = result.Flags;

            if (result.IsSuccess())
            {
                OnRetrievedObject(result.Object);
            }

            return result.IsSuccess();
        }

        async Task<(bool HasObject, T Object)> ILazilyRetrievable.Get<T>()
        {
            var result = await Get();
            return (result.HasObject, (T)(object)result.Object); // HARDCAST
        }

        /// <summary>
        /// Invokes get_Object, forcing a lazy retrieve if it was null.  Override to avoid this.
        /// If the user has set the object, then this will return true even if the object is not committed back to the source yet.
        /// </summary>
        /// <seealso cref="Exists"/>
        /// <returns>True if an object was found after a retrieval or was manually set on the handle, false otherwise.</returns>
        public virtual async Task<(bool HasObject, TObject Object)> Get()
        {
            if (HasValue)
            {
                return (true, _object);
            }

            if (!IsPersisted)
            {
                //await DoTryRetrieve().ConfigureAwait(false);
                await Retrieve().ConfigureAwait(false);
            }

            return (HasValue, _object); ;
        }

        /// <summary>
        /// Invokes get_Object, forcing a lazy retrieve if it was null.  Override to avoid this.
        /// </summary>
        /// <seealso cref="Get"/>
        /// <returns>True if an object was found after a retrieval, false otherwise.</returns>
        public virtual async Task<bool> Exists(bool forceCheck = false)
        {
            if (forceCheck)
            {
                return (await RetrieveImpl().ConfigureAwait(false)).IsFound();
            }
            else if (IsPersisted)
            {
                // Note: if delete is pending, it should set IsPersisted to false after deleting
                return true;
            }
            else
            {
                await Retrieve().ConfigureAwait(false);
                return HasValue;
            }
        }

        #endregion

        #region Misc
        private static bool IsDefaultValue(TObject value) => EqualityComparer<TObject>.Default.Equals(value, default);

        #endregion
    }
}
