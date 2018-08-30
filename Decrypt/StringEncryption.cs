using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
public class StringEncryption : IDisposable
{
    private readonly Random random;
    private readonly byte[] key;
    private readonly RijndaelManaged rm;
    private readonly UTF8Encoding encoder;

    public StringEncryption()
    {
        this.random = new Random();
        this.rm = new RijndaelManaged();
        this.encoder = new UTF8Encoding();
        this.key = Convert.FromBase64String("Your+Secret+Static+Encryption+Key+Goes+Here=");
    }

    public string Encrypt(string unencrypted)
    {
        var vector = new byte[16];
        this.random.NextBytes(vector);
        var cryptogram = vector.Concat(this.Encrypt(this.encoder.GetBytes(unencrypted), vector));
        return Convert.ToBase64String(cryptogram.ToArray());
    }

    public string Decrypt(string encrypted)
    {
        var cryptogram = Convert.FromBase64String(encrypted);
        if (cryptogram.Length < 17)
        {
            throw new ArgumentException("Not a valid encrypted string", "encrypted");
        }

        var vector = cryptogram.Take(16).ToArray();
        var buffer = cryptogram.Skip(16).ToArray();
        return this.encoder.GetString(this.Decrypt(buffer, vector));
    }

    private byte[] Encrypt(byte[] buffer, byte[] vector)
    {
        var encryptor = this.rm.CreateEncryptor(this.key, vector);
        return this.Transform(buffer, encryptor);
    }

    private byte[] Decrypt(byte[] buffer, byte[] vector)
    {
        var decryptor = this.rm.CreateDecryptor(this.key, vector);
        return this.Transform(buffer, decryptor);
    }

    private byte[] Transform(byte[] buffer, ICryptoTransform transform)
    {
        var stream = new MemoryStream();
        using (var cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
        {
            cs.Write(buffer, 0, buffer.Length);
        }

        return stream.ToArray();
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                rm.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            disposedValue = true;
        }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~StringEncryption() {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        // GC.SuppressFinalize(this);
    }
    #endregion
}