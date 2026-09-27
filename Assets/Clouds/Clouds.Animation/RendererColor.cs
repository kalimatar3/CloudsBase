using UnityEngine;

namespace Clouds.Animation
{
    /// <summary>
    /// Đọc/ghi màu của Renderer qua MaterialPropertyBlock.
    ///
    /// Không dùng renderer.material vì nó tạo một bản sao material riêng cho từng instance — phá
    /// batching và rò rỉ khi object bị destroy. Cũng không dùng sharedMaterial vì đó là asset dùng
    /// chung: đổi màu một object là nhuộm luôn mọi object khác đang dùng chính material đó.
    /// </summary>
    public static class RendererColor
    {
        // URP đặt tên property màu là _BaseColor; shader built-in và Sprites/Default dùng _Color.
        private static readonly int BaseColorId   = Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyColorId = Shader.PropertyToID("_Color");

        public static int ResolveProperty(Renderer renderer)
        {
            Material mat = renderer.sharedMaterial;
            return mat != null && !mat.HasProperty(BaseColorId) ? LegacyColorId : BaseColorId;
        }

        public static Color Read(Renderer renderer, MaterialPropertyBlock block, int propertyId)
        {
            renderer.GetPropertyBlock(block);
            if (block.HasColor(propertyId)) return block.GetColor(propertyId);

            Material mat = renderer.sharedMaterial;
            return mat != null && mat.HasProperty(propertyId) ? mat.GetColor(propertyId) : Color.white;
        }

        public static void Write(Renderer renderer, MaterialPropertyBlock block, int propertyId, Color color)
        {
            renderer.GetPropertyBlock(block);
            block.SetColor(propertyId, color);
            renderer.SetPropertyBlock(block);
        }
    }
}
