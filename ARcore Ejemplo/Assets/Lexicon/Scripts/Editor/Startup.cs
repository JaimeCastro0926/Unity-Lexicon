// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mixspace.Lexicon
{
    [InitializeOnLoad]
    public class Startup
    {
        static Startup()
        {
            //Debug.Log("Startup");
            EditorManager instance = EditorManager.Instance;
        }
    }
}
