using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentHousingBV.Classes.House
{
    public class Room
    {
        private static int currentId = 1;

        public int id { get; }
        public RoomType name { get; }
        public bool isReserveable { get; }
        public bool isOccupied { get; set; }
        public List<Scheduler.SchedulerCategory> allowedCategory { get; set; }

        public Room(RoomType name, List<Scheduler.SchedulerCategory> schedulerCategory)
        {
            this.name = name;
            this.id = UpId();
            allowedCategory = schedulerCategory;
            isOccupied = false;
            if (name == RoomType.Bedroom)
            {
                isReserveable = false;
            }
            else
            {
                isReserveable = true;
            }
        }

        public Room(RoomType name)
        {
            this.isReserveable = false;
            this.name = name;
            this.id = UpId();
            allowedCategory = new List<Scheduler.SchedulerCategory>();
            isOccupied = false;
            if (name != RoomType.Bedroom)
            {
                isReserveable = true;
            }
        }

        private int UpId()
        {
            return currentId++;
        }

        public bool Equals(Room room)
        {
            if (this.id == room.id)
                return true;
            return false;
        }
    }
}
