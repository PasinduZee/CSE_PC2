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
        //Main map which contains data
        public MapItem[,] grid{get;set;} 
        
        //Coin queue(a list) which contains current coin piles available
        public List<Coin> coin_queue { get; set; } //Coins

        //Health pack queue which contains current health packs available
        public List<HealthPack> health_pack_queue { get; set; } //HealthPacks
        
        //my client id in the game
        public int myid { get; set; }

        //A string which have character map about the world
        public String map_string { get; set; } //map character grid string for cmd

        /* playing method will be decided on this value
           
         *          0  ---------------- collect coin piles
         *          1  ---------------- collect health packs
         
        */
        public int playingMethod { get; set; }

        private Player[] players = new Player[5]; //players
        private int player_count;//Number of players in the game (from countable numbers)
        private BasicCommandReader basicCommandReader=new BasicCommandReader();
        private BasicCommandSender basicCommandSender=new BasicCommandSender();
        private Communicator com;
        
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
            playingMethod = 0;
        }

        #region Map Printing functions

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

            //initial playing method set to coin collect
            playingMethod = 0;

        }
  
        public String getMapString()
        {
            return this.map_string;
        }


        #endregion

        #region Map update functions
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
        private void updateHealthPackAqquire()
        {
            foreach (Player player in players)
            {
                if (player != null)
                {
                    for (int i = 0; i < health_pack_queue.Count; i++)
                    {
                        if (health_pack_queue[i].x_cordinate == player.cordinateX && health_pack_queue[i].y_cordinate == player.cordinateY)
                        {
                            grid[health_pack_queue[i].x_cordinate, health_pack_queue[i].y_cordinate] = new EmptyCell();
                            health_pack_queue.RemoveAt(i);
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
        private void updateWorld() {
            updateCoinAqquire();
            updateHealthPackAqquire();
            timerUpdateCoin();
            timerUpdateHealthPack();
        }
        #endregion

        #region evaluating recieved msgs from communicator functions
        public void read(String read)
        {
            String readMsg = read.Substring(0, read.Length - 2);
            Char c = readMsg[0];
            Console.WriteLine("Type "+c+" found " + readMsg);
            if (!basicCommandReader.Read(readMsg))
            {
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
            updateWorld();
            gamePlay();
            foreach (HealthPack hp in health_pack_queue)
            {
                Console.WriteLine("coin piles in queue " + hp.x_cordinate + " " + hp.y_cordinate);
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

        public void gamePlay()
        {
            if (playingMethod == 0) { collectCoin(); }
            else if (playingMethod == 1) { collectHealthPack(); }
        }
        public void collectCoin()  //make command for collect coin
        {
            List<int> costs = new List<int>();
            List<List<int>> list_of_commandLists = new List<List<int>>();
            foreach (Coin coin_pile in coin_queue)
            {
                List<int> coin_command_list = getCommandList(coin_pile.x_cordinate, coin_pile.y_cordinate);
                if (coin_command_list != null && ((coin_pile.left_time / 1000) > coin_command_list.Count()))
                {
                    //add commandLists for the lists        
                    costs.Add(coin_command_list.Count);
                    list_of_commandLists.Add(coin_command_list);
                }
            
            }
            //select minimum cost commandlist
            if (costs.Count > 0)
            {
                int index_min_cost = costs.IndexOf(costs.Min());
                sendCommandToServer(list_of_commandLists[index_min_cost]);
            }
        }
        public void collectHealthPack()  //make command for collect Health Pack
        {
            List<int> costs = new List<int>();
            List<List<int>> list_of_commandLists = new List<List<int>>();
            foreach (HealthPack health_pack in health_pack_queue)
            {
                List<int> health_command_list = getCommandList(health_pack.x_cordinate, health_pack.y_cordinate);
                if (health_command_list != null && ((health_pack.left_time / 1000) > health_command_list.Count()))
                {
                    //add commandLists for the lists
                    costs.Add(health_command_list.Count);
                    list_of_commandLists.Add(health_command_list);
                }

            }
            //select minimum cost commandlist
            if (costs.Count > 0)
            {
                int index_min_cost = costs.IndexOf(costs.Min());
                sendCommandToServer(list_of_commandLists[index_min_cost]);
            }
        }

        //check direct route first X axis then Y axis
        public List<int> checkXYroute(int end_x, int end_y)
        {
            int start_x = players[myid].cordinateX;
            int start_y = players[myid].cordinateY;
            bool have_direct_path = true;
            int x_relative = end_x - start_x;
            int y_relative = end_y - start_y;
            List<int> direct_path = new List<int>();
            
            for (int i = 1; i < Math.Abs(x_relative) + 1; i++)
            {
                if (!(grid[start_x + i * (x_relative / Math.Abs(x_relative)), start_y].GetType().BaseType.ToString().Equals("tank_game.MovableMapItem")))
                {
                    have_direct_path = false;
                }
                else
                {
                    if (x_relative > 0) { direct_path.Add(1); }
                    else if (x_relative < 0) { direct_path.Add(3); }

                }
            }
            for (int j = 1; j < Math.Abs(y_relative) + 1; j++)
            {
                if (!(grid[start_x + x_relative, start_y + j * (y_relative / Math.Abs(y_relative))].GetType().BaseType.ToString().Equals("tank_game.MovableMapItem")))
                {
                    have_direct_path = false;
                }
                else
                {
                    if (y_relative > 0) { direct_path.Add(2); }
                    else if (y_relative < 0) { direct_path.Add(0); }
                }
            }

            if (have_direct_path)
            {
                return commandList(direct_path, players[myid].direction);
            }
            else { return null; }
        }
        //check direct route first Y axis then X axis
        public List<int> checkYXroute(int end_x, int end_y)
        {
            int start_x = players[myid].cordinateX;
            int start_y = players[myid].cordinateY;
            bool have_direct_path = true;
            int x_relative = end_x - start_x;
            int y_relative = end_y - start_y;
            List<int> direct_path = new List<int>();

            for (int j = 1; j < Math.Abs(y_relative) + 1; j++)
            {
                if (!(grid[start_x, start_y + j * (y_relative / Math.Abs(y_relative))].GetType().BaseType.ToString().Equals("tank_game.MovableMapItem")))
                {
                    have_direct_path = false;
                }
                else
                {
                    if (y_relative > 0) { direct_path.Add(2); }
                    else if (y_relative < 0) { direct_path.Add(0); }
                }
            }
            for (int i = 1; i < Math.Abs(x_relative) + 1; i++)
            {
                if (!(grid[start_x + i * (x_relative / Math.Abs(x_relative)), start_y + y_relative].GetType().BaseType.ToString().Equals("tank_game.MovableMapItem")))
                {
                    have_direct_path = false;
                }
                else
                {
                    if (x_relative > 0) { direct_path.Add(1); }
                    else if (x_relative < 0) { direct_path.Add(3); }

                }
            }

            if (have_direct_path)
            {
                return commandList(direct_path, players[myid].direction);
            }
            else { return null; }
        }
        public List<int> getCommandList(int end_x, int end_y)
        {
            List<int> path;
            if (players[myid].direction % 2 == 1) 
            {
                path = checkXYroute(end_x,end_y);
                if (path != null)
                {
                    return commandList(path, players[myid].direction);
                }
                path = checkYXroute(end_x, end_y);
                if (path != null)
                {
                    return commandList(path, players[myid].direction);
                }
            }
            else 
            {
                path = checkYXroute(end_x, end_y);
                if (path != null)
                {
                    return commandList(path, players[myid].direction);
                }
                path = checkXYroute(end_x, end_y);
                if (path != null)
                {
                    return commandList(path, players[myid].direction);
                }
            }
           
            pathEvaluationBFS(players[myid].cordinateX, players[myid].cordinateY);
            return commandList(((MovableMapItem)(grid[end_x, end_y])).path, players[myid].direction);
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
        public void pathEvaluationBFS( int x_s,int y_s)
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

                    ((MovableMapItem)(grid[cordinate.x, cordinate.y])).color = 2;
                
            }
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
