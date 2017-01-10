using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.VisualBasic.FileIO;
using Microsoft.WindowsAPICodePack.Dialogs;
using Image = System.Windows.Controls.Image;
using SearchOption = System.IO.SearchOption;

namespace ImageMover_V5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _currentDirectory;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            
            _currentDirectory = @"C:\Users\Shane\Desktop\TempCamPhotos";

            lblCurrentLocation.Content = _currentDirectory;
            cmbMoveToLocation.Items.Add(@"E:\Personal\Images\Clean\People\Freddy Lee\");
            cmbMoveToLocation.SelectedIndex = 0;

            LoadImages();


        }

        BitmapSource LoadImage(Byte[] imageData)
        {
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                var decoder = BitmapDecoder.Create(ms,
                    BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                return decoder.Frames[0];
            }
        }

        public static BitmapImage BitmapFromUri(Uri source)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        private void LoadImages()
        {
            lblCurrentLocation.Content += " Total Images - " +
                Directory.GetFiles(_currentDirectory, "*.jpg", SearchOption.TopDirectoryOnly).Count().ToString();   
            int i = 0;
            Thumbnails.Items.Clear();
            foreach (string currentimage in Directory.GetFiles(_currentDirectory, "*.jpg", SearchOption.TopDirectoryOnly))
            {
                //var bitmap = new BitmapImage(new Uri(currentimage, UriKind.Absolute));
                var bitmap = BitmapFromUri(new Uri(currentimage));
                
                //bitmap.BeginInit();
                //bitmap.CacheOption = BitmapCacheOption.None; 
                //bitmap.UriSource = new Uri(currentimage,  UriKind.Absolute);
                //bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                //bitmap.EndInit();


                Image image = new Image() { Source = bitmap };
                
                image.Width = 140;
                

                CheckBox checkbox = new CheckBox();

                checkbox.HorizontalAlignment = HorizontalAlignment.Left;
                checkbox.VerticalAlignment = VerticalAlignment.Bottom;
                var grid = new Grid();
                grid.Width = 160;
                grid.Height = image.Height + 20;

                string filename = new FileInfo(currentimage).Name;

                grid.ToolTip = filename;
                image.ToolTip = filename;
                checkbox.ToolTip = filename;

                grid.Children.Add(image);
                grid.Children.Add(checkbox);

                grid.PreviewMouseLeftButtonDown += ItemSelect;
                grid.KeyDown += Grid_KeyDown;

                Thumbnails.Items.Add(grid);
                
                if (i > 10)
                {
                    return;

                }
                i++;

            }
        }

       

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            ///TODO: find out why spacebar is not being called.
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                var test = ((ListView) sender).SelectedItems;

                ///TODO: do a loop for each selected
                foreach (Grid listViewItem in ((ListView)sender).SelectedItems)
                {
                    CheckBox selectedbox = ((CheckBox)(listViewItem).Children[1]);

                    if (selectedbox.IsChecked == true)
                        selectedbox.IsChecked = false;
                    else
                        selectedbox.IsChecked = true;
                }
                
            }
        }

        private void ItemSelect(object sender, MouseButtonEventArgs e)
        {
            CheckBox selectedbox = ((CheckBox) ((Grid) sender).Children[1]);
            
            if (selectedbox.IsChecked == true)
                selectedbox.IsChecked = false;
            else
                selectedbox.IsChecked = true;

            var bitmap = new BitmapImage(new Uri($"{_currentDirectory}\\{selectedbox.ToolTip}"));
        
            SelectedPhoto.Source = bitmap;

            lblCurrentFile.Content = selectedbox.ToolTip;

            bitmap = null;


        }

        private void btnLookIn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();

            _currentDirectory = dialog.FileNames.First() + @"\";
            lblCurrentLocation.Content = _currentDirectory;

            LoadImages();


        }

        private void btnAddMoveLocation_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();

            cmbMoveToLocation.Items.Add(dialog.FileNames.First() + @"\"); 

        }


        struct MoveImages
        {
            public string MoveFrom;
            public string MoveTo;

        }


        private void btnMoveImageTo_Click(object sender, RoutedEventArgs e)
        {
            
            //List<MoveImages> imagestomove = new List<MoveImages>();
            
            foreach (var loopitem in Thumbnails.Items)
            {
                Grid item = (Grid) loopitem;

                if (((CheckBox) item.Children[1]).IsChecked==true)
                {
                    string MoveFile = ((Image)item.Children[0]).ToolTip.ToString();
                    string FromFullLocation = _currentDirectory + "\\"  + MoveFile;
                    string MoveToFullLocation = cmbMoveToLocation.SelectedValue.ToString() + MoveFile;

                    if (File.Exists(MoveToFullLocation))
                        MoveToFullLocation = MoveToFullLocation.Replace(".", $"_{new Random().Next(0, 234234234).ToString()}_.");

                    File.Move(FromFullLocation, MoveToFullLocation);

                    //imagestomove.Add(new MoveImages() {MoveFrom = FromFullLocation,MoveTo = MoveToFullLocation});
                    
                }
                
             }
            Thumbnails.Items.Clear();

            //foreach (var movefile in imagestomove)
            //{
            //    File.Move(movefile.MoveFrom, movefile.MoveTo);
            //}

            LoadImages();

        }

        private void btnDeleteMoveTo_Click(object sender, RoutedEventArgs e)
        {
            foreach (var loopitem in Thumbnails.Items)
            {
                Grid item = (Grid)loopitem;

                if (((CheckBox) item.Children[1]).IsChecked == true)
                {
                    string MoveFile = ((Image)item.Children[0]).ToolTip.ToString();
                    FileSystem.DeleteFile(MoveFile, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
            }
    }
}
