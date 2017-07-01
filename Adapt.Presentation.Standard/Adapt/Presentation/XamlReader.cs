using System;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace Adapt.Presentation
{
    /// <summary>
    /// Dynamically converts XAML in to UI elements
    /// </summary>
    public static class XamlReader
    {
        #region Fields
        private static MethodInfo _FirstExtensionsMethods;
        private const string _ClrNamespaceParamName = "clrNamespace";
        #endregion

        #region Static Constructor
        private static void Initialise()
        {
            // This is the current situation, where the LoadFromXaml is the only non-public static method.
            _FirstExtensionsMethods = typeof(Xamarin.Forms.Xaml.Extensions).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).FirstOrDefault() ?? typeof(Xamarin.Forms.Xaml.Extensions).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(m => m.GetParameters().Last().ParameterType == typeof(string));

            if (_FirstExtensionsMethods == null)
            {
                // If we didn't find it, it may be because the extension method may be public now :)
                throw new NotSupportedException("Xamarin.Forms implementation of XAML loading not found. Please update the Dynamic nuget package.");
            }
            _FirstExtensionsMethods = _FirstExtensionsMethods.MakeGenericMethod(typeof(BindableObject));
        }
        #endregion

        #region Private Static Methods
        /// <summary>
        /// Applies the given XAML to the view.
        /// </summary>
        private static TBindableObject Load<TBindableObject>(this TBindableObject view, string xaml) where TBindableObject : BindableObject
        {
            try
            {
                if (_FirstExtensionsMethods == null)
                {
                    Initialise();
                }

                return (TBindableObject)_FirstExtensionsMethods.Invoke(null, new object[] { view, xaml });
            }
            catch (Exception ex)
            {
                var anex = ex.InnerException as ArgumentNullException;
                if (anex == null)
                {
                    throw;
                }

                if (anex.ParamName == _ClrNamespaceParamName)
                {
                    throw new Exception($"The XAML parser did not receive the argument {_ClrNamespaceParamName}. This probably indicates that a default namespace was not specified in the XAML.\r\n\r\nXaml:\r\n{xaml}");
                }

                throw;
            }
        }
        #endregion

        #region Public Static Methods
        /// <summary>
        /// Applies the given XAML to the view.
        /// </summary>
        public static TBindableObject Load<TBindableObject>(string xaml) where TBindableObject : BindableObject
        {
            if (string.IsNullOrEmpty(xaml)) throw new ArgumentNullException(nameof(xaml));

            var bindableObject = (TBindableObject)Activator.CreateInstance(typeof(TBindableObject));

            return Load(bindableObject, xaml);
        }
        #endregion
    }
}
