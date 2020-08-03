using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;

namespace BrettspielEngine
{
    public partial class Form1 : Form
    {
        bool gameStarted = true;
        public class Player
        {
            public List<Tile> figures = new List<Tile>();
            public Bitmap figureImage;
            public int number;
            public int endTile;
            public int numHouseFigures = 4;

            public Player(Bitmap figureImage, int number)
            {
                this.figureImage = figureImage;
                this.number = number;
            }
        }
        static Player[] players;
        Player currentPlayer;
        Dictionary<Tile, Tile> tileDict = new Dictionary<Tile, Tile>();

        public class Tile
        {
            public string kind;
            public PictureBox picBox; //The picBox that belongs to this instance
            public Color defaultColor;
            public int tileNum;
            private int player; //What player stands on this tile? -1 = none
            public int Player
            {
                get { return player; }
                set
                {
                    if (value != -1) //Assign this pictureBox to a player and show figure image
                    {
                        players[value].figures.Add(this);
                        if (picBox != null)
                            picBox.Image = players[value].figureImage;
                    }
                    else //no new player on tile
                    {
                        players[player].figures.Remove(this);
                        if(picBox != null)
                            picBox.Image = null;
                    }

                    player = value;
                }
            }

            public Tile(int player, int tileNum, PictureBox picBox, Color defaultColor, string kind)
            {
                Player = player;
                this.tileNum = tileNum;
                this.picBox = picBox;
                this.defaultColor = defaultColor;
                this.kind = kind;
            }
        }
        Tile[] gameTiles = new Tile[40];
        Tile[] houseTiles = new Tile[16];
        Tile[] endTiles = new Tile[16];
        Dictionary<string, Tile[]> tiles;
        string status = "rollDie";
        int eyes;
        Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
        int emptyRolls = 0;

        public Form1()
        {
            InitializeComponent();

            GenerateAssets();
            tiles = new Dictionary<string, Tile[]> { { "game", gameTiles }, { "house", houseTiles }, { "end", endTiles } };
            players = new Player[4] { new Player(bitmaps["red"], 0 ),
                                      new Player(bitmaps["blue"], 1 ),
                                      new Player(bitmaps["yellow"], 2 ),
                                      new Player(bitmaps["green"], 3 ) };

            //Place figures
            housered0.Image = bitmaps["red"];
            housered1.Image = bitmaps["red"];
            housered2.Image = bitmaps["red"];
            housered3.Image = bitmaps["red"];
            houseblue0.Image = bitmaps["blue"];
            houseblue1.Image = bitmaps["blue"];
            houseblue2.Image = bitmaps["blue"];
            houseblue3.Image = bitmaps["blue"];
            houseyellow0.Image = bitmaps["yellow"];
            houseyellow1.Image = bitmaps["yellow"];
            houseyellow2.Image = bitmaps["yellow"];
            houseyellow3.Image = bitmaps["yellow"];
            housegreen0.Image = bitmaps["green"];
            housegreen1.Image = bitmaps["green"];
            housegreen2.Image = bitmaps["green"];
            housegreen3.Image = bitmaps["green"];

            currentPlayer = players[0];

            //PictureBox data structure: 
            //Tag = index
            //Name = kind ( game / house / end )

            foreach (Control c in Controls)
            {
                if (c is PictureBox && (c as PictureBox).Name != "die") //If it is a PictureBox but not the die
                {
                    PictureBox p = c as PictureBox;
                    int pIndex = Convert.ToInt32(p.Tag); //Get Index

                    int player = -1;
                    if (p.Image == bitmaps["red"])
                        player = 0;
                    else if (p.Image == bitmaps["blue"])
                        player = 1;
                    else if (p.Image == bitmaps["yellow"])
                        player = 2;
                    else if (p.Image == bitmaps["green"])
                        player = 3;

                    Color defaultColor = p.BackColor;

                    //Get and sort kind
                    if (p.Name.Contains("game"))
                        gameTiles[pIndex] = new Tile(player, pIndex, p, defaultColor, "game");
                    else  if(p.Name.Contains("house"))
                    {
                        houseTiles[pIndex + player*4] = new Tile(player, pIndex, p, defaultColor, "house");
                    }
                    else if (p.Name.Contains("end"))
                    {
                        int playerKacke = -1;
                        if (p.BackColor == Color.Red)
                            playerKacke = 0;
                        else if (p.BackColor == Color.Blue)
                            playerKacke = 1;
                        else if (p.BackColor == Color.Yellow)
                            playerKacke = 2;
                        else if (p.BackColor == Color.Lime)
                            playerKacke = 3;

                        endTiles[playerKacke*4 + pIndex] = new Tile(player, pIndex, p, defaultColor, "end");
                    }
                }
            }

            //Set endTiles
            players[0].endTile = 39;
            players[1].endTile = 9;
            players[2].endTile = 19;
            players[3].endTile = 29;

            KeyDown += RollDie;
        }

