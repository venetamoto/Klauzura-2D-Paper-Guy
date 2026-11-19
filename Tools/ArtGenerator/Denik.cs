using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security;
using System.Text;

// Denik prace (Word .docx) - postaveny rucne z XML, bez knihoven
static class Denik
{
    // (datum, odstavce)
    static readonly List<(string, string[])> zapisy = new()
    {
        ("21.–22. 10. 2026", new[] {
            "Začátek projektu. Navrhla jsem, jaké skripty bude hra potřebovat: pohyb postavičky (chůze, skok), kamera, která ji sleduje, plošiny, jídlo a přechod do dalšího levelu.",
            "Kód jsem zjednodušila tak, aby byl přehledný – jednoduché skripty, komentáře a výpisy v češtině." }),
        ("22. 10. 2026", new[] {
            "První verze mého levelu podle náčrtu. Ruka v levelu sama nakreslí hvězdu – to byl první pokus o „živý“ sešit." }),
        ("26. 10. 2026", new[] {
            "Druhá verze levelu: velký graf paraboly, po kterém postavička sjíždí, kamera s velikostí 7,5, plošiny, které se dají proskočit zespodu, a přechod do dalšího levelu.",
            "První animace postavičky podle videa a pak podle konceptu (kulatá hlava, černý plášť, tenké nožičky)." }),
        ("27. 10. 2026", new[] {
            "Animace: plášť z boku, při pádu se plášť roztáhne jako padák, jedení – vystřelí jazyk a jídlo přiletí do pusy." }),
        ("2. 11. 2026", new[] {
            "Pozadí z fotky papíru – level vypadá jako školní sešit z matematiky. Zeslabila jsem třes kamery při dopadu." }),
        ("4.–5. 11. 2026", new[] {
            "Level jsem přestavěla podle svého nákresu: velikost 2 × 2 obrazovky, řádky textu jako plošiny, tmavé obrázky, graf nahoře. Přidala jsem okraje sešitu (listy pod hlavním listem a stín).",
            "Postavičku jsem rozpohybovala svými kresbami: stání s mrkáním, chůze ze 2 kreseb, skok, pád a jedení z 9 kreseb (jazyk vystřelí, zatočí se a vrátí se).",
            "Klíč a dveře: klíč se bere klávesou F, zelené dveře se klávesou F otevřou, když má postavička klíč. U klíče a dveří je kreslená nápověda.",
            "Ruka s gumou smaže červenou plošinu, až na ní postavička stojí a sní modrý čtvereček; potom ruka s tužkou nakreslí malou plošinu. V místnosti ruka nakreslí klíč a smaže fialové dveře.",
            "Když postavička vyjde ze sešitu, level začne znovu.",
            "Po cizím commitu se scéna nedala otevřít (v souboru zůstaly konflikty) – obnovila jsem ji ze starší verze v gitu." }),
        ("6. 11. 2026", new[] {
            "Poslední level VarvaraKonec „Prázdné stránky“: obrovská guma honí postavičku zleva doprava a maže plošiny za ní, některé plošiny se rozpadají. Když guma postavičku dožene, smaže ji a ruka napíše na prázdný sešit závěrečnou větu.",
            "Nový klidný level Varvara2 mezi nimi: prázdný list, stopy po gumě, ruka postavičce smaže chocholku, ona se lekne a uteče do dalšího levelu.",
            "Druhý (dobrý) konec: kdo najde tajný čtvereček v jámě, dostane na konci tužku. Tužka postavičce dokreslí chocholku a nakreslí zeď, o kterou se guma rozbije.",
            "Opravila jsem animaci chůze – z kreseb zmizely slabé čárky pohybu, které vypadaly jako třetí noha." }),
        ("12. 11. 2026", new[] {
            "Hra nešla spustit v žádné scéně – ze společného skriptu jídla zmizela proměnná, kterou používají dveře v levelu Ines. Vrátila jsem ji tak, aby fungoval kód Filipa i Ines.",
            "Animace postavičky jsem dala i do levelů Filipa a Ines. Nápisy v mých levelech jsem přeložila do angličtiny.",
            "Menu konce hry („You finished the game!“, Play again, Controls, Settings, Credits) a startovní menu: dole je hřiště, kde se dá postavičkou chodit a chytat ručně nakreslená čísla. Pauza na klávesu Esc.",
            "Animace rozpadu postavičky, když vypadne ze sešitu (6 mých kreseb). Kamera v levelu Ines je teď stejná jako v mých levelech." }),
        ("16. 11. 2026", new[] {
            "Na řádky v levelu Varvara jsem napsala modrou fixou úlohu o míčku, který letí po parabole: „QUADRATIC FUNCTIONS“, vzorec f(x) = −0,5x² + 18, vrchol, osu souměrnosti, maximální výšku a tabulku hodnot x a f(x). Texty jsem fotila a vložila do hry jako obrázky.",
            "Modrý čtvereček jídla jsem nahradila napsaným číslem 18, takže vzorec pokračuje.",
            "Kreslené ruce jsem vyměnila za fotky své ruky – jednou s gumou, jednou s tužkou.",
            "Na řádcích s textem se dá chodit jen tam, kde je text napsaný." }),
        ("18. 11. 2026", new[] {
            "Ruce jsem zvětšila, aby byly lépe vidět." }),
        ("19. 11. 2026", new[] {
            "Oprava: v levelu Ines a Filipa hra přecházela dál, i když nebylo všechno snědené – počítadlo jídla se po změně kódu nenulovalo. Pořadí levelů je teď Ines → Filip → Varvara.",
            "Další texty: na červené plošině „The ball lands after 6 metres“ s červeně přeškrtnutou šestkou – nápis se smaže spolu s plošinou. Čtverečky jídla jsou teď napsaná čísla 9 a 4. Dole výpočet −0,5x² + 18 = 0 ⇒ x² = 36 ⇒ x = ±6 a vpravo další řádky (f(0), f(1), f(4), vrchol, a = −0,5).",
            "Texty jsem srovnala, aby byla písmena všude stejně velká a nebyla nakřivo.",
            "Mezi dvěma dveřmi stojí domeček z prken nakreslený jako skica. Postavička je uvnitř vidět jen v oknech." }),
    };

