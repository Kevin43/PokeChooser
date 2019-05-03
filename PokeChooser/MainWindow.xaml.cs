using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using Colors;

namespace PokeChooser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        WebClient webClient = new WebClient();
        string path = Directory.GetCurrentDirectory() + "\\PokeImages\\";
        int pokesCount = 0;
        MainWindow mw = (MainWindow)Application.Current.MainWindow;
        List<PokeGrid> pokelistItems = new List<PokeGrid>();
        List<PokeGrid> blockedlistItems = new List<PokeGrid>();
        bool savedFile = true;

        PokeGrid currentPokemon;

        public MainWindow()
        {
            InitializeComponent();
            PopulateList();
            pokesCount = PokeList.Items.Count;
            PokesLeft.Text = String.Format("Pokemon Left:\n {0}", pokesCount);
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (savedFile == true)
            {
                MessageBoxResult result = MessageBox.Show("Really close? You may need to save.", "Warning", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        public class Pokemon
        {
            [JsonProperty("id")]
            public int id { get; set; }
            [JsonProperty("name")]
            public string name { get; set; }
            [JsonProperty("isNfe")]
            public bool isNfe { get; set; }
            [JsonProperty("types")]
            public string[] types { get; set; }
            [JsonProperty("forms", NullValueHandling=NullValueHandling.Ignore)]
            public Forms[] forms { get; set; }
        }

        public class PokeGrid
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Forms
        {
            [JsonProperty("name")]
            public string name { get; set; }
            [JsonProperty("types")]
            public string[] types { get; set; }
            [JsonProperty("spriteSuffix", NullValueHandling=NullValueHandling.Ignore)]
            public string spriteSuffix { get; set; }
            [JsonProperty("isMega", NullValueHandling = NullValueHandling.Ignore)]
            public bool isMega { get; set; }
        }

        public class PokeRoot
        {
            public Dictionary<int, Pokemon> PokeDict { get; set; }
        }

        void LoadJson()
        {
            var pokes = JsonConvert.DeserializeObject<Pokemon[]>(File.ReadAllText("all.json"));
            foreach(var f in pokes)
            {
                //Console.WriteLine("ID = {0} NAME = {1}", f.id, f.name);
                //PokeList.Items.Add(f.name);
                //PokeList.Items.Add(new PokeGrid { Id = f.id, Name = f.name });
                pokelistItems.Add(new PokeGrid { Id = f.id, Name = f.name });
            }
        }

        private void PokeBlockedList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //block button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (PokeList.SelectedItem != null)
            {
                PokeGrid pokemon = PokeList.SelectedItem as PokeGrid;
                //PokeBlockedList.Items.Add(PokeList.SelectedItem);
                blockedlistItems.Add(pokemon);
                PokeBlockedList.ClearValue(ItemsControl.ItemsSourceProperty);
                PokeBlockedList.ItemsSource = blockedlistItems;
                //PokeList.Items.Remove(PokeList.SelectedItem);
                pokelistItems.Remove(pokemon);
                PokeList.ClearValue(ItemsControl.ItemsSourceProperty);
                PokeList.ItemsSource = pokelistItems;
                pokesCount -= 1;
                PokesLeft.Text = String.Format("Pokemon Left:\n {0}", pokesCount);
                RerollPokemon();
            }
            else
            {
                if (currentPokemon != null)
                {
                    Console.WriteLine("unselected item, blocking current poke");
                    //PokeBlockedList.Items.Add(PokeName.Text);
                    blockedlistItems.Add(currentPokemon);
                    //PokeList.Items.Remove(PokeName.Text);
                    pokelistItems.Remove(currentPokemon);
                    PokeBlockedList.ClearValue(ItemsControl.ItemsSourceProperty);
                    PokeBlockedList.ItemsSource = blockedlistItems;
                    PokeList.ClearValue(ItemsControl.ItemsSourceProperty);
                    PokeList.ItemsSource = pokelistItems;
                    pokesCount -= 1;
                    PokesLeft.Text = String.Format("Pokemon Left:\n {0}", pokesCount);
                    Console.WriteLine(currentPokemon.Name);
                    RerollPokemon();
                }
            }
            savedFile = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RerollPokemon();
        }

        void RerollPokemon()
        {
            Random random = new Random();
            int num = random.Next(0, PokeList.Items.Count);
            //Console.WriteLine(PokeList.Items.Count);
            PokeGrid poke = PokeList.Items.GetItemAt(num) as PokeGrid;
            //Console.WriteLine(poke);
            currentPokemon = poke;
            PokeName.Text = poke.Name;

            GetPokeImage(poke.Name);
           
            GC.Collect();
            //Console.WriteLine(path);
            //PokeImage.Source
        }

        void PopulateList()
        {
            GridView myGridView = new GridView
            {
                AllowsColumnReorder = false,
                ColumnHeaderToolTip = "Pokemon"
            };

            GridViewColumn gvc1 = new GridViewColumn
            {
                DisplayMemberBinding = new Binding("Id"),
                Header = "Id",
                Width = 50
            };
            myGridView.Columns.Add(gvc1);
            GridViewColumn gvc2 = new GridViewColumn
            {
                DisplayMemberBinding = new Binding("Name"),
                Header = "Name",
                Width = 100
            };
            myGridView.Columns.Add(gvc2);
            PokeList.View = myGridView;

            GridView blockedView = new GridView
            {
                AllowsColumnReorder = false,
                ColumnHeaderToolTip = "Blocked Pokemon"
            };

            GridViewColumn bvc1 = new GridViewColumn
            {
                DisplayMemberBinding = new Binding("Id"),
                Header = "Id",
                Width = 50
            };
            blockedView.Columns.Add(bvc1);
            GridViewColumn bvc2 = new GridViewColumn
            {
                DisplayMemberBinding = new Binding("Name"),
                Header = "Name",
                Width = 100
            };
            blockedView.Columns.Add(bvc2);
            PokeBlockedList.View = blockedView;

            
            if (File.Exists(Directory.GetCurrentDirectory() + @"\list.json"))
            {
                PokeGrid[] output = JsonConvert.DeserializeObject<PokeGrid[]>(File.ReadAllText("list.json"));
                
                foreach (var f in output)
                {
                    //PokeList.Items.Add(f.ToString());
                    //PokeList.Items.Add(new PokeGrid { Id = f.Id, Name = f.Name});
                    pokelistItems.Add(new PokeGrid { Id = f.Id, Name = f.Name });
                }
                //PokeList.ClearValue(ItemsControl.ItemsSourceProperty);
                PokeList.ItemsSource = pokelistItems;
               // CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(PokeList.ItemsSource);
                //view.SortDescriptions.Add(new SortDescription("Id", System.ComponentModel.ListSortDirection.Ascending));
            }
            if (File.Exists(Directory.GetCurrentDirectory() + @"\blockedlist.json"))
            {
                PokeGrid[] output = JsonConvert.DeserializeObject<PokeGrid[]>(File.ReadAllText("blockedlist.json"));
                foreach (var f in output)
                {
                    //PokeBlockedList.Items.Add(f.ToString());
                    //PokeBlockedList.Items.Add(new PokeGrid { Id = f.Id, Name = f.Name });
                    blockedlistItems.Add(new PokeGrid { Id = f.Id, Name = f.Name });
                }
                //PokeBlockedList.ClearValue(ItemsControl.ItemsSourceProperty);
                PokeBlockedList.ItemsSource = blockedlistItems;
                //CollectionView blockedSort = (CollectionView)CollectionViewSource.GetDefaultView(PokeBlockedList.ItemsSource);
                //blockedSort.SortDescriptions.Add(new SortDescription("Id", System.ComponentModel.ListSortDirection.Ascending));
                return;
            }
            else if (!File.Exists(Directory.GetCurrentDirectory() + @"\list.json"))
            {
                LoadJson();
                //PokeList.ClearValue(ItemsControl.ItemsSourceProperty);
                PokeList.ItemsSource = pokelistItems;
                //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(PokeList.ItemsSource);
               // view.SortDescriptions.Add(new SortDescription("Id", System.ComponentModel.ListSortDirection.Ascending));
            }
            
        }

        private void PokeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void PokeList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (PokeList.SelectedItem != null )
                {
                    PokeGrid pokemon = PokeList.SelectedItem as PokeGrid;
                    //Console.WriteLine(pokemon.Name);
                    PokeName.Text = pokemon.Name;
                    currentPokemon = pokemon;
                    GetPokeImage(PokeName.Text);
                }
            }
            catch (System.ArgumentOutOfRangeException)
            {
                Console.WriteLine("argument out of range");
            }
        }

        private void PokeBlockedList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //PokeList.Items.Add(PokeBlockedList.SelectedItem.ToString());
            PokeGrid pokemon = PokeBlockedList.SelectedItem as PokeGrid;
            blockedlistItems.Remove(pokemon);
            PokeBlockedList.ClearValue(ItemsControl.ItemsSourceProperty);
            PokeBlockedList.ItemsSource = blockedlistItems;
            pokelistItems.Add(pokemon);
            PokeList.ClearValue(ItemsControl.ItemsSourceProperty);
            PokeList.ItemsSource = pokelistItems;
            pokesCount += 1;
            PokesLeft.Text = String.Format("Pokemon Left:\n {0}", pokesCount);
            savedFile = false;
        }

        void GetPokeImage(string poke)
        {
            switch (poke)
            {
                case "Nidoran ♀":
                    poke = "nidoran-f";
                    break;

                case "Nidoran ♂":
                    poke = "nidoran-m";
                    break;

                case "Type: Null":
                    poke = "type-null";
                    break;

                case "Tapu Bulu":
                    poke = "tapu-bulu";
                    break;

                case "Tapu Fini":
                    poke = "tapu-fini";
                    break;

                case "Tapu Koko":
                    poke = "tapu-koko";
                    break;

                case "Flabébé":
                    poke = "flabebe";
                    break;

                case "Mr. Mime":
                    poke = "mr-mime";
                    break;

                case "Giratina":
                    poke = "giratina-altered";
                    break;
            }
            /*if (poke == "Nidoran ♀")
            {
                poke = "nidoran-f";
            }
            else if (poke == "Nidoran ♂")
            {
                poke = "nidoran-m";
            }
            else if (poke == "Type: Null")
            {
                poke = "type-null";
            }
            else if (poke == "Tapu Bulu")
            {
                poke = "tapu-bulu";
            }
            else if (poke == "Tapu Fini")
            {
                poke = "tapu-fini";
            }
            else if (poke == "Tapu Koko")
            {
                poke = "tapu-koko";
            }*/
            bool exists = Directory.Exists(path);
            string remotePath = "https://img.pokemondb.net/artwork/" + poke.ToLower() + ".jpg";
            if (!exists)
            {
                Directory.CreateDirectory(path);
            }
            try
            {
                string localPath = path + poke.ToLower() + ".png";
                bool imageExists = File.Exists(localPath);
                if (!imageExists)
                {
                    webClient.DownloadFile(remotePath, localPath);
                    Bitmap croppedImg = Crop(new Bitmap(localPath));
                    PokeImage.Source = BitmapConversion.ToWpfBitmap(croppedImg);

                    /*System.Drawing.Color color;
                    color = ColorMath.getDominantColor(croppedImg);
                    //Console.WriteLine(color);
                    System.Windows.Media.Color newColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                    
                    SolidColorBrush brush = new SolidColorBrush(newColor);
                    mw.Background = brush;*/
                }
                else if (imageExists)
                {
                    PokeImage.Source = new BitmapImage(new Uri(localPath));
                }
            }
            catch (WebException)
            {
                Console.WriteLine("Path not found! " + remotePath);
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("wtf something is null, " + e);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string output = AscJson(JsonConvert.SerializeObject(PokeList.Items));
            File.WriteAllText(Directory.GetCurrentDirectory() + @"\list.json", output);
            string blockedOutput = AscJson(JsonConvert.SerializeObject(PokeBlockedList.Items));
            File.WriteAllText(Directory.GetCurrentDirectory() + @"\blockedlist.json", blockedOutput);
            savedFile = true;
        }

        public static Bitmap Crop(Bitmap bmp)
        {
            int w = bmp.Width;
            int h = bmp.Height;

            Func<int, bool> allWhiteRow = row =>
            {
                for (int i = 0; i < w; ++i)
                    if (bmp.GetPixel(i, row).R != 255)
                        return false;
                return true;
            };

            Func<int, bool> allWhiteColumn = col =>
            {
                for (int i = 0; i < h; ++i)
                    if (bmp.GetPixel(col, i).R != 255)
                        return false;
                return true;
            };

            int topmost = 0;
            for (int row = 0; row < h; ++row)
            {
                if (allWhiteRow(row))
                    topmost = row;
                else break;
            }

            int bottommost = 0;
            for (int row = h - 1; row >= 0; --row)
            {
                if (allWhiteRow(row))
                    bottommost = row;
                else break;
            }

            int leftmost = 0, rightmost = 0;
            for (int col = 0; col < w; ++col)
            {
                if (allWhiteColumn(col))
                    leftmost = col;
                else
                    break;
            }

            for (int col = w - 1; col >= 0; --col)
            {
                if (allWhiteColumn(col))
                    rightmost = col;
                else
                    break;
            }

            if (rightmost == 0) rightmost = w; // As reached left
            if (bottommost == 0) bottommost = h; // As reached top.

            int croppedWidth = rightmost - leftmost;
            int croppedHeight = bottommost - topmost;

            if (croppedWidth == 0) // No border on left or right
            {
                leftmost = 0;
                croppedWidth = w;
            }

            if (croppedHeight == 0) // No border on top or bottom
            {
                topmost = 0;
                croppedHeight = h;
            }

            try
            {
                var target = new Bitmap(croppedWidth, croppedHeight);
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(bmp,
                      new RectangleF(0, 0, croppedWidth, croppedHeight),
                      new RectangleF(leftmost, topmost, croppedWidth, croppedHeight),
                      GraphicsUnit.Pixel);
                }
                return target;
            }
            catch (Exception ex)
            {
                throw new Exception(
                  string.Format("Values are topmost={0} btm={1} left={2} right={3} croppedWidth={4} croppedHeight={5}", topmost, bottommost, leftmost, rightmost, croppedWidth, croppedHeight),
                  ex);
            }
        }

        private string AscJson(string json)
        {
            var listOb = JsonConvert.DeserializeObject<List<Pokemon>>(json);
            var desc = listOb.OrderBy(x => x.id);
            return JsonConvert.SerializeObject(desc);
        }
    }

    public static class BitmapConversion
    {

        public static Bitmap ToWinFormsBitmap(this BitmapSource bitmapsource)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(stream);

                using (var tempBitmap = new Bitmap(stream))
                {
                    // According to MSDN, one "must keep the stream open for the lifetime of the Bitmap."
                    // So we return a copy of the new bitmap, allowing us to dispose both the bitmap and the stream.
                    return new Bitmap(tempBitmap);
                }
            }
        }

        public static BitmapSource ToWpfBitmap(this Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
    }
}
