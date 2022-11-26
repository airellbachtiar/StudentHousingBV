using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using StudentHousingBV.Classes.House;
using StudentHousingBV.Classes.User;
using StudentHousingBV.Classes.Tracker;
using StudentHousingBV.Classes.Scheduler;
using StudentHousingBV.Classes.Report;
using System.Globalization;

namespace StudentHousingBV
{
    public partial class TenantForm : Form
    {
        private House house;
        private Student loggedUser;
        private Scheduler scheduler;
        private Report report;

        private bool showAll = true; // Show all dates
        private DateTime currentShownDate; // Show only the reservations with this date

        public TenantForm(House house, Student loggedUser, Scheduler scheduler, Report report)
        {
            InitializeComponent();

            this.house = house;
            this.loggedUser = loggedUser;
            this.Name = loggedUser.name;
            this.scheduler = scheduler;
            this.report = report;

            txtInfoNewPassword.PasswordChar = '*';
            txtInfoNewPasswordAgain.PasswordChar = '*';

            reportSetup();  //Report
            infoSetup(); // Info
            schedulerSetup(); // Scheduler
            trackerSetup(); // Tracker
        }

        #region Report System

        public void reportSetup()
        {
           // cboxComlaintCategory.Items.AddRange(Enum.GetValues(typeof(complaintCategory)).Cast<complaintCategory>().Select(x => x.ToString()).ToArray());

            foreach (complaintCategory c in Enum.GetValues(typeof(complaintCategory)))
            {
                cboxComlaintCategory.Items.Add(c);
            }

            cboxComplaintTargetStudent.Items.Add("None"); // 'None' is meant to indicate that is is a general complaint   
            foreach (User u in house.users) // Adding the students to target user, but not the itself.
            {
                if (u.isStudent() && (u.Equals(loggedUser) == false))
                {
                    cboxComplaintTargetStudent.Items.Add(u.displayName);
                }
            }
            cboxComlaintCategory.SelectedIndex = 0;
            cboxComplaintTargetStudent.SelectedIndex = 0;

            //Update the list of the complaint
            updateComplaint();
        }

        // Update the list of complaints in the listview
        public void updateComplaint()
        {
            lsvReportComplaints.Items.Clear();
            rtxtComplaintComment.Clear();

            foreach (Complaint c in report.complaints) // Adding the complaints to the listview
            {
                string targetStudentName = "None";  // A: General complaint
                if (c.type.Equals('B')) // B: complaint with target student
                    targetStudentName = c.targetStudent.displayName;

                string studentName = c.student.displayName;
                if (c.anonymous) // Anon or not
                    studentName = "Anonymous";

                if (report.ComplaintVisibility(c, loggedUser)) // Checking if the complaint is indeed visible to the logged user.
                {
                    ListViewItem complaintItem = new ListViewItem(c.id.ToString()); // First subitem is the id for the complaint
                    complaintItem.SubItems.Add(studentName);
                    complaintItem.SubItems.Add(targetStudentName);
                    complaintItem.SubItems.Add(c.category.ToString());

                    lsvReportComplaints.Items.Add(complaintItem);
                }
            }
        }

        public Complaint createComplaint(string comment, complaintCategory category, bool anonymous, string targetStudentName)
        {
            if (targetStudentName.Equals("None"))
                return new Complaint(comment, category, anonymous, this.loggedUser); // Type A Complaint: General
            else
               return new Complaint(comment, category, anonymous, this.loggedUser, this.house.GetStudent(targetStudentName)); // Type B Complaint: To a specific student
        }

        private void btnSendComplaint_Click(object sender, EventArgs e)
        {
            int messageCode = 11; // 11: error message regarding. Will be send to determine the message to the user. This will change into another message if everything worked

            complaintCategory category = (complaintCategory)cboxComlaintCategory.SelectedItem;
            string targetStudentName = cboxComplaintTargetStudent.Text;
            string comment = rtxtComplaintComment.Text;
            bool anonymous = ckboxAnon.Checked;

            Complaint complaint = createComplaint(comment, category, anonymous, targetStudentName);

            if (this.report.AddCompaint(complaint) == true) // Adding the complaint to the list. The method returns true if it's done succesfully
            {
                updateComplaint();

                messageCode = 10; //10 is a succesfull message
            }

            Messages(messageCode);
        }

