﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using LionFire.Assets;
using LionFire.Composables;
using LionFire.DependencyInjection;
using LionFire.Execution;
using LionFire.Execution.Composition;
using LionFire.Instantiating;
using LionFire.MultiTyping;
using LionFire.Referencing;
using LionFire.Structures;
using Microsoft.Extensions.DependencyInjection;
using TInitializable = LionFire.Execution.IInitializable;

namespace LionFire.Applications.Hosting
{
    // Derive from new ExecutionContainer, move most stuff there?
    public class AppHost : ExecutablesHost<AppHost>, IAppHost, IReadOnlyMultiTyped
    {
        private static IAppHost _CreateUnitTestApp()
        {
            if (CreateUnitTestApp == null)
            {
                return null;
            }

            IsCreatingUnitTestApp = true;
            return CreateUnitTestApp();
        }
        public static Func<IAppHost> CreateUnitTestApp = () => new AppHost().Initialize();
        private volatile static bool IsCreatingUnitTestApp = false;

        public static IAppHost MainApp { get => ManualSingleton<IAppHost>.Instance; protected set => ManualSingleton<IAppHost>.Instance = value; }

        #region MultiType

        protected readonly MultiType multiType = new MultiType();

        // REVIEW - Not sure this is needed or a good idea
        T IReadOnlyMultiTyped.AsType<T>()
        {
            switch (typeof(T).Name)
            {
                case nameof(IServiceCollection):
                    return (T)ServiceCollection;
                case nameof(IServiceProvider):
                    return (T)ServiceProvider;
                default:
                    var result = multiType.AsType<T>();
                    if (result != null)
                    {
                        return result;
                    }

                    //if (ServiceProvider == null)
                    //{
                    //    return null;
                    //}
                    //else
                    //{
                    //    return ServiceProvider.GetService<T>();
                    //}
                    return null;
            }
        }

        #endregion

        IAppHost IComposable<IAppHost>.Add<TComponent>(TComponent component) { base.Add(component); return this; }

        #region Injection

        public InjectionContext InjectionContext { get; private set; } = new InjectionContext();


        #endregion

        #region Microsoft.Extensions.DependencyInjection

        public IServiceCollection ServiceCollection { get; private set; }

        #region ServiceProvider

        public IServiceProvider ServiceProvider
        {
            get => serviceProvider;
            set
            {
                //if (IsRootApplication)
                //{
                //    if (ManualSingleton<IServiceProvider>.Instance != null && ManualSingleton<IServiceProvider>.Instance != serviceProvider)
                //    {
                //        throw new AlreadyException("ManualSingleton<IServiceProvider>.Instance already set to an unknown ServiceProvider, but this is the root AppHost.");
                //    }
                //}
                serviceProvider = value;

                if (SetManualSingletons != null)
                {
                    foreach (var type in SetManualSingletons)
                    {
                        ManualSingleton.SetInstance(serviceProvider.GetService(type), type);
                    }
                    SetManualSingletons = null;
                }

                InjectionContext.ServiceProvider = value;
            }
        }
        private IServiceProvider serviceProvider;

        public void SetManualSingletonFromService(Type serviceType)
        {
            if (!object.ReferenceEquals(MainApp, this))
            {
                return;
            }

            if (SetManualSingletons == null)
            {
                SetManualSingletons = new List<Type>();
            }

            SetManualSingletons.Add(serviceType);
        }
        private List<Type> SetManualSingletons; // MOVE to AppHostBuilder

        #endregion

        #endregion


