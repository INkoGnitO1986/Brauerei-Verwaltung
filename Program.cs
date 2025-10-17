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
    static List<Bier> biere = new List<Bier>();
    static string dateiPfad = "biere.txt";
    static bool altdeutsch = false;
    static string sprache = "de"; // "de" = Deutsch, "en" = Englisch

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        LadeDaten();

        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine(" ⚜️   GotterBier Lager   ⚜️");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.Write("Altdeutsch-Modus aktivieren? (j/n): ");
        string antwort = Console.ReadLine()?.Trim().ToLower();
        altdeutsch = (antwort == "j");

        bool läuft = true;
        while (läuft)
        {
            Console.Clear();
            ZeigeMenue();

            string auswahl = Console.ReadLine();

            switch (auswahl)
            {
                case "1": NeuesBier(); break;
                case "2": ZeigeBiere(); break;
                case "3": VerkaufeBier(); break;
                case "4": BearbeiteBier(); break;
                case "5": LöscheBier(); break;
                case "6": SpeichereDaten(); break;
                case "7":
                    SpeichereDaten();
                    läuft = false;
                    break;
                case "8":
                    altdeutsch = !altdeutsch;
                    Schreibe(altdeutsch ? Text("alt_on") : Text("alt_off"));
                    break;
                case "9":
                    sprache = (sprache == "de") ? "en" : "de";
                    Schreibe(Text("lang_switched"));
                    break;
                default:
                    Schreibe(Text("invalid"));
                    break;
            }

            Schreibe(Text("continue"));
            Console.ReadKey();
        }

        Schreibe(Text("end"));
    }

    static void ZeigeMenue()
    {
        Console.Clear();

        // Flaggenanzeige in Textform (keine Farben, klarer Stil)
        string flagIcon = sprache == "de"
            ? "[  DE  ]"
            : "[  EN  ]";

        string titel = altdeutsch
            ? $"⚜️   𝕲𝖔𝖙𝖙𝖊𝖗𝕭𝖎𝖊𝖗 𝕷𝖆𝖌𝖊𝖗   ⚜️   {flagIcon}"
            : $"⚜️   GotterBier Lager   ⚜️   {flagIcon}";

        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine(titel);
        Console.WriteLine("═══════════════════════════════════════════════════════════\n");

        if (sprache == "de")
        {
            if (altdeutsch)
            {
                Console.WriteLine("① - 𝕹𝖊𝖚𝖊𝖘 𝕭𝖎𝖊𝖗 𝖆𝖓𝖑𝖊𝖌𝖊𝖓");
                Console.WriteLine("② - 𝕭 𝖎𝖊𝖗𝖊 𝖆𝖓𝖟𝖊𝖎𝖌𝖊𝖓");
                Console.WriteLine("③ - 𝕭 𝖎𝖊𝖗 𝖛𝖊𝖗𝖐𝖆𝖚𝖋𝖊𝖓");
                Console.WriteLine("④ - 𝕭𝖎𝖊𝖗 𝖇𝖊𝖆𝖗𝖇𝖊𝖎𝖙𝖊𝖓");
                Console.WriteLine("⑤ - 𝕭𝖎𝖊𝖗 𝖑ö𝖘𝖈𝖍𝖊𝖓");
                Console.WriteLine("⑥ - 𝕯𝖆𝖙𝖊𝖓 𝖘𝖕𝖊𝖎𝖈𝖍𝖊𝖗𝖓");
                Console.WriteLine("⑦ - 𝕭𝖊𝖊𝖓𝖉𝖊𝖓");
                Console.WriteLine("⑧ - 𝕾𝖈𝖍𝖗𝖊𝖎𝖇𝖜𝖊𝖎𝖘𝖊 𝖜𝖊𝖈𝖍𝖘𝖊𝖑𝖓");
                Console.WriteLine("⑨ - 𝕾𝖕𝖗𝖆𝖈𝖍𝖊 𝖜𝖊𝖈𝖍𝖘𝖊𝖑𝖓");
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.Write("𝕬𝖚𝖘𝖜𝖆𝖍𝖑: ");
            }
            else
            {
                Console.WriteLine("① - Neues Bier anlegen");
                Console.WriteLine("② - Biere anzeigen");
                Console.WriteLine("③ - Bier verkaufen");
                Console.WriteLine("④ - Bier bearbeiten");
                Console.WriteLine("⑤ - Bier löschen");
                Console.WriteLine("⑥ - Daten speichern");
                Console.WriteLine("⑦ - Beenden");
                Console.WriteLine("⑧ - Schreibweise wechseln");
                Console.WriteLine("⑨ - Sprache wechseln");
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.Write("Auswahl: ");
            }
        }
        else
        {
            if (altdeutsch)
            {
                Console.WriteLine("① - 𝕬𝖉𝖉 𝕭𝖊𝖊𝖗");
                Console.WriteLine("② - 𝕾𝖍𝖔𝖜 𝕭𝖊𝖊𝖗𝖘");
                Console.WriteLine("③ - 𝕾𝖊𝖑𝖑 𝕭𝖊𝖊𝖗");
                Console.WriteLine("④ - 𝕰𝖉𝖎𝖙 𝕭𝖊𝖊𝖗");
                Console.WriteLine("⑤ - 𝕯𝖊𝖑𝖊𝖙𝖊 𝕭𝖊𝖊𝖗");
                Console.WriteLine("⑥ - 𝕾𝖆𝖛𝖊 𝕯𝖆𝖙𝖆");
                Console.WriteLine("⑦ - 𝕰𝖝𝖎𝖙");
                Console.WriteLine("⑧ - 𝕿𝖔𝖌𝖌𝖑𝖊 𝖋𝖔𝖓𝖙");
                Console.WriteLine("⑨ - 𝕾𝖜𝖎𝖙𝖈𝖍 𝖑𝖆𝖓𝖌𝖚𝖆𝖌𝖊");
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.Write("𝕮𝖍𝖔𝖎𝖈𝖊: ");
            }
            else
            {
                Console.WriteLine("① - Add new beer");
                Console.WriteLine("② - Show beers");
                Console.WriteLine("③ - Sell beer");
                Console.WriteLine("④ - Edit beer");
                Console.WriteLine("⑤ - Delete beer");
                Console.WriteLine("⑥ - Save data");
                Console.WriteLine("⑦ - Exit");
                Console.WriteLine("⑧ - Toggle font style");
                Console.WriteLine("⑨ - Switch language");
                Console.WriteLine("\n═══════════════════════════════════════════════════════════");
                Console.Write("Choice: ");
            }
        }
    }

    // ===== FUNKTIONEN =====

    static void NeuesBier()
    {
        Schreibe(Text("beer_name"));
        string name = Console.ReadLine();

        Schreibe(Text("beer_price"));
        double preis = Convert.ToDouble(Console.ReadLine());

        Schreibe(Text("beer_stock"));
        int bestand = Convert.ToInt32(Console.ReadLine());

        biere.Add(new Bier { Name = name, Preis = preis, Bestand = bestand });
        Schreibe(Text("beer_added"));
    }

    static void ZeigeBiere()
    {
        if (biere.Count == 0)
        {
            Schreibe(Text("no_beer"));
            return;
        }

        Schreibe(Text("beer_list"));
        foreach (var b in biere)
        {
            Schreibe($"{b.Name} - {b.Preis} € - {Text("stock")}: {b.Bestand}");
        }
    }

    static void VerkaufeBier()
    {
        Schreibe(Text("beer_name"));
        string name = Console.ReadLine();

        Bier bier = biere.Find(b => b.Name.ToLower() == name.ToLower());
        if (bier == null)
        {
            Schreibe(Text("not_found"));
            return;
        }

        Schreibe(Text("sell_amount"));
        int anzahl = Convert.ToInt32(Console.ReadLine());

        if (anzahl > bier.Bestand)
        {
            Schreibe(Text("not_enough"));
        }
        else
        {
            bier.Bestand -= anzahl;
            double gesamt = bier.Preis * anzahl;

            if (gesamt >= 100)
            {
                double rabatt = gesamt * 0.10;
                gesamt -= rabatt;
                Schreibe(Text("discount") + $" ({rabatt:F2} €)!");
            }

            Schreibe(Text("sale_done") + $" {gesamt:F2} €");
        }
    }

    static void BearbeiteBier()
    {
        Schreibe(Text("beer_name"));
        string name = Console.ReadLine();

        Bier bier = biere.Find(b => b.Name.ToLower() == name.ToLower());
        if (bier == null)
        {
            Schreibe(Text("not_found"));
            return;
        }

        Schreibe($"{Text("current")}: {bier.Name} - {bier.Preis} € - {Text("stock")}: {bier.Bestand}");
        Schreibe("1 - " + Text("edit_price"));
        Schreibe("2 - " + Text("edit_stock"));
        Schreibe(Text("choice"));
        string wahl = Console.ReadLine();

        if (wahl == "1")
        {
            Schreibe(Text("new_price"));
            bier.Preis = Convert.ToDouble(Console.ReadLine());
            Schreibe(Text("price_changed"));
        }
        else if (wahl == "2")
        {
            Schreibe(Text("new_stock"));
            bier.Bestand = Convert.ToInt32(Console.ReadLine());
            Schreibe(Text("stock_changed"));
        }
        else
        {
            Schreibe(Text("invalid"));
        }
    }

    static void LöscheBier()
    {
        Schreibe(Text("delete_name"));
        string name = Console.ReadLine();

        Bier bier = biere.Find(b => b.Name.ToLower() == name.ToLower());
        if (bier == null)
        {
            Schreibe(Text("not_found"));
            return;
        }

        biere.Remove(bier);
        Schreibe(Text("deleted") + $" '{bier.Name}'");
    }

    static void SpeichereDaten()
    {
        using (StreamWriter writer = new StreamWriter(dateiPfad))
        {
            foreach (var b in biere)
            {
                writer.WriteLine($"{b.Name};{b.Preis};{b.Bestand}");
            }
        }
        Schreibe(Text("saved"));
    }

    static void LadeDaten()
    {
        if (!File.Exists(dateiPfad))
        {
            Schreibe(Text("no_data"));
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

        Schreibe(Text("loaded"));
    }

    // ===== STIL UND SPRACHE =====

    static void Schreibe(string text)
    {
        if (altdeutsch)
            Console.WriteLine(text
                .Replace("Bier", "𝕭𝖎𝖊𝖗")
                .Replace("Daten", "𝕯𝖆𝖙𝖊𝖓")
                .Replace("Preis", "𝕻𝖗𝖊𝖎𝖘")
                .Replace("Bestand", "𝕭𝖊𝖘𝖙𝖆𝖓𝖉")
                .Replace("Programm", "𝕻𝖗𝖔𝖌𝖗𝖆𝖒𝖒"));
        else
            Console.WriteLine(text);
    }

    static string Text(string key)
    {
        Dictionary<string, (string de, string en)> t = new()
        {
            ["invalid"] = ("Ungültige Eingabe!", "Invalid input!"),
            ["continue"] = ("\nDrücke eine Taste, um fortzufahren...", "\nPress any key to continue..."),
            ["end"] = ("\nProgramm beendet. Prost! 🍺", "\nProgram ended. Cheers! 🍺"),
            ["beer_name"] = ("Name des Bieres:", "Beer name:"),
            ["beer_price"] = ("Preis (z. B. 2,5):", "Price (e.g. 2.5):"),
            ["beer_stock"] = ("Bestand in Flaschen:", "Stock (bottles):"),
            ["beer_added"] = ("Bier wurde hinzugefügt!", "Beer added!"),
            ["no_beer"] = ("Keine Biere vorhanden!", "No beers available!"),
            ["beer_list"] = ("Aktuelle Biere:", "Current beers:"),
            ["stock"] = ("Bestand", "Stock"),
            ["not_found"] = ("Bier nicht gefunden!", "Beer not found!"),
            ["sell_amount"] = ("Wie viele Flaschen verkaufen?", "How many bottles to sell?"),
            ["not_enough"] = ("Nicht genug Bestand!", "Not enough stock!"),
            ["discount"] = ("10% Rabatt gewährt", "10% discount applied"),
            ["sale_done"] = ("Verkauf erfolgreich! Einnahme:", "Sale successful! Total:"),
            ["current"] = ("Aktuell", "Current"),
            ["edit_price"] = ("Preis ändern", "Edit price"),
            ["edit_stock"] = ("Bestand ändern", "Edit stock"),
            ["choice"] = ("Auswahl:", "Choice:"),
            ["new_price"] = ("Neuer Preis:", "New price:"),
            ["new_stock"] = ("Neuer Bestand:", "New stock:"),
            ["price_changed"] = ("Preis wurde geändert!", "Price updated!"),
            ["stock_changed"] = ("Bestand wurde geändert!", "Stock updated!"),
            ["delete_name"] = ("Name des Bieres, das gelöscht werden soll:", "Name of the beer to delete:"),
            ["deleted"] = ("Bier gelöscht!", "Beer deleted!"),
            ["saved"] = ("Daten gespeichert!", "Data saved!"),
            ["no_data"] = ("Keine gespeicherten Daten gefunden.", "No saved data found."),
            ["loaded"] = ("Daten wurden geladen.", "Data loaded."),
            ["alt_on"] = ("Altdeutsche Schreibweise aktiviert!", "Old German font activated!"),
            ["alt_off"] = ("Normale Schreibweise aktiviert!", "Normal font activated!"),
            ["lang_switched"] = ("Sprache gewechselt! 🇩🇪↔🇬🇧", "Language switched! 🇬🇧↔🇩🇪")
        };

        return sprache == "de" ? t[key].de : t[key].en;
    }
}
