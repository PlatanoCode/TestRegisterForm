using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.ExpressApp.SystemModule;
using TestRegisterForm.Module.BusinessObjects;
using Microsoft.EntityFrameworkCore;
using DevExpress.ExpressApp.EFCore;
using DevExpress.EntityFrameworkCore.Security;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using Security.Extensions;
using Security.Extensions.Services;

namespace TestRegisterForm.Blazor.Server;

public class TestRegisterFormBlazorApplication : BlazorApplication {
    public TestRegisterFormBlazorApplication() {
        ApplicationName = "TestRegisterForm";
        CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;
        DatabaseVersionMismatch += TestRegisterFormBlazorApplication_DatabaseVersionMismatch;
    }
    protected override void OnSetupStarted() {
        base.OnSetupStarted();
#if DEBUG
        if(System.Diagnostics.Debugger.IsAttached && CheckCompatibilityType == CheckCompatibilityType.DatabaseSchema) {
            DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
        }
#endif
    }
    private void TestRegisterFormBlazorApplication_DatabaseVersionMismatch(object sender, DatabaseVersionMismatchEventArgs e) {
#if EASYTEST
        e.Updater.Update();
        e.Handled = true;
#else
        if(System.Diagnostics.Debugger.IsAttached) {
            e.Updater.Update();
            e.Handled = true;
        }
        else {
            string message = "The application cannot connect to the specified database, " +
                "because the database doesn't exist, its version is older " +
                "than that of the application or its schema does not match " +
                "the ORM data model structure. To avoid this error, use one " +
                "of the solutions from the https://www.devexpress.com/kb=T367835 KB Article.";

            if(e.CompatibilityError != null && e.CompatibilityError.Exception != null) {
                message += "\r\n\r\nInner exception: " + e.CompatibilityError.Exception.Message;
            }
            throw new InvalidOperationException(message);
        }
#endif
    }

    
}

public static class ApplicationBuilderExtensions
{
    // Adds the SecurityExtensionsModule to the application and configures the required services.
    public static IModuleBuilder<IBlazorApplicationBuilder> AddSecurityExtensions(this IModuleBuilder<IBlazorApplicationBuilder> builder,
        Action<SecurityExtensionsOptions> configureOptions)
    {
        SecurityExtensionsOptions options = new();
        configureOptions.Invoke(options);
        ArgumentNullException.ThrowIfNull(options.CreateSecuritySystemUser);

        builder.Add<SecurityExtensionsModule>();
        builder.Context.Services.Configure<SecurityExtensionsOptions>(o => o.CreateSecuritySystemUser = options.CreateSecuritySystemUser);
        builder.Context.Services.AddScoped<RestorePasswordService>();
        builder.Context.Services.AddScoped<UserRegistrationService>();
        return builder;
    }
}
