﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LionFire.Copying;
using LionFire.Ontology;
using LionFire.Persistence;
using LionFire.Serialization;
using LionFire.Structures;

namespace LionFire.Instantiating
{
    /// <summary>
    /// Represents the intent to instantiate an object from a template, and optionally template parameters and
    /// optionally state as understood by the instance.
    /// REVIEW - should this have anything to do with IInstantiation?  If so, maybe rename to Instantiation
    /// </summary>
    [LionSerializable(SerializeMethod.ByValue)] // REVIEW
    public abstract class InstantiationBase : IInstantiation, IParentedTemplateParameters, IParented
    {

        #region Ontology - TODO - both Key and Pid???

        #region Parent

        #region Parent

        [Ignore]
        public object Parent { get; set; }

        #endregion

        [SerializeDefaultValue(false)]
        public string ParentKey
        {
            get;
            set;
        }

        #endregion

        #region Key

        // object IKeyed<string>.Key { get { return Key; } }

        [SerializeDefaultValue(false)]
        public abstract string Key { get; set; }

        #endregion

        #region Pid

        [DefaultValue(0)]
        [SerializeDefaultValue(false)]
        public short Pid
        {
            get { return pid; }
            set
            {
                if (pid == value) return;
                if (value != 0 && pid != 0) throw new NotSupportedException("Pid can only be set once (unless first set back to default).");
                pid = value;
            }
        }
        private short pid = 0;
        public IPidRoot PidRoot => this.ParentOfType<IPidRoot>();


        public bool HasPid { get { return pid != 0; } }

        public void EnsureHasPid()
        {
            if (!TryEnsureHasPid()) throw new Exception("Failed to get Pid: " + this.ToStringSafe());
        }

        public bool TryEnsureHasPid()
        {
            if (Pid == 0)
            {
                var pidRoot = PidRoot;
                if (pidRoot != null)
                {
                    Pid = pidRoot.KeyKeeper.GetNextKey();
                    return Pid != 0;
                }
                else { return false; }
            }
            else
            {
                return true;
            }
        }
        #endregion

        #endregion

        /// <summary>
        /// OverlayTargets: by default, the template's IInstantiation tree
        /// </summary>
        public virtual IEnumerable<IEnumerable<IInstantiation>> OverlayTargets
        {
            get
            {
                throw new NotImplementedException("TOPORT");
                //if (Template != null && Template.Object != null)
                //{
                //    IHierarchicalTemplate hierarchicalTemplate = Template.Object as IHierarchicalTemplate;
                //    if (hierarchicalTemplate != null && hierarchicalTemplate.Children != null)
                //    {
                //        yield return hierarchicalTemplate.Children;
                //        //foreach (var child in hierarchicalTemplate.Children)
                //        //{
                //        //    yield return child;
                //        //}
                //    }
                //}
                ////var templateOverlayable = this.Parent as ITemplateOverlayable;
                ////if (templateOverlayable != null)
                ////{
                ////    foreach (var target in templateOverlayable.OverlayTargets) { yield return target; }
                ////}
                //yield break;
            }
        }

        #region Prototype

        [SerializeDefaultValue(false)]
        public ICloneable Prototype { get; set; }

        #endregion

        #region Template

        ITemplate IHasTemplate.Template { get => Template?.Value; set => throw new NotImplementedException(); }


        //public ITemplate TemplateObject
        //{
        //    get
        //    {
        //        if (Template != null && Template.Object != null)
        //        {
        //            return Template.Object;
        //        }
        //        //if (TemplateHandle != null)
        //        //{
        //        //    return TemplateHandle.Object;
        //        //}
        //        return null;
        //    }
        //}
        //#region TemplateHandle

        ////public IReadHandle<ITemplate> TemplateHandle
        ////{
        ////    get { return templateHandle; }
        ////    set
        ////    {
        ////        if (templateHandle == value) return;
        ////        if (templateHandle != default(IReadHandle<ITemplate>)) throw new NotSupportedException("TemplateHandle can only be set once.");
        ////        templateHandle = value;
        ////    }
        ////} private IReadHandle<ITemplate> templateHandle;

        //#endregion

        //public IReadHandle TemplateAH
        //{
        //    get;
        //    set;
        //}

        #region Template

        public object TemplateObj
        {
            get
            {
                if (template != null && template.GetType() == typeof(string))
                    throw new UnreachableCodeException("get_TemplateObj - got string: " + (string)(object)template);

                return template;
            }
        }


        //#if !AOT && !UNITY // Unity crashes with contravariant IReadHandle -- commented out the generic part of RH<>
        [Assignment(AssignmentMode.Assign)]
        public IReadHandleBase<ITemplate> Template
        {
            get
            {
#if RuntimeDebug
                if (template != null && template.GetType() == typeof(string))
                    throw new UnreachableCodeException("get_Template - got string: " + (string)(object)template);
#endif
                if (template == null && !object.ReferenceEquals(Parameters, this))
                {
                    return Parameters.Template;
                }
                return template;
            }
            set
            {
                if ((template == null && value == null) || (template != null && value != null && template.Equals(value))) return;
                //if (template != null) throw new NotSupportedException("Can only be set once.");
                if (template != null && value != null) throw new AlreadySetException();
                if (value != null && value.GetType() == typeof(string))
                    throw new UnreachableCodeException("set_Template - got string: " + (string)(object)value);
                template = value;

                //if (template != null && this.Key != null)
                //{
                //    this.TryGetOverlayParent();
                //}
            }
        }
        protected IReadHandleBase<ITemplate> template;

