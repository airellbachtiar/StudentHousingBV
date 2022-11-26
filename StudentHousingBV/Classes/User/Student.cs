 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StudentHousingBV.Classes.Tracker;
using StudentHousingBV.Classes.Scheduler;
using StudentHousingBV.Classes.Report;
using StudentHousingBV.Classes.House;

namespace StudentHousingBV.Classes.User
{
    public class Student : User
    {
        public double balance { get; set; }
        public double totalItemPrices { get; set; }
        public double paidBalance { get; set; } // Keeps tracks of what has the user paid to otheres and what others paid to this user. This will be added ontop of the balance
        public int roomId { get; set; }
        
        public List<Item> items { get; }
        public List<Strike> strikes { get; }

        public Student(string name, string password, int roomId) : base(name, password)
        {
            this.roomId = roomId;

            this.paidBalance = 0;
            this.balance = 0;
            this.totalItemPrices = 0;
            this.items = new List<Item>();
            this.strikes = new List<Strike>();
        }

        public void AddStrike(Strike strike)
        {
            strikes.Add(strike);
        }

        public void RemoveStrike(Strike strike)
        {
            this.strikes.Remove(strike);
        }

        public Strike GetStrike(int id)
        {
            foreach (Strike s in this.strikes)
            {
                if (s.id == id)
                {
                    return s;
                }
            }
            return null;
        }

        public void AddItem(Item item)
        {
            if (CheckDuplicateItem(item) == false)
            {
                this.items.Add(item);
            }
            UpdateTotalPrices();
        }

        // If the item already exists, only change the amount and not add an entire new item to the list.
        private bool CheckDuplicateItem(Item item)
        {
            foreach (Item i in items)
            {
                if ((i.name.Equals(item.name)) && (i.price == item.price))  //&& (i.student.Equals(item.student))
                {
                    i.amount += item.amount;
                    return true;
                }
            }
            return false;
        }

        public void RemoveItem(Item item)
        {
            this.items.Remove(item);
            UpdateTotalPrices();
        }

        public Item GetItem(int id)
        {
            foreach (Item i in this.items)
            {
                if (i.id == id)
                    return i;   
            }
            return null;
        }

        public void UpdateTotalPrices()
        {
            foreach(Item i in this.items)
            {
                this.totalItemPrices += (i.price * i.amount);
            }
        }
    }
}
