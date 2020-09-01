using System;
using System.Collections.Generic;
using System.Globalization;
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
            Power.GetSchemes();

            notify = new NotifyIcon();

            ContextMenu menu = new ContextMenu();
            menu.Popup += new EventHandler(OnMenuPopup);
            menu.MenuItems.AddRange(CreateMenuItems());

            notify.Icon = Properties.Resources.Icon;
            notify.Text = Resources.strings.Title;
            notify.ContextMenu = menu;
            notify.Visible = true;

            // Do initial power plan update.
            UpdatePowerPlans();

            Application.Run();

            notify.Visible = false;
        }

        private static void UpdatePowerPlans()
        {
            // Get the currently active scheme.
            Guid activeScheme = Power.GetActiveScheme();

            IEnumerable<Guid> availablePlans = Power.GetSchemes();

            // Create list of Guids to remove (by default all).
            List<Guid> remove = new List<Guid>(plans.Keys);

            int index = 0;

            // Update all current plans.
            foreach (Guid guid in availablePlans)
            {
                string name = Power.ReadFriendlyName(guid);
                bool active = guid == activeScheme;

                if (!plans.ContainsKey(guid))
                {
                    // Create the menu item.
                    MenuItem plan = new MenuItem();
                    plan.Index = index;
                    plan.Text = name;
                    plan.Checked = active;
                    plan.Click += (obj, e) =>
                    {
                        Power.SetActiveScheme(guid);
                    };

                    // Add the menu item to the dictionary.
                    plans[guid] = plan;

                    notify.ContextMenu.MenuItems.Add(index++, plan);
                }
                else
                {
                    // Update the menu item details.
                    plans[guid].Text = name;
                    plans[guid].Checked = active;
                }

                // We won't remove this item anymore.
                remove.Remove(guid);
            }

            // Remove all stale plans.
            foreach (Guid guid in remove)
            {
                notify.ContextMenu.MenuItems.Remove(plans[guid]);
                plans[guid].Dispose();
                plans.Remove(guid);
            }
        }

        /// <summary>
        /// Called whenever the context menu opens.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnMenuPopup(object sender, EventArgs args)
        {
            UpdatePowerPlans();
        }

        /// <summary>
        /// Construct all of the initial menu items.
        /// </summary>
        /// <returns>An array of MenuItems.</returns>
        private static MenuItem[] CreateMenuItems()
        {
            MenuItem separator = new MenuItem();
            separator.Text = "-";

            MenuItem exitMenuItem = new MenuItem();
            exitMenuItem.Text = Resources.strings.ResourceManager.GetString("Close", CultureInfo.CurrentUICulture);
            exitMenuItem.Click += new EventHandler(ExitMenu);

            return new MenuItem[] { separator, exitMenuItem };
        }

        /// <summary>
        /// Callback for exiting the application.
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        private static void ExitMenu(object Sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
