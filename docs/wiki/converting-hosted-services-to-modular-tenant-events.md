# Converting hosted services to modular tenant events

The `ConvertHostedServicesToModularTenantEvents` extension method is used to
convert all hosted services to OrchardCore modular tenant events.

Example of usage:

```cs
var serviceCollection = new ServiceCollection();
var actual = serviceCollection.ConvertHostedServicesToModularTenantEvents();
```

This is necessary in any scenario when using hosted services in OrchardCore
tenants.

Example of usage from tests:

<!-- markdownlint-disable MD013 -->

```cs
{{ #include ../../test/Altibiz.DependencyInjection.Extensions.Test/ServiceCollectionExtensionsTest.cs:1:7 }}
{{ #include ../../test/Altibiz.DependencyInjection.Extensions.Test/ServiceCollectionExtensionsTest.cs:135:207 }}
{{ #include ../../test/Altibiz.DependencyInjection.Extensions.Test/ServiceCollectionExtensionsTest.cs:208: }}
```

<!-- markdownlint-enable MD013 -->
