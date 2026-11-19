using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

// Pomocnik: z .unity souboru spocita svetove pozice a scale objektu (bez prefabu), vypise ty v obdelniku
static class Scena
{
    class Tr { public string go; public float px, py, sx = 1, sy = 1, rz, rw = 1; public string otec; }

    public static void Vypis(string soubor, float x0, float x1, float y0, float y1)
    {
        var jmena = new Dictionary<string, string>();
        var tr = new Dictionary<string, Tr>();
        string id = null, typ = null; Tr t = null;
        var ci = CultureInfo.InvariantCulture;
        foreach (string r in File.ReadLines(soubor))
        {
            if (r.StartsWith("--- !u!"))
            {
                var m = Regex.Match(r, @"!u!(\d+) &(-?\d+)");
                typ = m.Groups[1].Value; id = m.Groups[2].Value; t = null;
                if (typ == "4") { t = new Tr(); tr[id] = t; }
                continue;
            }
            if (typ == "1" && r.StartsWith("  m_Name: ")) jmena[id] = r.Substring(10);
            if (t == null) continue;
            if (r.StartsWith("  m_GameObject:")) t.go = Regex.Match(r, @"fileID: (-?\d+)").Groups[1].Value;
            else if (r.StartsWith("  m_Father:")) t.otec = Regex.Match(r, @"fileID: (-?\d+)").Groups[1].Value;
            else if (r.StartsWith("  m_LocalPosition:")) { var m = Regex.Matches(r, @"-?[\d.]+(e-?\d+)?"); t.px = float.Parse(m[0].Value, ci); t.py = float.Parse(m[1].Value, ci); }
            else if (r.StartsWith("  m_LocalScale:")) { var m = Regex.Matches(r, @"-?[\d.]+(e-?\d+)?"); t.sx = float.Parse(m[0].Value, ci); t.sy = float.Parse(m[1].Value, ci); }
            else if (r.StartsWith("  m_LocalRotation:")) { var m = Regex.Matches(r, @"-?[\d.]+(e-?\d+)?"); t.rz = float.Parse(m[2].Value, ci); t.rw = float.Parse(m[3].Value, ci); }
        }
        foreach (var kv in tr)
        {
            float wx = 0, wy = 0, sx = 1, sy = 1, uhel = 0;
            // retez od korene dolu
            var cesta = new List<Tr>();
            Tr c = kv.Value; int pojistka = 0;
            while (c != null && pojistka++ < 50) { cesta.Insert(0, c); c = c.otec != null && tr.ContainsKey(c.otec) ? tr[c.otec] : null; }
            foreach (Tr u in cesta)
            {
                double a = uhel * Math.PI / 180;
                float lx = u.px * sx, ly = u.py * sy;
                wx += (float)(lx * Math.Cos(a) - ly * Math.Sin(a));
                wy += (float)(lx * Math.Sin(a) + ly * Math.Cos(a));
                sx *= u.sx; sy *= u.sy;
                uhel += (float)(2 * Math.Atan2(u.rz, u.rw) * 180 / Math.PI);
            }
            if (wx < x0 || wx > x1 || wy < y0 || wy > y1) continue;
            string jm = kv.Value.go != null && jmena.ContainsKey(kv.Value.go) ? jmena[kv.Value.go] : "?";
            string cela = jm;
            if (cesta.Count > 1) { var o = cesta[cesta.Count - 2]; if (o.go != null && jmena.ContainsKey(o.go)) cela = jmena[o.go] + "/" + jm; }
            Console.WriteLine($"{cela,-40} pos ({wx:0.00}, {wy:0.00})  scale ({sx:0.00}, {sy:0.00})  rot {uhel:0}");
        }
    }
}
