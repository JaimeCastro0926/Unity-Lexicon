// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Globalization;
using UnityEngine;

namespace Mixspace.Lexicon
{
    public class StringUtility : MonoBehaviour
    {
        public static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }

            s = s.Replace("-", " ");
            s = s.Replace("_", " ");

            string[] words = s.Split(' ');
            string final = "";

            foreach (string word in words)
            {
                final += char.ToUpper(word[0]);
                final += word.Substring(1);
            }

            return final;
        }

        public static string Base64Encode(string plainText)
        {
            if (plainText == null)
            {
                return "";
            }

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            if (base64EncodedData == null)
            {
                return "";
            }

            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
