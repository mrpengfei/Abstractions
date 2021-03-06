﻿using AspectCore.Abstractions.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Abstractions.Resolution.Internal
{
    public class PropertyInjectorSelector : IPropertyInjectorSelector
    {
        private static readonly ConcurrentDictionary<Type, IPropertyInjector[]> PropertyInjectorCache = new ConcurrentDictionary<Type, IPropertyInjector[]>();
        public IPropertyInjector[] SelectPropertyInjector(Type interceptorType)
        {
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }
            return PropertyInjectorCache.GetOrAdd(interceptorType, type => SelectPropertyInjectorCache(type).ToArray());
        }

        private IEnumerable<IPropertyInjector> SelectPropertyInjectorCache(Type type)
        {
            foreach (var property in type.GetTypeInfo().DeclaredProperties)
            {
                if (property.CanWrite && property.IsDefined(typeof(FromServicesAttribute)))
                {
                    yield return new PropertyInjector(
                        p => p.GetService(property.PropertyType),
                        new PropertyAccessor(property).CreatePropertySetter());
                }
            }
        }
    }
}
