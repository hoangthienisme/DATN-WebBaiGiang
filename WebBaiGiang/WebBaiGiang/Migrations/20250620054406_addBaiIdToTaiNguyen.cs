using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang.Migrations
{
    /// <inheritdoc />
    public partial class addBaiIdToTaiNguyen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "BaiGiangId",
                table: "TaiNguyen",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "BaiId",
                table: "TaiNguyen",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiNguyen_BaiId",
                table: "TaiNguyen",
                column: "BaiId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaiNguyen_Bai_BaiId",
                table: "TaiNguyen",
                column: "BaiId",
                principalTable: "Bai",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaiNguyen_Bai_BaiId",
                table: "TaiNguyen");

            migrationBuilder.DropIndex(
                name: "IX_TaiNguyen_BaiId",
                table: "TaiNguyen");

            migrationBuilder.DropColumn(
                name: "BaiId",
                table: "TaiNguyen");

            migrationBuilder.AlterColumn<int>(
                name: "BaiGiangId",
                table: "TaiNguyen",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
