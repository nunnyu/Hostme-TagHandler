using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HostmeTagHandler;
public class MessageBundle
{
    public List<ReceiptInfo> receiptInfoList = new List<ReceiptInfo>();
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

    public List<Item> items;
    private List<TagCategories> Tags;

    public ReceiptInfo(List<Item> items)
    {
        this.items = items;
        this.Tags = new List<TagCategories>();
    }

    public string getTagCategories()
    {
        string categories = string.Join(", ", Enum.GetNames(typeof(TagCategories)));

        return categories;
    }

    public override string ToString()
    {
        string toReturn = "";

        foreach (var item in items)
        {
            toReturn += (item + "\n");
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