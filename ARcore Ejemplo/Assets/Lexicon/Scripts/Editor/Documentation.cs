// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    public class Documentation : MonoBehaviour
    {
        [MenuItem("Lexicon/Documentation", false, 102)]
        static void OpenDocumentation()
        {
            Application.OpenURL("http://mixspace.tech/lexicon-documentation");
        }
    }
}
