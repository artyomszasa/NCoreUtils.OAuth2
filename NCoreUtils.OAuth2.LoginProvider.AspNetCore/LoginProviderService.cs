using NCoreUtils.AspNetCore.Internal;
using NCoreUtils.OAuth2.LoginProvider;
using NCoreUtils.Proto;

namespace NCoreUtils.AspNetCore;

[ProtoService(typeof(LoginProviderInfo), typeof(LoginProviderSerializerContext))]
public class LoginProviderService { }