using PortToNet.Ecan;
using PortToNet.Model;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void PackCanFramePackCanTest()
        {
            List<CAN_OBJ> list = new List<CAN_OBJ>();
            CAN_OBJ can1 = new CAN_OBJ();
            can1.ID = 0x123;
            can1.DataLen = 8;
            can1.data = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                can1.data[i] = (byte)i;
            }
            can1.SendType = 0;
            can1.RemoteFlag = 1;
            can1.ExternFlag = 1;

            list.Add(can1);
            byte[] pack = PackCanFrame.PackCan(list);
            var list2 = PackCanFrame.UnPackCan(pack);
            CAN_OBJ can2 = list2[0];

            Assert.Equal(can1.ID, can2.ID);
            Assert.Equal(can1.DataLen, can2.DataLen);
            Assert.Equal(can1.data.Length, can2.data.Length);
            for (int i = 0; i < 8; i++)
            {
                Assert.Equal(can1.data[i], can2.data[i]);
            }
            Assert.Equal(can1.SendType, can2.SendType);
            Assert.Equal(can1.RemoteFlag, can2.RemoteFlag);
            Assert.Equal(can1.ExternFlag, can2.ExternFlag);
        }
    }
}