﻿// Based on https://blog.rsuter.com/advanced-newtonsoft-json-dynamically-rename-or-ignore-properties-without-changing-the-serialized-class/
using Microsoft.Extensions.Configuration;
using LionFire.FlexObjects;
using LionFire.Applications.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace LionFire.Serialization.Json.Newtonsoft
{
    public class LionFireNewtonsoftJsonContractResolver : DefaultContractResolver
    {
        #region Parameters

        public LionSerializeContext SerializationPurpose { get; set; }

        #endregion

        #region Construction

        public LionFireNewtonsoftJsonContractResolver()
        {
        }

        #endregion

        #region (Public) Methods

        public void IgnoreProperty(Type type, params string[] jsonPropertyNames)
        {
            if (!_ignores.ContainsKey(type))
                _ignores[type] = new HashSet<string>();

            foreach (var prop in jsonPropertyNames)
                _ignores[type].Add(prop);
        }

        public void RenameProperty(Type type, string propertyName, string newJsonPropertyName)
        {
            if (!_renames.ContainsKey(type))
                _renames[type] = new Dictionary<string, string>();

            _renames[type][propertyName] = newJsonPropertyName;
        }

        #endregion

        #region (protected) IContractResolver Implementation

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (IsIgnored(property.DeclaringType, property.PropertyName))
            {
                property.ShouldSerialize = i => false;
                property.Ignored = true;
            }

            if (IsRenamed(property.DeclaringType, property.PropertyName, out var newJsonPropertyName))
                property.PropertyName = newJsonPropertyName;

            return property;
        }

        #endregion

        #region (Private) Implementation

        private readonly Dictionary<Type, HashSet<string>> _ignores = new Dictionary<Type, HashSet<string>>();
        private readonly Dictionary<Type, Dictionary<string, string>> _renames = new Dictionary<Type, Dictionary<string, string>>();

        private bool IsIgnored(Type type, string jsonPropertyName)
        {
            var propertyInfo = type.GetProperties().SingleOrDefault(p => p.Name == jsonPropertyName && p.DeclaringType == type);
            if (propertyInfo?.ShouldSerialize(SerializationPurpose) == false) return true;

            if (!_ignores.ContainsKey(type))
                return false;

            return _ignores[type].Contains(jsonPropertyName);
        }

        private bool IsRenamed(Type type, string jsonPropertyName, out string newJsonPropertyName)
        {
            Dictionary<string, string> renames;

            if (!_renames.TryGetValue(type, out renames) || !renames.TryGetValue(jsonPropertyName, out newJsonPropertyName))
            {
                newJsonPropertyName = null;
                return false;
            }

            return true;
        }

        #endregion

    }
}