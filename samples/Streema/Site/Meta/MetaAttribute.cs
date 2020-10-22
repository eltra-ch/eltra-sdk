using System;
using System.Collections.Generic;

namespace StreemaMaster.Site.Meta
{
    class MetaAttribute
    {
        private static readonly string[] MetaAttributeNames = { "charset", "http-equiv", "name", "scheme", "content" };

        private static bool AttributeExists(List<string> skipAttributes, string attributeName)
        {
            bool result = false;

            foreach (var skipAttribute in skipAttributes)
            {
                if (skipAttribute == attributeName)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        private static int FindNextAttributeIndex(string metaNode, List<string> skipAttributes, int offset)
        {
            int result = 0;
            string metaAfterAttribute = metaNode.Substring(offset);

            foreach (var attributeName in MetaAttributeNames)
            {
                int i = metaAfterAttribute.IndexOf($"{attributeName}=", StringComparison.OrdinalIgnoreCase);

                if (!AttributeExists(skipAttributes, attributeName) && i > 0)
                {
                    result = i + offset;
                    break;
                }
            }

            if (result == 0)
            {
                result = metaAfterAttribute.IndexOf("/>", StringComparison.OrdinalIgnoreCase) + offset;
            }

            return result;
        }

        public static string TryFix(string metaNode)
        {
            string result = metaNode;

            try
            {
                List<string> skipAttributes = new List<string>();

                foreach (var attributeName in MetaAttributeNames)
                {
                    int index = metaNode.IndexOf($"{attributeName}=", StringComparison.Ordinal);

                    if (index > 0 && index + attributeName.Length < metaNode.Length)
                    {
                        if (metaNode[index + attributeName.Length + 1] != '\"' && metaNode[index + attributeName.Length + 1] != '\'')
                        {
                            metaNode = metaNode.Insert(index + attributeName.Length + 1, "\"");

                            skipAttributes.Add(attributeName);

                            int nextAttributeIndex = FindNextAttributeIndex(metaNode, skipAttributes, index + attributeName.Length);

                            if (metaNode.Length > nextAttributeIndex - 1)
                            {
                                metaNode = metaNode.Insert(nextAttributeIndex - 1, "\" ");

                                result = TryFix(metaNode);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return result;
        }
    }
}
