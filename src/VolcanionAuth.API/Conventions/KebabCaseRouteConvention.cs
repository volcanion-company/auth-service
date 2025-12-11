using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace VolcanionAuth.API.Conventions;

/// <summary>
/// Applies a route convention that converts controller route tokens to kebab-case in ASP.NET Core MVC applications.
/// </summary>
/// <remarks>This convention replaces the "[controller]" token in attribute route templates with a kebab-case
/// version of the controller name. Kebab-case formatting uses lowercase letters and hyphens to separate words (for
/// example, "MyApi" becomes "my-api"). This can improve URL readability and consistency across APIs. The convention
/// should be registered during MVC configuration, typically in the application's startup code.</remarks>
public class KebabCaseRouteConvention : IApplicationModelConvention
{
    /// <summary>
    /// Replaces the [controller] token in attribute route templates with the controller's name in kebab-case for all
    /// controllers in the specified application model.
    /// </summary>
    /// <remarks>This method modifies the route templates of controllers by converting their names to
    /// kebab-case and substituting them for the [controller] token in attribute routes. This ensures consistent,
    /// URL-friendly routing conventions across all controllers. The operation affects only selectors with non-null
    /// attribute route models.</remarks>
    /// <param name="application">The application model containing the controllers whose route templates will be updated. Cannot be null.</param>
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            // Get the controller name without the "Controller" suffix
            var controllerName = controller.ControllerName;

            // Convert to kebab-case
            var kebabCaseName = ConvertToKebabCase(controllerName);

            // Update all selectors with the new route
            foreach (var selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel != null)
                {
                    // Replace [controller] token with kebab-case name
                    selector.AttributeRouteModel.Template = 
                        selector.AttributeRouteModel.Template?.Replace("[controller]", kebabCaseName);
                }
            }
        }
    }

    /// <summary>
    /// Converts a PascalCase or camelCase string to kebab-case format.
    /// </summary>
    /// <remarks>This method is useful for generating URL-friendly or CSS class names from PascalCase or
    /// camelCase identifiers. Only uppercase letters are considered as word boundaries; numbers and other characters
    /// are not specially handled.</remarks>
    /// <param name="value">The input string to convert. If <paramref name="value"/> is null or empty, the method returns it unchanged.</param>
    /// <returns>A kebab-case representation of the input string, with words separated by hyphens and all letters in lowercase.
    /// Returns the original value if it is null or empty.</returns>
    private static string ConvertToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // Insert a hyphen before each uppercase letter (except the first one)
        // and convert the entire string to lowercase
        return Regex.Replace(value, "(?<!^)([A-Z])", "-$1").ToLower();
    }
}
