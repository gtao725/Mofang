using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface ILoadWinceManger
    {
        bool CheckLoad(string WhCode, string LoadId);
        bool CheckPltScan(string WhCode, string LoadId, string HuId);
        string ToSerialNumberOut(string WhCode, string LoadId);
        List<string> GetSerialNumber(int HuDetailId);
        ShipLoadDesModel GetShipLoadDesModel(string WhCode, string LoadId);
        string LoadSugPlt(string WhCode, string LoadId);
        string CheckPltLoad(string WhCode, string LoadId, string HuId);
        string LoadComplete(LoadPlt loadPlt);

        bool CheckLoadIfComplete(string WhCode, string LoadId);
    }
}
