﻿using LionFire.Referencing;
using System.Collections.Generic;

namespace LionFire.Vos.Assets
{

    public class VobAssetReference<TValue> : VosReferenceBase<TValue, VobAssetReference<TValue>>, IVobAssetReference
    {
        #region Scheme

        public new const string UriSchemeDefault = "asset";
        public new const string UriPrefixDefault = "asset:";

        public override IEnumerable<string> AllowedSchemes
        {
            get { yield return UriSchemeDefault; }
        }

        public override string Scheme => UriSchemeDefault;
        public new static readonly string[] UriSchemes = new string[] { UriSchemeDefault };

        #endregion
    }

#if TOOPTIMIZE // OPTIMIZE ideas:
    //  - VosPathChunksReference
    //  - IVosReference
    // TODO: Create this from VosReference
    public class VosChunksReference : VosReferenceBase, IVosReference
    {
        public IVosReference ParentReference { get; }
        public ArraySegment<string> PathChunks { get; set; }
    }

    public class VosReferenceBase<TConcrete> : ReferenceBase<TConcrete>, IVosReference
    {
        // TODO: Get most of this from VosReference
    }
#endif

    // Persister: VobRootName or ::VobRootName#AssetCollectionName or #AssetCollectionName

}