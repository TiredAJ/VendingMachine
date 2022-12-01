#region Using
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using VendingMachine_Final.Properties;
#endregion

namespace VendingMachine_Final
{
    public partial class frm_VendingMachine : Form
    {
        Basket UserBasket = new Basket();

        //TIMEOUT is in seconds, default is 300 (5mins)
        private const int TIMEOUT = 90;        
        private int TimeElapsed = 0;

        public frm_VendingMachine()
        {InitializeComponent();}

        private void frm_VendingMachine_Load(object sender, EventArgs e)
        {
            //ensures the machine's in a ready state at load
            ResetVM();
        }

        public void ItemSelected(object sender, MouseEventArgs e)
        {//handles PictureBox drag and drop to add items to a basket
            PictureBox SelectedItem = sender as PictureBox;

            SelectedItem.DoDragDrop(SelectedItem.Tag,DragDropEffects.Copy);

            pb_BinIcon.Enabled = false;

            ResetTimer();
        }

        public void ItemSelected(object sender, EventArgs e)
        {//handles buttons to add items to a basket
            Button SelectedItem = sender as Button;

            UserBasket.AddToBasket(Convert.ToInt32(SelectedItem.Tag));

            RefreshVM();
            lbl_LastAction.Text = "Item added to basket";
        }

        private void btn_Checkout_Click(object sender, EventArgs e)
        {
            if (UserBasket.GetBasketSize() > 0)
            {
                //changes the machine to payment state
                grp_Payment.Enabled = true;

                pnl_ItemDisplay.Enabled = false;
                grp_Basket.Enabled = false;


                txt_RemainderToPay.Text = UserBasket.GetTotal().ToString("C");
            }
            else
            {MessageBox.Show("Please add an item to the basket");}

            ResetTimer();

            lbl_LastAction.Text = "Moved to payment";
        }

        private void pb_BasketIcon_DragEnter(object sender, DragEventArgs e)
        {//changes the picture of the basket item if it's the correct format
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.Copy;
                pb_BasketIcon.Image = Properties.Resources.Basket_Open;
            }
            else
            {
                e.Effect = DragDropEffects.None;
                pb_BasketIcon.Image = Properties.Resources.Basket_Closed;
            }
        }

        private void pb_BasketIcon_DragDrop(object sender, DragEventArgs e)
        {
            int StockID = Convert.ToInt32(e.Data.GetData(DataFormats.Text));

            //passes through the dragdropped index
            UserBasket.AddToBasket(StockID);

            //resets the icon to the default image
            pb_BasketIcon.Image = Properties.Resources.Basket_Closed;

            RefreshVM();

            lbl_LastAction.Text = "Item added to basket";
        }

        private void lst_Basket_MouseDown(object sender, MouseEventArgs e)
        {
            if (lst_Basket.Items.Count > 0)
            {
                lst_Basket.DoDragDrop(lst_Basket.SelectedIndex.ToString(),DragDropEffects.Copy);

                //enables the bin if dragging from the ListBox
                pb_BinIcon.Enabled = true;
            }
        }

