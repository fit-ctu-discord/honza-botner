using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HonzaBotner.Services.Contract;
using HonzaBotner.Services.Contract.Dto;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;

namespace HonzaBotner.Services;

public class SuzCanteenService: ICanteenService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;

    public SuzCanteenService(HttpClient httpClient, IMemoryCache memoryCache)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
    }

    public async Task<IList<CanteenDto>> ListCanteensAsync(bool onlyOpen = false, CancellationToken cancellationToken = default)
    {
        const string cacheKey = $"{nameof(SuzCanteenService)}_{nameof(ListCanteensAsync)}";

        if (!_memoryCache.TryGetValue(cacheKey, out List<CanteenDto> canteens))
        {

            const string url = "https://agata.suz.cvut.cz/jidelnicky/index.php";
            string pageContent = await _httpClient.GetStringAsync(url, cancellationToken);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageContent);

            canteens = htmlDoc.DocumentNode.SelectNodes("//ul[@id='menzy']/li/a")
                .Select(node =>
                {
                    int.TryParse(node.Id.Replace("podSh", ""), out int id);
                    bool open = node.SelectSingleNode($"{node.XPath}/img")
                        .GetAttributeValue("src", "closed").Contains("Otevreno");
                    string name = node.InnerText.Trim();
                    return new CanteenDto(id, name, open);
                }).ToList();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            _memoryCache.Set(cacheKey, canteens, cacheEntryOptions);
        }

        if (onlyOpen)
            return canteens.Where(c => c.Open).ToList();
        return canteens.ToList();
    }

    public async Task<CanteenDto> GetCurrentMenuAsync(CanteenDto canteen, CancellationToken cancellationToken = default)
    {
        const string url = "https://agata.suz.cvut.cz/jidelnicky/index.php?clPodsystem={0}";
        string pageContent = await _httpClient.GetStringAsync(string.Format(url, canteen.Id), cancellationToken);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(pageContent);

        var rows = htmlDoc.DocumentNode
            .SelectNodes("//div[@class='data']/table[@class='table table-condensed']/tbody/tr");
        if (rows is null)
        {
            // TODO: Log and maybe say something to user?
            return canteen with { TodayDishes = ArraySegment<CanteenDishDto>.Empty};
        }

        var dishes = new List<CanteenDishDto>();

        string currentDishType = "Jiné";

        foreach (var row in rows)
        {
            if (row is null) continue;

            var th = row.ChildNodes["th"];
            if (th is not null)
            {
                currentDishType = th.InnerText.Trim();
                continue;
            }

            var tds = row.SelectNodes("td");
            if (tds is null) continue;

            string amount = TrimHtml(tds[1].InnerText);
            string name = TrimHtml(tds[2].InnerText);
            string photo = ParsePhoto(tds[4]);

            string studentPrice = TrimHtml(tds[5].InnerText);
            string otherPrice = TrimHtml(tds[6].InnerText);

            dishes.Add(new CanteenDishDto(currentDishType, name, amount, studentPrice, otherPrice, photo));
        }


        return canteen with { TodayDishes = dishes };
    }


    private static string ParsePhoto(HtmlNode node)
    {
        var photo = node.Descendants("a")?.FirstOrDefault();

        if (photo is null)
        {
            return string.Empty;
        }

        string link = photo.GetAttributeValue("href", string.Empty);

        if (string.IsNullOrEmpty(link)) return link;

        return $"https://agata.suz.cvut.cz/jidelnicky/{link}";
    }

    private static string TrimHtml(string text)
    {
        return text.Replace("&nbsp;", " ").Trim();
    }
}
