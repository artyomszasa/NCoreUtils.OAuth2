using NCoreUtils.Data.Build;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2;

[DataDefinitionGenerationOptions(GenerateFirestoreDecorators = true)]
[DataEntity(typeof(RefreshToken), NameFactory = typeof(DefaultRefreshTokenNameFactory))]
public partial class DefaultRefreshTokenDataContext { }