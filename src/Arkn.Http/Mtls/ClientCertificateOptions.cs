using System.Security.Cryptography.X509Certificates;

namespace Arkn.Http.Mtls;

/// <summary>
/// Holds the resolved client certificate for mTLS.
/// Created by <c>.WithClientCertificate()</c> overloads on the builder.
/// </summary>
public sealed class ClientCertificateOptions
{
    /// <summary>The resolved X.509 certificate to present on every TLS handshake.</summary>
    public required X509Certificate2 Certificate { get; init; }

    // ── Factory methods ──────────────────────────────────────────────────────

    /// <summary>Wraps an already-loaded certificate.</summary>
    public static ClientCertificateOptions FromCertificate(X509Certificate2 cert) =>
        new() { Certificate = cert };

    /// <summary>Loads a PFX/PKCS#12 file.</summary>
    public static ClientCertificateOptions FromPfx(string path, string? password = null) =>
        new() { Certificate = new X509Certificate2(path, password) };

    /// <summary>Loads from PEM-encoded certificate and private key files (.pem / .crt / .key).</summary>
    public static ClientCertificateOptions FromPem(string certPemPath, string keyPemPath) =>
        new()
        {
            Certificate = X509Certificate2.CreateFromPemFile(certPemPath, keyPemPath)
        };

    /// <summary>
    /// Loads from the OS certificate store by thumbprint.
    /// Searches <paramref name="location"/> first; falls back to <see cref="StoreLocation.CurrentUser"/>.
    /// </summary>
    public static ClientCertificateOptions FromStore(
        StoreName     storeName,
        StoreLocation location,
        string        thumbprint)
    {
        using var store = new X509Store(storeName, location);
        store.Open(OpenFlags.ReadOnly);

        var results = store.Certificates.Find(
            X509FindType.FindByThumbprint, thumbprint, validOnly: false);

        if (results.Count == 0)
            throw new InvalidOperationException(
                $"No certificate with thumbprint '{thumbprint}' found in store {storeName}/{location}.");

        return new() { Certificate = results[0] };
    }
}
