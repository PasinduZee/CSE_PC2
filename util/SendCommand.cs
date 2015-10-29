using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tank_game.util;

namespace tank_game
{
    class SendCommand
    {
        private util.Communicator com;
        private static SendCommand sc = new SendCommand();

        private SendCommand()
        {
            com = util.Communicator.GetInstance();
        }
        public static SendCommand getInstance(){return sc;}
        public void Join() { com.SendData(Constant.C2S_INITIALREQUEST); }
        public void Up() { com.SendData(Constant.UP); }
        public void Down() { com.SendData(Constant.DOWN); }
        public void Left() { com.SendData(Constant.LEFT); }
        public void Right() { com.SendData(Constant.RIGHT); }
        public void Shoot() { com.SendData(Constant.SHOOT); }


    }
}
