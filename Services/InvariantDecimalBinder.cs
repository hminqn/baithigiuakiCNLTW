using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace FoodManager_BIT240194.Services;

// HTML number inputs submit a dot decimal separator regardless of UI language.
public class InvariantDecimalBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext context)
    {
        var value = context.ValueProvider.GetValue(context.ModelName);
        if (value == ValueProviderResult.None) return Task.CompletedTask;
        context.ModelState.SetModelValue(context.ModelName, value);
        if (string.IsNullOrWhiteSpace(value.FirstValue))
        {
            if (Nullable.GetUnderlyingType(context.ModelType) is not null)
                context.Result = ModelBindingResult.Success(null);
            else context.ModelState.TryAddModelError(context.ModelName, UiText.Message("Required"));
            return Task.CompletedTask;
        }
        const NumberStyles style = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
        if (decimal.TryParse(value.FirstValue, style, CultureInfo.InvariantCulture, out var number))
            context.Result = ModelBindingResult.Success(number);
        else context.ModelState.TryAddModelError(context.ModelName, UiText.Message("InvalidNumber"));
        return Task.CompletedTask;
    }
}
public class InvariantDecimalBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
        (Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType) == typeof(decimal)
            ? new InvariantDecimalBinder() : null;
}
