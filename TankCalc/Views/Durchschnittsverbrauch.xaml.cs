using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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


        //Berechnung des Durchschnittsverbrauchs 
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (checkCorrect())
            {
                double kilo = double.Parse(textbox1.Text);
                double liter = double.Parse(textbox2.Text);
                double res = Math.Round((liter * 100) / kilo, 2);
                result.Text = res.ToString();
                SpeichernClick();
            }
            else
            {
                ErrorButton("Fehler", "Falsche oder keine Eingabe! Eingabe darf nur postive Zahlen beinhalten.", "Schließen");
            }
        }


        //Daten zur CO2 Ausstoß hier entnommen: https://www.co2online.de/klima-schuetzen/mobilitaet/auto-co2-ausstoss/
        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            double co = 0;
            if (checkCorrect())
            {
                if (benzin.IsChecked == true)
                {
                    co = 2370;
                }

                if (diesel.IsChecked == true)
                {
                    co = 2650;
                }
                double kilo = double.Parse(textbox1.Text);
                double liter = double.Parse(textbox2.Text);
                double verbrauch = Math.Round((liter * 100) / kilo, 2);
                double ausstos = verbrauch * co;
                result1.Text = ausstos.ToString();
                resultprokm.Text = (ausstos / 100).ToString();
            }
            else
            {
                ErrorButton("Fehler", "Falsche oder keine Eingabe! Eingabe darf nur postive Zahlen beinhalten.", "Schließen");
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




        //ButtonSpeichern wird betätigt -> SpeicherFunktion wird aufgerufen
        private void SpeichernClick()
        {
            DatenSichern();
            ErrorButton("Meldung" ,"Daten wurden in Datei geschrieben", "Schließen");
        }

        //Daten werden in einer Datei namens verbrauch.txt gespeichert
        private async void DatenSichern()
        {
            StorageFolder Ordner = ApplicationData.Current.LocalFolder;
            StorageFile Datendatei = await Ordner.CreateFileAsync("verbrauch.txt", CreationCollisionOption.ReplaceExisting);

            string Daten = result.Text;
            if (Daten != "")
            {
                await FileIO.WriteTextAsync(Datendatei, Daten);
            }
        }
    }
}
