﻿using LionFire.Instantiating;
using LionFire.Structures;

namespace LionFire.Assets
{
    public class DisplayNamableTemplateAssetBase<TTemplate, TInstance, TParameters> : DisplayNamableAssetBase<TTemplate>, ITemplateAsset<TTemplate, TInstance>
        where TTemplate : ITemplateAsset<TTemplate, TInstance>
        where TParameters : ITemplateParameters<TTemplate, TInstance>, ITemplateParameters
    {
        public DisplayNamableTemplateAssetBase() { }
        public DisplayNamableTemplateAssetBase(AssetReference<TTemplate> template) : base(template)
        {
        }
    }
}
