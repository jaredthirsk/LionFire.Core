﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LionFire.Collections {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SR {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SR() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LionFire.Collections.trenttobler.SR", typeof(SR).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Btree node capacity must be 3 or larger..
        /// </summary>
        internal static string btreeCapacityError {
            get {
                return ResourceManager.GetString("btreeCapacityError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Collection must be empty to set allow duplicates to false..
        /// </summary>
        internal static string collectionMustBeEmptyToClearAllowDuplicates {
            get {
                return ResourceManager.GetString("collectionMustBeEmptyToClearAllowDuplicates", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Duplicates are not allowed..
        /// </summary>
        internal static string duplicateNotAllowedError {
            get {
                return ResourceManager.GetString("duplicateNotAllowedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An operation that modifies the collection was invoked while IsReadOnly was true..
        /// </summary>
        internal static string immutableError {
            get {
                return ResourceManager.GetString("immutableError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Index out of range..
        /// </summary>
        internal static string indexOutOfRangeError {
            get {
                return ResourceManager.GetString("indexOutOfRangeError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Key was not found..
        /// </summary>
        internal static string keyNotFoundError {
            get {
                return ResourceManager.GetString("keyNotFoundError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Argument cannot be null..
        /// </summary>
        internal static string nullArgumentError {
            get {
                return ResourceManager.GetString("nullArgumentError", resourceCulture);
            }
        }
    }
}
