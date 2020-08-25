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

            Application.Run();

            notify.Visible = false;
        }

        /// <summary>
        /// Updates each menu item from the power plans state.
        /// </summary>
        private static void RefreshMenuItems()
        {
            foreach (KeyValuePair<Guid, MenuItem> pair in plans)
            {
                pair.Value.Checked = Power.Plans[pair.Key].Active;
            }
        }

        /// <summary>
        /// Construct all of the initial menu items.
        /// </summary>
        /// <returns>An array of MenuItems.</returns>
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
            exitMenuItem.Text = Resources.strings.Close;
            exitMenuItem.Click += new EventHandler(exitMenu);
            menuItems.Add(exitMenuItem);

            return menuItems.ToArray();
        }

        /// <summary>
        /// Callback for exiting the application.
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        private static void exitMenu(object Sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
