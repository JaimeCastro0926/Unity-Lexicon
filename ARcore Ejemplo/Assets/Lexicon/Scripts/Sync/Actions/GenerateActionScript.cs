// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using System.IO;
using System.Text;
using UnityEditor;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class GenerateActionScript : SyncAction
    {
        public LexiconIntent intent;

        public static GenerateActionScript CreateInstance(LexiconIntent intent)
        {
            GenerateActionScript instance = CreateInstance<GenerateActionScript>();
            instance.intent = intent;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            string normalizedName = StringUtility.ToCamelCase(intent.ActionName);

            string path = AssetDatabase.GetAssetPath(intent);
            string directory = Path.GetDirectoryName(path);
            string filename = normalizedName + "Action";
            string enumScript = ActionScript(normalizedName);

            CreateScript(directory, filename, enumScript);

            //AssetDatabase.Refresh();

            //AssetDatabase.ImportAsset(directory + "/" + filename + ".cs");
            //AssetDatabase.SaveAssets();

            succeeded = true;
            isDone = true;
        }

        private string ActionScript(string actionName)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("using System.Collections;");
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine("using Mixspace.Lexicon;");
            builder.AppendLine();
            builder.AppendLine("using Strings = " + intent.NamespaceString + "." + actionName + "Strings;");
            builder.AppendLine();

            builder.AppendLine("namespace " + intent.NamespaceString);
            builder.AppendLine("{");
            builder.AppendLine("    public class " + actionName + "Action" + " : LexiconAction");
            builder.AppendLine("    {");
            builder.AppendLine("        public override bool Process(LexiconRuntimeResult runtimeResult)");
            builder.AppendLine("        {");
            builder.AppendLine("            // LexiconEntityMatch match = runtimeResult.GetEntityMatch(Strings.EntityName);");
            builder.AppendLine("            ");
            builder.AppendLine("            return false;");
            builder.AppendLine("        }");
            builder.AppendLine("    }");
            builder.AppendLine("}");

            return builder.ToString();
        }

        protected void CreateScript(string folder, string filename, string content)
        {
            string path = folder + "/" + filename + ".cs";
            using (StreamWriter writer = File.CreateText(path))
            {
                //Debug.Log("creating script: " + path);
                writer.Write(content);
            }
        }
    }
}

#endif