        // When clicked on a row. The comment will be shown
        private void lsvReportComplaints_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsvReportComplaints.SelectedItems.Count > 0)
            {
                int selectedIndex = lsvReportComplaints.SelectedIndices[0]; // The index of the current selected item in the ListView
                int cId = int.Parse(lsvReportComplaints.Items[selectedIndex].SubItems[0].Text); // The ID of the Complaint

                rtxtComplaintSelectedComment.Text = report.GetComplaint(cId).comment; // Showing the comment
            }
        }

        #endregion

        #region Info System

        // Perhaps not needed, but can make more sense for the initialization
        public void infoSetup()
        {
            updateInfo();   // Update/Refresh their info page
            addInfoStrikeSelection(); // Adding the user his/her strikes to their info page
        }

        public void updateInfo()    // Updates the info 
        {
            txtInfoName.Text = this.loggedUser.name;
            txtInfoRoom.Text = this.loggedUser.roomId.ToString();
            txtInfoBalance.Text = this.loggedUser.balance.ToString();
        }

        public void addInfoStrikeSelection() // Refresh
        {
            cboxInfoStrike.Enabled = true;
            cboxInfoStrike.Items.Clear(); // Clearing the combobox
            rtxtInfoStrike.Clear(); // Clearing the comment

            if (loggedUser.strikes.Count() > 0)
            {
                foreach (Strike s in this.loggedUser.strikes)
                {
                    cboxInfoStrike.Items.Add(s.id.ToString()); // Change this later to show a name or "Strike: {this.id}"
                }
                cboxInfoStrike.SelectedIndex = 0;
            }
            else
            {
                cboxInfoStrike.Enabled = false;
            }
        }

        public void updateInfoStrikeComment()
        {
            Strike strike = this.loggedUser.GetStrike(int.Parse(cboxInfoStrike.Text));

            rtxtInfoStrike.Text = strike.GetInfo();
        }

        public void cancelPassword()
        {
            // clear text in the boxes
            clearPasswordBoxes();

            // User can type in the boxes
            txtInfoNewPassword.ReadOnly = true;
            txtInfoNewPasswordAgain.ReadOnly = true;

            // Save and cancel button are enabled. Change button disabled
            btnInfoCancelPassword.Enabled = false;
            btnInfoSavePassword.Enabled = false;
            btnInfoChangePassword.Enabled = true;
        }

        public void clearPasswordBoxes()
        {
            // clear text in the boxes
            txtInfoNewPassword.Clear();
            txtInfoNewPasswordAgain.Clear();
        }

        private void cboxInfoStrike_SelectedIndexChanged(object sender, EventArgs e) // Show the selected strike in the box
        {
            updateInfoStrikeComment();
        }

        private void btnInfoChange_Click(object sender, EventArgs e)    // Being able to change by enabling the boxes and save button
        {
            txtInfoName.ReadOnly = false;
            btnInfoSave.Enabled = true;
        }

        private void btnInfoSave_Click(object sender, EventArgs e)
        {
            int messageCode = 21; // Error message

            if (this.loggedUser.changeName(txtInfoName.Text) == true)   // If name is succesfully changed, then bool = true
            {
                messageCode = 20; // Succesfull message

                //Locking the boxes and save button
                txtInfoName.ReadOnly = true;
                btnInfoSave.Enabled = false;

                //Update all info for the whole program
                updateInfo();
                updateComplaint();
                updateInfoStrikeComment();
                updateSchedulerListBox();
                updateTrackerList();
            }
            Messages(messageCode);
        }

        private void btnInfoChangePassword_Click(object sender, EventArgs e)
        {
            // User can type in the boxes
            txtInfoNewPassword.ReadOnly = false;
            txtInfoNewPasswordAgain.ReadOnly = false;

            // Save and cancel button are enabled. Change button disabled
            btnInfoCancelPassword.Enabled = true;
            btnInfoSavePassword.Enabled = true;
            btnInfoChangePassword.Enabled = false;
        }

        private void btnInfoSavePassword_Click(object sender, EventArgs e)
        {
            int messageCode = 24; // Succesfull code: Password is changed succesfully

            string newPassword = txtInfoNewPassword.Text;
            string newPasswordAgain = txtInfoNewPasswordAgain.Text;

            if (string.IsNullOrWhiteSpace(newPassword) == false && string.IsNullOrWhiteSpace(newPasswordAgain) == false) // This doesn't work because if you do 0000 then it counts as a null
            {
                if (newPassword.Equals(newPasswordAgain) == true)
                {
                    this.loggedUser.password = newPassword;
                    cancelPassword(); // Succesfully saved, so the boxes need to be disabled again
                }
                else
                {
                    messageCode = 23; // Error code: Both password have to be the same
                    clearPasswordBoxes();
                }
            }
            else
            {
                messageCode = 22; // Error code: Password can't be empty
                clearPasswordBoxes();
            }

            Messages(messageCode);
        }

        private void btnInfoCancelPassword_Click(object sender, EventArgs e)
        {
            cancelPassword();
        }

        #endregion

        #region Scheduler

        // If there are no rooms in the house, the program would crash probably. Fix this later
        public void schedulerSetup()
        {
            foreach (SchedulerCategory s in Enum.GetValues(typeof(SchedulerCategory)))
            { 
                if(s != SchedulerCategory.Repair) // The repair category doesn't need to be added for the Students
                    cboxSchedulerCategory.Items.Add(s);
            }
            cboxSchedulerCategory.SelectedIndex = 0;

            cboxHour.SelectedIndex = 0;

            //Scheduler Room list
            foreach (Room r in house.rooms)
            {
                if(r.isReserveable == true) // Bedrooms shouldn't be able to be reserved
                    cboxSchedulerRoom.Items.Add(r.name);
            }
            cboxSchedulerRoom.SelectedIndex = 0;

            updateSchedulerListBox();
        }

        // Show the reservations in the listbox
        public void updateSchedulerListBox()
        {
            //update listbox
            lsvSchedulerReservations.Items.Clear();
            foreach (Reservation r in scheduler.reservations)
            {
                if(showAll == false)
                {
                    if (r.date.ToString(this.scheduler.DateFormat).Equals(currentShownDate.ToString(this.scheduler.DateFormat)))
                    {
                        lsvSchedulerReservations.Items.Add(createReservationItem(r)); // Adding only the reservations that are the same as the selected date to be shown
                    }
                }
                else
                {
                    lsvSchedulerReservations.Items.Add(createReservationItem(r)); // Adding every reservation to the listbox
                }
            }
        }

        // Maybe move this to the scheduler class for when you actually create the reservation
        public ListViewItem createReservationItem(Reservation r)
        {
            ListViewItem ReservationItem = new ListViewItem(r.id.ToString());
            ReservationItem.SubItems.Add(r.date.ToString(this.scheduler.DateFormat));
            ReservationItem.SubItems.Add(r.date.ToString("HH:mm"));
            ReservationItem.SubItems.Add(r.date.AddHours(r.duration).ToString("HH:mm"));
            ReservationItem.SubItems.Add(r.user.displayName.ToString());
            ReservationItem.SubItems.Add(r.room.name.ToString());
            ReservationItem.SubItems.Add(r.category.ToString());
            ReservationItem.SubItems.Add(r.status);

            return ReservationItem;
        }

        private void btnAddReservation_Click(object sender, EventArgs e)
        {

            //convert string to datetime format for easier comparison with other reservation
            DateTime schedulerDate = DateTime.ParseExact($"{dateTimePicker.Value.ToString(this.scheduler.DateFormat)} {cboxHour.SelectedItem}", $"{this.scheduler.DateFormat} HH:mm", System.Globalization.CultureInfo.InvariantCulture);//culture info can be null


            RoomType roomName = (RoomType)cboxSchedulerRoom.SelectedItem;
            Room room = this.house.GetRoom(roomName);
            SchedulerCategory schedulerCategory = (SchedulerCategory)cboxSchedulerCategory.SelectedItem;
            string schedulerDescription = txtSchedulerDescription.Text;
            int schedulerDuration = Convert.ToInt32(nudSchedulerDuration.Value);

            //If possible the reservation is added and a succes code will be given, else an errorCode
            int messageCode = this.scheduler.AddReservation(schedulerDate, this.loggedUser, schedulerCategory, schedulerDescription, room, schedulerDuration);
            txtSchedulerDescription.Clear();

            Messages(messageCode); // Show the right message to the user
            updateSchedulerListBox(); //The whole box should be updated.
        }

        private void btnSchedulerRemoveReservation_Click(object sender, EventArgs e)
        {
            int messageCode = 36; // Succesfull message code

            if (lsvSchedulerReservations.SelectedIndices.Count > 0)
            {
                int reservationId = int.Parse(lsvSchedulerReservations.Items[lsvSchedulerReservations.SelectedIndices[0]].SubItems[0].Text);
                if (scheduler.RemoveCheck(reservationId, loggedUser)) // True if it's your reservation
                {
                    scheduler.RemoveReservation(reservationId);
                    updateSchedulerListBox();
                }
                else
                {
                    messageCode = 35; // Error message: it's not the user's reservation
                }
            }
            else
            {
                messageCode = 34; // Error message: No reservation is selected to be removed
            }

            Messages(messageCode);
        }

        private void btnSchedulerShowAll_Click(object sender, EventArgs e)
        {
            this.showAll = true;
            scheduler.Refresh();
            updateSchedulerListBox();
        }

        private void lsvSchedulerReservations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsvSchedulerReservations.SelectedItems.Count > 0)
            {
                int id = int.Parse(lsvSchedulerReservations.Items[lsvSchedulerReservations.SelectedIndices[0]].SubItems[0].Text);
                rtbxSchedulerDescription.Text = scheduler.GetReservation(id).description;
            }
            else
            {
                rtbxSchedulerDescription.Clear();
            }
        }

        private void dateTimePicker_ValueChanged(object sender, EventArgs e)//show picked date
        {
            this.showAll = false;
            this.currentShownDate = dateTimePicker.Value;
            scheduler.Refresh();
            updateSchedulerListBox();
        }

        #endregion

        #region Tracker

        // Check if it works without any students in the program on the admin side
        public void trackerSetup()
        {
            //puts everything on the tracker listboxes
            updateTrackerList();

            // Adding the users to the pay cbox
            if(this.house.numOfStudents > 0)
            {
                foreach (User u in house.users)
                {
                    if (u.isStudent() == true && u.Equals(this.loggedUser) == false)
                    {
                        cmbUserToPay.Items.Add(u.displayName);
                    }
                }
                cmbUserToPay.SelectedIndex = 0;
            }
        }

        public void updateTrackerList()//display all item purchased and users balance
        {
            Tracker.UpdateBalance(this.house.users, this.house.deactivatedStudents);

            lsvTrackerItems.Items.Clear();
            lsbTrackerBalance.Items.Clear();
            lsbTrackerDeactiveBalance.Items.Clear();

            foreach (User u in house.users)
            {
                if (u.isStudent())
                {
                    foreach (Item i in ((Student)u).items)
                    {
                        ListViewItem itemToStore = Tracker.CreateItemLVI(i, (Student)u);
                        lsvTrackerItems.Items.Add(itemToStore);
                    }
                    lsbTrackerBalance.Items.Add($"{u.displayName} - €{Convert.ToString(((Student)u).balance)}");    // Adding the balances of the users
                }
            }

            foreach(Student s in this.house.deactivatedStudents)
            {
                lsbTrackerDeactiveBalance.Items.Add($"{s.displayName} - €{Convert.ToString(s.balance)}");
            }
        }

        private void btnTrackerItemAdd_Click(object sender, EventArgs e)
        {
            int messageCode = 40; // Succesfull code: everything is added

            string itemName = txtTrackerName.Text;
            double price = Convert.ToDouble(nudPrice.Value);
            int amount = Convert.ToInt32(nudAmount.Value);

            if ((string.IsNullOrWhiteSpace(itemName) == false))
            {
                if ((price > 0) && (amount > 0))
                {
                    
                    Item temp = new Item(itemName, price, amount, Tracker.GetStudentsNeedToPay(this.loggedUser, this.house.users));
                    this.loggedUser.AddItem(temp);
                   // this.tracker.updateBalance(price * amount, loggedUser, this.house.users, this.house.numOfStudents); // Needs to be changed
                    updateTrackerList();
                }
                else
                {
                    messageCode = 42; // Error code:  The numeric values can't be negative or zero
                }
            }
            else
            {
                messageCode = 41; // Error code: Name can't be empty
            }

            Messages(messageCode);
        }

        private void btnTrackerItemRemove_Click(object sender, EventArgs e)
        {
            int messageCode = 43; // Succesfull code: Item(amount) is removed

            if (lsvTrackerItems.SelectedItems.Count > 0)
            {
                int amountToRemove = Convert.ToInt32(nudRemoveAmount.Value);

                int selectedIndex = lsvTrackerItems.SelectedIndices[0]; // The index of the current selected item in the ListView
                int itemId = int.Parse(lsvTrackerItems.Items[selectedIndex].SubItems[0].Text); // The ID of the item

                Student owner = this.house.GetStudent(lsvTrackerItems.Items[selectedIndex].SubItems[4].Text);

                Item itemToRemove = owner.GetItem(itemId);

                if (this.loggedUser.items.Contains(itemToRemove))
                {
                    if (amountToRemove <= itemToRemove.amount)
                    {
                        //tracker.updateBalance(itemToRemove.price * amountToRemove * -1, this.loggedUser, this.house.users, this.house.numOfStudents);

                        //If the amount and amount to remove are the same, the item should be removed from the tracker
                        if (itemToRemove.amount == amountToRemove)
                        {
                            this.loggedUser.RemoveItem(itemToRemove);
                        }
                        else
                        {
                            itemToRemove.amount -= amountToRemove;
                        }
                        updateTrackerList();
                    }
                    else
                    {
                        messageCode = 47; // Error code: Amount is too much to remove
                    }
                }
                else
                {
                    messageCode = 45; // Error code: Item not bought by logged user
                }
            }
            else
            {
                messageCode = 44; // Error code: No item selected
            }

            Messages(messageCode);
        }

        private void btnPay_Click(object sender, EventArgs e)
        {
            int messageCode = 46; // Succes code: User is paid

            string userToPay = Convert.ToString(cmbUserToPay.SelectedItem);
            double amountToPay = Math.Round(Convert.ToDouble(nudPay.Value),2);

            bool isPayed = Tracker.PayUser(this.loggedUser, amountToPay, this.house.GetStudent(userToPay)); /// Pays the userToPay and check if it worked

            if (isPayed == false)
            {
                messageCode = 48; // Error Code: You can't overpay or pay the user;
            }

            updateTrackerList();
            Messages(messageCode); // You succesfully paid 

        }
        #endregion

        private void btnLogout_Click(object sender, EventArgs e)
        {
            int index = house.users.IndexOf(loggedUser);
            this.house.users[index] = loggedUser;

            this.Hide();
            LoginForm form1 = new LoginForm(house, scheduler, report);
            form1.Closed += (s, args) => this.Close();
            form1.Show();
        }

        public void Messages(int i)
        {
            // Maybe make it so that in the 10s we get good messages and 20s error messages 
            // OR
            // 10-19 are for info messages, 20-29 for report and so forth
            string message = "";

            switch (i)
            {
                // Report Messages 10-19
                case 10:
                    message = "Thank you for reporting. Your complaint is succesfully saved.";
                    break;
                case 11:
                    message = "The comment box can't be empty!";
                    break;

                // Info Messages 20-29
                case 20:
                    message = "Your name is succesfully changed!";
                    break;
                case 21:
                    message = "Your name can't contain any numbers!";
                    break;
                case 22:
                    message = "Your password can't be empty!";
                    break;
                case 23:
                    message = "Passwords are not the same! Try again!";
                    break;
                case 24:
                    message = "Your password is succesfully saved!";
                    break;

                // Scheduler Messages 30-39
                case 30:
                    message = "Your reservation is added!";
                    break;
                case 31:
                    message = "This room already booked at this hour!";
                    break;
                case 32:
                    message = "Can't assign this category to this room!";
                    break;
                case 33:
                    message = "You have reach maximum strikes to book this place!";
                    break;
                case 34:
                    message = "You have to select a reservation first to remove one!";
                    break;
                case 35:
                    message = "You can't remove this reservation! This one is not yours!";
                    break;
                case 36:
                    message = "The reservation is succesfully removed!";
                    break;

                // Tracker Messages 40-49
                case 40:
                    message = "The item is succesfully added!";
                    break;
                case 41:
                    message = "The item name can't be empty!";
                    break;
                case 42:
                    message = "The price and amount have to be 1 or higher!";
                    break;
                case 43:
                    message = "The item is succesfully removed!";
                    break;
                case 44:
                    message = "You need to select an item before trying to remove one!";
                    break;
                case 45:
                    message = "This item is not bought by you so you can't remove it!";
                    break;
                case 46:
                    message = "You succesfully paid someone!";
                    break;
                case 47:
                    message = "Trying to remove more than there actually is, is not possible!";
                    break;
                case 48:
                    message = "You'can't either pay that user or overpay!";
                    break;
            }

            MessageBox.Show(message);
        }

        #region Button Tabs
        private void btnShowScheduler_Click(object sender, EventArgs e)
        {
            pnlScheduler.BringToFront();
            lblTitle.Text = "SCHEDULER";
            btnShowScheduler.BackColor = Color.FromArgb(62, 76, 89);
            btnShowTracker.BackColor = Color.FromArgb(97, 110, 124);
            btnShowReport.BackColor = Color.FromArgb(97, 110, 124);
            btnShowInfo.BackColor = Color.FromArgb(97, 110, 124);
        }

        private void btnShowTracker_Click(object sender, EventArgs e)
        {
            pnlTracker.BringToFront();
            lblTitle.Text = "TRACKER";
            btnShowScheduler.BackColor = Color.FromArgb(97, 110, 124);
            btnShowTracker.BackColor = Color.FromArgb(62, 76, 89);
            btnShowReport.BackColor = Color.FromArgb(97, 110, 124);
            btnShowInfo.BackColor = Color.FromArgb(97, 110, 124);
        }

        private void btnShowReport_Click(object sender, EventArgs e)
        {
            pnlReport.BringToFront();
            lblTitle.Text = "REPORT";
            btnShowScheduler.BackColor = Color.FromArgb(97, 110, 124);
            btnShowTracker.BackColor = Color.FromArgb(97, 110, 124);
            btnShowReport.BackColor = Color.FromArgb(62, 76, 89);
            btnShowInfo.BackColor = Color.FromArgb(97, 110, 124);
        }

        private void btnShowInfo_Click(object sender, EventArgs e)
        {
            updateInfo(); // Update balance info
            pnlInfo.BringToFront();
            lblTitle.Text = "INFO";
            btnShowScheduler.BackColor = Color.FromArgb(97, 110, 124);
            btnShowTracker.BackColor = Color.FromArgb(97, 110, 124);
            btnShowReport.BackColor = Color.FromArgb(97, 110, 124);
            btnShowInfo.BackColor = Color.FromArgb(62, 76, 89);
        }

        private void lblCompany_Click(object sender, EventArgs e)
        {
            pnlHome.BringToFront();
            lblTitle.Text = "HOME";
            btnShowScheduler.BackColor = Color.FromArgb(97, 110, 124);
            btnShowTracker.BackColor = Color.FromArgb(97, 110, 124);
            btnShowReport.BackColor = Color.FromArgb(97, 110, 124);
            btnShowInfo.BackColor = Color.FromArgb(97, 110, 124);
            btnShowStrike.BackColor = Color.FromArgb(97, 110, 124);
            btnShowAddStudent.BackColor = Color.FromArgb(97, 110, 124);
        }
        #endregion
    }
}
