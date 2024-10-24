using System.Globalization;
using System.Text.Json;
using HtmlAgilityPack;
using Newtonsoft.Json;

using MMAMath.Models;
using System.Text.RegularExpressions;

namespace MMAMath.Operations.Scraper
{
    public class Scraper
    {
        public const string BaseUrl = "https://www.tapology.com/fightcenter/promotions/1-ultimate-fighting-championship-ufc";

        private static int GetEventsLastPage(string url)
        {
            var web = new HtmlWeb();
            var document = web.Load(url);

            var lastLinkNode = document.DocumentNode
            .SelectSingleNode("//h3[@id='events']//span[@class='last']/a");
            string lastLink = lastLinkNode.GetAttributeValue("href", string.Empty);
            int lastPage = int.Parse(lastLink.Split('=')[1]);

            return lastPage;
        }

        private static List<EventDetails> GetAllUFCEvents(int lastPage)
        {
            var eventList = new List<EventDetails>();

            for (int i = 30; i > 0; i--)
            {
                Thread.Sleep(1000);
                var pageEventList = ScrapeEvents($"{BaseUrl}?page={i.ToString()}");
                eventList.AddRange(pageEventList);
            }

            return eventList;
        }

        private static List<EventDetails> ScrapeEvents(string url)
        {
            var eventList = new List<EventDetails>();

            var web = new HtmlWeb();
            var document = web.Load(url);
            var sections = document.DocumentNode.SelectNodes("//div[contains(@class, 'flex') and contains(@class, 'flex-col') and contains(@class, 'border-b') and contains(@class, 'border-solid')]");


            if (sections.Count > 0)
            {
                var reversedSections = sections.Cast<HtmlNode>().Reverse().ToList();
                foreach (var section in reversedSections)
                {
                    var linkNode = section.SelectSingleNode(".//a[@href and contains(@href, '/fightcenter/events')]");
                    var longDateNode = section.SelectSingleNode(".//span[contains(@class, 'md:inline') and contains(@class, 'hidden') and not(contains(@class, 'md:hidden')) and not(contains(@class, 'font-bold'))]");

                    if (linkNode != null)
                    {
                        var eventLink = linkNode.GetAttributeValue("href", "");
                        eventLink = $"https://www.tapology.com{eventLink}";

                        var eventName = linkNode.InnerText.Trim();

                        var longDate = longDateNode?.InnerText.Trim();
                        longDate = Regex.Replace(longDate, @"\s{2,}", " ");
                        longDate = longDate.Replace(" ET", "");

                        DateTime eventDate = new DateTime();
                        try
                        {
                            eventDate = DateTime.ParseExact(longDate, "dddd, MMMM d, yyyy", CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            // some recent events do not include year
                            Thread.Sleep(1000);
                            eventDate = GetDateFromEventPage(eventLink);
                        }

                        var eventDetails = new EventDetails
                        {
                            Name = eventName,
                            Date = eventDate,
                            Link = eventLink
                        };

                        eventList.Add(eventDetails);
                    }

                }

            }
            return eventList;
        }

        private static DateTime GetDateFromEventPage(string url)
        {
            var web = new HtmlWeb();
            var document = web.Load(url);

            var dateTimeNode = document.DocumentNode.SelectSingleNode("//li[span[contains(text(), 'Date/Time:')]]/span[2]");
            var dateTimeString = dateTimeNode?.InnerText.Trim();
            dateTimeString = dateTimeString.Replace(" ET", "");
            DateTime eventDateTime;
            try
            {
                eventDateTime = DateTime.ParseExact(dateTimeString, "dddd MM.dd.yyyy 'at' hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None);
            }
            catch
            {
                eventDateTime = DateTime.MaxValue;
            }


            return eventDateTime;
        }

        private static void SaveEventsToJson(List<EventDetails> events)
        {
            try
            {
                string filePath = Path.Combine(AppContext.BaseDirectory, "ufc_events.json");
                string jsonString = System.Text.Json.JsonSerializer.Serialize(events, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to file: {ex.Message}");
            }
        }

        private static List<EventDetails> GetEventDetailsFromFile()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "ufc_events.json");
            string jsonString = File.ReadAllText(filePath);
            List<EventDetails> eventList = JsonConvert.DeserializeObject<List<EventDetails>>(jsonString);

            return eventList;
        }

        private static List<FightDetails> GetFightsFromPage(string url, DateTime eventDate)
        {
            List<FightDetails> fightDetails = new List<FightDetails>();

            var web = new HtmlWeb();
            var document = web.Load(url);

            var liNodes = document.DocumentNode.SelectNodes("//ul[contains(@class, 'mt-5')]/li[contains(@class, 'border-b border-dotted border-tap_6')]");
            if (liNodes != null)
            {
                var liNodesFightOrder = liNodes.Reverse();
                foreach (var li in liNodesFightOrder)
                {
                    var fighterNodes = li.SelectNodes(".//div[contains(@class, 'md:flex') and (contains(@class, 'order-1') or contains(@class, 'order-2'))]/a");
                    string fighterA = "";
                    string fighterB = "";
                    string fighterAId = "";
                    string fighterBId = "";
                    if (fighterNodes != null)
                    {
                        var fighterNames = new HashSet<string>();
                        var fighterIds = new HashSet<string>();
                        foreach (var node in fighterNodes)
                        {
                            fighterNames.Add(node.InnerText.Trim());

                            var href = node.GetAttributeValue("href", "");
                            var fighterId = "";
                            fighterId = Regex.Match(href, @"fighters/([^/]+)").Groups[1].Value;
                            fighterIds.Add(fighterId);
                        }

                        if (fighterNames.Count == 2)
                        {
                            var fighterList = fighterNames.ToList();
                            fighterA = fighterList[0];
                            fighterB = fighterList[1];

                            var fighterIdList = fighterIds.ToList();
                            fighterAId = fighterIdList[0];
                            fighterBId = fighterIdList[1];

                        }
                    }

                    var resultNodes = li.SelectNodes(".//div[contains(@class, 'rounded-full')]//span[contains(@class, 'font-bold')]");
                    string fighterAResult = "";
                    string fighterBResult = "";
                    if (resultNodes != null)
                    {
                        int resultCount = 0;
                        foreach (var node in resultNodes)
                        {
                            string result = node.InnerText.Trim();

                            if (!string.IsNullOrEmpty(result))
                            {
                                if (resultCount == 0)
                                {
                                    fighterAResult = result;
                                }
                                else if (resultCount == 1)
                                {
                                    fighterBResult = result;
                                    break;
                                }
                                resultCount++;
                            }
                        }
                    }

                    var methodNode = li.SelectSingleNode(".//div[contains(@class, 'w-full')]/span[2]");
                    string method = "";
                    if (methodNode != null)
                    {
                        method = methodNode.InnerText.Trim();
                    }

                    var currFight = new FightDetails
                    {
                        FighterA = fighterA,
                        FighterB = fighterB,
                        FighterAId = fighterAId,
                        FighterBId = fighterBId,
                        FighterAResult = fighterAResult,
                        FighterBResult = fighterBResult,
                        Method = method,
                        Date = eventDate,
                    };

                    fightDetails.Add(currFight);
                }
            }

            return fightDetails;
        }
        private static void SaveFightsToJson(List<FightDetails> fights)
        {
            try
            {
                string filePath = Path.Combine(AppContext.BaseDirectory, "ufc_fights.json");
                string jsonString = System.Text.Json.JsonSerializer.Serialize(fights, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to file: {ex.Message}");
            }
        }

        private static List<FightDetails> GetAllFights(List<EventDetails> events)
        {
            List<FightDetails> allFights = new List<FightDetails>();

            var eventsSorted = events.OrderBy(e => e.Date).ToList();
            int i = 0;
            foreach (var currEvent in eventsSorted)
            {
                if (currEvent.Date < DateTime.UtcNow)
                {
                    Thread.Sleep(1000);
                    allFights.AddRange(GetFightsFromPage(currEvent.Link, currEvent.Date));

                    if (i % 10 == 0)
                    {
                        SaveFightsToJson(allFights);
                    }
                    i++;
                }
            }

            return allFights;
        }

        public static async Task<List<FighterPics>> GetAllFighterPics()
        {
            int page = 0;
            bool moreFighters = true;
            var fightersAndPics = new List<FighterPics>();

            while (moreFighters)
            {
                string url = $"https://www.ufc.com/views/ajax?gender=All&search=&view_name=all_athletes&view_display_id=page&view_args=&view_path=%2Fathletes%2Fall&view_base_path=&view_dom_id=4e9bde8cc3decb72663fbd77ebbfc57c161687ca98d905dc7f6f212900374ab0&pager_element=0&page={page}&ajax_page_state%5Btheme%5D=ufc&ajax_page_state%5Btheme_token%5D=&ajax_page_state%5Blibraries%5D=some-libraries";

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                    client.DefaultRequestHeaders.Add("Referer", "https://www.ufc.com/");

                    HttpResponseMessage response;
                    string responseBody = "";
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        responseBody = await response.Content.ReadAsStringAsync();

                    }
                    catch (Exception ex)
                    {
                        moreFighters = false;
                    }

                    var jsonArray = System.Text.Json.JsonSerializer.Deserialize<List<JsonElement>>(responseBody);

                    var targetObject = jsonArray.Find(obj => obj.GetProperty("command").ToString() == "insert");
                    var data = targetObject.GetProperty("data").GetString();

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(data);

                    var fighterNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'view-items-wrp')]//div[contains(@class, 'c-listing-athlete-flipcard__inner')]");
                    if (fighterNodes != null)
                    {
                        foreach (var node in fighterNodes)
                        {
                            string innerHtml = node.InnerHtml;
                            var fighterDoc = new HtmlDocument();
                            fighterDoc.LoadHtml(innerHtml);

                            var fighterAndPic = new FighterPics();

                            var nameNode = fighterDoc.DocumentNode.SelectSingleNode("//span[contains(@class, 'c-listing-athlete__name')]");
                            if (nameNode != null)
                            {
                                string fighterName = nameNode.InnerText.Trim();
                                fighterAndPic.Name = fighterName;
                            }

                            var imageNode = node.SelectSingleNode(".//img");
                            if (imageNode != null)
                            {
                                string picURL = imageNode.GetAttributeValue("src", string.Empty);
                                fighterAndPic.PicURL = picURL;
                            }

                            fightersAndPics.Add(fighterAndPic);
                        }
                    }
                    else
                    {
                        moreFighters = false;
                    }
                }
                page++;
            }

            string filePath = Path.Combine(AppContext.BaseDirectory, "fighter_pics.json");
            string jsonString = System.Text.Json.JsonSerializer.Serialize(fightersAndPics, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(filePath, jsonString);

            return fightersAndPics;
        }

        public static void SaveEvents()
        {
            var lastPage = GetEventsLastPage(BaseUrl);
            List<EventDetails> allUFCEvents = GetAllUFCEvents(lastPage);
            SaveEventsToJson(allUFCEvents);
        }

        public static void SaveFights()
        {
            var allEvents = GetEventDetailsFromFile();
            var allFights = GetAllFights(allEvents);
            SaveFightsToJson(allFights);
        }
    }
}