        // TODO: Put this into the Multitype?  Maybe make it a common extensionmethod thing?
        public IDictionary<string, object> Properties { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Can be used to conduct multiple unit tests in one process.
        /// 
        /// Resets these to null:
        ///  - MainApp
        ///  - InjectionContext.Default 
        ///  - InjectionContext.Current
        ///  - ManualSingleton.Instance for all types
        /// </summary>
        public static void Reset()
        {
            MainApp = null;
            InjectionContext.Default = null;
            InjectionContext.Current = null;
            ManualSingletonRegistrar.ResetAll();
        }

        #region Construction and Initialization

        public static bool AutoMultipleAppsWithoutSpecifyingPrimaryAppFalse = true;

        private static object ctorLock = new object();

        public static bool DetectUnitTestMode = true;

        public string Name { get; set; }
        public AppHost(string name = null, bool primaryApp = true)
        {
            this.Name = name;
            lock (ctorLock)
            {
                ServiceCollection = new ServiceCollection();
                ServiceCollection.AddSingleton(typeof(IAppHost), this);

                if (MainApp == null)
                {
                    if (DetectUnitTestMode && !IsCreatingUnitTestApp && UnitTestingDetection.IsInUnitTest && CreateUnitTestApp != null)
                    {
                        _CreateUnitTestApp();
                        if (MainApp == null)
                        {
                            throw new Exception("CreateUnitTestApp did not set MainApp");
                        }
                        primaryApp = true;
                    }
                }

                if (MainApp == null)
                {
                    MainApp = this; // IsRootApplication == true
                }
                else
                {
                    if (AutoMultipleAppsWithoutSpecifyingPrimaryAppFalse)
                    {
                        primaryApp = false;
                    }

                    if (primaryApp)
                    {
                        throw new Exception("Already has a AppHost.MainApp set.  Create AppHost with primaryApp set to false to create multiple applications, or else set AppHost.MainApp to null first before creating another AppHost.");
                    }
                }

                if (primaryApp)
                {
                    if (InjectionContext.Default != null)
                    {
                        throw new Exception($"Already has an InjectionContext.Default.  Create AppHost with {nameof(primaryApp)} set to false to create multiple applications, or else set InjectionContext.Default to null first before creating another AppHost.");
                    }
                    InjectionContext.Default = InjectionContext;
                }
                else
                {
                    InjectionContext.AsyncLocal = InjectionContext;
                }
            }
        }

        

        /// <summary>
        /// Default implementation is Microsoft.Extensions.DependencyInjection.ServiceCollectionContainerBuilderExtensions.BuildServiceProvider().
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        protected virtual IServiceProvider BuildServiceProvider(IServiceCollection serviceCollection)
            => serviceCollection.BuildServiceProvider(); // Microsoft extension method


        public IAppHost InstantiateTemplates() // MOVE to ITemplateExtensions
        {
            foreach (var tComponent in children.OfType<ITemplate>().ToArray())
            {
                var component = tComponent.Create();
                Add(component);
                children.Remove(tComponent);
            }

            return this;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="mode"></param>
        ///// <returns></returns>
        //public IAppHost Bootstrap(BootstrapMode mode = BootstrapMode.Rebuild)
        //{
        //}

        public virtual void ResolveComponentDependencyProperties() => children.TryResolveDependencies(ServiceProvider);

        /// <summary>
        /// Injects ServiceProvider to components implementing IRequiresServices
        /// </summary>
        protected void InjectServiceProviderToComponents()
        {
            foreach (var component in children.OfType<IRequiresServices>())
            {
                component.ServiceProvider = ServiceProvider;
            }
        }

        /// <summary>
        /// Injects InjectionContext to components implementing IRequiresServices
        /// </summary>
        protected void InjectInjectionContextToComponents()
        {
            foreach (var component in children.OfType<IRequiresInjection>())
            {
                component.InjectionContext = InjectionContext;
            }
        }

        public IAppHost Initialize(BootstrapMode mode = BootstrapMode.Rebuild)
        {
            if (IsInitializeFrozen)
            {
                throw new Exception("AppHost can no longer be initialized");
            }

            if (IsRootApplication)
            {
                InjectionContext.Current = InjectionContext;
            }

            // FUTURE TODO: Use Resolution context?
            children.ResolveHandlesAsync().GetResultSafe();

            InstantiateTemplates();

            foreach (var configurer in children.OfType<IConfigures<IAppHost>>())
            {
                configurer.Configure(this);
            }

            foreach (var configurer in children.OfType<IConfigures<IServiceCollection>>())
            {
                configurer.Configure(ServiceCollection);
            }

            // FUTURE option: Consider reusing existing service object instances during a rebuild?

            ServiceProvider = BuildServiceProvider(ServiceCollection);

            InjectInjectionContextToComponents();
            InjectServiceProviderToComponents();

            ResolveComponentDependencyProperties();

            // LIMITATION: These can't be lazily initialized as a batch.
            children.OfType<TInitializable>().InitializeAll().Wait(); // Deprecated
            children.OfType<IInitializable2>().InitializeAll().Wait();
            children.OfType<IInitializable3>().InitializeAll().Wait();

            if (mode == BootstrapMode.Discard)
            {
                ServiceCollection.Clear();
            }

            return this;
        }

        #endregion

        #region State

        public bool IsInitializeFrozen { get; private set; } = false;

        #endregion
        #region Derived Properties

        public bool IsRootApplication => object.ReferenceEquals(this, MainApp);

        #endregion


        #region Run

        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private List<Task> WaitForTasks { get; set; } = new List<Task>();
        private List<Task> Tasks { get; set; } = new List<Task>();

        public class StateMachineWrapper<TState, TTransition>
        {
            static StateMachineWrapper()
            {

            }

        }

        public async Task Run()
        {
            Initialize();
            IsInitializeFrozen = true;

            WaitForTasks.Clear();
            Tasks.Clear();

            await children.OfType<IInitializable>().InitializeAll();
            //var validationErrors = await components.OfType<IInitializable2>().InitializeAll();

            // FUTURE: Handle 

            #region Start

            List<Task> startTasks = new List<Task>();

            foreach (var component in children.OfType<IStartable>())
            {
                // Parallel start
                startTasks.Add(component.Start(tokenSource.Token));
            }

            Task.WaitAll(startTasks.ToArray());

            #endregion


            foreach (var component in children.OfType<IHasRunTask>())
            {
                if (component.WaitForRunCompletion() == true && component.RunTask != null)
                {
                    WaitForTasks.Add(component.RunTask);
                }
                if (component.RunTask != null)
                {
                    Tasks.Add(component.RunTask);
                }
            }

            await WaitForShutdown();
        }

        #endregion


        #region Shutdown

        //public int ShutdownMillisecondsDelay = 60000;  // FUTURE?

        public Task Shutdown(long millisecondsTimeout = 0)
        {
            tokenSource.Cancel();
            var shutdownTask = Task.Factory.ContinueWhenAll(Tasks.ToArray(), _ => { Tasks.Clear(); });
            return shutdownTask;
        }

        public Task WaitForShutdown()
        {
            var tasks = WaitForTasks;

            if (tasks.Count > 0)
            {
                var shutdownTask = Task.Factory.ContinueWhenAll(tasks.ToArray(), _ => { });
                return shutdownTask;
            }
            else
            {
                return Task.CompletedTask;
            }
        }




        #endregion
    }


}
