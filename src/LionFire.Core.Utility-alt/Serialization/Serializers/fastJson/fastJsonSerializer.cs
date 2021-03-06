﻿#if FASTJSON
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
//using LionFire.Serialization.JsonEx;

using fastJSON;

namespace LionFire.Serialization
{

    public class FastJsonSerializer : LionSerializer
    {
        public override string Name
        {
            get { return "FastJson"; }
        }

        public override byte[][] IdentifyingHeaders
        {
            get { return new byte[][] { UTF8Encoding.UTF8.GetBytes("("), UTF8Encoding.UTF8.GetBytes("{") }; }
        }

        public override string DefaultFileExtension { get { return "jsx"; } }

        private static bool didStaticInit = false;
        private static void StaticInit()
        {
            if (didStaticInit) return;

            fastJSON.JSON.IgnoreAttributes.Add(typeof(IgnoreAttribute));

            didStaticInit = true;
        }

        #region Serialize

        public override void Serialize(Stream stream, object obj)
        {
            if(!didStaticInit) StaticInit();
            if (obj == null) throw new ArgumentNullException();

            byte[] bytes = UTF8Encoding.UTF8.GetBytes(fastJSON.JSON.Instance.ToJSON(obj));
            stream.Write(bytes, 0, bytes.Length);
        }

        #endregion


        public override T Deserialize<T>(Stream stream)
        {
            if (!didStaticInit) StaticInit();
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            string jsonText = UTF8Encoding.UTF8.GetString(memoryStream.ToArray());

            object obj = fastJSON.JSON.Instance.ToObject(jsonText);

            return (T) obj;
        }

        public override object Deserialize(Stream stream, Type type)
        {
            if (!didStaticInit) StaticInit();

            if (stream.Length == 0) throw new Exception("Empty stream");

            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            string jsonText = UTF8Encoding.UTF8.GetString(memoryStream.ToArray());

            object obj = fastJSON.JSON.Instance.ToObject(jsonText);

            if(obj != null && !type.IsAssignableFrom(obj.GetType()))
            {
                throw new ArgumentException("Json data was not of expected type.");
            }
            return obj;
        }

    //    private static void Configure(JsonExSerializer.Serializer objectSerializer)
    //    {

    //        //objectSerializer.Config.TypeHandlerFactory.AttributeProcessors.Add(new UnignoreAttributeProcessor());

    //        objectSerializer.Config.TypeHandlerFactory.AttributeProcessors.Add(
    //new IgnoreProcessor()
    //{
    //    IgnoreContexts = LionSerializeContext.Persistence,
    //});
    //        objectSerializer.Config.OutputTypeComment = false;
    //        //objectSerializer.Config.OutputTypeInformation = false;
    //        objectSerializer.Config.TypeHandlerFactory.AttributeProcessors.Add(new SerializeDefaultProcessor());
    //        objectSerializer.Config.TypeHandlerFactory.AttributeProcessors.Add(new DefaultValueProcessor());


    //        //objectSerializer.Config.RegisterTypeConverter(typeof(Type), new LionFire.Serialization.Serializers.JsonEx.JsonExTypeConverter());
    //        //objectSerializer.Config.RegisterTypeConverter(typeof(Type), new TypeToStringConverter());

    //        //objectSerializer.Config.TypeAliases.Add(typeof(), "");
    //        //objectSerializer.Config.CollectionHandlers.Add(new TypeCollectionHandler());
    //        //objectSerializer.Config.ExpressionHandlers.Add(
    //        //serializer.Config.RegisterTypeConverter(typeof(Type), new LionFire.Assets.AssetReferenceSerializationConverter());
    //        //serializer.Config.RegisterTypeConverter(typeof(Type), new LionFire.Assets.AssetIDSerializationConverter());
    //    }


    }


}
#endif