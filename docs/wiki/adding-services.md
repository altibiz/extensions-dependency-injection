# Adding services

Extension methods called `Add*AssignableTo` are used to add services to the
service collection like so:

```cs
var serviceCollection = new ServiceCollection();
serviceCollection.AddSingletonAssignableTo<IMyInterface>();
```

These methods add a service for each concrete type that implements the interface
and for each of the types it adds them as implementations to all of their
interfaces.

If a type implements `IHostedLifecycleService` then the service is not added as
`IHostedService` even though it implements it.

Example of usage from tests:

<!-- markdownlint-disable MD013 -->

```cs
{{ #include ../../test/Altibiz.DependencyInjection.Extensions.Test/ServiceCollectionExtensionsTest.cs:1:7 }}
{{ #include ../../test/Altibiz.DependencyInjection.Extensions.Test/ServiceCollectionExtensionsTest.cs:8:149 }}
{{ #include ../../test/Altibiz.DependencyInjection.Extensions.Test/ServiceCollectionExtensionsTest.cs:244: }}
```

<!-- markdownlint-enable MD013 -->
