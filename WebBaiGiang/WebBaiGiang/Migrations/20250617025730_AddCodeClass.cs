using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JoinCode",
                table: "LopHoc",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JoinCode",
                table: "LopHoc");
        }
    }
}
