using Microsoft.Extensions.Configuration;

namespace DStack.Projections.RavenDB.IntegrationTests;

public class RavenConfig
{
    public string[] Urls { get; set; }
    public string DatabaseName { get; set; }
    public string CertificateFilePath { get; set; }
    public string CertificateFilePassword { get; set; }

    public static RavenConfig FromConfiguration(IConfiguration conf)
    {
        return new RavenConfig
        {
            Urls = conf["RavenDB:Urls"].Split(';'),
            CertificateFilePassword = conf["RavenDB:CertificatePassword"],
            CertificateFilePath = conf["RavenDB:CertificatePath"],
            DatabaseName = conf["RavenDB:DatabaseName"]
        };
    }
}
