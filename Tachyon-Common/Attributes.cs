using System;

namespace TachyonCommon
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class GenerateBindingsAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class HostBindingAttribute : Attribute {
        public Type ServiceType { get; }

        public HostBindingAttribute(Type serviceType) {
            ServiceType = serviceType;
        }
    }
}