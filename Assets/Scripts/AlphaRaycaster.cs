using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("Event/Alpha Check"), ExecuteInEditMode]
public class AlphaCheck : MonoBehaviour, ICanvasRaycastFilter
{
    [Range(0, 1), Tooltip("Texture regions with opacity (alpha) lower than alpha threshold won't react to input events.")]
    public float AlphaThreshold = .9f;
    [Tooltip("Whether material tint color should affect alpha threshold.")]
    public bool IncludeMaterialAlpha;

    private GameObject gameObj;
    private Image checkedImage;
    private RawImage checkedRawImage;
    private Text checkedText;
    private bool isSetupValid;

    private void Awake()
    {
        gameObj = gameObject;
        checkedImage = GetComponent<Image>();
        checkedRawImage = GetComponent<RawImage>();
        checkedText = GetComponent<Text>();
        isSetupValid = checkedImage || checkedRawImage || checkedText;
    }

    public bool IsRaycastLocationValid(Vector2 screenPosition, Camera eventCamera)
    {
        if (!isSetupValid) return true;

        if (checkedImage)
            return !AlphaRaycaster.AlphaCheckImage(gameObj, checkedImage, screenPosition, eventCamera, AlphaThreshold, IncludeMaterialAlpha);
        if (checkedRawImage)
            return !AlphaRaycaster.AlphaCheckRawImage(gameObj, checkedRawImage, screenPosition, eventCamera, AlphaThreshold, IncludeMaterialAlpha);
        if (checkedText)
            return !AlphaRaycaster.AlphaCheckText(gameObj, checkedText, screenPosition, eventCamera, AlphaThreshold, IncludeMaterialAlpha);

        return true;
    }
}

[AddComponentMenu("Event/Alpha Raycaster"), ExecuteInEditMode]
public class AlphaRaycaster : GraphicRaycaster
{
    [Header("Alpha test properties")]
    [Range(0, 1), Tooltip("Texture regions of the UI objects with opacity (alpha) lower than alpha threshold won't react to input events.")]
    public float AlphaThreshold = .9f;
    [Tooltip("Whether material tint color of the UI objects should affect alpha threshold.")]
    public bool IncludeMaterialAlpha;
    [Tooltip("When selective mode is active the alpha testing will only execute for UI objects with AlphaCheck component.")]
    public bool SelectiveMode;
    [Tooltip("Show warnings in the console when attempting to alpha test objects with a not-readable texture.")]
    public bool ShowTextureWarnings;

    protected override void OnEnable()
    {
        base.OnEnable();

        // Remove default raycaster (we're replacing it).
        var graphicRaycaster = GetComponent<GraphicRaycaster>();
        if (graphicRaycaster && graphicRaycaster != this) DestroyImmediate(graphicRaycaster);
    }

    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        base.Raycast(eventData, resultAppendList);

        if (SelectiveMode) return;

