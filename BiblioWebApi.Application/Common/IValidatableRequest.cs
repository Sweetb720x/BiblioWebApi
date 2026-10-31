namespace BiblioWebApi.Application.Common;

public interface IValidatableRequest
{
    IReadOnlyCollection<string> Validate();
}




