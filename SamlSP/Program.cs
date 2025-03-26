using Microsoft.AspNetCore.Authentication.Cookies;
using Sustainsys.Saml2;
using Sustainsys.Saml2.AspNetCore2;
using Sustainsys.Saml2.Configuration;
using Sustainsys.Saml2.Metadata;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddSaml2(options =>
    {
        options.SPOptions.EntityId = new EntityId("https://localhost:7191");

        options.SPOptions.ReturnUrl = new Uri("https://localhost:7096/Saml2/ProxyAcs");

        options.SPOptions.ServiceCertificates.Add(new ServiceCertificate
        {
            Certificate = new X509Certificate2("saml-sp-certificate.pfx", "your_password", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable),
            Use = CertificateUse.Signing // 🔹 Ensures it's used for signing, not encryption
        });

        options.Notifications.AuthenticationRequestCreated = (request, provider, spOptions) =>
        {
            request.AssertionConsumerServiceUrl = new Uri("https://localhost:7096/Saml2/ProxyAcs");
        };

        // Add Entra ID as the Identity Provider
        var idp = new IdentityProvider(
            new EntityId("https://sts.windows.net/1f085bb5-6521-428a-9c44-857ed2efceca/"), options.SPOptions)
        {
            MetadataLocation = "https://login.microsoftonline.com/1f085bb5-6521-428a-9c44-857ed2efceca/federationmetadata/2007-06/federationmetadata.xml?appid=9a586e69-5691-4fd9-bab1-2a9f087a7ccc",
            LoadMetadata = true,
            AllowUnsolicitedAuthnResponse = true
        };

        options.IdentityProviders.Add(idp);

        // Add the Service Provider (SP) signing certificate
        ////options.SPOptions.ServiceCertificates.Add(new X509Certificate2("saml-sp-certificate.pfx", "your_password"));
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
