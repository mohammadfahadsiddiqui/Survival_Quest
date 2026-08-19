using UnityEngine;
using UnityEngine.UI;

namespace SurvivalGame.UI
{
    /// <summary>
    /// Runtime health bar matching the supplied reference:
    /// heart icon + red segmented bar + current/max value below.
    /// No external sprite/font asset is required.
    /// </summary>
    public sealed class HealthBarUI : MonoBehaviour
    {
        private Image m_Fill;
        private Text m_ValueText;
        private const float MaxHealth = 100f;

        public static HealthBarUI Create(Canvas canvas)
        {
            if (canvas == null)
            {
                return null;
            }

            HealthBarUI existing = canvas.GetComponentInChildren<HealthBarUI>(true);
            if (existing != null)
            {
                return existing;
            }

            GameObject root = new GameObject("HealthBarUI", typeof(RectTransform), typeof(HealthBarUI));
            root.transform.SetParent(canvas.transform, false);

            RectTransform rootRect = (RectTransform)root.transform;
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(0f, 1f);
            rootRect.pivot = new Vector2(0f, 1f);
            rootRect.anchoredPosition = new Vector2(24f, -22f);
            rootRect.sizeDelta = new Vector2(355f, 92f);

            HealthBarUI ui = root.GetComponent<HealthBarUI>();
            ui.Build();
            return ui;
        }

        private void Build()
        {
            GameObject heartObject = new GameObject("Heart", typeof(RectTransform), typeof(Image));
            heartObject.transform.SetParent(transform, false);
            RectTransform heartRect = (RectTransform)heartObject.transform;
            heartRect.anchorMin = new Vector2(0f, 0.5f);
            heartRect.anchorMax = new Vector2(0f, 0.5f);
            heartRect.pivot = new Vector2(0f, 0.5f);
            heartRect.anchoredPosition = Vector2.zero;
            heartRect.sizeDelta = new Vector2(64f, 64f);

            Image heart = heartObject.GetComponent<Image>();
            heart.sprite = CreateHeartSprite(128);
            heart.preserveAspect = true;
            heart.raycastTarget = false;

            GameObject frameObject = CreateRect("HealthBarFrame", transform);
            RectTransform frameRect = (RectTransform)frameObject.transform;
            frameRect.anchorMin = new Vector2(0f, 0.5f);
            frameRect.anchorMax = new Vector2(0f, 0.5f);
            frameRect.pivot = new Vector2(0f, 0.5f);
            frameRect.anchoredPosition = new Vector2(72f, 9f);
            frameRect.sizeDelta = new Vector2(270f, 28f);

            Image frame = frameObject.AddComponent<Image>();
            frame.color = new Color(0.055f, 0.055f, 0.065f, 1f);
            frame.raycastTarget = false;

            GameObject backgroundObject = CreateRect("HealthBarBackground", frameObject.transform);
            RectTransform bgRect = (RectTransform)backgroundObject.transform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = new Vector2(3f, 3f);
            bgRect.offsetMax = new Vector2(-3f, -3f);

            Image background = backgroundObject.AddComponent<Image>();
            background.color = new Color(0.60f, 0.61f, 0.64f, 1f);
            background.raycastTarget = false;

            GameObject fillObject = CreateRect("HealthFill", backgroundObject.transform);
            RectTransform fillRect = (RectTransform)fillObject.transform;
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            m_Fill = fillObject.AddComponent<Image>();
            m_Fill.type = Image.Type.Filled;
            m_Fill.fillMethod = Image.FillMethod.Horizontal;
            m_Fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            m_Fill.fillAmount = 1f;
            m_Fill.color = new Color(0.88f, 0.13f, 0.16f, 1f);
            m_Fill.raycastTarget = false;

            CreateSegment(backgroundObject.transform, 0.32f);
            CreateSegment(backgroundObject.transform, 0.67f);

            GameObject valueObject = CreateRect("HealthValue", transform);
            RectTransform valueRect = (RectTransform)valueObject.transform;
            valueRect.anchorMin = new Vector2(0f, 0.5f);
            valueRect.anchorMax = new Vector2(0f, 0.5f);
            valueRect.pivot = new Vector2(1f, 0.5f);
            valueRect.anchoredPosition = new Vector2(342f, -27f);
            valueRect.sizeDelta = new Vector2(110f, 30f);

            m_ValueText = valueObject.AddComponent<Text>();
            m_ValueText.text = "100/100";
            m_ValueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_ValueText.fontSize = 20;
            m_ValueText.fontStyle = FontStyle.Bold;
            m_ValueText.alignment = TextAnchor.MiddleRight;
            m_ValueText.color = new Color(0.06f, 0.06f, 0.07f, 1f);
            m_ValueText.raycastTarget = false;
        }

