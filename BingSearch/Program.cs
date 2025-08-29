using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;
using System.IO;

class Program
{
    public static async Task Main()
    {
        // Charger config
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        IConfiguration config = builder.Build();
        string UserAccount = config["User:Account"] ?? throw new InvalidOperationException("User:Account manquant dans appsettings.json");
        string UserPass = config["User:Pass"] ?? throw new InvalidOperationException("User:Pass manquant dans appsettings.json");

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 250,
            Args = new[] { "--start-maximized" }
        });

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36"
        });

        var page = await context.NewPageAsync();

        // Aller sur Bing
        Console.WriteLine("Go to Bing");
        await page.GotoAsync("https://www.bing.com");

        // Accepter les cookies si présent
        var acceptBtn = await page.QuerySelectorAsync("#bnp_btn_accept a");
        if (acceptBtn != null && await acceptBtn.IsVisibleAsync())
        {
            await acceptBtn.ClickAsync();
            Console.WriteLine("✅ Cookies acceptés !");
        }

        // Aller sur la page login
        Console.WriteLine("Go to Bing login");
        await page.GotoAsync("https://login.live.com/");

        // Username
        var usernameInput = await page.WaitForSelectorAsync("input[id='usernameEntry']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
        if (usernameInput != null)
        {
            await usernameInput.FillAsync(UserAccount);
            Console.WriteLine("✅ Username rempli");

            var nextBtn = await page.QuerySelectorAsync("button[data-testid='primaryButton']");
            if (nextBtn != null && await nextBtn.IsVisibleAsync())
            {
                await nextBtn.ClickAsync();
                Console.WriteLine("✅ Click Next");
            }
        }

        // Password
        var passwordInput = await page.WaitForSelectorAsync("input[name='passwd']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
        if (passwordInput != null)
        {
            await passwordInput.FillAsync(UserPass);
            Console.WriteLine("✅ Password rempli");

            // Essayer de récupérer le bouton Enter/Sign in
            var signInBtn = await page.QuerySelectorAsync("input[name='passwd'] ~ button[data-testid='primaryButton']")
                            ?? await page.QuerySelectorAsync("button[data-testid='primaryButton']");

            if (signInBtn != null && await signInBtn.IsVisibleAsync())
            {
                await signInBtn.ClickAsync();
                Console.WriteLine("✅ Click Sign In");
            }
        }

        // Bouton "Oui" (Stay signed in)
        try
        {
            var staySignedBtn = await page.WaitForSelectorAsync("button[data-testid='primaryButton']", new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });

            if (staySignedBtn != null && await staySignedBtn.IsVisibleAsync())
            {
                await staySignedBtn.ClickAsync();
                Console.WriteLine("✅ Click 'Oui' (Stay signed in)");
            }
        }
        catch (TimeoutException)
        {
            Console.WriteLine("⚠️ Bouton 'Oui' non apparu après 5s, on continue");
        }

        // Retour sur Bing
        Console.WriteLine("Return to Bing");
        await page.GotoAsync("https://www.bing.com");
        await page.WaitForTimeoutAsync(3000);

        // Recherche
        var searchBox = await page.WaitForSelectorAsync("textarea#sb_form_q", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
        if (searchBox != null && await searchBox.IsVisibleAsync())
        {
            await searchBox.FillAsync("test 1");
            await searchBox.PressAsync("Enter");
            Console.WriteLine("✅ Recherche lancée");
        }

        // Accepter cookies si besoin
        var acceptCookie = await page.QuerySelectorAsync("#bnp_btn_accept a");
        if (acceptCookie != null && await acceptCookie.IsVisibleAsync())
        {
            await acceptCookie.ClickAsync();
            Console.WriteLine("✅ Cookies acceptés !");
        }

        Console.WriteLine("✅ Script terminé");
    }
}
