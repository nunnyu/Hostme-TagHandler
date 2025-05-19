using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HostmeTagHandler;
public class MessageBundle
{
    public List<ReceiptInfo> receiptInfoList {  get; set; }
    
    public MessageBundle(List<ReceiptInfo> receiptInfoList)
    {
        this.receiptInfoList = receiptInfoList;
    }

    public override string ToString()
    {
        string toReturn = "";

        int index = 1;
        foreach (var receiptInfo in receiptInfoList)
        {
            toReturn += ("-- Receipt Info: " + index + " --\n");
            toReturn += (receiptInfo + "\n");
            index++;
        }

        return toReturn;
    }

    public void PrintTags()
    {
        foreach (var receiptInfo in receiptInfoList)
        {
            receiptInfo.PrintTags();
        }
    }

    public void TagReceipts(List<List<string>> lolot)
    {
        int receiptIndex = 0;
        foreach (var list in lolot)
        {
            this.receiptInfoList[receiptIndex].AddTags(list);
            receiptIndex++;
        }
    }
}

public class ReceiptInfo
{
    private enum TagCategories
    {
        // Food Preferences
        MeatLover, 
        SoupFan, 
        SteakFan,
        SeafoodFan,
        PlantBased,
        GlutenFree,
        LowCarb, 
        ComfortFood, 
        PastaFan,
        SpicyFoodFan,
        SweetsFan,
        LuxuryFood, 

        // Drink Habits
        WhiteWineFan,
        RedWineFan,
        WhiskeyFan,
        BeerFan,
        CoffeeFan,
        CocktailFan,

        // Time-Based Behavior
        Morning,
        Lunch,
        Dinner,
        LateNight,
        Weekend,
    }

    public List<Item> items { get; set; }
    public string identifier { get; set; }
    private List<TagCategories> Tags;

    public ReceiptInfo(string identifier, List<Item> items)
    {
        this.identifier = identifier;
        this.items = items;
        this.Tags = new List<TagCategories>();
    }

    public GuestProfile? BuildProfile(List<GuestProfile> allProfiles)
    {
        if (this.identifier != "Unknown" && this.items.Count() > 0)
        {
            GuestProfile profile = new GuestProfile(this.identifier);
            List<string> tagStrings = Tags.Select(tag => tag.ToString()).ToList();
            profile.AddOrder(tagStrings, items[0].CreationDate, items[0].TotalPrice);

            return profile;
        }
        else
        {
            return null;
        }
    }

    public string GetTagCategories()
    {
        string categories = string.Join(", ", Enum.GetNames(typeof(TagCategories)));

        return categories;
    }

    public void AddTags(List<string> tags)
    {
        List<TagCategories> enumTags = tags
        .Select(tag => Enum.Parse<TagCategories>(tag, ignoreCase: true))
        .ToList();

        foreach (TagCategories tagCategory in enumTags)
        {
            Tags.Add(tagCategory);
        }
    }

    public void PrintTags()
    {
        Console.WriteLine("-- Tagging --");
        Console.WriteLine("Identifier: " + this.identifier);

        int index = 0;

        foreach (var tag in Tags)
        {
            Console.Write(tag.ToString());

            if (index != Tags.Count - 1)
            {
                Console.Write(", ");
            }

            index++;
        }

        Console.WriteLine("\n");
    }

    public override string ToString()
    {
        string toReturn = "Identifier: " + this.identifier + "\n\n";

        foreach (var item in items)
        {
            toReturn += (item + "\n");
        }

        if (this.items.Count == 0)
        {
            toReturn += "No items ordered.";
            toReturn += "\n";
        }

        return toReturn;
    }
}

public class Item
{
    public string? ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Amount { get; set; }
    public double TotalPrice { get; set; } // Later we need to account for different currencies 
    public int Course { get; set; }
    public DateTime CreationDate { get; set; }

    public Item(string? productId, string? productName, int amount, double totalPrice, int course, DateTime creationDate)
    {
        this.ProductId = productId;
        this.ProductName = productName;
        this.Amount = amount;
        this.TotalPrice = totalPrice;
        this.Course = course;
        this.CreationDate = creationDate;
    }

    public override string ToString()
    {
        string builtString = "Name: " + this.ProductName + "\n"
            + "Price: " + this.TotalPrice + "\n"
            + "Order Date: " + this.CreationDate + "\n";

        return builtString;
    }
}