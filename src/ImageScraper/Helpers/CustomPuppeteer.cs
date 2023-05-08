using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageScraper.Helpers
{
    public class CustomPuppeteer : IDisposable
    {
        private LaunchOptions launchOptions;
        private IBrowser browser;

        public CustomPuppeteer()
        {
            launchOptions = new LaunchOptions();
        }

        public void SetProxy(string proxyIp, int proxyPort)
        {
            launchOptions.Args = new string[]
            {
                $"--proxy-server={proxyIp}:{proxyPort}",
            };
        }

        public async Task LaunchBrowserAsync()
        {
            var argsList = new List<string>(launchOptions.Args);
            argsList.AddRange(new string[]
            {
                "--disable-gpu",
                "--disable-dev-shm-usage",
                "--disable-software-rasterizer",
                "--disable-setuid-sandbox",
                "--no-sandbox",
                "--disable-web-security"
            });
            browser = await Puppeteer.LaunchAsync(launchOptions);
        }

        public async Task CloseBrowserAsync()
        {
            await browser.CloseAsync();
        }

        public async Task ClosePageAsync(IPage page)
        {
            await page.CloseAsync();
        }

        public async Task<IPage> CreateNewPageAsync(string userAgent = "")
        {
            var page = await browser.NewPageAsync();
            if (!string.IsNullOrEmpty(userAgent))
            {
                await page.SetUserAgentAsync(userAgent);
            }
            await page.SetRequestInterceptionAsync(true);
            page.Request += async (sender, e) =>
            {
                var resourceType = e.Request.ResourceType;

                if (resourceType == ResourceType.Image || resourceType == ResourceType.Font || resourceType == ResourceType.StyleSheet)
                {
                    await e.Request.AbortAsync();
                }
                else
                {
                    await e.Request.ContinueAsync();
                }
            };
            return page;
        }

        public async Task NavigateToAsync(IPage page, string url)
        {
            if (page == null)
            {
                throw new InvalidOperationException("You must call InitializeAsync() before navigating.");
            }
            await page.GoToAsync(url);
        }

        public void Dispose()
        {
            browser?.Dispose();
        }
    }
}