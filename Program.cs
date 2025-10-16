using System;
using System.Collections.Generic;
using System.IO;

class Bier
{
    public string Name { get; set; }
    public double Preis { get; set; }
    public int Bestand { get; set; }
}

class Program
{
    // Liste, in der alle Biere gespeichert werden
    static List<Bier> biere = new List<Bier>();

    // Der Speicherort für unsere Datei
    static string dateiPfad = "biere.txt";

    static void Main()
    {
        // Beim Start alle vorhandenen Daten laden
        LadeDaten();

        bool läuft = true;
        while (läuft)
        {
            Console.WriteLine("\n🍺 Brauerei-Verwaltung");
            Console.WriteLine("1 - Neues Bier anlegen");
            Console.WriteLine("2 - Biere anzeigen");
            Console.WriteLine("3 - Bier verkaufen");
            Console.WriteLine("4 - Daten speichern");
            Console.WriteLine("5 - Beenden");
            Console.Write("Auswahl: ");
            string auswahl = Console.ReadLine();

            switch (auswahl)
            {
                case "1":
                    NeuesBier();
                    break;
                case "2":
                    ZeigeBiere();
                    break;
                case "3":
                    VerkaufeBier();
                    break;
                case "4":
                    SpeichereDaten();
                    break;
                case "5":
                    // Vor dem Beenden nochmal speichern
                    SpeichereDaten();
                    läuft = false;
                    break;
                default:
                    Console.WriteLine("Ungültige Eingabe!");
                    break;
            }
        }

        Console.WriteLine("\nProgramm beendet. Prost!");
    }

    // Neues Bier anlegen
    static void NeuesBier()
    {
        Console.Write("Name des Bieres: ");
        string name = Console.ReadLine();

        Console.Write("Preis (z. B. 2,5): ");
        double preis = Convert.ToDouble(Console.ReadLine());

        Console.Write("Bestand in Flaschen: ");
        int bestand = Convert.ToInt32(Console.ReadLine());

        biere.Add(new Bier { Name = name, Preis = preis, Bestand = bestand });
        Console.WriteLine("✅ Bier wurde hinzugefügt!");
    }

    // Alle Biere anzeigen
    static void ZeigeBiere()
    {
        if (biere.Count == 0)
        {
            Console.WriteLine("Keine Biere vorhanden!");
            return;
        }

        Console.WriteLine("\n📋 Aktuelle Biere:");
        foreach (var b in biere)
        {
            Console.WriteLine($"{b.Name} - {b.Preis} Euro - Bestand: {b.Bestand} Flaschen");
        }
    }

    // Verkauf mit Rabatt ab 100 €
    static void VerkaufeBier()
    {
        Console.Write("Name des Bieres: ");
        string name = Console.ReadLine();

        Bier bier = biere.Find(b => b.Name.ToLower() == name.ToLower());
        if (bier == null)
        {
            Console.WriteLine("❌ Bier nicht gefunden!");
            return;
        }

        Console.Write("Wie viele Flaschen verkaufen? ");
        int anzahl = Convert.ToInt32(Console.ReadLine());

        if (anzahl > bier.Bestand)
        {
            Console.WriteLine("❌ Nicht genug Bestand!");
        }
        else
        {
            bier.Bestand -= anzahl;
            double gesamt = bier.Preis * anzahl;

            // Rabatt ab 100 €
            if (gesamt >= 100)
            {
                double rabatt = gesamt * 0.10;
                gesamt -= rabatt;
                Console.WriteLine($"💰 10% Rabatt gewährt ({rabatt:F2} Euro)!");
            }

            Console.WriteLine($"✅ Verkauf erfolgreich! Einnahme: {gesamt:F2} Euro");
        }
    }

    // Daten in Datei schreiben
    static void SpeichereDaten()
    {
        using (StreamWriter writer = new StreamWriter(dateiPfad))
        {
            foreach (var b in biere)
            {
                writer.WriteLine($"{b.Name};{b.Preis};{b.Bestand}");
            }
        }
        Console.WriteLine("💾 Daten gespeichert!");
    }

    // Daten beim Start laden
    static void LadeDaten()
    {
        if (!File.Exists(dateiPfad))
        {
            Console.WriteLine("Keine gespeicherten Daten gefunden.");
            return;
        }

        string[] zeilen = File.ReadAllLines(dateiPfad);
        foreach (string zeile in zeilen)
        {
            string[] teile = zeile.Split(';');
            if (teile.Length == 3)
            {
                biere.Add(new Bier
                {
                    Name = teile[0],
                    Preis = Convert.ToDouble(teile[1]),
                    Bestand = Convert.ToInt32(teile[2])
                });
            }
        }

        Console.WriteLine("📂 Daten wurden geladen.");
    }
}
