using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

class Bier
{
    public string Name { get; set; } = "";
    public double Preis { get; set; }
    public int Bestand { get; set; }
}

class Benutzer
{
    public string Benutzername { get; set; } = "";
    public string Passwort { get; set; } = "";
    public bool IstAdmin { get; set; } = false;
    public bool IstBraumeister { get; set; } = false;

    public override string ToString() => $"{Benutzername};{Passwort};{IstAdmin};{IstBraumeister}";
    public static bool TryParse(string line, out Benutzer u)
    {
        u = new Benutzer();
        if (string.IsNullOrWhiteSpace(line)) return false;
        var t = line.Split(';');
        if (t.Length != 4) return false;
        u.Benutzername = t[0];
        u.Passwort = t[1];
        u.IstAdmin = bool.TryParse(t[2], out var a) && a;
        u.IstBraumeister = bool.TryParse(t[3], out var b) && b;
        return true;
    }
}

class Program
{
    // Dateien
    static readonly string BiereDatei = "biere.txt";
    static readonly string NutzerDatei = "users.txt";

    // Daten
    static readonly List<Bier> Biere = new();
    static readonly List<Benutzer> Nutzer = new();

    // Status
    static Benutzer? Aktueller;
    static string Sprache = "de"; // "de" | "en"
    static bool Alt = false;      // Altdeutsch (echte Fraktur-Strings)
    static bool Läuft = true;

    static readonly string[] Num = { "①", "②", "③", "④", "⑤", "⑥", "⑦", "⑧", "⑨", "⑩", "⑪" };

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        LadeNutzer();     // stellt Braumeister sicher
        LadeBiere();

        LoginLoop();

        WLn(Text("ask_alt"));
        Alt = (Console.ReadLine()?.Trim().ToLower() == "j");

        while (Läuft)
        {
            Console.Clear();
            ZeigeMenue();

            string a = Console.ReadLine()?.Trim() ?? "";
            bool darfAlles = Aktueller!.IstBraumeister || Aktueller!.IstAdmin;

            switch (a)
            {
                case "1": if (darfAlles) NeuesBier(); else KeineRechte(); break;
                case "2": ZeigeBiere(); break;
                case "3": VerkaufeBier(); break;
                case "4": if (darfAlles) BearbeiteBier(); else KeineRechte(); break;
                case "5": if (darfAlles) LöscheBier(); else KeineRechte(); break;
                case "6": SpeichereBiere(); Info("saved"); break;
                case "7": SpeichereBiere(); Läuft = false; break;
                case "8": Alt = !Alt; Info(Alt ? "alt_on" : "alt_off"); break;
                case "9": Sprache = (Sprache == "de") ? "en" : "de"; Info("lang_switched"); break;
                case "10": Logout(); LoginLoop(); break;
                case "11": if (Aktueller!.IstBraumeister) BenutzerVerwaltung(); else KeineRechte(); break;
                default: Info("invalid"); break;
            }

            if (!Läuft) break;
            WLn(Text("continue"));
            Console.ReadKey(true);
        }

