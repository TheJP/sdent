using UnityEngine;

namespace Assets.Scripts.Utility
{
    public static class GUIHelper
    {
        private static Texture2D whiteTexture;
        public static Texture2D WhiteTexture
        {
            get
            {
                if (whiteTexture == null)
                {
                    whiteTexture = new Texture2D(1, 1);
                    whiteTexture.SetPixel(0, 0, Color.white);
                    whiteTexture.Apply();
                }

                return whiteTexture;
            }
        }

        public static void DrawScreenRect(Rect rect, Color color)
        {
            var oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, WhiteTexture);
            GUI.color = oldColor;
        }

        public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
        {
            // Top
            GUIHelper.DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            // Left
            GUIHelper.DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            // Right
            GUIHelper.DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
            // Bottom
            GUIHelper.DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        }

        public static GUIStyle ScaleStyle(float scaleFactor, GUIStyle baseStyle)
        {
            GUIStyle scaledStyle = new GUIStyle(baseStyle);
            scaledStyle.fixedWidth *= scaleFactor;
            scaledStyle.fixedHeight *= scaleFactor;
            scaledStyle.padding = new RectOffset
            {
                top = (int)(baseStyle.padding.top * scaleFactor),
                bottom = (int)(baseStyle.padding.bottom * scaleFactor),
                left = (int)(baseStyle.padding.left * scaleFactor),
                right = (int)(baseStyle.padding.right * scaleFactor)
            };
            scaledStyle.margin = new RectOffset
            {
                top = (int)(baseStyle.margin.top * scaleFactor),
                bottom = (int)(baseStyle.margin.bottom * scaleFactor),
                left = (int)(baseStyle.margin.left * scaleFactor),
                right = (int)(baseStyle.margin.right * scaleFactor)
            };

            scaledStyle.fontSize = (int)(baseStyle.fontSize * scaleFactor);

            return scaledStyle;
        }

        public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
        {
            // Move origin from bottom left to top left
            screenPosition1.y = Screen.height - screenPosition1.y;
            screenPosition2.y = Screen.height - screenPosition2.y;
            // Calculate corners
            var topLeft = Vector3.Min(screenPosition1, screenPosition2);
            var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
            // Create Rect
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }
    }
}