        private static GameObject CreateRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void CreateSegment(Transform parent, float normalizedX)
        {
            GameObject segment = CreateRect("Segment", parent);
            RectTransform rect = (RectTransform)segment.transform;
            rect.anchorMin = new Vector2(normalizedX, 0f);
            rect.anchorMax = new Vector2(normalizedX, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(3f, 0f);
            Image image = segment.AddComponent<Image>();
            image.color = new Color(0.55f, 0.03f, 0.05f, 0.22f);
            image.raycastTarget = false;
        }

        public void SetHealth(float health)
        {
            float normalized = Mathf.Clamp01(health / MaxHealth);
            if (m_Fill != null)
            {
                m_Fill.fillAmount = normalized;
            }

            if (m_ValueText != null)
            {
                m_ValueText.text = Mathf.Clamp(Mathf.RoundToInt(health), 0, 100) + "/100";
            }
        }

        private static Sprite CreateHeartSprite(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            const int samples = 360;
            Vector2[] points = new Vector2[samples + 1];
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;

            for (int i = 0; i <= samples; i++)
            {
                float t = Mathf.PI * 2f * i / samples;
                float x = 16f * Mathf.Sin(t) * Mathf.Sin(t) * Mathf.Sin(t);
                float y = 13f * Mathf.Cos(t) - 5f * Mathf.Cos(2f * t)
                        - 2f * Mathf.Cos(3f * t) - Mathf.Cos(4f * t);

                points[i] = new Vector2(x, y);
                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }

            const float padding = 0.06f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = Mathf.Lerp(minX - padding, maxX + padding, (x + 0.5f) / size);
                    float ny = Mathf.Lerp(maxY + padding, minY - padding, (y + 0.5f) / size);
                    Vector2 p = new Vector2(nx, ny);

                    bool inside = PointInPolygon(p, points);
                    bool outline = !inside && PointNearPolygon(p, points, 0.75f);

                    if (inside)
                        pixels[y * size + x] = new Color(0.88f, 0.08f, 0.12f, 1f);
                    else if (outline)
                        pixels[y * size + x] = new Color(0.08f, 0.035f, 0.04f, 1f);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static bool PointInPolygon(Vector2 p, Vector2[] polygon)
        {
            bool inside = false;
            int j = polygon.Length - 1;

            for (int i = 0; i < polygon.Length; i++)
            {
                Vector2 a = polygon[i];
                Vector2 b = polygon[j];

                if (((a.y > p.y) != (b.y > p.y)) &&
                    (p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y + 0.00001f) + a.x))
                {
                    inside = !inside;
                }

                j = i;
            }

            return inside;
        }

        private static bool PointNearPolygon(Vector2 p, Vector2[] polygon, float distance)
        {
            float best = float.MaxValue;

            for (int i = 0; i < polygon.Length - 1; i++)
            {
                Vector2 a = polygon[i];
                Vector2 b = polygon[i + 1];
                Vector2 ab = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 0.00001f));
                Vector2 closest = a + ab * t;
                best = Mathf.Min(best, Vector2.Distance(p, closest));
            }

            return best <= distance;
        }
    }
}
