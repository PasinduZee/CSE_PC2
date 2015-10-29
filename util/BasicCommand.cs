using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tank_game.util;

namespace tank_game
{
    class BasicCommand
    {
        private util.Communicator com;
        private static BasicCommand sc = new BasicCommand();

        private BasicCommand()
        {
            com = util.Communicator.GetInstance();
        }
        public static BasicCommand getInstance(){return sc;}


        //Basic send commands
        public void Join() { com.SendData(Constant.C2S_INITIALREQUEST); }
        public void Up() { com.SendData(Constant.UP); }
        public void Down() { com.SendData(Constant.DOWN); }
        public void Left() { com.SendData(Constant.LEFT); }
        public void Right() { com.SendData(Constant.RIGHT); }
        public void Shoot() { com.SendData(Constant.SHOOT); }

        
        //Basic receive commands

        public Boolean Read(String msg)
        {
            if (msg.Equals(Constant.S2C_HITONOBSTACLE))
            {
                Console.WriteLine("cannot move,obstacles found");
                return true;
            }
            else if (msg.Equals(Constant.S2C_CELLOCCUPIED))
            {
                Console.WriteLine("Cell is already occupied");
                return true;
            }
            else if (msg.Equals(Constant.S2C_NOTALIVE))
            {
                Console.WriteLine("Dead");
                return true;
            }
            else if (msg.Equals(Constant.S2C_TOOEARLY))
            {
                Console.WriteLine("Too Quick response");
                return true;
            }
            else if (msg.Equals(Constant.S2C_INVALIDCELL))
            {
                Console.WriteLine("Invalid Cell");
                return true;
            }

            else if (msg.Equals(Constant.S2C_GAMEJUSTFINISHED))
            {
                Console.WriteLine("Game has finished");
                return true;
            }
            else if (msg.Equals(Constant.S2C_NOTSTARTED))
            {
                Console.WriteLine("game not started yet");
                return true;
            }
            else if (msg.Equals(Constant.S2C_NOTACONTESTANT))
            {
                Console.WriteLine("Not a valid contestant");
                return true;
            }
            return false;
        }

    }
}
