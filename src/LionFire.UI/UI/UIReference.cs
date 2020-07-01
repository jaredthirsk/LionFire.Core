﻿using System;

namespace LionFire.UI
{
    public class UIReference
    {

        /// <summary>
        /// Must be a UI type that the ILionFireShell is capable of showing, or else a IHostedService
        /// </summary>
        public Type ViewType { get; set; }

        /// <summary>
        /// Must be a UI object that the ILionFireShell is capable of showing, or else a IHostedService
        /// </summary>
        public object View { get; set; }

        public Action ViewAction { get; set; }

        public Type ViewModelType { get; set; }

        public object ViewModel { get; set; }
    }
}
