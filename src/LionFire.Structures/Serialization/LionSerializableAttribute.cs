﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LionFire.Serialization
{

    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public sealed class LionSerializableAttribute : Attribute
    {
        readonly SerializeMethod method;
        //readonly LionSerializationMode? mode;

        //public LionSerializableAttribute(LionSerializationMode mode, SerializeMethod method)
        //{
        //this.method = method;
        //}
        public LionSerializableAttribute( SerializeMethod method)
        {
            //this.mode = null;
            this.method = method;
        }

        public SerializeMethod Method
        {
            get { return method; }
        }

    }
}
