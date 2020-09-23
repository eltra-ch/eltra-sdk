using System;
using System.Web;
using System.Xml;
using StreemaMaster.Helpers;

namespace StreemaMaster.Site.Meta
{
    public class MetaTag
    {
        #region Private fields

        private string _charset = string.Empty;
        
        #endregion

        #region Properties

        public string Name { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
        
        public string CharSet
        {
            get => _charset.CharsetFix();
            set => _charset = value;
        }
        public string HttpEquiv { get; set; } = string.Empty;
        public string Scheme { get; set; } = string.Empty;
        public string Property { get; set; } = string.Empty;

        #endregion

        #region Methods

        public bool Parse(string html)
        {
            bool result = false;

            try
            {
                result = XmlMetaParse(html);

                if (!result)
                {
                    result = FallbackMetaParse(html);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return result;
        }
        
        private bool FallbackMetaParse(string html)
        {
            bool result = false;

            var fallbackParser = new MetaTagFallbackParser();

            fallbackParser.Parse(html);

            foreach (var pair in fallbackParser.KeyValuePairs)
            {
                if (string.Compare(pair.Key, "name", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Name = pair.Value;
                }
                else if (string.Compare(pair.Key, "content", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Content = pair.Value.Trim();

                    if (!string.IsNullOrEmpty(Name) || !string.IsNullOrEmpty(HttpEquiv) || !string.IsNullOrEmpty(Property))
                    {
                        result = true;
                    }
                }
                else if (string.Compare(pair.Key, "charset", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    CharSet = pair.Value;
                    result = true;
                }
                else if (string.Compare(pair.Key, "http-equiv", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    HttpEquiv = pair.Value;
                }
                else if (string.Compare(pair.Key, "scheme", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Scheme = pair.Value;
                    result = true;
                }
                else if (string.Compare(pair.Key, "property", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Property = pair.Value;
                }
            }

            return result;
        }

        private bool XmlMetaParse(string html)
        {
            bool result = false;
            var doc = new XmlDocument();

            string processingHtml;

            try
            {
                doc.LoadXml(html);

                processingHtml = html;
            }
            catch (XmlException)
            {
                processingHtml = TryFixMalformedHtml(html);
            }
            catch (Exception)
            {
                processingHtml = TryFixMalformedHtml(html);
            }

            try
            {
                doc.LoadXml(processingHtml);

                var metaXmlNode = doc.SelectSingleNode("meta");

                var metaAttributes = metaXmlNode?.Attributes;

                if (metaAttributes != null)
                {
                    var nameAttribute = metaAttributes["name"];
                    var contentAttribute = metaAttributes["content"];
                    var charsetAttribute = metaAttributes["charset"];
                    var httpEquivAttribute = metaAttributes["http-equiv"];
                    var schemeAttribute = metaAttributes["scheme"];
                    var propertyAttribute = metaAttributes["property"];

                    if (schemeAttribute != null)
                    {
                        Scheme = schemeAttribute.InnerText;
                        result = true;
                    }

                    if (propertyAttribute != null)
                    {
                        Property = propertyAttribute.InnerText;
                    }

                    if (httpEquivAttribute != null)
                    {
                        HttpEquiv = httpEquivAttribute.InnerText;
                    }

                    if (nameAttribute != null)
                    {
                        Name = nameAttribute.InnerText;
                    }

                    if (contentAttribute != null)
                    {
                        Content = contentAttribute.InnerText.Trim();

                        if (!string.IsNullOrEmpty(Name) || !string.IsNullOrEmpty(HttpEquiv) || !string.IsNullOrEmpty(Property))
                        {
                            result = true;
                        }
                    }

                    if (charsetAttribute != null)
                    {
                        CharSet = charsetAttribute.InnerText.CharsetFix();
                        result = true;
                    }
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        private static string TryFixMalformedHtml(string html)
        {
            var modifiedHtml = Entity.RemoveAllEntities(html);

            modifiedHtml = MetaAttribute.TryFix(modifiedHtml);

            modifiedHtml = modifiedHtml.Replace("&", " ");
            modifiedHtml = modifiedHtml.Replace("#", "");

            return modifiedHtml;
        }

        public bool Equals(MetaTag source)
        {
            bool result = source?.Name == Name &&
                source?.Content == Content &&
                source?.CharSet == CharSet &&
                source?.HttpEquiv == HttpEquiv &&
                source?.Property == Property && 
                source?.Scheme == Scheme;

            return result;
        }

        public string GetKey()
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(Name))
            {
                result = Name;
            }
            else if(!string.IsNullOrEmpty(CharSet))
            {
                result = "charset";
            }
            else if (!string.IsNullOrEmpty(HttpEquiv))
            {
                result = HttpEquiv;
            }
            
            return result;
        }

        public string GetValue()
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(Name))
            {
                result = HttpUtility.HtmlDecode(Content);
            }
            else if (!string.IsNullOrEmpty(HttpEquiv))
            {
                result = HttpUtility.HtmlDecode(Content);
            }
            else if (!string.IsNullOrEmpty(CharSet))
            {
                result = HttpUtility.HtmlDecode(CharSet);

                result = result.CharsetFix();
            }

            return result;
        }

        #endregion


    }
}
