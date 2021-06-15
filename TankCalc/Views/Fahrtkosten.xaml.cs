using System;
using System.Globalization;
using System.IO;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using Color = Windows.UI.Color;
using Page = Windows.UI.Xaml.Controls.Page;
using TextChangedEventArgs = Windows.UI.Xaml.Controls.TextChangedEventArgs;


// Die Elementvorlage wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace TankCalc.Views
{
    public sealed partial class Fahrtkosten : Page
    {
        private string key =
            "KXqyLbZVARd83es5oMsK~5-tIBsrqh802DO3xzYZRew~Al5Ns_-9mCgBcDDCPjqUlKbyolDU4sOegJf8bdgQ0aEymfe-SKZjbFmSgwF4UArE";

        public Fahrtkosten()
        {
            this.InitializeComponent();
        }

        #region Main-Methode-Fahrtkosten

        private void fahrtCalc_Click(object sender, RoutedEventArgs e)
        {
            if (checkCorrect())
            {
                //Deklarierung der TextBox Einträge (Wenn möglich parsen, wenn nicht Standardeingaben wie 0 oder 1.

                double kilo = double.Parse(entfernung.Text);
                double liter = double.Parse(verbrauch.Text);
                double preis = double.Parse(this.preis.Text);
                double sonstige = double.TryParse(this.sonstige.Text, out sonstige) ? sonstige : 0;
                int personenAnzahl = int.TryParse(this.personen.Text, out personenAnzahl) ? personenAnzahl : 1;
                bool abnutzung = (bool)this.abnutzung.IsChecked;
                bool rueckweg = (bool)this.back.IsChecked;

                //Berechnung des Spritkosten + sonstige Kosten = Fahrtkosten

                //Wenn Rückweg eingecheckt mit 2x Kilometer weiterrechnen
                double literverbrauch = ((rueckweg ? kilo * 2 : kilo) * liter)/100;
                double spritkosten = literverbrauch * preis;

                //Abnutzung gecheckt, wenn ja auch Rückweg Checkbox prüfen -> 2x Abnutzung
                double zwischensumme = spritkosten + sonstige + (abnutzung ? (kilo * 0.05 *(rueckweg?2:1)) : 0); 
                double gesamt = Math.Round(zwischensumme / personenAnzahl, 2);


                if (gesamt > 0 && gesamt.ToString() != String.Empty)
                {
                    result.Text = "";
                    result.Text = result.Text + " Bei " + personenAnzahl +
                                  " Personen: \n Spritverbrauch: " +
                                  Math.Round(spritkosten /personenAnzahl,2) + " Euro pro Person \n Insgesamte Fahrtkosten: " + gesamt + " Euro pro Person";
                    resultGrid.Background = new Windows.UI.Xaml.Media.SolidColorBrush(Color.FromArgb(255, 58, 249, 75));
                }
                else
                {
                    result.Text = "";
                    result.Text = result.Text + " Fehler bei der Berechnung!";
                    resultGrid.Background = new Windows.UI.Xaml.Media.SolidColorBrush(Color.FromArgb(255, 248, 68, 68));
                }
            }
            else
            {
                ErrorButton("Falsche Eingabe",
                    "Überprüfen Sie ihre Eingaben!", "Schließen");
            }

        }

        #endregion

        #region Distanzberechnung

        //Button zur Distanzkalkulation zwischen 2 Orten -> getDistance();
        private void calcDistance_Click(object sender, RoutedEventArgs e)
        {
            if (checkCityCorrect())
            {
                String origin = this.origin.Text;
                String dest = this.destination.Text;

                entfernung.Text = Math.Round(getDistance(origin, dest), 2).ToString();
            }
        }

        /* Zur Distanzkalkulation zwischen 2 Orten
           Benutzt wird BingMaps API, JSON Ausgabe wird requested 
           -> response wird durchgearbeitet bis Attribut travelDistance gefunden wird
           Wert des Attributs wird als double ausgegeben!

            Angelehnt an der Methode zur Berechnung des Distanz beim Google Maps API:
            https://stackoverflow.com/questions/30666977/find-the-distance-between-two-places-on-the-map-and-the-car-route-in-c-sharp-or
        */
        private double getDistance(String origin, String destination)
        {
            double distance = 0;

            //REST - Service requestURL von Bing
            string url =
                string.Format(
                    "http://dev.virtualearth.net/REST/V1/Routes/Driving?o=json&wp.0={0}&wp.1={1}&avoid=minimizeTolls&key={2}",
                    origin, destination, key);

            string requesturl = url;

            //JSON 
            string content = fileGetContents(requesturl);
            JObject output = JObject.Parse(content);
            try
            {
                distance = (double)output.SelectToken("resourceSets[0].resources[0].routeLegs[0].travelDistance");
                return distance;
            }
            catch
            {
                return 0;
            }
        }


        //Sobald Inhalt vom Textfeld geändert, überprüfung per onlyLetters()

        private void origin_TextChanged(object sender, TextChangedEventArgs e)
        {
            onlyLetters(origin);
        }

        //Sobald Inhalt vom Textfeld geändert, überprüfung per onlyLetters()

        private void destination_TextChanged(object sender, TextChangedEventArgs e)
        {
            onlyLetters(destination);
        }
        #endregion

        #region getJetzigesLocation

        private Geolocator loc;
        private String longitude;
        private String latitude;

        /* Per GeoLocator Logitude und Latitude herausfinden
           Zur Beginn ein RequestAccess an Benutzer schicken und je nach Antwort, kann die Funktion benutzt werden!  */
        private async void locationButton_Click(object sender, RoutedEventArgs e)
        {
                var accessStatus = await Geolocator.RequestAccessAsync();

                if (accessStatus == GeolocationAccessStatus.Allowed)
                {
                    loc = new Geolocator();
                    Geoposition pos = await loc.GetGeopositionAsync();
                    longitude = pos.Coordinate.Point.Position.Longitude.ToString(new CultureInfo("en-US"));
                    latitude = pos.Coordinate.Point.Position.Latitude.ToString(new CultureInfo("en-US"));

                    origin.Text = getLocation(latitude, longitude);
                }
                else
                {
                    ErrorButton("Keine Rechte", "Die App kann nicht auf ihren Standort zugreifen", "Schließen");
                }
        }
    

        /* getLocation Methode benutzt den REST Service, um
           mithilfe des Longitudes und Latitudes den jetzigen Standort (Stadt) herauszufinden!
        */
        private string getLocation(String lat, String longi)
        {

            string Location = "NaN";

            //REST - Service requestURL von Bing
            string url =
                string.Format(
                    "http://dev.virtualearth.net/REST/v1/Locations/{0},{1}?o=json&key={2}",
                    lat, longi, key);

            string requesturl = url;

            //JSON 
            string content = fileGetContents(requesturl);
            JObject output = JObject.Parse(content);
            try
            {
                //Pfad zum Attribut
                Location = (string) output.SelectToken("resourceSets[0].resources[0].address.locality");
                return Location;
            }
            catch
            {
                return "NaN";
            }
        }

        #endregion

        #region getLatestDurchschnitssVerbrauch
        private void lastVerbrauch_Click(object sender, RoutedEventArgs e)
        {
            Datenholen();
        }

        private async void Datenholen()
        {
            StorageFolder Ordner = ApplicationData.Current.LocalFolder;
            bool fileExist = File.Exists(Ordner.Path + @"\" + "verbrauch.txt");
            if (fileExist)
            {
                StorageFile Datendatei = await Ordner.GetFileAsync("verbrauch.txt");
                string inhalt = await FileIO.ReadTextAsync(Datendatei);
                verbrauch.Text = inhalt;
            }
            else
            {
                ErrorButton("Keine Einträge", "Es wurden keine alten Einträge zum Durchschnittswert gefunden", "Schließen");
            }
        }
        #endregion

        #region Nebenfunktionen

        //Gibt den letzten Buchstabe vom TextBox aus
        private string lastLetter(TextBox t)
        {
            string s = t.Text;

            if (s.Length > 1)
            {
                s = s.Substring(s.Length - 1, 1);
            }

            return s;
        }

        //Mit ReGex nur Buchstaben und Komma zulassen, überprüft, ob das zuletzt eingegebene Zeichen ein Buchstabe ist
        private void onlyLetters(TextBox t)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(lastLetter(t), "^[a-zA-ZäÄöÖüÜß ,]"))
            {
                if (t.Text != String.Empty)
                {
                    t.Text = t.Text.Remove(t.Text.Length - 1);
                    t.SelectionStart = t.Text.Length;
                }

            }
        }

        //Mit ReGex nur Zahlen, Punkt und Komma zulassen

        private void onlyDigits(TextBox t)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(t.Text, "^[0-9 .,]"))
            {
                ErrorButton("Falsche Eingabe ", "Dieser Textbox lässt keine Nummern zu!", "Schließen");
                if (t.Text != String.Empty)
                    t.Text = t.Text.Remove(t.Text.Length - 1);
            }
        }

        //Überprüfung, ob Textfelder leer

        private bool checkCityCorrect()
        {
            if (origin.Text == String.Empty || destination.Text == String.Empty)
            {
                ErrorButton("Falsche Eingabe", "Startort und Zielort dürfen zur Berechnung nicht leer sein!",
                    "Schließen");
                return false;
            }

            return true;
        }

        //Überprüfung ob falsche/nicht prozessbare Eingaben -> wenn ja return true; 

        private bool checkCorrect()
        {
            double i;
            if (!double.TryParse(personen.Text, out i))
            {
                return false;
            }
            else if (double.Parse(personen.Text) < 1)
            {
                return false;
            }


            if (!double.TryParse(sonstige.Text, out i))
            {
                return false;
            }
            else if (double.Parse(sonstige.Text) < 0)
            {
                return false;
            }

            if (!double.TryParse(entfernung.Text, out i) ||
                    !double.TryParse(verbrauch.Text, out i) || !double.TryParse(preis.Text, out i))
            {
                return false;
            }
            else if (double.Parse(entfernung.Text) <= 0 ||
                     double.Parse(verbrauch.Text) <= 0 || double.Parse(preis.Text) <= 0)
            {
                return false;
            }


            if (String.IsNullOrEmpty(entfernung.Text.ToString()) ||
                     String.IsNullOrEmpty(verbrauch.Text.ToString()) || String.IsNullOrEmpty(preis.Text.ToString()))
            {
                return false;
            }

            return true;
        }


        //MessageBox - Fehlermeldung

        private async void ErrorButton(string Titel, string Meldung, string buttonText)
        {
            ContentDialog wrong = new ContentDialog
            {
                Title = Titel,
                Content = Meldung,
                CloseButtonText = buttonText
            };

            ContentDialogResult result = await wrong.ShowAsync();
        }

        //JSON Datei von BingMaps downloaden & compilen als sContents-Typ
        protected string fileGetContents(string fileName)
        {
            string sContents = string.Empty;
            string me = string.Empty;
            try
            {
                //Wenn JSON als Seite ausgegeben, soll dieser Code es runterladen und zu einem String compilen.
                if (fileName.ToLower().IndexOf("http:") > -1) 
                {
                    System.Net.WebClient wc = new System.Net.WebClient();
                    byte[] response = wc.DownloadData(fileName);
                    sContents = System.Text.Encoding.ASCII.GetString(response);

                }
                else //Wenn JSON als File mit Dateipfad übergeben
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(fileName);
                    sContents = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch
            {
                sContents = "Keine Verbindung zum Server!";
            }

            return sContents;
        }





        #endregion
    }
}
    
