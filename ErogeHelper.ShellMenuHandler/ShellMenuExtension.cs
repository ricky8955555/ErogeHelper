using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ErogeHelper.ShellMenuHandler.Properties;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace ErogeHelper.ShellMenuHandler
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".exe")]
    public class ShellMenuExtension : SharpContextMenu
    {
        private ContextMenuStrip _menu = new ContextMenuStrip();

        /// <summary>
        /// Determines whether the menu item can be shown for the selected item.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if item can be shown for the selected item for this instance.; 
        ///   otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanShowMenu()
        {
            // We can show the item only for a single selection.
            if (SelectedItemPaths.Count() != 1)
                return false;

            UpdateMenu();
            return true;
        }

        /// <summary>
        /// Creates the context menu. This can be a single menu item or a tree of them.
        /// Here we create the menu based on the type of item
        /// </summary>
        /// <returns>
        /// The context menu for the shell context menu.
        /// </returns>
        protected override ContextMenuStrip CreateMenu()
        {
            _menu.Items.Clear();

            var is64Bit = PeFileReader.GetPeType(SelectedItemPaths.First()) == PeType.X64;
            // check if the selected executable is 64 bit
            if (is64Bit)
            {
                MenuX64();
            }
            else
            {
                MenuX86();
            }

            // return the menu item
            return _menu;
        }

        private static readonly bool Is4K = SystemHelper.Is4KDisplay();

        private static readonly Image E = Is4K ? Resource.E_200 : Resource.E;

        private static readonly Image CheckboxComposite =
            Is4K ? Resource.CheckboxComposite_200 : Resource.CheckboxComposite;
        private static readonly Image Checkbox = Is4K ? Resource.Checkbox_200 : Resource.Checkbox;

        private void MenuX64()
        {
            var mainMenu = new ToolStripMenuItem
            {
                Text = Language.Strings.ShellMenu_DirectStart,
                Image = E
            };
            mainMenu.Click += (sender, args) => MainProcess(false);

            _menu.Items.Clear();
            _menu.Items.Add(mainMenu);
        }

        /// <summary>
        /// Creates the context menu when the selected .exe is 32 bit.
        /// </summary>
        protected void MenuX86()
        {
            var mainMenu = new ToolStripMenuItem
            {
                Text = Language.Strings.About_AppName,
                Image = E
            };

            var directStartItem = new ToolStripMenuItem
            {
                Text = Language.Strings.ShellMenu_DirectStart,
                Image = E
            };

            var leStartItem = new ToolStripMenuItem
            {
                Text = Language.Strings.ShellMenu_LEStart,
                Image = E
            };

            directStartItem.Click += (sender, args) => MainProcess(false);
            leStartItem.Click += (sender, args) => MainProcess(true);

            mainMenu.DropDownItems.Add(directStartItem);
            mainMenu.DropDownItems.Add(leStartItem);
            var adminItem = new ToolStripMenuItem
            {
                Text = Language.Strings.ShellMenu_Administrator,
                Image = _isAdmin ? CheckboxComposite : Checkbox,
            };

            adminItem.Click += (sender, args) =>
            {
                _isAdmin = !_isAdmin;
            };
            mainMenu.DropDownItems.Add(adminItem);

            _menu.Items.Clear();
            _menu.Items.Add(mainMenu);
        }

        private static bool _isAdmin;

        private void MainProcess(bool useLe)
        {
            var startInfo = new ProcessStartInfo();

            #region Get Path of dll (Same as project binary Path)
            // https://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            #endregion

            var dirPath = Path.GetDirectoryName(path);
            var erogeHelperProc = dirPath + @"\ErogeHelper.exe";
            var gamePath = SelectedItemPaths.First();
            startInfo.FileName = erogeHelperProc;
            startInfo.Arguments = $"\"{gamePath}\"";
            startInfo.UseShellExecute = false;

            if (useLe)
            {
                // gamePath must be Args[0]
                startInfo.Arguments += " /le";
            }

            if (_isAdmin)
            {
                //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.UseShellExecute = true;
                startInfo.Verb = "runas";
            }

            try
            {
                Process.Start(startInfo);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Updates the context menu. 
        /// </summary>
        private void UpdateMenu()
        {
            // release all resources associated to existing menu
            _menu.Dispose();
            _menu = CreateMenu();
        }
    }
}
