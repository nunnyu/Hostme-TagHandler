using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace HostmeTagHandler;
public class GuestProfile // Respresents aggregated data, rather than a single order 
{
    public string identifier { get; set; }
    public Dictionary<string, int> TagCounts { get; set; } = new Dictionary<string, int>();

    public TimeSpan TotalOrderTime { get; set; } = TimeSpan.Zero;
    public double TotalOrderPrice { get; set; } = 0;
    public int OrderCount { get; set; } = 0;

    public GuestProfile(string identifier)
    {
        this.identifier = identifier;
    }

    public void Merge(GuestProfile other)
    {
        // Merge tags 
        foreach (var tag in other.TagCounts)
        {
            for (int c = 0; c < tag.Value; c++)
            {
                AddTag(tag.Key);
            }
        }

        TotalOrderTime += other.TotalOrderTime;
        TotalOrderPrice += other.TotalOrderPrice;
        OrderCount += other.OrderCount;
    }

    public void AddOrder(List<string> tags, DateTime orderDateTime, double orderPrice)
    {
        foreach (string tag in tags)
        {
            AddTag(tag);
        }

        TotalOrderTime += orderDateTime.TimeOfDay;
        TotalOrderPrice += orderPrice;
        OrderCount++;
    }

    private void AddTag(string tag, int count = 1)
    {
        if (TagCounts.ContainsKey(tag))
            TagCounts[tag] += count;
        else
            TagCounts[tag] = count;
    }

    public TimeSpan GetAverageOrderTime()
    {
        return OrderCount == 0 ? TimeSpan.Zero : TimeSpan.FromTicks(TotalOrderTime.Ticks / OrderCount);
    }

    public double GetAverageOrderPrice()
    {
        return OrderCount == 0 ? 0 : TotalOrderPrice / OrderCount;
    }

    public override string ToString()
    {
        var tagsSummary = "";
        foreach (var kv in TagCounts)
        {
            tagsSummary += kv.Key + ": " + kv.Value + ", ";
        }

        if (tagsSummary.Length > 2)
            tagsSummary = tagsSummary.Substring(0, tagsSummary.Length - 2);

        return "GuestProfile:\n" +
               "- Identifier: " + identifier + "\n" +
               "- Orders: " + OrderCount + "\n" +
               "- Avg Order Time: " + GetAverageOrderTime().ToString(@"hh\:mm") + "\n" +
               "- Avg Order Price: " + GetAverageOrderPrice().ToString("0.00") + "\n" +
               "- Tags: " + tagsSummary + "\n";
    }
}