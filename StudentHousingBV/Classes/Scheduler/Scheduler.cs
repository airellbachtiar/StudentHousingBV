using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StudentHousingBV.Classes.User;
using StudentHousingBV.Classes.House;
using System.Globalization;

namespace StudentHousingBV.Classes.Scheduler
{
    public class Scheduler
    {
        public List<Reservation> reservations { get; set; }
        public string DateFormat { get; }

        public Scheduler()
        {
            reservations = new List<Reservation>();


            CultureInfo cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
            this.DateFormat = cultureInfo.DateTimeFormat.ShortDatePattern.ToString();
        }

        public int AddReservation(DateTime schedulerDate, User.User user, SchedulerCategory schedulerCategory, string schedulerDescription, Room room, int duration)
        {
            int messageCode = 30; // Code: everything worked fine

            if (IsAvailable(schedulerDate, room, duration))
            {
                if(CheckCategory(room, schedulerCategory))
                {
                    if(user.isStudent()) // Checking if a Student is making a reservation or the landlord/admin
                    {
                        if(CheckStrikes(((Student)user).strikes.Count(), schedulerCategory))
                        {
                            Reservation reservation = new Reservation(room, schedulerDate, (Student)user, schedulerCategory, schedulerDescription, duration); // Create the reservation
                            reservations.Add(reservation);  // Add the reservation to the report
                        }
                        else
                        {
                            messageCode = 33; // Too many strikes code
                        }
                    }
                    else
                    {
                        Reservation reservation = new Reservation(room, schedulerDate, (Admin)user, schedulerCategory, schedulerDescription, duration); // Create the reservation
                        reservations.Add(reservation);
                    }
                }
                else
                {
                    messageCode = 32; // Error code: wrong category
                }
            }
            else
            {
                messageCode = 31; // Error code: not available
            }

            return messageCode;
        }

        //remove reservation
        public void RemoveReservation(int resId)
        {
            this.reservations.Remove(this.GetReservation(resId));
            Refresh();
        }

        //check if the remover is their reservation
        public bool RemoveCheck(int resId, User.User loggedUser)
        {
            bool result = true;
            if (loggedUser.isStudent())
            {
                if ((this.GetReservation(resId).user.Equals(loggedUser)) == false)
                    result = false;
            }
            return result;
        }

        //check how many strikes to be allowed to reserve
        private bool CheckStrikes(int amountOfStrikes, SchedulerCategory schedulerCategory)
        {
            bool isAllowed = true;
            if(schedulerCategory == SchedulerCategory.Party)
            {
                if (amountOfStrikes >= 3)
                    isAllowed = false;
            }
            return isAllowed;
        }

        private bool IsAvailable(DateTime date, Room room, int duration)
        {
            DateTime newDateEnd = date.AddHours(duration); // The end date of the new reservation

            bool result = true;
            foreach (Reservation r in this.reservations)
            {
                DateTime end = r.date.AddHours(r.duration); // The end date of already existing reservations

                if(((date >= r.date && date < end) || (newDateEnd >=r.date && date < end)) && (r.room.Equals(room))) // If the begin or end time of the new reservation overlaps of existing reservations AND it is the same room then you can't make a reservation
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        //check room categories that are allowed
        private bool CheckCategory(Room chosenRoom, SchedulerCategory schedulerCategory)
        {
            bool result = false;
            foreach (SchedulerCategory s in chosenRoom.allowedCategory)
            {
                if (schedulerCategory == s)
                {
                    result = true;
                }
            }
            return result;
        }

        public Reservation GetReservation(int id)
        {
            foreach (Reservation r in reservations)
            {
                if (r.id == id)
                {
                    return r;
                }
            }
            return null;
        }

        //rearrange list ascending and delete past event except repair and cleaning
        public void Refresh()
        {
            reservations.Sort((x, y) => x.date.CompareTo(y.date));
            string current = DateTime.Now.ToString($"{this.DateFormat} HH:mm");
            DateTime currentDate = DateTime.ParseExact(current, $"{this.DateFormat} HH:mm", null);
            foreach (Reservation r in reservations.ToList())
            {
                DateTime thisDate = r.date.Add(new TimeSpan(0, 2, 0, 0));
                if (currentDate > r.date)
                {
                    r.status = "Ongoing";
                    if (currentDate > thisDate && !(r.category == SchedulerCategory.Repair || r.category == SchedulerCategory.Cleaning))
                    {
                        RemoveReservation(r.id);
                    }
                }
            }
        }
    }
}
