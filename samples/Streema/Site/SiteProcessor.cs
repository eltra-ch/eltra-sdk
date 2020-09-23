using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using StreemaMaster.Helpers;
using StreemaMaster.Site.Meta;

namespace StreemaMaster.Site
{
    public class SiteProcessor
    {
        #region

        private const string Agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:80.0) Gecko/20100101 Firefox/80.0";

        #endregion

        #region Private fields

        private string _userAgent;
        private string _url;

        #endregion

        #region Constructors

        public SiteProcessor(string url)
        {
            _url = url;
            _userAgent = Agent;

            MetaTagList = new List<MetaTag>();
        }

        #endregion

        #region Properties

        public List<MetaTag> MetaTagList { get; set; }

        public string UserAgent
        {
            get => _userAgent;
            set => _userAgent = value;
        }

        #endregion

        #region Methods
        
        private Stream OpenUrlRead(string url, ref bool secure)
        {
            NodeWebClient client = new NodeWebClient();
            client.Headers.Add("user-agent", UserAgent);
            Stream data = null;

            try
            {
                string targetUrl = url;

                if (!targetUrl.StartsWith("http://") && !targetUrl.StartsWith("https://"))
                {
                    if (secure)
                    {
                        targetUrl = $"https://{url}";
                    }
                    else
                    {
                        targetUrl = $"http://{url}";
                    }
                }

                data = client.OpenRead(targetUrl);
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (NotSupportedException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (data == null && secure)
            {
                secure = false;

                data = OpenUrlRead(url, ref secure);
            }

            return data;
        }
                
        public bool Parse()
        {
            bool result = true;
            string previousBlock = string.Empty;
            
            MetaTagList.Clear();
            
            try
            {
                bool secure = false;
                Stream data = OpenUrlRead(_url, ref secure);

                var reader = new StreamReader(data);
                Encoding pageEncoding = Encoding.UTF8;

                while (reader.Peek() >= 0)
                {
                    char[] buffer = new char[1024];

                    if (reader.Read(buffer, 0, buffer.Length) > 0)
                    {
                        var currentBlock = new string(buffer);
                        var dataBlock = HttpUtility.HtmlDecode(previousBlock + currentBlock);

                        ParseMeta(dataBlock, ref pageEncoding);

                        ParseTitle(dataBlock, pageEncoding);
                        
                        int lastClosingTag = currentBlock.LastIndexOf(">", StringComparison.Ordinal);

                        if (lastClosingTag > 0 && currentBlock.Length > lastClosingTag + 1)
                        {
                            previousBlock = currentBlock.Substring(lastClosingTag + 1);
                        }
                        else
                        {
                            previousBlock = currentBlock;
                        }
                    }
                }

                reader.Close();
            }
            catch (Exception e)
            {
                result = false;

                Console.WriteLine(e.Message);
            }
            
            return result;
        }

        private void ParseTitle(string html, Encoding encoding)
        {
            try
            {
                const string titleStartTag = "<title";
                var titleStartIndex = html.IndexOf(titleStartTag, 0, StringComparison.OrdinalIgnoreCase);

                if (titleStartIndex > 0 && html.Length > titleStartIndex + titleStartTag.Length + 1)
                {
                    var titleStopIndex = html.IndexOf(">", titleStartIndex + titleStartTag.Length + 1, StringComparison.OrdinalIgnoreCase);

                    int titleContentStart = titleStartIndex + titleStartTag.Length + 1;
                    int titleContentLength = titleStopIndex - titleStartIndex - 2 * titleStartTag.Length - 2;

                    if (titleStopIndex > 0 && html.Length > titleContentStart + titleContentLength)
                    {
                        string titleNode = html.Substring(titleContentStart, titleContentLength);
                            
                        if(!string.IsNullOrEmpty(titleNode))
                        { 
                            var metaTag = new MetaTag
                            {
                                Name = "html:title",
                                Content = titleNode.Trim().ConvertToUtf8(encoding)
                            };
                                
                            if (!MetaTagExists(metaTag))
                            {
                                MetaTagList.Add(metaTag);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void ParseMeta(string html, ref Encoding encoding)
        {
            try
            {
                const string metaStartTag = "<meta";
               
                if (html.IndexOf(metaStartTag,0, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    int index = 0;
                    int metaStartIndex;

                    do
                    {
                        if (html.Length > index)
                        {
                            metaStartIndex = html.IndexOf(metaStartTag, index, StringComparison.OrdinalIgnoreCase);

                            if (metaStartIndex > 0 && html.Length > metaStartIndex + metaStartTag.Length + 1)
                            {
                                var metaStopIndex = html.IndexOf(">", metaStartIndex + metaStartTag.Length + 1, StringComparison.OrdinalIgnoreCase);

                                if (metaStopIndex > 0 && html.Length > (metaStopIndex + 1))
                                {
                                    string metaNode = html.Substring(metaStartIndex,
                                        metaStopIndex - metaStartIndex + 1);

                                    if (metaNode[metaNode.Length - 2] != '/')
                                    {
                                        metaNode = metaNode.Insert(metaNode.Length - 1, "/");
                                    }

                                    var metaTag = new MetaTag();

                                    if (metaTag.Parse(metaNode))
                                    {
                                        if (!MetaTagExists(metaTag))
                                        {
                                            if (DetectEncoding(metaTag, out var metaTagencoding))
                                            {
                                                encoding = metaTagencoding;
                                            }

                                            metaTag.Content = metaTag.Content.ConvertToUtf8(encoding);

                                            MetaTagList.Add(metaTag);
                                        }
                                    }

                                    index = metaStartIndex + metaNode.Length;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }

                    } while (metaStartIndex > 0);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private bool DetectEncoding(MetaTag metaTag, out Encoding encoding)
        {
            bool result = false;

            encoding = Encoding.UTF8;
            
            try
            {
                if (!string.IsNullOrEmpty(metaTag.CharSet) &&
                    string.Compare(metaTag.CharSet, "utf-8", StringComparison.OrdinalIgnoreCase) != 0)
                {
                    if (!metaTag.CharSet.Contains("ml"))
                    {
                        encoding = Encoding.GetEncoding(metaTag.CharSet);
                        result = true;
                    }
                }

                if (string.Compare(metaTag.HttpEquiv, "content-type", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    string charsetKeyword = "charset=";
                    var idx = metaTag.Content.IndexOf(charsetKeyword, StringComparison.OrdinalIgnoreCase);

                    if (metaTag.Content.Length > idx + charsetKeyword.Length)
                    {
                        var charset = metaTag.Content.Substring(idx + charsetKeyword.Length).Trim();

                        if (!charset.Contains("ml;"))
                        {
                            if (string.Compare(charset, "utf-8", StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                encoding = Encoding.GetEncoding(charset);
                                result = true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("CharSet -> Encoding", e);
            }
            
            return result;
        }

        private bool MetaTagExists(MetaTag metaTag)
        {
            bool result = false;

            foreach (var meta in MetaTagList)
            {
                if (meta.Equals(metaTag))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        [SuppressMessage("ReSharper", "UnusedVariable")]
        public static bool IsUrlValid(string url)
        {
            bool result = true;

            if (!(url.StartsWith("http://") || url.StartsWith("https://")))
            {
                url = $"http://{url}";
            }

            try
            {
                Uri uri = new Uri(url);

                string host = uri.Host;
            }
            catch (UriFormatException)
            {
                result = false;
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public static byte[] DownloadUrlData(string userAgent, string url, ref bool secure)
        {
            var client = new WebClient();
            client.Headers.Add("user-agent", userAgent);
            byte[] data = null;

            try
            {
                string targetUrl = url;

                if (!targetUrl.StartsWith("http://") && !targetUrl.StartsWith("https://"))
                {
                    if (secure)
                    {
                        targetUrl = $"https://{url}";
                    }
                    else
                    {
                        targetUrl = $"http://{url}";
                    }
                }

                data = client.DownloadData(targetUrl);
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (NotSupportedException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (data == null && secure)
            {
                secure = false;

                data = DownloadUrlData(userAgent, url, ref secure);
            }

            return data;
        }

        public MetaTag FindMetaTagByPropertyName(string propertyName)
        {
            MetaTag result = null;

            foreach (var metaTag in MetaTagList)
            {
                if(metaTag.Property == propertyName)
                {
                    result = metaTag;
                    break;
                }
            }

            return result;
        }

        #endregion
    }
}
