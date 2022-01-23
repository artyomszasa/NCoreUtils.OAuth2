# Version history

## 6.0.0

**Breaking Changes**:

* `ScopeCollectionConverter` has been unified to use (RFC6749)[https://datatracker.ietf.org/doc/html/rfc6749#section-3.3] i.e. `scope-token *( SP scope-token )` everywhere. Previously `LoginProvider` serialization used array-based approach.\
For compatibility reasons deserialization from array is still supported but will be removed in future versions.