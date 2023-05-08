using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading;

namespace ImageScraper.Helpers
{
    public class TinsoftHelper
    {
        public string api_key { get; set; }
        public string proxy { get; set; }
        public string ip { get; set; }
        public int port { get; set; }
        public int timeout { get; set; }
        public int next_change { get; set; }
        public string errorCode = "";
        public int location { get; set; }
        private string svUrl = "http://proxy.tinsoftsv.com";
        private long lastRequest = 0;
        public string api_key_status = "";

        public TinsoftHelper()
        {
            this.api_key = "";
            this.proxy = "";
            this.ip = "";
            this.port = 0;
            this.timeout = 0;
            this.next_change = 0;
            this.location = 0;
        }

        public bool CheckApiKeyStatus()
        {
            WaitUntilLastRequestIsTrue();
            string res = GetSVContent("http://proxy.tinsoftsv.com/api/getKeyInfo.php?key=" + api_key);
            JObject rsObject = JObject.Parse(res);
            if (bool.Parse(rsObject["success"].ToString()))
            {
                return true;
            }
            api_key_status = rsObject["description"].ToString();
            return false;
        }

        private int GetRandomLocation()
        {
            return new Random().Next(0, 25);
        }

        public string GetTinsoftIp(bool isGetNew = false)
        {
            if (CheckApiKeyStatus())
            {
                this.api_key = api_key;
                this.location = GetRandomLocation();
                string proxyAddress = "";
                if (isGetNew)
                {
                    ChangeProxy();
                    if (this.proxy == "")
                    {
                        GetProxyStatus();
                    }
                }
                else if (!GetProxyStatus())
                {
                    ChangeProxy();
                }
                proxyAddress = this.proxy;
                return proxyAddress;
            }
            return "Invalid Api Key";
        }

        public bool WaitUntilLastRequestIsTrue()
        {
            while (!CheckLastRequest())
            {
                Thread.Sleep(1000);
            }
            return true;
        }

        public bool ChangeProxy()
        {
            if (WaitUntilLastRequestIsTrue())
            {
                errorCode = "";
                this.next_change = 0;
                this.proxy = "";
                this.ip = "";
                this.port = 0;
                this.timeout = 0;
                string rs = GetSVContent(svUrl + "/api/changeProxy.php?key=" + this.api_key + "&location=" + this.location);
                if (rs != "")
                {
                    try
                    {
                        JObject rsObject = JObject.Parse(rs);
                        if (bool.Parse(rsObject["success"].ToString()))
                        {
                            this.proxy = rsObject["proxy"].ToString();
                            string[] proxyArr = this.proxy.Split(':');
                            this.ip = proxyArr[0];
                            this.port = int.Parse(proxyArr[1]);
                            this.timeout = int.Parse(rsObject["timeout"].ToString());
                            this.next_change = int.Parse(rsObject["next_change"].ToString());
                            this.errorCode = "";
                            return true;
                        }
                        else
                        {
                            this.errorCode = rsObject["description"].ToString();
                        }
                    }
                    catch { }
                }
                else
                {
                    this.errorCode = "request server timeout!";
                }
            }
            else
            {
                errorCode = "Request so fast!";
            }
            return false;
        }

        public void StopProxy()
        {
            errorCode = "";
            this.proxy = "";
            this.ip = "";
            this.port = 0;
            this.timeout = 0;
            if (this.api_key != "")
            {
                GetSVContent(svUrl + "/api/stopProxy.php?key=" + this.api_key);
            }
        }

        public bool GetProxyStatus()
        {
            if (WaitUntilLastRequestIsTrue())
            {
                errorCode = "";
                this.proxy = "";
                this.ip = "";
                this.port = 0;
                this.timeout = 0;
                string rs = GetSVContent(svUrl + "/api/getProxy.php?key=" + this.api_key);
                if (rs != "")
                {
                    try
                    {
                        JObject rsObject = JObject.Parse(rs);
                        if (bool.Parse(rsObject["success"].ToString()))
                        {
                            this.proxy = rsObject["proxy"].ToString();
                            string[] proxyArr = this.proxy.Split(':');
                            this.ip = proxyArr[0];
                            this.port = int.Parse(proxyArr[1]);
                            this.timeout = int.Parse(rsObject["timeout"].ToString());
                            this.next_change = int.Parse(rsObject["next_change"].ToString());
                            this.errorCode = "";
                            if (timeout < 0)
                            {
                                return false;
                            }
                            return true;
                        }
                        else
                        {
                            this.errorCode = rsObject["description"].ToString();
                        }
                    }
                    catch { }
                }
            }
            else
            {
                errorCode = "Request so fast!";
            }

            return false;
        }

        public bool CheckLastRequest()
        {
            try
            {
                DateTimeOffset currentTime = DateTimeOffset.Now;
                long unixTime = currentTime.ToUnixTimeSeconds();

                if ((unixTime - lastRequest) >= 6)
                {
                    lastRequest = unixTime;

                    return true;
                }
            }
            catch { }

            return false;
        }

        private string GetSVContent(string url)
        {
            Console.WriteLine(url);
            string rs = "";
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; " +
                                  "Windows NT 5.2; .NET CLR 1.0.3705;)");
                    rs = client.DownloadString(url);
                }
                if (string.IsNullOrEmpty(rs)) { rs = ""; }
            }
            catch { rs = ""; }
            return rs;
        }
    }
}