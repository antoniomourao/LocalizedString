# Localized String

Wrap localized content with code that facilitates replacing that content for different cultures. The service will get the localized strings from the resource file and present according to the current culture.

Each culture is placed in its own files with a suffix with the culture 2 digit iso code.

Default culture should not have any suffix.

Not supported cultures will fallback into the default culture.

Tranlsations are defined in pairs, key and value. Key is used to access the value that is the translated string.
File name where the translations are located depends os the approach that is taken. One of two approaches can be used: One file for all or one file for each resource.

File content supported in two flavours:
 - strings: content is saved in plain strings with a "=" separating the key from the value.
 - json: content is saved in json formatted content 

Steps to setup Localizer:

## 1 Setup application settings

**ResourcesDirectory**: Identify target folder where to read the resources files from.
**SupportedCultures**: List all supported cultures, the first entry will be set as the default culture.
**ResourcesExtension**: Resource file extensions, that will set the file content format: `json`, `strings`.
``` json
  "LocalizedStringOptions": {
    "ResourcesDirectory": "Resources",
    "SupportedCultures": [
      "en",
      "pt"
    ],
    "ResourcesExtension": "strings" 
  },
```
## 2 Setup Resource files

Files with localization strings should be placed in the `Resources` directory defined in the application settings file. 
```
Resources
``````

The project should be configured to copy the resources directory content into the output folder:
(Edit `.csproj` file and add the following)
``` xml
  </ItemGroup>
    <ItemGroup>
    <None Update="Resources\*.*" CopyToOutputDirectory="PreserveNewest" ExcludeFromSingleFile="true" />
  </ItemGroup>
```

## 3 Configure Localization services
Configuration services are configured in ´program.cs´:
``` csharp
        // Read localization Options
        service.AddOptions<LocalizedStringOptions>()
            .Bind(configuration.GetSection("LocalizedStringOptions"))
            .ValidateDataAnnotations();

        // Add localization Options to the container.
        var instanceLocalizerOptions = service
            .BuildServiceProvider()
            .GetRequiredService<IOptionsMonitor<LocalizedStringOptions>>()
            .CurrentValue;
        // Set the ResourcesPath to the full path of the ResourcesDirectory
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        instanceLocalizerOptions.ResourcesPath = Path.Combine(basePath, instanceLocalizerOptions.ResourcesDirectory);
        service.AddSingleton<ILocalizedStringOptions>(_ => instanceLocalizerOptions);

        // Add localization directory to the container.
        service.AddLocalization(options => options.ResourcesPath = instanceLocalizerOptions.ResourcesPath);

        // Add localization factory and helper classes into the container.
        service.AddScoped(typeof(ILocalizedString<>), typeof(LocalizedString<>));
        service.AddScoped<ILocalizedStringResources, LocalizedStringResources>();
        service.AddScoped<ILocalizedStringFactory, LocalizedStringFactory>();
```

## 4 Configure supported cultures
``` csharp
        // set applications supported cultures
        var localizedOptions = new LocalizedStringOptions();
        configuration.GetSection("LocalizedStringOptions")
                .Bind(localizedOptions);
        var localizationOptions = new LocalizedStringOptions()
            .SetDefaultCulture(localizedOptions.SupportedCultures[0])
            .AddSupportedCultures(localizedOptions.SupportedCultures)
            .AddSupportedUICultures(localizedOptions.SupportedCultures);
```
## 5 How to use 

### 5.1 Let dependency injection provide an instance for it

### 5.2 Use it in code

### 5.3 Use it in HTML