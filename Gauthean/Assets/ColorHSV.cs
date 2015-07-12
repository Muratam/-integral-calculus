using UnityEngine;
using System.Collections;

using System;

namespace UnityEngine {
    /// <summary>
    /// HSV色空間を扱うクラス
    /// </summary>
    public static class ColorHSV {



        /// <summary>
        /// HSV色空間による指定でUnityEngine.Colorを作成します。
        /// </summary>
        /// <param name="h">色相(Hue) 0.0-360.0</param>
        /// <param name="s">彩度(Saturation) 0.0-1.0</param>
        /// <param name="v">明度(Value) 0.0-1.0</param>
        /// <returns></returns>
        public static Color FromHsv(float h, float s, float v) {
            while (h >= 360) h -= 360;
            while (h < 0) h += 360;
            if (s > 1) s = 1;
            if (s < 0) s = 0;
            if (v > 1) v = 1;
            if (v < 0) v = 0;

            Color resColor = Color.clear;

            if (s == 0.0) {//Gray
                int rgb = Convert.ToInt16((float)(v * 255));
                resColor = new Color(rgb, rgb, rgb);
            } else {
                int Hi = (int)(Mathf.Floor(h / 60.0f) % 6.0f);
                float f = (h / 60.0f) - Hi;

                float p = v * (1 - s);
                float q = v * (1 - f * s);
                float t = v * (1 - (1 - f) * s);

                float r = 0.0f;
                float g = 0.0f;
                float b = 0.0f;

                switch (Hi) {
                    case 0: r = v; g = t; b = p; break;
                    case 1: r = q; g = v; b = p; break;
                    case 2: r = p; g = v; b = t; break;
                    case 3: r = p; g = q; b = v; break;
                    case 4: r = t; g = p; b = v; break;
                    case 5: r = v; g = p; b = q; break;
                    default: break;
                }

                resColor = new Color(r, g, b);
            }

            return resColor;
        }
    }




    /// <summary>
    /// UnityEngine.ColorのHSV色空間への拡張
    /// </summary>
    public static class ColorExtension {
        /// <summary>
        /// 色相(Hue)
        /// 0-360
        /// </summary>
        public static int h(this Color c) {
            float min = Mathf.Min(new float[] { c.r, c.g, c.b });
            float max = Mathf.Max(new float[] { c.r, c.g, c.b });

            if (max == 0) return 0;

            float h = 0;

            if (max == c.r) h = 60 * (c.g - c.b) / (max - min) + 0;
            else if (max == c.g) h = 60 * (c.b - c.r) / (max - min) + 120;
            else if (max == c.b) h = 60 * (c.r - c.g) / (max - min) + 240;

            if (h < 0) h += 360;

            return (int)Mathf.Round(h);
        }

        /// <summary>
        /// 彩度(Saturation)
        /// 0-255
        /// </summary>
        public static int s(this Color c) {
            float min = Mathf.Min(new float[] { c.r, c.g, c.b });
            float max = Mathf.Max(new float[] { c.r, c.g, c.b });

            if (max == 0) return 0;
            return (int)(255 * (max - min) / max);
        }

        /// <summary>
        /// 明度(Value)
        /// 0-255
        /// </summary>
        public static int v(this Color c) {
            return (int)(255.0f * Mathf.Max(new float[] { c.r, c.g, c.b }));
        }


        /// <summary>
        /// 現在の色を基準にHSV色空間を移動します。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="offsetH">色相(Hue)オフセット値</param>
        /// <param name="offsetS">彩度(Saturation)オフセット値</param>
        /// <param name="offsetV">明度(Value)オフセット値</param>
        public static Color Offset(this Color c, int offsetH, int offsetS, int offsetV) {
            int newH = (int)(c.h() + offsetH);
            int newS = (int)(c.s() + offsetS);
            int newV = (int)(c.v() + offsetV);
            return ColorHSV.FromHsv(newH, newS, newV);
        }


        /// <summary>
        /// 現在の色を文字列として返します。
        /// </summary>
        public static string ToString2(this Color c) {
            return string.Format("R={0}, G={1}, B={2}, H={3}, S={4}, V={5}",
                                     new object[]{c.r,c.g,c.b,c.h(),c.s(),c.v()});
        }
    }
}