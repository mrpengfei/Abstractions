﻿using System;

namespace AspectCore.Abstractions
{
    public interface IProxyGenerator
    {
        Type CreateInterfaceProxyType(Type serviceType, Type implementationType, params Type[] interfaces);

        Type CreateClassProxyType(Type serviceType, Type implementationType, params Type[] interfaces);
    }
}