        private void GenerateAssets()
        {
            System.Resources.ResourceSet set = Properties.Resources.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true);
            foreach (DictionaryEntry entry in set)
            {
                bitmaps.Add((string)entry.Key, (Bitmap)entry.Value);
            }
        }

        private void RollDie(object sender, EventArgs e)
        {
            if (gameStarted)
            {
                //KeyEventArgs b = (KeyEventArgs)e;
                if (status == "rollDie")
                {
                    Random random = new Random();
                    eyes = random.Next(1, 7);
                    //eyes = b.KeyValue - 48;
                    die.Image = bitmaps["_" + eyes.ToString()];

                    MarkFigures();
                }
            }
        }

        private void MarkFigures()
        {
            foreach (Tile figureTile in currentPlayer.figures)
            {
                if (figureTile.kind == "house")
                {
                    if (eyes == 6 && gameTiles[currentPlayer.number*10].Player != currentPlayer.number) //6 rolled and startTile not from same player
                    {
                        Tile newTile = gameTiles[currentPlayer.number * 10]; //newTile is startTile
                        tileDict.Clear(); //Remove all other marked tiles, because this tile has to be moved
                        tileDict.Add(figureTile, newTile);
                        break; //Dont check for any other tiles
                    }
                }
                else if (figureTile.kind == "game")
                {
                    //Would this step cross the endTile for this player?
                    int oldTileNumber = figureTile.tileNum;
                    int newTileNumber = figureTile.tileNum + eyes;
                    if (!(currentPlayer.endTile >= oldTileNumber && currentPlayer.endTile < newTileNumber))
                    {
                        //go from game tile to game tile
                        if (newTileNumber >= 40)
                            newTileNumber -= 40;
                        Tile newTile = gameTiles[newTileNumber];

                        if (newTile.Player != currentPlayer.number) //No same player on new tile
                            tileDict.Add(figureTile, newTile); //Go ahead and mark it

                        if (oldTileNumber == currentPlayer.number * 10 && currentPlayer.numHouseFigures > 0) //figure stands on start tile and figures are in the house
                        {
                            Weitergeben(newTile); //Checks if this figure which has to move can move, if it's blocked, check if blocking figure can move and so on, once final figure is found, remove all others
                            break; //Don't look for any other tiles
                        }

                    }
                    else //go from game tile to end tile
                    { 
                        newTileNumber -= currentPlayer.endTile +1;
                        if (newTileNumber <= 3)
                        {
                            Tile newTile = endTiles[currentPlayer.number*4 + newTileNumber];
                            bool figureOnTheWay = false;
                            for (int i = currentPlayer.number * 4; i <= currentPlayer.number * 4 + newTileNumber; i++)
                            {
                                if (endTiles[i].Player == currentPlayer.number) //No same player on any end tile on the way
                                {
                                    figureOnTheWay = true;
                                    break;
                                }
                            }
                            if (!figureOnTheWay)
                                tileDict.Add(figureTile, newTile);
                        }
                    }
                }
                else if (figureTile.kind == "end")
                {
                    int newTileNumber = figureTile.tileNum + eyes;
                    if (newTileNumber <= 3)
                    {
                        Tile newTile = endTiles[currentPlayer.number * 4 + newTileNumber];
                        bool figureOnTheWay = false;
                        for (int i = currentPlayer.number * 4 + figureTile.tileNum + 1; i <= currentPlayer.number * 4 + newTileNumber; i++)
                        {
                            if (endTiles[i].Player == currentPlayer.number) //No same player on any end tile on the way
                            {
                                figureOnTheWay = true;
                                break;
                            }
                        }
                        if (!figureOnTheWay)
                            tileDict.Add(figureTile, newTile);
                    }
                }
            }

            if (tileDict.Count > 0) //At least one tile was marked
            {
                if (tileDict.Count > 1) //Schlagzwang, if there's a pair that kicks another player, then remove all others that don't kick players
                {
                    if(tileDict.Any(t => t.Value.Player != -1 && t.Value.Player != currentPlayer.number)) //Opponent player on new tile (value)
                        tileDict = tileDict.Where(t => t.Value.Player != -1).ToDictionary(t => t.Key, t => t.Value);
                }


                //Mark all old and new Tiles
                foreach (KeyValuePair<Tile, Tile> pair in tileDict)
                {
                    pair.Key.picBox.BackColor = Color.Gray; //oldTile
                    pair.Value.picBox.BackColor = Color.HotPink; //newTile
                }

                status = "moveFigure";
            }
            else //nothing was marked, so check if any figures in end could move
            {
                int figuresThatCantMove = currentPlayer.numHouseFigures;
                for (int t = currentPlayer.number * 4 + 3; t >= currentPlayer.number * 4; t--) //Start on last endTile
                {
                    if (endTiles[t].Player == currentPlayer.number) //Check if its occupied
                        if (endTiles[t + 1].Player != currentPlayer.number) //Check if it can't move
                            figuresThatCantMove++;
                }

                if (figuresThatCantMove == 4) //No one can move at all, so count up to three tries
                {
                    status = "rollDie";
                    emptyRolls++;

                    if (emptyRolls == 3)
                    {
                        NextPlayer();
                    }
                }
                else //At least one figure could move
                {
                    NextPlayer();
                }
            }
        }

