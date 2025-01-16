namespace MLoc.Targets
{
    using TMPro;
    using UnityEngine;

    [AddComponentMenu("MyLocalization/TextMeshPro-MyLocalizationTarget")]
    public class TextMeshProMyLocalizationTarget : BaseMyLocalizationTarget
    {
        [SerializeField] private TMP_Text _target;

        protected override Object Target => _target;

        protected override void OnValidate()
        {
            base.OnValidate();

            if (_target == null)
                _target = GetComponent<TMP_Text>();

            if (_target == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    Debug.LogWarning(
                        $"Object {name} automatically added component TextMeshProUGUI for working MyLocalization with TextMeshPro.");
                    _target = gameObject.AddComponent<TextMeshProUGUI>();
                }
                else
                {
                    Debug.LogWarning(
                        $"Object {name} automatically added component TextMeshPro for working MyLocalization with TextMeshPro.");
                    _target = gameObject.AddComponent<TextMeshPro>();
                }
            }

            if (_target == null)
                Debug.LogError("TextMeshProMyColorTarget requires a TMP_Text component.");
        }

        protected override string GetBaseText() => _target?.text;

        protected override void SetLocalizedText(string value)
        {
            if (_target == null)
                return;

            _target.text = value;
        }
    }
}
