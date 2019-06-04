using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.UI;
using RTool.LayoutSwitcher;

public class TextureSwitcherInjector : DataSwitcherInjector<Texture>
{
    protected override Dictionary<Type, Action<Component, Texture>> ComponentDictionary
    {
        get
        {
            return _dictionary;
        }
    }

    [System.Serializable]
    protected class TextureEvent : UnityEvent<Texture> { };
    [SerializeField]
    protected TextureEvent onTextureLoaded;

    protected override void OnManualSwitch(Texture _texture)
    {
        onTextureLoaded.Invoke(_texture);
    }

    [SerializeField, HideInInspector]
    private List<Sprite> _spriteList = new List<Sprite>();
    private Sprite GetSprite(Texture _texture)
    {
        for (int i = 0; i < dataList.Count; i++)
        {
            if (_texture == dataList[i])
            {
                if (_spriteList[i] == null)
                    _spriteList[i] = Sprite.Create(dataList[i] as Texture2D, new Rect(0, 0, dataList[i].width, dataList[i].height), new Vector2(0.5f, 0.5f));
                return _spriteList[i];
            }
        }
        return null;
    }
    

    private readonly Dictionary<Type, Action<Component, Texture>> _dictionary
        = new Dictionary<Type, Action<Component, Texture>>()
        {
            //{
            //    typeof(UITexture), (_component, _texture) =>
            //    {
            //        UITexture uiTexture = _component as UITexture;
            //        uiTexture.mainTexture = _texture;
            //        TransformSwitcher daTransformSwitcher = uiTexture.GetComponent<TransformSwitcher>();
            //        if (daTransformSwitcher == null)
            //        {
            //            uiTexture.width = _texture.width;
            //            uiTexture.height = _texture.height;
            //        }
            //    }
            //},
            {
                typeof(RawImage), (_component, _texture) =>
                {
                    RawImage rawImage = _component as RawImage;
                    rawImage.texture = _texture;
                    TransformSwitcher daTransformSwitcher = rawImage.GetComponent<TransformSwitcher>();
                    if (daTransformSwitcher == null)
                        rawImage.SetNativeSize();
                }
            },
            {
                typeof(Image), (_component, _texture) =>
                {
                    Image image = _component as Image;
                    Sprite sprite = null;
                    if (_texture != null)
                        sprite = Sprite.Create(_texture as Texture2D, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f));
                    image.sprite = sprite;
                }
            },
            {
                typeof(SpriteRenderer), (_component, _texture) =>
                {
                    SpriteRenderer spriteRenderer = _component as SpriteRenderer;
                    Sprite sprite = null;
                    if (_texture != null)
                        sprite = Sprite.Create(_texture as Texture2D, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f));
                    spriteRenderer.sprite = sprite;
                }
            },
            {
                typeof(MeshRenderer), (_component, _texture) =>
                {
                    SpriteRenderer spriteRenderer = _component as SpriteRenderer;
                    Sprite sprite = null;
                    if (_texture != null)
                        sprite = Sprite.Create(_texture as Texture2D, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f));
                    spriteRenderer.sprite = sprite;
                }
            },
        };
}