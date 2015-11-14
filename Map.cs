using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tank_game
{
    class Map
    {
        #region MapVariables
        //The map grid
        public MapItem[,] grid{get;set;}

        //coins 
        public List<Coin> coin_queue { get; set; }
        
        //health packs
        public List<HealthPack> health_pack_queue { get; set; }
        
        
        //Array to keep players (only 5 can and player[0]th will be respected client ,other 4 positions=other clients)
        public Player[] players = new Player[5];

        //client id in the server
        public int myid { get; set; }

        //Number of players in the game (from countable numbers)
        public int player_count;

        //Constructor to initialize map with all null values
        public BasicCommandReader basicCommandReader=new BasicCommandReader();
         
        public Communicator com;

        //method for check initial player names are set;
        private Boolean isNameSet;

        #endregion
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
            coin_queue = new List<Coin>();
            health_pack_queue = new List<HealthPack>();
            
            com = Communicator.getInstance();
            com.setMap(this);
            com.StartListening();

        }

        #region MapUpdateFunctions
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
            //Console.WriteLine("Type "+c+" found " + readMsg);
            if (c.Equals('S'))
            {   
                readAcceptanceS(readMsg);
            }
            else if (c.Equals('I'))
            {
                readInitiationI(readMsg);
            }
            else if (c.Equals('G'))
            {
                readMovingG(readMsg);
            }
            else if (c.Equals('C'))
            {
                readCoinC(readMsg);
            }
            else if (c.Equals('L'))
            {
                readHealthPackL(readMsg);
            }

            //update coin timers
            for (int i = 0; i < coin_queue.Count;i++ )
            {
                bool vanished = coin_queue[i].timer_update();
                if(vanished)
                {
                    grid[coin_queue[i].x_cordinate, coin_queue[i].y_cordinate] = null;
                    coin_queue.RemoveAt(i);
                }
            }
            
            //update health pack timers
            for (int i = 0; i <health_pack_queue.Count; i++)
            {
                bool vanished = health_pack_queue[i].timer_update();
                if (vanished)
                {
                    grid[health_pack_queue[i].x_cordinate, health_pack_queue[i].y_cordinate] = null;
                    health_pack_queue.RemoveAt(i);
                }
            }
            
        }
        private void readAcceptanceS(String readMsg)
        {//S:P1: 1,1:0
            
            String[] mainSplit = readMsg.Split(':');
            String[] subSplit = mainSplit[1].Split(';');
            myid=Int32.Parse(subSplit[0][1]+"");
            player_count += 1;

            //set the name in constructor
            players[myid] = new Player(subSplit[0]);

            //set initial cordinates of the player
            players[myid].cordinateX = Int32.Parse(subSplit[1][0]+"");
            players[myid].cordinateY = Int32.Parse(subSplit[1][2]+"");

            //set initial dirctions
            players[myid].direction = Int32.Parse(subSplit[2]+"");

            //update map from player
            grid[players[myid].cordinateX, players[myid].cordinateY] = players[myid];

            Console.WriteLine("Mustank player no : " + players[myid]);
            Console.WriteLine("Start Cordinate : " + players[myid].cordinateX + "," + players[myid].cordinateY);
            Console.WriteLine("Connected to the server \n");
            
        }
        private void readInitiationI(String readMsg)
        {   //I:P<num>: < x>,<y>;< x>,<y>;< x>,<y>…..< x>,<y>: < x>,<y>;< x>,<y>;< x>,<y>…..< x>,<y>: < x>,<y>;< x>,<y>;< x>,<y>…..< x>,<y>
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
        {/*G:P1;< player location  x>,< player location  y>;<Direction>;< whether shot>;<health>;< coins>;< points>:
            …. P5;< player location  x>,< player location  y>;<Direction>;< whether shot>;<health>;
                < coins>;< points>: < x>,<y>,<damage-level>;< x>,<y>,<damage-level>;< x>,<y>,<damage-level>;
                < x>,<y>,<damage-level>…..< x>,<y>,<damage-level> */
            
            
            Boolean b=basicCommandReader.Read(read);
            if (!b)
            {
                

                String[] mainSplit = read.Split(':');
                int playerC = mainSplit.Count()-2;
                for (int i = 1; i < playerC + 1; i++)
                {
                    
                    String[] playerSplit = mainSplit[i].Split(';');
                    int playerNum = Int32.Parse(playerSplit[0][1] + "");
                    if (!isNameSet)
                    {
                        players[playerNum].name = playerNum.ToString();
                    }
                    players[playerNum].cordinateX=Int32.Parse(playerSplit[1][0]+"");
                    players[playerNum].cordinateY= Int32.Parse(playerSplit[1][2] + "");
                    players[playerNum].direction = Int32.Parse(playerSplit[2] + "");
                    players[playerNum].whetherShot = Int32.Parse(playerSplit[3] + "");
                    players[playerNum].health = Int32.Parse(playerSplit[4] + "");
                    players[playerNum].coins = Int32.Parse(playerSplit[5] + "");
                    players[playerNum].points = Int32.Parse(playerSplit[6] + "");
                }
                String[] brickSplit = mainSplit[mainSplit.Count()-1].Split(';');
                int brickCount = brickSplit.Count();
                for (int j = 0; j < brickCount; j++)
                {
                    String[] brick = brickSplit[j].Split(',');
                    int damage_val=Int32.Parse(brick[2] + "");
                    if (damage_val == 0 && grid[Int32.Parse(brick[0] + ""), Int32.Parse(brick[1] + "")]!=null)
                    {
                        
                        ((Brick)(grid[Int32.Parse(brick[0] + ""), Int32.Parse(brick[1] + "")])).health = 0;
                        grid[Int32.Parse(brick[0] + ""), Int32.Parse(brick[1] + "")]=null;
                    
                    }
                    else{
                        ((Brick)(grid[Int32.Parse(brick[0] + ""), Int32.Parse(brick[1] + "")])).health = (4-damage_val)*25;
                    }
                }

                isNameSet = true;
            }
        }
        private void readCoinC(String read)
        {   //C:<x>,<y>:<LT>:<Val>#


            String[] mainSplit = read.Split(':');
            int x=Int32.Parse(mainSplit[1][0] + "");
            int y=Int32.Parse(mainSplit[1][2] + "");
            Coin coin_pile = new Coin(x, y, Int32.Parse(mainSplit[2] + ""), Int32.Parse(mainSplit[3] + ""));
            coin_queue.Add(coin_pile);
            grid[x, y] = coin_pile;
        }
        private void readHealthPackL(String read)
        {   //L:<x>,<y>:<LT>#

            String[] mainSplit = read.Split(':');
            int x = Int32.Parse(mainSplit[1][0] + "");
            int y = Int32.Parse(mainSplit[1][2] + "");
            HealthPack health_pack = new HealthPack(x, y, Int32.Parse(mainSplit[2] + ""));
            health_pack_queue.Add(health_pack);
            grid[x, y] = health_pack;
        }
        #endregion

    }
}
