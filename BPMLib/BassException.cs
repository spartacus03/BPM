using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace BPMLib
{
    public class BassException: System.Exception
    {
        public BASSError errorCode { get; set; }
        public string msg { get; set; }

        public BassException(BASSError err, String msg)
        {
            this.errorCode = err;
            this.msg = msg;
        }
    }
}
