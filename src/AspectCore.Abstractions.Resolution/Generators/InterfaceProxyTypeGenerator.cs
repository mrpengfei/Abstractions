﻿using AspectCore.Abstractions.Extensions;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal class InterfaceProxyTypeGenerator : ProxyTypeGenerator
    {
        private readonly Type parentType;
        public InterfaceProxyTypeGenerator(Type serviceType, Type parentType, Type[] interfaces, IAspectValidator aspectValidator) : base(serviceType, aspectValidator)
        {
            if (!serviceType.GetTypeInfo().IsInterface)
            {
                throw new ArgumentException($"Type '{serviceType}' should be interface.", nameof(serviceType));
            }
            if (parentType == null)
            {
                throw new ArgumentNullException(nameof(parentType));
            }
            Interfaces = new Type[] { serviceType }.Concat(interfaces ?? Type.EmptyTypes).ToArray();
            this.parentType = parentType;
        }

        public override Type[] Interfaces { get; }

        public override Type ParentType => typeof(object);

        public override string TypeName => $"{ModuleGenerator.ProxyNameSpace}.ServiceInstance.Proxy{parentType.Name}As{ServiceType.Name}";

        protected override void GeneratingConstructor(TypeBuilder declaringType)
        {
            new DefaultProxyConstructorGenerator(declaringType, ServiceType, MethodInfoConstant.Object_Ctor, serviceInstanceFieldBuilder, serviceProviderFieldBuilder).Build();
        }

        protected override void GeneratingMethod(TypeBuilder declaringType)
        {
            for (var i = 0; i < Interfaces.Length; i++)
            {
                foreach (var method in Interfaces[i].GetTypeInfo().DeclaredMethods)
                {
                    if (method.IsPropertyBinding())
                    {
                        continue;
                    }
                    if (!AspectValidator.Validate(method))
                    {
                        new NonProxyMethodGenerator(declaringType, parentType, method, serviceInstanceFieldBuilder, i != 0).Build();
                        continue;
                    }
                    new ProxyMethodGenerator(declaringType, Interfaces[i], parentType, method, serviceInstanceFieldBuilder, serviceProviderFieldBuilder, i != 0).Build();
                }
            }
        }

        protected override void GeneratingProperty(TypeBuilder declaringType)
        {
            for (var i = 0; i < Interfaces.Length; i++)
            {
                foreach (var property in Interfaces[i].GetTypeInfo().DeclaredProperties)
                {
                    if (AspectValidator.Validate(property))
                    {
                        new ProxyPropertyGenerator(declaringType, property, Interfaces[i], parentType, serviceInstanceFieldBuilder, serviceProviderFieldBuilder, i != 0).Build();
                    }
                    else
                    {
                        new NonProxyPropertyGenerator(declaringType, property, Interfaces[i], parentType, serviceInstanceFieldBuilder, i != 0).Build();
                    }
                }
            }
        }
    }
}