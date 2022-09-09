namespace MISA.HUST._21H._2022.API.Entities
{
    public class Department
    {
        /// <summary>
        /// ID phòng ban
        /// </summary>
        public Guid DepartmentID { get; set; }

        /// <summary>
        /// Mã phòng ban
        /// </summary>
        public String DepartmentCode { get; set; }

        /// <summary>
        /// Tên phòng ban
        /// </summary>
        public String DepartmentName { get; set; }

        /// <summary>
        /// Thời điểm tạo
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Người tạo
        /// </summary>
        public String CreatedBy { get; set; }
        
        /// <summary>
        /// Thời điểm chỉnh sửa gần nhất
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Người sửa cuối cùng
        /// </summary>
        public String ModifiedBy { get; set; }




    }
}
