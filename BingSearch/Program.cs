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
        string UserAccount = config["User:Account"];
        string UserPass = config["User:Pass"];

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 500 // 👈 ralentit un peu chaque action pour plus de stabilité
        });

        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        // Aller sur Bing
        await page.GotoAsync("https://www.bing.com");

        // Attendre le bouton cookies
        var acceptBtn = await page.QuerySelectorAsync("#bnp_btn_accept a");
        if (acceptBtn != null)
        {
            await acceptBtn.ClickAsync();
            Console.WriteLine("✅ Cookies acceptés !");
        }

        // Aller sur la page de login
        await page.GotoAsync("https://login.live.com/");

        // Username
        await page.FillAsync("input[id='usernameEntry']", UserAccount);
        await page.PressAsync("button[data-testid='primaryButton']", "Enter");

        // Password
        await page.WaitForSelectorAsync("input[name='passwd']", new() { State = WaitForSelectorState.Visible });
        await page.FillAsync("input[name='passwd']", UserPass);
        await page.PressAsync("input[name='passwd']", "Enter");

        // Bouton "Oui" (Stay signed in)
        var submitBtn = await page.WaitForSelectorAsync("button[data-testid='primaryButton']", new() { State = WaitForSelectorState.Visible });
        await submitBtn.ClickAsync();

        // Retour sur Bing
        await page.GotoAsync("https://www.bing.com");
        await page.WaitForTimeoutAsync(5000);

        // Attendre la barre de recherche
        var searchBox2 = await page.WaitForSelectorAsync("textarea#sb_form_q", new() { State = WaitForSelectorState.Visible });

        // Recherche
        await searchBox2.FillAsync("test 1");
        await searchBox2.PressAsync("Enter");


        // Attendre le bouton cookies
        var acceptCookie = await page.QuerySelectorAsync("#bnp_btn_accept a");
        if (acceptCookie != null)
        {
            await acceptCookie.ClickAsync();
            Console.WriteLine("✅ Cookies acceptés !");
        }

        //IElementHandle? searchBox = null;
        //// Search 
        //if (searchBox == null)
        //    searchBox = await page.QuerySelectorAsync("input[name='q']");
        //if (searchBox != null)
        //{
        //    await searchBox.FillAsync("Test 1");
        //    await searchBox.PressAsync("Enter");
        //}
        //else
        //{
        //    Console.WriteLine("Champ de recherche introuvable !");
        //}

        Console.ReadLine();
    }
}
