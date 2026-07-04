// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local - Without this the setter for Id is stripped out, breaking storage integrations.

namespace WebApi.Authentication;

/// <summary>
///     ApiSecret entity, can be inherited to extend with additional properties.
/// </summary>
public class ApiSecret : BaseApiSecret
{
}