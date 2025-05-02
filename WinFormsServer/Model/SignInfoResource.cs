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
        public int page { get; set; }
        // public string searchText { get; set; }
        // public string FileType { get; set; }
        public string token { get; set; }
        public Guid documentId { get; set; }
    }

    public class ResponseModel
    {
        public ContentModel Content { get; set; }
        public string Message { get; set; }
        public int? Size { get; set; }
        public int StatusCode { get; set; }
        public MeatadataDto MeatadataDto { get; set; }
    }
    public class ContentModel
    {
        public string File { get; set; }
        public string Image { get; set; }
    }
    public class MeatadataDto
    {
        public MeatadataDto()
        {
        }

        public MeatadataDto(bool hasNextPage, bool hasPrevPage, int limit, int total, int page)
        {
            this.hasNextPage = hasNextPage;
            this.hasPrevPage = hasPrevPage;
            this.limit = limit;
            this.total = total;
            this.page = page;
        }

        public bool hasNextPage { get; set; } = false;
        public bool hasPrevPage { get; set; } = false;
        public int limit { get; set; } = 1;
        public int total { get; set; } = 1;
        public int page { get; set; }=1;
    }

    public class UploadFileResource
    {
        public string fileName { get; set; }
        public FileStream File { get; set; }
    }
}
