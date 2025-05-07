using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostmeTagHandler;
class Item
{
    public string? ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Amount { get; set; }
    public double TotalPrice { get; set; }
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