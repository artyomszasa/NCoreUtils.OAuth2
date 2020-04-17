using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.OAuth2.Internal
{
    public class IntrospectionClientInput : CustomClientInput
    {
        public override InputSerializer CreateSerializer(MethodDescriptor method)
            => IntrospectionInputSerializer.Instance;
    }
}