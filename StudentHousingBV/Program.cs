using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using StudentHousingBV.Classes.House;
using StudentHousingBV.Classes.User;
using StudentHousingBV.Classes.Tracker;
using StudentHousingBV.Classes.Scheduler;
using StudentHousingBV.Classes.Report;

namespace StudentHousingBV
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            List<SchedulerCategory> kitchenCategories = Enum.GetValues(typeof(SchedulerCategory)).Cast<SchedulerCategory>().ToList();
            List<SchedulerCategory> livingroomCategories = new List<SchedulerCategory>
            {
                SchedulerCategory.Cleaning,
                SchedulerCategory.Party,
                SchedulerCategory.Repair,
                SchedulerCategory.Other
            };
            List<SchedulerCategory> bathroomCategories = new List<SchedulerCategory>
            {
                SchedulerCategory.Cleaning,
                SchedulerCategory.Repair,
                SchedulerCategory.Other
            };

            Room room1 = new Room(RoomType.Bedroom);
            Room room2 = new Room(RoomType.Bedroom);
            Room room3 = new Room(RoomType.Bedroom);
            Room room4 = new Room(RoomType.Bedroom);

            Room livingroom = new Room(RoomType.Livingroom, livingroomCategories);
            Room toilet = new Room(RoomType.Bathroom, bathroomCategories);
            Room kitchen = new Room(RoomType.Kitchen, kitchenCategories);

            Admin admin = new Admin("Admin", "000");

            Student student1 = new Student("Ismet", "123", room1.id);
            Student student2 = new Student("Airell", "1234", room2.id);
            Student student3 = new Student("Russell", "12345", room3.id);

            room1.isOccupied = true;
            room2.isOccupied = true;
            room3.isOccupied = true;

            House house = new House();

            house.AddUser(admin);
            house.AddUser(student1);
            house.AddUser(student2);
            house.AddUser(student3);

            house.AddRoom(room1);
            house.AddRoom(room2);
            house.AddRoom(room3);
            house.AddRoom(room4);
            house.AddRoom(livingroom);
            house.AddRoom(kitchen);
            house.AddRoom(toilet);

            //Classes setup
            Scheduler scheduler = new Scheduler();
            Report report = new Report();

            //Type A complaints
            Complaint c1 = new Complaint("The domestic pigeon (Columba livia domestica) is a pigeon subspecies that was derived from the rock dove (also called the rock pigeon).", complaintCategory.Misconduct, false, student1);
            Complaint c2 = new Complaint(" The rock pigeon is the world's oldest domesticated bird.", complaintCategory.Misconduct, false, student2);
            Complaint c3 = new Complaint(" Mesopotamian cuneiform tablets mention the domestication of pigeons more than 5,000 years ago, as do Egyptian hieroglyphics. ", complaintCategory.Misconduct, true, student3);

            //Type B complaints
            Complaint c4 = new Complaint("Research suggests that domestication of pigeons occurred as early as 10,000 years ago", complaintCategory.Misconduct, false, student1, student3);
            Complaint c5 = new Complaint("Pigeons have made contributions of considerable importance to humanity, especially in times of war.", complaintCategory.Misconduct, false, student2, student1);
            Complaint c6 = new Complaint(" In war the homing ability of pigeons has been put to use by making them messengers.", complaintCategory.Misconduct, true, student3, student2);

            report.AddCompaint(c1);
            report.AddCompaint(c2);
            report.AddCompaint(c3);
            report.AddCompaint(c4);
            report.AddCompaint(c5);
            report.AddCompaint(c6);

            //Strikes
            Strike s1 = new Strike(strikeCategory.Misconduct, "So-called war pigeons have carried many vital messages and some have been decorated for their services.", student1);
            Strike s2 = new Strike(strikeCategory.Other, "Medals such as the Croix de guerre, awarded to Cher Ami, and the Dickin Medal awarded to the pigeons G.I. Joe and Paddy, amongst 32 others,, have been awarded to pigeons for their services in saving human lives.", student2);
            Strike s3 = new Strike(strikeCategory.Misconduct, "Despite this, city pigeons today are seen as pests, mainly due to their droppings.", student3);
            Strike s4 = new Strike(strikeCategory.Noise, "Feral pigeons are considered invasive in many parts of the world, though they have a low impact on wild bird populations and environment.", student3);

            student1.AddStrike(s1);
            student2.AddStrike(s2);
            student3.AddStrike(s3);
            student3.AddStrike(s4);

            Application.Run(new LoginForm(house, scheduler, report));
        }
    }
}