        private void Weitergeben(Tile newTile)
        {
            if (newTile.Player == currentPlayer.number) //Same player on new tile
            {
                Weitergeben(gameTiles[(newTile.tileNum + eyes) % 40]);
            }
            else
            {

                tileDict.Clear(); //Remove all other marked tiles, because this tile has to be moved
                tileDict.Add(gameTiles[(newTile.tileNum - eyes) % 40], newTile);
            }
        }

        private void NextPlayer()
        {
            emptyRolls = 0; //Reset the emptyRolls

            if (currentPlayer.number == 0)
                currentPlayer = players[2];
            else
                currentPlayer = players[0];

            playerLabel.Text = "PLAYER " + (currentPlayer.number + 1);
        }

        private void TileClick(object sender, EventArgs e)
        {
            PictureBox p = sender as PictureBox;
            if (status == "moveFigure" && p.BackColor == Color.Gray)
            {
                //What tile was clicked?
                Tile clickedTile = null;
                foreach (Tile tile in tileDict.Keys)
                    if (tile.picBox == p)
                        clickedTile = tile;

                Tile newTile = tileDict[clickedTile];
                //If new tile has a different player on it, then move the different player to his house
                if (newTile.Player != -1) //opponent
                {
                    players[newTile.Player].figures.Remove(newTile);
                    //Find free house tile
                    for (int i = 0; i < 4; i++)
                    {
                        if (houseTiles[newTile.Player*4 + i].picBox.Image == null)
                        {
                            //players[newTile.Player].figures.Add(houseTiles[newTile.Player * 4 + i]); //Opponent player add houseTile
                            //houseTiles[newTile.Player * 4 + i].picBox.Image = players[newTile.Player].figureImage; //opponents houseTile has image
                            houseTiles[newTile.Player * 4 + i].Player = newTile.Player; //opponents houseTile has Player property of opponent player
                            players[newTile.Player].numHouseFigures++; //newTile's player has 1 more in house
                            break;
                        }
                    }
                }

                newTile.Player = clickedTile.Player; //Transfer player property from old to new tile
                clickedTile.Player = -1;

                //Unmark all marked tiles
                foreach (KeyValuePair<Tile, Tile> pair in tileDict)
                {
                    pair.Key.picBox.BackColor = pair.Key.defaultColor; //oldTile
                    pair.Value.picBox.BackColor = pair.Value.defaultColor; //newTile
                }

                //Empty tileDict
                tileDict.Clear();

                if (clickedTile.kind == "house") //Decrease house figure count for checking whether a figure on the start tile has to be moved or not
                    currentPlayer.numHouseFigures--;

                if(eyes != 6) //Roll again when 6 was rolled
                    NextPlayer();

                status = "rollDie";
            }
        }

    }
}