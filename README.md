# Vehicles

## Vol.3 – järjestikune ja paralleelne töö

### Käivitamine
1. Ava `Vehicles.slnx` Visual Studios.
2. Vali startup-projektiks **Vehicles.WpfApp** ja vajuta **F5**.

### Kuidas näidata
1. Lisa garaažidesse 3–4 sõidukit (nt Car, Boat, AmphibiousCar).
2. Liiguta kõiki peale ühe (**Move**). Liigutamata sõiduki odomeeter on 0 km – see annab **vea**.
3. **Sequential** – sõidukid arvutatakse ükshaaval, järgmine alustab alles siis, kui eelmine on valmis.
4. **Parallel** – kõik sõidukid käivitatakse korraga (`Task.Run` + `Task.WhenAll`). Raportis on võrdlus: kas tulemused on samad ja mitu korda kiirem.
5. **Tühista** – käivita režiim ja vajuta kohe „Tühista”. Pooleli olevad sõidukid saavad oleku **Cancelled**.
6. Töö ajal liigub edenemisriba ja aken reageerib (UI ei hangu).

Raportis: sõiduki nimi, lõppolek (Done / Error / Cancelled), tulemus või veateade, kestus ja kogu töö aeg.
