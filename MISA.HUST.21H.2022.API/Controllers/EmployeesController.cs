using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MISA.HUST._21H._2022.API.Entities;
using MISA.HUST._21H._2022.API.Entities.DTO;
using MISA.HUST._21H._2022.API.Helper;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MISA.HUST._21H._2022.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        public AppDb Db { get; }

        public EmployeesController(AppDb db)
        {
            Db = db;
        }
        /// <summary>
        /// API lấy danh sách nhân viên
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAllEmployees(
            [FromQuery] string? keyword, 
            [FromQuery(Name = "position_id")] Guid ? positionId, 
            [FromQuery(Name = "department_id")] Guid ? departmentId, 
            [FromQuery(Name = "size")] int perPage = Constants.PER_PAGE,
            [FromQuery(Name = "page")] int pageNumber = 1
        )
        {
            string filterCondition = "";
            List<string> andCondition = new List<string>();
            List<string> orCondition = new List<string>();
            try
            {
                if (positionId != null && positionId != Guid.Empty)
                {
                    andCondition.Add($"PositionID = '{positionId}'");
                }

                if (departmentId != null && departmentId != Guid.Empty)
                {
                    andCondition.Add($"DepartmentID = '{departmentId}'");
                }

                if (andCondition.Count > 0)
                {
                    filterCondition += String.Join(" AND ", andCondition);
                }


                if (keyword != null && keyword.Length > 0)
                {
                    orCondition.Add($"EmployeeName LIKE '%{keyword}%'");
                    orCondition.Add($"EmployeeCode LIKE '%{keyword}%'");
                    orCondition.Add($"PhoneNumber LIKE '%{keyword}%'");
                }

                if (orCondition.Count > 0)
                {
                    if (andCondition.Count > 0)
                    {
                        filterCondition += $" AND ({String.Join(" OR ", orCondition)}) ";
                    }
                    else
                    {
                        filterCondition += $" ({String.Join(" OR ", orCondition)}) ";
                    }
                }
                System.Console.WriteLine(filterCondition);
                var parameters = new DynamicParameters();
                parameters.Add("@v_Offset", (pageNumber - 1) * perPage);
                parameters.Add("@v_Limit", perPage);
                parameters.Add("@v_Sort", "");
                parameters.Add("@v_Where", filterCondition);
                string storedProcedureName = "Proc_employee_GetPaging";

                var queryResults = Db.Connection.QueryMultiple(storedProcedureName, parameters, commandType: System.Data.CommandType.StoredProcedure);

                if (queryResults != null)
                {
                    var employees = queryResults.Read<Employee>().ToList();
                    var totalCount = queryResults.Read<int>().Single();
                    return StatusCode(StatusCodes.Status200OK, new PagingData<Employee>()
                    {
                        Data = employees,
                        TotalCount = totalCount
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        errorCode = "e001",
                        devMsg = "Query error",
                        userMsg = "Có lỗi xảy ra! vui lòng liên hệ admin",
                        moreInfo = "",
                        traceId = "",
                    });
                }
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

        /// <summary>
        /// API lấy thông tin một nhân viên
        /// </summary>
        /// <param name="employeeID">ID nhân viên</param>
        /// <returns>Thông tin chi tiết 1 nhân viên</returns>
        /// Created by: ND Doanh
        [HttpGet]
        [Route("{employeeID}")]
        public IActionResult GetEmployeeById([FromRoute] Guid employeeID)
        {
            try {
                var cmd = Db.Connection;
                string queryString = "SELECT * FROM employee WHERE EmployeeID = @EmployeeID";
                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeID", employeeID);
                var queryResult = cmd.QueryFirstOrDefault(queryString, parameters);
                if (queryResult != null)
                {
                    return StatusCode(StatusCodes.Status200OK, queryResult);
                } else
                {
                    return StatusCode(StatusCodes.Status404NotFound, MyHelper.buildError(userMsg: "Nhân viên này không tồn tại", type: "e002"));
                }
            } catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, MyHelper.buildError(devMsg: ex.Message));
            }
            
        }

        /// <summary>
        /// API lọc danh sách nhân viên có điều kiện tìm kiếm và phân trang
        /// </summary>
        /// <param name="keyword">Từ khoá muốn tìm kiếm</param>
        /// <param name="positionID">ID Vị trí</param>
        /// <param name="departmentID">ID phòng ban</param>
        /// <param name="limit">Số bản ghi trong 1 trang</param>
        /// <param name="offset">Vị trí bản ghi bắt đầu lấy dữ liệu</param>
        /// <returns>Danh sách nhân viên</returns>
        /// Created by: ND Doanh
        [HttpGet]
        [Route("filter")]        
        
        public IActionResult FilterEmployees(
            [FromQuery] string keyword, 
            [FromQuery] Guid positionID,
            [FromQuery] Guid departmentID,
            [FromQuery] int limit,
            [FromQuery] int offset)
        {
            return StatusCode(StatusCodes.Status200OK, new PagingData<Employee>
            {
                Data = new List<Employee>
                {
                        
                },
                TotalCount = 1000,
            });
        }


        /// <summary>
        /// Lấy mã nhân viên mới nhất
        /// </summary>
        /// <returns></returns>
        /// Created by: ND Doanh
        [HttpGet]
        [Route("NewEmployeeCode")]
        public IActionResult NewEmployeeCode()
        {
           
            try
            {
                var cmd = Db.Connection;
                var latestEmployeeCode = cmd.QueryFirstOrDefault("SELECT MAX(EmployeeCode) as EmployeeCode  FROM employee");
                string newCode;
                if (latestEmployeeCode == null)
                {
                    newCode = "NV00001";
                } else
                {
                    string tempCode = Regex.Match(latestEmployeeCode.EmployeeCode, @"\d+").Value;

                    newCode = "NV" + (Convert.ToInt32(tempCode) + 1).ToString();
                }
                return StatusCode(StatusCodes.Status200OK, new
                {
                    code = newCode,
                });
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
        /// <summary>
        /// API thêm mới 1 nhân viên
        /// </summary>
        /// <param name="employee">Đối tượng nhân viên cần thêm mới</param>
        /// <returns>ID của nhân viên vừa thêm mới</returns>
        /// Created by: Doanh
        [HttpPost]
        public IActionResult InsertEmployee([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] Employee employee)
        {
            try
            {
                // Kiem tra cac truong bat buoc phai nhap
                if (
                    String.IsNullOrEmpty(employee.EmployeeName) ||
                    String.IsNullOrEmpty(employee.EmployeeCode) ||
                    String.IsNullOrEmpty(employee.IdentityNumber) ||
                    String.IsNullOrEmpty(employee.Email) ||
                    String.IsNullOrEmpty(employee.PhoneNumber)
                  )
                {
                    return StatusCode(StatusCodes.Status400BadRequest, MyHelper.buildError(userMsg:"Vui lòng nhập đầy đủ các trường bắt buộc", type:"e002"));
                }
                // Kiem tra du lieu ngay sinh, thang sinh, nam sinh
                List<Time> listCheckTime = new List<Time>();
                if (employee.DateOfBirth.HasValue)
                {
                    listCheckTime.Add(new Time
                    {
                        value = employee.DateOfBirth.Value,
                        label = "Ngày sinh"
                    });
                }

                if (employee.IdentityIssuedDate.HasValue)
                {
                    listCheckTime.Add(new Time
                    {
                        value = employee.IdentityIssuedDate.Value,
                        label = "Ngày cấp CMND"
                    });
                }

                if (employee.JoiningDate.HasValue)
                {
                    listCheckTime.Add(new Time
                    {
                        value = employee.JoiningDate.Value,
                        label = "Ngày gia nhập"
                    });
                }

                foreach (Time fieldCheck in listCheckTime)
                {
                    if (MyHelper.isGreaterThanNow(fieldCheck.value))
                    {
                        return StatusCode(StatusCodes.Status400BadRequest, MyHelper.buildError(userMsg: $"{fieldCheck.label} không được lớn hơn thời điểm hiện tại", type: "e002"));
                    }
                }
                // Kiểm tra tính hợp lệ cảu email
                if(MyHelper.isValidEmail(employee.Email) == false)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, MyHelper.buildError(userMsg: "Địa chỉ email không hợp lệ", type: "e002"));
                }
                // Kiểm tra mã nhân viên, cmnd, email đã tồn tại hay chưa
                List<ValidateExist> listCheckExist = new List<ValidateExist>();
                listCheckExist.Add(new ValidateExist {
                    field = "EmployeeCode",
                    fieldName = "Mã nhân viên",
                });
                listCheckExist.Add(new ValidateExist
                {
                    field = "IdentityNumber",
                    fieldName = "CMTND/CCCD",
                });
                listCheckExist.Add(new ValidateExist
                {
                    field = "Email",
                    fieldName = "Địa chỉ email",
                });
               
                var cmd = Db.Connection;
                foreach (ValidateExist fieldCheck in listCheckExist)
                {
                    var checkParamters = new DynamicParameters();
                    string field = fieldCheck.field;
                    checkParamters.Add($"@{fieldCheck.field}", employee.GetType().GetProperty(field).GetValue(employee, null));
                    var checkExist = cmd.QueryFirstOrDefault($"SELECT count({field}) as countEmployee FROM employee WHERE {field} = @{field}", checkParamters);
                    if (checkExist != null && checkExist.countEmployee > 0)
                    {
                        return StatusCode(StatusCodes.Status400BadRequest, MyHelper.buildError(userMsg: $"{fieldCheck.fieldName} này đã tồn tại, hãy sử dụng giá trị khác", type: "e002"));
                    }

                }
                string queryString = "";
                string DepartmentName = null;
                string PositionName = null;
                // Lấy tên phòng ban
                if (employee.DepartmentID != null)
                {
                    queryString = "SELECT DepartmentName FROM department WHERE DepartmentID = @DepartmentID limit 1";
                    var parameters = new DynamicParameters();
                    parameters.Add("@DepartmentID", employee.DepartmentID);
                    var result = cmd.QueryFirstOrDefault(queryString, parameters);
                    if(result != null)
                    {
                        DepartmentName = result.DepartmentName;
                    }
                }

                // Lấy tên vị trí

                if (employee.PositionID != null)
                {
                    queryString = "SELECT PositionName FROM positions WHERE PositionID = @PositionID limit 1";
                    var parameters = new DynamicParameters();
                    parameters.Add("@PositionID", employee.PositionID);
                    var result = cmd.QueryFirstOrDefault(queryString, parameters);
                    if (result != null)
                    {
                        PositionName = result.PositionName;
                    }
                }

                // Insert vào database
                queryString = "INSERT INTO employee ( EmployeeID ,EmployeeCode ,EmployeeName ,DateOfBirth ,Gender ,IdentityNumber ,IdentityIssuedPlace ,IdentityIssuedDate ,Email ,PhoneNumber ,PositionID ,PositionName ,DepartmentID ,DepartmentName ,TaxCode ,Salary ,JoiningDate ,WorkStatus ,CreatedDate ,CreatedBy ,ModifiedDate ,ModifiedBy ) " +
                    "VALUES ( @EmployeeID ,@EmployeeCode ,@EmployeeName ,@DateOfBirth ,@Gender ,@IdentityNumber ,@IdentityIssuedPlace ,@IdentityIssuedDate ,@Email ,@PhoneNumber ,@PositionID ,@PositionName ,@DepartmentID ,@DepartmentName ,@TaxCode ,@Salary ,@JoiningDate ,@WorkStatus ,@CreatedDate ,@CreatedBy ,@ModifiedDate ,@ModifiedBy );";
                var paramters = new DynamicParameters();
                paramters.Add("@EmployeeID", Guid.NewGuid());
                paramters.Add("@EmployeeCode", employee.EmployeeCode);
                paramters.Add("@EmployeeName", employee.EmployeeName);
                paramters.Add("@DateOfBirth", employee.DateOfBirth);
                paramters.Add("@Gender", employee.Gender);
                paramters.Add("@IdentityNumber", employee.IdentityNumber);
                paramters.Add("@IdentityIssuedPlace", employee.IdentityIssuedPlace);
                paramters.Add("@IdentityIssuedDate", employee.IdentityIssuedDate);
                paramters.Add("@Email", employee.Email);
                paramters.Add("@PhoneNumber", employee.PhoneNumber);
                paramters.Add("@PositionID", employee.PositionID);
                paramters.Add("@PositionName", PositionName);
                paramters.Add("@DepartmentID", employee.DepartmentID);
                paramters.Add("@DepartmentName", DepartmentName);
                paramters.Add("@TaxCode", employee.TaxCode);
                paramters.Add("@Salary", employee.Salary);
                paramters.Add("@JoiningDate", employee.JoiningDate);
                paramters.Add("@WorkStatus", employee.WorkStatus);
                paramters.Add("@CreatedDate", employee.CreatedDate);
                paramters.Add("@CreatedBy", employee.CreatedBy);
                paramters.Add("@ModifiedDate", employee.ModifiedDate);
                paramters.Add("@ModifiedBy", employee.ModifiedBy);

                int row = cmd.Execute(queryString, paramters);
                if (row > 0)
                {
                    return StatusCode(StatusCodes.Status201Created, new
                    {
                        success = true,
                        Message = "Đã thêm nhân viên có mã " + employee.EmployeeCode + " vào hệ thống",
                    }) ; 
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, MyHelper.buildError(userMsg:"Có lỗi trong quá trình thêm nhân viên !"));
                }
            } catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, MyHelper.buildError(devMsg: ex.Message, userMsg: "Lỗi hệ thống, liên hệ admin để được hỗ trợ"));
            }
            
        }

        /// <summary>
        /// API sửa 1 nhân viên
        /// </summary>
        /// <param name="employee">Đối tượng nhân viên cần sửa</param>
        /// <param name="employeeID">ID nhân viên cần sửa</param>
        /// <returns>ID của nhân viên vừa sửa</returns>
        /// Created by: Doanh
        [HttpPut]
        [Route("{employeeID}")]
        public IActionResult UpdateEmployee([FromBody] Employee employee, [FromRoute] Guid employeeID)
        {
            try {
                var cmd = Db.Connection;
                // Lấy thông tin hiện tại
                string queryString = "SELECT * FROM employee WHERE EmployeeID = @EmployeeID;";
                var paramters = new DynamicParameters();
                paramters.Add("@EmployeeID", employeeID);
                var current = cmd.QueryFirstOrDefault(queryString, paramters);
                if (current == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, MyHelper.buildError(userMsg: "Nhân viên này không tồn tại"));
                }
                // Kiểm tra tính hợp lệ của 1 số trường thời gian
                List<Time> listCheckTime = new List<Time>();

                if (employee.DateOfBirth.HasValue)
                {
                    listCheckTime.Add(new Time
                    {
                        value = employee.DateOfBirth.Value,
                        label = "Ngày sinh"
                    });
                }

                if (employee.IdentityIssuedDate.HasValue)
                {
                    listCheckTime.Add(new Time
                    {
                        value = employee.IdentityIssuedDate.Value,
                        label = "Ngày cấp CMND"
                    });
                }

                if (employee.JoiningDate.HasValue)
                {
                    listCheckTime.Add(new Time
                    {
                        value = employee.JoiningDate.Value,
                        label = "Ngày gia nhập"
                    });
                }

                foreach (Time fieldCheck in listCheckTime)
                {
                    if (MyHelper.isGreaterThanNow(fieldCheck.value))
                    {
                        return StatusCode(StatusCodes.Status400BadRequest, MyHelper.buildError(userMsg: $"{fieldCheck.label} không được lớn hơn thời điểm hiện tại", type: "e002"));
                    }
                }

                // Kiểm tra tính hợp lệ cảu email
                if (MyHelper.isValidEmail(employee.Email) == false)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, MyHelper.buildError(userMsg: "Địa chỉ email không hợp lệ", type: "e002"));
                }

                System.Console.WriteLine(employee.Email);
                System.Console.WriteLine(new EmailAddressAttribute().IsValid(employee.Email));

                // Nếu có thay đổi về mã nhân viên, email, CMND cần kiểm tra xem giá trị đó có trùng không
                List<ValidateExist> listCheckExist = new List<ValidateExist>();
                if (current.EmployeeCode != employee.EmployeeCode)
                {
                    listCheckExist.Add(new ValidateExist
                    {
                        field = "EmployeeCode",
                        fieldName = "Mã nhân viên",
                    });
                }

                if (current.Email != employee.Email)
                {
                    listCheckExist.Add(new ValidateExist
                    {
                        field = "Email",
                        fieldName = "Email",
                    });
                }

                if (current.IdentityNumber != employee.IdentityNumber)
                {
                    listCheckExist.Add(new ValidateExist
                    {
                        field = "IdentityNumber",
                        fieldName = "CMTND/CCCD",
                    });
                }

                foreach (ValidateExist fieldCheck in listCheckExist)
                {
                    var checkParamters = new DynamicParameters();
                    string field = fieldCheck.field;
                    checkParamters.Add($"@{fieldCheck.field}", employee.GetType().GetProperty(field).GetValue(employee, null));
                    checkParamters.Add($"@EmployeeID", employeeID);
                    var checkExist = cmd.QueryFirstOrDefault($"SELECT count({field}) as countEmployee FROM employee WHERE {field} = @{field} AND EmployeeID != @EmployeeID", checkParamters);
                    if (checkExist != null && checkExist.countEmployee > 0)
                    {
                        return StatusCode(StatusCodes.Status400BadRequest, MyHelper.buildError(userMsg: $"{fieldCheck.fieldName} này đã tồn tại, hãy sử dụng giá trị khác", type: "e002"));
                    }

                }

                string DepartmentName = current.DepartmentName;
                string PositionName = current.PositionName;
                // Lấy tên phòng ban
                if (employee.DepartmentID != null && current.DepartmentID != employee.DepartmentID)
                {
                    queryString = "SELECT DepartmentName FROM department WHERE DepartmentID = @DepartmentID limit 1";
                    var param = new DynamicParameters();
                    param.Add("@DepartmentID", employee.DepartmentID);
                    var department = cmd.QueryFirstOrDefault(queryString, param);
                    if (department != null)
                    {
                        DepartmentName = department.DepartmentName;
                    }
                }

                // Lấy tên vị trí

                if (employee.PositionID != null && current.PositionID != employee.PositionID)
                {
                    queryString = "SELECT PositionName FROM positions WHERE PositionID = @PositionID limit 1";
                    var param = new DynamicParameters();
                    param.Add("@PositionID", employee.PositionID);
                    var position = cmd.QueryFirstOrDefault(queryString, param);
                    if (position != null)
                    {
                        PositionName = position.PositionName;
                    }
                }

                // Cập nhật nhân viên
                queryString = "UPDATE employee SET EmployeeCode = @EmployeeCode ,EmployeeName = @EmployeeName ,DateOfBirth = @DateOfBirth ,Gender = @Gender ,IdentityNumber = @IdentityNumber ,IdentityIssuedPlace = @IdentityIssuedPlace ,IdentityIssuedDate = @IdentityIssuedDate ,Email = @Email ,PhoneNumber = @PhoneNumber ,PositionID = @PositionID ,PositionName = @PositionName ,DepartmentID = @DepartmentID ,DepartmentName = @DepartmentName ,TaxCode = @TaxCode ,Salary = @Salary ,JoiningDate = @JoiningDate ,WorkStatus = @WorkStatus ,ModifiedDate = NOW() ,ModifiedBy = @ModifiedBy WHERE EmployeeID = @EmployeeID ;";
                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeID", employeeID);
                parameters.Add("@EmployeeCode", employee.EmployeeCode);
                parameters.Add("@EmployeeName", employee.EmployeeName);
                parameters.Add("@DateOfBirth", employee.DateOfBirth);
                parameters.Add("@Gender", employee.Gender);
                parameters.Add("@IdentityNumber", employee.IdentityNumber);
                parameters.Add("@IdentityIssuedPlace", employee.IdentityIssuedPlace);
                parameters.Add("@IdentityIssuedDate", employee.IdentityIssuedDate);
                parameters.Add("@Email", employee.Email);
                parameters.Add("@PhoneNumber", employee.PhoneNumber);
                parameters.Add("@PositionID", employee.PositionID);
                parameters.Add("@PositionName", PositionName);
                parameters.Add("@DepartmentID", employee.DepartmentID);
                parameters.Add("@DepartmentName", DepartmentName);
                parameters.Add("@TaxCode", employee.TaxCode);
                parameters.Add("@Salary", employee.Salary);
                parameters.Add("@JoiningDate", employee.JoiningDate);
                parameters.Add("@WorkStatus", employee.WorkStatus);
                parameters.Add("@ModifiedBy", employee.ModifiedBy);

                int result = cmd.Execute(queryString, parameters);
                return StatusCode(StatusCodes.Status200OK, new
                {
                    success = true,
                    Message = "Cập nhật dữ liệu cho nhân viên " + employee.EmployeeCode + " thành công",
                });
            } catch(Exception ex) {
                return StatusCode(StatusCodes.Status400BadRequest, MyHelper.buildError(devMsg: ex.Message, userMsg: "Lỗi hệ thống, liên hệ admin để được hỗ trợ"));
            }
        }

        /// <summary>
        /// API xoá 1 nhân viên
        /// </summary>
        /// <param name="listEmployeeID">List ID nhân viên cần xoá</param>
        /// <returns></returns>
        /// Created by: Doanh
        [HttpDelete()]
        public IActionResult RemoveEmployee([FromBody] List<string> listEmployeeID)
        {
            try {
                if(listEmployeeID.Count <= 0)
                {
                    return StatusCode(StatusCodes.Status200OK, listEmployeeID);
                }
                var cmd = Db.Connection;

                string queryString = "DELETE FROM employee where EmployeeID In @EmployeeID";
                var parameters = new Dictionary<string, object>() {
                    ["EmployeeID"] = listEmployeeID
                };
                int countDelete = cmd.Execute(queryString, parameters);
                return StatusCode(StatusCodes.Status200OK, new
                {
                    success = true,
                    message = "Đã xoá " + Convert.ToString(countDelete) + " nhân viên",
                });

            }
            catch(Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status400BadRequest, MyHelper.buildError(devMsg: ex.Message, userMsg: "Lỗi hệ thống, liên hệ admin để được hỗ trợ"));
                
            }
        }

    }

    public class ValidateExist
    {
        public string field { set; get; }

        public string fieldName { set; get; }
    }

    public class Time
    {
        public DateTime value { set; get; }

        public string label { set; get; }
    }

}
