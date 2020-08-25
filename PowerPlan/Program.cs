using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PowerPlan
{
    static class Program
    {
        private static NotifyIcon notify;
        private static Dictionary<Guid, MenuItem> plans = new Dictionary<Guid, MenuItem>();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Load all the power plans.
            Power.GetPlans();

            notify = new NotifyIcon();

            ContextMenu menu = new ContextMenu();
            menu.MenuItems.AddRange(CreateMenuItems());

            notify.Icon = Properties.Resources.Icon;
            notify.Text = "Power Plan";
            notify.ContextMenu = menu;
            notify.Visible = true;
            //notify.Click += new EventHandler(NotifyClick);

            Application.Run();

            notify.Visible = false;
        }

        private static void RefreshMenuItems()
        {
            foreach (KeyValuePair<Guid, MenuItem> pair in plans)
            {
                pair.Value.Checked = Power.Plans[pair.Key].Active;
            }
        }

        private static MenuItem[] CreateMenuItems()
        {
            List<MenuItem> menuItems = new List<MenuItem>();

            int i = 0;
            foreach (KeyValuePair<Guid, Plan> pair in Power.Plans)
            {
                MenuItem plan = new MenuItem();
                plan.Index = i++;
                plan.Text = pair.Value.Name;
                plan.Checked = pair.Value.Active;
                plan.Click += (obj, e) => 
                {
                    Power.SetActiveScheme(pair.Key);
                    Power.GetPlans();
                    RefreshMenuItems();
                };
                plans[pair.Key] = plan;
                menuItems.Add(plan);
            }

            MenuItem separator = new MenuItem();
            separator.Index = i++;
            separator.Text = "-";
            menuItems.Add(separator);

            MenuItem exitMenuItem = new MenuItem();
            exitMenuItem.Index = i++;
            exitMenuItem.Text = "Exit";
            exitMenuItem.Click += new EventHandler(exitMenu);
            menuItems.Add(exitMenuItem);

            return menuItems.ToArray();
        }

        private static void exitMenu(object Sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
