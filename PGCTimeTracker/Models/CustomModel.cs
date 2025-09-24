using Newtonsoft.Json;
using PGCTimeTracker_V2.Helpers;
using System.IO;
using System.Reflection;

namespace PGCTimeTracker_V2.Models
{
    public class ApiResponse
    {
        public string ResponseStatus { get; set; } = ResponseStatuses.Failure;
        public string Message { get; set; } = string.Empty;
    }
    public class TypedApiResponse<T> : ApiResponse
    {
        public new T? ResponseData { get; set; }
    }

    public class CommonWorkPlanItemVM
    {
        public long WorkPlanId { get; set; }
        public long CpaId { get; set; }
        public long ClientId { get; set; }
        public long TaskId { get; set; }
        public long Quantity { get; set; }
        public long StatusId { get; set; }
    }

    public class WorkPlanDataItemVM : CommonWorkPlanItemVM
    {
        public Exelocation Res = new Exelocation();
        public string CpaName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string TaskName { get; set; } = string.Empty;
        public string EstimatedTime { get; set; } = string.Empty;
        public string TotalTime { get; set; } = string.Empty;

    }

    public class WorkplanCommentResponseVM
    {
        public long Id { get; set; }
        public string CreatedOn { get; set; } = string.Empty;
        public string CommentBy { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
    }

    public class WorkplanChecklistResponseVM
    {
        public long Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool Checked { get; set; }
    }

    public class Exelocation
    {
        public string exelocation =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
    }
}
