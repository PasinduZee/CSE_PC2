using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace tank_game
{
    class Map
    {
        #region MapVariables
        
        public MapItem[,] grid{get;set;} //Map
        public List<Coin> coin_queue { get; set; } //Coins
        public List<HealthPack> health_pack_queue { get; set; } //HealthPacks
        public int myid { get; set; }//My player id in the server
        public String map_string { get; set; } //map character grid string for cmd
        public Player[] players = new Player[5]; //players
        public int player_count;//Number of players in the game (from countable numbers)
        public BasicCommandReader basicCommandReader=new BasicCommandReader();
        public BasicCommandSender basicCommandSender=new BasicCommandSender();
        public Communicator com;
        public bool coin_on=false; //temperary attribute for algorithm checking
        #endregion

        //Constructor to initialize map with all EmptyCells
        public Map()
        {
            grid = new MapItem[10, 10];
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    grid[i, j] = new EmptyCell();
                }
            }
            
            clearMapForBFS();
            coin_queue = new List<Coin>();
            health_pack_queue = new List<HealthPack>();
            
            com = Communicator.getInstance();
            com.setMap(this);
            com.StartListening();
            map_string = "";
        }

        #region MapUpdateFunctions

        public void updateMapString() 
        {
            string map = "";
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                     
                     map=map+grid[i, j].name+"  "; 
                            
                }
                map = map + "\n";

            }
            Console.WriteLine("");
            Console.WriteLine(map);
            this.map_string = map;

        }
  
        public String getMapString()
        {
            return this.map_string;
        }


        #endregion


        #region map update functions
        private void updateCoinAqquire()
        {
            foreach (Player player in players)
            {
                if (player != null)
                {
                    for (int i = 0; i < coin_queue.Count; i++)
                    {
                        if (coin_queue[i].x_cordinate == player.cordinateX && coin_queue[i].y_cordinate == player.cordinateY)
                        {
                            grid[coin_queue[i].x_cordinate, coin_queue[i].y_cordinate] = new EmptyCell();
                            coin_queue.RemoveAt(i);
                        }
                    }
                }
            }
        }

        private void timerUpdateCoin() 
        {
            for (int i = 0; i < coin_queue.Count; i++)
            {
                bool vanished = coin_queue[i].timer_update();
                if (vanished)
                {
                    grid[coin_queue[i].x_cordinate, coin_queue[i].y_cordinate] = new EmptyCell();
                    coin_queue.RemoveAt(i);
                }
            }
        }

        private void timerUpdateHealthPack() 
        {
            for (int i = 0; i < health_pack_queue.Count; i++)
            {
                bool vanished = health_pack_queue[i].timer_update();
                if (vanished)
                {
                    grid[health_pack_queue[i].x_cordinate, health_pack_queue[i].y_cordinate] = new EmptyCell();
                    health_pack_queue.RemoveAt(i);
                }
            }
        }
        #endregion

        #region evaluating recieved msgs from communicator functions
        public void read(String read)
        {
            String readMsg = read.Substring(0, read.Length - 2);
            Char c = readMsg[0];
            Console.WriteLine("Type "+c+" found " + readMsg);
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
            else
            {
                basicCommandReader.Read(readMsg);
            }


            timerUpdateCoin(); //update coin timers
            timerUpdateHealthPack();//update health pack timers
            updateCoinAqquire(); //update coin queue and world about coin aquire
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
            players[myid].cordinateX = Int32.Parse(subSplit[1][0] + "");
            players[myid].cordinateY = Int32.Parse(subSplit[1][2] + "");
            
            
            //set initial dirctions
            players[myid].direction = Int32.Parse(subSplit[2]+"");

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

            }
            catch (Exception exception)
            {
                Console.WriteLine("readMsg is "+readMsg+  "\n Exception occured "+exception.Message);
            }
            //update map string and print
            this.updateMapString();
        }
        private void readMovingG(String read)
        {/*G:P1;< player location  x>,< player location  y>;<Direction>;< whether shot>;<health>;< coins>;< points>:
            …. P5;< player location  x>,< player location  y>;<Direction>;< whether shot>;<health>;
                < coins>;< points>: < x>,<y>,<damage-level>;< x>,<y>,<damage-level>;< x>,<y>,<damage-level>;
                < x>,<y>,<damage-level>…..< x>,<y>,<damage-level> */


            Boolean b = basicCommandReader.Read(read);
            if (!b)
            {


                String[] mainSplit = read.Split(':');
                int playerC = mainSplit.Count() - 2;
                for (int i = 1; i < playerC + 1; i++)
                {


                    String[] playerSplit = mainSplit[i].Split(';');
                    int playerNum = Int32.Parse(playerSplit[0][1] + "");
                    if (players[playerNum] == null)
                    {
                        players[playerNum] = new Player(playerNum.ToString());
                    }
                    
                    players[playerNum].cordinateX = Int32.Parse(playerSplit[1][0] + "");
                    players[playerNum].cordinateY = Int32.Parse(playerSplit[1][2] + "");
                    players[playerNum].direction = Int32.Parse(playerSplit[2] + "");
                    players[playerNum].whetherShot = Int32.Parse(playerSplit[3] + "");
                    players[playerNum].health = Int32.Parse(playerSplit[4] + "");
                    players[playerNum].coins = Int32.Parse(playerSplit[5] + "");
                    players[playerNum].points = Int32.Parse(playerSplit[6] + "");
                }
                String[] brickSplit = mainSplit[mainSplit.Count() - 1].Split(';');
                int brickCount = brickSplit.Count();
                for (int j = 0; j < brickCount; j++)
                {
                    String[] brick = brickSplit[j].Split(',');
                    int damage_val = Int32.Parse(brick[2] + "");
                    if (grid[Int32.Parse(brick[0] + ""), Int32.Parse(brick[1] + "")] != null)
                    {
                        ((Brick)(grid[Int32.Parse(brick[0] + ""), Int32.Parse(brick[1] + "")])).health = (4 - damage_val) * 25;
                        if (damage_val == 4)
                        {
                            grid[Int32.Parse(brick[0] + ""), Int32.Parse(brick[1] + "")] = new EmptyCell();
                        }

                    }


                }
            }

            //hunt coins
            if (coin_queue.Count > 0)
            {
                collectCoin();
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

        #region Logic
        public void collectCoin()
        {
            int count=0;
            List<int> costs = new List<int>();
            List<List<int>> list_of_commandLists = new List<List<int>>();
            foreach (Coin coin_pile in coin_queue)
            {
                List<int> command_list = commandList(pathEvaluationBFS(players[myid].cordinateX,
                    players[myid].cordinateY, coin_pile.x_cordinate, coin_pile.y_cordinate), players[myid].direction);
                
                if (command_list != null && ((coin_pile.left_time / 1000) > command_list.Count()))
                {
                    
                    costs.Add(command_list.Count);
                    list_of_commandLists.Add(command_list);
                }
            
            }

            if (costs.Count > 0)
            {
                int index_min_cost = costs.IndexOf(costs.Min());
                count += 1;
                sendCommandToServer(list_of_commandLists[index_min_cost]);
            }
            Console.WriteLine("count= "+count);
        }
        public List<int> commandList(List<int> pathE,int initial_dir)
        {
            if (pathE.Count>0)
            {
                List<int> path = pathE;
                List<int> commandList = new List<int>();
                
                if (path[0] != initial_dir) { commandList.Add(path[0]); }
                for (int i = 0; i < path.Count;i++ )
                {
                    commandList.Add(path[i]);
                    if (i + 1 < path.Count)
                    {
                        if (path[i] != path[i + 1]) { commandList.Add(path[i + 1]); }
                    }
                }
                
               
                return commandList;
            }
            return null;
        }
        public void sendCommandToServer(List<int> commandList)
        {
           
            if (commandList != null)
            {
                String temp = "sending command";
                foreach (int i in commandList)
                {
                    temp += i.ToString();
                }
                Console.WriteLine("selected command List " + temp);

                if (commandList[0] == 0) { basicCommandSender.Up(); }
                else if (commandList[0] == 1) { basicCommandSender.Right(); }
                else if (commandList[0] == 2) { basicCommandSender.Down(); }
                else if (commandList[0] == 3) { basicCommandSender.Left(); }

            }
        }
        public List<int> pathEvaluationBFS( int x_s,int y_s,int x_e, int y_e)
        {
            clearMapForBFS();   
            Queue<Cordinate> item_queue=new Queue<Cordinate>();
            item_queue.Enqueue(new Cordinate(x_s, y_s));
            ((MovableMapItem)(grid[x_s, y_s])).color = 1;
            
            while (item_queue.Count > 0) 
            {
                Cordinate cordinate = item_queue.Dequeue();
                MovableMapItem current = ((MovableMapItem)(grid[cordinate.x, cordinate.y]));
                List<int> current_path = ((MovableMapItem)(grid[cordinate.x, cordinate.y])).path;
                
                #region prioratise by recent value in the path list
                if (current.path.Count>1 && current.path[current.path.Count-1]==2)
                {
                    if (cordinate.y < 9 && (grid[cordinate.x, cordinate.y + 1]).GetType().BaseType.ToString().Equals("tank_game.MovableMapItem")
                   && ((MovableMapItem)(grid[cordinate.x, cordinate.y + 1])).color == 0)
                    {
                        item_queue.Enqueue(new Cordinate(cordinate.x, cordinate.y + 1));
                        ((MovableMapItem)(grid[cordinate.x, cordinate.y + 1])).color = 1;
                        ((MovableMapItem)(grid[cordinate.x, cordinate.y + 1])).path = new List<int>(current_path);
                        ((MovableMapItem)(grid[cordinate.x, cordinate.y + 1])).path.Add(2);

                    }
                }
                else if (current.path.Count > 1 && current.path[current.path.Count - 1] == 1)
                {
                    if (cordinate.x < 9 && (grid[cordinate.x + 1, cordinate.y]).GetType().BaseType.ToString().Equals("tank_game.MovableMapItem")
                                       && ((MovableMapItem)(grid[cordinate.x + 1, cordinate.y])).color == 0)
                    {
                        item_queue.Enqueue(new Cordinate(cordinate.x + 1, cordinate.y));
                        ((MovableMapItem)(grid[cordinate.x + 1, cordinate.y])).color = 1;
                        ((MovableMapItem)(grid[cordinate.x + 1, cordinate.y])).path = new List<int>(current_path);
                        ((MovableMapItem)(grid[cordinate.x + 1, cordinate.y])).path.Add(1);

                    }
                }
                else if (current.path.Count > 1 && current.path[current.path.Count - 1] == 0)
                {
                    if (cordinate.y > 0 && (grid[cordinate.x, cordinate.y - 1]).GetType().BaseType.ToString().Equals("tank_game.MovableMapItem")
                    && ((MovableMapItem)(grid[cordinate.x, cordinate.y - 1])).color == 0)
                    {
                        item_queue.Enqueue(new Cordinate(cordinate.x, cordinate.y - 1));
                        ((MovableMapItem)(grid[cordinate.x, cordinate.y - 1])).color = 1;
                        ((MovableMapItem)(grid[cordinate.x, cordinate.y - 1])).path = new List<int>(current_path);
                        ((MovableMapItem)(grid[cordinate.x, cordinate.y - 1])).path.Add(0);
                    }
                }

                else if (current.path.Count > 1 && current.path[current.path.Count - 1] == 3)
                {

                    if (cordinate.x > 0 && (grid[cordinate.x - 1, cordinate.y]).GetType().BaseType.ToString().Equals("tank_game.MovableMapItem")
                        && ((MovableMapItem)(grid[cordinate.x - 1, cordinate.y])).color == 0)
                    {
                        item_queue.Enqueue(new Cordinate(cordinate.x - 1, cordinate.y));
                        ((MovableMapItem)(grid[cordinate.x - 1, cordinate.y])).color = 1;
                        ((MovableMapItem)(grid[cordinate.x - 1, cordinate.y])).path = new List<int>(current_path);
                        ((MovableMapItem)(grid[cordinate.x - 1, cordinate.y])).path.Add(3);
                    }
                }

                #endregion

                if (cordinate.y < 9 && (grid[cordinate.x, cordinate.y + 1]).GetType().BaseType.ToString().Equals("tank_game.MovableMapItem")
                  && ((MovableMapItem)(grid[cordinate.x, cordinate.y + 1])).color == 0)
                {
                    item_queue.Enqueue(new Cordinate(cordinate.x, cordinate.y + 1));
                    ((MovableMapItem)(grid[cordinate.x, cordinate.y + 1])).color = 1;
                    ((MovableMapItem)(grid[cordinate.x, cordinate.y + 1])).path = new List<int>(current_path);
                    ((MovableMapItem)(grid[cordinate.x, cordinate.y + 1])).path.Add(2);

                }


                if (cordinate.x < 9 && (grid[cordinate.x + 1, cordinate.y]).GetType().BaseType.ToString().Equals("tank_game.MovableMapItem")
                                      && ((MovableMapItem)(grid[cordinate.x + 1, cordinate.y])).color == 0)
                {
                    item_queue.Enqueue(new Cordinate(cordinate.x + 1, cordinate.y));
                    ((MovableMapItem)(grid[cordinate.x + 1, cordinate.y])).color = 1;
                    ((MovableMapItem)(grid[cordinate.x + 1, cordinate.y])).path = new List<int>(current_path);
                    ((MovableMapItem)(grid[cordinate.x + 1, cordinate.y])).path.Add(1);

                }

                if (cordinate.y > 0 && (grid[cordinate.x, cordinate.y - 1]).GetType().BaseType.ToString().Equals("tank_game.MovableMapItem")
                    && ((MovableMapItem)(grid[cordinate.x, cordinate.y - 1])).color == 0)
                {
                    item_queue.Enqueue(new Cordinate(cordinate.x, cordinate.y - 1));
                    ((MovableMapItem)(grid[cordinate.x, cordinate.y - 1])).color = 1;
                    ((MovableMapItem)(grid[cordinate.x, cordinate.y - 1])).path = new List<int>(current_path);
                    ((MovableMapItem)(grid[cordinate.x, cordinate.y - 1])).path.Add(0);
                }

                if (cordinate.x > 0 && (grid[cordinate.x - 1, cordinate.y]).GetType().BaseType.ToString().Equals("tank_game.MovableMapItem")
                        && ((MovableMapItem)(grid[cordinate.x - 1, cordinate.y])).color == 0)
                {
                    item_queue.Enqueue(new Cordinate(cordinate.x - 1, cordinate.y));
                    ((MovableMapItem)(grid[cordinate.x - 1, cordinate.y])).color = 1;
                    ((MovableMapItem)(grid[cordinate.x - 1, cordinate.y])).path = new List<int>(current_path);
                    ((MovableMapItem)(grid[cordinate.x - 1, cordinate.y])).path.Add(3);
                }



                if (x_e == cordinate.x && y_e == cordinate.y)
                {
                    String temp = ""; 
                    foreach (int i in current_path)
                    {
                        temp += i.ToString();
                    }
                    Console.WriteLine("a path for "+x_e +" "+y_e+"  "+temp);
                    return current_path;

                }
                else 
                {
                    ((MovableMapItem)(grid[cordinate.x, cordinate.y])).color = 2;
                }
            }
            return null;
        }
        public void clearMapForBFS()
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (grid[i, j].GetType().BaseType.ToString().Equals("tank_game.MovableMapItem"))
                    {

                        ((MovableMapItem)(grid[i, j])).color = 0;
                        if (((MovableMapItem)(grid[i, j])).path != null)
                        {
                            ((MovableMapItem)(grid[i, j])).clearPathList();
                        }
                        else { ((MovableMapItem)(grid[i, j])).path = new List<int>(); }

                    }
                
                }
            }
        }
        #endregion

       
    }
}