        WLn(Text("end"));
    }

    // ───────────── Login / Registrierung ─────────────
    static void LoginLoop()
    {
        bool ok = false;
        while (!ok)
        {
            Console.Clear();
            Kopf();
            WLn(Text("login_menu"));
            W(Text("choice"));
            string w = Console.ReadLine()?.Trim() ?? "";
            switch (w)
            {
                case "1": ok = Login(); break;
                case "2": Registrieren(); break;
                case "3": Environment.Exit(0); break;
                default: Info("invalid"); break;
            }
        }
    }

    static bool Login()
    {
        Console.Clear(); Kopf();
        W(Text("user")); string u = Console.ReadLine()?.Trim() ?? "";
        W(Text("pass")); string p = ReadHidden();

        foreach (var n in Nutzer)
        {
            if (n.Benutzername.Equals(u, StringComparison.OrdinalIgnoreCase) && n.Passwort == p)
            {
                Aktueller = n; Info("login_ok"); return true;
            }
        }
        Info("login_fail");
        return false;
    }

    static void Registrieren()
    {
        Console.Clear(); Kopf();
        WLn(Text("register"));
        W(Text("user"));
        string u = Console.ReadLine()?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(u) || u.Equals("Braumeister", StringComparison.OrdinalIgnoreCase))
        { Info("invalid"); return; }

        if (Nutzer.Exists(x => x.Benutzername.Equals(u, StringComparison.OrdinalIgnoreCase)))
        { Info("user_exists"); return; }

        W(Text("pass")); string p1 = ReadHidden();
        W(Text("pass2")); string p2 = ReadHidden();
        if (p1 != p2) { Info("pw_mismatch");Console.ReadKey(); return; }

        Nutzer.Add(new Benutzer { Benutzername = u, Passwort = p1, IstAdmin = false, IstBraumeister = false });
        SpeichereNutzer();
        Info("reg_ok");
    }

    static void Logout() { Aktueller = null; Info("logout"); }

    static string ReadHidden()
    {
        var sb = new StringBuilder();
        ConsoleKeyInfo k;
        while (true)
        {
            k = Console.ReadKey(true);
            if (k.Key == ConsoleKey.Enter) { Console.WriteLine(); break; }
            if (k.Key == ConsoleKey.Backspace && sb.Length > 0) { sb.Remove(sb.Length - 1, 1); Console.Write("\b \b"); }
            else if (!char.IsControl(k.KeyChar)) { sb.Append(k.KeyChar); Console.Write("*"); }
        }
        return sb.ToString();
    }

    // ───────────── Benutzerverwaltung (nur Braumeister) ─────────────
    static void BenutzerVerwaltung()
    {
        bool back = false;
        while (!back)
        {
            Console.Clear(); Kopf();
            WLn(Text("user_admin_menu"));
            W(Text("choice"));
            string w = Console.ReadLine()?.Trim() ?? "";

            switch (w)
            {
                case "1": ListeBenutzer(); break;
                case "2": SetAdmin(true); break;
                case "3": SetAdmin(false); break;
                case "4": ResetPass(); break;
                case "5": DeleteUser(); break;
                case "6": back = true; break;
                default: Info("invalid"); break;
            }
            if (!back) { WLn(Text("continue")); Console.ReadKey(true); }
        }
    }

    static void ListeBenutzer()
    {
        WLn("");
        foreach (var n in Nutzer)
            WLn($"- {n.Benutzername} | Admin: {n.IstAdmin} | Braumeister: {n.IstBraumeister}");
    }

    static void SetAdmin(bool promote)
    {
        W(Text("user")); string u = Console.ReadLine()?.Trim() ?? "";
        var n = Nutzer.Find(x => x.Benutzername.Equals(u, StringComparison.OrdinalIgnoreCase));
        if (n == null || n.IstBraumeister) { Info("invalid"); return; }
        n.IstAdmin = promote; SpeichereNutzer();
        WLn(promote ? Text("admin_set") : Text("admin_removed"));
    }

    static void ResetPass()
    {
        W(Text("user")); string u = Console.ReadLine()?.Trim() ?? "";
        var n = Nutzer.Find(x => x.Benutzername.Equals(u, StringComparison.OrdinalIgnoreCase));
        if (n == null || n.IstBraumeister) { Info("invalid"); return; }
        W(Text("newpw")); string p1 = ReadHidden();
        W(Text("pass2")); string p2 = ReadHidden();
        if (p1 != p2) { Info("pw_mismatch"); return; }
        n.Passwort = p1; SpeichereNutzer(); Info("pw_ok");
    }

    static void DeleteUser()
    {
        W(Text("user")); string u = Console.ReadLine()?.Trim() ?? "";
        var n = Nutzer.Find(x => x.Benutzername.Equals(u, StringComparison.OrdinalIgnoreCase));
        if (n == null || n.IstBraumeister) { Info("invalid"); return; }
        Nutzer.Remove(n); SpeichereNutzer(); Info("user_deleted");
    }

    // ───────────── Menü ─────────────
    static void ZeigeMenue()
    {
        string flag = Sprache == "de" ? "[DE]" : "[EN]";
        string name = Aktueller?.Benutzername ?? "—";
        string rolle = Aktueller!.IstBraumeister ? Text("role_master") :
                       Aktueller!.IstAdmin ? Text("role_admin") :
                                                    $"({name})";

        // Titel
        Linie();
        if (Alt)
            WLn($"⚜️   𝔊𝔬𝔱𝔱𝔢𝔯𝔅𝔦𝔢𝔯 𝔏𝔞𝔤𝔢𝔯   ⚜️   {flag} {rolle}");
        else
            WLn($"⚜️   GotterBier Lager   ⚜️   {flag} {rolle}");
        Linie();

        bool admin = Aktueller!.IstBraumeister || Aktueller!.IstAdmin;
        bool chef = Aktueller!.IstBraumeister;

        for (int i = 1; i <= 11; i++)
        {
            bool sichtbar = !(i == 11 && !chef);
            if (!sichtbar) continue;

            string label = Text($"m{i}");     // kommt in richtiger Sprache + Stil
            bool erlaubt = i switch
            {
                1 or 4 or 5 => admin,
                11 => chef,
                _ => true
            };

            Console.ForegroundColor = erlaubt ? ConsoleColor.White : ConsoleColor.DarkGray;
            WLn($"{Num[i - 1]} - {label}" + (erlaubt ? "" : " 🔒"));
            Console.ResetColor();
        }

        Linie();
        W(Text("choice"));
    }

    // ───────────── Bierfunktionen ─────────────
    static void NeuesBier()
    {
        W(Text("beer_name")); string n = Console.ReadLine() ?? "";
        W(Text("beer_price")); double p = Convert.ToDouble(Console.ReadLine());
        W(Text("beer_stock")); int s = Convert.ToInt32(Console.ReadLine());
        Biere.Add(new Bier { Name = n, Preis = p, Bestand = s });
        Info("beer_added");
    }

    static void ZeigeBiere()
    {
        if (Biere.Count == 0) { Info("no_beer"); return; }
        WLn(Text("beer_list"));
        foreach (var b in Biere)
            WLn($"{b.Name} - {b.Preis} € - {Text("stock")}: {b.Bestand}");
    }

    static void VerkaufeBier()
    {
        W(Text("beer_name"));
        string n = Console.ReadLine() ?? "";
        var b = Biere.Find(x => x.Name.Equals(n, StringComparison.OrdinalIgnoreCase));
        if (b == null) { Info("not_found"); return; }

        W(Text("sell_amount"));
        int m = Convert.ToInt32(Console.ReadLine());
        if (m > b.Bestand) { Info("not_enough"); return; }

        b.Bestand -= m;
        double g = b.Preis * m;
        if (g >= 100) { g *= 0.9; WLn(Text("discount")); }
        WLn(Text("sale_done") + $" {g:F2} €");
    }

    static void BearbeiteBier()
    {
        W(Text("beer_name")); string n = Console.ReadLine() ?? "";
        var b = Biere.Find(x => x.Name.Equals(n, StringComparison.OrdinalIgnoreCase));
        if (b == null) { Info("not_found"); return; }

        WLn($"{Text("current")}: {b.Name} - {b.Preis} € - {Text("stock")}: {b.Bestand}");
        WLn("1 - " + Text("edit_price"));
        WLn("2 - " + Text("edit_stock"));
        W(Text("choice"));
        string w = Console.ReadLine() ?? "";
        if (w == "1") { W(Text("new_price")); b.Preis = Convert.ToDouble(Console.ReadLine()); Info("price_changed"); }
        else if (w == "2") { W(Text("new_stock")); b.Bestand = Convert.ToInt32(Console.ReadLine()); Info("stock_changed"); }
        else Info("invalid");
    }

    static void LöscheBier()
    {
        W(Text("delete_name"));
        string n = Console.ReadLine() ?? "";
        var b = Biere.Find(x => x.Name.Equals(n, StringComparison.OrdinalIgnoreCase));
        if (b == null) { Info("not_found"); return; }
        Biere.Remove(b); Info("deleted");
    }

    // ───────────── Datei I/O ─────────────
    static void LadeBiere()
    {
        if (!File.Exists(BiereDatei)) return;
        foreach (var l in File.ReadAllLines(BiereDatei, Encoding.UTF8))
        {
            var t = l.Split(';');
            if (t.Length != 3) continue;
            if (!double.TryParse(t[1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var p)) continue;
            if (!int.TryParse(t[2], out var s)) continue;
            Biere.Add(new Bier { Name = t[0], Preis = p, Bestand = s });
        }
    }

    static void SpeichereBiere()
    {
        using var w = new StreamWriter(BiereDatei, false, Encoding.UTF8);
        foreach (var b in Biere) w.WriteLine($"{b.Name};{b.Preis};{b.Bestand}");
    }

    static void LadeNutzer()
    {
        if (File.Exists(NutzerDatei))
        {
            foreach (var l in File.ReadAllLines(NutzerDatei, Encoding.UTF8))
                if (Benutzer.TryParse(l, out var u)) Nutzer.Add(u);
        }

        // Braumeister sicherstellen
        var bm = Nutzer.Find(n => n.IstBraumeister || n.Benutzername.Equals("Braumeister", StringComparison.OrdinalIgnoreCase));
        if (bm == null)
            Nutzer.Add(new Benutzer { Benutzername = "Braumeister", Passwort = "0815", IstAdmin = true, IstBraumeister = true });
        else { bm.Benutzername = "Braumeister"; bm.Passwort = "0815"; bm.IstAdmin = true; bm.IstBraumeister = true; }

        SpeichereNutzer();
    }

    static void SpeichereNutzer()
    {
        using var w = new StreamWriter(NutzerDatei, false, Encoding.UTF8);
        foreach (var u in Nutzer) w.WriteLine(u);
    }

    // ───────────── Ausgabe-Helfer ─────────────
    static void W(string s) => Console.Write(Alt ? ToAltIfNeeded(s) : s);
    static void WLn(string s) => Console.WriteLine(Alt ? ToAltIfNeeded(s) : s);
    static void Info(string key) => WLn(Text(key));
    static void KeineRechte() => WLn(Sprache == "de" ? "⚠️  Keine Berechtigung!" : "⚠️  No permission!");
    static void Linie() => Console.WriteLine("═══════════════════════════════════════════════════════════");
    static void Kopf() => Kopf("");

    static void Kopf(string extra)
    {
        Linie();
        var baseTitle = Alt ? "⚜️   𝔊𝔬𝔱𝔱𝔢𝔯𝔅𝔦𝔢𝔯 𝔏𝔞𝔤𝔢𝔯   ⚜️" : "⚜️   GotterBier Lager   ⚜️";
        WLn(string.IsNullOrWhiteSpace(extra) ? baseTitle : $"{baseTitle}   {extra}");
        Linie();
    }

    // Für Alt= true werden NUR Texte in Fraktur dargestellt, die wir explizit so hinterlegt haben.
    // Dynamische Teile (Zahlen, Biernamen) bleiben normal; hier keine Vollkonvertierung, um Fehler zu vermeiden.
    static string ToAltIfNeeded(string text) => text;

    // ───────────── Text-Ressourcen (4 Varianten) ─────────────
    static string Text(string key)
    {
        // Deutsch NORMAL
        var de = new Dictionary<string, string>
        {
            ["ask_alt"] = "Altdeutsch-Schrift aktivieren? (j/n):",
            ["invalid"] = "Ungültige Eingabe!",
            ["continue"] = "\nTaste drücken, um fortzufahren...",
            ["end"] = "\nProgramm beendet. Prost! 🍺",
            ["alt_on"] = "Altdeutsch-Schrift aktiviert!",
            ["alt_off"] = "Altdeutsch-Schrift deaktiviert.",
            ["lang_switched"] = "Sprache gewechselt. [DE] ↔ [EN]",
            ["login_menu"] = "① - Einloggen\n② - Registrieren\n③ - Beenden",
            ["login_ok"] = "✅ Erfolgreich eingeloggt!",
            ["login_fail"] = "❌ Benutzername oder Passwort falsch.",
            ["logout"] = "↩️  Abgemeldet.",
            ["register"] = "Neuen Benutzer anlegen:",
            ["user_exists"] = "Benutzer existiert bereits!",
            ["pw_mismatch"] = "Passwörter stimmen nicht überein!",
            ["reg_ok"] = "Benutzer wurde angelegt!",
            ["user_admin_menu"] = "① - Benutzer auflisten\n② - Zum Admin machen\n③ - Adminrechte entziehen\n④ - Passwort zurücksetzen\n⑤ - Benutzer löschen\n⑥ - Zurück",
            ["admin_set"] = "✅ Benutzer ist jetzt Admin.",
            ["admin_removed"] = "✅ Adminrechte entzogen.",
            ["pw_ok"] = "Passwort geändert.",
            ["user_deleted"] = "Benutzer gelöscht.",
            ["role_master"] = "(Braumeister)",
            ["role_admin"] = "(Admin)",
            ["choice"] = "Auswahl: ",
            ["m1"] = "Neues Bier anlegen",
            ["m2"] = "Biere anzeigen",
            ["m3"] = "Bier verkaufen",
            ["m4"] = "Bier bearbeiten",
            ["m5"] = "Bier löschen",
            ["m6"] = "Daten speichern",
            ["m7"] = "Beenden",
            ["m8"] = "Schreibweise wechseln",
            ["m9"] = "Sprache wechseln",
            ["m10"] = "Login wechseln",
            ["m11"] = "Benutzerverwaltung",
            ["beer_name"] = "Name des Bieres: ",
            ["beer_price"] = "Preis: ",
            ["beer_stock"] = "Bestand: ",
            ["beer_added"] = "Bier hinzugefügt!",
            ["no_beer"] = "Keine Biere vorhanden!",
            ["beer_list"] = "Aktuelle Biere:",
            ["stock"] = "Bestand",
            ["not_found"] = "Bier nicht gefunden!",
            ["sell_amount"] = "Wie viele verkaufen? ",
            ["not_enough"] = "Nicht genug Bestand!",
            ["discount"] = "10% Rabatt gewährt.",
            ["sale_done"] = "Verkauf erfolgreich! Einnahme:",
            ["current"] = "Aktuell",
            ["edit_price"] = "Preis ändern",
            ["edit_stock"] = "Bestand ändern",
            ["new_price"] = "Neuer Preis: ",
            ["new_stock"] = "Neuer Bestand: ",
            ["price_changed"] = "Preis geändert!",
            ["stock_changed"] = "Bestand geändert!",
            ["delete_name"] = "Bier löschen (Name): ",
            ["deleted"] = "Bier gelöscht!",
            ["user"] = "Benutzername: ",
            ["pass"] = "Passwort: ",
            ["pass2"] = "Wiederholen: ",
            ["newpw"] = "Neues Passwort: "
        };

        // Deutsch ALTDEUTSCH (echte Fraktur-Strings)
        var de_alt = new Dictionary<string, string>
        {
            ["ask_alt"] = "𝔄𝔩𝔱𝔡𝔢𝔲𝔱𝔰𝔠𝔥-𝔖𝔠𝔥𝔯𝔦𝔣𝔱 𝔞𝔨𝔱𝔦𝔳𝔦𝔢𝔯𝔢𝔫? (j/n):",
            ["invalid"] = "𝔘𝔫𝔤ü𝔩𝔱𝔦𝔤𝔢 𝔈𝔦𝔫𝔤𝔞𝔟𝔢!",
            ["continue"] = "\n𝔗𝔞𝔰𝔱𝔢 𝔡𝔯ü𝔠𝔨𝔢𝔫, 𝔲𝔪 𝔣𝔬𝔯𝔱𝔷𝔲𝔣𝔲𝔥𝔯𝔢𝔫...",
            ["end"] = "\n𝔓𝔯𝔬𝔤𝔯𝔞𝔪𝔪 𝔟𝔢𝔢𝔫𝔡𝔢𝔱. 𝔓𝔯𝔬𝔰𝔱! 🍺",
            ["alt_on"] = "𝔄𝔩𝔱𝔡𝔢𝔲𝔱𝔰𝔠𝔥-𝔖𝔠𝔥𝔯𝔦𝔣𝔱 𝔞𝔨𝔱𝔦𝔳!",
            ["alt_off"] = "𝔑𝔬𝔯𝔪𝔞𝔩𝔢 𝔖𝔠𝔥𝔯𝔦𝔣𝔱 𝔞𝔨𝔱𝔦𝔳.",
            ["lang_switched"] = "𝔖𝔭𝔯𝔞𝔠𝔥𝔢 𝔤𝔢𝔴𝔢𝔠𝔥𝔰𝔢𝔩𝔱. [DE] ↔ [EN]",
            ["login_menu"] = "① - 𝔈𝔦𝔫𝔩𝔬𝔤𝔤𝔢𝔫\n② - 𝔅𝔢𝔫𝔲𝔱𝔷𝔢𝔯 𝔞𝔫𝔩𝔢𝔤𝔢𝔫\n③ - 𝔅𝔢𝔢𝔫𝔡𝔢𝔫",
            ["login_ok"] = "✅ 𝔈𝔯𝔣𝔬𝔩𝔤𝔯𝔢𝔦𝔠𝔥 𝔢𝔦𝔫𝔤𝔢𝔩𝔬𝔤𝔤𝔱!",
            ["login_fail"] = "❌ 𝔉𝔞𝔩𝔰𝔠𝔥𝔢𝔯 𝔅𝔢𝔫𝔲𝔱𝔷𝔢𝔯𝔫𝔞𝔪𝔢 𝔬𝔡𝔢𝔯 𝔓𝔞𝔰𝔰𝔴𝔬𝔯𝔱.",
            ["logout"] = "↩️  𝔄𝔟𝔤𝔢𝔪𝔢𝔩𝔡𝔢𝔱.",
            ["register"] = "𝔑𝔢𝔲𝔢𝔫 𝔅𝔢𝔫𝔲𝔱𝔷𝔢𝔯 𝔞𝔫𝔩𝔢𝔤𝔢𝔫:",
            ["user_exists"] = "𝔅𝔢𝔫𝔲𝔱𝔷𝔢𝔯 𝔢𝔵𝔦𝔰𝔱𝔦𝔢𝔯𝔱 𝔟𝔢𝔯𝔢𝔦𝔱𝔰!",
            ["pw_mismatch"] = "𝔓𝔞𝔰𝔰𝔴ö𝔯𝔱𝔢𝔯 𝔰𝔱𝔦𝔪𝔪𝔢𝔫 𝔫𝔦𝔠𝔥𝔱 ü𝔟𝔢𝔯𝔢𝔦𝔫!",
            ["reg_ok"] = "𝔅𝔢𝔫𝔲𝔱𝔷𝔢𝔯 𝔞𝔫𝔤𝔢𝔩𝔢𝔤𝔱!",
            ["user_admin_menu"] = "① - 𝔅𝔢𝔫𝔲𝔱𝔷𝔢𝔯 𝔞𝔲𝔣𝔩𝔦𝔰𝔱𝔢𝔫\n② - 𝔄𝔡𝔪𝔦𝔫 𝔢𝔯𝔫𝔢𝔫\n③ - 𝔄𝔡𝔪𝔦𝔫 𝔢𝔫𝔱𝔷𝔦𝔢𝔥𝔢𝔫\n④ - 𝔓𝔞𝔰𝔰𝔴𝔬𝔯𝔱 𝔷𝔲𝔯ü𝔠𝔨𝔰𝔢𝔱𝔷𝔢𝔫\n⑤ - 𝔅𝔢𝔫𝔲𝔱𝔷𝔢𝔯 𝔩ö𝔰𝔠𝔥𝔢𝔫\n⑥ - 𝔃𝔲𝔯ü𝔠𝔨",
            ["admin_set"] = "✅ 𝔅𝔢𝔫𝔲𝔱𝔷𝔢𝔯 𝔦𝔰𝔱 𝔫𝔲𝔫 𝔄𝔡𝔪𝔦𝔫.",
            ["admin_removed"] = "✅ 𝔄𝔡𝔪𝔦𝔫𝔯𝔢𝔠𝔥𝔱𝔢 𝔢𝔫𝔱𝔷𝔬𝔤𝔢𝔫.",
            ["pw_ok"] = "𝔓𝔞𝔰𝔰𝔴𝔬𝔯𝔱 𝔤𝔢ä𝔫𝔡𝔢𝔯𝔱.",
            ["user_deleted"] = "𝔅𝔢𝔫𝔲𝔱𝔷𝔢𝔯 𝔤𝔢𝔩ö𝔰𝔠𝔥𝔱.",
            ["role_master"] = "(𝔅𝔯𝔞𝔲𝔪𝔢𝔦𝔰𝔱𝔢𝔯)",
            ["role_admin"] = "(𝔄𝔡𝔪𝔦𝔫)",
            ["choice"] = "𝔄𝔲𝔰𝔴𝔞𝔥𝔩: ",
            ["m1"] = "𝔑𝔢𝔲𝔢𝔰 𝔅𝔦𝔢𝔯 𝔞𝔫𝔩𝔢𝔤𝔢𝔫",
            ["m2"] = "𝔅𝔦𝔢𝔯𝔢 𝔞𝔫𝔷𝔢𝔦𝔤𝔢𝔫",
            ["m3"] = "𝔅𝔦𝔢𝔯 𝔳𝔢𝔯𝔨𝔞𝔲𝔣𝔢𝔫",
            ["m4"] = "𝔅𝔦𝔢𝔯 𝔟𝔢𝔞𝔯𝔟𝔢𝔦𝔱𝔢𝔫",
            ["m5"] = "𝔅𝔦𝔢𝔯 𝔩ö𝔰𝔠𝔥𝔢𝔫",
            ["m6"] = "𝔇𝔞𝔱𝔢𝔫 𝔰𝔭𝔢𝔦𝔠𝔥𝔢𝔯𝔫",
            ["m7"] = "𝔅𝔢𝔢𝔫𝔡𝔢𝔫",
            ["m8"] = "𝔖𝔠𝔥𝔯𝔢𝔦𝔟𝔴𝔢𝔦𝔰𝔢 𝔴𝔢𝔠𝔥𝔰𝔢𝔩𝔫",
            ["m9"] = "𝔖𝔭𝔯𝔞𝔠𝔥𝔢 𝔴𝔢𝔠𝔥𝔰𝔢𝔩𝔫",
            ["m10"] = "𝔏𝔬𝔤𝔦𝔫 𝔴𝔢𝔠𝔥𝔰𝔢𝔩𝔫",
            ["m11"] = "𝔅𝔢𝔫𝔲𝔱𝔷𝔢𝔯𝔳𝔢𝔯𝔴𝔞𝔩𝔱𝔲𝔫𝔤",
            ["beer_name"] = "𝔑𝔞𝔪𝔢 𝔡𝔢𝔰 𝔅𝔦𝔢𝔯𝔢𝔰: ",
            ["beer_price"] = "𝔓𝔯𝔢𝔦𝔰: ",
            ["beer_stock"] = "𝔅𝔢𝔰𝔱𝔞𝔫𝔡: ",
            ["beer_added"] = "𝔅𝔦𝔢𝔯 𝔥𝔦𝔫𝔷𝔲𝔤𝔢𝔣ü𝔤𝔱!",
            ["no_beer"] = "𝔎𝔢𝔦𝔫𝔢 𝔅𝔦𝔢𝔯𝔢 𝔳𝔬𝔯𝔥𝔞𝔫𝔡𝔢𝔫!",
            ["beer_list"] = "𝔄𝔨𝔱𝔲𝔢𝔩𝔩𝔢 𝔅𝔦𝔢𝔯𝔢:",
            ["stock"] = "𝔅𝔢𝔰𝔱𝔞𝔫𝔡",
            ["not_found"] = "𝔅𝔦𝔢𝔯 𝔫𝔦𝔠𝔥𝔱 𝔤𝔢𝔣𝔲𝔫𝔡𝔢𝔫!",
            ["sell_amount"] = "𝔚𝔦𝔢 𝔳𝔦𝔢𝔩𝔢 𝔳𝔢𝔯𝔨𝔞𝔲𝔣𝔢𝔫? ",
            ["not_enough"] = "𝔑𝔦𝔠𝔥𝔱 𝔤𝔢𝔫𝔲𝔤 𝔅𝔢𝔰𝔱𝔞𝔫𝔡!",
            ["discount"] = "10% 𝔕𝔞𝔟𝔞𝔱𝔱 𝔤𝔢𝔴ä𝔥𝔯𝔱.",
            ["sale_done"] = "𝔙𝔢𝔯𝔨𝔞𝔲𝔣 𝔢𝔯𝔣𝔬𝔩𝔤𝔯𝔢𝔦𝔠𝔥! 𝔈𝔦𝔫𝔫𝔞𝔥𝔪𝔢:",
            ["current"] = "𝔄𝔨𝔱𝔲𝔢𝔩𝔩",
            ["edit_price"] = "𝔓𝔯𝔢𝔦𝔰 ä𝔫𝔡𝔢𝔯𝔫",
            ["edit_stock"] = "𝔅𝔢𝔰𝔱𝔞𝔫𝔡 ä𝔫𝔡𝔢𝔯𝔫",
            ["new_price"] = "𝔑𝔢𝔲𝔢𝔯 𝔓𝔯𝔢𝔦𝔰: ",
            ["new_stock"] = "𝔑𝔢𝔲𝔢𝔯 𝔅𝔢𝔰𝔱𝔞𝔫𝔡: ",
            ["price_changed"] = "𝔓𝔯𝔢𝔦𝔰 𝔤𝔢ä𝔫𝔡𝔢𝔯𝔱!",
            ["stock_changed"] = "𝔅𝔢𝔰𝔱𝔞𝔫𝔡 𝔤𝔢ä𝔫𝔡𝔢𝔯𝔱!",
            ["delete_name"] = "𝔅𝔦𝔢𝔯 𝔩ö𝔰𝔠𝔥𝔢𝔫 (𝔑𝔞𝔪𝔢): ",
            ["deleted"] = "𝔅𝔦𝔢𝔯 𝔤𝔢𝔩ö𝔰𝔠𝔥𝔱!",
            ["user"] = "𝔅𝔢𝔫𝔲𝔱𝔷𝔢𝔯𝔫𝔞𝔪𝔢: ",
            ["pass"] = "𝔓𝔞𝔰𝔰𝔴𝔬𝔯𝔱: ",
            ["pass2"] = "𝔚𝔦𝔢𝔡𝔢𝔯𝔥𝔬𝔩𝔢𝔫: ",
            ["newpw"] = "𝔑𝔢𝔲𝔢𝔰 𝔓𝔞𝔰𝔰𝔴𝔬𝔯𝔱: "
        };

        // Englisch NORMAL
        var en = new Dictionary<string, string>
        {
            ["ask_alt"] = "Enable Old German font? (y/n):",
            ["invalid"] = "Invalid input!",
            ["continue"] = "\nPress any key to continue...",
            ["end"] = "\nProgram ended. Cheers! 🍺",
            ["alt_on"] = "Old German font enabled!",
            ["alt_off"] = "Old German font disabled.",
            ["lang_switched"] = "Language switched. [EN] ↔ [DE]",
            ["login_menu"] = "① - Login\n② - Register\n③ - Exit",
            ["login_ok"] = "✅ Successfully logged in!",
            ["login_fail"] = "❌ Wrong username or password.",
            ["logout"] = "↩️  Logged out.",
            ["register"] = "Create new user:",
            ["user_exists"] = "User already exists!",
            ["pw_mismatch"] = "Passwords do not match!",
            ["reg_ok"] = "User created!",
            ["user_admin_menu"] = "① - List users\n② - Make admin\n③ - Remove admin\n④ - Reset password\n⑤ - Delete user\n⑥ - Back",
            ["admin_set"] = "✅ User is now admin.",
            ["admin_removed"] = "✅ Admin rights removed.",
            ["pw_ok"] = "Password updated.",
            ["user_deleted"] = "User deleted.",
            ["role_master"] = "(Brewmaster)",
            ["role_admin"] = "(Admin)",
            ["choice"] = "Choice: ",
            ["m1"] = "Add new beer",
            ["m2"] = "Show beers",
            ["m3"] = "Sell beer",
            ["m4"] = "Edit beer",
            ["m5"] = "Delete beer",
            ["m6"] = "Save data",
            ["m7"] = "Exit",
            ["m8"] = "Toggle font style",
            ["m9"] = "Switch language",
            ["m10"] = "Switch login",
            ["m11"] = "Manage users",
            ["beer_name"] = "Beer name: ",
            ["beer_price"] = "Price: ",
            ["beer_stock"] = "Stock: ",
            ["beer_added"] = "Beer added!",
            ["no_beer"] = "No beers available!",
            ["beer_list"] = "Current beers:",
            ["stock"] = "Stock",
            ["not_found"] = "Beer not found!",
            ["sell_amount"] = "How many to sell? ",
            ["not_enough"] = "Not enough stock!",
            ["discount"] = "10% discount applied.",
            ["sale_done"] = "Sale successful! Total:",
            ["current"] = "Current",
            ["edit_price"] = "Edit price",
            ["edit_stock"] = "Edit stock",
            ["new_price"] = "New price: ",
            ["new_stock"] = "New stock: ",
            ["price_changed"] = "Price updated!",
            ["stock_changed"] = "Stock updated!",
            ["delete_name"] = "Delete beer (name): ",
            ["deleted"] = "Beer deleted!",
            ["user"] = "Username: ",
            ["pass"] = "Password: ",
            ["pass2"] = "Repeat: ",
            ["newpw"] = "New password: "
        };

        // Englisch ALTDEUTSCH (Fraktur-Strings)
        var en_alt = new Dictionary<string, string>
        {
            ["ask_alt"] = "𝔈𝔫𝔞𝔟𝔩𝔢 𝔒𝔩𝔡 𝔊𝔢𝔯𝔪𝔞𝔫 𝔣𝔬𝔫𝔱? (y/n):",
            ["invalid"] = "𝔐𝔦𝔰𝔪𝔞𝔱𝔠𝔥𝔢𝔡 𝔦𝔫𝔭𝔲𝔱!",
            ["continue"] = "\n𝔓𝔯𝔢𝔰𝔰 𝔞𝔫𝔶 𝔨𝔢𝔶 𝔱𝔬 𝔠𝔬𝔫𝔱𝔦𝔫𝔲𝔢...",
            ["end"] = "\n𝔓𝔯𝔬𝔤𝔯𝔞𝔪 𝔢𝔫𝔡𝔢𝔡. 𝔠𝔥𝔢𝔢𝔯𝔰! 🍺",
            ["alt_on"] = "𝔒𝔩𝔡 𝔊𝔢𝔯𝔪𝔞𝔫 𝔣𝔬𝔫𝔱 𝔢𝔫𝔞𝔟𝔩𝔢𝔡!",
            ["alt_off"] = "𝔑𝔬𝔯𝔪𝔞𝔩 𝔣𝔬𝔫𝔱 𝔢𝔫𝔞𝔟𝔩𝔢𝔡.",
            ["lang_switched"] = "𝔏𝔞𝔫𝔤𝔲𝔞𝔤𝔢 𝔰𝔴𝔦𝔱𝔠𝔥𝔢𝔡. [EN] ↔ [DE]",
            ["login_menu"] = "① - 𝔏𝔬𝔤𝔦𝔫\n② - 𝔅𝔢𝔤𝔦𝔰𝔱𝔢𝔯\n③ - 𝔈𝔵𝔦𝔱",
            ["login_ok"] = "✅ 𝔖𝔲𝔠𝔠𝔢𝔰𝔰𝔣𝔲𝔩𝔩𝔶 𝔩𝔬𝔤𝔤𝔢𝔡 𝔦𝔫!",
            ["login_fail"] = "❌ 𝔚𝔯𝔬𝔫𝔤 𝔲𝔰𝔢𝔯𝔫𝔞𝔪𝔢 𝔬𝔯 𝔭𝔞𝔰𝔰𝔴𝔬𝔯𝔡.",
            ["logout"] = "↩️  𝔏𝔬𝔤𝔤𝔢𝔡 𝔬𝔲𝔱.",
            ["register"] = "𝔠𝔯𝔢𝔞𝔱𝔢 𝔫𝔢𝔴 𝔲𝔰𝔢𝔯:",
            ["user_exists"] = "𝔘𝔰𝔢𝔯 𝔞𝔩𝔯𝔢𝔞𝔡𝔶 𝔢𝔵𝔦𝔰𝔱𝔰!",
            ["pw_mismatch"] = "𝔓𝔞𝔰𝔰𝔴𝔬𝔯𝔡𝔰 𝔡𝔬 𝔫𝔬𝔱 𝔪𝔞𝔱𝔠𝔥!",
            ["reg_ok"] = "𝔘𝔰𝔢𝔯 𝔠𝔯𝔢𝔞𝔱𝔢𝔡!",
            ["user_admin_menu"] = "① - 𝔏𝔦𝔰𝔱 𝔲𝔰𝔢𝔯𝔰\n② - 𝔐𝔞𝔨𝔢 𝔞𝔡𝔪𝔦𝔫\n③ - 𝔯𝔢𝔪𝔬𝔳𝔢 𝔞𝔡𝔪𝔦𝔫\n④ - 𝔯𝔢𝔰𝔢𝔱 𝔭𝔞𝔰𝔰𝔴𝔬𝔯𝔡\n⑤ - 𝔇𝔢𝔩𝔢𝔱𝔢 𝔲𝔰𝔢𝔯\n⑥ - 𝔅𝔞𝔠𝔨",
            ["admin_set"] = "✅ 𝔘𝔰𝔢𝔯 𝔦𝔰 𝔫𝔬𝔴 𝔞𝔡𝔪𝔦𝔫.",
            ["admin_removed"] = "✅ 𝔄𝔡𝔪𝔦𝔫 𝔯𝔦𝔤𝔥𝔱𝔰 𝔯𝔢𝔪𝔬𝔳𝔢𝔡.",
            ["pw_ok"] = "𝔓𝔞𝔰𝔰𝔴𝔬𝔯𝔡 𝔲𝔭𝔡𝔞𝔱𝔢𝔡.",
            ["user_deleted"] = "𝔘𝔰𝔢𝔯 𝔡𝔢𝔩𝔢𝔱𝔢𝔡.",
            ["role_master"] = "(𝔅𝔯𝔢𝔴𝔪𝔞𝔰𝔱𝔢𝔯)",
            ["role_admin"] = "(𝔄𝔡𝔪𝔦𝔫)",
            ["choice"] = "ℭ𝔥𝔬𝔦𝔠𝔢: ",
            ["m1"] = "𝔄𝔡𝔡 𝔫𝔢𝔴 𝔟𝔢𝔢𝔯",
            ["m2"] = "𝔖𝔥𝔬𝔴 𝔟𝔢𝔢𝔯𝔰",
            ["m3"] = "𝔖𝔢𝔩𝔩 𝔟𝔢𝔢𝔯",
            ["m4"] = "𝔈𝔡𝔦𝔱 𝔟𝔢𝔢𝔯",
            ["m5"] = "𝔇𝔢𝔩𝔢𝔱𝔢 𝔟𝔢𝔢𝔯",
            ["m6"] = "𝔖𝔞𝔳𝔢 𝔡𝔞𝔱𝔞",
            ["m7"] = "𝔈𝔵𝔦𝔱",
            ["m8"] = "𝔗𝔬𝔤𝔤𝔩𝔢 𝔣𝔬𝔫𝔱 𝔰𝔱𝔶𝔩𝔢",
            ["m9"] = "𝔖𝔴𝔦𝔱𝔠𝔥 𝔩𝔞𝔫𝔤𝔲𝔞𝔤𝔢",
            ["m10"] = "𝔖𝔴𝔦𝔱𝔠𝔥 𝔩𝔬𝔤𝔦𝔫",
            ["m11"] = "𝔐𝔞𝔫𝔞𝔤𝔢 𝔲𝔰𝔢𝔯𝔰",
            ["beer_name"] = "𝔅𝔢𝔢𝔯 𝔫𝔞𝔪𝔢: ",
            ["beer_price"] = "𝔓𝔯𝔦𝔠𝔢: ",
            ["beer_stock"] = "𝔖𝔱𝔬𝔠𝔨: ",
            ["beer_added"] = "𝔅𝔢𝔢𝔯 𝔞𝔡𝔡𝔢𝔡!",
            ["no_beer"] = "𝔑𝔬 𝔟𝔢𝔢𝔯𝔰 𝔞𝔳𝔞𝔦𝔩𝔞𝔟𝔩𝔢!",
            ["beer_list"] = "𝔠𝔲𝔯𝔯𝔢𝔫𝔱 𝔟𝔢𝔢𝔯𝔰:",
            ["stock"] = "𝔖𝔱𝔬𝔠𝔨",
            ["not_found"] = "𝔅𝔢𝔢𝔯 𝔫𝔬𝔱 𝔣𝔬𝔲𝔫𝔡!",
            ["sell_amount"] = "ℌ𝔬𝔴 𝔪𝔞𝔫𝔶 𝔱𝔬 𝔰𝔢𝔩𝔩? ",
            ["not_enough"] = "𝔑𝔬𝔱 𝔢𝔫𝔬𝔲𝔤𝔥 𝔰𝔱𝔬𝔠𝔨!",
            ["discount"] = "10% 𝔡𝔦𝔰𝔠𝔬𝔲𝔫𝔱 𝔞𝔭𝔭𝔩𝔦𝔢𝔡.",
            ["sale_done"] = "𝔖𝔞𝔩𝔢 𝔰𝔲𝔠𝔠𝔢𝔰𝔰𝔣𝔲𝔩!",
            ["current"] = "ℭ𝔲𝔯𝔯𝔢𝔫𝔱",
            ["edit_price"] = "𝔈𝔡𝔦𝔱 𝔭𝔯𝔦𝔠𝔢",
            ["edit_stock"] = "𝔈𝔡𝔦𝔱 𝔰𝔱𝔬𝔠𝔨",
            ["new_price"] = "𝔑𝔢𝔴 𝔭𝔯𝔦𝔠𝔢: ",
            ["new_stock"] = "𝔑𝔢𝔴 𝔰𝔱𝔬𝔠𝔨: ",
            ["price_changed"] = "𝔓𝔯𝔦𝔠𝔢 𝔲𝔭𝔡𝔞𝔱𝔢𝔡!",
            ["stock_changed"] = "𝔖𝔱𝔬𝔠𝔨 𝔲𝔭𝔡𝔞𝔱𝔢𝔡!",
            ["delete_name"] = "𝔇𝔢𝔩𝔢𝔱𝔢 𝔟𝔢𝔢𝔯 (𝔫𝔞𝔪𝔢): ",
            ["deleted"] = "𝔅𝔢𝔢𝔯 𝔡𝔢𝔩𝔢𝔱𝔢𝔡!",
            ["user"] = "𝔘𝔰𝔢𝔯𝔫𝔞𝔪𝔢: ",
            ["pass"] = "𝔓𝔞𝔰𝔰𝔴𝔬𝔯𝔡: ",
            ["pass2"] = "ℜ𝔢𝔭𝔢𝔞𝔱: ",
            ["newpw"] = "𝔑𝔢𝔴 𝔭𝔞𝔰𝔰𝔴𝔬𝔯𝔡: "
        };

        var dic = (Sprache == "de")
                  ? (Alt ? de_alt : de)
                  : (Alt ? en_alt : en);

      
        return dic.ContainsKey(key) ? dic[key] : $"[{key}]";
    }
}
