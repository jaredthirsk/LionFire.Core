﻿// -- Experimental service supervisor concept ...?
using LionFire.Execution;
using LionFire.Execution.Executables;
using LionFire.Instantiating;
using LionFire.Ontology;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Execution // MOVE to Execution dll
{
    [Flags]
    public enum ServiceSupervisorFlags
    {
        None = 0,
        IngestAppServices = 1 << 0,
    }

    public class TServiceSupervisor : ITemplate
    {
        public TServiceSupervisor()
        {
        }

        public const ServiceSupervisorFlags DefaultFlags = ServiceSupervisorFlags.IngestAppServices;

        [DefaultValue(DefaultFlags)]
        public ServiceSupervisorFlags Flags { get; set; } = DefaultFlags;
    }

    public enum OperationalStatus
    {
        Unspecified,
        Normal,
        Caution,
        Fault,
        UrgentWarning,
    }

    public interface IHasOperationalStatus
    {
        OperationalStatus OperationalStatus { get; }
        event Action<OperationalStatus, OperationalStatus, IHasOperationalStatus> OperationalStatusChangedFromToFor;
    }

    /// <summary>
    /// Oversees a set of services in a process.  By default it monitors all of the top level services, and raises a fault
    /// </summary>
    public class ServiceSupervisor : ExecutableBase, IStartable, IHasOperationalStatus, IParented
    {
        #region Parent

        public object Parent
        {
            get { return parent; }
            set
            {
                if (parent == value) return;
                if (parent != default(object)) throw new AlreadyException();
                parent = value;
            }
        }
        private object parent;

        public IEnumerable<object> ParentObjects => Parent as IEnumerable<object>;

        #endregion

        #region OperationalStatus

        public OperationalStatus OperationalStatus
        {
            get { return operationalStatus; }
            set
            {
                if (operationalStatus == value) return;
                var oldValue = operationalStatus;
                operationalStatus = value;
                OperationalStatusChangedFromToFor?.Invoke(oldValue, operationalStatus, this);
            }
        }
        private OperationalStatus operationalStatus;

        #endregion

        public event Action<OperationalStatus, OperationalStatus, IHasOperationalStatus> OperationalStatusChangedFromToFor;

        public Task Start()
        {
            if (ParentObjects != null)
            {
                foreach (var s in ParentObjects.OfType<IExecutableEx>())
                {
                    s.StateChangedToFor += S_StateChangedToFor;
                    UpdateState(s as IExecutableEx);
                }
            }
            return Task.CompletedTask;
        }

        private void S_StateChangedToFor(ExecutionStateEx arg1, object arg2)
        {
            UpdateState((IExecutableEx)arg2);
        }

        private void UpdateState(IExecutableEx arg2)
        {
            if (arg2 == null) return;

        }
    }
}