        private void pb_BinIcon_DragEnter(object sender, DragEventArgs e)
        {//swaps the bin's icon when dragged over
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.Copy;
                pb_BinIcon.Image = Properties.Resources.Bin_Open;
            }
            else
            {
                e.Effect = DragDropEffects.None;
                pb_BinIcon.Image = Properties.Resources.Bin_Closed;
            }
        }

        private void pb_BinIcon_DragDrop(object sender, DragEventArgs e)
        {
            //passes through the copied index
            UserBasket.RemoveFromBasket(Convert.ToInt32(e.Data.GetData(DataFormats.Text)));

            //closes the bin icon
            pb_BinIcon.Image = Properties.Resources.Bin_Closed;

            RefreshVM();

            lbl_LastAction.Text = "Item removed from basket";
        }

        private void Icon_DragLeave(object sender, EventArgs e)
        {//resets both icons to their closed states
            pb_BasketIcon.Image = Properties.Resources.Basket_Closed;
            pb_BinIcon.Image = Properties.Resources.Bin_Closed;

            //ensures the bin icon's closed
            pb_BinIcon.Enabled = false;
        }

        private void RefreshVM()
        {//handles refreshing the machine
            UserBasket.Refresh(lst_Basket);

            //updates the total TextBox
            txt_BasketTotal.Text = UserBasket.GetTotal().ToString("C");

            pb_BinIcon.Enabled = false;

            ResetTimer();
        }

        private void ResetVM()
        {//sets the machine to it's default state

            //forces the internal basket to be cleared
            UserBasket.ClearBasket();
            //cleares the ListBox
            UserBasket.Refresh(lst_Basket);

            txt_BasketTotal.Text = "£0.00";
            lbl_LastAction.Text = "Machine loaded";

            pb_CardReader.Enabled = true;

            //resets the DepositIndicator to it's default red colour
            pb_DepositIndicator.BackColor = Color.Red;

            grp_Basket.Enabled = true;
            pnl_ItemDisplay.Enabled = true;

            grp_Payment.Enabled = false;
        }

        private void btn_ClearBasket_Click(object sender, EventArgs e)
        {//forces the basket to clear and refreshes the machine
            UserBasket.ClearBasket();
            RefreshVM();

            lbl_LastAction.Text = "Basket cleared";
        }

        private void btn_TotalReset_Click(object sender, EventArgs e)
        {//verifies that the user wants to reset the machine
            DialogResult DR_Trans;

            DR_Trans = MessageBox.Show
            (
                "Are you sure you'd like to reset?",
                "Confirmation",
                MessageBoxButtons.YesNo
            );

            if (DR_Trans == DialogResult.Yes)
            {ResetVM();}
        }

        private void SlotCheck(object sender, DragEventArgs e)
        {//checks that the right money icons are allowed in the right slots
            float Temp = Convert.ToSingle(e.Data.GetData(DataFormats.Text));
            PictureBox PBX = sender as PictureBox;

            switch (PBX.Tag)
            {
                case "C":
                {//coin slot
                    if (Temp <= 2.0f)//£2 and less would be a coin
                    {e.Effect = DragDropEffects.Copy;}
                    else
                    {e.Effect= DragDropEffects.None;}

                    break;
                }
                case "N":
                {//note slot
                    if (Temp > 2.0f)//greater than £2 would be a note
                    {e.Effect = DragDropEffects.Copy;}
                    else
                    {e.Effect = DragDropEffects.None;}

                    break;
                }
                default:
                {//error message
                    MessageBox.Show("Please visit frm_VendingMachine.SlotCheck", "uh oh");
                    e.Effect = DragDropEffects.None;

                    break;
                }                    
            }
        }

        private void Money_MouseDown(object sender, MouseEventArgs e)
        {
            PictureBox PBX = sender as PictureBox;

            //copies the tag's value
            PBX.DoDragDrop(PBX.Tag, DragDropEffects.Copy);

            ResetTimer();
        }

        private void Money_DragDrop(object sender, DragEventArgs e)
        {
            float Value = Convert.ToSingle(e.Data.GetData(DataFormats.Text));

            //deposit the money's value into the UserBasket
            UserBasket.DepositValue(Value);

            //refreshes deposit info
            DepositRefresh();

            lbl_LastAction.Text = "Money deposited";
        }

        private void btn_Vend_Click(object sender, EventArgs e)
        {//finalises the transaction
            float Change = UserBasket.GetChange();
            DialogResult DR_Receipt;

            DR_Receipt = MessageBox.Show
                (//checks if the user would like a receipt
                    "Please enjoy your items, would you like a receipt?",
                    "Transaction Complete",
                    MessageBoxButtons.YesNo
                );

            //passes true if the user requests a receipt, false otherwise
            if (DR_Receipt == DialogResult.Yes)
            {PrintBasket(true);}
            else
            {
                PrintBasket(false);
                //also grabs and presents the user with change
                if (UserBasket.GetChange() > 0)
                {
                    MessageBox.Show
                    (
                    "Here's your change: " + UserBasket.GetChange().ToString("C"),
                    "Change"
                    );
                }
            }
            ResetVM();
        }

        private void btn_CancelPayment_Click(object sender, EventArgs e)
        {//cancels payment and allows the user to go back and edit their basket
            pnl_ItemDisplay.Enabled = true;
            grp_Basket.Enabled = true;

            pb_DepositIndicator.BackColor = Color.Red;

            grp_Payment.Enabled = false;
        }

        private void PrintBasket(bool ReceiptQ)
        {
            //gets path of the user's desktop
            string SaveLoc = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string CustDate;

            //generates a string with today's date and time (d/t)
            CustDate = DateTime.Now.ToString("dd-mm-yy HH-mm-ss");

            //uses the d/t string to generate a folder for the basket
            SaveLoc += ("\\Basket [" + CustDate + "]");

            //creates the basket folder
            System.IO.Directory.CreateDirectory(SaveLoc);

            if (ReceiptQ)
            {//writes the receipts if true is passed

                //creates the Streamwriter object in the basket folder
                StreamWriter Writer = new StreamWriter(SaveLoc + "\\_Receipt.txt");

                Writer.WriteLine(CustDate);

                Writer.WriteLine("-----------------------------------");

                UserBasket.PrintContents(Writer);

                Writer.WriteLine("-----------------------------------");

                Writer.WriteLine("Total = " + UserBasket.GetTotal().ToString("C"));

                Writer.WriteLine("-----------------------------------");

                if (!UserBasket.CardUsedQ())
                {Writer.WriteLine("Change = " + UserBasket.GetChange().ToString("C"));}
                else
                {Writer.WriteLine("Card Used");}

                Writer.Close();
            }

            UserBasket.VendBasket(SaveLoc);

            //attempts to open the basket folder in the file explorer
            try
            {Process.Start(SaveLoc);}
            catch (Win32Exception ExceptionMessage)
            {//error message
                MessageBox.Show("There was an error opening the basket");
                MessageBox.Show(ExceptionMessage.Message);
                throw;
            }
        }

        private void TT_MouseHover(object sender, EventArgs e)
        {//displays the item's name and price as a tooltip

            //creates a control object
            Control Ctrl;

            //determines if the sender is a PictureBox or Button and assigns accordingly
            if (sender is PictureBox)
            {Ctrl = sender as PictureBox;}
            else
            {Ctrl = sender as Button;}

            //retreives the data relating to the hovered control from Stock[]
            tt_HoverDisplay.ToolTipTitle = UserBasket.Stock[Convert.ToInt32(Ctrl.Tag)].GetName();
            tt_HoverDisplay.SetToolTip(Ctrl, UserBasket.Stock[Convert.ToInt32(Ctrl.Tag)].GetPrice().ToString("C"));

            //resets the timer
            ResetTimer();
        }

        private void pb_CardReader_MouseEnter(object sender, EventArgs e)
        {//for the bank card custom cursor
            //creates a new cursor object with the picture in Resources
            Cursor cur = new Cursor(Properties.Resources.BankCard.Handle);
            //assigns to the cursor
            this.Cursor = cur;
        }

        private void pb_CardReader_MouseLeave(object sender, EventArgs e)
        {//resets the cursor to it's default
            this.Cursor = Cursors.Default;
        }

        private void pb_CardReader_Click(object sender, EventArgs e)
        {//deposits the full value of the basket when the card is used
            UserBasket.DepositValue(UserBasket.GetTotal());

            DepositRefresh();
            UserBasket.CardUsedTrue();

            lbl_LastAction.Text = "(card) payment made";
        }

        private void DepositRefresh()
        {
            if (UserBasket.GetChange() >= 0.0f)
            {//checks if the total deposited is enough or more
                pb_DepositIndicator.BackColor = Color.Green;
                btn_Vend.Enabled = true;
                btn_Vend.Select();
                txt_RemainderToPay.Text = (0.0f).ToString("C");

                //disables the card reader
                pb_CardReader.Enabled = false;
            }
            else
            {
                pb_DepositIndicator.BackColor = Color.Red;
                btn_Vend.Enabled = false;
                //updates the user with the remainder to pay
                txt_RemainderToPay.Text = (UserBasket.GetChange() * -1).ToString("C");
            }
        }

        private void tim_TimeoutTimer_Tick(object sender, EventArgs e)
        {//ticks every 1000ms (1s)
            DialogResult DR_OkResult;

            TimeElapsed++;

            //if it hits the Timeout limit
            if (TimeElapsed == TIMEOUT)
            {
                TimeElapsed = 0;
                ResetVM();
            }
            else if (TimeElapsed == 30)
            {//alerts the user that the machine will reset in 4mins
                DR_OkResult = MessageBox.Show("Vending Machine will reset in 1 minute(s)");

                //resets the timer
                if (DR_OkResult == DialogResult.OK)
                {ResetTimer();}
            }
        }

        private void ResetTimer()
        {TimeElapsed = 0;}
    }

    class Basket
    {
        private const float EASTER_EGG_VAL = 47.0f;
        public Item[] Stock = new Item[15]
        {
            new Item("Pepsi", 1.20f, Properties.Resources.Pepsi),
            new Item("Pepsi Cherry", 1.30f, Properties.Resources.PepsiCherry),
            new Item("Pepsi Max", 1.20f, Properties.Resources.PepsiMax),
            new Item("Coca Cola Zero", 1.20f, Properties.Resources.CocaColaZero),
            new Item("Coca Cola Vanilla", 1.30f, Properties.Resources.CocaColaVanilla),
            new Item("Coca Cola", 1.80f, Properties.Resources.CocaCola),
            new Item("Lucozade Sport Raspberry", 1.80f, Properties.Resources.LucozadeSportRasp),
            new Item("Lucozade Sport Orange", 1.80f, Properties.Resources.LucozadeSportOrange),
            new Item("Barebells Vanilla Milkshake", 1.50f, Properties.Resources.BarebellsVanillaMilkshake),
            new Item("7up", 1.0f, Properties.Resources._7up),
            new Item("Galaxy Smooth Milk", 1.0f, Properties.Resources.GalaxySmoothMilk),
            new Item("Mars Bar", 0.75f, Properties.Resources.MarsBar),
            new Item("Skittles Giants", 2.1f, Properties.Resources.SkittlesGiants),
            new Item("Waf-fulls Chocolate", 2.0f, Properties.Resources.WaffullsChoc),
            new Item("Waf-fulls Strawberry", 2.0f, Properties.Resources.WaffullsStrawb)  
        };
        private LinkedList<Item> BasketInv = new LinkedList<Item>();
        private float Total = 0.0f, DepositedAmount = 0.0f;
        private bool CardUsed = false;

        public Basket()
        {ResetQuantities();}

        public void Refresh(ListBox lst_Basket)
        {//refreshes the ListBox with the contents of BasketInv
            lst_Basket.Items.Clear();

            foreach (Item item in BasketInv)
            {lst_Basket.Items.Add(item.GetText());}

            //recalculates the total
            CalculateTotal();
        }

        public void AddToBasket(int ItemID)
        {
            //bool Found = false;
            int FoundIndex = -1;

            //loops through the basket to check if the latest added item is already 
            //in the basket...
            if (BasketInv.Count > 0)
            {
                for (int i = 0; i < BasketInv.Count(); i++)
                {
                    if (BasketInv.ElementAt<Item>(i).GetName() == Stock[ItemID].GetName())
                    {//...if it is, FoundIndex is set to it's index...
                        FoundIndex = i;
                        break;
                    }
                }
            }

            //...if it's greater than -1, it increments the item's quantity as it's
            //index, otherwise, it adds it to the basket.
            if (FoundIndex > -1)
            {BasketInv.ElementAt<Item>(FoundIndex).IncrementQuantity();}
            else
            {BasketInv.AddLast(Stock[ItemID]);}
        }

        public void RemoveFromBasket(int Index)
        {
            if (BasketInv.Count > 0)
            {
                if (BasketInv.ElementAt<Item>(Index).GetQuantity() > 1)
                {BasketInv.ElementAt<Item>(Index).DecrementQuantity();}
                else
                {BasketInv.Remove(BasketInv.ElementAt<Item>(Index));}
            }
        }

        public void ClearBasket()
        {
            //clear listbox
            foreach (Item item in Stock)
            {item.ResetQuantity();}

            BasketInv.Clear();
            Total = 0.0f;
        }

        public void CalculateTotal()
        {
            Total = 0.0f;

            foreach (Item item in BasketInv)
            {Total += item.GetPrice();}

        }

        public void ResetQuantities()
        {
            foreach (Item item in BasketInv)
            {item.ResetQuantity();}
        }

        public void DepositValue(float DepositVal)
        {DepositedAmount += DepositVal;}

        public void VendBasket(string FolderLoc)
        {
            foreach (Item item in BasketInv)
            {item.PrintItem(FolderLoc);}

            if (Total == EASTER_EGG_VAL)
            {Properties.Resources.CatBlehh.Save(FolderLoc + "\\CatBlehh.png");}
        }

        public void PrintContents(StreamWriter Writer)
        {
            foreach (Item item in BasketInv)
            {Writer.WriteLine(item.GetText() + " - " + item.GetPrice().ToString("C"));}
        }

        public void CardUsedTrue()
        {CardUsed = true;}

        public float GetTotal()
        {
            CalculateTotal();
            return Total;
        }

        public float GetChange()
        {return (DepositedAmount - Total);}

        public int GetBasketSize()
        {return BasketInv.Count;}

        public bool CardUsedQ()
        {return CardUsed;}

    }

    class Item
    {
        private string Name;
        private float Price = 0.0f;
        private int Quantity = 1;
        private Image ItemPic;

        public Item(string name, float price, Image itemPic)
        {
            Name = name;
            Price = price;
            ItemPic = itemPic;
        }

        public void ResetQuantity()
        {Quantity = 1;}

        public void IncrementQuantity()
        {Quantity++;}

        public void DecrementQuantity()
        {Quantity--;}

        public void PrintItem(string FolderLoc)
        {
            if (Quantity > 1)
            {
                for (int i = 0; i < Quantity; i++)
                {ItemPic.Save(FolderLoc + "\\" + Name + "_" + (i+1).ToString() + ".png");}
            }
            else
            {ItemPic.Save(FolderLoc + "\\" + Name + ".png");}
        }

        public float GetPrice()
        {return (Price * Quantity);}

        public string GetText()
        {
            if (Quantity > 1)
            {return (Name + " x" + Quantity.ToString());}
            else
            {return Name;}
        }

        public string GetName()
        {return Name;}

        public int GetQuantity()
        {return Quantity;}
    }
}
