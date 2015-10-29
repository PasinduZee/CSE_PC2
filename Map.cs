using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tank_game
{
    class Map
    {
        public Char[,] grid{get;set;}
        public String playerName { get; set; }

        private int startX;

        private int startY;

        private String startDirection;

        BasicCommand basicCommandhadler = BasicCommand.getInstance();
            
        public Map()
        {
            grid = new Char[10, 10];
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    grid[i, j] = 'N';
                }
            }
        }
        public void printGrid()
        {
            for (int i = 0; i < 10; i++) 
            {
                for (int j = 0; j < 10; j++)
                {
                    Console.Write(grid[j, i]+" ");
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
            this.printGrid();
            
        }
        private void readAcceptanceS(String readMsg)
        {
            String[] mainSplit = readMsg.Split(':');
            String[] subSplit = mainSplit[1].Split(';');
            this.playerName = subSplit[0];
            this.startX = Int32.Parse(subSplit[1][0]+"");
            this.startY = Int32.Parse(subSplit[1][2]+"");
            this.startDirection = (subSplit[2]);

            Console.WriteLine("Mustank player no : " + this.playerName);
            Console.WriteLine("Start Cordinate : " + startX + "," + startY);
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
                        foreach (String cordinate in cordinates)
                        {
                            
                            this.grid[Int32.Parse(cordinate[0]+""), Int32.Parse(cordinate[2]+"")] = 'B';
                        }
                    }
                    else if (i == 3)
                    {
                        foreach (String cordinate in cordinates)
                        {
                            this.grid[Int32.Parse(cordinate[0] + ""), Int32.Parse(cordinate[2] + "")] = 'S';
                        }
                    }

                    else if (i == 4)
                    {
                        foreach (String cordinate in cordinates)
                        {
                            this.grid[Int32.Parse(cordinate[0] + ""), Int32.Parse(cordinate[2] + "")] = 'W';
                        }
                    }
                }

                this.printGrid();
            }
            catch (Exception e)
            {
                Console.WriteLine(readMsg + "\n");
            }
        }
        private void readMovingG(String read)
        {
            String readMsg = read.Substring(0, read.Length - 2);
            basicCommandhadler.Read(read);
             
        }
    
    
    }
}
