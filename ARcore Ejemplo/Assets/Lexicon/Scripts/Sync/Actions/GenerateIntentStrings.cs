// Copyright (c) 2018 Mixspace Technologies, LLC. All rights reserved.

#if UNITY_EDITOR

using System;
using System.IO;
using System.Text;
using UnityEditor;

namespace Mixspace.Lexicon
{
    [Serializable]
    public class GenerateIntentStrings : SyncAction
    {
        private LexiconIntent intent;

        public static GenerateIntentStrings CreateInstance(LexiconIntent intent)
        {
            GenerateIntentStrings instance = CreateInstance<GenerateIntentStrings>();
            instance.intent = intent;
            return instance;
        }

        public override void Process(SyncQueue queue)
        {
            if (intent == null)
            {
                // Intent was deleted.
                succeeded = false;
                isDone = true;
                return;
            }

            string path = AssetDatabase.GetAssetPath(intent);
            string directory = Path.GetDirectoryName(path);
            string filename = StringUtility.ToCamelCase(intent.ActionName) + "Strings";
            string stringsScript = StringsScript();

            CreateScript(directory, filename, stringsScript);

            AssetDatabase.Refresh();

            succeeded = true;
            isDone = true;
        }

        private string StringsScript()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("namespace " + intent.NamespaceString);
            builder.AppendLine("{");
            builder.AppendLine("    public class " + StringUtility.ToCamelCase(intent.ActionName) + "Strings");
            builder.AppendLine("    {");

            foreach (LexiconEntity entity in intent.GetAllEntities())
            {
                if (entity != null)
                {
                    builder.AppendLine("        " + "public const string " + StringUtility.ToCamelCase(entity.EntityName) + " = \"" + entity.EntityName + "\";");
                }
            }

            foreach (LexiconEntity entity in intent.GetAllEntities())
            {
                if (entity != null && entity.Values != null && entity.Values.Length > 0)
                {
                    builder.AppendLine("        ");
                    builder.AppendLine("        public class " + StringUtility.ToCamelCase(entity.EntityName) + "Values");
                    builder.AppendLine("        {");

                    foreach (LexiconEntityValue entityValue in entity.Values)
                    {
                        builder.AppendLine("            " + "public const string " + StringUtility.ToCamelCase(entityValue.ValueName) + " = \"" + entityValue.ValueName + "\";");
                    }
                    builder.AppendLine("        }");
                }
            }

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
