# 🚢 Battleship Basescu — Documentație Tehnică

> **Proiect:** Joc de Bătălia Navelor (Battleship) multiplayer, arhitectură client-server  
> **Tehnologie:** C# (.NET), WinForms (client), Console App (server)  
> **Comunicare:** TCP/IP cu mesaje JSON, port `5000`

---

## 1. Descrierea jocului

**Battleship** este un joc clasic pentru **2 jucători** în care fiecare jucător plasează nave pe o grilă 10×10 și încearcă să scufunde navele adversarului atacând celule din grila lui.

### Flux general de joc

```
1. Ambii clienți se conectează la server
2. Fiecare jucător plasează navele pe propria grilă
3. Fiecare jucător apasă "Ready" → trimite tabla la server
4. Serverul anunță START și stabilește cine lovește primul
5. Jucătorii atacă pe rând, serverul validează și notifică
6. Primul jucător care distruge toate navele adverse câștigă
```

### Condiția de câștig

Fiecare navă ocupă un număr de celule (ex. Carrier = 5 celule). Total celule ocupate de nave = **17** (5+4+3+3+2). Primul jucător care acumulează **17 lovituri** câștigă jocul.

---

## 2. Arhitectura proiectului

```
BattleshipBasescu/
├── ConsoleAppServerBattleship/   ← Serverul TCP
│   ├── Program.cs                ← Logica principală a serverului
│   ├── GameMessage.cs            ← Clasa de mesaj partajat
│   └── NetworkHelper.cs          ← Trimitere/primire mesaje JSON
│
└── WinFormsAppClientBattleship/  ← Clientul GUI
    ├── Form1.cs                  ← Interfața și logica de joc
    ├── Form1.Designer.cs         ← Layout-ul generat de designer
    ├── GameMessage.cs            ← Aceeași clasă de mesaj (duplicată)
    └── NetworkHelper.cs          ← Helper de rețea (versiune client)
```

### Diagrama arhitecturală

```
┌─────────────────────┐    TCP:5000    ┌──────────────────────┐
│   Client 1 (WinForms)│◄─────────────►│                      │
│                     │                │   Server (Console)   │
│   Client 2 (WinForms)│◄─────────────►│                      │
└─────────────────────┘                └──────────────────────┘
```

---

## 3. Structura mesajelor — clasa `GameMessage`

Toate comunicările între client și server se fac prin obiecte **`GameMessage`** serializate în format **JSON**.

```csharp
public class GameMessage
{
    public string Tip { get; set; }       // Tipul mesajului (ex. "Atac", "Ready")
    public int X { get; set; }            // Linia atacată (0–9)
    public int Y { get; set; }            // Coloana atacată (0–9)
    public string Status { get; set; }    // Starea: "Hit", "Miss", "Win", "Lose", "MyTurn", "Wait"
    public int JucatorActiv { get; set; } // 1 sau 2 (rezervat, nefolosit activ)
    public int[][] Board { get; set; }    // Matricea tablei (10×10) — folosit la "Ready"
}
```

### Valorile matricei `Board`

| Valoare | Semnificație |
|---------|-------------|
| `0` | Celulă goală |
| `1` | Celulă ocupată de o navă |
| `2` | Celulă lovită (marcată de server) |

---

## 4. Primitivele de comunicare (protocoalele)

Aceasta este **sintaxa protocolului** — lista completă a mesajelor schimbate între client și server.

---

### 4.1 `"Ready"` — Jucătorul este pregătit

**Direcție:** Client → Server  
**Când:** După ce jucătorul a plasat toate navele și apasă butonul "Ready"

```json
{
  "Tip": "Ready",
  "Board": [
    [0,0,1,1,1,1,1,0,0,0],
    [0,0,0,0,0,0,0,0,0,0],
    ...
  ]
}
```

| Câmp | Tip | Descriere |
|------|-----|-----------|
| `Tip` | `string` | `"Ready"` |
| `Board` | `int[][]` | Tabla 10×10 cu navele plasate (1 = navă) |

---

### 4.2 `"Start"` — Jocul începe

**Direcție:** Server → Client (ambii)  
**Când:** Ambii jucători au trimis `"Ready"`

```json
{
  "Tip": "Start"
}
```

---

### 4.3 `"SchimbareTura"` — Schimbarea turului

**Direcție:** Server → Client (ambii)  
**Când:** La start și după fiecare atac

```json
{
  "Tip": "SchimbareTura",
  "Status": "MyTurn"
}
```

```json
{
  "Tip": "SchimbareTura",
  "Status": "Wait"
}
```

| `Status` | Semnificație |
|----------|-------------|
| `"MyTurn"` | Rândul acestui jucător să atace |
| `"Wait"` | Trebuie să aștepte |

---

### 4.4 `"Atac"` — Jucătorul atacă

**Direcție:** Client → Server  
**Când:** Jucătorul activ face clic pe o celulă din tabla adversarului

```json
{
  "Tip": "Atac",
  "X": 3,
  "Y": 7
}
```

| Câmp | Tip | Descriere |
|------|-----|-----------|
| `Tip` | `string` | `"Atac"` |
| `X` | `int` | Linia (0–9) |
| `Y` | `int` | Coloana (0–9) |

---

### 4.5 `"RezultatAtac"` — Rezultatul atacului

**Direcție:** Server → Client (atacatorul)  
**Când:** Imediat după procesarea unui `"Atac"`