    public static void Uloz(string soubor)
    {
        var d = new StringBuilder();
        d.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
        d.Append("<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body>");
        Odst(d, "Title", "Deník práce – Klauzura 2D: Paper Guy");
        Odst(d, "Subtitle", "Varvara · levely Varvara, Varvara2 a VarvaraKonec, menu");
        Odst(d, null, "Hra je 2D plošinovka v Unity 6. Odehrává se ve školním sešitě z matematiky – postavička z papíru chodí po řádcích textu, jí nakreslená čísla a utíká před gumou. Grafiku kreslila Ines, programovali jsme s Filipem. V tomhle deníku píšu, co jsem kdy na hře dělala.");

        // koncept a zmeny konceptu (text tymu)
        Odst(d, "Heading1", "1. Původní koncept a vize projektu");
        Odst(d, null, "Na úplném začátku vývoje stála myšlenka vytvořit atmosférickou 2D plošinovku o oživené skice (nakreslené postavičce), která prochází jednotlivými stranami školního sešitu. Hlavní narativní linií a cílem postavy byla snaha dosáhnout úplně poslední stránky sešitu, kde ji podle původního záměru čekal tragický osud – neúprosné vymazání gumou ze strany autora.");
        Odst(d, "Heading1", "2. Vývojové změny a iterace konceptu");
        Odst(d, null, "Během produkční fáze a testování herních mechanik prošel koncept několika zásadními změnami, které výrazně posunuly hratelnost i technickou stabilitu hry:");
        Odst(d, "Heading3", "A. Hratelnost a gradace finále (Game Design)");
        Bod(d, "Původní stav:", "Pasivní zakončení hry, kde je postava na poslední stránce pouze sledovatelem svého vymazání.");
        Bod(d, "Iterace:", "Po společné diskusi jsme se rozhodli finální pasáž dramatizovat a přidat prvek překvapení. Místo obyčejné animace mazání jsme navrhli a implementovali interaktivní mini-bossfight s Rukou autora.");
        Bod(d, "Přínos:", "Závěr hry díky tomu získal potřebnou gradaci, dynamiku a hráči nabízí vyvrcholení v podobě aktivní výzvy, což výrazně zvyšuje celkový zážitek ze hry.");
        Odst(d, "Heading3", "B. Vizuální styl a technická optimalizace (Technical Art & Physics)");
        Bod(d, "Původní stav:", "Záměr zasadit hru do prostředí klasického linkovaného nebo čtverečkovaného sešitu.");
        Bod(d, "Technické komplikace:", "Během vývoje kódu a nastavování kolizí se ukázalo, že vizuální mřížka (linky/čtverečky) vytváří výrazná omezení při přesném pozicionování objektů, konfiguraci fyzikálního enginu a plynulém pohybu postavy.");
        Bod(d, "Optimální řešení:", "Rozhodli jsme se upravit vizuální podklad na čisté bílé listy papíru.");
        Bod(d, "Přínos:", "Tato změna zásadně zjednodušila technickou architekturu vývoje. Uvolnila nám ruce při návrhu level designu, umožnila preciznější nastavení fyzikálních kolizí a dovolila týmu plně se soustředit na plynulost mechaniky pohybu, skoků a detailní 2D animaci.");
        Odst(d, "Heading3", "C. Hranice sešitu (Level Design)");
        Bod(d, "Původní stav:", "Postavičku na okrajích levelu zastavovaly neviditelné stěny.");
        Bod(d, "Iterace:", "Rozhodli jsme se, že když postavička vyjde mimo list sešitu, vypaří se – rozpadne se na kousky (animace ze šesti kreseb) a level začne znovu od začátku.");
        Bod(d, "Přínos:", "Okraj sešitu se stal součástí hry místo neviditelné zdi. Hráč hned vidí, kde svět končí, a pád ze stránky má jasný a srozumitelný následek.");
        Odst(d, "Heading1", "3. Zhodnocení vývojového procesu");
        Odst(d, null, "Změny provedené během vývoje ukazují naši schopnost reagovat na technické výzvy a kriticky hodnotit game design. Výsledkem těchto iterací je stabilnější, hratelnější a dynamičtější projekt, který si zachovává svou původní osobitou myšlenku, ale nabízí výrazně kvalitnější herní zážitek.");
        Odst(d, null, "Přílohy (videa): sin.mp4, han.mp4");

        Odst(d, "Heading1", "4. Deník práce po dnech");
        foreach (var (datum, odstavce) in zapisy)
        {
            Odst(d, "Heading2", datum);
            foreach (string o in odstavce) Odst(d, null, o);
        }
        Odst(d, "Heading2", "Co zbývá");
        Odst(d, null, "Projít celou hru od startovního menu až po konec a vyzkoušet oba konce. Hra zatím nemá zvuky ani hudbu.");
        d.Append("<w:sectPr><w:pgSz w:w=\"11906\" w:h=\"16838\"/><w:pgMar w:top=\"1418\" w:right=\"1418\" w:bottom=\"1418\" w:left=\"1418\" w:header=\"709\" w:footer=\"709\" w:gutter=\"0\"/></w:sectPr>");
        d.Append("</w:body></w:document>");

        string styly = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
            "<w:styles xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\">" +
            "<w:docDefaults><w:rPrDefault><w:rPr><w:rFonts w:ascii=\"Calibri\" w:hAnsi=\"Calibri\" w:cs=\"Calibri\"/><w:sz w:val=\"23\"/><w:lang w:val=\"cs-CZ\"/></w:rPr></w:rPrDefault>" +
            "<w:pPrDefault><w:pPr><w:spacing w:after=\"120\" w:line=\"276\" w:lineRule=\"auto\"/></w:pPr></w:pPrDefault></w:docDefaults>" +
            "<w:style w:type=\"paragraph\" w:default=\"1\" w:styleId=\"Normal\"><w:name w:val=\"Normal\"/><w:pPr><w:jc w:val=\"both\"/></w:pPr></w:style>" +
            "<w:style w:type=\"paragraph\" w:styleId=\"Title\"><w:name w:val=\"Title\"/><w:basedOn w:val=\"Normal\"/><w:pPr><w:spacing w:before=\"0\" w:after=\"60\"/><w:jc w:val=\"left\"/></w:pPr><w:rPr><w:rFonts w:ascii=\"Calibri\" w:hAnsi=\"Calibri\"/><w:b/><w:color w:val=\"1F3864\"/><w:sz w:val=\"44\"/></w:rPr></w:style>" +
            "<w:style w:type=\"paragraph\" w:styleId=\"Subtitle\"><w:name w:val=\"Subtitle\"/><w:basedOn w:val=\"Normal\"/><w:pPr><w:spacing w:before=\"0\" w:after=\"240\"/><w:pBdr><w:bottom w:val=\"single\" w:sz=\"6\" w:space=\"6\" w:color=\"8EAADB\"/></w:pBdr><w:jc w:val=\"left\"/></w:pPr><w:rPr><w:color w:val=\"595959\"/><w:sz w:val=\"26\"/></w:rPr></w:style>" +
            "<w:style w:type=\"paragraph\" w:styleId=\"Heading1\"><w:name w:val=\"heading 1\"/><w:basedOn w:val=\"Normal\"/><w:next w:val=\"Normal\"/><w:pPr><w:keepNext/><w:spacing w:before=\"360\" w:after=\"120\"/><w:jc w:val=\"left\"/><w:outlineLvl w:val=\"0\"/></w:pPr><w:rPr><w:rFonts w:ascii=\"Calibri\" w:hAnsi=\"Calibri\"/><w:b/><w:color w:val=\"1F3864\"/><w:sz w:val=\"32\"/></w:rPr></w:style>" +
            "<w:style w:type=\"paragraph\" w:styleId=\"Heading3\"><w:name w:val=\"heading 3\"/><w:basedOn w:val=\"Normal\"/><w:next w:val=\"Normal\"/><w:pPr><w:keepNext/><w:spacing w:before=\"200\" w:after=\"80\"/><w:jc w:val=\"left\"/><w:outlineLvl w:val=\"2\"/></w:pPr><w:rPr><w:rFonts w:ascii=\"Calibri\" w:hAnsi=\"Calibri\"/><w:b/><w:color w:val=\"2F5496\"/><w:sz w:val=\"24\"/></w:rPr></w:style>" +
            "<w:style w:type=\"paragraph\" w:styleId=\"Heading2\"><w:name w:val=\"heading 2\"/><w:basedOn w:val=\"Normal\"/><w:next w:val=\"Normal\"/><w:pPr><w:keepNext/><w:spacing w:before=\"280\" w:after=\"80\"/><w:jc w:val=\"left\"/><w:outlineLvl w:val=\"1\"/></w:pPr><w:rPr><w:rFonts w:ascii=\"Calibri\" w:hAnsi=\"Calibri\"/><w:b/><w:color w:val=\"2F5496\"/><w:sz w:val=\"28\"/></w:rPr></w:style>" +
            "</w:styles>";

        if (File.Exists(soubor)) File.Delete(soubor);
        using var zip = ZipFile.Open(soubor, ZipArchiveMode.Create);
        Pis(zip, "[Content_Types].xml", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/><Override PartName=\"/word/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml\"/></Types>");
        Pis(zip, "_rels/.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"word/document.xml\"/></Relationships>");
        Pis(zip, "word/_rels/document.xml.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/></Relationships>");
        Pis(zip, "word/document.xml", d.ToString());
        Pis(zip, "word/styles.xml", styly);
        Console.WriteLine(soubor + ": " + zapisy.Count + " zapisu");
    }

    static void Odst(StringBuilder d, string styl, string text)
    {
        d.Append("<w:p>");
        if (styl != null) d.Append("<w:pPr><w:pStyle w:val=\"" + styl + "\"/></w:pPr>");
        d.Append("<w:r><w:t xml:space=\"preserve\">" + SecurityElement.Escape(text) + "</w:t></w:r></w:p>");
    }

    // odrazka: tucny nazev + text, odsazena (bez znaku odrazky)
    static void Bod(StringBuilder d, string nazev, string text)
    {
        d.Append("<w:p><w:pPr><w:ind w:left=\"567\"/><w:spacing w:after=\"80\"/></w:pPr>");
        d.Append("<w:r><w:rPr><w:b/></w:rPr><w:t xml:space=\"preserve\">" + SecurityElement.Escape(nazev) + " </w:t></w:r>");
        d.Append("<w:r><w:t xml:space=\"preserve\">" + SecurityElement.Escape(text) + "</w:t></w:r></w:p>");
    }

    static void Pis(ZipArchive zip, string cesta, string obsah)
    {
        var e = zip.CreateEntry(cesta);
        using var w = new StreamWriter(e.Open(), new UTF8Encoding(false));
        w.Write(obsah);
    }
}
