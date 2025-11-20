using Ams.Web.Client.Core;

namespace Ams.Web.Client.Web;

public class ClientWebSettings : ClientCoreSettings
{

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validationResults = base.Validate(validationContext).ToList();


        return validationResults;
    }
}

