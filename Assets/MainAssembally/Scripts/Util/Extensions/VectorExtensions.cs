using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
    public struct VectorComponent
    {
        readonly bool invert;
        readonly char character;

        public VectorComponent(bool invert, char character)
        {
            if (character == 'x' || character == 'y' || character == 'z' || character == 'w')
            {
                this.invert = invert;
                this.character = character;
            }
            else
            {
                this.invert = false;
                this.character = '0';
            }
        }

        public float GetValue(Vector2 v)
        {
            float sign = invert ? -1f : 1f;
            switch (character)
            {
                case 'x':
                    return sign * v.x;
                case 'y':
                    return sign * v.y;
                default:
                    return 0;
            }
        }

        public float GetValue(Vector3 v)
        {
            float sign = invert ? -1f : 1f;
            switch (character)
            {
                case 'x':
                    return sign * v.x;
                case 'y':
                    return sign * v.y;
                case 'z':
                    return sign * v.z;
                default:
                    return 0;
            }
        }

        public float GetValue(Vector4 v)
        {
            float sign = invert ? -1f : 1f;
            switch (character)
            {
                case 'x':
                    return sign * v.x;
                case 'y':
                    return sign * v.y;
                case 'z':
                    return sign * v.z;
                case 'w':
                    return sign * v.w;
                default:
                    return 0;
            }
        }

        private static (VectorComponent v, string s)? ParseOne(string s)
        {
            if (s == null)
            {
                return null;
            }
            if (s.Length == 0)
            {
                return null;
            }
            if (s[0] == '-')
            {
                if (s.Length >= 2)
                {
                    return (new VectorComponent(true, s[1]), s.Substring(2));
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return (new VectorComponent(false, s[0]), s.Substring(1));
            }
        }

        public static VectorComponent[] Parse(string s, int expectedLength)
        {
            VectorComponent[] output = new VectorComponent[expectedLength];
            (VectorComponent c, string s)? part = ParseOne(s);
            for (int i = 0; i < expectedLength; i++)
            {
                if (part == null)
                {
                    output[i] = new VectorComponent(false, '0');
                }
                else
                {
                    output[i] = part.Value.c;
                    part = ParseOne(part.Value.s);
                }
            }
            return output;
        }

        public override bool Equals(object obj)
        {
            if (obj is VectorComponent comp)
            {
                return this.character == comp.character && this.invert == comp.invert;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            if (invert)
            {
                return character.GetHashCode();
            }
            else
            {
                return ~character.GetHashCode();
            }
        }
    }

    public static Vector3 ToVector3(this Vector2 v, string conversion)
    {
        Vector3 output = Vector3.zero;
        VectorComponent[] comps = VectorComponent.Parse(conversion, 3);
        for (int i = 0; i < 3; i++)
        {
            output[i] = comps[i].GetValue(v);
        }
        return output;
    }
}