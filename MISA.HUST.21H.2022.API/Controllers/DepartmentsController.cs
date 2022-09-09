using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MISA.HUST._21H._2022.API.Entities;

namespace MISA.HUST._21H._2022.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        public AppDb Db { get; }
        public DepartmentsController(AppDb db)
        {
            Db = db;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public IActionResult GetAllDepartments()
        {
            try
            {
                var cmd = Db.Connection;
                var posotions = cmd.Query("select departmentID, departmentName from department");
                return StatusCode(StatusCodes.Status200OK, posotions);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    errorCode = "e001",
                    devMsg = ex.Message,
                    userMsg = "Có lỗi xảy ra! vui lòng liên hệ admin",
                    moreInfo = "",
                    traceId = "",
                });
            }
        }
    }
}
