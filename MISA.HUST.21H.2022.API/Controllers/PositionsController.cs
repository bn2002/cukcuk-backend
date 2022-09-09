using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MISA.HUST._21H._2022.API.Entities;

namespace MISA.HUST._21H._2022.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : ControllerBase
    {
        public AppDb Db { get; }
        public PositionsController(AppDb db)
        {
            Db = db;
        }
        /// <summary>
        /// API lấy danh sách tất cả các vị trí
        /// </summary>
        /// <returns></returns>
        /// Created by: ND Doanh
        [HttpGet]
        [Route("")]
        public IActionResult GetAllPositions()
        {
            try {
                var cmd = Db.Connection;
                var posotions =  cmd.Query("select positionID, positionName from positions");
                return StatusCode(StatusCodes.Status200OK, posotions);

            } catch(Exception ex)
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
