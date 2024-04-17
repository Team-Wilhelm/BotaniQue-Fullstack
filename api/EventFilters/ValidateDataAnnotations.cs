using System.Collections;
using System.ComponentModel.DataAnnotations;
using Fleck;
using lib;

namespace api.EventFilters;

public class ValidateDataAnnotations : BaseEventFilter
{
    public override Task Handle<T>(IWebSocketConnection socket, T dto)
    {
        ValidateObjectRecursive(dto);
        return Task.CompletedTask;
    }

    private static void ValidateObjectRecursive(object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var context = new ValidationContext(obj);
        Validator.ValidateObject(obj, context, true);

        foreach (var property in obj.GetType().GetProperties())
        {
            if (property.PropertyType.Assembly == typeof(string).Assembly) continue;

            var value = property.GetValue(obj);
            if (value == null) continue;

            if (value is IEnumerable enumerable)
                foreach (var element in enumerable)
                    ValidateObjectRecursive(element);
            else
                ValidateObjectRecursive(value);
        }
    }
}