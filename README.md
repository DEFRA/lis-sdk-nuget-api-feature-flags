# Defra Livestock API SDK (NuGet)
**Root Namespace**: *Defra.Livestock.Sdk.Api*

---

## Feature Flags

**Namespace**: *Defra.Livestock.Sdk.Api.FeatureFlags*

This package provides services which connect to the feature flag REST API in order to retrieve feature flags for a given product and environment.

### Installation & Registration

To use the Feature Flags SDK, register the services in your `IServiceCollection` using the `AddFeatureFlags` extension method after referencing the package.

```csharp
using Defra.Livestock.Sdk.Api.FeatureFlags;

// In your Program.cs or Startup.cs
builder.Services.AddFeatureFlags(options =>
{
    options.BaseUrl = "https://feature-flag-url.com";
    options.ApiKey = "your-api-key";
    options.ProductName = "YourProductName";
    options.EnvironmentName = "dev";
    options.CacheTimeoutSeconds = 1800; // Optional: Default is 1800, must be 30 or greater
    options.WithCorrelationId(() => Guid.NewGuid()); // Optional: Factory method for dynamic Correlation ID attachment
});
```

### Configuration Options

The `FeatureFlagOptions` class includes the following properties:

| Property / Method | Type | Description | Required |
| :--- | :--- | :--- | :--- |
| `BaseUrl` | `string` | The base URL of the Feature Flag API. | Yes |
| `ApiKey` | `string` | The API key for authentication. | Yes |
| `ProductName` | `string` | The name of your product. | Yes |
| `EnvironmentName` | `string` | The environment (e.g., dev, test, ext-test, prod). | Yes |
| `CacheTimeoutSeconds` | `int` | How long to cache flag values (min 30s). | No (Default: 1800) |
| `WithCorrelationId()` | `Func<Guid>` | Factory method to generate a Correlation ID for requests. If this is not supplied, the SDK will generate a GUID.| No |

### Usage

Inject `IFeatureFlagService` into your controllers or services to evaluate feature flags.

```csharp
public class MyService
{
    private readonly IFeatureFlagService _featureFlagService;

    public MyService(IFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }

    public async Task DoWork()
    {
        // Check a single feature flag
        var flag = await _featureFlagService.GetFeatureFlagStatusAsync("MyGroup", "MyFlag");
        
        if (flag.FlagEnabled)
        {
            // Execute new feature logic
        }

        // Check a whole group of flags
        var group = await _featureFlagService.GetFeatureFlagGroupStatusAsync("MyGroup");
        
        if (group.GroupEnabled)
        {
            foreach (var feature in group.Features)
            {
                Console.WriteLine($"Flag: {feature.FlagName}, Enabled: {feature.FlagEnabled}");
            }
        }
    }
}
```

### Response Objects

#### FeatureFlagStatus
Returned when querying an individual flag or as a list of features when querying a group.
- `FlagName` (string): The name of the flag.
- `FlagEnabled` (bool): Whether the flag is enabled.
- `Success` (bool): Whether the flag could be retrieved successfully.

#### FeatureFlagGroupStatus
Returned when querying a flag group.
- `GroupName` (string): The name of the group.
- `GroupEnabled` (bool): Whether the group itself is enabled.
- `Features` (List<FeatureFlagStatus>): List of individual feature flag statuses within the group.
- `Success` (bool): Whether the group could be retrieved successfully.

### Error Handling

The SDK may throw the following exceptions:

- `FeatureFlagParameterException`: Thrown when the parameters supplied to one of the methods are invalid or if the Correlation ID, if supplied, is an empty GUID.
- `FeatureFlagQueryException`: Thrown when there is an error communicating with the Feature Flag API or processing the response.

### Logging

The SDK will log out to the ILogger interface provided by the DI container. However, only the attempt at communicating with the API is logged.

Consumers of this package are expected to handle logging themselves should an exception be raised. This is in order to avoid double logging.

