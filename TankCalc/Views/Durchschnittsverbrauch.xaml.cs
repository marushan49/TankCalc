using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using System;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace TankCalc.Views
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class Durchschnittsverbrauch : Page
    {
        public Durchschnittsverbrauch()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (checkCorrect())
            {
                double kilo = double.Parse(textbox1.Text);
                double liter = double.Parse(textbox2.Text);
                double res = (liter * 100) / kilo;
                result.Text = res.ToString();
            }
            else
            {
                ErrorButton("Fehler", "Falsche Eingabe! Eingabe darf nur postive Zahlen beinhalten. ", "Schließen");
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {

            if (checkCorrect())
            {
                double kilo = double.Parse(textbox1.Text);
                double liter = double.Parse(textbox2.Text);
                double res = (liter * 100) / kilo;
                result.Text = res.ToString();
            }
            else
            {
               
            }
        }

        //Überprüfung ob kleiner, negative oder keine Zahlen -> wenn ja Fehlermeldung
        private bool checkCorrect()
        {
            double i;
            if (!double.TryParse(textbox1.Text, out i) || !double.TryParse(textbox2.Text, out i))
            {
                return false;
            }
            else if (double.Parse(textbox1.Text) <= 0 || double.Parse(textbox2.Text) <= 0)
            {
                return false;
            }
            return true;
        }

        //MessageBox - Fehlermeldung
        private async void ErrorButton(string Titel, string Meldung, string buttontext)
        {
            ContentDialog wrong = new ContentDialog
            {
                Title = Titel,
                Content = Meldung,
                CloseButtonText = buttontext
            };

            ContentDialogResult result = await wrong.ShowAsync();
        }

        private void SpeichernClick(object sender, RoutedEventArgs e)
        {
            DatenSichern();
            ErrorButton("Meldung" ,"Daten wurden in Datei geschrieben", "Schließen");
        }

        private async void DatenSichern()
        {
            StorageFolder Ordner = ApplicationData.Current.LocalFolder;
            StorageFile Datendatei = await Ordner.CreateFileAsync("beispiel.txt", CreationCollisionOption.ReplaceExisting);

            string Daten = result.Text;
            if (Daten != "")
            {
                await FileIO.WriteTextAsync(Datendatei, Daten);
            }
        }

        private async void Datenholen(object sender, RoutedEventArgs e)
        {
            StorageFolder Ordner = ApplicationData.Current.LocalFolder;
            StorageFile Datendatei = await Ordner.GetFileAsync("beispiel.txt");
            string inhalt = await FileIO.ReadTextAsync(Datendatei);
            inhalt = "gelesen: " + inhalt;
            result.Text = inhalt;
        }
    }
}
