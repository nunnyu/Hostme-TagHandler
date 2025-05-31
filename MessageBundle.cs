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

    public List<GuestProfile?> BuildProfiles()
    {
        List<GuestProfile?> list = new List<GuestProfile?>();
        foreach (ReceiptInfo receiptInfo in receiptInfoList)
        {
            list.Add(receiptInfo.BuildProfile());
        }

        return list;
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
    private List<string> tagCategories = new List<string>();

    public List<Item> items { get; set; }
    public string identifier { get; set; }
    private List<string> Tags;

    public ReceiptInfo(string identifier, List<Item> items)
    {
        this.identifier = identifier;
        this.items = items;
        this.Tags = new List<string>();
    }

    public GuestProfile? BuildProfile()
    {
        if (this.identifier != "Unknown" && this.items.Count() > 0)
        {
            GuestProfile profile = new GuestProfile(this.identifier);
            List<string> tagStrings = Tags.Select(tag => tag.ToString()).ToList();

            // Total price of the order
            double totalPrice = 0d;
            foreach (var item in items)
            {
                totalPrice += item.TotalPrice;
            }

            profile.AddOrder(tagStrings, items[0].CreationDate, totalPrice);

            return profile;
        }
        else
        {
            return null;
        }
    }

    public void AddTags(List<string> tags)
    {
        foreach (string tagCategory in tags)
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