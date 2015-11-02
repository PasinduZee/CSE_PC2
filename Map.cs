using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tank_game
{
    class Map
    {
        //The main source
        public MapItem[,] grid{get;set;}
        
        //Array to keep players (only 5 can and player[0]th will be respected client ,other 4 positions=other clients)
        public Player[] players = new Player[5];

        //Number of players in the game (from countable numbers)
        public int player_count;

        public BasicCommandReader basicCommandReader=new BasicCommandReader();
         //Constructor to initialize map with all null values

        public Communicator com;
                
        public Map()
        {
            grid = new MapItem[10, 10];
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    grid[i, j] = null;
                }
            }
            com = Communicator.getInstance();
            com.setMap(this);
            com.StartListening();
        }
        //print the grid

        public void printMap()
        {
            for (int i = 0; i < 10; i++) 
            {
                for (int j = 0; j < 10; j++)
                {
                    if (grid[j, i] != null)
                    {
                        Console.Write(grid[j, i].name+" ");
                    }
                    else
                    {
                        Console.Write("N ");
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine("\n");
        }
        public void read(String read)
        {
            String readMsg = read.Substring(0, read.Length - 2);
            Char c = readMsg[0];
            if (c.Equals('S'))
            {
                Console.WriteLine("Type S found " + readMsg);
                readAcceptanceS(readMsg);
            }
            else if (c.Equals('I'))
            {
                Console.WriteLine("Type I found " + readMsg);
                readInitiationI(readMsg);
            }
            else if (c.Equals('G'))
            {
                Console.WriteLine("Type G found " + readMsg);
                readMovingG(readMsg);
            }
            
        }
        private void readAcceptanceS(String readMsg)
        {
            String[] mainSplit = readMsg.Split(':');
            String[] subSplit = mainSplit[1].Split(';');
            player_count += 1;
            //set the name in constructor
            players[0] = new Player(subSplit[0]);

            //set initial cordinates of the player
            players[0].cordinateX = Int32.Parse(subSplit[1][0]+"");
            players[0].cordinateY = Int32.Parse(subSplit[1][2]+"");

            //set initial dirctions
            players[0].direction = Int32.Parse(subSplit[2]+"");

            //update map from player
            grid[players[0].cordinateX, players[0].cordinateY] = players[0];

            Console.WriteLine("Mustank player no : " + players[0]);
            Console.WriteLine("Start Cordinate : " + players[0].cordinateX + "," + players[0].cordinateY);
            Console.WriteLine("Connected to the server");
        }
        private void readInitiationI(String readMsg)
        {
            try
            {
                String[] mainSplit = readMsg.Split(':');
                

                for (int i = 2; i < mainSplit.Length; i++)
                {
                    String[] cordinates = mainSplit[i].Split(';');
                    if (i == 2)
                    {
                        //initial positions of bricks
                        foreach (String cordinate in cordinates)
                        {
                            
                            this.grid[Int32.Parse(cordinate[0]+""), Int32.Parse(cordinate[2]+"")] = new Brick();
                        }
                    }
                    else if (i == 3)
                    {
                        foreach (String cordinate in cordinates)
                        {
                            this.grid[Int32.Parse(cordinate[0] + ""), Int32.Parse(cordinate[2] + "")] = new Stone();
                        }
                    }

                    else if (i == 4)
                    {
                        foreach (String cordinate in cordinates)
                        {
                            this.grid[Int32.Parse(cordinate[0] + ""), Int32.Parse(cordinate[2] + "")] = new Water();
                        }
                    }
                }

                this.printMap();
                Console.WriteLine("Map printed from type I");
            }
            catch (Exception exception)
            {
                Console.WriteLine("readMsg is "+readMsg+  "\n Exception occured "+exception.Message);
            }
        }
        private void readMovingG(String read)
        {
            String readMsg = read.Substring(0, read.Length - 2);
            Boolean b=basicCommandReader.Read(read);
            if (!b)
            {
                //code for handle b
                Console.WriteLine(readMsg);
            }
        }
    
    
    }
}
