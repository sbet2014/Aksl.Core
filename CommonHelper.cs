using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Aksl.Core
{
    public class CommonHelper
    {
        #region Members
        //we use EmailValidator from FluentValidation. So let's keep them sync - https://github.com/JeremySkinner/FluentValidation/blob/master/src/FluentValidation/Validators/EmailValidator.cs
        private const string _emailExpression = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-||_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+([a-z]+|\d|-|\.{0,1}|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])?([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))$";
        private static readonly Regex _emailRegex;
        #endregion

        #region Constructors
        static CommonHelper()
        {
            _emailRegex = new Regex(_emailExpression, RegexOptions.IgnoreCase);
        }
        #endregion

        #region To Methods
        public static T To<T>(object value)
        {
            //return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            return (T)To(value, typeof(T));
        }

        public static object To(object value, Type destinationType)
        {
            return To(value, destinationType, CultureInfo.InvariantCulture);
        }

        public static object To(object value, Type destinationType, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var sourceType = value.GetType();

            var destinationConverter = TypeDescriptor.GetConverter(destinationType);
            if (destinationConverter.CanConvertFrom(value.GetType()))
            {
                return destinationConverter.ConvertFrom(null, culture, value);
            }

            var sourceConverter = TypeDescriptor.GetConverter(sourceType);
            if (sourceConverter.CanConvertTo(destinationType))
            {
                return sourceConverter.ConvertTo(null, culture, value, destinationType);
            }

            if (destinationType.IsEnum && value is int)
            {
                return Enum.ToObject(destinationType, (int)value);
            }

            if (!destinationType.IsInstanceOfType(value))
            {
                return Convert.ChangeType(value, destinationType, culture);
            }

            return value;
        }
        #endregion

        #region Set Property Method
        public static void SetProperty(object instance, string propertyName, object value)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            var instanceType = instance.GetType();
            var pi = instanceType.GetProperty(propertyName);
            if (pi == null)
            {
                throw new Exception($"No property '{ propertyName}' found on the instance of type '{instanceType}'.");
            }
            if (!pi.CanWrite)
            {
                throw new Exception("The property '{propertyName}' on the instance of type '{instanceType}' does not have a setter.");
            }

            if (value != null && !value.GetType().IsAssignableFrom(pi.PropertyType))
            {
                value = To(value, pi.PropertyType);
            }

            pi.SetValue(instance, value, new object[0]);
        }
        #endregion

        public static string GetSettingKey<T, K>(Expression<Func<T, K>> keySelector) where T : class, new()
        {
            if (!(keySelector.Body is MemberExpression member))
            {
                throw new ArgumentException($"Expression '{ keySelector}' refers to a method, not a property.");
            }

            if (!(member.Member is PropertyInfo propInfo))
            {
                throw new ArgumentException($"Expression '{ keySelector}' refers to a field, not a property.");
            }

            var key = $"{typeof(T).Name}.{propInfo.Name}";
            return key;
        }
    }
}
