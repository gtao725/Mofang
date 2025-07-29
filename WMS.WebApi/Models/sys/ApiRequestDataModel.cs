using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WMS.BLLClass;

namespace WMS.WebApi.Models
{
    public class ApiRequestDataModel
    {

        private ReceiptInsert _recModel;

        public ReceiptInsert recModel
        {
            get
            {
                return _recModel;
            }
            set
            {
                _recModel = value;
            }
        }
        private PallatesModel _pallatesModel;
        public PallatesModel pallatesModel
        {
            get
            {
                return _pallatesModel;
            }
            set
            {
                _pallatesModel = value;
            }
        }

        private ShipLoadPlt _shipLoadPlt;
        public ShipLoadPlt shipLoadPlt
        {
            get
            {
                return _shipLoadPlt;
            }
            set
            {
                _shipLoadPlt = value;
            }
        }

        private LoadPlt _loadPlt;

        public LoadPlt loadPlt
        {
            get { return _loadPlt; }
            set { _loadPlt = value; }
        }

        private PickTaskDetailResult _pickTaskDetailResult;
        public PickTaskDetailResult pickTaskDetailResult
        {
            get { return _pickTaskDetailResult; }
            set { _pickTaskDetailResult = value; }
        }

        
    }
}