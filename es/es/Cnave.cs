using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace es
{
    internal class Cnave
    {
        private int id;
        private int numcaselle;
        private int giocatore;
        public event EventHandler<(bool colpita, bool aff, int x, int y, int id, int pg)> interazione;


        public Cnave(int id, int numcaselle, int[] da_a, int[,] campo, Button[,] btn, int giocatore)//costruisce la nave
        {
            this.id = id;
            this.numcaselle = numcaselle;
            settanave(campo, da_a, btn);//funzione che posiziona la nave nel campo
            this.giocatore = giocatore;
        }

        private void settanave(int[,] campo, int[] da_a, Button[,] btn)
        {
            if (da_a == null || da_a.Length < 4)//se l'array di coordinate non è valido lancia eccezione
            {
                throw new ArgumentException("Array di coordinate non valido (deve contenere xStart,yStart,xEnd,yEnd).");
            }

            if (campo == null)//se il campo è null lancia eccezione
            {
                throw new ArgumentNullException(nameof(campo));
            }
            
            if (btn == null)//se i bottoni sono null lancia eccezione
            {
                throw new ArgumentNullException(nameof(btn));
            }

            //varie variabili per le coordinate
            int x1 = da_a[0];
            int y1 = da_a[1];
            int x2 = da_a[2]; 
            int y2 = da_a[3];

            int rows = campo.GetLength(0);
            int cols = campo.GetLength(1);

            //controllo coordinate valide
            if (x1 < 0 || x1 >= cols || x2 < 0 || x2 >= cols || y1 < 0 || y1 >= rows || y2 < 0 || y2 >= rows)
            {
                throw new ArgumentOutOfRangeException("Una o più coordinate sono fuori dal campo di gioco.");
            }

            //guardo se diagonale
            if (x1 != x2 && y1 != y2)
            {
                throw new ArgumentException("Coordinate non valide: la nave deve essere posizionata in orizzontale o verticale.");
            }

            //variabili per posizionamento
            int startX;
            int endX;
            int startY;
            int endY;
            int length;
            if (x1 == x2)//se orizzontale
            {
                startX = x1;
                endX = x2;
                startY = Math.Min(y1, y2);
                endY = Math.Max(y1, y2);
                length = endY - startY + 1;
            }
            else
            {
                startY = endY = y1;
                startX = Math.Min(x1, x2);
                endX = Math.Max(x1, x2);
                length = endX - startX + 1;
            }

            //se lunghezza non corrisponde a numcaselle lancia eccezione
            if (length != numcaselle)
            {
                throw new ArgumentException($"Lunghezza selezionata ({length}) non corrisponde a numcaselle ({numcaselle}).");
            }

            //guardo se ci sono sovrapposizioni
            for (int yy = startY; yy <= endY; yy++)
            {
                for (int xx = startX; xx <= endX; xx++)
                {
                    if (campo[yy, xx] != 0)//se non vuota lancia eccezione
                    {
                        throw new InvalidOperationException($"Sovrapposizione: la cella ({xx},{yy}) è già occupata.");
                    }
                }
            }

            //se tutto ok posiziona nave
            for (int yy = startY; yy <= endY; yy++)
            {
                for (int xx = startX; xx <= endX; xx++)
                {
                    campo[yy, xx] = this.id;
                    if (yy >= 0 && yy < btn.GetLength(0) && xx >= 0 && xx < btn.GetLength(1))
                    {
                        btn[yy, xx].BackColor = Color.Gray;
                    }
                }
            }
        }

        public bool ProcessShot(int x, int y, int[,] campo)//funzione per vedere se il colpo è su questa nave
        {
            if (campo == null)//se il campo è null lancia eccezione
            {
                throw new ArgumentNullException(nameof(campo));
            }

            int rows = campo.GetLength(0);
            int cols = campo.GetLength(1);

            if (x < 0 || x >= cols || y < 0 || y >= rows)//se coordinate fuori dal campo lancia eccezione
            {
                throw new ArgumentOutOfRangeException("Coordinate fuori dal campo.");
            }

            int cell = campo[y, x];
            if (cell <= 0)//se acqua o cella già colpita
            {
                return false;
            }

            if (cell != this.id)//se appartiene ad un'altra nave
            {
                return false;
            }

            campo[y, x] = -cell;//segno la cella come colpita

            bool pezziRimasti = false;

            // verifico se ci sono ancora celle positive della stessa nave
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (campo[i, j] == cell)
                    {
                        pezziRimasti = true;
                        break;
                    }
                }
                if (pezziRimasti) break;
            }

            if (!pezziRimasti)
            {
                // nave affondata
                interazione?.Invoke(this, (true, true, x, y, this.id, giocatore));
            }
            else
            {
                // solo colpita
                interazione?.Invoke(this, (true, false, x, y, this.id, giocatore));
            }

            return true;
        }
        //differenza tra check e ProcessShot è che check gestisce anche i colpi a vuoto
        public void check(int x, int y, int[,] campo)
        {
            if (campo == null)//se il campo è null lancia eccezione
            {
                throw new ArgumentNullException(nameof(campo));
            }

            int rows = campo.GetLength(0);
            int cols = campo.GetLength(1);

            if (x < 0 || x >= cols || y < 0 || y >= rows)//se coordinate fuori dal campo lancia eccezione
            {
                throw new ArgumentOutOfRangeException("Coordinate fuori dal campo.");
            }

            if (campo[y, x] == 0 || campo[y, x] < -10)//se non c'è nessuna nave
            {
                campo[y, x] = -11;   //segno l'acqua colpita
                interazione?.Invoke(this, (false, false, x, y, this.id, giocatore));//invoco l'evento per far capire che ho fatto
                return;
            }

            int nid = campo[y, x];//se c'è una nave prendo il suo id
            campo[y, x] = -nid;//cella colpita

            bool pezziRimasti = false;//variabile per vedere se ci sono pezzi rimasti

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (campo[i, j] == nid)//se trovo un pezzo rimasto blocco tutto
                    {
                        pezziRimasti = true;
                        break;
                    }
                }
                if (pezziRimasti) break;
            }

            if (!pezziRimasti)//se non ci sono pezzi rimasti invoco l'evento affondata
            {
                interazione?.Invoke(this, (true, true, x, y, this.id, giocatore));
            }
            else
            {
                interazione?.Invoke(this, (true, false, x, y, this.id, giocatore));//se no evento solo colpita
            }
        }

    }
}