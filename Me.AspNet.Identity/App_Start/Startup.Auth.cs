using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using Me.AspNet.Identity.Models;
using JA.Helpers;
using Owin.Security.Providers.GitHub;

namespace Me.AspNet.Identity
{
    public partial class Startup
    {
        // Pour plus d’informations sur la configuration de l’authentification, rendez-vous sur http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configurer le contexte de base de données, le gestionnaire des utilisateurs et le gestionnaire des connexions pour utiliser une instance unique par demande
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
            // [10000]
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);


            // Autoriser l’application à utiliser un cookie pour stocker des informations pour l’utilisateur connecté
            // et pour utiliser un cookie à des fins de stockage temporaire des informations sur la connexion utilisateur avec un fournisseur de connexion tiers
            // Configurer le cookie de connexion
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Permet à l'application de valider le timbre de sécurité quand l'utilisateur se connecte.
                    // Cette fonction de sécurité est utilisée quand vous changez un mot de passe ou ajoutez une connexion externe à votre compte.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                },
                //CookieName = ".YAFNET_Authentication"
            });            
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Permet à l'application de stocker temporairement les informations utilisateur lors de la vérification du second facteur dans le processus d'authentification à 2 facteurs.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Permet à l'application de mémoriser le second facteur de vérification de la connexion, un numéro de téléphone ou un e-mail par exemple.
            // Lorsque vous activez cette option, votre seconde étape de vérification pendant le processus de connexion est mémorisée sur le poste à partir duquel vous vous êtes connecté.
            // Ceci est similaire à l'option RememberMe quand vous vous connectez.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // - [10006] - ADD: Social network login
            // Enable logging with third party login providers
            const string XmlSchemaString = "http://www.w3.org/2001/XMLSchema#string";
            ///
            /// MICROSOFT
            ///
            var microsoftProvider = new Microsoft.Owin.Security.MicrosoftAccount.MicrosoftAccountAuthenticationProvider
            {
                OnAuthenticated = (context) =>
                {
                    foreach (var claim in context.User)
                    {
                        var claimType = string.Format("urn:microsoft:{0}", claim.Key);
                        string claimValue = claim.Value.ToString();
                        if (!context.Identity.HasClaim(claimType, claimValue))
                            context.Identity.AddClaim(new System.Security.Claims.Claim(claimType, claimValue, XmlSchemaString, "Microsoft"));
                    }
                    return System.Threading.Tasks.Task.FromResult(0);
                }
            };
            var mio = new Microsoft.Owin.Security.MicrosoftAccount.MicrosoftAccountAuthenticationOptions
            {
                ClientId = Utils.GetAppSetting("MicrosoftClientId"),
                ClientSecret = Utils.GetAppSetting("MicrosoftClientSecret"),
                CallbackPath = new PathString("/signin-microsoft"),
                Provider = microsoftProvider,
            };
            mio.Scope.Add("wl.basic");
            mio.Scope.Add("wl.emails");
            mio.Scope.Add("wl.birthday");
            mio.Scope.Add("wl.photos");
            mio.Scope.Add("wl.postal_addresses");
            app.UseMicrosoftAccountAuthentication(mio);

            ///
            /// TWITTER
            ///
            app.UseTwitterAuthentication(new Microsoft.Owin.Security.Twitter.TwitterAuthenticationOptions
            {
                ConsumerKey = Utils.GetAppSetting("TwitterConsumerKey"),
                ConsumerSecret = Utils.GetAppSetting("TwitterConsumerSecret"),
            });

            ///
            /// FACEBOOK
            ///
            var facebookProvider = new Microsoft.Owin.Security.Facebook.FacebookAuthenticationProvider
            {
                OnAuthenticated = (context) =>
                {
                    context.Identity.AddClaim(new System.Security.Claims.Claim("urn:facebook:access_token", context.AccessToken, XmlSchemaString, "Facebook"));
                    //context.Identity.AddClaim(new System.Security.Claims.Claim("FacebookAccessToken", context.AccessToken));
                    foreach (var claim in context.User)
                    {
                        var claimType = string.Format("urn:facebook:{0}", claim.Key);
                        string claimValue = claim.Value.ToString();
                        if (!context.Identity.HasClaim(claimType, claimValue))
                            context.Identity.AddClaim(new System.Security.Claims.Claim(claimType, claimValue, XmlSchemaString, "Facebook"));
                    }
                    return System.Threading.Tasks.Task.FromResult(0);
                }
            };
            var fao = new Microsoft.Owin.Security.Facebook.FacebookAuthenticationOptions
            {
                AppId = Utils.GetAppSetting("FaceBookAppId"),
                AppSecret = Utils.GetAppSetting("FaceBookAppSecret"),
                Provider = facebookProvider,
                CallbackPath = new PathString("/signin-facebook"),
            };
            fao.Scope.Add("public_profile");
            fao.Scope.Add("user_friends");
            fao.Scope.Add("email");
            //fao.Scope.Add("gender");
            fao.Scope.Add("user_birthday");
            ////fao.Scope.Add("first_name");
            ////fao.Scope.Add("last_name");
            fao.Scope.Add("user_likes");
            fao.Scope.Add("user_about_me");
            fao.Scope.Add("user_photos");
            app.UseFacebookAuthentication(fao);
            ///
            /// GOOGLE
            ///
            var googleProvider = new Microsoft.Owin.Security.Google.GoogleOAuth2AuthenticationProvider
            {
                OnAuthenticated = (context) =>
                {
                    foreach (var claim in context.User)
                    {
                        var claimType = string.Format("urn:google:{0}", claim.Key);
                        string claimValue = claim.Value.ToString();
                        if (!context.Identity.HasClaim(claimType, claimValue))
                            context.Identity.AddClaim(new System.Security.Claims.Claim(claimType, claimValue, XmlSchemaString, "Google"));
                    }
                    return System.Threading.Tasks.Task.FromResult(0);
                }
            };
            var goo = new Microsoft.Owin.Security.Google.GoogleOAuth2AuthenticationOptions
            {
                ClientId = Utils.GetAppSetting("GoogleClientId"),
                ClientSecret = Utils.GetAppSetting("GoogleClientSecret"),
                CallbackPath = new PathString("/signin-google"),
                Provider = googleProvider,
            };
            app.UseGoogleAuthentication(goo);

            ///
            /// GITHUB : [10026] ADD: Github for external login
            ///
            var githubProvider = new GitHubAuthenticationProvider
            {
                OnAuthenticated = (context) =>
                {
                    foreach (var claim in context.User)
                    {
                        var claimType = string.Format("urn:github:{0}", claim.Key);
                        string claimValue = claim.Value.ToString();
                        if (!context.Identity.HasClaim(claimType, claimValue))
                            context.Identity.AddClaim(new System.Security.Claims.Claim(claimType, claimValue, XmlSchemaString, "GitHub"));
                    }
                    return System.Threading.Tasks.Task.FromResult(0);
                }
            };
            var git = new GitHubAuthenticationOptions
            {
                ClientId = Utils.GetAppSetting("GitHubClientId"),
                ClientSecret = Utils.GetAppSetting("GitHubClientSecret"),
                Provider = githubProvider,
            };
            //git.Scope.Add("avatar_url");
            //git.Scope.Add("user");
            //git.Scope.Add("email");
            //git.Scope.Add("repo");
            //git.Scope.Add("gist");
            app.UseGitHubAuthentication(git);

        }
    }
}