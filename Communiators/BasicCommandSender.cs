using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tank_game.util;

namespace tank_game
{
    // the class for handle basic 6 sending commands and 8 receiving commands
    class BasicCommandSender
    {
        private Communicator com;

        public BasicCommandSender()
        {
            com = Communicator.getInstance();
        }
        

        //Basic send commands
        public void Join() { com.SendData(Constant.C2S_INITIALREQUEST); }
        public void Up() { com.SendData(Constant.UP); }
        public void Down() { com.SendData(Constant.DOWN); }
        public void Left() { com.SendData(Constant.LEFT); }
        public void Right() { com.SendData(Constant.RIGHT); }
        public void Shoot() { com.SendData(Constant.SHOOT); }

        
     
    }
}