        for (int i = resultAppendList.Count - 1; i >= 0; i--)
        {
            if (resultAppendList[i].gameObject.GetComponent<AlphaCheck>())
                continue;

            try
            {
                var objImage = resultAppendList[i].gameObject.GetComponent<Image>();
                if (objImage)
                {
                    if (objImage.sprite && AlphaCheckImage(resultAppendList[i].gameObject, objImage, eventData.position,
                        eventCamera, AlphaThreshold, IncludeMaterialAlpha))
                        resultAppendList.RemoveAt(i);
                    continue;
                }

                var objRawImage = resultAppendList[i].gameObject.GetComponent<RawImage>();
                if (objRawImage)
                {
                    if (AlphaCheckRawImage(resultAppendList[i].gameObject, objRawImage, eventData.position,
                        eventCamera, AlphaThreshold, IncludeMaterialAlpha))
                        resultAppendList.RemoveAt(i);
                    continue;
                }

                var objText = resultAppendList[i].gameObject.GetComponent<Text>();
                if (objText)
                {
                    if (AlphaCheckText(resultAppendList[i].gameObject, objText, eventData.position,
                        eventCamera, AlphaThreshold, IncludeMaterialAlpha))
                        resultAppendList.RemoveAt(i);
                    continue;
                }
            }
            catch (UnityException e)
            {
                if (Application.isEditor && ShowTextureWarnings)
                    Debug.LogWarning(string.Format("Alpha test failed: {0}", e.Message));
            };
        }
    }

    // Return true if alpha check for image is positive (need to exclude the result).
    public static bool AlphaCheckImage(GameObject obj, Image objImage, Vector2 pointerPos, Camera eventCamera,
        float alphaThreshold, bool includeMaterialAlpha)
    {
        var objTrs = obj.transform as RectTransform;
        var pointerLPos = ScreenToLocalObjectPosition(pointerPos, objTrs, eventCamera);
        var objTex = objImage.mainTexture as Texture2D;

        var texRect = GetImageTextureRect(objImage);
        var objSize = objTrs.rect.size;

        // Correcting objSize in case "preserve aspect" is enabled 
        if (objImage.preserveAspect)
        {
            if (objSize.x < objSize.y) objSize.y = objSize.x * (texRect.height / texRect.width);
            else objSize.x = objSize.y * (texRect.width / texRect.height);

            // Also we need to cut off empty object space
            var halfPivot = new Vector2(Mathf.Abs(objTrs.pivot.x) == .5f ? 2 : 1, Mathf.Abs(objTrs.pivot.y) == .5f ? 2 : 1);
            if (Mathf.Abs(pointerLPos.x * halfPivot.x) > objSize.x || Mathf.Abs(pointerLPos.y * halfPivot.y) > objSize.y) return true;
        }

        // Evaluating texture coordinates of the targeted spot
        float texCorX = pointerLPos.x + objSize.x * objTrs.pivot.x;
        float texCorY = pointerLPos.y + objSize.y * objTrs.pivot.y;

        #region	TILED_SLICED
        // Will be used if image has a border
        var borderTotalWidth = objImage.sprite.border.x + objImage.sprite.border.z;
        var borderTotalHeight = objImage.sprite.border.y + objImage.sprite.border.w;
        var fillRect = new Rect(objImage.sprite.border.x, objImage.sprite.border.y,
            Mathf.Clamp(objSize.x - borderTotalWidth, 0f, Mathf.Infinity),
            Mathf.Clamp(objSize.y - borderTotalHeight, 0f, Mathf.Infinity));
        var isInsideFillRect = objImage.hasBorder && fillRect.Contains(new Vector2(texCorX, texCorY));

        // Correcting texture coordinates in case image is tiled
        if (objImage.type == Image.Type.Tiled)
        {
            if (isInsideFillRect)
            {
                if (!objImage.fillCenter) return true;

                texCorX = objImage.sprite.border.x + (texCorX - objImage.sprite.border.x) % (texRect.width - borderTotalWidth);
                texCorY = objImage.sprite.border.y + (texCorY - objImage.sprite.border.y) % (texRect.height - borderTotalHeight);
            }
            else if (objImage.hasBorder)
            {
                // If objSize is below border size the border areas will shrink
                texCorX *= Mathf.Clamp(borderTotalWidth / objSize.x, 1f, Mathf.Infinity);
                texCorY *= Mathf.Clamp(borderTotalHeight / objSize.y, 1f, Mathf.Infinity);

                if (texCorX > texRect.width - objImage.sprite.border.z && texCorX < objImage.sprite.border.x + fillRect.width)
                    texCorX = objImage.sprite.border.x + (texCorX - objImage.sprite.border.x) % (texRect.width - borderTotalWidth);
                else if (texCorX > objImage.sprite.border.x + fillRect.width)
                    texCorX = texCorX - fillRect.width + texRect.width - borderTotalWidth;

                if (texCorY > texRect.height - objImage.sprite.border.w && texCorY < objImage.sprite.border.y + fillRect.height)
                    texCorY = objImage.sprite.border.y + (texCorY - objImage.sprite.border.y) % (texRect.height - borderTotalHeight);
                else if (texCorY > objImage.sprite.border.y + fillRect.height)
                    texCorY = texCorY - fillRect.height + texRect.height - borderTotalHeight;
            }
            else
            {
                if (texCorX > texRect.width) texCorX %= texRect.width;
                if (texCorY > texRect.height) texCorY %= texRect.height;
            }
        }
        // Correcting texture coordinates in case image is sliced
        else if (objImage.type == Image.Type.Sliced)
        {
            if (isInsideFillRect)
            {
                if (!objImage.fillCenter) return true;

                texCorX = objImage.sprite.border.x + (texCorX - objImage.sprite.border.x) * ((texRect.width - borderTotalWidth) / fillRect.width);
                texCorY = objImage.sprite.border.y + (texCorY - objImage.sprite.border.y) * ((texRect.height - borderTotalHeight) / fillRect.height);
            }
            else
            {
                // If objSize is below border size the border areas will shrink
                texCorX *= Mathf.Clamp(borderTotalWidth / objSize.x, 1f, Mathf.Infinity);
                texCorY *= Mathf.Clamp(borderTotalHeight / objSize.y, 1f, Mathf.Infinity);

                if (texCorX > objImage.sprite.border.x && texCorX < objImage.sprite.border.x + fillRect.width)
                    texCorX = objImage.sprite.border.x + (texCorX - objImage.sprite.border.x) * ((texRect.width - borderTotalWidth) / fillRect.width);
                else if (texCorX > objImage.sprite.border.x + fillRect.width)
                    texCorX = texCorX - fillRect.width + texRect.width - borderTotalWidth;

                if (texCorY > objImage.sprite.border.y && texCorY < objImage.sprite.border.y + fillRect.height)
                    texCorY = objImage.sprite.border.y + (texCorY - objImage.sprite.border.y) * ((texRect.height - borderTotalHeight) / fillRect.height);
                else if (texCorY > objImage.sprite.border.y + fillRect.height)
                    texCorY = texCorY - fillRect.height + texRect.height - borderTotalHeight;
            }
        }
        #endregion
        // Correcting texture coordinates by scale in case simple or filled image
        else
        {
            texCorX *= texRect.width / objSize.x;
            texCorY *= texRect.height / objSize.y;
        }

        // For filled images, check if targeted spot is outside of the filled area
        #region FILLED
        if (objImage.type == Image.Type.Filled)
        {
            var nCorX = texRect.height > texRect.width ? texCorX * (texRect.height / texRect.width) : texCorX;
            var nCorY = texRect.width > texRect.height ? texCorY * (texRect.width / texRect.height) : texCorY;
            var nWidth = texRect.height > texRect.width ? texRect.height : texRect.width;
            var nHeight = texRect.width > texRect.height ? texRect.width : texRect.height;

            if (objImage.fillMethod == Image.FillMethod.Horizontal)
            {
                if (objImage.fillOrigin == (int)Image.OriginHorizontal.Left && texCorX / texRect.width > objImage.fillAmount) return true;
                if (objImage.fillOrigin == (int)Image.OriginHorizontal.Right && texCorX / texRect.width < (1 - objImage.fillAmount)) return true;
            }

            if (objImage.fillMethod == Image.FillMethod.Vertical)
            {
                if (objImage.fillOrigin == (int)Image.OriginVertical.Bottom && texCorY / texRect.height > objImage.fillAmount) return true;
                if (objImage.fillOrigin == (int)Image.OriginVertical.Top && texCorY / texRect.height < (1 - objImage.fillAmount)) return true;
            }

            #region RADIAL_90
            if (objImage.fillMethod == Image.FillMethod.Radial90)
            {
                if (objImage.fillOrigin == (int)Image.Origin90.BottomLeft)
                {
                    if (objImage.fillClockwise && Mathf.Atan(nCorY / nCorX) / (Mathf.PI / 2) < (1 - objImage.fillAmount)) return true;
                    if (!objImage.fillClockwise && Mathf.Atan(nCorY / nCorX) / (Mathf.PI / 2) > objImage.fillAmount) return true;
                }

                if (objImage.fillOrigin == (int)Image.Origin90.TopLeft)
                {
                    if (objImage.fillClockwise && nCorY < -(1 / Mathf.Tan((1 - objImage.fillAmount) * Mathf.PI / 2)) * nCorX + nHeight) return true;
                    if (!objImage.fillClockwise && nCorY > -(1 / Mathf.Tan(objImage.fillAmount * Mathf.PI / 2)) * nCorX + nHeight) return true;
                }

                if (objImage.fillOrigin == (int)Image.Origin90.TopRight)
                {
                    if (objImage.fillClockwise && nCorY > Mathf.Tan((1 - objImage.fillAmount) * Mathf.PI / 2) * (nCorX - nWidth) + nHeight) return true;
                    if (!objImage.fillClockwise && nCorY < Mathf.Tan(objImage.fillAmount * Mathf.PI / 2) * (nCorX - nWidth) + nHeight) return true;
                }

                if (objImage.fillOrigin == (int)Image.Origin90.BottomRight)
                {
                    if (objImage.fillClockwise && nCorY > (1 / Mathf.Tan((1 - objImage.fillAmount) * Mathf.PI / 2)) * (nWidth - nCorX)) return true;
                    if (!objImage.fillClockwise && nCorY < (1 / Mathf.Tan(objImage.fillAmount * Mathf.PI / 2)) * (nWidth - nCorX)) return true;
                }
            }
            #endregion

            #region RADIAL_180
            if (objImage.fillMethod == Image.FillMethod.Radial180)
            {
                if (objImage.fillOrigin == (int)Image.Origin180.Bottom)
                {
                    if (objImage.fillClockwise && Mathf.Atan2(nCorY, 2 * (nCorX - nWidth / 2)) < (1 - objImage.fillAmount) * Mathf.PI) return true;
                    if (!objImage.fillClockwise && Mathf.Atan2(texCorY, 2 * (nCorX - nWidth / 2)) > objImage.fillAmount * Mathf.PI) return true;
                }

                if (objImage.fillOrigin == (int)Image.Origin180.Left)
                {
                    if (objImage.fillClockwise && Mathf.Atan2(nCorX, -2 * (nCorY - nHeight / 2)) < (1 - objImage.fillAmount) * Mathf.PI) return true;
                    if (!objImage.fillClockwise && Mathf.Atan2(nCorX, -2 * (nCorY - nHeight / 2)) > objImage.fillAmount * Mathf.PI) return true;
                }

                if (objImage.fillOrigin == (int)Image.Origin180.Top)
                {
                    if (objImage.fillClockwise && Mathf.Atan2(nHeight - nCorY, -2 * (nCorX - nWidth / 2)) < (1 - objImage.fillAmount) * Mathf.PI) return true;
                    if (!objImage.fillClockwise && Mathf.Atan2(nHeight - nCorY, -2 * (nCorX - nWidth / 2)) > objImage.fillAmount * Mathf.PI) return true;
                }

                if (objImage.fillOrigin == (int)Image.Origin180.Right)
                {
                    if (objImage.fillClockwise && Mathf.Atan2(nWidth - nCorX, 2 * (nCorY - nHeight / 2)) < (1 - objImage.fillAmount) * Mathf.PI) return true;
                    if (!objImage.fillClockwise && Mathf.Atan2(nWidth - nCorX, 2 * (nCorY - nHeight / 2)) > objImage.fillAmount * Mathf.PI) return true;
                }
            }
            #endregion

            #region RADIAL_360
            if (objImage.fillMethod == Image.FillMethod.Radial360)
            {
                if (objImage.fillOrigin == (int)Image.Origin360.Bottom)
                {
                    if (objImage.fillClockwise)
                    {
                        var angle = Mathf.Atan2(nCorY - nHeight / 2, nCorX - nWidth / 2) + Mathf.PI / 2;
                        var checkAngle = Mathf.PI * 2 * (1 - objImage.fillAmount);
                        angle = angle < 0 ? Mathf.PI * 2 + angle : angle;
                        if (angle < checkAngle) return true;
                    }
                    if (!objImage.fillClockwise)
                    {
                        var angle = Mathf.Atan2(nCorY - nHeight / 2, nCorX - nWidth / 2) + Mathf.PI / 2;
                        var checkAngle = Mathf.PI * 2 * objImage.fillAmount;
                        angle = angle < 0 ? Mathf.PI * 2 + angle : angle;
                        if (angle > checkAngle) return true;
                    }
                }

                if (objImage.fillOrigin == (int)Image.Origin360.Right)
                {
                    if (objImage.fillClockwise)
                    {
                        var angle = Mathf.Atan2(nCorY - nHeight / 2, nCorX - nWidth / 2);
                        var checkAngle = Mathf.PI * 2 * (1 - objImage.fillAmount);
                        angle = angle < 0 ? Mathf.PI * 2 + angle : angle;
                        if (angle < checkAngle) return true;
                    }
                    if (!objImage.fillClockwise)
                    {
                        var angle = Mathf.Atan2(nCorY - nHeight / 2, nCorX - nWidth / 2);
                        var checkAngle = Mathf.PI * 2 * objImage.fillAmount;
                        angle = angle < 0 ? Mathf.PI * 2 + angle : angle;
                        if (angle > checkAngle) return true;
                    }
                }

                if (objImage.fillOrigin == (int)Image.Origin360.Top)
                {
                    if (objImage.fillClockwise)
                    {
                        var angle = Mathf.Atan2(nCorY - nHeight / 2, nCorX - nWidth / 2) - Mathf.PI / 2;
                        var checkAngle = Mathf.PI * 2 * (1 - objImage.fillAmount);
                        angle = angle < 0 ? Mathf.PI * 2 + angle : angle;
                        if (angle < checkAngle) return true;
                    }
                    if (!objImage.fillClockwise)
                    {
                        var angle = Mathf.Atan2(nCorY - nHeight / 2, nCorX - nWidth / 2) - Mathf.PI / 2;
                        var checkAngle = Mathf.PI * 2 * objImage.fillAmount;
                        angle = angle < 0 ? Mathf.PI * 2 + angle : angle;
                        if (angle > checkAngle) return true;
                    }
                }

                if (objImage.fillOrigin == (int)Image.Origin360.Left)
                {
                    if (objImage.fillClockwise)
                    {
                        var angle = Mathf.Atan2(nCorY - nHeight / 2, nCorX - nWidth / 2) - Mathf.PI;
                        var checkAngle = Mathf.PI * 2 * (1 - objImage.fillAmount);
                        angle = angle < 0 ? Mathf.PI * 2 + angle : angle;
                        if (angle < checkAngle) return true;
                    }
                    if (!objImage.fillClockwise)
                    {
                        var angle = Mathf.Atan2(nCorY - nHeight / 2, nCorX - nWidth / 2) - Mathf.PI;
                        var checkAngle = Mathf.PI * 2 * objImage.fillAmount;
                        angle = angle < 0 ? Mathf.PI * 2 + angle : angle;
                        if (angle > checkAngle) return true;
                    }
                }
            }
            #endregion

        }
        #endregion

        // Getting targeted pixel alpha from object's texture 
        float alpha = objTex.GetPixel((int)(texCorX + texRect.x), (int)(texCorY + texRect.y)).a;

        // Deciding if we need to exclude the object from results list
        if (includeMaterialAlpha) alpha *= objImage.color.a;
        if (alpha < alphaThreshold) return true;

        return false;
    }

    // Return true if alpha check for text is positive (need to exclude the result).
    public static bool AlphaCheckText(GameObject obj, Text objText, Vector2 pointerPos, Camera eventCamera,
        float alphaThreshold, bool includeMaterialAlpha)
    {
        var objTrs = obj.transform as RectTransform;
        Vector2 pointerLPos;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(objTrs, pointerPos, eventCamera, out pointerLPos))
            return true;

        var characterRects = new List<CharacterRect>();
        for (int i = 0; i < objText.cachedTextGenerator.verts.Count; i += 4)
        {
            characterRects.Add(new CharacterRect(objText,
                objText.cachedTextGenerator.verts[i],
                objText.cachedTextGenerator.verts[i + 1],
                objText.cachedTextGenerator.verts[i + 2],
                objText.cachedTextGenerator.verts[i + 3]
            ));
        }

        var alpha = -1f;
        foreach (var charRect in characterRects)
        {
            if (charRect.Contains(pointerLPos))
            {
                alpha = charRect.GetTextureAlphaFromPosition(pointerLPos);
                break;
            }
        }
        if (alpha == -1) return true;

        if (includeMaterialAlpha) alpha *= objText.color.a;
        if (alpha < alphaThreshold) return true;

        return false;
    }

    // Return true if alpha check for raw image is positive (need to exclude the result).
    public static bool AlphaCheckRawImage(GameObject obj, RawImage objRawImage, Vector2 pointerPos, Camera eventCamera,
        float alphaThreshold, bool includeMaterialAlpha)
    {
        var objTrs = obj.transform as RectTransform;
        var pointerLPos = ScreenToLocalObjectPosition(pointerPos, objTrs, eventCamera);
        var objTex = objRawImage.mainTexture as Texture2D;

        var uvRect = objRawImage.uvRect;
        var objSize = objTrs.rect.size;

        // Evaluating texture coordinates of the targeted spot
        float texCorX = pointerLPos.x + objSize.x * objTrs.pivot.x;
        float texCorY = pointerLPos.y + objSize.y * objTrs.pivot.y;

        // Correcting texture coordinates by UV rect
        texCorX = (texCorX + uvRect.x) * uvRect.width;
        texCorY = (texCorY + uvRect.y) * uvRect.height;

        // Correcting texture coordinates by scale
        //texCorX *= texRect.width / objSize.x;
        //texCorY *= texRect.height / objSize.y;


        // Getting targeted pixel alpha from object's texture 
        float alpha = objTex.GetPixel((int)(texCorX + uvRect.x), (int)(texCorY + uvRect.y)).a;

        // Deciding if we need to exclude the object from results list
        if (includeMaterialAlpha) alpha *= objRawImage.color.a;
        if (alpha < alphaThreshold) return true;

        return false;
    }

    // Evaluating pointer position relative to UI object local space.
    private static Vector3 ScreenToLocalObjectPosition(Vector2 screenPosition, RectTransform objTrs, Camera eventCamera)
    {
        Vector3 pointerGPos;
        if (eventCamera)
        {
            var objPlane = new Plane(objTrs.forward, objTrs.position);
            float distance;
            var cameraRay = eventCamera.ScreenPointToRay(screenPosition);
            objPlane.Raycast(cameraRay, out distance);
            pointerGPos = cameraRay.GetPoint(distance);
        }
        else
        {
            pointerGPos = screenPosition;
            float rotationCorrection = (-objTrs.forward.x * (pointerGPos.x - objTrs.position.x) - objTrs.forward.y * (pointerGPos.y - objTrs.position.y)) / objTrs.forward.z;
            pointerGPos += new Vector3(0, 0, objTrs.position.z + rotationCorrection);
        }
        return objTrs.InverseTransformPoint(pointerGPos);
    }

    private static Rect GetImageTextureRect(Image objImage)
    {
        Rect texRect;

        // Case for sprites with redundant transparent areas (Unity trims them internally, so we have to handle that)
        if (objImage.sprite.textureRectOffset.sqrMagnitude > 0)
        {
            texRect = objImage.sprite.packed ? new Rect(objImage.sprite.textureRect.xMin - objImage.sprite.textureRectOffset.x,
            objImage.sprite.textureRect.yMin - objImage.sprite.textureRectOffset.y,
            objImage.sprite.textureRect.width + objImage.sprite.textureRectOffset.x * 2f,
            objImage.sprite.textureRect.height + objImage.sprite.textureRectOffset.y * 2f) : objImage.sprite.rect;
        }
        else texRect = objImage.sprite.textureRect;

        return texRect;
    }

    // Helper struct for alpha checking text.
    private struct CharacterRect
    {
        public Rect Rect;
        public Text Text;
        public UIVertex UpperLeftVertex;
        public UIVertex UpperRightVertex;
        public UIVertex BottomRightVertex;
        public UIVertex BottomLeftVertex;

        public Rect ScaledRect
        {
            get
            {
                var fontScaleFactor = (float)Text.fontSize / Text.font.fontSize;
                return new Rect(Rect.xMin * fontScaleFactor, Rect.yMin * fontScaleFactor,
                    Rect.width * fontScaleFactor, Rect.height * fontScaleFactor);
            }
        }

        public CharacterRect(Text text, UIVertex ulv, UIVertex urv, UIVertex brv, UIVertex blv)
        {
            UpperLeftVertex = ulv;
            UpperRightVertex = urv;
            BottomRightVertex = brv;
            BottomLeftVertex = blv;
            Text = text;
            Rect = new Rect(blv.position.x, blv.position.y,
                Mathf.Abs(ulv.position.x - urv.position.x),
                Mathf.Abs(ulv.position.y - blv.position.y));
        }

        public bool Contains(Vector2 position)
        {
            return ScaledRect.Contains(position);
        }

        public float GetTextureAlphaFromPosition(Vector2 position)
        {
            var normalizedPosition = Rect.PointToNormalized(ScaledRect, position);

            var texture = Text.mainTexture as Texture2D;
            var texCorX = Mathf.Lerp(BottomLeftVertex.uv0.x, BottomRightVertex.uv0.x, normalizedPosition.x) * texture.width;
            var texCorY = Mathf.Lerp(BottomLeftVertex.uv0.y, UpperLeftVertex.uv0.y, normalizedPosition.y) * texture.height;

            return texture.GetPixel((int)texCorX, (int)texCorY).a;
        }
    }
}
