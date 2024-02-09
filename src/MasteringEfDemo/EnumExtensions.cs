using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MasteringEfDemo;

/// <summary>
///     Helpful extension methods targeting Enums to help display formatted enum values
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    ///     Returns the configured Display Name, or default string value for the given enum value
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static string GetDisplayNameOrStringValue(this Enum enumValue)
    {
        return enumValue?
            .GetType()?
            .GetMember(enumValue.ToString())?
            .First()?
            .GetCustomAttribute<DisplayAttribute>()?
            .GetName() ?? enumValue?.ToString();
    }

    /// <summary>
    ///     Gets the display name of an enum value, will return null if the [Display] attribute isn't found
    /// </summary>
    /// <exception cref="NullReferenceException">When the custom attribute is not found</exception>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static string GetDisplayName(this Enum enumValue)
    {
        return enumValue.GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()
            .GetName();
    }

    /// <summary>
    ///     Checks to see if an element has a Display Attribute added
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static bool HasDisplayName(this Enum enumValue)
    {
        return enumValue.GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>() != null;
    }
}