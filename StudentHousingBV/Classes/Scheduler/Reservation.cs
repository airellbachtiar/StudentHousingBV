using System;

namespace StudentHousingBV.Classes.Scheduler
{
    public class Reservation
    {
        private static int currentID = 1;

        public House.Room room { get; set; }
        public DateTime date { get; set; }
        public int duration { get; set; }
        public User.User user { get; set; }
        public SchedulerCategory category { get; set; }
        public string description { get; set; }
        public int id { get; set; }
        public string status { get; set; }

        public Reservation(House.Room room, DateTime date, User.User user, SchedulerCategory category, string description, int duration)
        {
            this.room = room;
            this.date = date;
            this.user = user;
            this.category = category;
            this.description = description;
            this.id = currentID++;
            this.duration = duration;
            this.status = "Soon";
        }
    }
}
