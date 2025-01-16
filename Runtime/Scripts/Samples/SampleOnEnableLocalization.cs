using MLoc;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class SampleOnEnableLocalization : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _target;
    [SerializeField] private string _tag;

    private void OnEnable()
    {
        if (_target == null)
        {
            Debug.LogError("TextMeshProUGUI is not assigned.");
            return;
        }

        if (string.IsNullOrEmpty(_tag))
        {
            Debug.LogError("Tag is not assigned.");
            return;
        }

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

        _target.text = MyLocalization.Get(_tag);
    }
}
