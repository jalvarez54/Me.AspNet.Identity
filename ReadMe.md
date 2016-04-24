2016-04-23: ASP.NET MVC Identity
=================================================================


Internet Ref. projects

"Render therefore unto Caesar the things which are Caesar's; and unto God the things that are God's."

- [ASP.NET Identity](http://www.asp.net/identity)
    
////////////////////////////////////////////////////////////////////////////////////////////////////////

VERSION PRODUIT : 1

////////////////////////////////////////////////////////////////////////////////////////////////////////

- EXEC: PM> Enable-Migrations -EnableAutomaticMigrations

- ADD: seed in Configuration.cs

```
            if (!context.Roles.Any(r => r.Name == "admin"))
            {
                var store = new RoleStore<IdentityRole>(context);
                var manager = new RoleManager<IdentityRole>(store);
                var role = new IdentityRole { Name = "admin" };

                manager.Create(role);
            }
            if (!context.Roles.Any(r => r.Name == "member"))
            {
                var store = new RoleStore<IdentityRole>(context);
                var manager = new RoleManager<IdentityRole>(store);
                var role = new IdentityRole { Name = "member" };

                manager.Create(role);
            }
            if (!context.Roles.Any(r => r.Name == "canedit"))
            {
                var store = new RoleStore<IdentityRole>(context);
                var manager = new RoleManager<IdentityRole>(store);
                var role = new IdentityRole { Name = "canedit" };

                manager.Create(role);
            }

            if (!context.Users.Any(u => u.UserName == "admin"))
            {
                var store = new UserStore<ApplicationUser>(context);
                var manager = new UserManager<ApplicationUser>(store);
                var user = new ApplicationUser { UserName = "admin", Pseudo = "admin", Email = "jose.alvarez54@live.fr" };

                manager.Create(user, "P@ssword2016");
                manager.AddToRole(user.Id, "admin");
            }
```

- EXEC: PM> Add-Migration Initialization

- EXEC: PM> Update-Database

- RENAME files from App_Settings folder:

    - Distrib-appSettings-Debug.config to appSettings-Debug.config
    - Distrib-appSettings-Release.config to appSettings-Release.config
    - Distrib-MailSettings-Common.config to MailSettings-Common.config
    - Distrib-connectionStrings-Release.config to connectionStrings-Release.config

Replace _"YOUR_VALUE"_ fields with your values.