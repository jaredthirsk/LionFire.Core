﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Assets
{
    //#if !AOT
    public interface IHasHAsset<ConcreteType>
        //: IHasHAsset
        where ConcreteType : class
    {

#if !AOT // new HAsset<ConcreteType> HAsset
        HAsset<ConcreteType> HAsset {
            get;
            set;
        } // Add set?#endif
          //		ConcreteType AssetObject { get; } // Might as well expose this?
#endif

        ConcreteType AssetObject { get; }
    }

    public interface IHasHAsset : IHasHAsset<object>
    {
        //IHAsset<object> HAsset { get; set; }
        //object AssetObject { get; }

    }

}
