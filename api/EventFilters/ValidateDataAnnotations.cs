using System.Collections;
using System.ComponentModel.DataAnnotations;
using Fleck;
using lib;
using Shared.Models.Exceptions;

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
        try
        {
            Validator.ValidateObject(obj, context, true);
        }
        catch (ValidationException e)
        {
            throw new ModelValidationException(e.Message);
        }

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