using UnityEngine;
using System.Collections;
using static RTool.Localization.LocalizedDataManager;

[System.Serializable]
public class StringDictionary : LocalizationDictionary<string>
{
    public override string DefaultData
    {
        get
        {
            return string.Empty;
        }
    }
    public sealed override bool HasAdvancedFilter => true;
    public sealed override bool Filter(string _string, string _filter)
    {
        if (string.IsNullOrEmpty(_string))
            return false;
        if (_string.Contains(_filter))
            return true;
        return false;
    }
}
[System.Serializable]
public class TextureDictionary : UnityLocalizationDictionary<Texture> { }
