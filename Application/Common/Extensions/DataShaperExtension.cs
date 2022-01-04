using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Domain.Common;
using Domain.Interfaces;

namespace Application.Common.Extensions
{
    public static class DataShaperExtension
    {
        public static ExpandoObject ShapeData<TSource>(
            this TSource source,
            string fields)
        {
            var filteredProperties = GetProperties<TSource>(fields);
            return GetData(
                source,
                filteredProperties);
        }

        public static IEnumerable<ExpandoObject> ShapeData<TSource>(
            this IEnumerable<TSource> sources,
            string fields)
        {
            var filteredProperties = GetProperties<TSource>(fields);
            return sources.Select(
                    source => GetData(
                        source,
                        filteredProperties))
                .ToList();
        }

        private static ExpandoObject GetData<TSource>(
            TSource source,
            IEnumerable<PropertyInfo> filteredProperties)
        {
            var shapedObject = new ExpandoObject();
            foreach (var property in filteredProperties)
            {
                var propertyValue = property.GetValue(source);
                shapedObject.TryAdd(
                    property.Name,
                    propertyValue);
            }

            if (typeof(IHaveCustomFields).IsAssignableFrom(typeof(TSource)))
            {
                MapCustomFields(
                    (IHaveCustomFields) source,
                    shapedObject);
            }

            return shapedObject;
        }

        private static IEnumerable<PropertyInfo> GetProperties<TSource>(
            string fields)
        {
            var properties = typeof(TSource)
                .GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            var customFieldProperties = typeof(IHaveCustomFields).GetProperties();

            //ignore custom field properties here as they will shaped as individual properties
            properties = properties.Where(x => customFieldProperties.Any(y => y.Name != x.Name)).ToArray();

            var filteredProperties = new List<PropertyInfo>();
            if (!string.IsNullOrWhiteSpace(fields))
            {
                foreach (var field in fields.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    var propertyName = field.Trim();
                    var property = properties.FirstOrDefault(
                        pi => pi.Name.Equals(
                            propertyName,
                            StringComparison.InvariantCultureIgnoreCase));
                    if (property == null)
                        throw new Exception($"Property {propertyName} not found");
                    ;
                    filteredProperties.Add(property);
                }
            }
            else
            {
                filteredProperties = properties.ToList();
            }

            return filteredProperties;
        }

        private static ExpandoObject MapCustomFields(
            IHaveCustomFields customFieldObject,
            ExpandoObject expandoObject)
        {
            dynamic obj = expandoObject;
            obj.CustomFields = new List<dynamic>();
            foreach (var customField in customFieldObject.CustomFields)
            {
                var cf = new ExpandoObject();
                cf.TryAdd(
                    customField.Name,
                    customField.Value);
                obj.CustomFields.Add(cf);
            }

            return obj;
        }
    }
}