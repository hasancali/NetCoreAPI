using System;

namespace Domain.Common
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DoNotAudit : Attribute
    {
        
    }
}