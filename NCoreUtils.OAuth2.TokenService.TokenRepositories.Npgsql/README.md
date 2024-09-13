# Initialization

Repository requires single table. It is not created automatically to improve performance and size. Its name can be
passed to the `AddNpgsqlTokenRepository` methods. By default `refresh_token` is used:

```
CREATE TABLE refresh_token (
    id INT NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    sub TEXT NOT NULL,
    issuer TEXT NOT NULL,
    email TEXT,
    username TEXT,
    scopes TEXT[] NOT NULL,
    issued_at BIGINT NOT NULL,
    expires_at BIGINT NOT NULL
);

CREATE INDEX "IX_refresh_token_check" ON refresh_token (sub, issued_at, scopes);
```