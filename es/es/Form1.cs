using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Windows.Forms;

namespace es
{
    public partial class Form1 : Form
    {
        int[,] campop1 = new int[10, 10];//campo per valori primo giocatore
        int[,] campop2 = new int[10, 10];
        Button[,] btnp1 = new Button[10, 10];// bottoni per primo giocatore
        Button[,] btnp2 = null;
        List<Cnave> navi1;//liste navi per i due giocatori
        List<Cnave> navi2;
        int[] dim = { 1, 2, 2, 3, 3, 4 };//dimensioni navi da posizionare
        int contp1 = 0;//contatore navi posizionate primo giocatore
        int contp2 = 0;
        bool mode = true; // true = posizionamento, false = fase di sparo/gioco
        int x1 = -9;// coordinate temporanee per posizionamento navi (prima estremità)
        int y1 = -9;// coordinate temporanee per posizionamento navi (prima estremità)
        bool twoPlayer = false;// modalità 2 giocatori attiva
        bool placingPlayer1 = true; // chi sta piazzando navi: true=player1, false=player2
        int activePlayer = 1; // chi sta giocando: 1 o 2
        int x1_p1 = -9, y1_p1 = -9;
        int x1_p2 = -9, y1_p2 = -9;


        public Form1()
        {
            // Inizializzazione liste e componenti UI
            navi1 = new List<Cnave>();//crea navi per primo giocatore
            navi2 = new List<Cnave>();
            InitializeComponent();
            settaimpostazioni();//chiama la funzione per settare le impostazioni iniziali
        }

        // Crea la griglia 10x10 di bottoni per il player1 e inizializza campop1
        private void settaimpostazioni()//funzione per settare le impostazioni iniziali
        {
            btnp1 = new Button[10, 10];//creo la lista di bottoni per il primo giocatore
            for (int i = 0; i < 10; i++)//doppio ciclo per creare la griglia 10x10 e aggiungere i bottoni alla lista
            {
                for (int j = 0; j < 10; j++)
                {
                    campop1[i, j] = 0;//assegno 0 a tutte le posizioni del campo del primo giocatore
                    btnp1[i, j] = new Button();//creo il bottone
                    btnp1[i, j].Dock = DockStyle.Fill;
                    btnp1[i, j].Margin = new Padding(0);
                    btnp1[i, j].BackColor = Color.LightBlue;//colore di sfondo (acqua)
                    btnp1[i, j].Enabled = true;//schiacciabile
                    btnp1[i, j].Text = "";//nessun testo visibile
                    btnp1[i, j].Name = $"btn_{i}{j}";//nome del bottone utile per debug
                    btnp1[i, j].Font = new Font("Microsoft Sans Serif", 8);//font se servisse
                    btnp1[i, j].Click += btn_click;//associo l'evento click al metodo btn_click

                    tbl_pl1.Controls.Add(btnp1[i, j], j, i);//aggiungo il bottone alla TableLayoutPanel del player1
                }
            }

            // Imposta il messaggio iniziale per il posizionamento delle navi del player1
            lst_log.Text = displaynave(1);//mostro il messaggio per iniziare il posizionamento del primo giocatore
        }

