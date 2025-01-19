# MyLocalization_UnityAsset
![GitHub release (latest by date)](https://img.shields.io/github/v/release/okhotnikov/MyLocalization_UnityAsset) ![GitHub License](https://img.shields.io/github/license/okhotnikov/MyLocalization_UnityAsset)

A powerful and easy-to-use localization system for Unity projects. Designed to simplify the management of multilingual content in games and applications.

## Installation

1. Open Unity and navigate to **Window -> Package Manager**.
2. Click the `+` button in the top left corner and select **Add package from git URL...**.
3. Enter the following URL and click **Add**:

https://github.com/okhotnikov/MyLocalization_UnityAsset.git

Alternatively, clone the repository directly and place it in your `Packages` folder.

Using .tgz archive
1. Download the archive [com.yourname.mypackage-0.1.1.tgz](https://github.com/yourname/repository/releases/download/v0.1.1/com.yourname.mypackage-0.1.1.tgz).
2. In Unity, open `Window > Package Manager`.
3. Click `+ > Add package from tarball...`.
4. Select the downloaded file.

## Usage

1. Add the `LocalizationManager` prefab to your scene.
2. Use the `LocalizationManager.GetText(key)` method to retrieve localized strings.
3. To add new languages, update the `LocalizationTable` asset in the `Resources` folder.

## Features
- Easy-to-use localization system
- Support for multiple languages
- Runtime language switching
- JSON-based localization files

## Example

```csharp
using Localization;

public class Example : MonoBehaviour
{
    void Start()
    {
        if (!MyLocalization.Instance.IsLoaded)
        {
            Debug.LogError("Localization is not loaded.");
            return;
        }

        if (!MyLocalization.Instance.IsSynced)
        {
            Debug.LogError("Localization is not synced.");
            return;
        }

        string greeting = MyLocalization.Get("greeting");
        Debug.Log(greeting);
    }
}
```

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

For information about third-party libraries and their licenses, see [THIRD_PARTY_LICENSES.md](THIRD_PARTY_LICENSES.md).

## Latest Release
[Download v0.1.1](https://github.com/okhotnikov/MyLocalization_UnityAsset/releases/tag/v0.1.1)