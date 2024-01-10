using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfAppRegex.Model;

namespace WpfAppRegex
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string line = "";
        public MainWindow()
        {
            InitializeComponent();
        }
        
        string WebClient()
        {
            using (WebClient wc = new WebClient())
                return wc.DownloadString("https://api.tutiempo.net/json/?lan=en&apid=zwD4zqqaq4zM5oj&lid=31493");
        }
         void RegexButton_Click(object sender, RoutedEventArgs e)
         {
            line = WebClient();

            string date = string.Format("{0:yyyy-m-d}", DateTime.Now.ToString("O"));
            DateTime dateTime = DateTime.Parse(date);
            try
            {
                if (DateTime.TryParseExact(dayDataBox.Text, dayDataBox.Text, null,
                                      DateTimeStyles.None, out dateTime))
                {
                    if (Regex.IsMatch(dayDataBox.Text, "[^0-9--]") ||
                        dateTime <= DateTime.Parse(dayDataBox.Text) &&
                        dateTime.AddDays(6) >= DateTime.Parse(dayDataBox.Text))
                    {
                        Match match = Regex.Match(line,
                        $"\"name\":\"(.*?)\",(.*?)" +
                        $"\"day1\":(.*?)" +
                        $"\"date\":\"{dayDataBox.Text}\",(.*?)" +
                        $"\"temperature_max\":(.*?),(.*?)" +
                        $"\"temperature_min\":(.*?)," +
                        $"\"icon\":\"(.*?)\",(.*?)" +
                        $"\"text\":\"(.*?)\"," +
                        $"(.*?)\"humidity\":(.*?)," +
                        $"(.*?)\"wind\":(.*?),(.*?)" +
                        $"\"wind_direction\":\"(.*?)\",(.*?)" +
                        $"\"icon_wind\":\"(.*?)\",(.*?)" +
                        $"\"sunrise\":\"(.*?)\",(.*?)" +
                        $"\"sunset\":\"(.*?)\",(.*?)" +
                        $"\"moonrise\":\"(.*?)\",(.*?)" +
                        $"\"moonset\":\"(.*?)\",(.*?)" +
                        $"\"moon_phases_icon\":\"(.*?)\"");

                        icoDayWeather.Source = new BitmapImage(new Uri($"/Resources/{match.Groups[8].Value}.png", UriKind.Relative));
                        NameCity.Content = $"{match.Groups[1].Value}";
                        Temp.Content = $"{match.Groups[5].Value}'";
                        Text.Content = $"{match.Groups[10].Value} \n" +
                                       $"feels {match.Groups[7].Value}' \n";
                    }
                    else
                    {
                        MessageBox.Show("Только цифры, прогноз до 7 дней");
                        dayDataBox.Text = dayDataBox.Text.Remove(dayDataBox.Text.Length - 1);
                        dayDataBox.SelectionStart = dayDataBox.Text.Length;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            line = WebClient();
            ObservableCollection<HourDataModel> hourData = new ObservableCollection<HourDataModel>();
            for (int hour = 1; hour <= 24; hour++)
            {
                Match match = Regex.Match(line, 
                    $"\"hour{hour}\":(.*?)," +
                    $"\"icon\":\"(.*?)\",(.*?)" +
                    $"\"hour_data\":\"(.*?)\",(.*?)" +
                    $"\"temperature\":(.*?),");
                hourData.Add(new HourDataModel
                {
                    Name = match.Groups[4].Value,
                    IcoWeather = $"/Resources/{match.Groups[2].Value}.png",
                    Temp = match.Groups[6].Value,
                });
            }
            ListHourWeather.ItemsSource = hourData;
        }
    }
}
