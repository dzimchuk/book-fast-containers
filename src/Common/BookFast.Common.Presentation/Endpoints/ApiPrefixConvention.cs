using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BookFast.Common.Presentation.Endpoints;

public class ApiPrefixConvention(string prefix) : IControllerModelConvention
{
    private readonly AttributeRouteModel prefix = new(new RouteAttribute(prefix));

    public void Apply(ControllerModel controller)
    {
        if (!controller.Attributes.OfType<ApiControllerAttribute>().Any())
        {
            return;
        }

        foreach (var action in controller.Actions)
        {
            foreach (var selector in action.Selectors)
            {
                selector.AttributeRouteModel = selector.AttributeRouteModel is not null
                    ? AttributeRouteModel.CombineAttributeRouteModel(prefix, selector.AttributeRouteModel)
                    : prefix;
            }
        }
    }
}
