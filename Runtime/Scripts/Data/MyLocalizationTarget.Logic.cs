namespace MLoc.Data
{
    public partial class MyLocalizationTarget
    {
        // public string GetColorName() => MyLocalization.Instance?.GetColorName(_colorGuid);

        // public bool IsLinked() => MyColorGallery.Instance?.GetColorData(_colorGuid) != null;

        public void ResetGuid() => _tagGuid = string.Empty;
    }
}
