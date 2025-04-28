using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SgTest3.Model
{
    public class SignInfoResource
    {
        public int llx { get; set; }
        public int lly { get; set; }
        public int urx { get; set; }
        public int ury { get; set; }
        public string searchText { get; set; }
        public string FileType { get; set; }
        public string Token { get; set; }
        public Guid FileID { get; set; }
    }

    public class ResponseModel
    {
        public bool isSuccess { get; set; }
        public int code { get; set; }
        public string responseSuccess { get; set; }
        public string responseFailed { get; set; }
    }

    public class UploadFileResource
    {
        public string fileName { get; set; }
        public FileStream File { get; set; }
    }
}
