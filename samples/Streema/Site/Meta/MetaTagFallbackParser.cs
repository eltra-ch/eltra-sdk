using System;
using System.Collections.Generic;
using System.Linq;

namespace StreemaMaster.Site.Meta
{
    class MetaTagFallbackParser
    {
        #region Private fields

        Dictionary<string, string> _keyValuePairs = new Dictionary<string, string>();

        #endregion

        #region Properties

        public Dictionary<string, string> KeyValuePairs
        {
            get
            {
                return _keyValuePairs ?? (_keyValuePairs = new Dictionary<string, string>());
            }
        }

        #endregion

        #region Methods

        public void Parse(String metaTagHtmlNode)
        {
            string metaTagName = "<meta";

            KeyValuePairs.Clear();

            int metaIdx = metaTagHtmlNode.IndexOf(metaTagName, StringComparison.OrdinalIgnoreCase);

            if (metaIdx > -1)
            {
                try
                {
                    ParseMetaTag(metaTagHtmlNode);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private void ParseMetaTag(string metaTagHtmlNode)
        {
            bool keyFound = false;
            bool valueFound = false;
            char[] metaArray = metaTagHtmlNode.ToArray();
            string lastKeyName = string.Empty;
            int lastValueIndex = 0;
            string lastValueName = string.Empty;

            for (int i = 0; i < metaArray.Length; i++)
            {
                char c = metaArray[i];

                if (!keyFound && c == '=')
                {
                    keyFound = SingleKeyProcessing(metaArray, i, out _, out lastKeyName);
                }

                if (keyFound && !valueFound && c == '=')
                {
                    valueFound = SingleValueProcessing(metaArray, i, lastKeyName, out lastValueIndex, out lastValueName);
                }

                if (valueFound && c == '=')
                {
                    var value = NormalizeValue(lastValueIndex, lastValueName, metaArray, lastKeyName, ref i);

                    if (!string.IsNullOrEmpty(value))
                    {
                        KeyValuePairs[lastKeyName] = value;
                    }

                    keyFound = false;
                    valueFound = false;
                }
            }
        }

        private string NormalizeValue(int lastValueIndex, string lastValueName, char[] metaArray, string lastKeyName, ref int startIndex)
        {
            string result = string.Empty;

            var lastEndingSpaceIndex = WalkBackToLastSpace(lastValueIndex + lastValueName.Length, metaArray);

            var parsedValueWithNextTag = KeyValuePairs[lastKeyName];

            int tagValueLength = (lastEndingSpaceIndex - lastValueIndex);

            if (tagValueLength >= 0 && parsedValueWithNextTag.Length > tagValueLength)
            {
                result = parsedValueWithNextTag.Substring(0, tagValueLength);
                result = RemoveQuotes(result);

                int offset = parsedValueWithNextTag.Length - result.Length;

                startIndex = startIndex + parsedValueWithNextTag.Length - offset;
            }

            return result;
        }

        private string RemoveQuotes(string text)
        {
            text = text.Trim();

            if (text.StartsWith("\"") || text.StartsWith("'") && text.Length > 1)
            {
                text = text.Substring(1);
            }

            if (text.EndsWith("\"") || text.EndsWith("'") && text.Length > 1)
            {
                text = text.Substring(0, text.Length - 1);
            }

            if (text.EndsWith("\"/") || text.EndsWith("'/") && text.Length > 2)
            {
                text = text.Substring(0, text.Length - 2);
            }

            return text;
        }

        private int WalkBackToLastSpace(int i, char[] metaArray)
        {
            for (int j = i; j >= 0; j--)
            {
                if (metaArray[j] == ' ')
                {
                    i = j;
                    break;
                }
            }

            return i;
        }

        private bool SingleValueProcessing(char[] metaArray, int i, string keyName, out int valueIndex, out string foundValue)
        {
            bool result = false;
            int j = i;

            valueIndex = 0;
            foundValue = string.Empty;

            if (metaArray[j] == '=')
            {
                j++;
            }

            string value = string.Empty;
            bool beginsWithQuotationMark = metaArray[j] == '\"' || metaArray[j] == '\'';
            bool endQuotationFound = false;
            bool ignore = false;
            for (int k = j; k < metaArray.Length; k++)
            {
                if (!ignore && beginsWithQuotationMark && !endQuotationFound)
                {
                    ignore = true;
                }
                else if (ignore && (metaArray[k] == '\'' || metaArray[k] == '\"'))
                {
                    ignore = false;
                    endQuotationFound = true;
                }

                if ((metaArray[k] != '=' && metaArray[k] != '>') || ignore)
                {
                    value += metaArray[k];
                }
                else
                {
                    valueIndex = k - value.Length;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(value))
            {
                value = RemoveQuotes(value);

                if (!string.IsNullOrEmpty(value))
                {
                    foundValue = value;

                    KeyValuePairs[keyName] = value;

                    result = true;
                }
            }

            return result;
        }

        public string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        private bool SingleKeyProcessing(char[] metaArray, int i, out int keyIndex, out string foundKey)
        {
            bool result = false;
            int j = i;

            keyIndex = 0;
            foundKey = string.Empty;

            if (metaArray[j] == '=')
            {
                j--;
            }

            string key = string.Empty;
            for (int k = j; k > 0; k--)
            {
                if (metaArray[k] != ' ')
                {
                    key += metaArray[k];
                }
                else
                {
                    key = Reverse(key);
                    keyIndex = k - key.Length;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(key))
            {
                key = RemoveQuotes(key);

                if (!string.IsNullOrEmpty(key))
                {
                    foundKey = key;
                    KeyValuePairs.Add(key, "");
                    result = true;
                }
            }

            return result;
        }

        #endregion
    }
}
