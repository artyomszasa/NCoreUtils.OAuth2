using NCoreUtils.Proto;

namespace NCoreUtils.OAuth2.Internal;

[ProtoInfo(typeof(ITokenServiceEndpoints), Input = InputType.Form, ParameterNaming = Naming.SnakeCase)]
[ProtoMethodInfo(nameof(ITokenServiceEndpoints.IntrospectAsync), Input = InputType.Custom)]
public partial class TokenServiceEndpointsInfo { }