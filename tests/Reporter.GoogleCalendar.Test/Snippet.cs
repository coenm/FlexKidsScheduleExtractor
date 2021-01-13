namespace Reporter.GoogleCalendar.Test
{
    using System.Security.Cryptography.X509Certificates;

    public class Snippet
    {
        public string X509CertificateToBase64String(string googleCertificateFilename)
        {
            using var certificate = new X509Certificate2(googleCertificateFilename, "notasecret", X509KeyStorageFlags.Exportable);
            var bytes = certificate.Export(X509ContentType.Pfx);
            return System.Convert.ToBase64String(bytes);
        }
    }
}
