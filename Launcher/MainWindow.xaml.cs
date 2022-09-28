using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace Launcher
{




    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Shortcut> _shortcuts { get; set; } = new ObservableCollection<Shortcut>();
        public MainWindow()
        {
            this.DataContext = this;
            InitializeComponent();

            if (System.IO.File.Exists("Data.xml"))
            {
                // Load save shortcut list
                LoadData();
            }
            else    // First run if no data file exists, just create one
            {
                _shortcuts.Add(
                    new Shortcut
                    {
                        Name = "Brave",
                        Path = @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe"
                    });
                _shortcuts.Add(
                    new Shortcut
                    {
                        Name = "Firefox",
                        Path = @"C:\Program Files\Mozilla Firefox\firefox.exe"
                    });
                _shortcuts.Add(
                    new Shortcut
                    {
                        Name = "Internet Explorer",
                        Path = @"C:\Program Files\Internet Explorer\iexplore.exe"
                    });

                SaveData();
            }

        }
        // Load data file
        private void LoadData()
        {
            DataContractSerializer x = new DataContractSerializer(_shortcuts.GetType());
            FileStream file = File.OpenRead("Data.xml");
            ObservableCollection<Shortcut> s = (ObservableCollection<Shortcut>)x.ReadObject(file);

            foreach (Shortcut shortcut in s)
            {
                _shortcuts.Add(shortcut);
            }
        }
    
        // Save Data to file
        private void SaveData()
        {
            DataContractSerializer x = new DataContractSerializer(_shortcuts.GetType());

            System.IO.FileStream file = System.IO.File.Create("Data.xml");
            x.WriteObject(file, _shortcuts);
            file.Close();
        }
        
        // Handle File Drop
        private void DDBox_Drop(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Extract dropped data and display itin the textbox to let user modufy them if needed
                // DDBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                DDBox.Text = System.IO.Path.GetFileNameWithoutExtension(files[0]);
                DDBox.Text += "\r\n" + files[0];
                btnAdd.Visibility = Visibility.Visible;
                newShortcutIcon.Visibility = Visibility.Visible;
                // Extract Icon if found
                try
                {
                    Icon i = System.Drawing.Icon.ExtractAssociatedIcon(files[0]);
                    newShortcutIcon.Source = Imaging.CreateBitmapSourceFromHIcon(i.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                } catch (Exception)
                {
                    // get the file attributes for file or directory
                    FileAttributes attr = File.GetAttributes(files[0]);

                    //detect whether its a directory or file
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        newShortcutIcon.Source = Imaging.CreateBitmapSourceFromHIcon(((Icon)StockIcon.GetStockIcon(StockIcon.SHSIID_FOLDER, StockIcon.SHGSI_LARGEICON)).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    }
                }  
            }
        }
        
        private void DDBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = DragDropEffects.Copy;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Add new shortcut to the JumpList
            btnAdd.Visibility = Visibility.Hidden;
            newShortcutIcon.Visibility=Visibility.Hidden;

            string [] data = DDBox.Text.Split('\n');

            _shortcuts.Add(new Shortcut
            {
                Name = data[0].Trim(),
                Path = data[1].Trim()
            });

            DDBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(120,120,120) ); 
            DDBox.Text = "Drag & Drop new shortcut here !";

            SetJumpList();
        }

        // Delete JumpListItem when Del button is pressed
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(sender.ToString());
            var removable = Shortcuts.SelectedItem as Shortcut;
            _shortcuts.Remove(removable);
            SetJumpList();
        }

        // Fix item auto selection
        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Shortcuts.SelectedItem = (sender as Grid).DataContext;
            txtPath.Text = ((Shortcut)Shortcuts.SelectedItem).Path;
            if (!Shortcuts.IsFocused)
                Shortcuts.Focus();
        }

        // Save to disk before closing
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveData();
        }

        // Jumplist cannot be set in class constructor the window should have been displayed first
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetJumpList();

        }

        // Set Jumplist based on saved data
        private void SetJumpList()
        {
            var currentJumplist = JumpList.GetJumpList(App.Current);
            if (currentJumplist == null)
            {
                currentJumplist = new JumpList();
            }
            else
                currentJumplist.JumpItems.Clear();

            foreach (Shortcut shortcut in _shortcuts)
            {
                FileAttributes attr = File.GetAttributes(shortcut.Path);

                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    currentJumplist.JumpItems.Add(new JumpTask
                    {
                        ApplicationPath = shortcut.Path,
                        Title = shortcut.Name,
                        CustomCategory = "Links",
                        IconResourcePath = "shell32.dll",
                        IconResourceIndex = 3

                    });
                }
                else
                {
                    currentJumplist.JumpItems.Add(new JumpTask
                    {
                        ApplicationPath = shortcut.Path,
                        Title = shortcut.Name,
                        CustomCategory = "Links",
                        IconResourcePath = shortcut.Path

                    });
                }
               
            }
            currentJumplist.Apply();
            JumpList.SetJumpList(App.Current, currentJumplist);
            
        }

    }
}
