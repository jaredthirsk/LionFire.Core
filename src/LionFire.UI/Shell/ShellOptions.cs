﻿using System.Diagnostics;
using System.Linq;

namespace LionFire.Shell
{


    //public class ShellOptions 
    //{
    //    ///// <summary>
    //    ///// If true, Shell will invoke its own StartAsync method after IHostApplicationLifetime.ApplicationStarted fires.
    //    ///// If false, start another way such as IServicesCollection.AddSingletonHostedServiceDependency<WpfShell>()
    //    ///// </summary>
    //    //public bool AutoStart { get; set; } = false;

    //    public bool AutoClose { get; } = true;

    //}

#if MOVED // LionFire.UI
using System.Diagnostics;

namespace LionFire.Shell
{
    
    public class ShellOptions
    {
        /// <summary>
        /// If true, Shell will invoke its own StartAsync method after IHostApplicationLifetime.ApplicationStarted fires.
        /// If false, start another way such as IServicesCollection.AddSingletonHostedServiceDependency<WpfShell>()
        /// </summary>
        public bool AutoStart { get; set; } = false;

        public bool MinimizeAllOnFullScreen { get; set; } = true; 
        public bool UndoMinimizeAllOnRestore { get; set; } = true;

        /// <summary>
        /// Set to true to disable default Windows TitleBar and use the custom one.
        /// </summary>
        public bool UseCustomTitleBar { get; set; } = true;

        public int DefaultWindowWidth { get; set; } = 850;
        public int DefaultWindowHeight { get; set; } = 600;

        //public bool IsFullScreenDefault => !DevMode.IsDevMode; // TODO: Different default based on DevMode


        // FUTURE: Default sizes for different modes: PC/Tablet/etc.
        //public virtual Size DefaultWindowedSize => new Size(1368, 768);

        public bool StartMaximizedToFullScreen { get; internal set; }

        public SourceLevels DataBindingSourceLevel { get; set; } = System.Diagnostics.SourceLevels.Verbose;

        public bool StopOnMainPresenterClose { get; set; } = true; // ENH: Also make this a settable option for each UIReference in UIReference.StopShellOnClose

    }
}
#endif

}
