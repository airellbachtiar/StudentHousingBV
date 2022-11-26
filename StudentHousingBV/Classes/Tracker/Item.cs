using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentHousingBV.Classes.Tracker
{
    public class Item
    {
        // If we want to distinguish between custom prices and fixed prices with amounts, we have to make a bool in the
        // sorts of bool customPrice. For now we go with fixed prices with amounts.


        private static int currentID = 0;

        public string name { get; set; }
        public double price { get; set; }
        public int id { get; }
        public int amount { get; set; }
        public List<User.Student> studentNeedToPay { get; set; }

        public Item(string name, double price, int amount, List<User.Student> studentNeedToPay)
        {
            this.name = name;
            this.price = price;
            this.amount = amount;
            this.studentNeedToPay = studentNeedToPay;
            this.id = UpId();
        }

        private int UpId()
        {
            return currentID++;
        }
    }
}