        #endregion


        //public Reference<ITemplate> Template
        //{
        //    get { return template; }
        //    set
        //    {
        //        if (template == value) return;
        //        if (template != default(Reference<ITemplate>)) throw new NotSupportedException("Template can only be set once.");
        //        template = value;
        //    }
        //} private Reference<ITemplate> template;

        #endregion

        [SerializeDefaultValue(false)]
        public virtual ITemplateParameters Parameters { get; set; }

        #region Overlaying

        //public virtual ParameterOverlayMode OverlayMode { get { if (Parameters == null) return ParameterOverlayMode.None; return Parameters.OverlayMode; } set { throw new NotSupportedException("Cannot set OverlayMode on Instantiation."); } }

        //public virtual object OverlayParent
        //{
        //    get
        //    {
        //        if (Parameters == null)
        //        {
        //            return null;
        //        }
        //        return Parameters.OverlayParent;
        //    }
        //    set
        //    {
        //        if (Parameters == null) { throw new InvalidOperationException("Parameters == null"); }
        //        Parameters.OverlayParent = value;
        //    }
        //}

        #region OverlayMode

        [DefaultValue(ParameterOverlayMode.None)]
        [SerializeDefaultValue(false)]
        public ParameterOverlayMode OverlayMode
        {
            get { return overlayMode; }
            set { overlayMode = value; }
        }
        private ParameterOverlayMode overlayMode = ParameterOverlayMode.None;

        #endregion

        #region OverlayParent

        [Ignore]
        [SerializeDefaultValue(false)]
        public object OverlayParent
        {
            get
            {
                return _overlayParent;
            }
            set { _overlayParent = value; }
        }
        private object _overlayParent;

        #endregion

        #endregion

        [Ignore]
        public Func<Instantiation, string> GetDefaultKey = GetDefaultKeyMethodDefault;

        public static Func<Instantiation, string> GetDefaultKeyMethodDefault = ins =>
            {
                if (ins == null) return null;

                if (ins.Parameters is IDefaultInstanceKeyProvider provider) { return provider.DefaultKey; }

                provider = ins.Template as IDefaultInstanceKeyProvider;
                if (provider != null) { return provider.DefaultKey; }

                return null;
            };

        //[Ignore]
        [SerializeDefaultValue(false)]
        public virtual object State { get; set; }

        /// <summary>
        /// REVIEW - A way to avoid this is to break up Create into Construct and InitializeState
        /// </summary>
        /// <remarks>
        /// Valor: This is set by PortalSpawner.Spawn* methods.
        /// </remarks>
        [Ignore]
#if AOT
		public ActionObject InitializationMethod { get; set; }
#else
        public Action<object> InitializationMethod { get; set; }
#endif

        #region Children

        public bool HasChildren { get { return children != null && children.Count > 0; } }

        //private static readonly List<Instance> defaultList = new List<Instance>();
        [SerializeDefaultValue(false)]
        //[JsonExSerializer.JsonExDefault(defaultList)]
        //[JsonExSerializer.JsonExDefaultValues(typeof(List<Instance>), )]
        [LionSerializable(SerializeMethod.ByValue)]
        public IInstantiationCollection Children
        {
            get { if (children == null) { Children = new InstantiationCollection(); } return children; }
            set
            {
                if (children == value) return;
                children = value;
                children.Parent = this;
            }
        }
        private IInstantiationCollection children;

        #endregion

        #region IEnumerable

        public IEnumerator GetEnumerator()
        {
            if (children != null)
            {
                foreach (IInstantiation child in this.children.Values)
                {
                    yield return child;

                    //foreach (IInstantiation result in child)
                    //{
                    //    yield return result;
                    //}
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
        bool IHasPid.TryEnsureHasPid() => throw new NotImplementedException();
        void IHasPid.EnsureHasPid() => throw new NotImplementedException();

        #endregion

        #region AllChildren

        public IEnumerable
#if !AOT
            <IInstantiation>
#endif
                AllChildren
        {
            get
            {
                if (HasChildren)
                {
#if AOT
					foreach (LionFire.IEnumerableExtensions.Recursion child in (this.Children.SelectRecursive(x => ((IInstantiation)x).GetChildrenEnumerable())))
					{
						yield return child.Item;
					}
#else
                    foreach (var child in (this.Children.SelectRecursive(x => x.GetChildrenEnumerable()).Select(r => r.Item)))
                    {
                        yield return child;
                    }
#endif
                }
            }
        }

        IInstantiationCollection IInstantiation.Children { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IInstantiationBase.Key { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ICloneable IInstantiationBase.Prototype { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        object IInstantiationBase.TemplateObj => throw new NotImplementedException();

        ITemplateParameters IInstantiationBase.Parameters { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Action<object> IInstantiationBase.InitializationMethod { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        bool IInstantiationBase.HasChildren => throw new NotImplementedException();

        object IStateful.State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        string IKeyed<string>.Key => throw new NotImplementedException();

        short IHasPid.Pid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        bool IHasPid.HasPid => throw new NotImplementedException();

        object ITemplateOverlayable.OverlayParent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ParameterOverlayMode ITemplateOverlayable.OverlayMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IEnumerable<IEnumerable<IInstantiation>> ITemplateOverlayable.OverlayTargets => throw new NotImplementedException();

        object IParented.Parent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #endregion

    }

}
