using NCoreUtils.OAuth2.LoginProvider;
using NCoreUtils.Proto;

namespace NCoreUtils.OAuth2;

[ProtoClient(typeof(LoginProviderInfo), typeof(LoginProviderSerializerContext))]
public partial class LoginProviderClient { }