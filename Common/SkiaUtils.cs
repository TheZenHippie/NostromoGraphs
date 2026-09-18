using System;
using SkiaSharp;

namespace NostromoGraphs.Common
{
    public struct Vector3D
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3D RotateY(float angle)
        {
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            return new Vector3D(X * cos + Z * sin, Y, -X * sin + Z * cos);
        }

        public Vector3D RotateX(float angle)
        {
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            return new Vector3D(X, Y * cos - Z * sin, Y * sin + Z * cos);
        }

        public Vector3D RotateZ(float angle)
        {
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            return new Vector3D(X * cos - Y * sin, X * sin + Y * cos, Z);
        }

        public SKPoint Project(float width, float height, float fov = 400f, float cameraDistance = 300f)
        {
            float zDist = Z + cameraDistance;
            if (zDist < 1f) zDist = 1f;
            float factor = fov / zDist;
            return new SKPoint(width / 2f + X * factor, height / 2f + Y * factor);
        }
    }

    public static class SkiaUtils
    {
        private static SKTypeface? _monoTypeface;

        public static SKTypeface MonospaceTypeface
        {
            get
            {
                if (_monoTypeface == null)
                {
                    // Try typical monospace fonts
                    string[] fontCandidates = { "Cascadia Code", "Consolas", "Lucida Console", "Courier New" };
                    foreach (var font in fontCandidates)
                    {
                        var tf = SKTypeface.FromFamilyName(font);
                        if (tf != null && tf.FamilyName.Equals(font, StringComparison.OrdinalIgnoreCase))
                        {
                            _monoTypeface = tf;
                            break;
                        }
                    }
                    _monoTypeface ??= SKTypeface.Default;
                }
                return _monoTypeface;
            }
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Math.Clamp(t, 0f, 1f);
        }

        public static float SmoothStep(float edge0, float edge1, float x)
        {
            float t = Math.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
            return t * t * (3f - 2f * t);
        }

        public static float PseudoNoise(float x, float y = 0f)
        {
            return MathF.Sin(x * 12.9898f + y * 78.233f) * 43758.5453f % 1.0f;
        }
    }
}

