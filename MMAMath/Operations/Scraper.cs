﻿using System.Globalization;
using System.Text.Json;
using HtmlAgilityPack;
using Newtonsoft.Json;

using MMAMath.Models;

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
            var sections = document.DocumentNode.SelectNodes("//section[contains(@class, 'fcListing')]");

            if (sections.Count > 0)
            {
                foreach (var section in sections)
                {
                    var promotionNode = section.SelectSingleNode(".//div[@class='promotion']//span[@class='name']/a");
                    string promotionName = promotionNode.InnerText.Trim();

                    string eventLink = promotionNode.GetAttributeValue("href", string.Empty);
                    if (!string.IsNullOrEmpty(eventLink))
                    {
                        eventLink = new Uri(new Uri("https://www.tapology.com/"), eventLink).ToString();
                    }

                    var dateNode = section.SelectSingleNode(".//div[@class='promotion']//span[@class='datetime']");
                    string eventDateStr = dateNode.InnerText.Trim();
                    DateTime eventDate = new DateTime();
                    try
                    {
                        eventDate = DateTime.ParseExact(eventDateStr, "dddd, MMMM dd, yyyy", CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                        eventDate = GetDateFromEventPage(eventLink);
                    }


                    var eventDetails = new EventDetails
                    {
                        Name = promotionName,
                        Date = eventDate,
                        Link = eventLink
                    };

                    eventList.Add(eventDetails);
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
            DateTime eventDateTime;
            try
            {
                eventDateTime = DateTime.ParseExact(dateTimeString, "dddd MM.dd.yyyy 'at' hh:mm tt ET", CultureInfo.InvariantCulture, DateTimeStyles.None);
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
                    if (fighterNodes != null)
                    {
                        var fighterNames = new HashSet<string>();
                        foreach (var node in fighterNodes)
                        {
                            fighterNames.Add(node.InnerText.Trim());
                        }

                        if (fighterNames.Count == 2)
                        {
                            var fighterList = fighterNames.ToList();
                            fighterA = fighterList[0];
                            fighterB = fighterList[1];
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
                if (new DateTime (2024, 07, 20) < currEvent.Date && currEvent.Date < DateTime.UtcNow)
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