```json
{
  "Tip": "RezultatAtac",
  "X": 3,
  "Y": 7,
  "Status": "Hit"
}
```

| `Status` | Semnificație |
|----------|-------------|
| `"Hit"` | Lovitura a nimerit o navă (celula devine roșie) |
| `"Miss"` | Lovitura a ratat (celula devine albastră) |

---

### 4.6 `"NotificareAtacPrimit"` — Notificare atac primit

**Direcție:** Server → Client (apărătorul)  
**Când:** Adversarul a atacat o celulă din tabla ta

```json
{
  "Tip": "NotificareAtacPrimit",
  "X": 3,
  "Y": 7
}
```

> Celula (X, Y) din propria tablă devine **roșie**, indiferent dacă e lovitură sau nu (serverul nu trimite statusul apărătorului).

---

### 4.7 `"GameOver"` — Sfârșitul jocului

**Direcție:** Server → Client (ambii)  
**Când:** Un jucător a acumulat 17 lovituri (a distrus toate navele adverse)

```json
{
  "Tip": "GameOver",
  "Status": "Win"
}
```

```json
{
  "Tip": "GameOver",
  "Status": "Lose"
}
```

| `Status` | Semnificație |
|----------|-------------|
| `"Win"` | Clientul curent a câștigat |
| `"Lose"` | Clientul curent a pierdut |

---

## 5. Diagrama fluxului de mesaje

```
Client 1                    Server                    Client 2
   │                           │                           │
   │──── Ready (Board) ───────►│                           │
   │                           │◄────── Ready (Board) ─────│
   │                           │                           │
   │◄─── Start ────────────────│──── Start ───────────────►│
   │◄─── SchimbareTura(MyTurn)─│──── SchimbareTura(Wait) ─►│
   │                           │                           │
   │──── Atac (X,Y) ──────────►│                           │
   │◄─── RezultatAtac (Hit) ───│──NotificareAtacPrimit(X,Y)►│
   │◄─── SchimbareTura(Wait) ──│──── SchimbareTura(MyTurn)─►│
   │                           │                           │
   │                           │◄───── Atac (X,Y) ─────────│
   │◄NotificareAtacPrimit(X,Y)─│────RezultatAtac (Miss) ──►│
   │◄─── SchimbareTura(MyTurn)─│──── SchimbareTura(Wait) ─►│
   │                           │                           │
   │         ... (continuă) ...                            │
   │                           │                           │
   │◄─── GameOver(Win) ────────│──── GameOver(Lose) ───────►│
```

---

## 6. Clasa `NetworkHelper`

Utilizată atât pe server cât și pe client pentru serializare/deserializare.

### `SendMessageAsync`

```csharp
public static async Task SendMessageAsync(NetworkStream stream, GameMessage message)
```

Serializează `GameMessage` în JSON și îl trimite pe stream, terminat cu `\n`.

```csharp
// Exemplu de utilizare:
await NetworkHelper.SendMessageAsync(stream, new GameMessage { Tip = "Start" });
```

**Output pe fir (bytes UTF-8):**
```
{"Tip":"Start","X":0,"Y":0,"Status":null,"JucatorActiv":0,"Board":null}\n
```

---

### `ReceiveMessageAsync`

```csharp
public static async Task<GameMessage?> ReceiveMessageAsync(NetworkStream stream)
```

Citește bytes din stream, decodifică UTF-8, desparte mesajele multiple (apărute din rapid-click) pe `\n`, și deserializează primul mesaj valid.

Returnează `null` dacă:
- Conexiunea a fost închisă (`bytesRead == 0`)
- JSON-ul este malformat (excepție prinsă silențios)

---

## 7. Navele disponibile

| Navă | Lungime | Celule ocupate |
|------|---------|----------------|
| Carrier | 5 | 5 |
| Battleship | 4 | 4 |
| Cruiser | 3 | 3 |
| Submarine | 3 | 3 |
| Destroyer | 2 | 2 |
| **Total** | | **17** |

---

## 8. Reguli de plasare a navelor

- Navele pot fi plasate **orizontal** sau **vertical** (checkbox `isHorizontal`)
- Nu se permite **suprapunerea** navelor
- Nava nu poate depăși **marginile** grilei
- Toate navele trebuie plasate înainte ca butonul **"Ready"** să devină activ

---

## 9. Culorile interfeței (client WinForms)

| Culoare | Semnificație |
|---------|-------------|
| `LightBlue` | Celulă goală (tabla proprie) |
| `DarkGray` | Celulă ocupată de propria navă |
| `LightGray` | Celulă neattacată (tabla adversarului) |
| `Red` | Lovitură reușită (pe ambele table) |
| `Blue` | Rateuri pe tabla adversarului |

---

## 10. Configurare și pornire

### Server
```bash
# Pornire server (ascultă pe portul 5000)
cd ConsoleAppServerBattleship
dotnet run
# Output: "Server started. Waiting for players..."
```

### Client
```bash
# Pornire client (se conectează la 127.0.0.1:5000)
cd WinFormsAppClientBattleship
dotnet run
```

> Serverul trebuie pornit **înaintea** clienților.  
> Ambii clienți trebuie porniți pe aceeași mașină (sau IP-ul serverului trebuie schimbat din `127.0.0.1`).

---

*Documentație generată pentru proiectul BattleshipBasescu — mai 2026*