        // Crea la griglia del player2 solo quando serve (lazy init)
        private void CreatePlayer2Grid()//funzione per creare la griglia del secondo giocatore
        {
            if (btnp2 != null) return; //se gia esiste ritorno

            btnp2 = new Button[10, 10];
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    campop2[i, j] = 0;
                    btnp2[i, j] = new Button();
                    btnp2[i, j].Dock = DockStyle.Fill;
                    btnp2[i, j].Margin = new Padding(0);
                    btnp2[i, j].BackColor = Color.LightBlue;
                    btnp2[i, j].Enabled = true;
                    btnp2[i, j].Text = "";
                    btnp2[i, j].Name = $"btn2_{i}{j}";
                    btnp2[i, j].Font = new Font("Microsoft Sans Serif", 8);
                    btnp2[i, j].Click += btn_click;

                    tbl_pl2.Controls.Add(btnp2[i, j], j, i);//aggiunge il bottone al pannello del player2
                }
            }
        }

        // Scrive nel ListBox log con controllo di eccezione
        private void Log(string text)//funzione per scrivere nel log passandogli il testo
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
            try
            {
                if (lst_log == null) return;//se la list box non esiste ritorno
                lst_log.Items.Add(text);//aggiungo il testo al log

                if (lst_log.Items.Count > 0)//se ci sono elementi nel log
                    lst_log.TopIndex = lst_log.Items.Count - 1;//scorro in basso per vedere l'ultimo messaggio
            }
            catch (Exception ex)//catturo l'eccezione
            {
                MessageBox.Show(ex.Message);//mostro il messaggio di errore (minimale)
            }
        }

        // Associa gli handler OnInterazione a tutte le navi del giocatore passato
        private void ascolta(int player)//funzione per ascoltare gli eventi passandogli il player
        {
            if (player == 1)
            {
                for (int i = 0; i < navi1.Count; i++)
                {
                    navi1[i].interazione += OnInterazione;
                }
            }
            else
            {
                for (int i = 0; i < navi2.Count; i++)
                {
                    navi2[i].interazione += OnInterazione;
                }
            }
        }

        private void OnInterazione(object sender, (bool col, bool aff, int x, int y, int id, int pg) intera)
        {
            Button[,] btn;
            int[,] campo;
            int col = intera.x;
            int row = intera.y;
            int attacker;
            string risultato;

            if (intera.pg == 1)
            {
                btn = btnp1;
                campo = campop1;
            }
            else
            {
                btn = btnp2;
                campo = campop2;
            }

            if (btn == null) return;

            // Colore base
            if (intera.col)
            {
                btn[row, col].BackColor = Color.Red;
            }
            else
            {
                btn[row, col].BackColor = Color.White;
            }

            // Se affondata, coloro tutte le caselle della nave
            if (intera.col && intera.aff)
            {
                for (int i = 0; i < campo.GetLength(0); i++)
                {
                    for (int j = 0; j < campo.GetLength(1); j++)
                    {
                        if (campo[i, j] == -intera.id)
                        {
                            btn[i, j].BackColor = Color.Black;
                        }
                    }
                }
            }
            tutto_Aff(intera.pg);

            // Determino chi è l’attaccante
            if (twoPlayer)
            {
                if (intera.pg == 1)
                {
                    attacker = 2;
                }
                else
                {
                    attacker = 1;
                }
            }
            else
            {
                attacker = 1;
            }

            if (intera.col)
            {
                if (intera.aff)
                {
                    risultato = "Affondato nave";
                }
                else
                {
                    risultato = "Colpito";
                }
            }
            else
            {
                risultato = "Mancato";
            }

            Log($"Giocatore {attacker} -> campo Giocatore {intera.pg}: {risultato} id {intera.id} in ({col},{row})");
        }

        private void cmezz_general(int x, int y, int giocatore)
        {
            List<Cnave> currentNavi;
            int[,] currentCampo;
            Button[,] currentBtns;
            int player;

            // Coordinate temporanee separate per player
            ref int x1_ref = ref (placingPlayer1 ? ref x1_p1 : ref x1_p2);
            ref int y1_ref = ref (placingPlayer1 ? ref y1_p1 : ref y1_p2);

            if (placingPlayer1)
            {
                currentNavi = navi1;
                currentCampo = campop1;
                currentBtns = btnp1;
                player = 1;
            }
            else
            {
                currentNavi = navi2;
                currentCampo = campop2;
                currentBtns = btnp2;
                player = 2;
            }

            int desiredLength = dim[currentNavi.Count];

            // Navi lunghezza 1 -> piazzamento immediato
            if (desiredLength == 1)
            {
                int[] pos = { x, y, x, y };
                int id = currentNavi.Count + 1;

                try
                {
                    currentNavi.Add(new Cnave(id, desiredLength, pos, currentCampo, currentBtns, giocatore));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                if (placingPlayer1) contp1++;
                else contp2++;

                Log($"Giocatore {player} ha piazzato nave lunghezza {desiredLength} in ({x},{y})");
                lst_log.Text = displaynave(player);

                if (currentNavi.Count == 6) ascolta(player);

                x1_ref = -9;
                y1_ref = -9;

                // Gestione passaggio turno 2P
                if (twoPlayer)
                {
                    if (placingPlayer1 && currentNavi.Count == 6)
                    {
                        placingPlayer1 = false;
                        lst_log.Text = displaynave(2);
                    }
                    else if (!placingPlayer1 && currentNavi.Count == 6)
                    {
                        if (navi1.Count == 6)
                        {
                            mode = false;
                            activePlayer = 1;
                            lst_log.Text = $"Inizia il gioco: turno del giocatore {activePlayer}";
                        }
                    }
                }

                return;
            }

            // Navi lunghezza >1 -> piazzamento in due click
            if (x1_ref == -9 && y1_ref == -9)
            {
                x1_ref = x;
                y1_ref = y;
                return;
            }

            int[] pos2 = { x1_ref, y1_ref, x, y };
            int id2 = currentNavi.Count + 1;

            try
            {
                currentNavi.Add(new Cnave(id2, desiredLength, pos2, currentCampo, currentBtns, giocatore));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            if (placingPlayer1) contp1++;
            else contp2++;

            Log($"Giocatore {player} ha piazzato nave lunghezza {desiredLength} da ({pos2[0]},{pos2[1]}) a ({pos2[2]},{pos2[3]})");
            lst_log.Text = displaynave(player);

            if (currentNavi.Count == 6) ascolta(player);

            x1_ref = -9;
            y1_ref = -9;

            // Passaggio turno 2P
            if (twoPlayer)
            {
                if (placingPlayer1 && currentNavi.Count == 6)
                {
                    placingPlayer1 = false;
                    lst_log.Text = displaynave(2);
                }
                else if (!placingPlayer1 && currentNavi.Count == 6)
                {
                    if (navi1.Count == 6)
                    {
                        mode = false;
                        activePlayer = 1;
                        lst_log.Text = $"Inizia il gioco: turno del giocatore {activePlayer}";
                    }
                }
            }
        }

        private void btn_click(object sender, EventArgs e)
        {
            try
            {
                Button click_btn = sender as Button;
                if (click_btn == null) return;

                // Funzione locale per gestire il tiro
                void HandleShot(int row, int col, int[,] campo, List<Cnave> navi, int pg)
                {
                    bool handled = false;

                    for (int i = 0; i < navi.Count; i++)
                    {
                        if (navi[i].ProcessShot(row, col, campo))
                        {
                            handled = true;
                            break;
                        }
                    }

                    // Se nessuna nave ha gestito il colpo, segno acqua colpita
                    if (!handled)
                    {
                        Button[,] btns;
                        if (campo[row, col] <= 0) campo[row, col] = -11;

                        if (pg == 1)
                        {
                            btns = btnp1;
                        }
                        else
                        {
                            btns = btnp2;
                        }
                        if (btns != null && btns[row, col] != null)
                        {
                            btns[row, col].BackColor = Color.White;
                            btns[row, col].UseVisualStyleBackColor = false;
                            btns[row, col].Invalidate();
                        }

                        // Chiamo evento interazione per aggiornare log
                        if (navi.Count > 0)
                        {
                            navi[0].check(col, row, campo);
                        }
                    }
                }

                // Cerco click nella griglia player 1
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (btnp1[i, j] == click_btn)
                        {
                            if (mode) // posizionamento
                            {
                                if (!twoPlayer || (twoPlayer && placingPlayer1))
                                {
                                    cmezz_general(j, i, 1);
                                    return;
                                }
                                else
                                {
                                    MessageBox.Show("È il turno di posizionamento del giocatore 2.");
                                    return;
                                }
                            }
                            else // fase di gioco
                            {
                                if (twoPlayer && activePlayer == 2)
                                {
                                    HandleShot(i, j, campop1, navi1, 1);
                                    activePlayer = 1;
                                    lst_log.Text = $"Turno del giocatore {activePlayer}";
                                    return;
                                }
                                else if (!twoPlayer) // single-player
                                {
                                    HandleShot(i, j, campop1, navi1, 1);
                                    return;
                                }
                                else
                                {
                                    MessageBox.Show($"Non è il turno del giocatore {activePlayer}.");
                                    return;
                                }
                            }
                        }
                    }
                }

                // Cerco click nella griglia player 2 (PvP)
                if (btnp2 != null)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            if (btnp2[i, j] == click_btn)
                            {
                                if (mode) // posizionamento
                                {
                                    if (twoPlayer && !placingPlayer1)
                                    {
                                        cmezz_general(j, i, 2);
                                        return;
                                    }
                                    else
                                    {
                                        MessageBox.Show("È il turno di posizionamento del giocatore 1.");
                                        return;
                                    }
                                }
                                else // fase di gioco
                                {
                                    if (twoPlayer && activePlayer == 1)
                                    {
                                        HandleShot(i, j, campop2, navi2, 2);
                                        activePlayer = 2;
                                        lst_log.Text = $"Turno del giocatore {activePlayer}";
                                        return;
                                    }
                                    else
                                    {
                                        MessageBox.Show($"Non è il turno del giocatore {activePlayer}.");
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Genera la stringa descrittiva per il pannello di log che indica lo stato del posizionamento
        private string displaynave(int player)
        {
            int ct;

            if (player == 1)
            {
                ct = contp1;
            }
            else
            {
                ct = contp2;
            }

            switch (ct)
            {
                case 0:
                    if (player == 1)
                    {
                        return "Giocatore 1: inizia il posizionamento";
                    }
                    else
                    {
                        return "Giocatore 2: inizia il posizionamento";
                    }
                case 1:
                    return $"Hai piazzato 1 nave. Posiziona la successiva di lunghezza 2 (Giocatore {player})";
                case 2:
                    return $"Hai piazzato 2 navi. Prossima lunghezza 3 (Giocatore {player})";
                case 3:
                    return $"Hai piazzato 3 navi. Prossima lunghezza 4 (Giocatore {player})";
                case 4:
                    return $"Hai piazzato 4 navi. Prossima lunghezza 5 (Giocatore {player})";
                case 5:
                    return $"Hai piazzato 5 navi. Prossima lunghezza 6 (Giocatore {player})";
                case 6:
                    return $"Giocatore {player}: hai piazzato tutte le navi.";
                default:
                    return $"Giocatore {player}: Hai piazzato {ct} navi.";
            }
        }

        // Bottone che cambia la modalità (posizionamento <-> gioco)
        // Quando si passa a posizionamento rimette i colori di btnp1 in LightBlue
        // Quando si passa a gioco colora le posizioni con navie presenti in grigio (mostra le navi)
        private void btn_chmode_Click(object sender, EventArgs e)
        {
            if (mode)
            {
                // Passando da posizionamento -> gioco mostro l'acqua (azzurro) su tutte le caselle visibili.
                // Se sono in PvP, devo aggiornare anche la griglia del player2 (non solo player1).
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (btnp1[i, j] != null)
                        {
                            btnp1[i, j].BackColor = Color.LightBlue;
                        }
                    }
                }

                // Aggiorno anche la griglia del player2 se esiste (PvP)
                if (btnp2 != null)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            if (btnp2[i, j] != null)
                            {
                                btnp2[i, j].BackColor = Color.LightBlue;
                            }
                        }
                    }
                }

                mode = !mode;
            }
            else
            {
                // Passando da gioco -> posizionamento: evidenzio le celle che contengono navi (Gray)
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (campop1[i, j] > 0 && btnp1[i, j] != null)
                        {
                            btnp1[i, j].BackColor = Color.Gray;
                        }
                    }
                }

                // Aggiorno anche la griglia del player2 se esiste (PvP)
                if (btnp2 != null)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            if (campop2[i, j] > 0 && btnp2[i, j] != null)
                            {
                                btnp2[i, j].BackColor = Color.Gray;
                            }
                        }
                    }
                }

                mode = !mode;
            }
        }

        // Attiva la modalità 2 giocatori: crea la seconda griglia, resetta gli stati e prepara il posizionamento
        private void btn_2p_Click_1(object sender, EventArgs e)
        {
            if (twoPlayer)
            {
                MessageBox.Show("Modalità 2 giocatori già attiva.");
                return;
            }

            twoPlayer = true;
            placingPlayer1 = true;
            mode = true;
            activePlayer = 1;

            // reset strutture dati
            navi1 = new List<Cnave>();
            navi2 = new List<Cnave>();
            contp1 = 0;
            contp2 = 0;
            x1 = -9;
            y1 = -9;

            // reset campop1 e colori bottoni
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    campop1[i, j] = 0;
                    if (btnp1[i, j] != null)
                    {
                        btnp1[i, j].BackColor = Color.LightBlue;
                    }
                }
            }

            CreatePlayer2Grid();

            // reset campop2 e colori bottoni
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    campop2[i, j] = 0;
                    if (btnp2 != null && btnp2[i, j] != null)
                    {
                        btnp2[i, j].BackColor = Color.LightBlue;
                    }
                }
            }

            lst_log.Text = displaynave(1);
            Log("Modalità 2 giocatori attivata. Inizia piazzamento Giocatore 1.");
        }
        private void tutto_Aff(int player)
        {
            if (player == 1)
            {
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (campop1[i, j] > 0)
                        {
                            return;
                        }
                    }
                }

                if(twoPlayer)
                {
                    MessageBox.Show("il giocatore 2 ha vinto");
                }
                else
                {
                    MessageBox.Show("Hai vinto!");
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (campop2[i, j] > 0)
                        {
                            return;
                        }
                    }
                }
                MessageBox.Show("il giocatore 1 ha vinto");
            }
        }
    }
}