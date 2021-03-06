﻿#if TODO
//#define ASSETS_SUBPATH // Prefer this off?  TODO - make sure this works for packages

using System;
using System.Collections.Generic;
using System.Linq;
using LionFire.Persistence.Filesystem;
using LionFire.Referencing;
using LionFire.Vos.Mounts;
using Microsoft.Extensions.Logging;

namespace LionFire.Vos.VosApp
{
    public class StoreMounter
    {
        public static IEnumerable<IReference> Locations
        {
            get
            {
                yield return VosDiskPaths.UserSharedStoresRoot.ToFileReference();
            }
        }
        public static IEnumerable<IReference> GlobalLocations
        {
            get
            {
#if UNITY
                yield break;
#else
                yield return VosDiskPaths.GlobalSharedStoresRoot.ToFileReference();

#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="createIfMissing"></param>
        /// <param name="mountAsStoreName"></param>
        /// <param name="useGlobal">FUTURE: If true, use or create the global store.  If false, use or create the user store.  If null, load either or create user if none exists.</param>
        /// <returns></returns>
        public static Mount Mount(string storeName, bool createIfMissing = true, string mountAsStoreName = null, bool? useGlobal = null)
        {
            if (mountAsStoreName == null)
            {
                mountAsStoreName = storeName;
            }

            Mount mount = null;

            IEnumerable<IReference> locations;

            if (useGlobal.HasValue)
            {
                locations = useGlobal.Value ? GlobalLocations : Locations;
            }
            else
            {
                locations = Locations.Concat(GlobalLocations).Distinct();
            }

            foreach (var location in locations)
            {
                var storeLocation = location.GetChildSubpath(storeName);
                var storeMetaLocation = storeLocation.GetChildSubpath(StoreMetadata.DefaultName);
                var hStore = storeMetaLocation.ToReadHandle<StoreMetadata>();
                var metadata = hStore.Value;
                if (metadata != null)
                {
                    l.Info("[store] Found store at " + location);

                    mount = new Mount(V.Stores[mountAsStoreName], storeLocation, store: storeName, enable: true, mountOptions: new MountOptions()
                    {
                        IsExclusive = true,
                    });
                }
            }

            if (mount == null && createIfMissing)
            {
                foreach (var location in locations)
                {
                    try
                    {
                        var storeLocation = location.GetChildSubpath(storeName);
                        var storeMetaLocation = storeLocation.GetChildSubpath(StoreMetadata.DefaultName);
                        var hStore = storeMetaLocation.ToReadWriteHandle<StoreMetadata>();
                        var metadata = new StoreMetadata();

                        hStore.Value = metadata;
                        throw new NotImplementedException("Save()");
#if TODO
                        hStore.Save();

                        mount = new Mount(V.Stores[mountAsStoreName], storeLocation, store: storeName, enable: true, mountOptions: new MountOptions()
                        {
                            IsExclusive = true,
                        });
                        break;
#endif
                    }
                    catch (Exception)
                    {
                        l.Error("Failed to create mount at " + location);
                    }
                }
            }

            return mount;
        }

        //public static Mount Mount(LocalFileReference fileReference, bool createIfMissing = true, string mountAsStoreName = null, bool? useGlobal = null)
        //{

        //    return null;

        //}

#region Misc

        private static readonly ILogger l = Log.Get();

#endregion
    }

}

#endif