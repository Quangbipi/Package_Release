using UnityEngine;

namespace Quang.UI
{
    /// <summary>
    /// Scales this RectTransform uniformly so a block laid out at <see cref="designSize"/> fits inside its parent,
    /// never above <see cref="maxScale"/>. Used for the one flexible zone of a screen (e.g. the Home hero art)
    /// so fixed zones keep their size while the art shrinks on short screens such as tablets.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class UniformScaleFitter : MonoBehaviour
    {
        [SerializeField] private Vector2 designSize = new(390f, 406f);
        [SerializeField] private float maxScale = 1f;

        private Vector2 appliedParentSize = new(-1f, -1f);

        /// <summary>Uniform scale that fits <paramref name="design"/> into <paramref name="available"/>, capped at <paramref name="max"/>.</summary>
        public static float Compute(Vector2 available, Vector2 design, float max)
        {
            float scale = max;
            if (design.x > 0f) scale = Mathf.Min(scale, available.x / design.x);
            if (design.y > 0f) scale = Mathf.Min(scale, available.y / design.y);
            return Mathf.Max(0f, scale);
        }

        private void OnEnable()
        {
            appliedParentSize = new Vector2(-1f, -1f);
            Apply();
        }

        private void LateUpdate() => Apply();

        private void Apply()
        {
            if (transform.parent is not RectTransform parent) return;
            Vector2 size = parent.rect.size;
            if (size == appliedParentSize) return;
            appliedParentSize = size;

            float scale = Compute(size, designSize, maxScale);
            Vector3 target = new(scale, scale, 1f);
            if (transform.localScale != target) transform.localScale = target;
        }
    }
}
