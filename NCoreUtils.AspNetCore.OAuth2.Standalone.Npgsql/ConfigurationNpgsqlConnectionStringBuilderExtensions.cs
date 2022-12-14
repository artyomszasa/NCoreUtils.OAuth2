using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace NCoreUtils.AspNetCore.OAuth2;

internal static class ConfigurationNpgsqlConnectionStringBuilderExtensions
{
    private static ArrayNullabilityMode ParseArrayNullabilityMode(string raw) => raw switch
    {
        nameof(ArrayNullabilityMode.Always) => ArrayNullabilityMode.Always,
        nameof(ArrayNullabilityMode.Never) => ArrayNullabilityMode.Never,
        nameof(ArrayNullabilityMode.PerInstance) => ArrayNullabilityMode.PerInstance,
        _ => throw new InvalidOperationException("Invalid ArrayNullabilityMode.")
    };

    private static ServerCompatibilityMode ParseServerCompatibilityMode(string raw) => raw switch
    {
        nameof(ServerCompatibilityMode.None) => ServerCompatibilityMode.None,
        nameof(ServerCompatibilityMode.NoTypeLoading) => ServerCompatibilityMode.NoTypeLoading,
        nameof(ServerCompatibilityMode.Redshift) => ServerCompatibilityMode.Redshift,
        _ => throw new InvalidOperationException("Invalid ServerCompatibilityMode.")
    };

    private static SslMode ParseSslMode(string raw) => raw switch
    {
        nameof(SslMode.Allow) => SslMode.Allow,
        nameof(SslMode.Disable) => SslMode.Disable,
        nameof(SslMode.Prefer) => SslMode.Prefer,
        nameof(SslMode.Require) => SslMode.Require,
        nameof(SslMode.VerifyCA) => SslMode.VerifyCA,
        nameof(SslMode.VerifyFull) => SslMode.VerifyFull,
        _ => throw new InvalidOperationException("Invalid ServerCompatibilityMode.")
    };

    private static int I32(string raw) => int.Parse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture);

    private static bool B(string raw) => bool.Parse(raw);

    public static void BindNpgsqlConnectionStringBuilder(this IConfiguration configuration, NpgsqlConnectionStringBuilder connectionStringBuilder)
    {
        foreach (var (key, value) in configuration.AsEnumerable(makePathsRelative: true))
        {
            switch (key)
            {
                case nameof(NpgsqlConnectionStringBuilder.ApplicationName):
                    connectionStringBuilder.ApplicationName = value;
                    break;
                case nameof(NpgsqlConnectionStringBuilder.ArrayNullabilityMode) when value is not null:
                    connectionStringBuilder.ArrayNullabilityMode = ParseArrayNullabilityMode(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.AutoPrepareMinUsages) when value is not null:
                    connectionStringBuilder.AutoPrepareMinUsages = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.CancellationTimeout) when value is not null:
                    connectionStringBuilder.CancellationTimeout = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.CheckCertificateRevocation) when value is not null:
                    connectionStringBuilder.CheckCertificateRevocation = B(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.ClientEncoding):
                    connectionStringBuilder.ClientEncoding = value;
                    break;
                case nameof(NpgsqlConnectionStringBuilder.CommandTimeout) when value is not null:
                    connectionStringBuilder.CommandTimeout = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.ConnectionIdleLifetime) when value is not null:
                    connectionStringBuilder.ConnectionIdleLifetime = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.ConnectionLifetime) when value is not null:
                    connectionStringBuilder.ConnectionLifetime = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.ConnectionPruningInterval) when value is not null:
                    connectionStringBuilder.ConnectionPruningInterval = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.Database):
                    connectionStringBuilder.Database = value;
                    break;
                case nameof(NpgsqlConnectionStringBuilder.Encoding) when value is not null:
                    connectionStringBuilder.Encoding = value;
                    break;
                case nameof(NpgsqlConnectionStringBuilder.Enlist) when value is not null:
                    connectionStringBuilder.Enlist = B(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.Host):
                    connectionStringBuilder.Host = value;
                    break;
                case nameof(NpgsqlConnectionStringBuilder.HostRecheckSeconds) when value is not null:
                    connectionStringBuilder.HostRecheckSeconds = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.IncludeErrorDetail) when value is not null:
                    connectionStringBuilder.IncludeErrorDetail = B(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.IncludeRealm) when value is not null:
                    connectionStringBuilder.IncludeRealm = B(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.IntegratedSecurity) when value is not null:
                    connectionStringBuilder.IntegratedSecurity = B(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.InternalCommandTimeout) when value is not null:
                    connectionStringBuilder.InternalCommandTimeout = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.KeepAlive) when value is not null:
                    connectionStringBuilder.KeepAlive = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.LogParameters) when value is not null:
                    connectionStringBuilder.LogParameters = B(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.MaxAutoPrepare) when value is not null:
                    connectionStringBuilder.MaxAutoPrepare = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.MaxPoolSize) when value is not null:
                    connectionStringBuilder.MaxPoolSize = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.Multiplexing) when value is not null:
                    connectionStringBuilder.Multiplexing = B(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.NoResetOnClose) when value is not null:
                    connectionStringBuilder.NoResetOnClose = B(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.Password):
                    connectionStringBuilder.Password = value;
                    break;
                case nameof(NpgsqlConnectionStringBuilder.Pooling) when value is not null:
                    connectionStringBuilder.Pooling = B(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.Port) when value is not null:
                    connectionStringBuilder.Port = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.ReadBufferSize) when value is not null:
                    connectionStringBuilder.ReadBufferSize = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.SearchPath):
                    connectionStringBuilder.SearchPath = value;
                    break;
                case nameof(NpgsqlConnectionStringBuilder.ServerCompatibilityMode) when value is not null:
                    connectionStringBuilder.ServerCompatibilityMode = ParseServerCompatibilityMode(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.SocketReceiveBufferSize) when value is not null:
                    connectionStringBuilder.SocketReceiveBufferSize = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.SocketSendBufferSize) when value is not null:
                    connectionStringBuilder.SocketSendBufferSize = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.SslMode) when value is not null:
                    connectionStringBuilder.SslMode = ParseSslMode(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.TcpKeepAlive) when value is not null:
                    connectionStringBuilder.TcpKeepAlive = B(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.TcpKeepAliveInterval) when value is not null:
                    connectionStringBuilder.TcpKeepAliveInterval = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.TcpKeepAliveTime) when value is not null:
                    connectionStringBuilder.TcpKeepAliveTime = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.Timeout) when value is not null:
                    connectionStringBuilder.Timeout = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.Timezone):
                    connectionStringBuilder.Timezone = value;
                    break;
                case nameof(NpgsqlConnectionStringBuilder.TrustServerCertificate) when value is not null:
                    connectionStringBuilder.TrustServerCertificate = B(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.Username):
                    connectionStringBuilder.Username = value;
                    break;
                case nameof(NpgsqlConnectionStringBuilder.WriteBufferSize) when value is not null:
                    connectionStringBuilder.WriteBufferSize = I32(value);
                    break;
                case nameof(NpgsqlConnectionStringBuilder.WriteCoalescingBufferThresholdBytes) when value is not null:
                    connectionStringBuilder.WriteCoalescingBufferThresholdBytes = I32(value);
                    break;
            }
        }
    }
}