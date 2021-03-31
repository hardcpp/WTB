using UnityEngine;

namespace WTB.SDK.Unity
{
    /// <summary>
    /// Sprite helper
    /// </summary>
    internal static class Sprite
    {
        /// <summary>
        /// Create sprite from texture
        /// </summary>
        /// <param name="p_Texture">Source texture</param>
        /// <param name="p_PixelsPerUnit">Pixel per unit</param>
        /// <param name="p_Pivot">Pivot point</param>
        /// <returns></returns>
        internal static UnityEngine.Sprite CreateFromTexture(UnityEngine.Texture2D p_Texture, float p_PixelsPerUnit = 100.0f, Vector2 p_Pivot = default)
        {
            if (p_Texture != null && p_Texture)
            {
                var l_Sprite = UnityEngine.Sprite.Create(p_Texture, new Rect(0, 0, p_Texture.width, p_Texture.height), p_Pivot, p_PixelsPerUnit);
                l_Sprite.texture.wrapMode = TextureWrapMode.Clamp;

                return l_Sprite;
            }

            return null;
        }
    }
}
