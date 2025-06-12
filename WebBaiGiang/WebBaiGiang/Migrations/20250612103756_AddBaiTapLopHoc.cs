using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang.Migrations
{
    /// <inheritdoc />
    public partial class AddBaiTapLopHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK__BaiTap__Class_id__52593CB8",
            //    table: "BaiTap");

            //migrationBuilder.DropIndex(
            //    name: "IX_BaiTap_Class_id",
            //    table: "BaiTap");

            migrationBuilder.DropColumn(
                name: "Class_id",
                table: "BaiTap");

            migrationBuilder.CreateTable(
                name: "BaiTapLopHoc",
                columns: table => new
                {
                    BaiTapId = table.Column<int>(type: "int", nullable: false),
                    LopHocId = table.Column<int>(type: "int", nullable: false),
                    NgayGiao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiTapLopHoc", x => new { x.BaiTapId, x.LopHocId });
                    table.ForeignKey(
                        name: "FK_BaiTapLopHoc_BaiTap_BaiTapId",
                        column: x => x.BaiTapId,
                        principalTable: "BaiTap",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaiTapLopHoc_LopHoc_LopHocId",
                        column: x => x.LopHocId,
                        principalTable: "LopHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaiTapLopHoc_LopHocId",
                table: "BaiTapLopHoc",
                column: "LopHocId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaiTapLopHoc");

            migrationBuilder.AddColumn<int>(
                name: "Class_id",
                table: "BaiTap",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BaiTap_Class_id",
                table: "BaiTap",
                column: "Class_id");

            migrationBuilder.AddForeignKey(
                name: "FK__BaiTap__Class_id__52593CB8",
                table: "BaiTap",
                column: "Class_id",
                principalTable: "LopHoc",
                principalColumn: "Id");
        }
    }
}
